using Cosmos.System;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shinx.GUI
{
    public class Terminal
    {
        private static List<string> _lines = new List<string>();
        private static string _input = "";
        private static int _cursorPos = 0;
        private static int _historyIndex = -1;
        private static List<string> _history = new List<string>();

        private const int CharW = 8;
        private const int CharH = 16;
        private const int Padding = 4;
        private const int TitleBarH = 20;
        private const int Width = 500;
        private const int Height = 300;

        private static Color BgColor = Color.Black;
        private static Color TextColor = Color.LightGreen;
        private static Color InputColor = Color.White;
        private static Color CursorColor = Color.White;

        private static int MaxLines => (Height - TitleBarH - Padding * 2) / CharH - 1;
        private static int MaxCols => (Width - Padding * 2) / CharW;

        static Terminal()
        {
            PrintLine("SHINX Terminal");
            PrintLine("type 'exit' to close");
            PrintLine("");
        }

        public static void PrintLine(string line)
        {
            while (line.Length > MaxCols)
            {
                _lines.Add(line.Substring(0, MaxCols));
                line = line.Substring(MaxCols);
            }
            _lines.Add(line);
        }

        public static void Draw(Canvas canvas)
        {
            if (!Booleans.terminal_opened) return;

            Window.Draw(canvas,
                ref Int_Manager.terminal_x,
                ref Int_Manager.terminal_y,
                Width, Height,
                "Terminal",
                ref Booleans.terminal_opened);

            if (!Booleans.terminal_opened) return;

            int x = Int_Manager.terminal_x;
            int y = Int_Manager.terminal_y;

            canvas.DrawFilledRectangle(BgColor, x + 1, y + TitleBarH, Width - 2, Height - TitleBarH - 1);

            int contentY = y + TitleBarH + Padding;
            int startLine = Math.Max(0, _lines.Count - MaxLines);

            for (int i = startLine; i < _lines.Count; i++)
            {
                string line = _lines[i];
                if (line.Length > MaxCols)
                    line = line.Substring(0, MaxCols);
                ASC16.DrawACSIIString(canvas, line, TextColor,
                    (uint)(x + Padding),
                    (uint)(contentY + (i - startLine) * CharH));
            }

            int inputY = y + Height - CharH - Padding;
            string prompt = UserManager.currentUser + "@" + Shell.currentDirectory + "> ";
            string inputLine = prompt + _input;

            if (inputLine.Length > MaxCols)
                inputLine = inputLine.Substring(inputLine.Length - MaxCols);

            ASC16.DrawACSIIString(canvas, inputLine, InputColor,
                (uint)(x + Padding), (uint)inputY);

            int cursorX = x + Padding + (prompt.Length + _cursorPos) * CharW;
            canvas.DrawFilledRectangle(CursorColor, cursorX, inputY + CharH - 2, CharW, 2);
        }

        public static void HandleKey(ConsoleKeyInfo key)
        {
            if (!Booleans.terminal_opened) return;

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    string cmd = _input.Trim();
                    PrintLine(UserManager.currentUser + "@" + Shell.currentDirectory + "> " + cmd);

                    if (cmd == "exit" || cmd == "quit")
                    {
                        Booleans.terminal_opened = false;
                        _input = "";
                        _cursorPos = 0;
                        return;
                    }

                    if (!string.IsNullOrEmpty(cmd))
                    {
                        if (_history.Count == 0 || _history[_history.Count - 1] != cmd)
                            _history.Add(cmd);
                        _historyIndex = -1;

                        var oldOut = System.Console.Out;
                        var writer = new TerminalWriter();
                        System.Console.SetOut(writer);
                        try
                        {
                            Kernel.commandHandler.Execute(cmd);
                        }
                        catch (Exception e)
                        {
                            PrintLine("error: " + e.Message);
                        }
                        finally
                        {
                            System.Console.SetOut(oldOut);
                        }
                    }

                    _input = "";
                    _cursorPos = 0;
                    break;

                case ConsoleKey.Backspace:
                    if (_cursorPos > 0)
                    {
                        _input = _input.Substring(0, _cursorPos - 1) + _input.Substring(_cursorPos);
                        _cursorPos--;
                    }
                    break;

                case ConsoleKey.Delete:
                    if (_cursorPos < _input.Length)
                        _input = _input.Substring(0, _cursorPos) + _input.Substring(_cursorPos + 1);
                    break;

                case ConsoleKey.LeftArrow:
                    if (_cursorPos > 0) _cursorPos--;
                    break;

                case ConsoleKey.RightArrow:
                    if (_cursorPos < _input.Length) _cursorPos++;
                    break;

                case ConsoleKey.UpArrow:
                    if (_history.Count == 0) break;
                    if (_historyIndex == -1) _historyIndex = _history.Count - 1;
                    else if (_historyIndex > 0) _historyIndex--;
                    _input = _history[_historyIndex];
                    _cursorPos = _input.Length;
                    break;

                case ConsoleKey.DownArrow:
                    if (_historyIndex == -1) break;
                    if (_historyIndex < _history.Count - 1)
                    {
                        _historyIndex++;
                        _input = _history[_historyIndex];
                    }
                    else
                    {
                        _historyIndex = -1;
                        _input = "";
                    }
                    _cursorPos = _input.Length;
                    break;

                case ConsoleKey.Home:
                    _cursorPos = 0;
                    break;

                case ConsoleKey.End:
                    _cursorPos = _input.Length;
                    break;

                default:
                    if (key.KeyChar >= 32 && key.KeyChar < 127)
                    {
                        _input = _input.Substring(0, _cursorPos) + key.KeyChar + _input.Substring(_cursorPos);
                        _cursorPos++;
                    }
                    break;
            }
        }
    }

    public class TerminalWriter : System.IO.TextWriter
    {
        private string _buffer = "";

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void Write(char value)
        {
            if (value == '\n')
            {
                Terminal.PrintLine(_buffer);
                _buffer = "";
            }
            else if (value != '\r')
            {
                _buffer += value;
            }
        }

        public override void WriteLine(string value)
        {
            Terminal.PrintLine(_buffer + (value ?? ""));
            _buffer = "";
        }

        public override void Flush()
        {
            if (_buffer.Length > 0)
            {
                Terminal.PrintLine(_buffer);
                _buffer = "";
            }
        }
    }
}