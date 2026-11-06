using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Text;
using Cosmos.Network.Http;

namespace Shinx.Commands
{
    public class core_wget : ICommand
    {
        private const int MaxRedirects = 5;

        public void Execute(string[] args, HashSet<char> parameters)
        {
            foreach (char p in parameters)
            {
                if (p != 'q' && p != 'O')
                {
                    Console.WriteLine("wget: unknown option: -" + p);
                    return;
                }
            }

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string url;
            string output = null;

            if (parameters.Contains('O'))
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("wget: -O requires a URL and an output file");
                    return;
                }

                if (IsHttpUrl(args[0]) && !IsHttpUrl(args[1]))
                {
                    url = args[0];
                    output = args[1];
                }
                else if (!IsHttpUrl(args[0]) && IsHttpUrl(args[1]))
                {
                    url = args[1];
                    output = args[0];
                }
                else
                {
                    Console.WriteLine("wget: -O requires a URL and an output file");
                    return;
                }
            }
            else
            {
                if (args.Length > 2)
                {
                    PrintUsage();
                    return;
                }

                url = args[0];
                if (args.Length == 2)
                {
                    output = args[1];
                }
            }

            if (!NetworkManager.IsConnected)
            {
                Console.WriteLine("wget: no network connection");
                return;
            }

            bool quiet = parameters.Contains('q');

            try
            {
                WebUrl parsed = ParseUrl(url);
                if (parsed == null)
                {
                    Console.WriteLine("wget: only http:// and https:// URLs are supported");
                    return;
                }

                string destination = PrepareDestination(output, parsed);
                if (destination == null)
                {
                    return;
                }

                if (!quiet)
                {
                    Console.WriteLine("wget: " + parsed.UrlText);
                }

                DownloadResult response = SendRequest(parsed, quiet);
                if (response.StatusCode < 200 || response.StatusCode >= 300)
                {
                    Console.WriteLine("wget: HTTP " + response.StatusCode + " " + response.ReasonPhrase);
                    return;
                }

                File.WriteAllBytes(destination, response.Body);
                PermissionManager.SetDefault(destination, UserManager.currentUser);

                if (!quiet)
                {
                    Console.WriteLine("wget: saved " + destination + " (" + response.Body.Length + " bytes)");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("wget: " + e.Message);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("usage: wget [-q] [-O <file>] <url> [output]");
        }

        private static string PrepareDestination(string output, WebUrl url)
        {
            bool explicitOutput = !string.IsNullOrEmpty(output);
            string fileName = GetFileName(url);
            string path;

            if (explicitOutput)
            {
                path = ResolvePath(output);
                if (Directory.Exists(path) || output.EndsWith("/"))
                {
                    path = path.TrimEnd('/') + "/" + fileName;
                }
            }
            else
            {
                path = ResolvePath(fileName);
            }

            if (Directory.Exists(path))
            {
                Console.WriteLine("wget: output is a directory: " + path);
                return null;
            }

            if (!explicitOutput)
            {
                path = UniquePath(path);
            }

            string parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent))
            {
                parent = "/";
            }

            if (!Directory.Exists(parent))
            {
                Console.WriteLine("wget: no such directory: " + parent);
                return null;
            }

            if (!PermissionManager.CanAccess(parent, UserManager.currentUser))
            {
                Console.WriteLine("wget: permission denied: " + path);
                return null;
            }

