using System;
using Sys = Cosmos.System;
using System.Drawing;
using Cosmos.System.Graphics;
using CMouse = Cosmos.System.MouseManager;

namespace Shinx.GUI
{
    public static class Mouse
    {
        public static Sys.MouseState PrevMouseState = Sys.MouseState.None;

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
            PrevMouseState = CMouse.MouseState;
        }


        public static void DrawMouse(Canvas vbe, int x, int y)
        {
            DrawCursorInternal(vbe, x + 1, y + 1, borderPen);
            DrawCursorInternal(vbe, x, y, mousePen);
        }

        private static void DrawCursorInternal(Canvas vbe, int x, int y, Color color)
        {
            for (int i = 0; i < cursorShape.Length; i++)
            {
                vbe.DrawLine(color, x + cursorShape[i][0], y + i, x + cursorShape[i][1], y + i);
            }
        }
        public static bool Click()
        {
            return CMouse.MouseState == Sys.MouseState.Left && PrevMouseState != Sys.MouseState.Left;
        }
        public static bool IsPressed()
        {
            return CMouse.MouseState == Sys.MouseState.Left;
        }

        public static bool RightClick()
        {
            return CMouse.MouseState == Sys.MouseState.Right && PrevMouseState != Sys.MouseState.Right;
        }
    }
}