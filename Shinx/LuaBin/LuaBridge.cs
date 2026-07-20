using Cosmos.Kernel.System.Graphics;
using Shinx.Commands;
using Shinx.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UniLua;
using Cosmos.Kernel.System.Mouse;

namespace Shinx
{
    public static class LuaBridge
    {

        private static Canvas _guiCanvas;
        private static int _guiX, _guiY, _guiW, _guiH;
        private static bool _inGuiDraw = false;
        public static string InputFocus => _inputFocus;

        private static readonly Dictionary<string, string> _inputState = new Dictionary<string, string>();
        private static readonly Dictionary<string, bool> _checkboxState = new Dictionary<string, bool>();
        private static string _inputFocus = "";
        private static readonly Queue<ConsoleKeyInfo> _inputQueue = new Queue<ConsoleKeyInfo>();

        public static void EnqueueKey(ConsoleKeyInfo key)
        {
            if (string.IsNullOrEmpty(_inputFocus)) return;
            if (!_inputState.ContainsKey(_inputFocus)) return;

            if (key.Key == ConsoleKey.Backspace)
            {
                if (_inputState[_inputFocus].Length > 0)
                    _inputState[_inputFocus] = _inputState[_inputFocus].Substring(0, _inputState[_inputFocus].Length - 1);
            }
            else if (key.KeyChar >= 32 && key.KeyChar < 127)
            {
                _inputState[_inputFocus] += key.KeyChar;
            }
        }
        public static void SetGuiCanvas(Canvas canvas, int x, int y, int w, int h)
        {
            _guiCanvas = canvas;
            _guiX = x; _guiY = y; _guiW = w; _guiH = h;
            _inGuiDraw = true;
        }
        public static void ClearGuiCanvas()
        {
            _guiCanvas = null;
            _inGuiDraw = false;
        }
        private static readonly string[] blockedCommands = { "rm", "userdel", "groupdel", "chown", "chgrp", "useradd", "http", "net", "lua", "passwd", "su" };
        public static void Setup(ILuaState state)
        {
            state.PushNil(); state.SetGlobal("load");
            state.PushNil(); state.SetGlobal("loadfile");
            state.PushNil(); state.SetGlobal("dofile");
            state.PushNil(); state.SetGlobal("require");
            state.PushNil(); state.SetGlobal("io");
            state.PushNil(); state.SetGlobal("os");
            state.PushNil(); state.SetGlobal("debug");

            state.NewTable();

            state.PushCSharpFunction(L_WriteLine); state.SetField(-2, "writeline");
            state.PushCSharpFunction(L_Write); state.SetField(-2, "write");
            state.PushCSharpFunction(L_Clear); state.SetField(-2, "clear");
            state.PushCSharpFunction(L_Color); state.SetField(-2, "color");
            state.PushCSharpFunction(L_ResetColor); state.SetField(-2, "resetcolor");
            state.PushCSharpFunction(L_Read); state.SetField(-2, "read");
            state.PushCSharpFunction(L_ReadLine); state.SetField(-2, "readline");
            state.PushCSharpFunction(L_Exec); state.SetField(-2, "exec");
            state.PushCSharpFunction(L_CurrentDir); state.SetField(-2, "currentdir");
            state.PushCSharpFunction(L_SetDir); state.SetField(-2, "setdir");
            state.PushCSharpFunction(L_CurrentUser); state.SetField(-2, "currentuser");
            state.PushCSharpFunction(L_IsRoot); state.SetField(-2, "isroot");
            state.PushCSharpFunction(L_Register); state.SetField(-2, "register");
            state.PushCSharpFunction(L_Args); state.SetField(-2, "args");
            state.PushCSharpFunction(L_Params); state.SetField(-2, "params");
            state.PushCSharpFunction(L_Time); state.SetField(-2, "time");
            state.PushCSharpFunction(L_ListDir); state.SetField(-2, "listdir");
            state.PushCSharpFunction(L_ReadFile); state.SetField(-2, "readfile");
            state.PushCSharpFunction(L_WriteFile); state.SetField(-2, "writefile");
            state.PushCSharpFunction(L_Exists); state.SetField(-2, "exists");
            state.PushCSharpFunction(L_MkDir); state.SetField(-2, "mkdir");
            state.PushCSharpFunction(L_Delete); state.SetField(-2, "delete");
            state.PushCSharpFunction(L_MoveFile); state.SetField(-2, "movefile");
            state.PushCSharpFunction(L_CopyFile); state.SetField(-2, "copyfile");
            state.PushCSharpFunction(L_CanAccess); state.SetField(-2, "canaccess");
            state.PushCSharpFunction(L_GetOwner); state.SetField(-2, "getowner");
            state.PushCSharpFunction(L_CPUInfo); state.SetField(-2, "fetchcpu");
            state.PushCSharpFunction(L_RAMInfo); state.SetField(-2, "fetchram");
            state.PushCSharpFunction(L_Sleep); state.SetField(-2, "sleep");
            state.PushCSharpFunction(L_SetCursor); state.SetField(-2, "setcursor");
            state.PushCSharpFunction(L_HasKey); state.SetField(-2, "haskey");
            state.PushCSharpFunction(L_GetKey); state.SetField(-2, "getkey");
            state.PushCSharpFunction(L_HideCursor); state.SetField(-2, "hidecursor");
            state.PushCSharpFunction(L_ShowCursor); state.SetField(-2, "showcursor");
            state.PushCSharpFunction(L_NetIsConnected); state.SetField(-2, "netconnected");
            state.PushCSharpFunction(L_NetStatus); state.SetField(-2, "netstatus");
            state.PushCSharpFunction(L_NetResolve); state.SetField(-2, "netresolve");
            state.PushCSharpFunction(L_NetGet); state.SetField(-2, "netget");
            state.PushCSharpFunction(L_HttpServe); state.SetField(-2, "httpserve");
            state.PushCSharpFunction(L_GuiRegister); state.SetField(-2, "gui_register");
            state.PushCSharpFunction(L_GuiClose); state.SetField(-2, "gui_close");
            state.PushCSharpFunction(L_GuiRect); state.SetField(-2, "gui_rect");
            state.PushCSharpFunction(L_GuiRectFill); state.SetField(-2, "gui_rectfill");
            state.PushCSharpFunction(L_GuiLine); state.SetField(-2, "gui_line");
            state.PushCSharpFunction(L_GuiMouse); state.SetField(-2, "gui_mouse");
            state.PushCSharpFunction(L_GuiClick); state.SetField(-2, "gui_click");
            state.PushCSharpFunction(L_GuiWidth); state.SetField(-2, "gui_width");
            state.PushCSharpFunction(L_GuiHeight); state.SetField(-2, "gui_height");
            state.PushCSharpFunction(L_GuiButton); state.SetField(-2, "gui_button");
            state.PushCSharpFunction(L_GuiLabel); state.SetField(-2, "gui_label");
            state.PushCSharpFunction(L_GuiTextbox); state.SetField(-2, "gui_textbox");
            state.PushCSharpFunction(L_GuiInput); state.SetField(-2, "gui_input");
            state.PushCSharpFunction(L_GuiCheckbox); state.SetField(-2, "gui_checkbox");
            state.PushCSharpFunction(L_GuiProgressbar); state.SetField(-2, "gui_progressbar");
            state.PushCSharpFunction(L_GuiClearState); state.SetField(-2, "gui_clearstate");

            state.SetGlobal("shinx");
        }
        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "/";
            if (path.StartsWith("/")) return path;
            return Path.Combine(Shell.currentDirectory, path).Replace('\\', '/');
        }
        private static int L_WriteLine(ILuaState lua) { var vc = VirtualConsole.Current; if (vc != null) vc.WriteLine(lua.L_ToString(1)); else Console.WriteLine(lua.L_ToString(1)); return 0; }
        private static int L_Write(ILuaState lua) { var vc = VirtualConsole.Current; if (vc != null) vc.Write(lua.L_ToString(1)); else Console.Write(lua.L_ToString(1)); return 0; }
        private static int L_Clear(ILuaState lua) { var vc = VirtualConsole.Current; if (vc != null) vc.Clear(); else Console.Clear(); return 0; }
        private static int L_Color(ILuaState lua)
        {
            string fg = lua.L_CheckString(1);
            var vc = VirtualConsole.Current;
            if (TryParseColor(fg, out ConsoleColor fgColor))
            {
                if (vc != null) vc.SetForeground(fgColor);
                else Console.ForegroundColor = fgColor;
            }

            if (lua.GetTop() >= 2 && lua.Type(2) == LuaType.LUA_TSTRING)
            {
                string bg = lua.L_CheckString(2);
                if (TryParseColor(bg, out ConsoleColor bgColor))
                {
                    if (vc != null) vc.SetBackground(bgColor);
                    else Console.BackgroundColor = bgColor;
                }
            }
            return 0;
        }
        private static int L_ResetColor(ILuaState lua) { var vc = VirtualConsole.Current; if (vc != null) vc.ResetColor(); else Console.ResetColor(); return 0; }
        private static int L_Read(ILuaState lua) { var vc = VirtualConsole.Current; lua.PushString(vc != null ? vc.ReadLine() : Console.ReadLine()); return 1; }
        private static int L_ReadLine(ILuaState lua) { string prompt = lua.L_CheckString(1); var vc = VirtualConsole.Current; if (vc != null) { vc.Write(prompt); lua.PushString(vc.ReadLine()); } else { Console.Write(prompt); lua.PushString(Console.ReadLine()); } return 1; }
        private static int L_Exec(ILuaState lua)
        {
            string cmd = lua.L_ToString(1);

            if (!UserManager.IsRoot(UserManager.currentUser))
            {
                foreach (string b in blockedCommands)
                {
                    if (cmd.StartsWith(b))
                    {
                        Console.WriteLine("lua: permission denied");
                        return 0;
                    }
                }
            }

            Kernel.commandHandler.Execute(cmd);
            return 0;
        }
        private static int L_CurrentDir(ILuaState lua) { lua.PushString(Shell.currentDirectory); return 1; }
        private static int L_SetDir(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                lua.PushBoolean(false); lua.PushString("permission denied"); return 2;
            }

