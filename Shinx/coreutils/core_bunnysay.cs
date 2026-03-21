using System;
namespace Shinx.Commands
{
    public class core_bunnysay : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("bunnysay");
                Console.WriteLine("like cowsay, but way worse, now on SHINX");
                Console.Write("tell me: ");
                return;
            }
            string input = string.Join(" ", args);

            // logic for the textbox that dynamically changes in size
            int length = input.Length;
            string line = "";
            for (int i = 0; i < length + 4; i++)
            {
                line += "-";
            }

            Console.WriteLine(line);
            Console.WriteLine("| " + input + " |");
            Console.WriteLine(line);
            Console.WriteLine("^ ^      ||");
            Console.WriteLine("(0w0) <3 ||");
            Console.WriteLine("/> >|    ||");
            Console.WriteLine(" ");
            Console.WriteLine("by Bizzero"); // removed the year cuz yeah
        }
    }
}
