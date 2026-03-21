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

            string src = args[0];
            string dst = args[1];

            if (!src.StartsWith(@"0:\"))
                src = @"0:\" + src;
            if (!dst.StartsWith(@"0:\"))
                dst = @"0:\" + dst;

            if (!File.Exists(src))
            {
                Console.WriteLine("mv: " + args[0] + ": no such file");
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(src);
                File.WriteAllBytes(dst, data);
                File.Delete(src);
                Console.WriteLine("moved " + args[0] + " to " + args[1]);
            }
            catch (Exception e)
            {
                Console.WriteLine("mv: " + e.Message);
            }
        }
    }
}