            return path;
        }

        private static string UniquePath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                return path;
            }

            string directory = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);
            int dot = name.LastIndexOf('.');
            string baseName = dot > 0 ? name.Substring(0, dot) : name;
            string extension = dot > 0 ? name.Substring(dot) : "";

            for (int i = 1; i < 1000; i++)
            {
                string candidate = Path.Combine(directory, baseName + "." + i + extension);
                if (!File.Exists(candidate) && !Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new Exception("could not choose an output file name");
        }

        private static string GetFileName(WebUrl url)
        {
            string path = url.Target;
            int question = path.IndexOf('?');
            if (question >= 0)
            {
                path = path.Substring(0, question);
            }

            int slash = path.LastIndexOf('/');
            string name = slash >= 0 ? path.Substring(slash + 1) : path;
            name = PercentDecode(name);

            if (name.Length == 0 || name == "." || name == "..")
            {
                name = "index.html";
            }

            char[] chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (c < 32 || c == '\\' || c == '/' || c == ':' || c == '*' || c == '?'
                    || c == '"' || c == '<' || c == '>' || c == '|')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private static string PercentDecode(string value)
        {
            var bytes = new List<byte>(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '%' && i + 2 < value.Length)
                {
                    int high = HexValue(value[i + 1]);
                    int low = HexValue(value[i + 2]);
                    if (high >= 0 && low >= 0)
                    {
                        bytes.Add((byte)((high << 4) | low));
                        i += 2;
                        continue;
                    }
                }

                if (c < 128)
                {
                    bytes.Add((byte)c);
                }
                else
                {
                    byte[] encoded = Encoding.UTF8.GetBytes(c.ToString());
                    for (int j = 0; j < encoded.Length; j++)
                    {
                        bytes.Add(encoded[j]);
                    }
                }
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private static int HexValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return -1;
        }

        private static string ResolvePath(string path)
        {
            return Shell.ResolvePath(path);
        }

        private static DownloadResult SendRequest(WebUrl start, bool quiet)
        {
            WebUrl current = start;

            using (var client = new Cosmos.Network.Http.HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60),
                SslProtocols = SslProtocols.None,
            })
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Shinx-wget/1.0");
                client.DefaultRequestHeaders.Add("Accept", "*/*");

                for (int redirects = 0; redirects <= MaxRedirects; redirects++)
                {
                    using (Cosmos.Network.Http.HttpResponseMessage response = client.Get(
                        current.UrlText,
                        Cosmos.Network.Http.HttpCompletionOption.ResponseContentRead))
                    {
                        int statusCode = (int)response.StatusCode;

                        if (IsRedirect(statusCode))
                        {
                            string location = GetLocation(response);
                            if (string.IsNullOrEmpty(location))
                            {
                                throw new Exception("redirect response has no Location header");
                            }

                            WebUrl next = ResolveLocation(current, location);
                            if (next == null)
                            {
                                throw new Exception("invalid redirect location: " + location);
                            }

                            if (!quiet)
                            {
                                Console.WriteLine("wget: redirected to " + next.UrlText);
                            }

                            current = next;
                            continue;
                        }

                        byte[] body = response.Content == null
                            ? new byte[0]
                            : response.Content.ReadAsByteArray();

                        return new DownloadResult(statusCode, response.ReasonPhrase, body);
                    }
                }
            }

            throw new Exception("too many redirects");
        }

        private static string GetLocation(Cosmos.Network.Http.HttpResponseMessage response)
        {
            IEnumerable<string> values;
            if (response.Headers.TryGetValues("Location", out values))
            {
                foreach (string value in values)
                {
                    return value;
                }
            }

            return null;
        }

        private static bool IsRedirect(int statusCode)
        {
            return statusCode == 301
                || statusCode == 302
                || statusCode == 303
                || statusCode == 307
                || statusCode == 308;
        }

        private static WebUrl ResolveLocation(WebUrl baseUrl, string location)
        {
            location = location.Trim();
            if (location.Length == 0)
            {
                return null;
            }

            if (IsHttpUrl(location))
            {
                return ParseUrl(location);
            }

            string prefix = baseUrl.Scheme + "://" + baseUrl.Authority;

            if (location.StartsWith("//"))
            {
                return ParseUrl(baseUrl.Scheme + ":" + location);
            }

            if (location.StartsWith("/"))
            {
                return ParseUrl(prefix + location);
            }

            if (location.StartsWith("?"))
            {
                string path = baseUrl.Target;
                int question = path.IndexOf('?');
                if (question >= 0)
                {
                    path = path.Substring(0, question);
                }

                return ParseUrl(prefix + path + location);
            }

            string basePath = baseUrl.Target;
            int baseQuestion = basePath.IndexOf('?');
            if (baseQuestion >= 0)
            {
                basePath = basePath.Substring(0, baseQuestion);
            }

            int slash = basePath.LastIndexOf('/');
            string directory = slash >= 0 ? basePath.Substring(0, slash + 1) : "/";
            return ParseUrl(prefix + directory + location);
        }

        private static bool IsHttpUrl(string value)
        {
            return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private static WebUrl ParseUrl(string url)
        {
            bool useTls;
            int prefixLength;

            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                useTls = true;
                prefixLength = 8;
            }
            else if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                useTls = false;
                prefixLength = 7;
            }
            else
            {
                return null;
            }

            string rest = url.Substring(prefixLength);
            int hash = rest.IndexOf('#');
            if (hash >= 0)
            {
                rest = rest.Substring(0, hash);
            }

            int slash = rest.IndexOf('/');
            int question = rest.IndexOf('?');
            int split = slash;
            if (question >= 0 && (split < 0 || question < split))
            {
                split = question;
            }

            string authority = split >= 0 ? rest.Substring(0, split) : rest;
            string target = split >= 0 ? rest.Substring(split) : "/";

            if (target.Length == 0)
            {
                target = "/";
            }
            else if (target[0] == '?')
            {
                target = "/" + target;
            }
            else if (target[0] != '/')
            {
                target = "/" + target;
            }

            int at = authority.LastIndexOf('@');
            if (at >= 0)
            {
                authority = authority.Substring(at + 1);
            }

            if (authority.Length == 0)
            {
                return null;
            }

            string host = authority;
            int port = useTls ? 443 : 80;
            int colon = authority.LastIndexOf(':');
            if (colon > 0 && colon > authority.LastIndexOf(']'))
            {
                string portText = authority.Substring(colon + 1);
                if (portText.Length == 0 || !int.TryParse(portText, out port) || port <= 0 || port > 65535)
                {
                    return null;
                }

                host = authority.Substring(0, colon);
            }

            if (host.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < host.Length; i++)
            {
                char c = host[i];
                if (c <= 32 || c >= 127)
                {
                    return null;
                }
            }

            for (int i = 0; i < target.Length; i++)
            {
                char c = target[i];
                if (c <= 32 || c == 127)
                {
                    return null;
                }
            }

            return new WebUrl(useTls, host, port, target);
        }

        private sealed class DownloadResult
        {
            public DownloadResult(int statusCode, string reasonPhrase, byte[] body)
            {
                StatusCode = statusCode;
                ReasonPhrase = reasonPhrase ?? string.Empty;
                Body = body;
            }

            public int StatusCode { get; }

            public string ReasonPhrase { get; }

            public byte[] Body { get; }
        }

        private sealed class WebUrl
        {
            public WebUrl(bool useTls, string host, int port, string target)
            {
                UseTls = useTls;
                Host = host;
                Port = port;
                Target = target;
            }

            public bool UseTls { get; }

            public string Host { get; }

            public int Port { get; }

            public string Target { get; }

            public string Scheme
            {
                get { return UseTls ? "https" : "http"; }
            }

            public string Authority
            {
                get
                {
                    bool defaultPort = UseTls ? Port == 443 : Port == 80;
                    return defaultPort ? Host : Host + ":" + Port;
                }
            }

            public string UrlText
            {
                get { return Scheme + "://" + Authority + Target; }
            }
        }

        public void Usage()
        {
            Console.WriteLine("usage: wget [-q] [-O <file>] <url> [output]");
        }
    }
}
