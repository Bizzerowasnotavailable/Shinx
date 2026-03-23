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
                { "mkdir", new core_mkdir() },
                { "cat", new core_cat() },
                { "cd", new core_cd() },
                { "edit", new core_edit() },
                { "rm", new core_rm() },
                { "sha256sum", new core_sha256() },
                { "useradd", new core_useradd() },
                { "userdel", new core_userdel() },
                { "passwd", new core_passwd() },
                { "whoami", new core_whoami() },
                { "exit", new core_exit() },
                { "su", new core_su() },
                { "groupadd", new core_groupadd() },
                { "groupdel", new core_groupdel() },
                { "groupmod", new core_groupmod() },
                { "groups", new core_groups() },
                { "chown", new core_chown() },
                { "chgrp", new core_chgrp() }
            };
        }

        public void Execute(string input)
        {
            List<string> parts = new List<string>();
            HashSet<char> parameters = new HashSet<char>();
            string current = "";
            bool inQuotes = false;
            bool inParam = false;

            foreach (char c in input)
            {
                switch (c)
                {
                    case '"':
                        inQuotes = !inQuotes;
                        break;
                    case ' ' when !inQuotes:
                        if (current.Length > 0)
                        {
                            parts.Add(current);
                            current = "";
                        }
                        inParam = false;
                        break;
                    case '-' when !inQuotes:
                        inParam = true;
                        break;
                    default:
                        if (inParam)
                            parameters.Add(c);
                        else
                            current += c;
                        break;
                }
            }

            if (current.Length > 0)
                parts.Add(current);

            if (parts.Count == 0)
                return;

            string commandName = parts[0].ToLower();
            string[] args = new string[parts.Count - 1];
            for (int i = 1; i < parts.Count; i++)
                args[i - 1] = parts[i];

            if (commands.ContainsKey(commandName))
                commands[commandName].Execute(args, parameters);
            else
                Console.WriteLine($"command not found, what exactly is {commandName} ?? ");
        }
    }
}
