// I FUCKING FIGURED OUT HOW TO RUN THIS SHI WITHOUT VMWARE YIPEEE
// SHOUTOUT TO HIRPUS LAB

using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using IL2CPU.API.Attribs;
using Shinx.Commands;
using System;
using System.IO;
using UniLua;
using Sys = Cosmos.System;



namespace Shinx
{
    public class Kernel : Sys.Kernel
    {
        [ManifestResourceStream(ResourceName = "Shinx.LuaUtils.fetch.lua")]
        static byte[] fetchLua;

        public static peppe commandHandler;
        private CosmosVFS vfs;

        protected override void BeforeRun()
        {
            vfs = new CosmosVFS();
            VFSManager.RegisterVFS(vfs);

            if (!Directory.Exists(@"0:\sys"))
                Directory.CreateDirectory(@"0:\sys");
            if (!Directory.Exists(@"0:\home"))
                Directory.CreateDirectory(@"0:\home");
            if (!Directory.Exists(@"0:\etc"))
                Directory.CreateDirectory(@"0:\etc");
            if (!Directory.Exists(@"0:\bin"))
                Directory.CreateDirectory(@"0:\bin");
            if (!File.Exists(@"0:\bin\fetch.lua"))
            {
                string content = System.Text.Encoding.UTF8.GetString(fetchLua);
                File.WriteAllText(@"0:\bin\fetch.lua", content);
            }

            UserManager.Init();
            PermissionManager.Init();
            commandHandler = new peppe();
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

                if (File.Exists(@"0:\etc\init.lua"))
                {
                    var status = LuaBridge.State.L_DoString(File.ReadAllText(@"0:\etc\init.lua"));
                    if (status != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine("init.lua error: " + LuaBridge.State.L_ToString(-1));
                        LuaBridge.State.Pop(1);
                    }
                }

                commandHandler.Execute("fetch");
                Console.WriteLine("type lico for a list of commands");

                while (UserManager.currentUser != "")
                {
                    Run();
                }

                Console.WriteLine("logged out");
            }
        }

        protected override void Run()
        {
            Console.Write(UserManager.currentUser + "@" + Shell.currentDirectory + "> ");
            string input = ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                Shell.history.Add(input);
                commandHandler.Execute(input);
            }
        }

        private int historyIndex = -1;

        private string ReadLine()
        {
            string input = "";
            int cursorPos = 0;
            historyIndex = Shell.history.Count;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return input;

                    case ConsoleKey.Backspace:
                        if (cursorPos > 0)
                        {
                            input = input.Substring(0, cursorPos - 1) + input.Substring(cursorPos);
                            cursorPos--;
                            RedrawInput(input, cursorPos);
                        }
                        break;

                    case ConsoleKey.UpArrow:
                        if (historyIndex > 0)
                        {
                            historyIndex--;
                            input = Shell.history[historyIndex];
                            cursorPos = input.Length;
                            RedrawInput(input, cursorPos);
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        if (historyIndex < Shell.history.Count - 1)
                        {
                            historyIndex++;
                            input = Shell.history[historyIndex];
                            cursorPos = input.Length;
                            RedrawInput(input, cursorPos);
                        }
                        else
                        {
                            historyIndex = Shell.history.Count;
                            input = "";
                            cursorPos = 0;
                            RedrawInput(input, cursorPos);
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (cursorPos > 0)
                        {
                            cursorPos--;
                            Console.CursorLeft = (UserManager.currentUser + "@" + Shell.currentDirectory + "> ").Length + cursorPos;
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (cursorPos < input.Length)
                        {
                            cursorPos++;
                            Console.CursorLeft = (UserManager.currentUser + "@" + Shell.currentDirectory + "> ").Length + cursorPos;
                        }
                        break;

                    case ConsoleKey.C when key.Modifiers == ConsoleModifiers.Control:
                        input = "";
                        cursorPos = 0;
                        historyIndex = Shell.history.Count;
                        Console.WriteLine("^C");
                        Console.Write(UserManager.currentUser + "@" + Shell.currentDirectory + "> ");
                        break;

                    default:
                        if (key.KeyChar != '\0')
                        {
                            input = input.Substring(0, cursorPos) + key.KeyChar + input.Substring(cursorPos);
                            cursorPos++;
                            RedrawInput(input, cursorPos);
                        }
                        break;
                }
            }
        }

        private void RedrawInput(string input, int cursorPos)
        {
            int promptLen = (UserManager.currentUser + "@" + Shell.currentDirectory + "> ").Length;
            Console.CursorLeft = promptLen;
            Console.Write(new string(' ', Console.WindowWidth - promptLen - 1));
            Console.CursorLeft = promptLen;
            Console.Write(input);
            Console.CursorLeft = promptLen + cursorPos;
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