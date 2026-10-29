using System;

namespace Shinx
{
    public static class ConsoleUi
    {
        public static void Clear()
        {
            if (DesktopManager.Running && Shinx.GUI.Booleans.terminal_opened)
            {
                Shinx.GUI.Terminal.ClearBuffer();
                return;
            }

            Console.Clear();
        }
    }
}
