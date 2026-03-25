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

        private static Pen bgPen = new Pen(Color.LightGray);
        private static Pen borderPen = new Pen(Color.Black);
        private static Pen titleBarPen = new Pen(Color.DarkBlue);
        private static Pen closeBtnPen = new Pen(Color.Red);
        private static Pen xIconPen = new Pen(Color.White);

        public static void Draw(VBECanvas vbe, ref int x, ref int y, int width, int height, string title, ref bool isOpen)
        {
            if (!isOpen) return;

            int mouseX = (int)MouseManager.X;
            int mouseY = (int)MouseManager.Y;
            bool mouseClicked = Mouse.Click();

            if (mouseClicked && mouseX >= x && mouseX <= x + width && mouseY >= y && mouseY <= y + height)
            {
                WindowManager.windowToBringToFront = title;
            }

            if (mouseClicked && draggedWindow == "")
            {
                if (mouseX >= x + width - 20 && mouseX <= x + width && mouseY >= y && mouseY <= y + 20)
                {
                    isOpen = false;
                    return;
                }
            }

            if (mouseClicked)
            {
                if (draggedWindow == "" && mouseX >= x && mouseX <= x + width - 20 && mouseY >= y && mouseY <= y + 20)
                {
                    draggedWindow = title;
                    dragOffsetX = mouseX - x;
                    dragOffsetY = mouseY - y;
                }
            }
            else
            {
                draggedWindow = "";
            }

            if (draggedWindow == title)
            {
                x = mouseX - dragOffsetX;
                y = mouseY - dragOffsetY;

                if (x < 0) x = 0;
                if (x > 800 - width) x = 800 - width;
                if (y < 0) y = 0;
                if (y > 570 - height) y = 570 - height;
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