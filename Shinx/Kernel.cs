
// I FUCKING FIGURED OUT HOW TO RUN THIS SHI WITHOUT VMWARE YIPEEE
// SHOUTOUT TO HIRPUS LAB

using Cosmos.System.Graphics;
using IL2CPU.API.Attribs;
using Shinx.Commands;
using System;
using Sys = Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Shinx
{
    public class Kernel : Sys.Kernel
    {
        private peppe commandHandler;
        private CosmosVFS vfs;

        protected override void BeforeRun()
        {
            vfs = new CosmosVFS();
            VFSManager.RegisterVFS(vfs);

            commandHandler = new peppe();
            Console.WriteLine("SHINX booted successfully, keep in mind, this is PRE-ALPHA software :3");
            Console.WriteLine("type lico for a list of commands");
            commandHandler.Execute("fetch");
        }

        protected override void Run()
        {
            Console.Write(Shell.currentDirectory + ">");
            var input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                commandHandler.Execute(input);
            }
        }
    }
}