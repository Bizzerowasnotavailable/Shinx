using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_kill : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: kill <pid>");
                return;
            }

            if (!int.TryParse(args[0], out int pid))
            {
                Console.WriteLine("kill: invalid pid");
                return;
            }

            if (ProcessManager.Stop(pid))
                Console.WriteLine("stopped process " + pid);
            else
                Console.WriteLine("kill: no such process " + pid);
        }
    }
}
