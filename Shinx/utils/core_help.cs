using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx.Commands
{
    public class core_help : ICommand
    {
        private const string HelpPath = "/tmp/help.txt";

        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length == 0)
            {
                ShowAll();
                return;
            }

            string query = args[0].ToLower();

            if (peppe.commands.ContainsKey(query))
            {
                Console.WriteLine(query + ": " + Describe(query));
                return;
            }

            List<string> matches = new List<string>();
            foreach (var name in peppe.commands.Keys)
            {
                if (name.IndexOf(query) >= 0)
                    matches.Add(name);
            }

            if (matches.Count == 0)
            {
                Console.WriteLine("help: no command matches '" + args[0] + "'");
                return;
            }

            Print(Format(matches));
        }

        private static void ShowAll()
        {
            List<string> lines = Format(new List<string>(peppe.commands.Keys));

            try
            {
                if (!Directory.Exists("/tmp")) Directory.CreateDirectory("/tmp");
                File.WriteAllLines(HelpPath, lines.ToArray());
            }
            catch (Exception e)
            {
                // No writable filesystem: fall back to normal console output.
                Console.WriteLine("help: " + e.Message);
                Print(lines);
                return;
            }

            new core_edit().Execute(new[] { HelpPath }, new HashSet<char>());
        }

        private static void Print(List<string> lines)
        {
            foreach (var line in lines)
                Console.WriteLine(line);
        }

        private static List<string> Format(List<string> names)
        {
            names.Sort();

            int width = 0;
            foreach (var name in names)
                if (name.Length > width) width = name.Length;

            List<string> lines = new List<string>();
            lines.Add(names.Count + " command" + (names.Count == 1 ? "" : "s") + ":");

            foreach (var name in names)
                lines.Add("  " + name.PadRight(width + 2) + Describe(name));

            return lines;
        }

        private static string Describe(string name)
        {
            string desc;
            if (peppe.descriptions.TryGetValue(name, out desc) && !string.IsNullOrEmpty(desc))
                return desc;
            return "no description";
        }
    }
}
