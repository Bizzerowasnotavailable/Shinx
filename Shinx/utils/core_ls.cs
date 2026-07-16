using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx.Commands
{
    public class core_ls : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {            
            foreach (char p in parameters)
            {
                if (p != 'l')
                {
                    Console.WriteLine($"ls: unknown option: -{p}");
                    return;
                }
            }

            string path = args.Length > 0 
    ? (args[0].StartsWith("/") ? args[0] : Shell.currentDirectory.TrimEnd('/') + "/" + args[0])
    : Shell.currentDirectory;

            try
            {
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("ls: cannot access " + path);
                    return;
                }

                var files = Directory.GetFiles(path);
                var dirs = Directory.GetDirectories(path);

                if ((files.Length == 0 && dirs.Length == 0))
                {
                    if (parameters.Contains('l'))
                    {
                        Console.WriteLine("owner: " + PermissionManager.GetOwner(path) + " - empty");
                    }
                    else
                        Console.WriteLine("ls: " + path + " is empty");
                    return;
                }

                foreach (var dir in dirs)
                {
                    string fullEntryPath = dir;
                    if (parameters.Contains('l'))
                    {
                        string owner = PermissionManager.GetOwner(fullEntryPath);
                        string entryGroups = PermissionManager.GetPermissionGroups(fullEntryPath);
                        Console.WriteLine(owner + " " + entryGroups + " [DIR] " + Path.GetFileName(dir));
                    }
                    else
                    {
                        Console.WriteLine("[DIR] " + Path.GetFileName(dir));
                    }
                }

                foreach (var file in files)
                {
                    string fullEntryPath = file;
                    if (parameters.Contains('l'))
                    {
                        string owner = PermissionManager.GetOwner(fullEntryPath);
                        string entryGroups = PermissionManager.GetPermissionGroups(fullEntryPath);
                        Console.WriteLine(owner + " " + entryGroups + "       " + Path.GetFileName(file));
                    }
                    else
                    {
                        Console.WriteLine("      " + Path.GetFileName(file));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ls: " + e.Message);
            }
        }
    }
}