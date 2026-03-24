using Shinx.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using UniLua;

namespace Shinx
{
    public static class LuaBridge
    {
        public static ILuaState State;

        private static readonly string[] blockedCommands = { "rm", "userdel", "groupdel", "chown", "chgrp", "useradd" };

        public static void Init()
        {
            State = LuaAPI.NewState();
            State.L_OpenLibs();

            // strip dangerous globals
            State.PushNil(); State.SetGlobal("load");
            State.PushNil(); State.SetGlobal("loadfile");
            State.PushNil(); State.SetGlobal("dofile");
            State.PushNil(); State.SetGlobal("require");
            State.PushNil(); State.SetGlobal("io");
            State.PushNil(); State.SetGlobal("os");
            State.PushNil(); State.SetGlobal("debug");

            // create shinx table
            State.NewTable();

            State.PushCSharpFunction(L_Print);
            State.SetField(-2, "print");

            State.PushCSharpFunction(L_Clear);
            State.SetField(-2, "clear");

            State.PushCSharpFunction(L_Color);
            State.SetField(-2, "color");

            State.PushCSharpFunction(L_ResetColor);
            State.SetField(-2, "resetcolor");

            State.PushCSharpFunction(L_Read);
            State.SetField(-2, "read");

            State.PushCSharpFunction(L_ReadLine);
            State.SetField(-2, "readline");

            State.PushCSharpFunction(L_Exec);
            State.SetField(-2, "exec");

            State.PushCSharpFunction(L_CurrentDir);
            State.SetField(-2, "currentdir");

            State.PushCSharpFunction(L_SetDir);
            State.SetField(-2, "setdir");

            State.PushCSharpFunction(L_CurrentUser);
            State.SetField(-2, "currentuser");

            State.PushCSharpFunction(L_IsRoot);
            State.SetField(-2, "isroot");

            State.PushCSharpFunction(L_Register);
            State.SetField(-2, "register");

            State.PushCSharpFunction(L_Args);
            State.SetField(-2, "args");

            State.PushCSharpFunction(L_Params);
            State.SetField(-2, "params");

            State.PushCSharpFunction(L_Time);
            State.SetField(-2, "time");

            State.PushCSharpFunction(L_ListDir);
            State.SetField(-2, "listdir");

            State.PushCSharpFunction(L_ReadFile);
            State.SetField(-2, "readfile");

            State.PushCSharpFunction(L_WriteFile);
            State.SetField(-2, "writefile");

            State.PushCSharpFunction(L_Exists);
            State.SetField(-2, "exists");

            State.PushCSharpFunction(L_MkDir);
            State.SetField(-2, "mkdir");

            State.PushCSharpFunction(L_Delete);
            State.SetField(-2, "delete");

            State.PushCSharpFunction(L_MoveFile);
            State.SetField(-2, "movefile");

            State.PushCSharpFunction(L_CopyFile);
            State.SetField(-2, "copyfile");

            State.PushCSharpFunction(L_CanAccess);
            State.SetField(-2, "canaccess");

            State.PushCSharpFunction(L_GetOwner);
            State.SetField(-2, "getowner");

            State.SetGlobal("shinx");
        }

