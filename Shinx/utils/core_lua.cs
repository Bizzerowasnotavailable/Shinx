using System;
using System.Collections.Generic;
using System.IO;
using UniLua;
namespace Shinx.Commands
{
    public class core_lua : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: lua <file.lua> [args...] or lua \"inline code\"");
                return;
            }

            string input = args[0];

            string[] scriptArgs = args.Length > 1 ? args[1..] : Array.Empty<string>();
            LuaBridge.SetArgs(scriptArgs);
            LuaBridge.SetParams(parameters);

            try
            {
                if (input.EndsWith(".lua"))
                {
                    string path = input.StartsWith(@"0:\")
                        ? input
                        : Shell.currentDirectory + input;

                    if (!File.Exists(path))
                    {
                        Console.WriteLine($"lua: {args[0]}: no such file");
                        return;
                    }

                    if (!PermissionManager.CanAccess(path, UserManager.currentUser))
                    {
                        Console.WriteLine($"lua: permission denied: {args[0]}");
                        return;
                    }

                    string code = File.ReadAllText(path);
                    var status = LuaBridge.State.L_DoString(code);
                    if (status != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine($"lua: {LuaBridge.State.L_ToString(-1)}");
                        LuaBridge.State.Pop(1);
                    }
                }
                else
                {
                    if (!PermissionManager.CanAccess(Shell.currentDirectory, UserManager.currentUser))
                    {
                        Console.WriteLine("lua: permission denied");
                        return;
                    }

                    var status = LuaBridge.State.L_DoString(input);
                    if (status != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine($"lua: {LuaBridge.State.L_ToString(-1)}");
                        LuaBridge.State.Pop(1);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"lua: {e.Message}");
            }
        }
    }
}