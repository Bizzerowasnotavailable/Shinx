using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_clear : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            ConsoleUi.Clear(); // shrimple as that
        }
    }
}
