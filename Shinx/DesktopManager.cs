using System;
using System.Drawing;
using Cosmos.System;
using Cosmos.System.Graphics;

namespace Shinx
{
    public static class DesktopManager
    {
        public static bool Running = true;
        private static VBECanvas vbe;
        private static int frameCount = 0;

        private static Pen taskbarPen = new Pen(Color.FromArgb(255, 40, 40, 40));
        private static Pen startBtnPen = new Pen(Color.LightGray);

        public static void Start()
        {
            while (System.Console.KeyAvailable) { System.Console.ReadKey(true); }

            Running = true;

            try
            {
                if (vbe == null)
                    vbe = (VBECanvas)FullScreenCanvas.GetFullScreenCanvas(new Mode(800, 600, ColorDepth.ColorDepth32));

                MouseManager.ScreenWidth = 800;
                MouseManager.ScreenHeight = 600;

                while (Running)
                {
                    vbe.Clear(Shinx.GUI.Booleans.desktop_color);

                    Shinx.GUI.WindowManager.DrawWindows(vbe);

                    vbe.DrawFilledRectangle(taskbarPen, 0, 570, 800, 30);
                    vbe.DrawFilledRectangle(startBtnPen, 0, 570, 60, 30);
                    Shinx.GUI.ASC16.DrawACSIIString(vbe, "Start", Color.Black, 10, 578);

                    int mx = (int)MouseManager.X;
                    int my = (int)MouseManager.Y;

                    if (mx > 792)
                    {
                        MouseManager.X = 792;
                        mx = 792;
                    }
                    if (my > 592)
                    {
                        MouseManager.Y = 592;
                        my = 592;
                    }

                    if (Shinx.GUI.Mouse.Click())
                    {
                        if (mx >= 0 && mx <= 60 && my >= 570 && my <= 600)
                        {
                            Shinx.GUI.DrawMenu.menu = !Shinx.GUI.DrawMenu.menu;
                        }
                    }
                    Shinx.GUI.DrawMenu.update(new System.Drawing.Point(mx, my), vbe);
                    Shinx.GUI.Mouse.DrawMouse(vbe, mx, my);

                    vbe.Display();

                    frameCount++;
                    if (frameCount > 500) { Cosmos.Core.Memory.Heap.Collect(); frameCount = 0; }
                }

                vbe.Disable();
                System.Console.Clear();
            }
            catch (Exception e)
            {
                vbe = null;
                System.Console.WriteLine("GUI Panic: " + e.Message);
            }
        }
    }
}