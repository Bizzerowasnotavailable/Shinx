using System;
using System.Collections.Generic;

namespace Shinx
{
    public enum LineAction
    {
        None,
        Submit,
        Cancel,
        ClearScreen
    }

    public sealed class LineEditor
    {
        private readonly List<string> _history;

        private string _text = "";
        private int _cursor = 0;
        private int _historyIndex = 0;
        private bool _browsing = false;

        public LineEditor(List<string> history)
        {
            _history = history;
            _historyIndex = history.Count;
        }

        public string Text => _text;
        public int Cursor => _cursor;

        public LineAction Handle(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    return LineAction.Submit;

                case ConsoleKey.Backspace:
                    if (_cursor > 0)
                    {
                        _text = _text.Substring(0, _cursor - 1) + _text.Substring(_cursor);
                        _cursor--;
                    }
                    return LineAction.None;

                case ConsoleKey.Delete:
                    if (_cursor < _text.Length)
                        _text = _text.Substring(0, _cursor) + _text.Substring(_cursor + 1);
                    return LineAction.None;

                case ConsoleKey.LeftArrow:
                    if (_cursor > 0) _cursor--;
                    return LineAction.None;

                case ConsoleKey.RightArrow:
                    if (_cursor < _text.Length) _cursor++;
                    return LineAction.None;

                case ConsoleKey.Home:
                    _cursor = 0;
                    return LineAction.None;

                case ConsoleKey.End:
                    _cursor = _text.Length;
                    return LineAction.None;

                case ConsoleKey.UpArrow:
                    HistoryUp();
                    return LineAction.None;

                case ConsoleKey.DownArrow:
                    HistoryDown();
                    return LineAction.None;

                case ConsoleKey.Tab:
                    return LineAction.None;

                case ConsoleKey.L when key.Modifiers == ConsoleModifiers.Control:
                    return LineAction.ClearScreen;

                case ConsoleKey.C when key.Modifiers == ConsoleModifiers.Control:
                    Reset();
                    return LineAction.Cancel;

                default:
                    if (key.KeyChar >= 32 && key.KeyChar <= 126)
                    {
                        Insert(key.KeyChar);
                        _browsing = false;
                    }
                    return LineAction.None;
            }
        }

        public void Reset()
        {
            _text = "";
            _cursor = 0;
            _historyIndex = _history.Count;
            _browsing = false;
        }

        private void Insert(char c)
        {
            _text = _text.Substring(0, _cursor) + c + _text.Substring(_cursor);
            _cursor++;
        }

        private void HistoryUp()
        {
            if (_history.Count == 0) return;

            if (!_browsing)
            {
                _browsing = true;
                _historyIndex = _history.Count - 1;
            }
            else if (_historyIndex > 0)
            {
                _historyIndex--;
            }

            SetText(_history[_historyIndex]);
        }

        private void HistoryDown()
        {
            if (!_browsing) return;

            if (_historyIndex < _history.Count - 1)
            {
                _historyIndex++;
                SetText(_history[_historyIndex]);
            }
            else
            {
                _browsing = false;
                SetText("");
            }
        }

        private void SetText(string text)
        {
            _text = text;
            _cursor = text.Length;
        }
    }
}
