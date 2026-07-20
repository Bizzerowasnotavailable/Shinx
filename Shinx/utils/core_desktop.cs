using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_desktop : ICommand, ICancellable
    {
        public void Cancel()
        {
            DesktopManager.Running = false;
        }

        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (DesktopManager.Running)
            {
                Console.WriteLine("desktop: already running");
                return;
            }

            try
            {
                DesktopManager.Start();
                DesktopManager.Running = false;
                VirtualConsole.Current?.Clear();
                Console.WriteLine("Returned to Shinx Console.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"desktop: {e.Message}");
            }
        }
    }
}
