// I FUCKING FIGURED OUT HOW TO RUN THIS SHI WITHOUT VMWARE YIPEEE
// SHOUTOUT TO HIRPUS LAB

using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using IL2CPU.API.Attribs;
using Shinx.Commands;
using System;
using System.IO;
using Sys = Cosmos.System;

namespace Shinx
{
    public class Kernel : Sys.Kernel
    {
        private peppe commandHandler;
        private CosmosVFS vfs;

        protected override void BeforeRun()
        {
            vfs = new CosmosVFS();
            VFSManager.RegisterVFS(vfs);

            if (!Directory.Exists(@"0:\sys"))
                Directory.CreateDirectory(@"0:\sys");
            if (!Directory.Exists(@"0:\home"))
                Directory.CreateDirectory(@"0:\home");

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
                        Console.WriteLine("welcome " + username);
                        loggedIn = true;
                    }
                    else
                    {
                        Console.WriteLine("login incorrect");
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
            var input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                commandHandler.Execute(input);
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