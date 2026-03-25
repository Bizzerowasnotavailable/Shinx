using Cosmos.System.Graphics;
using System;
using System.Drawing;

namespace Shinx.GUI
{
    public static class Clock
    {
        public static void Draw(VBECanvas vbe)
        {
            if (Booleans.clock_opened)
            {
                Window.Draw(vbe, ref Int_Manager.clock_x, ref Int_Manager.clock_y, 200, 80, "Clock", ref Booleans.clock_opened);

                if (Booleans.clock_opened)
                {
                    string timeFormat = Booleans.use_24hr_clock ? "HH:mm:ss" : "hh:mm:ss tt";
                    ASC16.DrawACSIIString(vbe, DateTime.Now.ToString(timeFormat), Color.DarkGreen, (uint)Int_Manager.clock_x + 30, (uint)Int_Manager.clock_y + 40);
                }
            }
        }
    }
}