using System;
using System.Collections.Generic;
using Cosmos.Executable.Lua;
using Shinx.Commands;

namespace Shinx
{
    public class LuaCommand : ICommand
    {
        private string luaFunctionName;
        private string commandName;

        public LuaCommand(string functionName, string commandName)
        {
            luaFunctionName = functionName;
            this.commandName = commandName;
        }

        public void Execute(string[] args, HashSet<char> parameters)
        {
            LuaBridge.SetArgs(args);
            LuaBridge.SetParams(parameters);

            var L = LuaBridge.State;

            L.GetGlobal(luaFunctionName);

            if (L.Type(-1) != LuaType.LUA_TFUNCTION)
            {
                Console.WriteLine("lua: function not found: " + luaFunctionName);
                L.Pop(1);
                return;
            }

            var status = L.PCall(0, 0, 0);

            if (status != ThreadStatus.LUA_OK)
            {
                Console.WriteLine("lua error: " + L.ToString(-1));
                L.Pop(1);
            }
        }

        public void Usage()
        {
            string usage = LuaBridge.GetCommandUsage(commandName);
            if (!string.IsNullOrEmpty(usage))
                Console.WriteLine(usage);
            else
                Console.WriteLine("usage: " + commandName + " [-h]");
        }
    }
}
