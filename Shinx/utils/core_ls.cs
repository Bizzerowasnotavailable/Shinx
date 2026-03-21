using Cosmos.System.FileSystem.VFS;
using System;

namespace Shinx.Commands
{
    public class core_ls : ICommand
    {
        public void Execute(string[] args)
        {
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
                    Console.WriteLine("ls: " + path + " is empty");
                    return;
                }

                foreach (var entry in entries)
                {
                    if (entry.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory)
                        Console.WriteLine("[DIR] " + entry.mName);
                    else
                        Console.WriteLine("      " + entry.mName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ls: " + e.Message);
            }
        }
    }
}