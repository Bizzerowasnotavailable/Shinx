using System;
using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;

namespace Shinx.GUI
{
    public class SettingsApp : IGuiApp
    {
        public string AppID => "Settings";
        public string DisplayName => "Settings";
        public bool IsVisible { get => Booleans.settings_opened; set => Booleans.settings_opened = value; }

        public static bool wasSettingsClickedLastFrame = false;
        public void Draw(Canvas vbe)
        {
            if (!Booleans.settings_opened)
            {
                wasSettingsClickedLastFrame = true;
                return;
            }

            Window.Draw(vbe, ref Int_Manager.settings_x, ref Int_Manager.settings_y, 250, 280, DisplayName, ref Booleans.settings_opened, AppID);

            if (Booleans.settings_opened)
            {
                int x = Int_Manager.settings_x;
                int y = Int_Manager.settings_y;

                ASC16.DrawACSIIString(vbe, "24-Hour Clock:", Color.Black, (uint)x + 15, (uint)y + 40);
                string clockStatus = Booleans.use_24hr_clock ? "[ON ]" : "[OFF]";
                Color clockCol = Booleans.use_24hr_clock ? Color.DarkGreen : Color.DarkRed;
                ASC16.DrawACSIIString(vbe, clockStatus, clockCol, (uint)x + 180, (uint)y + 40);

                ASC16.DrawACSIIString(vbe, "Desktop Color:", Color.Black, (uint)x + 15, (uint)y + 80);

                vbe.DrawFilledRectangle(Color.RoyalBlue, x + 20, y + 105, 30, 30);
                vbe.DrawFilledRectangle(Color.DarkSlateGray, x + 60, y + 105, 30, 30);
                vbe.DrawFilledRectangle(Color.DarkOliveGreen, x + 100, y + 105, 30, 30);
                vbe.DrawFilledRectangle(Color.Maroon, x + 140, y + 105, 30, 30);
                vbe.DrawFilledRectangle(Color.Purple, x + 180, y + 105, 30, 30);

                int selX = 0;
                if (Booleans.desktop_color == Color.RoyalBlue) selX = 20;
                else if (Booleans.desktop_color == Color.DarkSlateGray) selX = 60;
                else if (Booleans.desktop_color == Color.DarkOliveGreen) selX = 100;
                else if (Booleans.desktop_color == Color.Maroon) selX = 140;
                else if (Booleans.desktop_color == Color.Purple) selX = 180;

                if (selX != 0) vbe.DrawRectangle(Color.White, x + selX - 2, y + 103, 34, 34);

                vbe.DrawFilledRectangle(Color.DarkRed, x + 20, y + 230, 210, 30);
                vbe.DrawRectangle(Color.White, x + 20, y + 230, 210, 30);
                ASC16.DrawACSIIString(vbe, "Exit to Console", Color.White, (uint)x + 55, (uint)y + 238);

                bool isClickedNow = Mouse.Click();
                int mouseX = (int)MouseManager.X;
                int mouseY = (int)MouseManager.Y;

                if (isClickedNow && !wasSettingsClickedLastFrame)
                {
                    if (mouseX >= x + 170 && mouseX <= x + 230 && mouseY >= y + 35 && mouseY <= y + 55)
                        Booleans.use_24hr_clock = !Booleans.use_24hr_clock;

                    if (mouseY >= y + 105 && mouseY <= y + 135)
                    {
                        if (mouseX >= x + 20 && mouseX <= x + 50) Booleans.desktop_color = Color.RoyalBlue;
                        if (mouseX >= x + 60 && mouseX <= x + 90) Booleans.desktop_color = Color.DarkSlateGray;
                        if (mouseX >= x + 100 && mouseX <= x + 130) Booleans.desktop_color = Color.DarkOliveGreen;
                        if (mouseX >= x + 140 && mouseX <= x + 170) Booleans.desktop_color = Color.Maroon;
                        if (mouseX >= x + 180 && mouseX <= x + 210) Booleans.desktop_color = Color.Purple;
                    }

                    if (mouseX >= x + 20 && mouseX <= x + 230 && mouseY >= y + 230 && mouseY <= y + 260)
                    {
                        DesktopManager.Running = false;
                    }
                }

                wasSettingsClickedLastFrame = isClickedNow;
            }
        }
        public void HandleKey(ConsoleKeyInfo key)
        {
        }
    }
}