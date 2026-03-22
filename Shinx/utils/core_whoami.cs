using System;
using System.Collections.Generic;


namespace Shinx.Commands
{
    public class core_whoami : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            try
            {
                Console.WriteLine(UserManager.currentUser);
            }
            catch (Exception e)
            {
                Console.WriteLine($"whoami: {e.Message}");
            }
        }
    }
}
