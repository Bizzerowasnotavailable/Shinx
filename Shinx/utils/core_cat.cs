using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx.Commands
{
    public class core_cat : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: cat <file>");
                return;
            }

            string path = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];

            if (!File.Exists(path))
            {
                Console.WriteLine("cat: " + args[0] + ": no such file");
                return;
            }

            try
            {
                string[] content = File.ReadAllLines(path);
                for(int i = 0; i < content.Length; i++)
                {
                    Console.WriteLine(content[i]);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"cat: {e.Message}");
            }
        }
    }
}