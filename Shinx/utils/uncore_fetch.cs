using System;
using Cosmos.Core;
namespace Shinx.Commands
{
    public class uncore_fetch : ICommand
    {
        public void Execute(string[] args)
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
                ulong memorySize = CPU.GetAmountOfRAM();
                return (memorySize / 1024 / 1024) + " MB";
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
