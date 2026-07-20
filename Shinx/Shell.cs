using System;
using System.Collections.Generic;
using System.Threading;

namespace Shinx
{
    public static class Shell
    {
        public static string currentDirectory = "/";
        public static List<string> history = new List<string>();

        public static volatile bool CancelRequested;
        public static readonly object ConsoleLock = new object();
        public static Thread CommandThread;

        public static void Cancel()
        {
            CancelRequested = true;
            lock (ConsoleLock)
                Console.WriteLine("^C");
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