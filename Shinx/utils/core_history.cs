using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_history : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            foreach (char p in parameters)
            {
                if (p != 'c')
                {
                    Console.WriteLine("history: unknown option: -" + p);
                    return;
                }
            }

            if (parameters.Contains('c'))
            {
                Shell.ClearHistory();
                Console.WriteLine("history cleared");
                return;
            }

            int count = Shell.history.Count;

            if (args.Length > 0)
            {
                int requested;
                if (!int.TryParse(args[0], out requested) || requested <= 0)
                {
                    Console.WriteLine("usage: history [-c] [count]");
                    return;
                }

                if (requested < count) count = requested;
            }

            int start = Shell.history.Count - count;
            for (int i = start; i < Shell.history.Count; i++)
                Console.WriteLine((i + 1).ToString().PadLeft(4) + "  " + Shell.history[i]);
        }
    }
}
