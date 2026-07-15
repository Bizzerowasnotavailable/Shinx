using System;
using System.Collections.Generic;
using System.IO;
namespace Shinx.Commands
{
    public class core_mv : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: mv [OPTIONS] <source> <destination>\nOPTIONS: -r: move directories recursively\n-f: force overwrite if destination exists");
                return;
            }

            bool force = parameters.Contains('f');
            bool recursive = parameters.Contains('r');

            if (parameters.Count > (force ? 1 : 0) + (recursive ? 1 : 0))
            {
                foreach (char p in parameters)
                {
                    if (p != 'r' && p != 'f')
                    {
                        Console.WriteLine($"mv: unknown option: -{p}");
                        return;
                    }
                }
            }

            try
            {
                string src = args[0].StartsWith("/") ? args[0] : Shell.currentDirectory.TrimEnd('/') + "/" + args[0];
                string dst = args[1].StartsWith("/") ? args[1] : Shell.currentDirectory.TrimEnd('/') + "/" + args[1];

                if (!PermissionManager.CanAccess(src, UserManager.currentUser))
                {
                    Console.WriteLine("mv: permission denied: " + args[0]);
                    return;
                }
                if (!PermissionManager.CanAccess(Shell.currentDirectory, UserManager.currentUser))
                {
                    Console.WriteLine("mv: permission denied: " + args[1]);
                    return;
                }

                if (recursive)
                {
                    if (!Directory.Exists(src))
                    {
                        Console.WriteLine("mv: " + args[0] + ": no such directory");
                        return;
                    }

                    if (Directory.Exists(dst) || File.Exists(dst))
                    {
                        if (!force)
                        {
                            Console.WriteLine("mv: cannot move '" + args[0] + "' to '" + args[1] + "': Destination exists (use -f to force)");
                            return;
                        }
                        if (Directory.Exists(dst))
                            Directory.Delete(dst, true);
                        else
                            File.Delete(dst);
                    }

                    Directory.Move(src, dst);
                    Console.WriteLine("moved " + args[0] + " to " + args[1]);
                }
                else
                {
                    if (!File.Exists(src))
                    {
                        Console.WriteLine("mv: " + args[0] + ": no such file");
                        return;
                    }

                    if (File.Exists(dst) || Directory.Exists(dst))
                    {
                        if (!force)
                        {
                            Console.WriteLine("mv: cannot move '" + args[0] + "' to '" + args[1] + "': Destination exists (use -f to force)");
                            return;
                        }

                        if (File.Exists(dst))
                            File.Delete(dst);
                        else
                            Directory.Delete(dst, true);
                    }

                    File.Move(src, dst, true);
                    Console.WriteLine("moved " + args[0] + " to " + args[1]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("mv: " + e.Message);
            }
        }
    }
}