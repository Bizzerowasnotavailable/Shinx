using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx.Commands
{
    public class core_cd : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
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
                string fullPath = args[0].StartsWith("/") ? args[0] : Shell.currentDirectory.TrimEnd('/') + "/" + args[0];

                if (!Directory.Exists(fullPath))
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