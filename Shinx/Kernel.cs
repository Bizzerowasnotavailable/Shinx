// I FUCKING FIGURED OUT HOW TO RUN THIS SHI WITHOUT VMWARE YIPEEE
// SHOUTOUT TO HIRPUS LAB

using Shinx.Commands;
using System;
using System.IO;
using System.Reflection;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.FileSystem;
using Cosmos.Kernel.System.FileSystem.Fat;
using Cosmos.Executable.Lua;
using Sys = Cosmos.Kernel.System;


namespace Shinx
{
    public class Kernel : Sys.Kernel
    {
        public static peppe commandHandler;

        protected override void BeforeRun()
        {
            try
            {
                var fat = new FatFileSystemType();
                VfsManager.RegisterFileSystem("fat", fat);
                if (VfsManager.TryMount("fat", "0", MountFlags.None, "/", out VfsMount mount))
                {
                    Console.WriteLine("Mounted " + mount.Name + " partition " + mount.Source + " at " + mount.MountPoint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FS mount error: " + e.Message);
            }

            FSManager.Init();
            FSManager.DeployLuaFiles();

            UserManager.Init();
            PermissionManager.Init();
            NetworkManager.Init();
            commandHandler = new peppe();
            Shinx.GUI.AppManager.Initialize();

            Console.WriteLine("[OK] Boot successful");

            while (true)
            {
                bool loggedIn = false;
                while (!loggedIn)
                {
                    Console.Write("login: ");
                    string username = Console.ReadLine();
                    Console.Write("password: ");
                    string password = ReadPassword();

                    if (UserManager.Login(username, password))
                    {
                        Console.WriteLine($"welcome {username}");
                        loggedIn = true;
                    }
                    else
                    {
                        Console.WriteLine("login incorrect");
                    }
                }

                LuaBridge.Init();
                LuaBridge.ScanBin();

                string initPath = "/etc/init.lua";
                if (File.Exists(initPath))
                {
                    var status = LuaBridge.State.L_DoString(File.ReadAllText(initPath));
                    if (status != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine("init.lua error: " + LuaBridge.State.L_ToString(-1));
                        LuaBridge.State.Pop(1);
                    }
                }

                commandHandler.Execute("fetch");
                Console.WriteLine("type help for a list of commands");

                while (UserManager.currentUser != "")
                {
                    Run();
                }

                Console.WriteLine("logged out");
            }
        }

        protected override void Run()
        {
            Console.Write(Shell.Prompt());
            Shell.Execute(ReadLine());
        }

        private string ReadLine()
        {
            LineEditor editor = new LineEditor(Shell.history);

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                LineAction action = editor.Handle(key);

                switch (action)
                {
                    case LineAction.Submit:
                        Console.WriteLine();
                        return editor.Text;

                    case LineAction.Cancel:
                        Console.WriteLine("^C");
                        BeginInputLine(editor);
                        break;

                    case LineAction.ClearScreen:
                        Console.Clear();
                        BeginInputLine(editor);
                        break;

                    default:
                        RedrawInput(editor);
                        break;
                }
            }
        }

        private void BeginInputLine(LineEditor editor)
        {
            Console.Write(Shell.Prompt());
            RedrawInput(editor);
        }

        private void RedrawInput(LineEditor editor)
        {
            int promptLen = Shell.Prompt().Length;
            Console.CursorLeft = promptLen;
            Console.Write(new string(' ', Console.WindowWidth - promptLen - 1));
            Console.CursorLeft = promptLen;
            Console.Write(editor.Text);
            Console.CursorLeft = promptLen + editor.Cursor;
        }

        private string ReadPassword()
        {
            string password = "";
            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                        password = password.Remove(password.Length - 1, 1);
                }
                else if (key.KeyChar >= 32)
                {
                    password += key.KeyChar;
                }
            }
            return password;
        }
    }
}