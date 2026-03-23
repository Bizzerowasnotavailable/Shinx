using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;

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

            string path = args.Length > 0 ? (args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0]) : Shell.currentDirectory;

            try
            {
                if (!VFSManager.DirectoryExists(path))
                {
                    Console.WriteLine("ls: cannot access " + path);
                    return;
                }

                var entries = VFSManager.GetDirectoryListing(path);

                if (entries == null || entries.Count == 0)
                {
                    if (parameters.Contains('l'))
                    {
                        Console.WriteLine("owner: " + PermissionManager.GetOwner(path) + " - empty");
                    }
                    else
                        Console.WriteLine("ls: " + path + " is empty");
                    return;
                }

                foreach (var entry in entries)
                {
                    string fullEntryPath = path.TrimEnd('\\') + '\\' + entry.mName;
                    if (parameters.Contains('l'))
                    {
                        string owner = PermissionManager.GetOwner(fullEntryPath);
                        string entryGroups = PermissionManager.GetPermissionGroups(fullEntryPath);
                        if (entry.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory)
                            Console.WriteLine(owner + " " + entryGroups + " [DIR] " + entry.mName);
                        else
                            Console.WriteLine(owner + " " + entryGroups + "       " + entry.mName);
                    }
                    else
                    {
                        if (entry.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory)
                            Console.WriteLine("[DIR] " + entry.mName);
                        else
                            Console.WriteLine("      " + entry.mName);
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