using System;
using Cosmos.System.Graphics;
using System.Drawing;

namespace Shinx.GUI
{
    public class Clock : IGuiApp
    {
        public string AppID => "Clock";
        public string DisplayName => "Clock";
        public bool IsVisible { get => Booleans.clock_opened; set => Booleans.clock_opened = value; }
        public void Draw(Canvas vbe)
        {
            if (Booleans.clock_opened)
            {
                Window.Draw(vbe, ref Int_Manager.clock_x, ref Int_Manager.clock_y, 200, 80, DisplayName, ref Booleans.clock_opened, AppID);

                if (Booleans.clock_opened)
                {
                    string timeFormat = Booleans.use_24hr_clock ? "HH:mm:ss" : "hh:mm:ss tt";

                    ASC16.DrawACSIIString(vbe, DateTime.Now.ToString(timeFormat), Color.DarkGreen, (uint)Int_Manager.clock_x + 30, (uint)Int_Manager.clock_y + 40);
                }
            }
        }
        public void HandleKey(ConsoleKeyInfo key)
        {
        }
    }
}