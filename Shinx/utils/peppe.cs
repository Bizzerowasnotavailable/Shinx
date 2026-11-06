using Shinx.utils;
using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class peppe // le funny name
    {
        public static Dictionary<string, string> descriptions = new Dictionary<string, string>();
        public static Dictionary<string, ICommand> commands; // implements the commandname commanddothings structure

        public peppe()
        {
            commands = new Dictionary<string, ICommand>();

            RegisterCommand("echo", new core_echo(), "display a message in the terminal");
            RegisterCommand("help", new core_help(), "list commands, or describe one (use -e to open the list in the editor)");
            RegisterCommand("clear", new core_clear(), "clear the terminal");
            RegisterCommand("ls", new core_ls(), "list files in a directory");
            RegisterCommand("pwd", new core_pwd(), "print the current working directory");
            RegisterCommand("history", new core_history(), "show or clear the command history");
            RegisterCommand("cp", new core_cp(), "copy files");
            RegisterCommand("mv", new core_mv(), "move files");
            RegisterCommand("mkdir", new core_mkdir(), "make a directory");
            RegisterCommand("cat", new core_cat(), "show the contents of a file");
            RegisterCommand("cd", new core_cd(), "change directory");
            RegisterCommand("edit", new core_edit(), "basic text editor");
            RegisterCommand("rm", new core_rm(), "delete a file or directory");
            RegisterCommand("sha256sum", new core_sha256(), "calculate the sha256 hash of a string");
            RegisterCommand("useradd", new core_useradd(), "add a new user");
            RegisterCommand("userdel", new core_userdel(), "delete a user");
            RegisterCommand("passwd", new core_passwd(), "change a user's password");
            RegisterCommand("whoami", new core_whoami(), "print the current username");
            RegisterCommand("exit", new core_exit(), "close the terminal window (logout in the console)");
            RegisterCommand("quit", new core_exit(), "alias for exit");
            RegisterCommand("su", new core_su(), "switch user");
            RegisterCommand("groupadd", new core_groupadd(), "create a new group");
            RegisterCommand("groupdel", new core_groupdel(), "delete a group");
            RegisterCommand("groupmod", new core_groupmod(), "add or remove a user from a group");
            RegisterCommand("groups", new core_groups(), "list groups for a user");
            RegisterCommand("chown", new core_chown(), "change the owner of a file or directory");
            RegisterCommand("chgrp", new core_chgrp(), "change the group of a file or directory");
            RegisterCommand("lua", new core_lua(), "run the Lua interpreter");
            RegisterCommand("http", new core_http(), "start the HTTP or HTTPS server");
            RegisterCommand("wget", new core_wget(), "download a file over HTTP or HTTPS");
            RegisterCommand("desktop", new core_desktop(), "launch the graphical environment");
            RegisterCommand("net", new core_network(), "manage network configuration");
            RegisterCommand("lpkg", new core_pkg(), "package manager");
            RegisterCommand("audio", new core_audio(), "play WAV files, beep and set volume");
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
                    case '-' when !inQuotes && current.Length == 0:
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

            if (!commands.ContainsKey(commandName))
            {
                Console.WriteLine($"command not found, what exactly is {commandName} ?? ");
                return;
            }

            if (parameters.Contains('h'))
            {
                commands[commandName].Usage();
                return;
            }

            commands[commandName].Execute(args, parameters);
        }
        public static void RegisterCommand(string name, ICommand command, string desc = "lua command")
        {
            commands[name] = command;
            descriptions[name] = desc;
        }
        public static void UnregisterCommand(string name)
        {
            if (commands.ContainsKey(name))
            {
                commands.Remove(name);
                descriptions.Remove(name);
            }
            
        }
    }
}
