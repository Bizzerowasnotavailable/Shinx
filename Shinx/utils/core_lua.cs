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

            try
            {
                if (input.EndsWith(".lua"))
                {
                    string path = input.StartsWith("/")
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
                    LuaExecutor.Run(() =>
                    {
                        LuaBridge.SetArgs(scriptArgs);
                        LuaBridge.SetParams(parameters);
                        var result = LuaExecutor.DoString(code);
                        if (result.Status != ThreadStatus.LUA_OK)
                            Console.WriteLine($"lua: {result.Error}");
                    });
                }
                else
                {
                    if (!PermissionManager.CanAccess(Shell.currentDirectory, UserManager.currentUser))
                    {
                        Console.WriteLine("lua: permission denied");
                        return;
                    }

                    LuaExecutor.Run(() =>
                    {
                        LuaBridge.SetArgs(scriptArgs);
                        LuaBridge.SetParams(parameters);
                        var result = LuaExecutor.DoString(input);
                        if (result.Status != ThreadStatus.LUA_OK)
                            Console.WriteLine($"lua: {result.Error}");
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"lua: {e.Message}");
            }
        }
    }
}
