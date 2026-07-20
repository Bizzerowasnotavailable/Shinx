using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_ps : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            var processes = ProcessManager.List();

            if (processes.Count == 0)
            {
                Console.WriteLine("no background processes");
                return;
            }

            Console.WriteLine("PID  NAME");
            foreach (var p in processes)
            {
                Console.WriteLine(p.Pid.ToString().PadRight(5) + p.Name);
            }
        }
    }
}
