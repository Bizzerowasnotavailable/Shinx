using System;
using System.Collections.Generic;


namespace Shinx
{
    public static class Shell
    {
        public static string currentDirectory = @"0:\";
        public static List<string> history = new List<string>();

        public static void returnDirectory()
        {
            if (currentDirectory == @"0:\")
                return;

            string copy = currentDirectory.TrimEnd('\\');
            int last = copy.LastIndexOf('\\');
            currentDirectory = copy.Substring(0, last + 1);
        }

        public static void setDirectory(string path)
        {
            if (!path.EndsWith(@"\"))
                path += @"\";

            currentDirectory = path;
        }
    }
}
