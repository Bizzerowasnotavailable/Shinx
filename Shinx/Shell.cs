using System;
using System.Collections.Generic;


namespace Shinx
{
    public static class Shell
    {
        public static string currentDirectory = "/";
        public static List<string> history = new List<string>();

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