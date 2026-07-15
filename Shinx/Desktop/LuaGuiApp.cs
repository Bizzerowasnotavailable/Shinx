using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Graphics;
using System;
using System.Drawing;
using UniLua;

namespace Shinx.GUI
{
    public class LuaGuiApp : IGuiApp
    {
        public string AppID { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsVisible { get; set; }

        private string _drawFn;
        private string _keyFn;

        public int X, Y, Width, Height;

        public LuaGuiApp(string id, string displayName, string drawFn, string keyFn,
                         int x, int y, int width, int height)
        {
            AppID = id;
            DisplayName = displayName;
            _drawFn = drawFn;
            _keyFn = keyFn;
            X = x; Y = y; Width = width; Height = height;
            IsVisible = false;
        }

        public void Draw(Canvas canvas)
        {
            if (!IsVisible) return;

            bool open = IsVisible;
            Window.Draw(canvas, ref X, ref Y, Width, Height, DisplayName, ref open, AppID);
            IsVisible = open;
            if (!IsVisible) return;

            LuaBridge.SetGuiCanvas(canvas, X, Y, Width, Height);

            var L = LuaBridge.State;
            L.GetGlobal(_drawFn);
            if (L.Type(-1) == LuaType.LUA_TFUNCTION)
            {
                var status = L.PCall(0, 0, 0);
                if (status != ThreadStatus.LUA_OK)
                {
                    string err = L.L_ToString(-1) ?? "draw error";
                    L.Pop(1);
                    ASC16.DrawACSIIString(canvas, err.Substring(0, Math.Min(err.Length, 60)),
                        Color.Red, (uint)(X + 4), (uint)(Y + 24));
                }
            }
            else
            {
                L.Pop(1);
            }

            LuaBridge.ClearGuiCanvas();
        }

        public void HandleKey(ConsoleKeyInfo key)
        {
            if (!IsVisible) return;

            if (!string.IsNullOrEmpty(LuaBridge.InputFocus))
            {
                LuaBridge.EnqueueKey(key);
                return;
            }

            if (string.IsNullOrEmpty(_keyFn)) return;

            var L = LuaBridge.State;
            L.GetGlobal(_keyFn);
            if (L.Type(-1) == LuaType.LUA_TFUNCTION)
            {
                L.NewTable();

                L.PushString("key");
                L.PushString(key.Key.ToString());
                L.SetTable(-3);

                L.PushString("char");
                L.PushString(key.KeyChar == '\0' ? "" : key.KeyChar.ToString());
                L.SetTable(-3);

                L.PushString("ctrl");
                L.PushBoolean((key.Modifiers & ConsoleModifiers.Control) != 0);
                L.SetTable(-3);

                L.PushString("shift");
                L.PushBoolean((key.Modifiers & ConsoleModifiers.Shift) != 0);
                L.SetTable(-3);

                L.PushString("alt");
                L.PushBoolean((key.Modifiers & ConsoleModifiers.Alt) != 0);
                L.SetTable(-3);

                var status = L.PCall(1, 0, 0);
                if (status != ThreadStatus.LUA_OK)
                    L.Pop(1);
            }
            else
            {
                L.Pop(1);
            }
        }
    }
}
