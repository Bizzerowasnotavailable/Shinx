using System;

namespace Shinx.Commands
{
    public class core_lico : ICommand
    {
        public void Execute(string[] args)
        {
            Console.WriteLine("echo: display a message in the terminal");
            Console.WriteLine("lico: list commands");
            Console.WriteLine("bunnysay: port of bunnysay utility");
            Console.WriteLine("fetch: show system info");
            Console.WriteLine("clear: clear the terminal");
            Console.WriteLine("ls: list files in a directory");
            Console.WriteLine("cp: copy files");
            Console.WriteLine("mv: move files");
            Console.WriteLine("mkdir: make directory");
            Console.WriteLine("cat: show contents of a file");
            Console.WriteLine("cd: change directory");
            Console.WriteLine("edit: basic text editor");
            Console.WriteLine("rm: delete file/directory");
        }
    }
}
