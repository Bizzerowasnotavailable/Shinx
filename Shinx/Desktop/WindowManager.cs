using Cosmos.System.Graphics;
using System.Collections.Generic;

namespace Shinx.GUI
{
    public static class WindowManager
    {
        public static List<string> drawOrder = new List<string> {
            "About",
            "Clock",
            "Calculator",
            "Calendar",
            "Settings",
            "Terminal",
            "Text Editor"
        };

        public static string windowToBringToFront = "";

        public static void DrawWindows(Canvas vbe)
        {
            for (int i = 0; i < drawOrder.Count; i++)
            {
                string id = drawOrder[i];
                IGuiApp app = AppManager.GetApp(id);

                if (app != null && app.IsVisible)
                {
                    app.Draw(vbe);
                }
            }

            if (windowToBringToFront != "")
            {
                BringToFront(windowToBringToFront);
                windowToBringToFront = "";
            }
        }

        public static void BringToFront(string id)
        {
            int index = drawOrder.IndexOf(id);
            if (index != -1)
            {
                drawOrder.RemoveAt(index);
                drawOrder.Add(id);
            }
        }
    }
}