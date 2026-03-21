using System;
using System.IO;
using System.Windows.Input;

namespace Shinx.Commands
{
    public class core_rm : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: rm [OPTIONS] <target>\nOPTIONS: -r: remove directories and their contents recursively");
                return;
            }

            if (args.Length > 1 && args[0] != "-r")
            {
                Console.WriteLine($"Unknown option: {args[0]}");
                return;
            }

            try
            {
                if (args.Length > 1)
                {
                    string path = args[1].StartsWith(@"0:\") ? args[1] : Shell.currentDirectory + args[1];
                    if (!Directory.Exists(path))
                    {
                        Console.WriteLine("rm: unknown path");
                        return;
                    }

                    Directory.Delete(path);
                }
                else
                {
                    string path = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];
                    if (!File.Exists(path))
                    {
                        Console.WriteLine("rm: unknown path");
                        return;
                    }

                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"rm: {e.Message}");
            }
        }
    }
}
