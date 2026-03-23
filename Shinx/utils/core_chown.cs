using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx.Commands
{
    public class core_chown : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: chown [OPTIONS] <owner> <path>\nOPTIONS: -r: apply recursively");
                return;
            }

            if (!UserManager.IsRoot(UserManager.currentUser))
            {
                Console.WriteLine("chown: permission denied");
                return;
            }

            if (!UserManager.UserExists(args[0]))
            {
                Console.WriteLine($"chown: user not found: {args[0]}");
                return;
            }

            foreach (char p in parameters)
            {
                if (p != 'r')
                {
                    Console.WriteLine($"chown: unknown option: -{p}");
                    return;
                }
            }

            string path = args[1].StartsWith(@"0:\") ? args[1] : Shell.currentDirectory + args[1];

            try
            {
                if (parameters.Contains('r'))
                {
                    ApplyOwnerRecursive(path, args[0]);
                    Console.WriteLine($"chown: changed owner of {args[1]} and contents to {args[0]}");
                }
                else
                {
                    PermissionManager.SetDefault(path, args[0]);
                    Console.WriteLine($"chown: changed owner of {args[1]} to {args[0]}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"chown: {e.Message}");
            }
        }

        private void ApplyOwnerRecursive(string path, string owner)
        {
            PermissionManager.SetDefault(path, owner);
            foreach (var file in Directory.GetFiles(path))
            {
                string fullPath = path.TrimEnd('\\') + '\\' + file;
                PermissionManager.SetDefault(fullPath, owner);
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                string fullPath = path.TrimEnd('\\') + '\\' + dir;
                ApplyOwnerRecursive(fullPath, owner);
            }
        }
    }
}