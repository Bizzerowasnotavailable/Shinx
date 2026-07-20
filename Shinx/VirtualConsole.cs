using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Shinx
{
    public class VirtualConsole
    {
        public static VirtualConsole Current;

        private enum OpType { Write, Clear, SetCursor, SetFg, SetBg, ResetColor }

        private struct Op
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

        public void Write(string s)
        {
            if (s == null) return;
            lock (_lock)
                _ops.Enqueue(new Op { Type = OpType.Write, Text = s });
        }

        public void WriteLine(string s) { Write(s + "\n"); }
        public void WriteLine() { Write("\n"); }

        public void Clear()
        {
            lock (_lock)
                _ops.Enqueue(new Op { Type = OpType.Clear });
        }

        public void SetCursorPosition(int x, int y)
        {
            lock (_lock)
                _ops.Enqueue(new Op { Type = OpType.SetCursor, X = x, Y = y });
        }

        public void SetForeground(ConsoleColor c)
        {
            lock (_lock)
                _ops.Enqueue(new Op { Type = OpType.SetFg, Color = c });
        }

        public void SetBackground(ConsoleColor c)
        {
            lock (_lock)
                _ops.Enqueue(new Op { Type = OpType.SetBg, Color = c });
        }

        public void ResetColor()
        {
            lock (_lock)
                _ops.Enqueue(new Op { Type = OpType.ResetColor });
        }

        public int WindowWidth
        {
            get
            {
                try { return Console.WindowWidth; }
                catch { return 80; }
            }
        }

        public int WindowHeight
        {
            get
            {
                try { return Console.WindowHeight; }
                catch { return 25; }
            }
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