        private static string ResolvePath(string path)
        {
            path = path.Replace('/', '\\');
            if (!path.StartsWith(@"0:\"))
                path = Shell.currentDirectory + path;
            return path;
        }
        private static int L_Print(ILuaState lua)
        {
            Console.WriteLine(lua.L_ToString(1));
            return 0;
        }

        private static int L_Clear(ILuaState lua)
        {
            Console.Clear();
            return 0;
        }
        private static int L_Color(ILuaState lua)
        {
            string fg = lua.L_CheckString(1);

            ConsoleColor fgColor;
            if (TryParseColor(fg, out fgColor))
                Console.ForegroundColor = fgColor;

            if (lua.GetTop() >= 2 && lua.Type(2) == LuaType.LUA_TSTRING)
            {
                string bg = lua.L_CheckString(2);
                ConsoleColor bgColor;
                if (TryParseColor(bg, out bgColor))
                    Console.BackgroundColor = bgColor;
            }

            return 0;
        }
        private static int L_ResetColor(ILuaState lua)
        {
            Console.ResetColor();
            return 0;
        }
        private static int L_Read(ILuaState lua)
        {
            lua.PushString(Console.ReadLine());
            return 1;
        }

        private static int L_ReadLine(ILuaState lua)
        {
            string prompt = lua.L_CheckString(1);
            Console.Write(prompt);
            lua.PushString(Console.ReadLine());
            return 1;
        }
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

        private static int L_CurrentDir(ILuaState lua)
        {
            lua.PushString(Shell.currentDirectory);
            return 1;
        }

        private static int L_SetDir(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                lua.PushBoolean(false);
                lua.PushString("permission denied");
                return 2;
            }

            if (!Cosmos.System.FileSystem.VFS.VFSManager.DirectoryExists(path))
            {
                lua.PushBoolean(false);
                lua.PushString("no such directory");
                return 2;
            }

            Shell.currentDirectory = path.TrimEnd('\\') + @"\";
            lua.PushBoolean(true);
            return 1;
        }

        private static int L_CurrentUser(ILuaState lua)
        {
            lua.PushString(UserManager.currentUser);
            return 1;
        }

        private static int L_IsRoot(ILuaState lua)
        {
            lua.PushBoolean(UserManager.IsRoot(UserManager.currentUser));
            return 1;
        }

        private static int L_Register(ILuaState lua)
        {
            string name = lua.L_CheckString(1);
            lua.PushValue(2);
            lua.SetGlobal("__cmd_" + name);
            peppe.RegisterCommand(name, new LuaCommand("__cmd_" + name));
            Console.WriteLine($"registered command: {name}");
            return 0;
        }

        private static int L_Args(ILuaState lua)
        {
            lua.GetGlobal("__args");
            return 1;
        }

        private static int L_Params(ILuaState lua)
        {
            lua.GetGlobal("__params");
            return 1;
        }

        private static int L_Time(ILuaState lua)
        {
            lua.PushString(DateTime.Now.ToString("HH:mm:ss"));
            return 1;
        }
        private static int L_ListDir(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                lua.PushNil();
                lua.PushString("permission denied");
                return 2;
            }

            try
            {
                var entries = Cosmos.System.FileSystem.VFS.VFSManager.GetDirectoryListing(path);

                lua.NewTable();
                int i = 1;

                foreach (var entry in entries)
                {
                    lua.NewTable();

                    lua.PushString("name");
                    lua.PushString(entry.mName);
                    lua.SetTable(-3);

                    lua.PushString("type");
                    lua.PushString(entry.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory ? "dir" : "file");
                    lua.SetTable(-3);

                    lua.RawSetI(-2, i++);
                }

                return 1;
            }
            catch (Exception e)
            {
                lua.PushNil();
                lua.PushString(e.Message);
                return 2;
            }
        }

        private static int L_ReadFile(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                lua.PushNil();
                lua.PushString("permission denied");
                return 2;
            }

            try
            {
                string content = File.ReadAllText(path);
                lua.PushString(content);
                return 1;
            }
            catch (Exception e)
            {
                lua.PushNil();
                lua.PushString(e.Message);
                return 2;
            }
        }

        private static int L_WriteFile(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            string data = lua.L_CheckString(2);

            string parent = Path.GetDirectoryName(path);
            if (!PermissionManager.CanAccess(parent ?? path, UserManager.currentUser))
            {
                lua.PushString("permission denied");
                return 1;
            }

            try
            {
                File.WriteAllText(path, data);
                return 0;
            }
            catch (Exception e)
            {
                lua.PushString(e.Message);
                return 1;
            }
        }

        private static int L_Exists(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            bool exists =
                File.Exists(path) ||
                Cosmos.System.FileSystem.VFS.VFSManager.DirectoryExists(path);

            lua.PushBoolean(exists);
            return 1;
        }

        private static int L_MkDir(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            string parent = Path.GetDirectoryName(path);

            if (!PermissionManager.CanAccess(parent ?? path, UserManager.currentUser))
            {
                lua.PushBoolean(false);
                lua.PushString("permission denied");
                return 2;
            }

            try
            {
                if (Cosmos.System.FileSystem.VFS.VFSManager.DirectoryExists(path))
                {
                    lua.PushBoolean(false);
                    lua.PushString("directory already exists");
                    return 2;
                }

                Directory.CreateDirectory(path);
                PermissionManager.SetDefault(path, UserManager.currentUser);
                lua.PushBoolean(true);
                return 1;
            }
            catch (Exception e)
            {
                lua.PushBoolean(false);
                lua.PushString(e.Message);
                return 2;
            }
        }

