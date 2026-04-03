using Cosmos.System;
using Cosmos.System.Graphics;
using System;

namespace Shinx.GUI
{
    public interface IGuiApp
    {
        string AppID { get; }
        string DisplayName { get; }
        bool IsVisible { get; set; }
        void Draw(Canvas vbe);
        void HandleKey(ConsoleKeyInfo key);
    }
}