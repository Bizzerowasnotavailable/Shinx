using System;
using System.IO;
namespace Shinx.Commands
{
    public class core_cp : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: cp [OPTIONS] <source> <destination>\nOPTIONS: -r: copy directories recursively");
                return;
            }
            if (args.Length > 2 && args[0] != "-r")
            {
                Console.WriteLine($"Unknown option: {args[0]}");
                return;
            }
            try
            {
                if (args.Length > 2)
                {
                    args[2] = args[2].Replace(' ', '_');
                    string src = args[1].StartsWith(@"0:\") ? args[1] : Shell.currentDirectory + args[1];
                    string dst = args[2].StartsWith(@"0:\") ? args[2] : Shell.currentDirectory + args[2];
                    if (!Directory.Exists(src))
                    {
                        Console.WriteLine("cp: " + args[1] + ": no such directory");
                        return;
                    }
                    CopyDirectory(src, dst);
                    Console.WriteLine("copied " + args[1] + " to " + args[2]);
                }
                else
                {
                    string src = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];
                    string dst = args[1].StartsWith(@"0:\") ? args[1] : Shell.currentDirectory + args[1];
                    if (!File.Exists(src))
                    {
                        Console.WriteLine("cp: " + args[0] + ": no such file");
                        return;
                    }
                    byte[] data = File.ReadAllBytes(src);
                    File.WriteAllBytes(dst, data);
                    Console.WriteLine("copied " + args[0] + " to " + args[1]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"cp: {e.Message}");
            }
        }
        private void CopyDirectory(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var file in Directory.GetFiles(src))
            {
                string fullFilePath = src.TrimEnd('\\') + '\\' + file;
                string destFile = dst.TrimEnd('\\') + '\\' + file;
                File.WriteAllBytes(destFile, File.ReadAllBytes(fullFilePath));
                Console.WriteLine("copied " + fullFilePath + " to " + destFile);
            }
            foreach (var dir in Directory.GetDirectories(src))
            {
                string fullDirPath = src.TrimEnd('\\') + '\\' + dir;
                string destDir = dst.TrimEnd('\\') + '\\' + dir;
                CopyDirectory(fullDirPath, destDir);
            }
        }
    }
}