using System;

namespace Shinx.Commands
{
    public class core_echo : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine();
                return;
            }

            Console.WriteLine(string.Join(" ", args));
        }
    }
}
