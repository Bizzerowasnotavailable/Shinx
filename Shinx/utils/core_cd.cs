using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx.Commands
{
    public class core_cd : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            string target;
            bool printTarget = false;

            if (args.Length < 1)
            {
                target = Shell.GetHomeDirectory();
            }
            else if (args[0] == "-")
            {
                target = Shell.previousDirectory;
                printTarget = true;
            }
            else
            {
                target = Shell.ResolvePath(args[0]);
            }

            try
            {
                if (!Directory.Exists(target))
                {
                    Console.WriteLine("cd: " + (args.Length > 0 ? args[0] : target) + ": no such directory");
                    return;
                }

                if (!PermissionManager.CanAccess(target, UserManager.currentUser))
                {
                    Console.WriteLine("cd: permission denied: " + (args.Length > 0 ? args[0] : target));
                    return;
                }

                Shell.setDirectory(target);
                if (printTarget) Console.WriteLine(target);
            }
            catch (Exception e)
            {
                Console.WriteLine("cd: " + e.Message);
            }
        }

        public void Usage()
        {
            Console.WriteLine("usage: cd [directory|-]");
        }
    }
}
