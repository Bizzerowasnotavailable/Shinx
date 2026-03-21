using System;

namespace Shinx.Commands
{
    public class core_lico : ICommand
    {
        public void Execute(string[] args)
        {
            Console.WriteLine("cp: copy files from one place to another (BROKEN)");
            Console.WriteLine("echo: display a message in the terminal");
            Console.WriteLine("lico: list commands");
            Console.WriteLine("bunnysay: port of bunnysay utility");
        }
    }
}
