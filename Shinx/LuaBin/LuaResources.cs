using System;
using System.Reflection;

namespace Shinx
{
    public static class LuaResources
    {
        private static byte[]? _fetch;
        private static byte[]? _bunnysay;

        public static byte[] Fetch => _fetch ??= LoadResource("Shinx.LuaUtils.fetch.lua");
        public static byte[] Bunnysay => _bunnysay ??= LoadResource("Shinx.LuaUtils.bunnysay.lua");

        private static byte[] LoadResource(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(name);
            if (stream == null)
            {
                Console.WriteLine($"[LuaResources] Resource not found: {name}");
                return Array.Empty<byte>();
            }
            using var ms = new System.IO.MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}