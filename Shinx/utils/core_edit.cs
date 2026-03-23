using System;
using System.IO;
using System.Collections.Generic;


namespace Shinx.Commands
{
    public class core_edit : ICommand
    {
        private List<string> lines = new List<string>();
        private int currentLine = 0;
        private string path = "";

        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: edit <file>");
                return;
            }

            path = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];

            if (File.Exists(path) && !PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                Console.WriteLine($"edit: permission denied: {args[0]}");
                return;
            }

            lines = new List<string>();
            currentLine = 0;

            if (File.Exists(path))
            {
                lines.AddRange(File.ReadAllLines(path));
                Console.WriteLine($"loaded {args[0]} ({lines.Count} lines)");
            }
            else
            {
                Console.WriteLine($"new file: {args[0]}");
            }

            PrintFile();
            Console.WriteLine("type :h for help");

            try
            {
                while (true)
                {
                    Console.Write($":{currentLine + 1}> ");
                    string input = ReadLine();

                    if (input.StartsWith(":d "))
                    {
                        try
                        {
                            int lineNum = int.Parse(input.Substring(3));
                            if (lineNum >= 1 && lineNum <= lines.Count)
                            {
                                lines.RemoveAt(lineNum - 1);
                                if (currentLine >= lines.Count)
                                    currentLine = lines.Count;
                                PrintFile();
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

                    if (input.StartsWith(":g "))
                    {
                        if (int.TryParse(input.Substring(3), out int lineNum))
                        {
                            if (lineNum >= 1 && lineNum <= lines.Count)
                            {
                                currentLine = lineNum - 1;
                                Console.WriteLine($"jumped to line {lineNum}: {lines[currentLine]}");
                            }
                            else
                            {
                                Console.WriteLine("edit: line out of range");
                            }
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
                            Console.WriteLine($"saved {args[0]}");
                            break;
                        case ":wq":
                            File.WriteAllLines(path, lines.ToArray());
                            PermissionManager.SetDefault(path, UserManager.currentUser);
                            Console.WriteLine($"saved {args[0]}");
                            return;
                        case ":p":
                            PrintFile();
                            break;
                        case ":n":
                            currentLine = lines.Count;
                            Console.WriteLine($"moved to new line {currentLine + 1}");
                            break;
                        case ":h":
                            Console.WriteLine(":w - save");
                            Console.WriteLine(":wq - save and quit");
                            Console.WriteLine(":q - quit");
                            Console.WriteLine(":p - print file");
                            Console.WriteLine(":g <line> - go to line");
                            Console.WriteLine(":d <line> - delete line");
                            Console.WriteLine(":n - go to new line at end");
                            break;
                        default:
                            if (currentLine >= lines.Count)
                                lines.Add(input);
                            else
                                lines[currentLine] = input;
                            currentLine++;
                            PrintFile();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"edit: {e.Message}");
            }
        }

        private void PrintFile()
        {
            Console.WriteLine($"--- {path} ---");
            if (lines.Count == 0)
            {
                Console.WriteLine("(empty)");
            }
            else
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    string marker = i == currentLine ? ">" : " ";
                    Console.WriteLine($"{marker}{i + 1}: {lines[i]}");
                }
            }
            Console.WriteLine("---");
        }

        private string ReadLine()
        {
            string input = "";
            int cursorPos = 0;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return input;

                    case ConsoleKey.Backspace:
                        if (cursorPos > 0)
                        {
                            input = input.Substring(0, cursorPos - 1) + input.Substring(cursorPos);
                            cursorPos--;
                            RedrawLine(input, cursorPos);
                        }
                        break;

                    case ConsoleKey.Delete:
                        if (cursorPos < input.Length)
                        {
                            input = input.Substring(0, cursorPos) + input.Substring(cursorPos + 1);
                            RedrawLine(input, cursorPos);
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (cursorPos > 0)
                        {
                            cursorPos--;
                            Console.CursorLeft--;
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (cursorPos < input.Length)
                        {
                            cursorPos++;
                            Console.CursorLeft++;
                        }
                        break;

                    default:
                        if (key.KeyChar != '\0')
                        {
                            input = input.Substring(0, cursorPos) + key.KeyChar + input.Substring(cursorPos);
                            cursorPos++;
                            RedrawLine(input, cursorPos);
                        }
                        break;
                }
            }
        }

        private void RedrawLine(string input, int cursorPos)
        {
            int promptLen = ($":{currentLine + 1}> ").Length;
            Console.CursorLeft = promptLen;
            Console.Write(new string(' ', Console.WindowWidth - promptLen - 1));
            Console.CursorLeft = promptLen;
            Console.Write(input);
            Console.CursorLeft = promptLen + cursorPos;
        }
    }
}