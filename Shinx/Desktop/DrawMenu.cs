using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;

namespace Shinx.GUI
{
    class DrawMenu
    {
        public static bool menu = false;

        public static void update(System.Drawing.Point CurMouse, VBECanvas vbe)
        {
            if (menu)
            {
                vbe.DrawFilledRectangle(new Pen(Color.DarkBlue), 0, 320, 150, 250);
                vbe.DrawRectangle(new Pen(Color.White), 0, 320, 150, 250);

                string username = UserManager.currentUser;
                ASC16.DrawACSIIString(vbe, "Hello, " + username, Color.Yellow, 10, 328);
                vbe.DrawLine(new Pen(Color.White), 5, 345, 145, 345);

                vbe.DrawFilledRectangle(new Pen(Color.Teal), 5, 355, 140, 25);
                ASC16.DrawACSIIString(vbe, "About", Color.White, 15, 360);

                vbe.DrawFilledRectangle(new Pen(Color.Teal), 5, 385, 140, 25);
                ASC16.DrawACSIIString(vbe, "Clock", Color.White, 15, 390);

                vbe.DrawFilledRectangle(new Pen(Color.Teal), 5, 415, 140, 25);
                ASC16.DrawACSIIString(vbe, "Calculator", Color.White, 15, 420);

                vbe.DrawFilledRectangle(new Pen(Color.Teal), 5, 445, 140, 25);
                ASC16.DrawACSIIString(vbe, "Calendar", Color.White, 15, 450);

                vbe.DrawFilledRectangle(new Pen(Color.Gray), 5, 475, 140, 25);
                ASC16.DrawACSIIString(vbe, "Settings", Color.White, 15, 480);

                vbe.DrawFilledRectangle(new Pen(Color.DarkRed), 5, 535, 140, 25);
                ASC16.DrawACSIIString(vbe, "Shutdown", Color.White, 15, 540);

                if (Mouse.Click())
                {
                    if (CurMouse.X > 5 && CurMouse.X < 145)
                    {
                        if (CurMouse.Y > 355 && CurMouse.Y < 380) { Booleans.info_opened = true; menu = false; }
                        if (CurMouse.Y > 385 && CurMouse.Y < 410) { Booleans.clock_opened = true; menu = false; }
                        if (CurMouse.Y > 415 && CurMouse.Y < 440) { Booleans.calc_opened = true; menu = false; }
                        if (CurMouse.Y > 445 && CurMouse.Y < 470) { Booleans.calendar_opened = true; menu = false; }
                        if (CurMouse.Y > 475 && CurMouse.Y < 500) { Booleans.settings_opened = true; menu = false; }
                        if (CurMouse.Y > 535 && CurMouse.Y < 560) { Cosmos.System.Power.Shutdown(); }
                    }
                }
            }
        }
    }
}