using Cosmos.System.Graphics;

namespace Shinx.GUI
{
    public static class WindowManager
    {
        public static string[] drawOrder = { "About Shinx OS", "Clock", "Calculator", "Calendar", "Settings" };

        public static string windowToBringToFront = "";

        public static void DrawWindows(VBECanvas vbe)
        {
            for (int i = 0; i < drawOrder.Length; i++)
            {
                string windowName = drawOrder[i];

                switch (windowName)
                {
                    case "About Shinx OS":
                        if (Booleans.info_opened) Computer_information.Draw(vbe);
                        break;
                    case "Clock":
                        if (Booleans.clock_opened) Clock.Draw(vbe);
                        break;
                    case "Calculator":
                        if (Booleans.calc_opened) Calculator.Draw(vbe);
                        break;
                    case "Calendar":
                        if (Booleans.calendar_opened) Calendar.Draw(vbe);
                        break;
                    case "Settings":
                        if (Booleans.settings_opened) settings.Draw(vbe);
                        break;
                }
            }

            if (windowToBringToFront != "")
            {
                BringToFront(windowToBringToFront);
                windowToBringToFront = "";
            }
        }

        public static void BringToFront(string title)
        {
            int index = -1;

            for (int i = 0; i < drawOrder.Length; i++)
            {
                if (drawOrder[i] == title)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1 && index != drawOrder.Length - 1)
            {
                string temp = drawOrder[index];

                for (int i = index; i < drawOrder.Length - 1; i++)
                {
                    drawOrder[i] = drawOrder[i + 1];
                }

                drawOrder[drawOrder.Length - 1] = temp;
            }
        }
    }
}