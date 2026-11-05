using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_pwd : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            Console.WriteLine(Shell.currentDirectory);
        }
    }
}
