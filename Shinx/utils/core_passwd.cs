using System;
using System.Collections.Generic;
namespace Shinx.Commands
{
    public class core_passwd : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("usage: passwd <username> <old password> <new password>");
                return;
            }

            if (!UserManager.UserExists(args[0]))
            {
                Console.WriteLine("passwd: user not found: " + args[0]);
                return;
            }

            char[] invalidPass = { ' ', '\\', ':' };
            foreach (char c in invalidPass)
            {
                if (args[2].Contains(c))
                {
                    Console.WriteLine($"passwd: new password contains invalid character: {c}");
                    return;
                }
            }

            try
            {
                if (args[0] == UserManager.currentUser)
                {
                    if (!UserManager.GetPasswordHash(args[0]).Equals(core_sha256.Hash(args[1])))
                    {
                        Console.WriteLine("passwd: incorrect old password");
                        return;
                    }
                    UserManager.ChangeUserPass(args[0], args[2]);
                    Console.WriteLine("passwd: password changed successfully");
                }
                else if (UserManager.currentUser == "root")
                {
                    if (args[0] == "root")
                    {
                        Console.WriteLine("passwd: use passwd root <old> <new> to change root password");
                        return;
                    }
                    UserManager.ChangeUserPass(args[0], args[2]);
                    Console.WriteLine($"passwd: changed {args[0]}'s password");
                }
                else
                {
                    Console.WriteLine($"passwd: no permission to change {args[0]}'s password");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"passwd: {e.Message}");
            }
        }
    }
}