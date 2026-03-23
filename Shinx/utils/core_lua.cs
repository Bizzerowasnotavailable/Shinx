using System;
using System.Collections.Generic;
using Cosmos.System.FileSystem.VFS;
using UniLua;

namespace Shinx.Commands
{
    public class core_lua : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("lua: no input");
                return;
            }

            try
            {
                ILuaState lua = LuaAPI.NewState();

                lua.L_OpenLibs();

                string input = args[0];

                if (input.EndsWith(".lua"))
                {
                    string path = input.StartsWith(@"0:\")
                        ? input
                        : Shell.currentDirectory + input;

                    if (!VFSManager.FileExists(path))
                    {
                        Console.WriteLine("lua: file not found");
                        return;
                    }

                    var file = VFSManager.GetFile(path);
                    var stream = file.GetFileStream();

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    string script = System.Text.Encoding.ASCII.GetString(buffer);

                    var status = lua.L_DoString(script);

                    if (status != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine("lua error: " + lua.ToString(-1));
                    }
                }
                else
                {
                    var status = lua.L_DoString(input);

                    if (status != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine("lua error: " + lua.ToString(-1));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("lua: " + e.Message);
            }
        }
    }
}
