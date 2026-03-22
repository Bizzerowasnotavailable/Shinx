using System;
using System.Collections.Generic;


namespace Shinx.Commands
{
    public class core_su : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters) {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: su <username> <password>");
                return;
            }

            try
            {
                if (UserManager.UserExists(args[0]) && UserManager.GetPasswordHash(args[0]).Equals(core_sha256.Hash(args[1])))
                {
                    UserManager.SwitchUser(args[0], args[1]);
                }
                else
                {
                    Console.WriteLine("su: Incorrect username/password");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"su: {e.Message}");
            }
        }
    }
}
