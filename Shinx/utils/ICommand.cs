using System.Collections.Generic;

namespace Shinx.Commands
{
    public interface ICommand
    {
        void Execute(string[] args, HashSet<char> parameters);
    }
}

// na pezza
