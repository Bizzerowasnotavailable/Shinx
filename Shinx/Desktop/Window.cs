using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;

namespace Shinx.GUI
{
    public static class Window
    {
        public static string draggedWindow = "";
        public static int dragOffsetX = 0;
        public static int dragOffsetY = 0;

        private static Color bgPen = Color.LightGray;
        private static Color borderPen = Color.Black;
        private static Color titleBarPen = Color.DarkBlue;
        private static Color closeBtnPen = Color.Red;
        private static Color xIconPen = Color.White;

        public static void Draw(Canvas vbe, ref int x, ref int y, int width, int height, string title, ref bool isOpen, string windowID)
        {
            if (!isOpen) return;

            int mouseX = (int)MouseManager.X;
            int mouseY = (int)MouseManager.Y;
            bool mouseClicked = Mouse.Click();
            bool mouseHeld = Mouse.IsPressed();

            if (mouseClicked)
            {
                if (mouseX >= x && mouseX <= x + width && mouseY >= y && mouseY <= y + height)
                {
                    WindowManager.windowToBringToFront = windowID;
                }

                if (mouseX >= x + width - 20 && mouseX <= x + width && mouseY >= y && mouseY <= y + 20)
                {
                    isOpen = false;
                    if (draggedWindow == windowID) draggedWindow = "";
                    return;
                }

                if (draggedWindow == "" && mouseX >= x && mouseX <= x + width - 20 && mouseY >= y && mouseY <= y + 20)
                {
                    draggedWindow = windowID;
                    dragOffsetX = mouseX - x;
                    dragOffsetY = mouseY - y;
                }
            }

            if (draggedWindow == windowID)
            {
                if (mouseHeld)
                {
                    x = mouseX - dragOffsetX;
                    y = mouseY - dragOffsetY;

                    if (x < 0) x = 0;
                    if (x > 800 - width) x = 800 - width;
                    if (y < 0) y = 0;
                    if (y > 570 - height) y = 570 - height;
                }
                else
                {
                    draggedWindow = "";
                }
            }

            vbe.DrawFilledRectangle(bgPen, x, y, width, height);
            vbe.DrawRectangle(borderPen, x, y, width, height);
            vbe.DrawFilledRectangle(titleBarPen, x, y, width, 20);
            vbe.DrawFilledRectangle(closeBtnPen, x + width - 20, y, 20, 20);
            vbe.DrawLine(xIconPen, x + width - 15, y + 5, x + width - 5, y + 15);
            vbe.DrawLine(xIconPen, x + width - 5, y + 5, x + width - 15, y + 15);

            ASC16.DrawACSIIString(vbe, title, Color.White, (uint)(x + 5), (uint)(y + 2));
        }
    }
}