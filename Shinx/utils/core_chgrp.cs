using System;
using System.Collections.Generic;
using System.IO;
namespace Shinx.Commands
{
    public class core_chgrp : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: chgrp [OPTIONS] <group1,group2,...> <path>\nOPTIONS: -r: apply recursively");
                return;
            }

            if (!UserManager.IsRoot(UserManager.currentUser))
            {
                Console.WriteLine("chgrp: permission denied");
                return;
            }

            foreach (char p in parameters)
            {
                if (p != 'r')
                {
                    Console.WriteLine($"chgrp: unknown option: -{p}");
                    return;
                }
            }

            string path = args[1].StartsWith(@"0:\") ? args[1] : Shell.currentDirectory + args[1];

            HashSet<string> groups = new HashSet<string>();
            foreach (string g in args[0].Split(','))
            {
                if (!UserManager.GroupExists(g))
                {
                    Console.WriteLine($"chgrp: group not found: {g}");
                    return;
                }
                groups.Add(g);
            }

            try
            {
                if (parameters.Contains('r'))
                {
                    ApplyGroupRecursive(path, groups);
                    Console.WriteLine($"chgrp: changed groups of {args[1]} and contents to {args[0]}");
                }
                else
                {
                    PermissionManager.SetPermission(path, groups);
                    Console.WriteLine($"chgrp: changed groups of {args[1]} to {args[0]}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"chgrp: {e.Message}");
            }
        }

        private void ApplyGroupRecursive(string path, HashSet<string> groups)
        {
            PermissionManager.SetPermission(path, groups);
            foreach (var file in Directory.GetFiles(path))
            {
                string fullPath = path.TrimEnd('\\') + '\\' + file;
                PermissionManager.SetPermission(fullPath, new HashSet<string>(groups));
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                string fullPath = path.TrimEnd('\\') + '\\' + dir;
                ApplyGroupRecursive(fullPath, new HashSet<string>(groups));
            }
        }
    }
}