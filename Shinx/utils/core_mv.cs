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

            string src = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];
            string dst = args[1].StartsWith(@"0:\") ? args[1] : Shell.currentDirectory + args[1];

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