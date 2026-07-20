using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Shinx.Commands
{
    internal class core_http : ICommand, ICancellable
    {
        private volatile bool _running;
        private TcpListener _listener;
        private Thread _serverThread;
        private int _pid;

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

        public void Cancel()
        {
            _running = false;
            try { _listener?.Stop(); } catch { }
            try { _serverThread?.Join(2000); } catch { }
            _serverThread = null;
        }

        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (!NetworkManager.IsConnected)
            {
                Console.WriteLine("http: no network connection");
                return;
            }

            string rootFolder = args.Length > 0
                ? (args[0].StartsWith("/") ? args[0] : Shell.currentDirectory + args[0])
                : Shell.currentDirectory;

            int port = args.Length > 1 ? int.Parse(args[1]) : 8080;

            string myIp = NetworkManager.CurrentIP;
            Console.WriteLine("http: http://" + myIp + ":" + port);
            Console.WriteLine("http: serving " + rootFolder);

            _running = true;
            _listener = new TcpListener(IPAddress.Any, port);

            _serverThread = new Thread(() => ServerLoop(rootFolder, port));
            _serverThread.Start();
            _pid = ProcessManager.Register("http", _serverThread, this);
            Console.WriteLine("http: pid " + _pid + " (kill to stop)");

            if (!DesktopManager.Running)
            {
                try
                {
                    while (_running)
                        Thread.Sleep(100);
                }
                finally
                {
                    _running = false;
                    try { _listener?.Stop(); } catch { }
                }
            }
        }

        private void ServerLoop(string rootFolder, int port)
        {
            try
            {
                _listener.Start();

                while (_running)
                {
                    TcpClient client = null;
                    try
                    {
                        if (_listener.Pending())
                        {
                            client = _listener.AcceptTcpClient();
                        }
                        else
                        {
                            Thread.Sleep(50);
                            continue;
                        }
                    }
                    catch
                    {
                        if (!_running) break;
                        try { _listener.Stop(); } catch { }
                        _listener = new TcpListener(IPAddress.Any, port);
                        try { _listener.Start(); } catch { }
                        continue;
                    }
                    if (client != null) HandleClient(client, rootFolder);
                }
            }
            catch (Exception e)
            {
                if (_running) Console.WriteLine("http: " + e.Message);
            }
            finally
            {
                _running = false;
                ProcessManager.Unregister(_pid);
                Console.WriteLine("http: stopped");
            }
        }

        private void HandleClient(TcpClient client, string rootFolder)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                string request = ReadRequest(stream);
                if (request == null)
                {
                    stream.Close();
                    client.Close();
                    return;
                }

                string firstLine = request.Split('\n')[0].Trim();
                string[] parts = firstLine.Split(' ');
                if (parts.Length < 2)
                {
                    stream.Close();
                    client.Close();
                    return;
                }

                string urlPath = parts[1].Split('?')[0];

                if (urlPath == "/favicon.ico")
                {
                    SendResponse(stream, "404 Not Found", "text/plain", "");
                }
                else if (urlPath == "/stop")
                {
                    SendResponse(stream, "200 OK", "text/html", "<h1>Stopped.</h1>");
                    stream.Close();
                    client.Close();
                    throw new Exception("stop requested");
                }
                else
                {
                    HandleRequest(stream, urlPath, rootFolder);
                }

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                if (e.Message == "stop requested") throw;
                try { client.Close(); } catch { }
            }
        }

        private string ReadRequest(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            int total = 0;

            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) return null;
                total = bytesRead;
            }
            catch { return null; }

            return Encoding.ASCII.GetString(buffer, 0, total);
        }

        private void HandleRequest(NetworkStream stream, string urlPath, string rootFolder)
        {
            string filePath = rootFolder.TrimEnd('/') + "/" + urlPath.TrimStart('/');

            if (urlPath == "/" || Directory.Exists(filePath))
            {
                if (urlPath == "/") filePath = rootFolder;
                string index = Path.Combine(filePath, "index.html");
                if (File.Exists(index))
                    filePath = index;
                else
                {
                    string listing = BuildDirectoryListing(urlPath, filePath);
                    SendResponse(stream, "200 OK", "text/html", listing);
                    return;
                }
            }

            if (!File.Exists(filePath))
            {
                SendResponse(stream, "404 Not Found", "text/html",
                    "<html><body><h1>404</h1></body></html>");
                return;
            }

            if (!PermissionManager.CanAccess(filePath, UserManager.currentUser))
            {
                SendResponse(stream, "403 Forbidden", "text/html",
                    "<html><body><h1>403</h1></body></html>");
                return;
            }

            string ext = "";
            int dotIdx = filePath.LastIndexOf('.');
            if (dotIdx >= 0) ext = filePath.Substring(dotIdx).ToLower();
            string mime = MimeTypes.ContainsKey(ext) ? MimeTypes[ext] : "application/octet-stream";

            byte[] fileData = File.ReadAllBytes(filePath);

            string header =
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: " + mime + "\r\n" +
                "Content-Length: " + fileData.Length + "\r\n" +
                "Connection: close\r\n\r\n";

            byte[] headerBytes = Encoding.ASCII.GetBytes(header);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(fileData, 0, fileData.Length);
        }

        private void SendResponse(NetworkStream stream, string status, string contentType, string body)
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
            string header =
                "HTTP/1.1 " + status + "\r\n" +
                "Content-Type: " + contentType + "\r\n" +
                "Content-Length: " + bodyBytes.Length + "\r\n" +
                "Connection: close\r\n\r\n";
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
                var entries = Directory.GetFileSystemEntries(dirPath);
                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        bool isDir = Directory.Exists(entry);
                        string name = Path.GetFileName(entry);
                        string href = urlPath.TrimEnd('/') + "/" + name + (isDir ? "/" : "");
                        string label = isDir ? "[" + name + "]" : name;
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
