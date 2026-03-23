using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_groupmod : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("usage: groupmod <add|del> <username> <groupname>");
                return;
            }

            if (!UserManager.IsRoot(UserManager.currentUser))
            {
                Console.WriteLine("groupmod: permission denied");
                return;
            }

            if (!UserManager.UserExists(args[1]))
            {
                Console.WriteLine($"groupmod: user not found: {args[1]}");
                return;
            }

            if (!UserManager.GroupExists(args[2]))
            {
                Console.WriteLine($"groupmod: group not found: {args[2]}");
                return;
            }

            try
            {
                switch (args[0])
                {
                    case "add":
                        UserManager.AddToGroup(args[1], args[2]);
                        Console.WriteLine($"groupmod: added {args[1]} to {args[2]}");
                        break;
                    case "del":
                        UserManager.RemoveFromGroup(args[1], args[2]);
                        Console.WriteLine($"groupmod: removed {args[1]} from {args[2]}");
                        break;
                    default:
                        Console.WriteLine($"groupmod: unknown subcommand: {args[0]}");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"groupmod: {e.Message}");
            }
        }
    }
}