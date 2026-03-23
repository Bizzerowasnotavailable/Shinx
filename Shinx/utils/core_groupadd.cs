using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_groupadd : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: groupadd <groupname>");
                return;
            }

            if (!UserManager.IsRoot(UserManager.currentUser))
            {
                Console.WriteLine("groupadd: permission denied");
                return;
            }

            try
            {
                UserManager.RegisterGroup(args[0]);
                Console.WriteLine($"groupadd: group {args[0]} created");
            }
            catch (Exception e)
            {
                Console.WriteLine($"groupadd: {e.Message}");
            }
            
        }
    }
}