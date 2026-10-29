using System;
using System.Collections.Generic;


namespace Shinx
{
    public static class Shell
    {
        public static string currentDirectory = "/";
        public static List<string> history = new List<string>();

        private const int MaxHistory = 200;

        public static string Prompt()
        {
            string user = UserManager.currentUser;
            if (string.IsNullOrEmpty(user)) user = "root";

            string dir = currentDirectory;
            if (string.IsNullOrEmpty(dir)) dir = "/";

            return user + "@" + dir + "> ";
        }

        public static void AddHistory(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            if (history.Count > 0 && history[history.Count - 1] == line) return;

            history.Add(line);
            if (history.Count > MaxHistory) history.RemoveAt(0);
        }

        public static void Execute(string line)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0) return;

            AddHistory(trimmed);

            try
            {
                if (Kernel.commandHandler != null)
                    Kernel.commandHandler.Execute(line);
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
            }
        }

        public static void returnDirectory()
        {
            if (currentDirectory == "/")
                return;

            string copy = currentDirectory.TrimEnd('/');
            int last = copy.LastIndexOf('/');
            if (last <= 0)
                currentDirectory = "/";
            else
                currentDirectory = copy.Substring(0, last + 1);
        }

        public static void setDirectory(string path)
        {
            if (!path.EndsWith("/"))
                path += "/";

            currentDirectory = path;
        }
    }
}