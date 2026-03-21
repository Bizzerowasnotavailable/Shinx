
// I FUCKING FIGURED OUT HOW TO RUN THIS SHI WITHOUT VMWARE YIPEEE
// SHOUTOUT TO HIRPUS LAB

using System;
using Sys = Cosmos.System;
using Shinx.Commands;

namespace Shinx
{
    public class Kernel : Sys.Kernel
    {
        private peppe commandHandler;

        protected override void BeforeRun()
        {
            Console.WriteLine("SHINX booted successfully, keep in mind, this is PRE-ALPHA software :3");
            Console.WriteLine("type lico for a list of commands");
            commandHandler = new peppe();
        }

        protected override void Run()
        {
            Console.Write("shinx> ");
            var input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                commandHandler.Execute(input);
            }
        }
    }
}
