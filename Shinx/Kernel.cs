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
            Shinx.Audio.AudioManager.Initialize();
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
                        Shell.LoadHistory();
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

                Shell.SaveHistory();
                Console.WriteLine("logged out");
            }
        }

        private int _inputRow;
        private int _inputCol;

        protected override void Run()
        {
            Shell.Execute(ReadLine());
        }

        private string ReadLine()
        {
            LineEditor editor = new LineEditor(Shell.history);
            BeginInputLine(editor);

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
                        try
                        {
                            ClearInputArea();
                            Console.SetCursorPosition(_inputCol, _inputRow);
                        }
                        catch { }
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
            _inputRow = Console.CursorTop;
            _inputCol = Console.CursorLeft;
            Console.Write(Shell.Prompt());
            RedrawInput(editor);
        }

        private void ClearInputArea()
        {
            int width = Console.WindowWidth;
            if (width < 1) width = 80;

            string blank = new string(' ', width - 1);
            int lastRow = Console.CursorTop;
            if (lastRow < _inputRow) lastRow = _inputRow;

            for (int row = lastRow; row >= _inputRow; row--)
            {
                Console.SetCursorPosition(0, row);
                Console.Write(blank);
            }
        }

        private void RedrawInput(LineEditor editor)
        {
            string prompt = Shell.Prompt();
            string text = editor.Text;
            int width = Console.WindowWidth;
            if (width < 1) width = 80;

            try
            {
                ClearInputArea();

                Console.SetCursorPosition(_inputCol, _inputRow);
                Console.Write(prompt);
                Console.Write(text);

                int column = _inputCol + prompt.Length + editor.Cursor;
                int row = _inputRow + (column / width);
                int col = column % width;
                Console.SetCursorPosition(col, row);
            }
            catch
            {
                try { Console.CursorLeft = 0; } catch { }
                Console.Write(prompt);
                Console.Write(text);
            }
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