
// I FUCKING FIGURED OUT HOW TO RUN THIS SHI WITHOUT VMWARE YIPEEE
// SHOUTOUT TO HIRPUS LAB

using Cosmos.System.Graphics;
using IL2CPU.API.Attribs;
using Shinx.Commands;
using System;
using Sys = Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

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
            UserManager.Init();
            commandHandler = new peppe();
            Console.WriteLine("SHINX booted successfully, keep in mind, this is PRE-ALPHA software :3");

            while (true)
            {
                bool loggedIn = false;
                while (!loggedIn)
                {
                    Console.Write("login: ");
                    string username = Console.ReadLine();
                    Console.Write("password: ");
                    string password = Console.ReadLine();

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
    }
}