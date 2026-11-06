using System;
using System.Collections.Generic;
using System.IO;


namespace Shinx
{
    public static class Shell
    {
        public static string currentDirectory = "/";
        public static string previousDirectory = "/";
        public static List<string> history = new List<string>();

        private const int MaxHistory = 200;

        public static string GetHomeDirectory()
        {
            string user = UserManager.currentUser;
            if (string.IsNullOrEmpty(user) || user == "root") return "/";
            return "/home/" + user;
        }

        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) path = "/";
            path = path.Replace('\\', '/');

            if (path == "~" || path.StartsWith("~/"))
                path = GetHomeDirectory() + path.Substring(1);

            string combined;
            if (path.StartsWith("/"))
                combined = path;
            else
                combined = currentDirectory.TrimEnd('/') + "/" + path;

            string[] parts = combined.Split('/');
            List<string> stack = new List<string>();

            foreach (string part in parts)
            {
                if (part.Length == 0 || part == ".") continue;

                if (part == "..")
                {
                    if (stack.Count > 0) stack.RemoveAt(stack.Count - 1);
                    continue;
                }

                stack.Add(part);
            }

            return "/" + string.Join("/", stack.ToArray());
        }

        public static string ResolvePath(string path)
        {
            return NormalizePath(path);
        }

        public static string Prompt()
        {
            string user = UserManager.currentUser;
            if (string.IsNullOrEmpty(user)) user = "root";

            string dir = currentDirectory;
            if (string.IsNullOrEmpty(dir)) dir = "/";

            string home = GetHomeDirectory();
            if (home != "/" && (dir == home || dir.StartsWith(home + "/")))
                dir = "~" + dir.Substring(home.Length);

            if (dir.Length > 1 && dir.EndsWith("/"))
                dir = dir.TrimEnd('/');

            if (dir.Length == 0) dir = "/";

            return user + "@" + dir + "> ";
        }

        public static void AddHistory(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            if (history.Count > 0 && history[history.Count - 1] == line) return;

            history.Add(line);
            if (history.Count > MaxHistory) history.RemoveAt(0);

            SaveHistory();
        }

        public static void LoadHistory()
        {
            history.Clear();

            try
            {
                string path = HistoryPath();
                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line)) history.Add(line);
                    }
                }
            }
            catch
            {
            }

            while (history.Count > MaxHistory) history.RemoveAt(0);
        }

        public static void SaveHistory()
        {
            try
            {
                List<string> lines = new List<string>();
                int start = history.Count > MaxHistory ? history.Count - MaxHistory : 0;
                for (int i = start; i < history.Count; i++)
                    lines.Add(history[i]);

                File.WriteAllLines(HistoryPath(), lines.ToArray());
            }
            catch
            {
            }
        }

        public static void ClearHistory()
        {
            history.Clear();
            SaveHistory();
        }

        private static string HistoryPath()
        {
            string home = GetHomeDirectory();
            if (home == "/") return "/.shinx_history";
            return home + "/.shinx_history";
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
            setDirectory(NormalizePath(".."));
        }

        public static void setDirectory(string path)
        {
            string normalized = NormalizePath(path);
            if (normalized == currentDirectory) return;

            previousDirectory = currentDirectory;
            currentDirectory = normalized;
        }
    }
}
