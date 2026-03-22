using Cosmos.System.FileSystem.VFS;
using Shinx.Commands;
using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx.utils
{
    public class core_mkdir : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: mkdir [-p] <directory>");
                return;
            }

            foreach (char p in parameters)
            {
                if (p != 'p')
                {
                    Console.WriteLine($"mkdir: unknown option: -{p}");
                    return;
                }
            }

            // i got you dont worry
            bool createParents = parameters.Contains('p');
            string dirArg = args[0].Replace(' ', '_');
            string fullPath = dirArg.StartsWith(@"0:\") ? dirArg : Shell.currentDirectory + dirArg;
            fullPath = fullPath.Replace('/', '\\');

            try
            {
                if (Directory.Exists(fullPath))
                {
                    Console.WriteLine("mkdir: directory already exists: " + dirArg);
                    return;
                }

                if (createParents)
                {
                    CreateRecursive(fullPath);
                }
                else
                {
                    string parent = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                    {
                        Console.WriteLine("mkdir: parent directory does not exist: " + parent);
                        Console.WriteLine("hint: add -p to create parent directories");
                        return;
                    }
                    Directory.CreateDirectory(fullPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("mkdir: " + e.Message);
            }
        }

        private void CreateRecursive(string fullPath)
        {
            var parts = new List<string>();
            string current = fullPath;
            while (!string.IsNullOrEmpty(current))
            {
                parts.Insert(0, current);
                string parent = Path.GetDirectoryName(current);
                if (parent == current || string.IsNullOrEmpty(parent))
                    break;
                current = parent;
            }
            foreach (var segment in parts)
            {
                if (!Directory.Exists(segment))
                    Directory.CreateDirectory(segment);
            }
        }
    }
}