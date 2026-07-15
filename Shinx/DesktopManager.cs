using System;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Mouse;
using Cosmos.Kernel.System.Timer;
using System.Drawing;

namespace Shinx
{
    public static class DesktopManager
    {
        public static bool Running = false;
        private static Canvas vbe;
        private static int frameCount = 0;

        private static Color taskbarPen = Color.FromArgb(255, 40, 40, 40);
        private static Color startBtnPen = Color.LightGray;

        public static void Start()
        {
            while (System.Console.KeyAvailable) { System.Console.ReadKey(true); }

            Running = true;

            try
            {
                if (vbe == null)
                {
                    vbe = Canvas.GetFullScreen();
                    TimerManager.Wait(50);
                }

                MouseManager.SetScreenSize(vbe.Width, vbe.Height);

                while (Running)
                {
                    Shinx.GUI.Mouse.UpdateState();

                    vbe.Clear(Shinx.GUI.Booleans.desktop_color);

                    Shinx.GUI.WindowManager.DrawWindows(vbe);

                    if (System.Console.KeyAvailable)
                    {
                        var key = System.Console.ReadKey(true);
                        if (Shinx.GUI.WindowManager.drawOrder.Count > 0)
                        {
                            string topAppID = Shinx.GUI.WindowManager.drawOrder[Shinx.GUI.WindowManager.drawOrder.Count - 1];
                            var activeApp = Shinx.GUI.AppManager.GetApp(topAppID);
                            if (activeApp != null && activeApp.IsVisible)
                            {
                                activeApp.HandleKey(key);
                            }
                        }
                    }

                    vbe.DrawFilledRectangle(taskbarPen, 0, 570, 800, 30);
                    vbe.DrawFilledRectangle(startBtnPen, 0, 570, 60, 30);
                    Shinx.GUI.ASC16.DrawACSIIString(vbe, "Start", Color.Black, 10, 578);

                    int mx = MouseManager.X;
                    int my = MouseManager.Y;

                    if (mx > 792) { MouseManager.SetPosition(792, my); mx = 792; }
                    if (my > 592) { MouseManager.SetPosition(mx, 592); my = 592; }

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
                    if (frameCount > 1000) { GC.Collect(); frameCount = 0; }
                }

                vbe.Disable();
                vbe = null;
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