using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Graphics;
using System;
using System.Drawing;

namespace Shinx.GUI
{
    public class Computer_information : IGuiApp
    {
        public string AppID => "About";
        public string DisplayName => "About Shinx OS";
        public bool IsVisible
        {
            get => Booleans.info_opened;
            set => Booleans.info_opened = value;
        }

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
                CachedCPU = Cosmos.Kernel.Core.CPU.CpuId.GetBrandString().Trim();
                _cpuLabel = "CPU: " + CachedCPU;

                uint ramTotal = (uint)Shinx.LimineMemory.GetTotalPhysicalRamMB();
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
        public void Draw(Canvas vbe)
        {
            if (!Booleans.info_opened) return;

            if (!_infoLoaded) Initialize();

            if (DateTime.Now.Second != lastSecond)
            {
                lastSecond = DateTime.Now.Second;
                UpdateTimerString();
            }

            Window.Draw(vbe, ref Int_Manager.computeri_x, ref Int_Manager.computeri_y, 350, 180, DisplayName, ref Booleans.info_opened, AppID);

            if (!Booleans.info_opened) return;

            int x = Int_Manager.computeri_x;
            int y = Int_Manager.computeri_y;

            ASC16.DrawACSIIString(vbe, "Shinx OS Desktop Environment", Color.Black, (uint)(x + 10), (uint)(y + 30));
            ASC16.DrawACSIIString(vbe, "----------------------------", Color.Black, (uint)(x + 10), (uint)(y + 50));

            ASC16.DrawACSIIString(vbe, _cpuLabel, Color.Black, (uint)(x + 10), (uint)(y + 70));
            ASC16.DrawACSIIString(vbe, _ramLabel, Color.Black, (uint)(x + 10), (uint)(y + 90));
            ASC16.DrawACSIIString(vbe, _timeLabel, Color.Black, (uint)(x + 10), (uint)(y + 110));
        }

        public void HandleKey(ConsoleKeyInfo key)
        {
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