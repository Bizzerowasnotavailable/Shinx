using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_clear : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            Console.Clear(); // shrimple as that
        }
    }
}