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
                Console.WriteLine("usage: cp <source> <destination>");
                return;
            }

            string src = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];
            string dst = args[1].StartsWith(@"0:\") ? args[1] : Shell.currentDirectory + args[1];

            if (!File.Exists(src))
            {
                Console.WriteLine("cp: " + args[0] + ": no such file");
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(src);
                File.WriteAllBytes(dst, data);
                Console.WriteLine("copied " + args[0] + " to " + args[1]);
            }
            catch (Exception e)
            {
                Console.WriteLine($"cp: {e.Message}");
            }
        }
    }
}