using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace Shinx
{
    public class VirtualConsole
    {
        public static VirtualConsole Current;

        public enum OpType { Write, Clear, SetCursor, SetFg, SetBg, ResetColor }

        public struct Op
        {
            public OpType Type;
            public string Text;
            public int X, Y;
            public ConsoleColor Color;
        }

        private readonly object _lock = new object();
        private readonly Queue<Op> _ops = new Queue<Op>();
        private readonly Queue<ConsoleKeyInfo> _input = new Queue<ConsoleKeyInfo>();
        private TextWriter _originalOut;

        private char[,] _scrChars;
        private Color[,] _scrFg;
        private Color[,] _scrBg;
        private int _scrW, _scrH;
        private int _scrCX, _scrCY;
        private Color _scrFgCur = Color.White;
        private Color _scrBgCur = Color.Black;
        private bool _hasScreen;

        private string _inputBuf = "";
        private int _inputCursor = 0;
        private int _inputStartX, _inputStartY;
        private int _inputVisibleLen = 0;
        private int _histIdx = -1;
        private readonly List<string> _hist = new List<string>();
        private bool _inputMode = false;
        private string _pendingCommand = null;

        private static readonly Color[] CcToColor = {
            Color.Black, Color.DarkBlue, Color.DarkGreen, Color.DarkCyan,
            Color.DarkRed, Color.DarkMagenta, Color.FromArgb(128, 128, 0), Color.Gray,
            Color.DarkGray, Color.Blue, Color.Green, Color.Cyan,
            Color.Red, Color.Magenta, Color.Yellow, Color.White
        };

        public bool HasScreen => _hasScreen;
        public int ScrW => _scrW;
        public int ScrH => _scrH;
        public int ScrCX => _scrCX;
        public int ScrCY => _scrCY;
        public char[,] ScrChars => _scrChars;
        public Color[,] ScrFgBuf => _scrFg;
        public Color[,] ScrBgBuf => _scrBg;
        public string PendingCommand { get { lock (_lock) { string r = _pendingCommand; _pendingCommand = null; return r; } } }
        public bool InputActive { get { lock (_lock) return _inputMode; } }

        public void InitScreen(int w, int h)
        {
            lock (_lock)
            {
                _scrW = w;
                _scrH = h;
                _scrChars = new char[h, w];
                _scrFg = new Color[h, w];
                _scrBg = new Color[h, w];
                for (int r = 0; r < h; r++)
                    for (int c = 0; c < w; c++)
                    {
                        _scrChars[r, c] = ' ';
                        _scrFg[r, c] = _scrFgCur;
                        _scrBg[r, c] = _scrBgCur;
                    }
                _scrCX = 0;
                _scrCY = 0;
                _hasScreen = true;
            }
        }

        private void ScrScrollUp()
        {
            for (int r = 0; r < _scrH - 1; r++)
                for (int c = 0; c < _scrW; c++)
                {
                    _scrChars[r, c] = _scrChars[r + 1, c];
                    _scrFg[r, c] = _scrFg[r + 1, c];
                    _scrBg[r, c] = _scrBg[r + 1, c];
                }
            for (int c = 0; c < _scrW; c++)
            {
                _scrChars[_scrH - 1, c] = ' ';
                _scrFg[_scrH - 1, c] = _scrFgCur;
                _scrBg[_scrH - 1, c] = _scrBgCur;
            }
        }

        private void ScrNewline()
        {
            _scrCX = 0;
            _scrCY++;
            if (_scrCY >= _scrH)
            {
                ScrScrollUp();
                _scrCY = _scrH - 1;
            }
        }

        private void ScrWriteChar(char ch)
        {
            if (ch == '\n') { ScrNewline(); return; }
            if (ch == '\r') { _scrCX = 0; return; }
            if (ch == '\t')
            {
                int next = (_scrCX / 8 + 1) * 8;
                while (_scrCX < next && _scrCX < _scrW)
                {
                    _scrChars[_scrCY, _scrCX] = ' ';
                    _scrFg[_scrCY, _scrCX] = _scrFgCur;
                    _scrBg[_scrCY, _scrCX] = _scrBgCur;
                    _scrCX++;
                }
                if (_scrCX >= _scrW) ScrNewline();
                return;
            }
            if (_scrCX >= _scrW) ScrNewline();
            _scrChars[_scrCY, _scrCX] = ch;
            _scrFg[_scrCY, _scrCX] = _scrFgCur;
            _scrBg[_scrCY, _scrCX] = _scrBgCur;
            _scrCX++;
        }

        private void ScrWriteString(string s)
        {
            for (int i = 0; i < s.Length; i++)
                ScrWriteChar(s[i]);
        }

        private void ScrClear()
        {
            for (int r = 0; r < _scrH; r++)
                for (int c = 0; c < _scrW; c++)
                {
                    _scrChars[r, c] = ' ';
                    _scrFg[r, c] = _scrFgCur;
                    _scrBg[r, c] = _scrBgCur;
                }
            _scrCX = 0;
            _scrCY = 0;
        }

        public void ScreenWrite(string s)
        {
            lock (_lock)
            {
                if (_hasScreen) ScrWriteString(s);
            }
        }

        public void ScreenWriteChar(char ch)
        {
            lock (_lock)
            {
                if (_hasScreen) ScrWriteChar(ch);
            }
        }

        public void ScreenSetCursor(int x, int y)
        {
            lock (_lock)
            {
                if (!_hasScreen) return;
                _scrCX = Math.Max(0, Math.Min(x, _scrW - 1));
                _scrCY = Math.Max(0, Math.Min(y, _scrH - 1));
            }
        }

        public void ScreenSetFg(Color fg)
        {
            lock (_lock) { _scrFgCur = fg; }
        }

        public void ScreenSetBg(Color bg)
        {
            lock (_lock) { _scrBgCur = bg; }
        }

        public void ScreenClear()
        {
            lock (_lock)
            {
                if (_hasScreen) ScrClear();
            }
        }

        public void StartInput()
        {
            lock (_lock)
            {
                _inputStartX = _scrCX;
                _inputStartY = _scrCY;
                _inputBuf = "";
                _inputCursor = 0;
                _inputVisibleLen = 0;
                _histIdx = -1;
                _inputMode = true;
                _pendingCommand = null;
            }
        }

        public bool HandleKey(ConsoleKeyInfo key)
        {
            lock (_lock)
            {
                if (!_inputMode) return false;

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        _inputMode = false;
                        _pendingCommand = _inputBuf;
                        if (!string.IsNullOrEmpty(_inputBuf))
                        {
                            if (_hist.Count == 0 || _hist[_hist.Count - 1] != _inputBuf)
                                _hist.Add(_inputBuf);
                        }
                        _histIdx = -1;
                        ScrWriteChar('\n');
                        _inputBuf = "";
                        _inputCursor = 0;
                        _inputVisibleLen = 0;
                        return true;

                    case ConsoleKey.Backspace:
                        if (_inputCursor > 0)
                        {
                            _inputBuf = _inputBuf.Substring(0, _inputCursor - 1) + _inputBuf.Substring(_inputCursor);
                            _inputCursor--;
                            RefreshInput();
                        }
                        break;

                    case ConsoleKey.Delete:
                        if (_inputCursor < _inputBuf.Length)
                        {
                            _inputBuf = _inputBuf.Substring(0, _inputCursor) + _inputBuf.Substring(_inputCursor + 1);
                            RefreshInput();
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (_inputCursor > 0) { _inputCursor--; RefreshInput(); }
                        break;

                    case ConsoleKey.RightArrow:
                        if (_inputCursor < _inputBuf.Length) { _inputCursor++; RefreshInput(); }
                        break;

                    case ConsoleKey.UpArrow:
                        if (_hist.Count == 0) break;
                        if (_histIdx == -1) _histIdx = _hist.Count - 1;
                        else if (_histIdx > 0) _histIdx--;
                        _inputBuf = _hist[_histIdx];
                        _inputCursor = _inputBuf.Length;
                        RefreshInput();
                        break;

                    case ConsoleKey.DownArrow:
                        if (_histIdx == -1) break;
                        if (_histIdx < _hist.Count - 1)
                        {
                            _histIdx++;
                            _inputBuf = _hist[_histIdx];
                        }
                        else
                        {
                            _histIdx = -1;
                            _inputBuf = "";
                        }
                        _inputCursor = _inputBuf.Length;
                        RefreshInput();
                        break;

                    case ConsoleKey.Home:
                        _inputCursor = 0;
                        RefreshInput();
                        break;

                    case ConsoleKey.End:
                        _inputCursor = _inputBuf.Length;
                        RefreshInput();
                        break;

                    default:
                        if (key.KeyChar >= 32 && key.KeyChar < 127)
                        {
                            _inputBuf = _inputBuf.Substring(0, _inputCursor) + key.KeyChar + _inputBuf.Substring(_inputCursor);
                            _inputCursor++;
                            RefreshInput();
                        }
                        break;
                }
                return false;
            }
        }

        private void RefreshInput()
        {
            _scrCX = Math.Max(0, Math.Min(_inputStartX, _scrW - 1));
            _scrCY = Math.Max(0, Math.Min(_inputStartY, _scrH - 1));
            ScrWriteString(_inputBuf);
            int clearCount = Math.Max(0, _inputVisibleLen - _inputBuf.Length);
            for (int i = 0; i < clearCount; i++)
                ScrWriteChar(' ');
            _inputVisibleLen = _inputBuf.Length;

            _scrCX = Math.Max(0, Math.Min(_inputStartX, _scrW - 1));
            _scrCY = Math.Max(0, Math.Min(_inputStartY, _scrH - 1));
            if (_inputCursor > 0)
            {
                string before = _inputBuf.Substring(0, _inputCursor);
                ScrWriteString(before);
            }
        }

        public void Write(string s)
        {
            if (s == null) return;
            lock (_lock)
            {
                _ops.Enqueue(new Op { Type = OpType.Write, Text = s });
                if (_hasScreen) ScrWriteString(s);
            }
        }

        public void WriteLine(string s) { Write(s + "\n"); }
        public void WriteLine() { Write("\n"); }

        public void Clear()
        {
            lock (_lock)
            {
                _ops.Enqueue(new Op { Type = OpType.Clear });
                if (_hasScreen) ScrClear();
            }
        }

        public void SetCursorPosition(int x, int y)
        {
            lock (_lock)
            {
                _ops.Enqueue(new Op { Type = OpType.SetCursor, X = x, Y = y });
                if (_hasScreen)
                {
                    _scrCX = Math.Max(0, Math.Min(x, _scrW - 1));
                    _scrCY = Math.Max(0, Math.Min(y, _scrH - 1));
                }
            }
        }

        public void SetForeground(ConsoleColor c)
        {
            lock (_lock)
            {
                _ops.Enqueue(new Op { Type = OpType.SetFg, Color = c });
                if (_hasScreen && (int)c < CcToColor.Length)
                    _scrFgCur = CcToColor[(int)c];
            }
        }

        public void SetBackground(ConsoleColor c)
        {
            lock (_lock)
            {
                _ops.Enqueue(new Op { Type = OpType.SetBg, Color = c });
                if (_hasScreen && (int)c < CcToColor.Length)
                    _scrBgCur = CcToColor[(int)c];
            }
        }

        public void ResetColor()
        {
            lock (_lock)
            {
                _ops.Enqueue(new Op { Type = OpType.ResetColor });
                _scrFgCur = Color.White;
                _scrBgCur = Color.Black;
            }
        }

        public int WindowWidth
        {
            get
            {
                if (_overrideWidth > 0) return _overrideWidth;
                try { return Console.WindowWidth; }
                catch { return 80; }
            }
        }

        public int WindowHeight
        {
            get
            {
                if (_overrideHeight > 0) return _overrideHeight;
                try { return Console.WindowHeight; }
                catch { return 25; }
            }
        }

        private int _overrideWidth;
        private int _overrideHeight;

        public void SetSize(int width, int height)
        {
            _overrideWidth = width;
            _overrideHeight = height;
        }

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            while (true)
            {
                lock (_lock)
                {
                    if (_input.Count > 0)
                        return _input.Dequeue();
                }
                Thread.Sleep(10);
            }
        }

        public bool KeyAvailable
        {
            get { lock (_lock) return _input.Count > 0; }
        }

        public string ReadLine()
        {
            string line = "";
            while (true)
            {
                ConsoleKeyInfo key = ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    WriteLine("");
                    return line;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (line.Length > 0)
                        line = line.Substring(0, line.Length - 1);
                }
                else if (key.KeyChar >= 32)
                {
                    line += key.KeyChar;
                }
            }
        }

        public void EnqueueKey(ConsoleKeyInfo key)
        {
            lock (_lock)
            {
                _input.Enqueue(key);
            }
        }

        public void RedirectConsole()
        {
            _originalOut = System.Console.Out;
            System.Console.SetOut(new VCWriter(this));
        }

        public void RestoreConsole()
        {
            if (_originalOut != null)
                System.Console.SetOut(_originalOut);
        }

        public void Flush()
        {
            lock (_lock)
            {
                while (_ops.Count > 0)
                {
                    Op op = _ops.Dequeue();
                    switch (op.Type)
                    {
                        case OpType.Write:
                            _originalOut.Write(op.Text);
                            break;
                        case OpType.Clear:
                            System.Console.Clear();
                            break;
                        case OpType.SetCursor:
                            System.Console.SetCursorPosition(op.X, op.Y);
                            break;
                        case OpType.SetFg:
                            System.Console.ForegroundColor = op.Color;
                            break;
                        case OpType.SetBg:
                            System.Console.BackgroundColor = op.Color;
                            break;
                        case OpType.ResetColor:
                            System.Console.ResetColor();
                            break;
                    }
                }
            }
        }

        public bool HasPendingOps
        {
            get { lock (_lock) return _ops.Count > 0; }
        }

        public List<Op> DrainOps()
        {
            var result = new List<Op>();
            lock (_lock)
            {
                while (_ops.Count > 0)
                    result.Add(_ops.Dequeue());
            }
            return result;
        }

        private class VCWriter : TextWriter
        {
            private readonly VirtualConsole _vc;
            public VCWriter(VirtualConsole vc) { _vc = vc; }
            public override Encoding Encoding => Encoding.UTF8;
            public override void Write(char value) { _vc.Write(value.ToString()); }
            public override void Write(string value) { _vc.Write(value); }
            public override void WriteLine(string value) { _vc.WriteLine(value); }
            public override void WriteLine() { _vc.WriteLine(); }
            public override void Flush() { }
        }
    }
}
