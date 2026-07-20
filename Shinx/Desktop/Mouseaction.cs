using System;
using Sys = Cosmos.Kernel.System;
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using CMouse = Cosmos.Kernel.System.Mouse.MouseManager;

namespace Shinx.GUI
{
    public static class Mouse
    {
        private static bool _wasPressed = false;
        private static bool _frameClick = false;
        private static bool _clickConsumed = false;

        private static Color mousePen = Color.White;
        private static Color borderPen = Color.Black;

        private static int[][] cursorShape = new int[][] {
            new int[] {0, 1}, new int[] {0, 2}, new int[] {0, 3}, new int[] {0, 4},
            new int[] {0, 5}, new int[] {0, 6}, new int[] {0, 7}, new int[] {0, 8},
            new int[] {0, 5}, new int[] {0, 2}, new int[] {1, 2}, new int[] {2, 2},
            new int[] {3, 2}
        };

        public static void UpdateState()
        {
            bool currentlyPressed = CMouse.LeftButton;

            _frameClick = (currentlyPressed && !_wasPressed);
            _clickConsumed = false;

            _wasPressed = currentlyPressed;
        }

        public static bool Click()
        {
            return _frameClick;
        }

        public static bool ConsumeClick()
        {
            if (_clickConsumed) return false;
            if (!_frameClick) return false;
            _clickConsumed = true;
            return true;
        }

        public static bool IsPressed()
        {
            return CMouse.LeftButton;
        }

        public static void DrawMouse(Canvas vbe, int x, int y)
        {
            if (x < 0 || y < 0 || x >= ScreenManager.Width || y >= ScreenManager.Height) return;
            DrawCursorInternal(vbe, x + 1, y + 1, borderPen);
            DrawCursorInternal(vbe, x, y, mousePen);
        }

        private static void DrawCursorInternal(Canvas vbe, int x, int y, Color color)
        {
            int maxW = ScreenManager.Width;
            int maxH = ScreenManager.Height;
            for (int i = 0; i < cursorShape.Length; i++)
            {
                int py = y + i;
                if (py >= maxH) break;
                int x0 = x + cursorShape[i][0];
                int x1 = x + cursorShape[i][1];
                if (x0 < 0) x0 = 0;
                if (x1 >= maxW) x1 = maxW - 1;
                if (x0 < x1)
                    vbe.DrawLine(color, x0, py, x1, py);
            }
        }
    }
}