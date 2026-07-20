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
        public static volatile bool Running = false;
        private static Canvas vbe;

        private static Color taskbarPen = Color.FromArgb(255, 40, 40, 40);
        private static Color startBtnPen = Color.LightGray;

        public static void Start()
        {
            var vc = VirtualConsole.Current;

            Running = true;

            try
            {
                ScreenManager.Refresh();

                if (vbe == null)
                {
                    vbe = Canvas.GetFullScreen();
                    TimerManager.Wait(50);
                }

                MouseManager.SetScreenSize(ScreenManager.Width, ScreenManager.Height);

                while (Running)
                {
                    Shinx.GUI.Mouse.UpdateState();

                    vbe.Clear(Shinx.GUI.Booleans.desktop_color);

                    Shinx.GUI.WindowManager.DrawWindows(vbe);

                    if (vc != null && vc.KeyAvailable)
                    {
                        var key = vc.ReadKey(true);
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

                    int taskbarY = ScreenManager.TaskbarY;
                    vbe.DrawFilledRectangle(taskbarPen, 0, ScreenManager.TaskbarY, ScreenManager.Width, ScreenManager.TaskbarHeight);
                    vbe.DrawFilledRectangle(Color.LightGray, 0, ScreenManager.TaskbarY, ScreenManager.StartButtonWidth, ScreenManager.StartButtonHeight);
                    Shinx.GUI.ASC16.DrawACSIIString(vbe, "Start", Color.Black, 10, (uint)ScreenManager.StartButtonY + 8);

                    int mx = MouseManager.X;
                    int my = MouseManager.Y;

                    if (mx > ScreenManager.MaxMouseX) { MouseManager.SetPosition(ScreenManager.MaxMouseX, my); mx = ScreenManager.MaxMouseX; }
                    if (my > ScreenManager.MaxMouseY) { MouseManager.SetPosition(mx, ScreenManager.MaxMouseY); my = ScreenManager.MaxMouseY; }

                    if (Shinx.GUI.Mouse.Click())
                    {
                        if (mx >= ScreenManager.StartButtonX && mx <= ScreenManager.StartButtonWidth &&
                            my >= ScreenManager.StartButtonY && my <= ScreenManager.MaxMouseY)
                        {
                            Shinx.GUI.DrawMenu.menu = !Shinx.GUI.DrawMenu.menu;
                        }
                    }

                    Shinx.GUI.DrawMenu.update(new System.Drawing.Point(mx, my), vbe);
                    Shinx.GUI.Mouse.DrawMouse(vbe, mx, my);

                    vbe.Display();
                }

                vbe.Disable();
                vbe = null;
                if (vc != null) vc.Clear();
                else System.Console.Clear();
            }
            catch (Exception e)
            {
                vbe = null;
                Console.WriteLine("GUI Panic: " + e.Message);
            }
        }
    }
}
