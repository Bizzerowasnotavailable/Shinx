using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_userdel : ICommand
    {
        public void Execute(string[] args, HashSet<char> paramaters) 
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: userdel <username>");
                return;
            }

            try
            {
                if (args[0] == UserManager.currentUser)
                {
                    Console.WriteLine("userdel: cannot delete current user.");
                } 
                else if(args[0] != "root" && UserManager.IsRoot(UserManager.currentUser))
                {
                    if (!UserManager.UserExists(args[0]))
                    {
                        Console.WriteLine("userdel: user not found");
                        return;
                    }

                    UserManager.RemoveUser(args[0]);
                    Console.WriteLine($"userdel: successfully deleted user {args[0]}");
                }
                else
                {
                    Console.WriteLine($"userdel: no permission to delete {args[0]}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"userdel: {e.Message}");
            }
        }
    }
}
