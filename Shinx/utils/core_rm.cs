using System;
using System.Collections.Generic;
using System.IO;
namespace Shinx.Commands
{
    public class core_rm : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: rm [OPTIONS] <target>\nOPTIONS: -r: remove directories recursively");
                return;
            }

            foreach (char p in parameters)
            {
                if (p != 'r')
                {
                    Console.WriteLine($"rm: unknown option: -{p}");
                    return;
                }
            }

            string path = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                Console.WriteLine("rm: permission denied: " + args[0]);
                return;
            }

            try
            {
                if (parameters.Contains('r'))
                {
                    if (!Directory.Exists(path))
                    {
                        Console.WriteLine("rm: " + args[0] + ": no such directory");
                        return;
                    }
                    Directory.Delete(path, true);
                    Console.WriteLine("removed " + args[0]);
                }
                else
                {
                    if (!File.Exists(path))
                    {
                        Console.WriteLine("rm: " + args[0] + ": no such file");
                        return;
                    }
                    File.Delete(path);
                    Console.WriteLine("removed " + args[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"rm: {e.Message}");
            }
        }
    }
}