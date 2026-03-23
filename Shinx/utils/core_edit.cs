using System;
using System.IO;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_edit : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: edit <file>");
                return;
            }

            string path = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];

            if (File.Exists(path) && !PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                Console.WriteLine("edit: permission denied: " + args[0]);
                return;
            }

            List<string> lines = new List<string>();

            if (File.Exists(path))
            {
                lines.AddRange(File.ReadAllLines(path));
                Console.WriteLine("loaded " + args[0] + " (" + lines.Count + " lines)");
            }
            else
            {
                Console.WriteLine("new file: " + args[0]);
            }

            Console.WriteLine(":w to save, :q to quit, :p to print, :d <line> to delete line, :h to show this message");

            try
            {
                while (true)
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();

                    if (input.StartsWith(":d "))
                    {
                        try
                        {
                            int index = int.Parse(input.Substring(3));
                            if (index >= 0 && index < lines.Count)
                            {
                                lines.RemoveAt(index);
                                Console.WriteLine("deleted line " + index);
                            }
                            else
                            {
                                Console.WriteLine("edit: line out of range");
                            }
                        }
                        catch
                        {
                            Console.WriteLine("edit: invalid line number");
                        }
                        continue;
                    }

                    switch (input)
                    {
                        case ":q":
                            return;
                        case ":w":
                            File.WriteAllLines(path, lines.ToArray());
                            PermissionManager.SetDefault(path, UserManager.currentUser);
                            Console.WriteLine("saved " + args[0]);
                            break;
                        case ":p":
                            for (int i = 0; i < lines.Count; i++)
                                Console.WriteLine(i + ": " + lines[i]);
                            break;
                        case ":h":
                            Console.WriteLine(":w to save, :q to quit, :p to print, :d <line> to delete line, :h to show this message");
                            break;
                        default:
                            lines.Add(input);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("edit: " + e.Message);
            }
        }
    }
}