        private static int L_Delete(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                lua.PushBoolean(false);
                lua.PushString("permission denied");
                return 2;
            }

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                else if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else
                {
                    lua.PushBoolean(false);
                    lua.PushString("no such file or directory");
                    return 2;
                }

                lua.PushBoolean(true);
                return 1;
            }
            catch (Exception e)
            {
                lua.PushBoolean(false);
                lua.PushString(e.Message);
                return 2;
            }
        }

        private static int L_MoveFile(ILuaState lua)
        {
            string src = ResolvePath(lua.L_CheckString(1));
            string dst = ResolvePath(lua.L_CheckString(2));

            if (!PermissionManager.CanAccess(src, UserManager.currentUser))
            {
                lua.PushBoolean(false);
                lua.PushString("permission denied: source");
                return 2;
            }

            string dstParent = Path.GetDirectoryName(dst);
            if (!PermissionManager.CanAccess(dstParent ?? dst, UserManager.currentUser))
            {
                lua.PushBoolean(false);
                lua.PushString("permission denied: destination");
                return 2;
            }

            try
            {
                File.WriteAllBytes(dst, File.ReadAllBytes(src));
                File.Delete(src);
                lua.PushBoolean(true);
                return 1;
            }
            catch (Exception e)
            {
                lua.PushBoolean(false);
                lua.PushString(e.Message);
                return 2;
            }
        }

        private static int L_CopyFile(ILuaState lua)
        {
            string src = ResolvePath(lua.L_CheckString(1));
            string dst = ResolvePath(lua.L_CheckString(2));

            if (!PermissionManager.CanAccess(src, UserManager.currentUser))
            {
                lua.PushBoolean(false);
                lua.PushString("permission denied: source");
                return 2;
            }

            string dstParent = Path.GetDirectoryName(dst);
            if (!PermissionManager.CanAccess(dstParent ?? dst, UserManager.currentUser))
            {
                lua.PushBoolean(false);
                lua.PushString("permission denied: destination");
                return 2;
            }

            try
            {
                File.Copy(src, dst);
                lua.PushBoolean(true);
                return 1;
            }
            catch (Exception e)
            {
                lua.PushBoolean(false);
                lua.PushString(e.Message);
                return 2;
            }
        }
        private static int L_CanAccess(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            lua.PushBoolean(PermissionManager.CanAccess(path, UserManager.currentUser));
            return 1;
        }

        private static int L_GetOwner(ILuaState lua)
        {
            string path = ResolvePath(lua.L_CheckString(1));
            lua.PushString(PermissionManager.GetOwner(path));
            return 1;
        }
        public static void SetArgs(string[] args)
        {
            State.NewTable();
            for (int i = 0; i < args.Length; i++)
            {
                State.PushString(args[i]);
                State.RawSetI(-2, i + 1);
            }
            State.SetGlobal("__args");
        }

        public static void SetParams(HashSet<char> parameters)
        {
            State.NewTable();
            foreach (char p in parameters)
            {
                State.PushBoolean(true);
                State.SetField(-2, p.ToString());
            }
            State.SetGlobal("__params");
        }

        public static void ScanBin()
        {
            string binPath = @"0:\bin\";
            if (!Directory.Exists(binPath)) return;

            foreach (var file in Directory.GetFiles(binPath))
            {
                string fullPath = file.StartsWith(@"0:\") ? file : binPath + Path.GetFileName(file);
                Console.WriteLine("bin: scanning " + fullPath);
                if (!fullPath.EndsWith(".lua")) continue;

                int stackBefore = State.GetTop();
                try
                {
                    string code = File.ReadAllText(fullPath);
                    var loadStatus = State.L_LoadString(code);
                    if (loadStatus != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine($"bin: parse error in {fullPath}: " + State.L_ToString(-1));
                        State.SetTop(stackBefore);
                        continue;
                    }
                    var runStatus = State.PCall(0, -1, 0);
                    if (runStatus != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine($"bin: runtime error in {fullPath}: " + State.L_ToString(-1));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"bin: exception in {fullPath}: {e.GetType().Name}: {e.Message}");
                }
                finally
                {
                    State.SetTop(stackBefore);
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
    }
}