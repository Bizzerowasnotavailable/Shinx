using Shinx.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UniLua;

namespace Shinx.Commands
{
    internal class core_pkg : ICommand
    {
        private const string RepoHost = "185.199.108.153";
        private const string RepoName = "repo.izzoserver.top";
        private const int RepoPort = 80;
        private const string BinDir = "/bin/";
        private const string ManifestPath = "/bin/lpkg.txt";

        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (!NetworkManager.IsConnected)
            {
                Console.WriteLine("lpkg: no network connection");
                return;
            }

            if (!UserManager.IsRoot(UserManager.currentUser))
            {
                Console.WriteLine("lpkg: must be root");
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("usage: lpkg list | install <name> | remove <name> | upgrade [name]");
                return;
            }

            switch (args[0])
            {
                case "list":
                    List();
                    break;
                case "install":
                    if (args.Length < 2) { Console.WriteLine("pkg: missing package name"); return; }
                    Install(args[1]);
                    break;
                case "remove":
                    if (args.Length < 2) { Console.WriteLine("pkg: missing package name"); return; }
                    Remove(args[1]);
                    break;
                case "upgrade":
                    if (args.Length < 2) Upgrade();
                    else Upgrade(args[1]);
                    break;
                default:
                    Console.WriteLine("lpkg: unknown command: " + args[0]);
                    break;
            }
        }
        private void List()
        {
            string body = HttpGet("/index.txt");
            if (body == null) return;

            var manifest = LoadManifest();
            Console.WriteLine("available packages:");

            foreach (var line in body.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                var parts = trimmed.Split(' ');
                string name = parts[0];
                string remoteVer = parts.Length > 1 ? parts[1] : "?";

                if (manifest.ContainsKey(name))
                {
                    string localVer = manifest[name];
                    string tag = IsNewer(remoteVer, localVer)
                        ? "[outdated: " + remoteVer + " available]"
                        : "[installed]";
                    Console.WriteLine("  " + name + " " + localVer + " " + tag);
                }
                else
                {
                    Console.WriteLine("  " + name + " " + remoteVer);
                }
            }
        }
        private void Install(string name)
        {
            string index = HttpGet("/index.txt");
            string version = "1.0";
            string expectedHash = null;

            if (index != null)
            {
                foreach (var line in index.Split('\n'))
                {
                    var parts = line.Trim().Split(' ');
                    if (parts.Length > 0 && parts[0] == name)
                    {
                        if (parts.Length > 1) version = parts[1];
                        if (parts.Length > 2) expectedHash = parts[2];
                        break;
                    }
                }
            }

            var manifest = LoadManifest();
            if (manifest.ContainsKey(name) && !IsNewer(version, manifest[name]))
            {
                Console.WriteLine("lpkg: " + name + " " + manifest[name] + " is already up to date");
                return;
            }

            Console.WriteLine("lpkg: fetching " + name + " " + version + "...");
            byte[] data = HttpGetRaw("/packages/" + name + ".lua");
            if (data == null) return;

            if (expectedHash != null)
            {
                Console.WriteLine("lpkg: verifying...");
                string actualHash = core_sha256.HashBytes(data);
                if (actualHash != expectedHash)
                {
                    Console.WriteLine("lpkg: checksum mismatch! aborting");
                    Console.WriteLine("lpkg:   expected " + expectedHash);
                    Console.WriteLine("lpkg:   got      " + actualHash);
                    return;
                }
                Console.WriteLine("lpkg: checksum ok");
            }
            else
            {
                Console.WriteLine("lpkg: warning: no checksum for " + name);
            }

            File.WriteAllText(BinDir + name + ".lua", Encoding.UTF8.GetString(data));

            try
            {
                string code = Encoding.UTF8.GetString(data);
                var status = LuaBridge.State.L_DoString(code);
                if (status != ThreadStatus.LUA_OK)
                {
                    Console.WriteLine("lpkg: register error: " + LuaBridge.State.L_ToString(-1));
                    LuaBridge.State.Pop(1);
                    return;
                }
            }
catch (Exception e)
            {
                Console.WriteLine("lpkg: register error: " + e.Message);
                return;
            }

            manifest[name] = version;
            SaveManifest(manifest);

            Console.WriteLine("lpkg: installed " + name + " " + version);
        }

