using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_desktop : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            try
            {
                DesktopManager.Start();
                Console.Clear();
                Console.WriteLine("Returned to Shinx Console.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"desktop: {e.Message}");
            }
        }
    }
}
