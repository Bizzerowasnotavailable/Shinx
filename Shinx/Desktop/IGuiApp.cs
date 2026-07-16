using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Graphics;
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