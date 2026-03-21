// I CANT FIGURE OUT HOW TO IMPLEMENT THIS YET SO RIGHT NOW THIS IS JUST A DUMMY


using System;
namespace Shinx.Commands
{
    public class core_cp : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Uso: cp <source> <destination>");
                return;
            }

            try
            {
                string source = args[0];
                string destination = args[1];

                Console.WriteLine($"Copiando {source} in {destination}...");
                // not implemented yet
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }
    }
}
