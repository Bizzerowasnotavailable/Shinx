using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Shinx.Commands
{
    internal class core_http : ICommand
    {
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
        {
            { ".html", "text/html" },
            { ".htm",  "text/html" },
            { ".css",  "text/css" },
            { ".js",   "application/javascript" },
            { ".txt",  "text/plain" },
            { ".json", "application/json" },
            { ".png",  "image/png" },
            { ".jpg",  "image/jpeg" },
            { ".lua",  "text/plain" },
        };

        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (DesktopManager.Running)
            {
                Console.WriteLine("http: cannot run while desktop is active");
                return;
            }

            if (!NetworkManager.IsConnected)
            {
                Console.WriteLine("http: no network connection");
                return;
            }

            string rootFolder = args.Length > 0
                ? (args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0])
                : Shell.currentDirectory;

            int port = args.Length > 1 ? int.Parse(args[1]) : 8080;

            string myIp = NetworkManager.CurrentIP;
            Console.WriteLine("http: serving " + rootFolder);
            Console.WriteLine("http: http://" + myIp + ":" + port);
            Console.WriteLine("http: press any key to stop");

            try
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        Console.WriteLine("http: stopping...");
                        listener.Stop();
                        return;
                    }

                    TcpClient client = listener.AcceptTcpClient();
                    HandleClient(client, rootFolder);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("http: " + e.Message);
            }
        }

        private void HandleClient(TcpClient client, string rootFolder)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    try { bytesRead = stream.Read(buffer, 0, buffer.Length); }
                    catch { break; }
                    if (bytesRead == 0) break;

                    string rawRequest = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    string firstLine = rawRequest.Split('\n')[0].Trim();
                    string[] parts = firstLine.Split(' ');
                    if (parts.Length < 2) break;

                    string method = parts[0];
                    string urlPath = parts[1].Split('?')[0];

                    bool keepAlive = rawRequest.Contains("Connection: keep-alive");

                    Console.WriteLine("http: " + method + " " + urlPath);

                    if (urlPath == "/favicon.ico")
                    {
                        SendResponse(stream, "404 Not Found", "text/plain", "", false);
                        if (!keepAlive) break;
                        continue;
                    }

                    if (urlPath == "/stop")
                    {
                        SendResponse(stream, "200 OK", "text/html", "<h1>Stopped.</h1>", false);
                        stream.Close();
                        client.Close();
                        throw new Exception("stop requested");
                    }

                    string filePath = rootFolder.TrimEnd('\\') + urlPath.Replace('/', '\\');

                    if (urlPath == "/" || Directory.Exists(filePath))
                    {
                        if (urlPath == "/") filePath = rootFolder;
                        string index = filePath.TrimEnd('\\') + "\\index.html";
                        if (File.Exists(index))
                            filePath = index;
                        else
                        {
                            string listing = BuildDirectoryListing(urlPath, filePath);
                            SendResponse(stream, "200 OK", "text/html", listing, keepAlive);
                            if (!keepAlive) break;
                            continue;
                        }
                    }

                    if (!File.Exists(filePath))
                    {
                        SendResponse(stream, "404 Not Found", "text/html",
                            "<html><body><h1>404</h1></body></html>", keepAlive);
                        if (!keepAlive) break;
                        continue;
                    }

                    if (!PermissionManager.CanAccess(filePath, UserManager.currentUser))
                    {
                        SendResponse(stream, "403 Forbidden", "text/html",
                            "<html><body><h1>403</h1></body></html>", keepAlive);
                        if (!keepAlive) break;
                        continue;
                    }

                    string ext = "";
                    int dotIdx = filePath.LastIndexOf('.');
                    if (dotIdx >= 0) ext = filePath.Substring(dotIdx).ToLower();
                    string mime = MimeTypes.ContainsKey(ext) ? MimeTypes[ext] : "application/octet-stream";

                    byte[] fileData = File.ReadAllBytes(filePath);

                    string responseHeader =
                        "HTTP/1.1 200 OK\r\n" +
                        "Content-Type: " + mime + "\r\n" +
                        "Content-Length: " + fileData.Length + "\r\n" +
                        "Connection: " + (keepAlive ? "keep-alive" : "close") + "\r\n\r\n";

                    byte[] headerBytes = Encoding.ASCII.GetBytes(responseHeader);
                    stream.Write(headerBytes, 0, headerBytes.Length);
                    stream.Write(fileData, 0, fileData.Length);

                    if (!keepAlive) break;
                }

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                if (e.Message == "stop requested") throw;
                Console.WriteLine("http: client error: " + e.Message);
                try { client.Close(); } catch { }
            }
        }

        private void SendResponse(NetworkStream stream, string status, string contentType, string body, bool keepAlive)
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
            string header =
                "HTTP/1.1 " + status + "\r\n" +
                "Content-Type: " + contentType + "\r\n" +
                "Content-Length: " + bodyBytes.Length + "\r\n" +
                "Connection: " + (keepAlive ? "keep-alive" : "close") + "\r\n\r\n";
            byte[] headerBytes = Encoding.ASCII.GetBytes(header);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(bodyBytes, 0, bodyBytes.Length);
        }

        private string BuildDirectoryListing(string urlPath, string dirPath)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head><style>");
            sb.Append("body{font-family:monospace;background:#111;color:#eee;padding:2rem;}");
            sb.Append("a{color:#7eb8f7;}h1{color:#f7c67e;}");
            sb.Append("</style></head><body>");
            sb.Append("<h1>Index of " + urlPath + "</h1><ul>");

            if (urlPath != "/")
                sb.Append("<li><a href=\"..\">[..]</a></li>");

            try
            {
                var entries = Cosmos.System.FileSystem.VFS.VFSManager.GetDirectoryListing(dirPath);
                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        bool isDir = entry.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory;
                        string href = urlPath.TrimEnd('/') + "/" + entry.mName + (isDir ? "/" : "");
                        string label = isDir ? "[" + entry.mName + "]" : entry.mName;
                        sb.Append("<li><a href=\"" + href + "\">" + label + "</a></li>");
                    }
                }
            }
            catch { sb.Append("<li>error reading directory</li>"); }

            sb.Append("</ul></body></html>");
            return sb.ToString();
        }
    }
}