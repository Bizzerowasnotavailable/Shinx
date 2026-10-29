using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Sessions;
using Cosmos.Kernel.System.Timers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace Shinx.GUI
{
    public class Terminal : IGuiApp
    {
        public string AppID => "Terminal";
        public string DisplayName => "Terminal";
        public bool IsVisible { get => Booleans.terminal_opened; set => Booleans.terminal_opened = value; }

        private const int Width = 640;
        private const int Height = 400;
        private const int TitleBarH = 20;
        private const int PadX = 6;
        private const int PadY = 4;
        private const int CharW = 8;
        private const int CharH = 16;

        private static readonly Color BgColor = Color.Black;
        private static readonly Color FgColor = Color.White;

        private static readonly object _gate = new object();
        private static readonly List<string> _buffer = new List<string>();
        private static readonly Queue<string> _pending = new Queue<string>();
        private static readonly LineEditor _editor = new LineEditor(Shell.history);
        private static readonly TerminalWriter _writer = new TerminalWriter();
        private static readonly TerminalInput _reader = new TerminalInput();

        private static ConsoleSession? _session;
        private static TextWriter? _previousOut;
        private static TextReader? _previousIn;
        private static volatile bool _started = false;
        private static bool _closed = false;

        private static long _blinkTicks = 0;
        private static long _lastBlink = 0;
        private static bool _cursorOn = true;

        private static int Cols => (Width - (PadX * 2)) / CharW;
        private static int Rows => (Height - TitleBarH - (PadY * 2)) / CharH;

        public void Draw(Canvas canvas)
        {
            if (!Booleans.terminal_opened) return;

            Window.Draw(canvas, ref Int_Manager.terminal_x, ref Int_Manager.terminal_y, Width, Height, DisplayName, ref Booleans.terminal_opened, AppID);

            if (!Booleans.terminal_opened) return;

            if (!_started) StartShell();

            int x = Int_Manager.terminal_x;
            int y = Int_Manager.terminal_y;

            canvas.DrawFilledRectangle(BgColor, x + 1, y + TitleBarH, Width - 2, Height - TitleBarH - 1);

            UpdateBlink();

            lock (_gate)
            {
                if (_buffer.Count == 0) _buffer.Add("");

                int rows = Rows;
                int first = _buffer.Count > rows ? _buffer.Count - rows : 0;
                int row = 0;
                for (int i = first; i < _buffer.Count; i++, row++)
                {
                    string line = _buffer[i];
                    if (line.Length > Cols) line = line.Substring(0, Cols);
                    ASC16.DrawACSIIString(canvas, line, FgColor,
                        (uint)(x + PadX),
                        (uint)(y + TitleBarH + PadY + (row * CharH)));
                }

                int lastRow = (_buffer.Count - 1) - first;
                int lastLen = _buffer[_buffer.Count - 1].Length;
                if (lastLen > Cols) lastLen = Cols;

                int inputY = y + TitleBarH + PadY + (lastRow * CharH);
                int room = Cols - lastLen;
                if (room < 1) room = 1;

                string input = _editor.Text;
                int start = 0;
                if (input.Length > room)
                {
                    if (_editor.Cursor >= room) start = _editor.Cursor - (room - 1);
                    if (start > input.Length - room) start = input.Length - room;
                    if (start < 0) start = 0;
                }

                if (input.Length > 0)
                {
                    string visible = start > 0 ? input.Substring(start) : input;
                    if (visible.Length > room) visible = visible.Substring(0, room);
                    ASC16.DrawACSIIString(canvas, visible, FgColor,
                        (uint)(x + PadX + (lastLen * CharW)), (uint)inputY);
                }

                if (_cursorOn)
                {
                    int caret = _editor.Cursor - start;
                    if (caret < 0) caret = 0;
                    if (caret > room - 1) caret = room - 1;
                    int caretX = x + PadX + ((lastLen + caret) * CharW);
                    canvas.DrawFilledRectangle(FgColor, caretX, inputY + 2, 2, CharH - 4);
                }
            }
        }

        private static void CalibrateBlink()
        {
            _blinkTicks = 0;
            _lastBlink = 0;
            _cursorOn = true;

            try
            {
                long start = Stopwatch.GetTimestamp();
                TimerManager.Wait(100);
                long elapsed = Stopwatch.GetTimestamp() - start;
                if (elapsed > 0) _blinkTicks = elapsed * 5;
            }
            catch
            {
                _blinkTicks = 0;
            }
        }

        private static void UpdateBlink()
        {
            if (_blinkTicks <= 0) return;

            long now = Stopwatch.GetTimestamp();
            if (_lastBlink == 0) _lastBlink = now;

            if (now - _lastBlink >= _blinkTicks)
            {
                _lastBlink = now;
                _cursorOn = !_cursorOn;
            }
        }

        public void HandleKey(ConsoleKeyInfo key)
        {
            if (!Booleans.terminal_opened) return;

            if (!_started) StartShell();

            _cursorOn = true;
            if (_blinkTicks > 0) _lastBlink = Stopwatch.GetTimestamp();

            LineAction action = _editor.Handle(key);

            switch (action)
            {
                case LineAction.Submit:
                    lock (_gate)
                    {
                        string typed = _editor.Text;
                        _editor.Reset();

                        Append(typed);
                        Append("\n");
                        TrimBuffer();
                        _pending.Enqueue(typed);
                    }
                    break;

                case LineAction.ClearScreen:
                    ClearBuffer();
                    AppendText(Shell.Prompt());
                    break;
            }
        }

        public static void ClearBuffer()
        {
            lock (_gate)
            {
                _buffer.Clear();
                _buffer.Add("");
            }
        }

        public static void CloseWindow()
        {
            AppendText("[session closed]\n");
            RestoreStreams();
            Booleans.terminal_opened = false;
            _session = null;
            _started = false;
            _closed = true;
            _editor.Reset();
        }

        public static void Shutdown()
        {
            lock (_gate)
            {
                _closed = true;
                _pending.Clear();
            }

            RestoreStreams();

            if (_session != null)
            {
                try { _session.Close(); } catch { }
                _session = null;
            }

            _started = false;
            _editor.Reset();
        }

        private static void RestoreStreams()
        {
            if (_previousOut != null)
            {
                try { Console.SetOut(_previousOut); } catch { }
                _previousOut = null;
            }
            if (_previousIn != null)
            {
                try { Console.SetIn(_previousIn); } catch { }
                _previousIn = null;
            }
        }

        private static void StartShell()
        {
            if (_started) return;
            _started = true;
            _closed = false;

            lock (_gate)
            {
                _buffer.Clear();
                _buffer.Add("");
                _pending.Clear();
            }
            _editor.Reset();
            CalibrateBlink();

            if (_previousOut == null)
            {
                try { _previousOut = Console.Out; } catch { }
            }
            try { Console.SetOut(_writer); } catch { }

            if (_previousIn == null)
            {
                try { _previousIn = Console.In; } catch { }
            }
            try { Console.SetIn(_reader); } catch { }

            try
            {
                _session = SessionManager.CreateVirtualConsole();
            }
            catch (Exception e)
            {
                AppendText("[no console session: " + e.Message + "]\n");
            }

            bool started = false;

            if (_session != null)
            {
                try
                {
                    SessionManager.Start(_session, ShellLoop);
                    started = true;
                }
                catch (Exception e)
                {
                    AppendText("[session start failed: " + e.Message + "]\n");
                }
            }

            if (!started)
            {
                try
                {
                    var thread = new System.Threading.Thread(ShellLoop);
                    thread.Start();
                    started = true;
                }
                catch (Exception e)
                {
                    AppendText("[shell start failed: " + e.Message + "]\n");
                }
            }
        }

        private static void ShellLoop()
        {
            try
            {
                while (true)
                {
                    Console.Write(Shell.Prompt());
                    string? line = ReadLineFromWindow();
                    if (line == null) break;

                    Shell.Execute(line);

                    if (_closed) break;
                }
            }
            catch (Exception e)
            {
                AppendText("\n[fatal: " + e.Message + "]\n");
            }
        }

        private static string? ReadLineFromWindow()
        {
            while (true)
            {
                lock (_gate)
                {
                    if (_pending.Count > 0) return _pending.Dequeue();
                    if (_closed) return null;
                }
                TimerManager.Wait(15);
            }
        }

        private static void AppendText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            lock (_gate)
            {
                Append(text);
                TrimBuffer();
            }
        }

        private static void Append(string text)
        {
            if (_buffer.Count == 0) _buffer.Add("");

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r') continue;

                if (c == '\n')
                {
                    _buffer.Add("");
                    continue;
                }

                if (c == '\b')
                {
                    string last = _buffer[_buffer.Count - 1];
                    if (last.Length > 0)
                        _buffer[_buffer.Count - 1] = last.Substring(0, last.Length - 1);
                    continue;
                }

                if (c == '\t')
                {
                    Append("    ");
                    continue;
                }

                string cur = _buffer[_buffer.Count - 1];
                if (cur.Length >= Cols)
                {
                    _buffer.Add(c.ToString());
                }
                else
                {
                    _buffer[_buffer.Count - 1] = cur + c;
                }
            }
        }

        private static void TrimBuffer()
        {
            while (_buffer.Count > 400) _buffer.RemoveAt(0);
        }

        private sealed class TerminalWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {
                lock (_gate)
                {
                    Append(value.ToString());
                    TrimBuffer();
                }
            }

            public override void Write(string? value)
            {
                if (value != null) AppendText(value);
            }

            public override void WriteLine(string? value)
            {
                AppendText((value ?? "") + "\n");
            }

            public override void WriteLine()
            {
                AppendText("\n");
            }

            public override void Flush() { }
        }

        private sealed class TerminalInput : TextReader
        {
            private string _rest = "";

            public override int Peek()
            {
                return _rest.Length > 0 ? _rest[0] : -1;
            }

            public override int Read()
            {
                if (_rest.Length == 0)
                {
                    string? line = ReadLineFromWindow();
                    if (line == null) return -1;
                    _rest = line + "\n";
                }

                char c = _rest[0];
                _rest = _rest.Substring(1);
                return c;
            }
        }
    }
}
