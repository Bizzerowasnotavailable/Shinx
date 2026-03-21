// should still be somewhat broken?

using System;
using System.IO;

namespace Shinx.Commands
{
    public class core_ls : ICommand
    {
        public void Execute(string[] args)
        {
            string path = args.Length > 0 ? args[0] : @"0:\";

            try
            {
                var dirs = Directory.GetDirectories(path);
                var files = Directory.GetFiles(path);

                if (dirs.Length == 0 && files.Length == 0)
                {
                    Console.WriteLine("ls: " + path + " is empty");
                    return;
                }

                foreach (var dir in dirs)
                    Console.WriteLine("[DIR] " + dir);

                foreach (var file in files)
                    Console.WriteLine(file);
            }
            catch
            {
                Console.WriteLine("ls: cannot access " + path);
            }
        }
    }
}