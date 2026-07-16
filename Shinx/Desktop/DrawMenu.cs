using ScreenManager = Shinx.ScreenManager;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Graphics;
using System.Drawing;

namespace Shinx.GUI
{
    public static class DrawMenu
    {
        public static bool menu = false;

        private static int scrollOffset = 0;
        private const int MaxVisible = 7;
        private const int BtnH = 25;
        private const int BtnGap = 30;

        public static void update(Point CurMouse, Canvas vbe)
        {
            if (!menu) return;

            int totalApps = AppManager.Apps.Count;
            int maxScroll = totalApps - MaxVisible;
            if (maxScroll < 0) maxScroll = 0;
            if (scrollOffset > maxScroll) scrollOffset = maxScroll;
            if (scrollOffset < 0) scrollOffset = 0;

            bool hasScroll = totalApps > MaxVisible;
            int visibleCount = totalApps < MaxVisible ? totalApps : MaxVisible;

            int menuH = 30 + 10 + (hasScroll ? BtnGap : 0) + visibleCount * BtnGap + (hasScroll ? BtnGap : 0) + 10 + BtnH + 5;

            int menuY = ScreenManager.TaskbarY - menuH;

            vbe.DrawFilledRectangle(Color.FromArgb(255, 0, 0, 150), 0, menuY, 150, menuH);
            vbe.DrawRectangle(Color.White, 0, menuY, 150, menuH);

            ASC16.DrawACSIIString(vbe, "Hello, " + UserManager.currentUser, Color.Yellow, 10, (uint)(menuY + 8));
            vbe.DrawLine(Color.White, 5, menuY + 26, 145, menuY + 26);

            int yOffset = menuY + 33;

            if (hasScroll)
            {
                Color arrowColor = scrollOffset > 0 ? Color.White : Color.DarkGray;
                vbe.DrawFilledRectangle(Color.FromArgb(255, 0, 0, 100), 5, yOffset, 140, BtnH);
                ASC16.DrawACSIIString(vbe, "    ^ scroll up", arrowColor, 15, (uint)(yOffset + 5));

                if (Mouse.Click() && scrollOffset > 0 &&
                    CurMouse.X > 5 && CurMouse.X < 145 &&
                    CurMouse.Y > yOffset && CurMouse.Y < yOffset + BtnH)
                {
                    scrollOffset--;
                }

                yOffset += BtnGap;
            }

            for (int i = scrollOffset; i < totalApps && i < scrollOffset + MaxVisible; i++)
            {
                var app = AppManager.Apps[i];

                vbe.DrawFilledRectangle(Color.Teal, 5, yOffset, 140, BtnH);
                string label = app.DisplayName;
                if (label.Length > 14) label = label.Substring(0, 13) + ".";
                ASC16.DrawACSIIString(vbe, label, Color.White, 15, (uint)(yOffset + 5));

                if (Mouse.Click() &&
                    CurMouse.X > 5 && CurMouse.X < 145 &&
                    CurMouse.Y > yOffset && CurMouse.Y < yOffset + BtnH)
                {
                    app.IsVisible = true;
                    WindowManager.windowToBringToFront = app.AppID;
                    menu = false;
                    scrollOffset = 0;
                }

                yOffset += BtnGap;
            }

            if (hasScroll)
            {
                Color arrowColor = scrollOffset < maxScroll ? Color.White : Color.DarkGray;
                vbe.DrawFilledRectangle(Color.FromArgb(255, 0, 0, 100), 5, yOffset, 140, BtnH);
                ASC16.DrawACSIIString(vbe, "    v scroll dn", arrowColor, 15, (uint)(yOffset + 5));

                if (Mouse.Click() && scrollOffset < maxScroll &&
                    CurMouse.X > 5 && CurMouse.X < 145 &&
                    CurMouse.Y > yOffset && CurMouse.Y < yOffset + BtnH)
                {
                    scrollOffset++;
                }

                yOffset += BtnGap;
            }

            yOffset += 5;
            vbe.DrawFilledRectangle(Color.DarkRed, 5, yOffset, 140, BtnH);
            ASC16.DrawACSIIString(vbe, "Shutdown", Color.White, 15, (uint)(yOffset + 5));

            if (Mouse.Click() &&
                CurMouse.X > 5 && CurMouse.X < 145 &&
                CurMouse.Y > yOffset && CurMouse.Y < yOffset + BtnH)
            {
                Power.Shutdown();
            }
        }
    }
}