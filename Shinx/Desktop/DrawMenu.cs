using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;

namespace Shinx.GUI
{
    public static class DrawMenu
    {
        public static bool menu = false;

        public static void update(Point CurMouse, Canvas vbe)
        {
            if (!menu) return;

            vbe.DrawFilledRectangle(Color.FromArgb(255, 0, 0, 150), 0, 240, 150, 330);
            vbe.DrawRectangle(Color.White, 0, 240, 150, 330);

            ASC16.DrawACSIIString(vbe, "Hello, " + UserManager.currentUser, Color.Yellow, (uint)10, (uint)250);

            vbe.DrawLine(Color.White, 5, 268, 145, 268);

            int yOffset = 275;
            foreach (var app in AppManager.Apps)
            {
                vbe.DrawFilledRectangle(Color.Teal, 5, yOffset, 140, 25);

                ASC16.DrawACSIIString(vbe, app.DisplayName, Color.White, (uint)15, (uint)(yOffset + 5));

                if (Mouse.Click() && CurMouse.X > 5 && CurMouse.X < 145 && CurMouse.Y > yOffset && CurMouse.Y < yOffset + 25)
                {
                    app.IsVisible = true;
                    WindowManager.windowToBringToFront = app.AppID;
                    menu = false;
                }
                yOffset += 30;
            }

            vbe.DrawFilledRectangle(Color.DarkRed, 5, 540, 140, 25);

            ASC16.DrawACSIIString(vbe, "Shutdown", Color.White, (uint)15, (uint)545);

            if (Mouse.Click() && CurMouse.X > 5 && CurMouse.X < 145 && CurMouse.Y > 540 && CurMouse.Y < 565)
            {
                Cosmos.System.Power.Shutdown();
            }
        }
    }
}