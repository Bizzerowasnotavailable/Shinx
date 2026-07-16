using Cosmos.Kernel.System.Graphics;

namespace Shinx
{
    public static class ScreenManager
    {
        private static int _width = 800;
        private static int _height = 600;
        private static bool _initialized = false;
        public static int Width
        {
            get
            {
                if (!_initialized) Initialize();
                return _width;
            }
        }
        public static int Height
        {
            get
            {
                if (!_initialized) Initialize();
                return _height;
            }
        }
        public static int TaskbarHeight => 30;
        public static int TaskbarY => Height - 30;
        public static int StartButtonWidth => 60;
        public static int StartButtonHeight => 30;
        public static int StartButtonX => 0;
        public static int StartButtonY => Height - 30;
        public static int MaxMouseX => Width - 8;
        public static int MaxMouseY => Height - 8;
        public static int ContentHeight => Height - 30;
        public static int MaxWindowX(int windowWidth) => Width - windowWidth;

        public static int MaxWindowY(int windowHeight) => Height - 30 - windowHeight;

        public static void Initialize()
        {
            var canvas = Canvas.GetFullScreen();
            if (canvas != null)
            {
                _width = canvas.Width;
                _height = canvas.Height;
            }
            _initialized = true;
        }

        public static void Refresh()
        {
            Initialize();
        }
    }
}