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

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("cp: " + args[0] + ": no such file");
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(args[0]);
                File.WriteAllBytes(args[1], data);
                Console.WriteLine("copied " + args[0] + " to " + args[1]);
            }
            catch
            {
                Console.WriteLine("cp: failed to copy " + args[0]);
            }
        }
    }
}