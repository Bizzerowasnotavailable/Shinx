using Shinx.Commands;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Graphics;
using System;
using System.Drawing;
using System.Threading;

namespace Shinx.GUI
{
    public class Terminal : IGuiApp
    {
        public string AppID => "Terminal";
        public string DisplayName => "Terminal";
        public bool IsVisible { get => Booleans.terminal_opened; set => Booleans.terminal_opened = value; }

        private const int CharW = 8;
        private const int CharH = 16;
        private const int Padding = 4;
        private const int TitleBarH = 20;
        private const int Width = 500;
        private const int Height = 300;

        private int _gridCols;
        private int _gridRows;

        private Thread _commandThread;
        private VirtualConsole _vc;
        private VirtualConsole _cmdVc;
        private ICancellable _cancellable;
        private bool _commandRunning;

        private bool _needsWelcome = true;
        private Canvas _termCanvas;
        private int _lastCursorX = -1;
        private int _lastCursorY = -1;

        private static readonly Color[] CcColors = {
            Color.Black, Color.DarkBlue, Color.DarkGreen, Color.DarkCyan,
            Color.DarkRed, Color.DarkMagenta, Color.FromArgb(128, 128, 0), Color.Gray,
            Color.DarkGray, Color.Blue, Color.Green, Color.Cyan,
            Color.Red, Color.Magenta, Color.Yellow, Color.White
        };

        private void EnsureScreen()
        {
            int contentW = Width - Padding * 2;
            int contentH = Height - TitleBarH - Padding * 2;
            int cols = contentW / CharW;
            int rows = contentH / CharH;

            if (_vc != null && _vc.HasScreen && _gridCols == cols && _gridRows == rows)
                return;

            _gridCols = cols;
            _gridRows = rows;
            _termCanvas = new Canvas(contentW, contentH);
            _lastCursorX = -1;
            _lastCursorY = -1;

            _vc = new VirtualConsole();
            _vc.InitScreen(cols, rows);
            _needsWelcome = true;
        }

        private void PrintLine(string line)
        {
            _vc.ScreenWrite(line);
            _vc.ScreenWriteChar('\n');
        }

        private void WritePrompt()
        {
            if (_vc.ScrCX != 0)
                _vc.ScreenWriteChar('\n');

            string user = string.IsNullOrEmpty(UserManager.currentUser) ? "guest" : UserManager.currentUser;
            string prompt = user + "@" + Shell.currentDirectory + "> ";

            _vc.ScreenWrite(prompt);

            _vc.StartInput();
        }

        public Terminal()
        {
            EnsureScreen();
        }

        private void ProcessCmdOps()
        {
            if (_cmdVc == null) return;
            var ops = _cmdVc.DrainOps();
            foreach (var op in ops)
            {
                switch (op.Type)
                {
                    case VirtualConsole.OpType.Write:
                        if (op.Text != null) _vc.ScreenWrite(op.Text);
                        break;
                    case VirtualConsole.OpType.Clear:
                        _vc.ScreenClear();
                        break;
                    case VirtualConsole.OpType.SetCursor:
                        _vc.ScreenSetCursor(op.X, op.Y);
                        break;
                    case VirtualConsole.OpType.SetFg:
                        if ((int)op.Color < CcColors.Length)
                            _vc.ScreenSetFg(CcColors[(int)op.Color]);
                        break;
                    case VirtualConsole.OpType.SetBg:
                        if ((int)op.Color < CcColors.Length)
                            _vc.ScreenSetBg(CcColors[(int)op.Color]);
                        break;
                    case VirtualConsole.OpType.ResetColor:
                        _vc.ScreenSetFg(Color.White);
                        _vc.ScreenSetBg(Color.Black);
                        break;
                }
            }
        }

