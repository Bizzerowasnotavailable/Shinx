using Shinx.utils;
using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class peppe // le funny name
    {
        private Dictionary<string, ICommand> commands; // implements the commandname commanddothings structure

        public peppe()
        {
            commands = new Dictionary<string, ICommand>
            {
                { "echo", new core_echo() },
                { "lico", new core_lico() },
                { "bunnysay", new core_bunnysay() },
                { "fetch", new uncore_fetch()  },
                { "cp", new core_cp() },
                { "clear", new core_clear() },
                { "ls", new core_ls() },
                { "mv", new core_mv() },
                { "mkdir", new core_mkdir() }
            };
        }

        public void Execute(string input)
        {
            string[] parts = input.Split(' ');
            string commandName = parts[0].ToLower();
            string[] args = new string[parts.Length - 1];
            Array.Copy(parts, 1, args, 0, parts.Length - 1);

            if (commands.ContainsKey(commandName))
            {
                commands[commandName].Execute(args);
            }
            else
            {
                Console.WriteLine($"command not found, what exactly is {commandName} ?? ");
            }
        }
    }
}