            if (!Directory.Exists(path))
            {
                lua.PushBoolean(false); lua.PushString("no such directory"); return 2;
            }

            Shell.currentDirectory = path.Replace('\\', '/').TrimEnd('/') + "/";
            lua.PushBoolean(true); return 1;
        }
        private static int L_CurrentUser(ILuaState lua) { lua.PushString(UserManager.currentUser); return 1; }
        private static int L_IsRoot(ILuaState lua) { lua.PushBoolean(UserManager.IsRoot(UserManager.currentUser)); return 1; }
        private static int L_Register(ILuaState lua)
        {
            string name = lua.L_CheckString(1);
            string desc = lua.GetTop() >= 3 ? lua.L_CheckString(3) : "lua command";
            lua.PushValue(2);
            lua.SetGlobal("__cmd_" + name);
            peppe.RegisterCommand(name, new LuaCommand("__cmd_" + name), desc);
            return 0;
        }
        private static int L_Args(ILuaState lua) { lua.GetGlobal("__args"); return 1; }
        private static int L_Params(ILuaState lua) { lua.GetGlobal("__params"); return 1; }
        private static int L_Time(ILuaState lua) { lua.PushString(DateTime.Now.ToString("HH:mm:ss")); return 1; }
        private static int L_ListDir(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                lua.PushNil(); lua.PushString("permission denied"); return 2;
            }

            try
            {
                var dirs = Directory.GetDirectories(path);
                var files = Directory.GetFiles(path);

                lua.NewTable();
                int i = 1;

                foreach (var dir in dirs)
                {
                    lua.NewTable();
                    lua.PushString("name"); lua.PushString(Path.GetFileName(dir)); lua.SetTable(-3);
                    lua.PushString("type"); lua.PushString("dir"); lua.SetTable(-3);
                    lua.RawSetI(-2, i++);
                }

                foreach (var file in files)
                {
                    lua.NewTable();
                    lua.PushString("name"); lua.PushString(Path.GetFileName(file)); lua.SetTable(-3);
                    lua.PushString("type"); lua.PushString("file"); lua.SetTable(-3);
                    lua.RawSetI(-2, i++);
                }

                return 1;
            }
            catch (Exception e)
            {
                lua.PushNil(); lua.PushString(e.Message); return 2;
            }
        }
        private static int L_ReadFile(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                lua.PushNil(); lua.PushString("permission denied"); return 2;
            }

            try { string content = File.ReadAllText(path); lua.PushString(content); return 1; }
            catch (Exception e) { lua.PushNil(); lua.PushString(e.Message); return 2; }
        }
        private static int L_WriteFile(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            string data = lua.L_CheckString(2);

            string parent = Path.GetDirectoryName(path);
            if (!PermissionManager.CanAccess(parent ?? path, UserManager.currentUser))
            {
                lua.PushString("permission denied"); return 1;
            }

            try { File.WriteAllText(path, data); return 0; }
            catch (Exception e) { lua.PushString(e.Message); return 1; }
        }

        private static int L_Exists(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            bool exists = File.Exists(path) || Directory.Exists(path);
            lua.PushBoolean(exists); return 1;
        }
        private static int L_MkDir(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            string parent = Path.GetDirectoryName(path);

            if (!PermissionManager.CanAccess(parent ?? path, UserManager.currentUser))
            {
                lua.PushBoolean(false); lua.PushString("permission denied"); return 2;
            }

            try
            {
                if (Directory.Exists(path))
                {
                    lua.PushBoolean(false); lua.PushString("directory already exists"); return 2;
                }
                Directory.CreateDirectory(path);
                PermissionManager.SetDefault(path, UserManager.currentUser);
                lua.PushBoolean(true); return 1;
            }
            catch (Exception e)
            {
                lua.PushBoolean(false); lua.PushString(e.Message); return 2;
            }
        }
        private static int L_Delete(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                lua.PushBoolean(false); lua.PushString("permission denied"); return 2;
            }

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                else if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else
                {
                    lua.PushBoolean(false); lua.PushString("no such file or directory"); return 2;
                }
                lua.PushBoolean(true); return 1;
            }
            catch (Exception e)
            {
                lua.PushBoolean(false); lua.PushString(e.Message); return 2;
            }
        }
        private static int L_MoveFile(ILuaState lua)
        {
            string src = ResolvePath(lua.L_CheckString(1));
            string dst = ResolvePath(lua.L_CheckString(2));

            if (!PermissionManager.CanAccess(src, UserManager.currentUser))
            {
                lua.PushBoolean(false); lua.PushString("permission denied: source"); return 2;
            }

            string dstParent = Path.GetDirectoryName(dst);
            if (!PermissionManager.CanAccess(dstParent ?? dst, UserManager.currentUser))
            {
                lua.PushBoolean(false); lua.PushString("permission denied: destination"); return 2;
            }

            try { File.WriteAllBytes(dst, File.ReadAllBytes(src)); File.Delete(src); lua.PushBoolean(true); return 1; }
            catch (Exception e) { lua.PushBoolean(false); lua.PushString(e.Message); return 2; }
        }
        private static int L_CopyFile(ILuaState lua)
        {
            string src = ResolvePath(lua.L_CheckString(1));
            string dst = ResolvePath(lua.L_CheckString(2));

            if (!PermissionManager.CanAccess(src, UserManager.currentUser))
            {
                lua.PushBoolean(false); lua.PushString("permission denied: source"); return 2;
            }

            string dstParent = Path.GetDirectoryName(dst);
            if (!PermissionManager.CanAccess(dstParent ?? dst, UserManager.currentUser))
            {
                lua.PushBoolean(false); lua.PushString("permission denied: destination"); return 2;
            }

            try { File.Copy(src, dst); lua.PushBoolean(true); return 1; }
            catch (Exception e) { lua.PushBoolean(false); lua.PushString(e.Message); return 2; }
        }
        private static int L_CanAccess(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            lua.PushBoolean(PermissionManager.CanAccess(path, UserManager.currentUser)); return 1;
        }
        private static int L_GetOwner(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            lua.PushString(PermissionManager.GetOwner(path)); return 1;
        }
        private static int L_CPUInfo(ILuaState lua)
        {
            string cpu = Cosmos.Kernel.Core.CPU.CpuId.GetBrandString();
            lua.PushString(cpu); return 1;
        }
        private static int L_RAMInfo(ILuaState lua)
        {
            string ram = "idk";
            try
            {
                ulong total = Shinx.LimineMemory.GetTotalPhysicalRamMB();
                ulong used = (ulong)(GC.GetTotalMemory(forceFullCollection: false) / 1024 / 1024);
                ram = $"{used} MB used / {total} MB total";
            }
            catch { ram = "idk"; }
            lua.PushString(ram); return 1;
        }
        private static int L_Sleep(ILuaState lua) { int ms = lua.L_CheckInteger(1); System.Threading.Thread.Sleep(ms); return 0; }
        private static int L_SetCursor(ILuaState lua) { int x = lua.L_CheckInteger(1); int y = lua.L_CheckInteger(2); x = Math.Clamp(x, 0, ScreenManager.Width - 1); y = Math.Clamp(y, 0, ScreenManager.Height - 1); var vc = VirtualConsole.Current; if (vc != null) vc.SetCursorPosition(x, y); else Console.SetCursorPosition(x, y); return 0; }
        private static int L_HasKey(ILuaState lua) { var vc = VirtualConsole.Current; lua.PushBoolean(vc != null ? vc.KeyAvailable : Console.KeyAvailable); return 1; }
        private static int L_GetKey(ILuaState lua)
        {
            var vc = VirtualConsole.Current;
            if (vc != null ? vc.KeyAvailable : Console.KeyAvailable)
            {
                ConsoleKeyInfo key = vc != null ? vc.ReadKey(true) : Console.ReadKey(true);
                lua.PushString(key.KeyChar.ToString());
            }
            else lua.PushNil();
            return 1;
        }
        private static int L_HideCursor(ILuaState lua) { try { Console.CursorVisible = false; } catch { } return 0; }
        private static int L_ShowCursor(ILuaState lua) { try { Console.CursorVisible = true; } catch { } return 0; }
        private static int L_NetIsConnected(ILuaState lua) { lua.PushBoolean(NetworkManager.IsConnected); return 1; }
        private static int L_NetStatus(ILuaState lua)
        {
            lua.NewTable();
            lua.PushString("connected"); lua.PushBoolean(NetworkManager.IsConnected); lua.SetTable(-3);
            lua.PushString("ip"); lua.PushString(NetworkManager.CurrentIP ?? ""); lua.SetTable(-3);
            lua.PushString("mask"); lua.PushString(NetworkManager.CurrentMask ?? ""); lua.SetTable(-3);
            lua.PushString("gateway"); lua.PushString(NetworkManager.CurrentGateway ?? ""); lua.SetTable(-3);
            lua.PushString("dns"); lua.PushString(NetworkManager.DNSServer ?? ""); lua.SetTable(-3);
            lua.PushString("mode"); lua.PushString(NetworkManager.Mode ?? "none"); lua.SetTable(-3);
            return 1;
        }
        private static int L_NetResolve(ILuaState lua)
        {
            if (!NetworkManager.IsConnected) { lua.PushNil(); lua.PushString("not connected"); return 2; }
            string hostname = lua.L_CheckString(1);
            string ip = NetworkManager.Resolve(hostname);
            if (ip != null) { lua.PushString(ip); return 1; }
            lua.PushNil(); lua.PushString("resolution failed"); return 2;
        }

        private static int L_NetGet(ILuaState lua)
        {
            if (!NetworkManager.IsConnected) { lua.PushNil(); lua.PushString("not connected"); return 2; }

            string url = lua.L_CheckString(1);
            if (!url.StartsWith("http://")) { lua.PushNil(); lua.PushString("only http:// is supported"); return 2; }

            try
            {
                string hostAndPath = url.Substring(7);
                int slash = hostAndPath.IndexOf('/');
                string host = slash >= 0 ? hostAndPath.Substring(0, slash) : hostAndPath;
                string path = slash >= 0 ? hostAndPath.Substring(slash) : "/";

                string ip = NetworkManager.Resolve(host);
                if (ip == null) { lua.PushNil(); lua.PushString("dns resolution failed"); return 2; }

                using (var client = new TcpClient())
                {
                    client.Connect(ip, 80);
                    NetworkStream stream = client.GetStream();

                    string req =
                        "GET " + path + " HTTP/1.0\r\n" +
                        "Host: " + host + "\r\n" +
                        "Connection: close\r\n\r\n";

                    byte[] reqBytes = Encoding.ASCII.GetBytes(req);
                    stream.Write(reqBytes, 0, reqBytes.Length);

                    var resp = new List<byte>();
                    byte[] buf = new byte[1024];
                    int read;
                    while ((read = stream.Read(buf, 0, buf.Length)) > 0)
                        for (int i = 0; i < read; i++) resp.Add(buf[i]);

                    stream.Close();

                    string full = Encoding.ASCII.GetString(resp.ToArray());
                    int headerEnd = full.IndexOf("\r\n\r\n");
                    if (headerEnd < 0) headerEnd = full.IndexOf("\n\n");
                    string body = headerEnd >= 0 ? full.Substring(headerEnd + 4) : full;

                    lua.PushString(body);
                    return 1;
                }
            }
            catch (Exception e)
            {
                lua.PushNil(); lua.PushString(e.Message); return 2;
            }
        }
        private static int L_HttpServe(ILuaState lua)
        {
            if (!UserManager.IsRoot(UserManager.currentUser)) { lua.PushBoolean(false); lua.PushString("permission denied"); return 2; }
            if (!NetworkManager.IsConnected) { lua.PushBoolean(false); lua.PushString("not connected"); return 2; }

            string root = lua.GetTop() >= 1 ? ResolvePath(lua.L_CheckString(1)) : Shell.currentDirectory;
            int port = lua.GetTop() >= 2 ? lua.L_CheckInteger(2) : 8080;

            Kernel.commandHandler.Execute("http " + root + " " + port);
            lua.PushBoolean(true); return 1;
        }
        public static void SetArgs(string[] args)
        {
            var L = LuaExecutor.State;
            L.NewTable();
            for (int i = 0; i < args.Length; i++)
            {
                L.PushString(args[i]);
                L.RawSetI(-2, i + 1);
            }
            L.SetGlobal("__args");
        }
        public static void SetParams(HashSet<char> parameters)
        {
            var L = LuaExecutor.State;
            L.NewTable();
            foreach (char p in parameters)
            {
                L.PushBoolean(true);
                L.SetField(-2, p.ToString());
            }
            L.SetGlobal("__params");
        }
        public static void ScanBin()
        {
            string binPath = "/bin/";
            if (!Directory.Exists(binPath)) return;

            foreach (var file in Directory.GetFiles(binPath))
            {
                if (!file.EndsWith(".lua")) continue;

                var L = LuaExecutor.State;
                int stackBefore = L.GetTop();
                try
                {
                    string code = File.ReadAllText(file);
                    var loadStatus = L.L_LoadString(code);
                    if (loadStatus != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine($"bin: parse error in {file}: " + L.L_ToString(-1));
                        L.SetTop(stackBefore);
                        continue;
                    }
                    var runStatus = L.PCall(0, -1, 0);
                    if (runStatus != ThreadStatus.LUA_OK)
                        Console.WriteLine($"bin: runtime error in {file}: " + L.L_ToString(-1));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"bin: exception in {file}: {e.GetType().Name}: {e.Message}");
                }
                finally
                {
                    L.SetTop(stackBefore);
                }
            }
        }
        private static bool TryParseColor(string name, out ConsoleColor color)
        {
            switch (name.ToLower())
            {
                case "black": color = ConsoleColor.Black; return true;
                case "darkblue": color = ConsoleColor.DarkBlue; return true;
                case "darkgreen": color = ConsoleColor.DarkGreen; return true;
                case "darkcyan": color = ConsoleColor.DarkCyan; return true;
                case "darkred": color = ConsoleColor.DarkRed; return true;
                case "darkmagenta": color = ConsoleColor.DarkMagenta; return true;
                case "darkyellow": color = ConsoleColor.DarkYellow; return true;
                case "gray": color = ConsoleColor.Gray; return true;
                case "darkgray": color = ConsoleColor.DarkGray; return true;
                case "blue": color = ConsoleColor.Blue; return true;
                case "green": color = ConsoleColor.Green; return true;
                case "cyan": color = ConsoleColor.Cyan; return true;
                case "red": color = ConsoleColor.Red; return true;
                case "magenta": color = ConsoleColor.Magenta; return true;
                case "yellow": color = ConsoleColor.Yellow; return true;
                case "white": color = ConsoleColor.White; return true;
                default: color = ConsoleColor.White; return false;
            }
        }
        private static bool TryParseGuiColor(string s, out System.Drawing.Color c)
        {
            c = System.Drawing.Color.White;
            if (string.IsNullOrEmpty(s)) return false;

            string[] parts = s.Split(',');
            if (parts.Length == 3)
            {
                byte r, g, b;
                if (byte.TryParse(parts[0].Trim(), out r) &&
                    byte.TryParse(parts[1].Trim(), out g) &&
                    byte.TryParse(parts[2].Trim(), out b))
                {
                    c = System.Drawing.Color.FromArgb(255, r, g, b);
                    return true;
                }
            }

            switch (s.ToLower())
            {
                case "white": c = System.Drawing.Color.White; return true;
                case "black": c = System.Drawing.Color.Black; return true;
                case "red": c = System.Drawing.Color.Red; return true;
                case "green": c = System.Drawing.Color.Green; return true;
                case "blue": c = System.Drawing.Color.Blue; return true;
                case "yellow": c = System.Drawing.Color.Yellow; return true;
                case "cyan": c = System.Drawing.Color.Cyan; return true;
                case "magenta": c = System.Drawing.Color.Magenta; return true;
                case "gray": c = System.Drawing.Color.Gray; return true;
                case "darkgray": c = System.Drawing.Color.DarkGray; return true;
                case "lightgray": c = System.Drawing.Color.LightGray; return true;
                case "orange": c = System.Drawing.Color.Orange; return true;
                case "darkblue": c = System.Drawing.Color.DarkBlue; return true;
                case "darkgreen": c = System.Drawing.Color.DarkGreen; return true;
                case "darkred": c = System.Drawing.Color.DarkRed; return true;
                default: return false;
            }
        }
        private static int L_GuiRegister(ILuaState lua)
        {
            string id = lua.L_CheckString(1);
            string displayName = lua.L_CheckString(2);
            string drawFn = lua.L_CheckString(3);
            string keyFn = lua.GetTop() >= 4 && lua.Type(4) == LuaType.LUA_TSTRING ? lua.L_CheckString(4) : "";
            int x = lua.GetTop() >= 5 ? lua.L_CheckInteger(5) : 80;
            int y = lua.GetTop() >= 6 ? lua.L_CheckInteger(6) : 60;
            int w = lua.GetTop() >= 7 ? lua.L_CheckInteger(7) : 400;
            int h = lua.GetTop() >= 8 ? lua.L_CheckInteger(8) : 300;

            var existing = AppManager.GetApp(id);
            if (existing != null)
            {
                AppManager.Apps.Remove(existing);
                WindowManager.drawOrder.Remove(id);
            }

            var app = new LuaGuiApp(id, displayName, drawFn, keyFn, x, y, w, h);
            AppManager.Apps.Add(app);

            if (!WindowManager.drawOrder.Contains(id)) WindowManager.drawOrder.Add(id);

            lua.PushBoolean(true); return 1;
        }
        private static int L_GuiClose(ILuaState lua) { string id = lua.L_CheckString(1); var app = AppManager.GetApp(id); if (app != null) app.IsVisible = false; return 0; }
        private static int L_GuiRectFill(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) return 0;
            int rx = _guiX + 1 + lua.L_CheckInteger(1);
            int ry = _guiY + 20 + lua.L_CheckInteger(2);
            int rw = lua.L_CheckInteger(3);
            int rh = lua.L_CheckInteger(4);
            string cs = lua.GetTop() >= 5 ? lua.L_CheckString(5) : "white";
            System.Drawing.Color c; if (!TryParseGuiColor(cs, out c)) c = System.Drawing.Color.White;
            if (rx < 0) { rw += rx; rx = 0; }
            if (ry < 0) { rh += ry; ry = 0; }
            if (rx >= ScreenManager.Width || ry >= ScreenManager.Height) return 0;
            if (rw <= 0 || rh <= 0) return 0;
            _guiCanvas.DrawFilledRectangle(c, rx, ry, rw, rh);
            return 0;
        }
        private static int L_GuiRect(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) return 0;
            int rx = _guiX + 1 + lua.L_CheckInteger(1);
            int ry = _guiY + 20 + lua.L_CheckInteger(2);
            int rw = lua.L_CheckInteger(3);
            int rh = lua.L_CheckInteger(4);
            string cs = lua.GetTop() >= 5 ? lua.L_CheckString(5) : "white";
            System.Drawing.Color c; if (!TryParseGuiColor(cs, out c)) c = System.Drawing.Color.White;
            if (rx < 0) { rw += rx; rx = 0; }
            if (ry < 0) { rh += ry; ry = 0; }
            if (rx >= ScreenManager.Width || ry >= ScreenManager.Height) return 0;
            if (rw <= 0 || rh <= 0) return 0;
            _guiCanvas.DrawRectangle(c, rx, ry, rw, rh);
            return 0;
        }
        private static int L_GuiLine(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) return 0;
            int x1 = _guiX + 1 + lua.L_CheckInteger(1);
            int y1 = _guiY + 20 + lua.L_CheckInteger(2);
            int x2 = _guiX + 1 + lua.L_CheckInteger(3);
            int y2 = _guiY + 20 + lua.L_CheckInteger(4);
            string cs = lua.GetTop() >= 5 ? lua.L_CheckString(5) : "white";
            System.Drawing.Color c; if (!TryParseGuiColor(cs, out c)) c = System.Drawing.Color.White;
            if ((x1 < 0 && x2 < 0) || (y1 < 0 && y2 < 0)) return 0;
            if ((x1 >= ScreenManager.Width && x2 >= ScreenManager.Width) ||
                (y1 >= ScreenManager.Height && y2 >= ScreenManager.Height)) return 0;
            _guiCanvas.DrawLine(c, x1, y1, x2, y2);
            return 0;
        }
        private static int L_GuiMouse(ILuaState lua)
        {
            if (!_inGuiDraw) { lua.PushInteger(0); lua.PushInteger(0); return 2; }
            int mx = (int)MouseManager.X - _guiX - 1;
            int my = (int)MouseManager.Y - _guiY - 20;
            lua.PushInteger(mx); lua.PushInteger(my); return 2;
        }
        private static int L_GuiClick(ILuaState lua) { lua.PushBoolean(_inGuiDraw && Mouse.Click()); return 1; }
        private static int L_GuiWidth(ILuaState lua) { lua.PushInteger(_inGuiDraw ? _guiW - 2 : 0); return 1; }
        private static int L_GuiHeight(ILuaState lua) { lua.PushInteger(_inGuiDraw ? _guiH - 22 : 0); return 1; }
        private static int L_GuiButton(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) { lua.PushBoolean(false); return 1; }

            int bx = _guiX + 1 + lua.L_CheckInteger(1);
            int by = _guiY + 20 + lua.L_CheckInteger(2);
            int bw = lua.L_CheckInteger(3);
            int bh = lua.L_CheckInteger(4);
            string label = lua.L_CheckString(5);
            string bgCs = lua.GetTop() >= 6 ? lua.L_CheckString(6) : "gray";
            string borderCs = lua.GetTop() >= 7 ? lua.L_CheckString(7) : "white";
            string textCs = lua.GetTop() >= 8 ? lua.L_CheckString(8) : "white";

            int mx = (int)MouseManager.X;
            int my = (int)MouseManager.Y;
            bool hover = mx >= bx && mx <= bx + bw && my >= by && my <= by + bh;
            bool clicked = hover && Mouse.ConsumeClick();

            System.Drawing.Color bg, border, textColor;
            if (!TryParseGuiColor(bgCs, out bg)) bg = System.Drawing.Color.Gray;
            if (!TryParseGuiColor(borderCs, out border)) border = System.Drawing.Color.White;
            if (!TryParseGuiColor(textCs, out textColor)) textColor = System.Drawing.Color.White;

            System.Drawing.Color face = hover
                ? System.Drawing.Color.FromArgb(255, Math.Min(255, bg.R + 40), Math.Min(255, bg.G + 40), Math.Min(255, bg.B + 40))
                : bg;

            _guiCanvas.DrawFilledRectangle(face, bx, by, bw, bh);
            _guiCanvas.DrawRectangle(border, bx, by, bw, bh);

            int textX = bx + (bw - label.Length * 8) / 2;
            int textY = by + (bh - 16) / 2;
            GUI.ASC16.DrawACSIIString(_guiCanvas, label, textColor, (uint)textX, (uint)textY);

            lua.PushBoolean(clicked); return 1;
        }
        private static int L_GuiLabel(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) return 0;
            int tx = _guiX + 1 + lua.L_CheckInteger(1);
            int ty = _guiY + 20 + lua.L_CheckInteger(2);
            string text = lua.L_CheckString(3);
            string cs = lua.GetTop() >= 4 ? lua.L_CheckString(4) : "white";
            System.Drawing.Color c; if (!TryParseGuiColor(cs, out c)) c = System.Drawing.Color.White;
            GUI.ASC16.DrawACSIIString(_guiCanvas, text, c, (uint)tx, (uint)ty);
            return 0;
        }
        private static int L_GuiTextbox(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) return 0;

            int bx = _guiX + 1 + lua.L_CheckInteger(1);
            int by = _guiY + 20 + lua.L_CheckInteger(2);
            int bw = lua.L_CheckInteger(3);
            int bh = lua.L_CheckInteger(4);
            string text = lua.L_CheckString(5);
            string tcs = lua.GetTop() >= 6 ? lua.L_CheckString(6) : "white";
            string bcs = lua.GetTop() >= 7 ? lua.L_CheckString(7) : "black";
            string bdcs = lua.GetTop() >= 8 ? lua.L_CheckString(8) : "gray";

            System.Drawing.Color tc, bc, bdc;
            if (!TryParseGuiColor(tcs, out tc)) tc = System.Drawing.Color.White;
            if (!TryParseGuiColor(bcs, out bc)) bc = System.Drawing.Color.Black;
            if (!TryParseGuiColor(bdcs, out bdc)) bdc = System.Drawing.Color.Gray;

            _guiCanvas.DrawFilledRectangle(bc, bx, by, bw, bh);
            _guiCanvas.DrawRectangle(bdc, bx, by, bw, bh);

            int maxCols = (bw - 4) / 8;
            int maxRows = (bh - 4) / 16;

            var lines = new List<string>();
            foreach (string rawLine in text.Split('\n'))
            {
                string s = rawLine.TrimEnd('\r');
                if (s.Length == 0) { lines.Add(""); continue; }
                while (s.Length > maxCols) { lines.Add(s.Substring(0, maxCols)); s = s.Substring(maxCols); }
                lines.Add(s);
            }

            int start = Math.Max(0, lines.Count - maxRows);
            for (int i = start; i < lines.Count; i++)
                GUI.ASC16.DrawACSIIString(_guiCanvas, lines[i], tc, (uint)(bx + 2), (uint)(by + 2 + (i - start) * 16));

            return 0;
        }
        private static int L_GuiInput(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) { lua.PushString(""); return 1; }

            string id = lua.L_CheckString(1);
            int ix = _guiX + 1 + lua.L_CheckInteger(2);
            int iy = _guiY + 20 + lua.L_CheckInteger(3);
            int iw = lua.L_CheckInteger(4);
            string placeholder = lua.GetTop() >= 5 ? lua.L_CheckString(5) : "";
            string bgCs = lua.GetTop() >= 6 ? lua.L_CheckString(6) : "black";
            string borderCs = lua.GetTop() >= 7 ? lua.L_CheckString(7) : "gray";
            string textCs = lua.GetTop() >= 8 ? lua.L_CheckString(8) : "white";

            if (!_inputState.ContainsKey(id)) _inputState[id] = "";

            if (_inputFocus != id)
            {
                int mx = (int)MouseManager.X;
                int my = (int)MouseManager.Y;
                if (mx >= ix && mx <= ix + iw && my >= iy && my <= iy + 20 && Mouse.ConsumeClick())
                    _inputFocus = id;
            }

            bool focused = _inputFocus == id;
            string val = _inputState[id];

            System.Drawing.Color bg, border, textColor;
            if (!TryParseGuiColor(bgCs, out bg)) bg = System.Drawing.Color.Black;
            if (!TryParseGuiColor(borderCs, out border)) border = System.Drawing.Color.Gray;
            if (!TryParseGuiColor(textCs, out textColor)) textColor = System.Drawing.Color.White;

            System.Drawing.Color activeBorder = focused ? System.Drawing.Color.Cyan : border;

            _guiCanvas.DrawFilledRectangle(bg, ix, iy, iw, 20);
            _guiCanvas.DrawRectangle(activeBorder, ix, iy, iw, 20);

            string display = val.Length > 0 ? val : placeholder;
            System.Drawing.Color displayColor = val.Length > 0 ? textColor : System.Drawing.Color.DarkGray;

            int maxChars = (iw - 4) / 8;
            if (display.Length > maxChars) display = display.Substring(display.Length - maxChars);

            GUI.ASC16.DrawACSIIString(_guiCanvas, display, displayColor, (uint)(ix + 2), (uint)(iy + 2));

            if (focused)
            {
                int curX = ix + 2 + Math.Min(val.Length, maxChars) * 8;
                _guiCanvas.DrawFilledRectangle(System.Drawing.Color.White, curX, iy + 14, 6, 2);
            }

            lua.PushString(val);
            return 1;
        }
        private static int L_GuiCheckbox(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) { lua.PushBoolean(false); return 1; }

            string id = lua.L_CheckString(1);
            int cx = _guiX + 1 + lua.L_CheckInteger(2);
            int cy = _guiY + 20 + lua.L_CheckInteger(3);
            string label = lua.L_CheckString(4);

            if (!_checkboxState.ContainsKey(id)) _checkboxState[id] = false;

            int mx = (int)MouseManager.X;
            int my = (int)MouseManager.Y;
            bool hover = mx >= cx && mx <= cx + 16 && my >= cy && my <= cy + 16;
            bool clicked = hover && Mouse.ConsumeClick();

            if (clicked) _checkboxState[id] = !_checkboxState[id];

            bool state = _checkboxState[id];

            _guiCanvas.DrawFilledRectangle(System.Drawing.Color.FromArgb(255, 30, 30, 30), cx, cy, 16, 16);
            _guiCanvas.DrawRectangle(System.Drawing.Color.White, cx, cy, 16, 16);

            if (state)
            {
                _guiCanvas.DrawLine(System.Drawing.Color.LightGreen, cx + 2, cy + 8, cx + 6, cy + 13);
                _guiCanvas.DrawLine(System.Drawing.Color.LightGreen, cx + 6, cy + 13, cx + 14, cy + 3);
            }

            GUI.ASC16.DrawACSIIString(_guiCanvas, label, System.Drawing.Color.White, (uint)(cx + 22), (uint)(cy));

            lua.PushBoolean(state);
            return 1;
        }
        private static int L_GuiProgressbar(ILuaState lua)
        {
            if (!_inGuiDraw || _guiCanvas == null) return 0;

            int px = _guiX + 1 + lua.L_CheckInteger(1);
            int py = _guiY + 20 + lua.L_CheckInteger(2);
            int pw = lua.L_CheckInteger(3);
            int ph = lua.L_CheckInteger(4);
            double val = lua.L_CheckNumber(5);
            double max = lua.L_CheckNumber(6);
            string cs = lua.GetTop() >= 7 ? lua.L_CheckString(7) : "green";

            System.Drawing.Color fc; if (!TryParseGuiColor(cs, out fc)) fc = System.Drawing.Color.Green;

            _guiCanvas.DrawFilledRectangle(System.Drawing.Color.FromArgb(255, 30, 30, 30), px, py, pw, ph);
            _guiCanvas.DrawRectangle(System.Drawing.Color.Gray, px, py, pw, ph);

            if (max > 0 && val > 0)
            {
                int filled = (int)((val / max) * (pw - 2));
                if (filled > 0) _guiCanvas.DrawFilledRectangle(fc, px + 1, py + 1, filled, ph - 2);
            }

            int pct = max > 0 ? (int)(val / max * 100) : 0;
            string pctStr = pct + "%";
            int textX = px + (pw - pctStr.Length * 8) / 2;
            int textY = py + (ph - 16) / 2;
            if (ph >= 16)
                GUI.ASC16.DrawACSIIString(_guiCanvas, pctStr, System.Drawing.Color.White, (uint)textX, (uint)textY);

            return 0;
        }
        private static int L_GuiClearState(ILuaState lua)
        {
            string id = lua.L_CheckString(1);
            _inputState.Remove(id);
            _checkboxState.Remove(id);
            if (_inputFocus == id) _inputFocus = "";
            return 0;
        }
    }
}