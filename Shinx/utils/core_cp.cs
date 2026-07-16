using System;
using System.Collections.Generic;
using System.IO;
namespace Shinx.Commands
{
    public class core_cp : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: cp [OPTIONS] <source> <destination>\nOPTIONS: -r: copy directories recursively\n-f: force overwrite if destination exists");
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
                        Console.WriteLine($"cp: unknown option: -{p}");
                        return;
                    }
                }
            }

            try
            {
                if (recursive)
                {
                    string src = args[0].StartsWith("/") ? args[0] : Shell.currentDirectory.TrimEnd('/') + "/" + args[0];
                    string dst = args[1].StartsWith("/") ? args[1] : Shell.currentDirectory.TrimEnd('/') + "/" + args[1];
                    if (!PermissionManager.CanAccess(src, UserManager.currentUser))
                    {
                        Console.WriteLine("cp: permission denied: " + args[0]);
                        return;
                    }
                    if (!PermissionManager.CanAccess(Shell.currentDirectory, UserManager.currentUser))
                    {
                        Console.WriteLine("cp: permission denied: " + args[1]);
                        return;
                    }
                    if (!Directory.Exists(src))
                    {
                        Console.WriteLine("cp: " + args[0] + ": no such directory");
                        return;
                    }

                    if (Directory.Exists(dst))
                    {
                        if (!force)
                        {
                            Console.WriteLine("cp: cannot copy '" + args[0] + "' to '" + args[1] + "': Destination exists (use -f to force)");
                            return;
                        }
                        Directory.Delete(dst, true);
                    }

                    CopyDirectory(src, dst);
                    Console.WriteLine("copied " + args[0] + " to " + args[1]);
                }
                else
                {
                    string src = args[0].StartsWith("/") ? args[0] : Shell.currentDirectory.TrimEnd('/') + "/" + args[0];
                    string dst = args[1].StartsWith("/") ? args[1] : Shell.currentDirectory.TrimEnd('/') + "/" + args[1];

                    if (!PermissionManager.CanAccess(src, UserManager.currentUser))
                    {
                        Console.WriteLine("cp: permission denied: " + args[0]);
                        return;
                    }
                    if (!PermissionManager.CanAccess(Shell.currentDirectory, UserManager.currentUser))
                    {
                        Console.WriteLine("cp: permission denied: " + args[1]);
                        return;
                    }
                    if (!File.Exists(src))
                    {
                        Console.WriteLine("cp: " + args[0] + ": no such file");
                        return;
                    }

                    if (File.Exists(dst))
                    {
                        if (!force)
                        {
                            Console.WriteLine("cp: cannot copy '" + args[0] + "' to '" + args[1] + "': Destination exists (use -f to force)");
                            return;
                        }
                        File.Delete(dst);
                    }

                    File.Copy(src, dst, true);
                    Console.WriteLine("copied " + args[0] + " to " + args[1]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"cp: {e.Message}");
            }
        }

        private void CopyDirectory(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var file in Directory.GetFiles(src))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(dst, fileName);
                File.Copy(file, destFile, true);
                PermissionManager.SetDefault(destFile, UserManager.currentUser);
                Console.WriteLine("copied " + file + " to " + destFile);
            }
            foreach (var dir in Directory.GetDirectories(src))
            {
                string dirName = Path.GetFileName(dir);
                string destDir = Path.Combine(dst, dirName);
                CopyDirectory(dir, destDir);
            }
        }
    }
}