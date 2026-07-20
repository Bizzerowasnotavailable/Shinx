using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_clear : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            VirtualConsole.Current?.Clear();
        }
    }
}