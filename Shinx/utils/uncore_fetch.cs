using Cosmos.Core;
using System;
using System.Collections.Generic;
namespace Shinx.Commands
{
    public class uncore_fetch : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("                                     YXXXXXXY                            ");
            Console.WriteLine("                                  YXXXXXXXXXXXXY                         ");
            Console.WriteLine("                                 XWHXXXXXFFXXXXXX                        ");
            Console.WriteLine("                                YXIXXXXXXEXXXXXXXX                       ");
            Console.WriteLine("                                XXXXXXXXXXXXXXXWVU                       ");
            Console.WriteLine("                                RSSSSSSSSSSVWVUUTS                       ");
            Console.WriteLine("                                NMMLLLLLLMNMTTSRQRXXXX                   ");
            Console.WriteLine("                                 QQRRRRQQPLRRQQPQXXXXXXX                 ");
            Console.WriteLine("                                YXYMLMMLLPPPPPXXXXXXXXXXXY               ");
            Console.WriteLine("                               XXXXXXXXXXXXXXXXXXXXXXXXXWW               ");
            Console.WriteLine("                              YXXXXXXXXXXXXXXXXXXXXXXWWVUS               ");
            Console.WriteLine("                              WXXXXXXXXXXXXXXXXXWWVVUTTSQW               ");
            Console.WriteLine("                               TTUVVVWWWWWVVVVUUTTSRRQQQY                ");
            Console.WriteLine("                                RRRRRRRRRRRRRQQQPPPPPQS                  ");
            Console.WriteLine("                                  RQQQQPPPPPPPPPPPPQ                     ");
            Console.WriteLine("                                       SQQQQQQY                          ");
            Console.WriteLine(" ");
            Console.WriteLine("");
            Console.WriteLine("OS: barebones SHINX");
            Console.WriteLine("Kernel: SHINX");
            Console.WriteLine("CPU: " + GetCPUInfo());
            Console.WriteLine("RAM: " + GetRAMInfo());
            Console.WriteLine("Resolution: " + GetResolution());
            Console.ForegroundColor = ConsoleColor.White;
        }
        private string GetCPUInfo()
        {
            try
            {
                return CPU.GetCPUBrandString();
            }
            catch
            {
                return "idk";
            }
        }
        private string GetRAMInfo()
        {
            try
            {
                ulong total = Cosmos.Core.GCImplementation.GetAvailableRAM();
                ulong used = Cosmos.Core.GCImplementation.GetUsedRAM() / 1024 / 1024;
                return used + " MB used / " + total + " MB total";
            }
            catch
            {
                return "idk";
            }
        }
        private string GetResolution()
        {
            return "currently in console mode";
        }
    }
}
