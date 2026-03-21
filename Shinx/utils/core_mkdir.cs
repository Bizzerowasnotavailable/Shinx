using Cosmos.System.FileSystem.VFS;
using Shinx.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shinx.utils
{
    public class core_mkdir : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: mkdir <directory>");
                return;
            }

            string path = args[0];


            if (!path.StartsWith(@"0:\"))
                path = @"0:\" + path;

            try
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine("mkdir: directory already exists: " + args[0]);
                    return;
                }

                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("mkdir: " + e.Message);
            }
        }

    }
}
