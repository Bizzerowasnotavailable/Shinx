// I FUCKING FIGURED OUT HOW TO RUN THIS SHI WITHOUT VMWARE YIPEEE
// SHOUTOUT TO HIRPUS LAB

using Shinx.Commands;
using System;
using System.IO;
using System.Reflection;
using System.IO;
using System.Threading;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using Cosmos.Kernel.System.Filesystems.Fat;
using UniLua;
using Sys = Cosmos.Kernel.System;
using Cosmos.Kernel.HAL.Vfs;


namespace Shinx
{
    public class Kernel : Sys.Kernel
    {
        public static peppe commandHandler;

        protected override void BeforeRun()
        {
            FatFilesystemType fat = new();

            VfsManager.RegisterFilesystem("fat", fat);

            if (VfsManager.TryMount("fat", "0", MountFlags.None, "/", out VfsManager.VfsMount? mount))
            {
                Console.WriteLine("Mounted " + mount.Name + " partition " + mount.Source + " at " + mount.MountPoint);
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

                LuaExecutor.Init();
                LuaBridge.ScanBin();

                string initPath = "/etc/init.lua";
                if (File.Exists(initPath))
                {
                    var result = LuaExecutor.DoString(File.ReadAllText(initPath));
                    if (result.Status != ThreadStatus.LUA_OK)
                        Console.WriteLine("init.lua error: " + result.Error);
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
                Shell.CancelRequested = false;

                ICancellable cancellable = null;
                string trimmed = input.TrimStart();
                int spaceIdx = trimmed.IndexOf(' ');
                string cmdName = spaceIdx > 0 ? trimmed.Substring(0, spaceIdx) : trimmed;
                if (peppe.commands.ContainsKey(cmdName) && peppe.commands[cmdName] is ICancellable c)
                    cancellable = c;

                VirtualConsole vc = new VirtualConsole();
                VirtualConsole.Current = vc;
                vc.RedirectConsole();

                try
                {
                    Thread cmdThread = new Thread(() => commandHandler.Execute(input));
                    Shell.CommandThread = cmdThread;
                    cmdThread.Start();

                    while (cmdThread.IsAlive)
                    {
                        LuaExecutor.ProcessPending();

                        while (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);
                            if (!DesktopManager.Running && key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
                            {
                                if (cancellable != null)
                                    cancellable.Cancel();
                                else
                                    Shell.CancelRequested = true;
                                lock (Shell.ConsoleLock)
                                    Console.WriteLine("^C");
                                break;
                            }
                            vc.EnqueueKey(key);
                        }

                        if (vc.HasPendingOps)
                            vc.Flush();

                        Thread.Sleep(50);
                    }

                    if (vc.HasPendingOps)
                        vc.Flush();
                }
                finally
                {
                    vc.RestoreConsole();
                    VirtualConsole.Current = null;
                    Shell.CommandThread = null;
                }
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