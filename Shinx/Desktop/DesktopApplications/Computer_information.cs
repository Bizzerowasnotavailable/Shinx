using Cosmos.System.Graphics;
using System;
using System.Drawing;

namespace Shinx.GUI
{
    public static class Computer_information
    {
        public static string CachedCPU = "Loading...";
        public static string CachedRAM = "0 MB";
        public static string CachedTime = "00:00:00";

        private static string _cpuLabel = "";
        private static string _ramLabel = "";
        private static string _timeLabel = "";

        private static int lastSecond = -1;
        private static bool _infoLoaded = false;

        public static void Initialize()
        {
            try
            {
                CachedCPU = Cosmos.Core.CPU.GetCPUBrandString().Trim();
                _cpuLabel = "CPU: " + CachedCPU;

                uint ramTotal = Cosmos.Core.CPU.GetAmountOfRAM();
                CachedRAM = ramTotal + " MB Total";
                _ramLabel = "RAM: " + CachedRAM;

                _infoLoaded = true;
            }
            catch
            {
                _cpuLabel = "CPU: Unknown";
                _ramLabel = "RAM: Unknown";
            }
        }

        public static void Draw(VBECanvas vbe)
        {
            if (!Booleans.info_opened) return;

            if (!_infoLoaded) Initialize();

            if (DateTime.Now.Second != lastSecond)
            {
                lastSecond = DateTime.Now.Second;
                UpdateTimerString();
            }

            Window.Draw(vbe, ref Int_Manager.computeri_x, ref Int_Manager.computeri_y, 350, 180, "About Shinx OS", ref Booleans.info_opened);

            int x = Int_Manager.computeri_x;
            int y = Int_Manager.computeri_y;

            ASC16.DrawACSIIString(vbe, "Shinx OS Desktop Environment", Color.Black, (uint)x + 10, (uint)y + 30);
            ASC16.DrawACSIIString(vbe, "----------------------------", Color.Black, (uint)x + 10, (uint)y + 50);

            ASC16.DrawACSIIString(vbe, _cpuLabel, Color.Black, (uint)x + 10, (uint)y + 70);
            ASC16.DrawACSIIString(vbe, _ramLabel, Color.Black, (uint)x + 10, (uint)y + 90);
            ASC16.DrawACSIIString(vbe, _timeLabel, Color.Black, (uint)x + 10, (uint)y + 110);
        }

        private static void UpdateTimerString()
        {
            var now = DateTime.Now;
            string h = now.Hour.ToString().PadLeft(2, '0');
            string m = now.Minute.ToString().PadLeft(2, '0');
            string s = now.Second.ToString().PadLeft(2, '0');
            _timeLabel = "OS Time: " + h + ":" + m + ":" + s;
        }
    }
}