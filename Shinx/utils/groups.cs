using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_groups : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            try
            {
                string username = args.Length > 0 ? args[0] : UserManager.currentUser;
                if (!UserManager.UserExists(username))
                {
                    Console.WriteLine($"groups: user not found: {username}");
                    return;
                }
                var userGroups = UserManager.GetGroups(username);
                Console.WriteLine(username + ": " + string.Join(" ", userGroups));
            }
            catch (Exception e)
            {
                Console.WriteLine($"groups: {e.Message}");
            }
        }
    }
}