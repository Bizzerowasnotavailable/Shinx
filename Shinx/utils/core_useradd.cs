using System;
using System.Collections.Generic;
namespace Shinx.Commands
{
    public class core_useradd : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: useradd <username> <password>");
                return;
            }

            if (!UserManager.IsRoot(UserManager.currentUser))
            {
                Console.WriteLine("useradd: no permission");
                return;
            }

            if (UserManager.UserExists(args[0]))
            {
                Console.WriteLine("useradd: user already exists");
                return;
            }

            char[] invalidUser = { ' ', '\\', ':', '/', '@', '[', ']', '!', '{', '}', '#' };
            char[] invalidPass = { ' ', '\\', ':' };

            foreach (char c in invalidUser)
            {
                if (args[0].Contains(c))
                {
                    Console.WriteLine($"useradd: username contains invalid character: {c}");
                    return;
                }
            }

            if (args[0].ToLower() != args[0])
            {
                Console.WriteLine("useradd: username must be lowercase");
                return;
            }

            foreach (char c in invalidPass)
            {
                if (args[1].Contains(c))
                {
                    Console.WriteLine($"useradd: password contains invalid character: {c}");
                    return;
                }
            }

            try
            {
                UserManager.AddUser(args[0], args[1]);
                Console.WriteLine($"useradd: user {args[0]} created");
            }
            catch (Exception e)
            {
                Console.WriteLine($"useradd: {e.Message}");
            }
        }
    }
}