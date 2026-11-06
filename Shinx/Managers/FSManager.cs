using System;
using System.IO;
using System.Reflection;

namespace Shinx
{
    public static class FSManager
    {
        public static void Init()
        {
            string[] defaultDirs = { "/sys", "/home", "/etc", "/bin" };

            foreach (string dir in defaultDirs)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            string lpkgPath = "/bin/lpkg.txt";
            if (!File.Exists(lpkgPath))
                File.Create(lpkgPath).Close();
        }
        public static void DeployLuaFiles()
        {
            var files = new (string Name, byte[] Data)[]
            {
                ("fetch.lua", LuaResources.Fetch),
                ("bunnysay.lua", LuaResources.Bunnysay)
            };

            foreach (var file in files)
            {
                string path = $"/bin/{file.Name}";

                if (!File.Exists(path))
                {
                    File.WriteAllBytes(path, file.Data);
                }
            }
        }
    }
}