        public void Draw(Canvas canvas)
        {
            if (!Booleans.terminal_opened) return;

            EnsureScreen();

            if (_needsWelcome && !_commandRunning)
            {
                _needsWelcome = false;
                PrintLine("SHINX Terminal");
                PrintLine("type 'exit' to close");
                WritePrompt();
            }

            if (_commandRunning)
            {
                ProcessCmdOps();
                if (_commandThread != null && !_commandThread.IsAlive)
                    FinishCommand();
            }

            string title = _commandRunning ? "Terminal (running)" : DisplayName;
            Window.Draw(canvas, ref Int_Manager.terminal_x, ref Int_Manager.terminal_y, Width, Height, title, ref Booleans.terminal_opened, AppID);
            if (!Booleans.terminal_opened) return;

            int x = Int_Manager.terminal_x;
            int y = Int_Manager.terminal_y;
            int contentX = x + Padding;
            int contentY = y + TitleBarH + Padding;

            char[,] chars = _vc.ScrChars;
            Color[,] fg = _vc.ScrFgBuf;
            Color[,] bg = _vc.ScrBgBuf;
            bool[,] dirty = _vc.DrainDirty();

            if (chars == null) return;

            for (int r = 0; r < _gridRows; r++)
            {
                for (int c = 0; c < _gridCols; c++)
                {
                    if (!dirty[r, c]) continue;

                    char ch = chars[r, c];
                    Color fgC = fg[r, c];
                    Color bgC = bg[r, c];

                    if (bgC != Color.Black)
                        _termCanvas.DrawFilledRectangle(bgC, c * CharW, r * CharH, CharW, CharH);
                    else
                        _termCanvas.DrawFilledRectangle(Color.Black, c * CharW, r * CharH, CharW, CharH);

                    if (ch != ' ')
                        ASC16.DrawACSIIString(_termCanvas, ch.ToString(), fgC,
                            (uint)(c * CharW), (uint)(r * CharH));
                }
            }

            int cx = Math.Clamp(_vc.ScrCX, 0, _gridCols - 1);
            int cy = Math.Clamp(_vc.ScrCY, 0, _gridRows - 1);

            if (_lastCursorX >= 0 && _lastCursorY >= 0 && (_lastCursorX != cx || _lastCursorY != cy))
            {
                char och = chars[_lastCursorY, _lastCursorX];
                Color ofg = fg[_lastCursorY, _lastCursorX];
                Color obg = bg[_lastCursorY, _lastCursorX];
                _termCanvas.DrawFilledRectangle(obg != Color.Black ? obg : Color.Black,
                    _lastCursorX * CharW, _lastCursorY * CharH, CharW, CharH);
                if (och != ' ')
                    ASC16.DrawACSIIString(_termCanvas, och.ToString(), ofg,
                        (uint)(_lastCursorX * CharW), (uint)(_lastCursorY * CharH));
            }

            _termCanvas.DrawFilledRectangle(Color.White,
                cx * CharW, cy * CharH + CharH - 2, CharW, 2);
            _lastCursorX = cx;
            _lastCursorY = cy;

            canvas.DrawCanvas(_termCanvas, contentX, contentY);
        }

        public void HandleKey(ConsoleKeyInfo key)
        {
            if (!Booleans.terminal_opened) return;

            EnsureScreen();

            if (_commandRunning)
            {
                if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
                {
                    if (_cancellable != null)
                        _cancellable.Cancel();
                    else
                        Shell.CancelRequested = true;
                    _vc.ScreenWrite("^C\n");
                    return;
                }
                _cmdVc?.EnqueueKey(key);
                return;
            }

            if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
            {
                _vc.ScreenWrite("^C\n");
                WritePrompt();
                return;
            }

            _vc.HandleKey(key);

            string cmd = _vc.PendingCommand;
            if (cmd != null)
            {
                if (cmd == "exit" || cmd == "quit")
                {
                    Booleans.terminal_opened = false;
                    return;
                }
                if (!string.IsNullOrEmpty(cmd))
                    StartCommand(cmd);
            }
        }

        private void StartCommand(string cmd)
        {
            _cmdVc = new VirtualConsole();
            _cmdVc.SetSize(_gridCols, _gridRows);
            VirtualConsole.Current = _cmdVc;
            _cmdVc.RedirectConsole();

            Shell.CancelRequested = false;
            _cancellable = null;
            string trimmed = cmd.TrimStart();
            int spaceIdx = trimmed.IndexOf(' ');
            string cmdName = spaceIdx > 0 ? trimmed.Substring(0, spaceIdx) : trimmed;
            if (peppe.commands.ContainsKey(cmdName) && peppe.commands[cmdName] is ICancellable c)
                _cancellable = c;

            _commandThread = new Thread(() =>
            {
                try
                {
                    Kernel.commandHandler.Execute(cmd);
                }
                catch (Exception e)
                {
                    VirtualConsole.Current = _cmdVc;
                    Console.WriteLine("error: " + e.Message);
                    VirtualConsole.Current = null;
                }
            });
            _commandRunning = true;
            _commandThread.Start();
        }

        private void FinishCommand()
        {
            _commandRunning = false;
            try { _cmdVc?.RestoreConsole(); } catch { }
            VirtualConsole.Current = null;
            _cmdVc = null;
            _cancellable = null;
            _commandThread = null;
            WritePrompt();
        }
    }
}
