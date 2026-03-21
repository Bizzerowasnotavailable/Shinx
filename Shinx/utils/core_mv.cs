using System;
using System.IO;

namespace Shinx.Commands
{
    public class core_mv : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: mv <source> <destination>");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("mv: " + args[0] + ": no such file");
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(args[0]);
                File.WriteAllBytes(args[1], data);
                File.Delete(args[0]);
                Console.WriteLine("moved " + args[0] + " to " + args[1]);
            }
            catch
            {
                Console.WriteLine("mv: failed to move " + args[0]);
            }
        }
    }
}