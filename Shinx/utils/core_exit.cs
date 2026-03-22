using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    internal class core_exit : ICommand
    {
        public void Execute(string[] args, HashSet<char> paramaters) 
        {
            try
            {
                UserManager.Logout();
            }
            catch (Exception e)
            {
                Console.WriteLine($"exit: {e.Message}");
            }
        }
    }
}
