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
                vbe.DrawFilledRectangle(Color.DarkBlue, 0, 240, 150, 330);
                vbe.DrawRectangle(Color.White, 0, 240, 150, 330);

                string username = UserManager.currentUser;
                ASC16.DrawACSIIString(vbe, "Hello, " + username, Color.Yellow, 10, 248);
                vbe.DrawLine(Color.White, 5, 265, 145, 265);

                vbe.DrawFilledRectangle(Color.Teal, 5, 275, 140, 25);
                ASC16.DrawACSIIString(vbe, "About", Color.White, 15, 280);

                vbe.DrawFilledRectangle(Color.Teal, 5, 305, 140, 25);
                ASC16.DrawACSIIString(vbe, "Clock", Color.White, 15, 310);

                vbe.DrawFilledRectangle(Color.Teal, 5, 335, 140, 25);
                ASC16.DrawACSIIString(vbe, "Calculator", Color.White, 15, 340);

                vbe.DrawFilledRectangle(Color.Teal, 5, 365, 140, 25);
                ASC16.DrawACSIIString(vbe, "Calendar", Color.White, 15, 370);

                vbe.DrawFilledRectangle(Color.Teal, 5, 395, 140, 25);
                ASC16.DrawACSIIString(vbe, "Terminal", Color.White, 15, 400);

                vbe.DrawFilledRectangle(Color.Teal, 5, 425, 140, 25);
                ASC16.DrawACSIIString(vbe, "Text Editor", Color.White, 15, 430);

                vbe.DrawFilledRectangle(Color.Gray, 5, 455, 140, 25);
                ASC16.DrawACSIIString(vbe, "Settings", Color.White, 15, 460);

                vbe.DrawFilledRectangle(Color.DarkRed, 5, 540, 140, 25);
                ASC16.DrawACSIIString(vbe, "Shutdown", Color.White, 15, 545);

                if (Mouse.Click())
                {
                    if (CurMouse.X > 5 && CurMouse.X < 145)
                    {
                        if (CurMouse.Y > 275 && CurMouse.Y < 300) { Booleans.info_opened = true; menu = false; }
                        if (CurMouse.Y > 305 && CurMouse.Y < 330) { Booleans.clock_opened = true; menu = false; }
                        if (CurMouse.Y > 335 && CurMouse.Y < 360) { Booleans.calc_opened = true; menu = false; }
                        if (CurMouse.Y > 365 && CurMouse.Y < 390) { Booleans.calendar_opened = true; menu = false; }
                        if (CurMouse.Y > 395 && CurMouse.Y < 420) { Booleans.terminal_opened = true; menu = false; }
                        if (CurMouse.Y > 425 && CurMouse.Y < 450)
                        {
                            TextEditor.Open(@"0:\newfile.txt");
                            menu = false;
                        }
                        if (CurMouse.Y > 455 && CurMouse.Y < 480) { Booleans.settings_opened = true; menu = false; }
                        if (CurMouse.Y > 540 && CurMouse.Y < 565) { Cosmos.System.Power.Shutdown(); }
                    }
                }
            }
        }
    }
}