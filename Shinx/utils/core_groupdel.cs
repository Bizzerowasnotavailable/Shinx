using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_groupdel : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: groupdel <groupname>");
                return;
            }

            if (!UserManager.IsRoot(UserManager.currentUser))
            {
                Console.WriteLine("groupdel: permission denied");
                return;
            }

            if (args[0] == "root" || args[0] == "user")
            {
                Console.WriteLine("groupdel: cannot delete default group");
                return;
            }

            try
            {
                UserManager.UnregisterGroup(args[0]);
                Console.WriteLine($"groupdel: group {args[0]} deleted");
            }
            catch (Exception e)
            { 
                Console.WriteLine($"groupdel: {e.Message}");
            }
        }
    }
}