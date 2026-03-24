using System;
using System.Collections.Generic;
using UniLua;
using Shinx.Commands;

namespace Shinx
{
    public class LuaCommand : ICommand
    {
        private string luaFunctionName;

        public LuaCommand(string functionName)
        {
            luaFunctionName = functionName;
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
    }
}