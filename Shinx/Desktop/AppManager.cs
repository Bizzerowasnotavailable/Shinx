using System.Collections.Generic;

namespace Shinx.GUI
{
    public static class AppManager
    {
        public static List<IGuiApp> Apps = new List<IGuiApp>();
        public static void Initialize()
        {
            Apps.Clear();

            Apps.Add(new Computer_information());
            Apps.Add(new Clock());
            Apps.Add(new Calculator());
            Apps.Add(new Calendar());
            Apps.Add(new SettingsApp());
            Apps.Add(new Terminal());
            Apps.Add(new TextEditor());
        }
        public static IGuiApp GetApp(string id)
        {
            return Apps.Find(a => a.AppID == id);
        }
    }
}