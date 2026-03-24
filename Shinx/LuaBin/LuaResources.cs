using IL2CPU.API.Attribs;

namespace Shinx
{
    public static class LuaResources
    {
        [ManifestResourceStream(ResourceName = "Shinx.LuaUtils.fetch.lua")]
        public static byte[] fetch;

        [ManifestResourceStream(ResourceName = "Shinx.LuaUtils.bunnysay.lua")]
        public static byte[] bunnysay;

        public static readonly (string Name, byte[] Data)[] AllFiles =
        {
            ("fetch.lua", fetch),
            ("bunnysay.lua", bunnysay)
        };
    }
}