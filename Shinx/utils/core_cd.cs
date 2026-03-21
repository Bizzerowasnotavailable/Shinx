using Cosmos.System.FileSystem.VFS;
using System;

namespace Shinx.Commands
{
    public class core_cd : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: cd <destination> or cd .. to go back");
                return;
            }

            if (args[0] == "..")
            {
                Shell.returnDirectory();
                return;
            }

            try
            {
                string fullPath = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];

                if (!VFSManager.DirectoryExists(fullPath))
                {
                    Console.WriteLine("cd: " + args[0] + ": no such directory");
                    return;
                }

                Shell.setDirectory(fullPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("cd: " + e.Message);
            }
        }
    }
}