using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System.Drawing;

namespace Shinx.GUI
{
    class DrawMenu
    {
        public static bool menu = false;

        public static void update(System.Drawing.Point CurMouse, Canvas vbe)
        {
            if (menu)
            {
                vbe.DrawFilledRectangle(Color.DarkBlue, 0, 270, 150, 300);
                vbe.DrawRectangle(Color.White, 0, 270, 150, 300);

                string username = UserManager.currentUser;
                ASC16.DrawACSIIString(vbe, "Hello, " + username, Color.Yellow, 10, 278);
                vbe.DrawLine(Color.White, 5, 295, 145, 295);

                vbe.DrawFilledRectangle(Color.Teal, 5, 305, 140, 25);
                ASC16.DrawACSIIString(vbe, "About", Color.White, 15, 310);

                vbe.DrawFilledRectangle(Color.Teal, 5, 335, 140, 25);
                ASC16.DrawACSIIString(vbe, "Clock", Color.White, 15, 340);

                vbe.DrawFilledRectangle(Color.Teal, 5, 365, 140, 25);
                ASC16.DrawACSIIString(vbe, "Calculator", Color.White, 15, 370);

                vbe.DrawFilledRectangle(Color.Teal, 5, 395, 140, 25);
                ASC16.DrawACSIIString(vbe, "Calendar", Color.White, 15, 400);

                vbe.DrawFilledRectangle(Color.Teal, 5, 425, 140, 25);
                ASC16.DrawACSIIString(vbe, "Terminal", Color.White, 15, 430);

                vbe.DrawFilledRectangle(Color.Gray, 5, 455, 140, 25);
                ASC16.DrawACSIIString(vbe, "Settings", Color.White, 15, 460);

                vbe.DrawFilledRectangle(Color.DarkRed, 5, 515, 140, 25);
                ASC16.DrawACSIIString(vbe, "Shutdown", Color.White, 15, 520);

                if (Mouse.Click())
                {
                    if (CurMouse.X > 5 && CurMouse.X < 145)
                    {
                        if (CurMouse.Y > 305 && CurMouse.Y < 330) { Booleans.info_opened = true; menu = false; }
                        if (CurMouse.Y > 335 && CurMouse.Y < 360) { Booleans.clock_opened = true; menu = false; }
                        if (CurMouse.Y > 365 && CurMouse.Y < 390) { Booleans.calc_opened = true; menu = false; }
                        if (CurMouse.Y > 395 && CurMouse.Y < 420) { Booleans.calendar_opened = true; menu = false; }
                        if (CurMouse.Y > 425 && CurMouse.Y < 450) { Booleans.terminal_opened = true; menu = false; }
                        if (CurMouse.Y > 455 && CurMouse.Y < 480) { Booleans.settings_opened = true; menu = false; }
                        if (CurMouse.Y > 515 && CurMouse.Y < 540) { Cosmos.System.Power.Shutdown(); }
                    }
                }
            }
        }
    }
}