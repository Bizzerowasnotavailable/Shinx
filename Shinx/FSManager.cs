using System;
using System.IO;
using System.Reflection;

namespace Shinx
{
    public static class FSManager
    {
        public static void Init()
        {
            string[] defaultDirs = { @"0:\sys", @"0:\home", @"0:\etc", @"0:\bin" };

            foreach (string dir in defaultDirs)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            if (!File.Exists(@"0:\bin\lpkg.txt"))
                File.Create(@"0:\bin\lpkg.txt");
        }
        public static void DeployLuaFiles()
        {
            foreach (var file in LuaResources.AllFiles)
            {
                string path = $@"0:\bin\{file.Name}";

                if (!File.Exists(path))
                {
                    File.WriteAllBytes(path, file.Data);
                }
            }
        }
    }
}