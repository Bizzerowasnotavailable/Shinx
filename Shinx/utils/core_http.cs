using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.Network.Http;

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

        private bool _stopRequested;

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

            if (args.Length > 2)
            {
                Console.WriteLine("usage: http [root] [port]");
                return;
            }

            string rootFolder = args.Length > 0
                ? ResolvePath(args[0])
                : Shell.currentDirectory;

            int port = 8080;
            if (args.Length > 1 && !int.TryParse(args[1], out port))
            {
                Console.WriteLine("http: invalid port");
                return;
            }

            string myIp = NetworkManager.CurrentIP;
            Console.WriteLine("http: serving " + rootFolder);
            Console.WriteLine("http: http://" + myIp + ":" + port);
            Console.WriteLine("http: navigate to http://" + myIp + ":" + port + "/stop to stop");

            _stopRequested = false;

            var listener = new HttpListener("http", port);

            try
            {
                listener.Start();

                while (!_stopRequested && listener.IsListening)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = listener.GetContext();
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        if (_stopRequested)
                        {
                            break;
                        }

                        Console.WriteLine("http: client error: " + e.Message);
                        continue;
                    }

                    if (context == null)
                    {
                        break;
                    }

                    try
                    {
                        HandleContext(context, rootFolder);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("http: client error: " + e.Message);
                    }

                    if (_stopRequested)
                    {
                        try { listener.Stop(); } catch { }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (!_stopRequested)
                {
                    Console.WriteLine("http: " + e.Message);
                }
            }
            finally
            {
                try { listener.Close(); } catch { }
            }
        }

        private void HandleContext(HttpListenerContext context, string rootFolder)
        {
            HttpListenerRequest request = context.Request;
            string urlPath = request.RawUrl ?? "/";
            int question = urlPath.IndexOf('?');
            if (question >= 0)
            {
                urlPath = urlPath.Substring(0, question);
            }

            Console.WriteLine("http: " + request.HttpMethod + " " + urlPath);

            FileResponse result = BuildResponse(urlPath, rootFolder);
            WriteResponse(context, result);
        }

        private FileResponse BuildResponse(string urlPath, string rootFolder)
        {
            if (urlPath.IndexOf('\\') >= 0 || HasParentSegment(urlPath))
            {
                return FileResponse.Html(403, "Forbidden", "<html><body><h1>403</h1></body></html>");
            }

            if (urlPath == "/favicon.ico")
            {
                return FileResponse.Html(404, "Not Found", string.Empty);
            }

            if (urlPath == "/stop")
            {
                _stopRequested = true;
                return new FileResponse
                {
                    StatusCode = 200,
                    ContentType = "text/html",
                    Body = Encoding.UTF8.GetBytes("<h1>Stopped.</h1>"),
                    CloseConnection = true,
                };
            }

            string filePath = rootFolder.TrimEnd('/') + "/" + urlPath.TrimStart('/');

            if (urlPath == "/" || Directory.Exists(filePath))
            {
                if (urlPath == "/")
                {
                    filePath = rootFolder;
                }

                string index = Path.Combine(filePath, "index.html");
                if (File.Exists(index))
                {
                    filePath = index;
                }
                else
                {
                    string listing = BuildDirectoryListing(urlPath, filePath);
                    return FileResponse.Html(200, "OK", listing);
                }
            }

            if (!File.Exists(filePath))
            {
                return FileResponse.Html(404, "Not Found", "<html><body><h1>404</h1></body></html>");
            }

            if (!PermissionManager.CanAccess(filePath, UserManager.currentUser))
            {
                return FileResponse.Html(403, "Forbidden", "<html><body><h1>403</h1></body></html>");
            }

            string ext = "";
            int dotIdx = filePath.LastIndexOf('.');
            if (dotIdx >= 0)
            {
                ext = filePath.Substring(dotIdx).ToLower();
            }

            return new FileResponse
            {
                StatusCode = 200,
                ContentType = MimeTypes.ContainsKey(ext) ? MimeTypes[ext] : "application/octet-stream",
                Body = File.ReadAllBytes(filePath),
            };
        }

        private static void WriteResponse(HttpListenerContext context, FileResponse result)
        {
            HttpListenerResponse response = context.Response;
            try
            {
                response.StatusCode = result.StatusCode;
                response.ContentType = result.ContentType;
                response.ContentLength64 = result.Body.Length;
                response.KeepAlive = result.CloseConnection ? false : context.Request.KeepAlive;

                bool head = string.Equals(context.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase);
                if (!head && result.Body.Length > 0)
                {
                    response.OutputStream.Write(result.Body, 0, result.Body.Length);
                }

                response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("http: response error: " + e.Message);
                try { response.Close(); } catch { }
            }
        }

        private static string ResolvePath(string path)
        {
            return Shell.ResolvePath(path);
        }

        private static bool HasParentSegment(string path)
        {
            int start = 0;
            while (start < path.Length)
            {
                int slash = path.IndexOf('/', start);
                int end = slash < 0 ? path.Length : slash;
                if (end - start == 2 && path[start] == '.' && path[start + 1] == '.')
                {
                    return true;
                }

                if (slash < 0)
                {
                    break;
                }

                start = slash + 1;
            }

            return false;
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
            {
                sb.Append("<li><a href=\"..\">[..]</a></li>");
            }

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
            catch
            {
                sb.Append("<li>error reading directory</li>");
            }

            sb.Append("</ul></body></html>");
            return sb.ToString();
        }

        private sealed class FileResponse
        {
            public int StatusCode;
            public string ContentType;
            public byte[] Body;
            public bool CloseConnection;

            public static FileResponse Html(int statusCode, string contentType, string body)
            {
                return new FileResponse
                {
                    StatusCode = statusCode,
                    ContentType = contentType,
                    Body = Encoding.UTF8.GetBytes(body),
                };
            }
        }

        public void Usage()
        {
            Console.WriteLine("usage: http [root] [port]");
        }
    }
}