        private void Remove(string name)
        {
            string path = BinDir + name + ".lua";
            if (!File.Exists(path))
            {
                Console.WriteLine("lpkg: not installed: " + name);
                return;
            }

            File.Delete(path);

            if (peppe.commands.ContainsKey(name))
                peppe.UnregisterCommand(name);
            else
                Console.WriteLine("lpkg: warning: " + name + " was not registered as a command");

            var app = AppManager.GetApp(name);
            if (app != null)
            {
                AppManager.Apps.Remove(app);
                WindowManager.drawOrder.Remove(name);
            }

            var manifest = LoadManifest();
            manifest.Remove(name);
            SaveManifest(manifest);

            Console.WriteLine("lpkg: removed " + name);
        }

        private void Upgrade(string name = null)
        {
            string index = HttpGet("/index.txt");
            if (index == null) return;

            var manifest = LoadManifest();
            if (manifest.Count == 0) { Console.WriteLine("lpkg: nothing installed"); return; }

            bool any = false;
            foreach (var line in index.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                var parts = trimmed.Split(' ');
                if (parts.Length < 2) continue;

                string pkgName = parts[0];
                string remoteVer = parts[1];

                if (name != null && pkgName != name) continue;
                if (!manifest.ContainsKey(pkgName)) continue;
                if (!IsNewer(remoteVer, manifest[pkgName])) continue;

                Console.WriteLine("lpkg: upgrading " + pkgName + " " + manifest[pkgName] + " -> " + remoteVer);
                Install(pkgName);
                any = true;
            }

            if (!any)
                Console.WriteLine("lpkg: everything is up to date");
        }

        private Dictionary<string, string> LoadManifest()
        {
            var manifest = new Dictionary<string, string>();
            if (!File.Exists(ManifestPath)) return manifest;
            string content = File.ReadAllText(ManifestPath);
            foreach (var line in content.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;
                var parts = trimmed.Split(' ');
                if (parts.Length == 2)
                    manifest[parts[0]] = parts[1];
            }
            return manifest;
        }
        private void SaveManifest(Dictionary<string, string> manifest)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var kv in manifest)
                sb.Append(kv.Key + " " + kv.Value + "\n");
            File.WriteAllText(ManifestPath, sb.ToString());
        }
        private byte[] HttpGetRaw(string path)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(RepoHost, RepoPort);
                    NetworkStream stream = client.GetStream();

                    string req =
                        "GET " + path + " HTTP/1.0\r\n" +
                        "Host: " + RepoName + "\r\n" +
                        "Connection: close\r\n\r\n";

                    byte[] reqBytes = Encoding.ASCII.GetBytes(req);
                    stream.Write(reqBytes, 0, reqBytes.Length);

                    var resp = new List<byte>();
                    byte[] buf = new byte[1024];
                    int read;
                    while ((read = stream.Read(buf, 0, buf.Length)) > 0)
                        for (int i = 0; i < read; i++) resp.Add(buf[i]);

                    stream.Close();

                    byte[] raw = resp.ToArray();

                    for (int i = 0; i < raw.Length - 3; i++)
                    {
                        if (raw[i] == '\r' && raw[i + 1] == '\n' && raw[i + 2] == '\r' && raw[i + 3] == '\n')
                        {
                            string header = Encoding.ASCII.GetString(raw, 0, i);
                            if (header.Contains(" 404 "))
                            {
                                Console.WriteLine("lpkg: not found on server");
                                return null;
                            }

                            byte[] body = new byte[raw.Length - (i + 4)];
                            Array.Copy(raw, i + 4, body, 0, body.Length);
                            return body;
                        }
                    }

                    return raw;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("lpkg: " + e.Message);
                return null;
            }
        }
        private string HttpGet(string path)
        {
            byte[] raw = HttpGetRaw(path);
            return raw != null ? Encoding.UTF8.GetString(raw) : null;
        }
        private bool IsNewer(string remote, string local)
        {
            var r = remote.Split('.');
            var l = local.Split('.');
            int rMaj = int.Parse(r[0]), rMin = r.Length > 1 ? int.Parse(r[1]) : 0;
            int lMaj = int.Parse(l[0]), lMin = l.Length > 1 ? int.Parse(l[1]) : 0;
            if (rMaj != lMaj) return rMaj > lMaj;
            return rMin > lMin;
        }
    }
}