using System;

namespace Shinx.Commands
{
    public class core_clear : ICommand
    {
        public void Execute(string[] args)
        {
            Console.Clear(); // shrimple as that
        }
    }
}