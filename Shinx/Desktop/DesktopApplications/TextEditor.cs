using Cosmos.Kernel.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Shinx.GUI
{
    public class TextEditor : IGuiApp
    {
        public string AppID => "Text Editor";
        public string DisplayName => "Text Editor";
        public bool IsVisible { get => Booleans.editor_opened; set => Booleans.editor_opened = value; }

        public static List<string> Lines = new List<string> { "" };
        public static string CurrentPath = "";

        private static int _cursorX = 0;
        private static int _cursorY = 0;
        private static int _topLine = 0;
        private static string _status = "Ready";
        private static int _statusTimer = 0;

        private static bool _isInputMode = false;
        private static string _inputBuffer = "";
        private static string _inputPrompt = "";
        private static string _inputPurpose = "";

        private const int CharW = 8;
        private const int CharH = 16;
        private const int TitleBarH = 20;
        private const int StatusBarH = 20;
        private const int Width = 600;
        private const int Height = 400;

        public static void Open(string path)
        {
            string fullPath = path.StartsWith("/") ? path : Shell.currentDirectory.TrimEnd('/') + "/" + path;

            if (File.Exists(fullPath))
            {
                CurrentPath = fullPath;
                Lines.Clear();
                foreach (var line in File.ReadAllLines(fullPath)) Lines.Add(line);
                _status = "Loaded: " + Path.GetFileName(fullPath);
            }
            else
            {
                CurrentPath = fullPath;
                Lines.Clear();
                Lines.Add("");
                _status = "New File: " + Path.GetFileName(fullPath);
            }
            _cursorX = 0;
            _cursorY = 0;
            _statusTimer = 100;
            Booleans.editor_opened = true;
        }

        public void Draw(Canvas canvas)
        {
            if (!Booleans.editor_opened) return;

            string fileName = string.IsNullOrEmpty(CurrentPath) ? "Untitled" : Path.GetFileName(CurrentPath);
            string dynamicTitle = "Editor - " + fileName;

            Window.Draw(canvas, ref Int_Manager.editor_x, ref Int_Manager.editor_y, Width, Height, dynamicTitle, ref Booleans.editor_opened, AppID);

            if (!Booleans.editor_opened) return;

            int x = Int_Manager.editor_x;
            int y = Int_Manager.editor_y;

            canvas.DrawFilledRectangle(Color.White, x + 1, y + TitleBarH, Width - 2, Height - TitleBarH - StatusBarH - 1);
            canvas.DrawFilledRectangle(Color.LightGray, x + 1, y + Height - StatusBarH, Width - 2, StatusBarH);

            string displayStatus = _isInputMode ? _inputPrompt + _inputBuffer + "_" : (_statusTimer > 0 ? _status : $"L: {_cursorY + 1} C: {_cursorX + 1} | Ctrl+S: Save | Ctrl+L: Load | Ctrl+G: GoTo");

            ASC16.DrawACSIIString(canvas, displayStatus, Color.Black, (uint)(x + 5), (uint)(y + Height - StatusBarH + 2));
            if (_statusTimer > 0) _statusTimer--;

            int visibleLines = (Height - TitleBarH - StatusBarH - 10) / CharH;
            if (_cursorY < _topLine) _topLine = _cursorY;
            if (_cursorY >= _topLine + visibleLines) _topLine = _cursorY - visibleLines + 1;

            for (int i = 0; i < visibleLines; i++)
            {
                int lineIdx = _topLine + i;
                if (lineIdx >= Lines.Count) break;
                ASC16.DrawACSIIString(canvas, Lines[lineIdx], Color.Black, (uint)(x + 5), (uint)(y + TitleBarH + 5 + (i * CharH)));
            }

            if (!_isInputMode)
            {
                int cX = x + 5 + (_cursorX * CharW);
                int cY = y + TitleBarH + 5 + ((_cursorY - _topLine) * CharH);
                if (cX < x + Width - 10)
                    canvas.DrawFilledRectangle(Color.Blue, cX, cY, 2, CharH);
            }
        }

        public void HandleKey(ConsoleKeyInfo key)
        {
            if (!Booleans.editor_opened) return;

            if (_isInputMode)
            {
                if (key.Key == ConsoleKey.Enter)
                {
                    if (_inputPurpose == "GOTO")
                    {
                        if (int.TryParse(_inputBuffer, out int targetLine))
                        {
                            _cursorY = Math.Clamp(targetLine - 1, 0, Lines.Count - 1);
                            _cursorX = 0;
                            _status = "Jumped to line " + targetLine;
                        }
                    }
                    else if (_inputPurpose == "LOAD")
                    {
                        Open(_inputBuffer);
                    }
                    else if (_inputPurpose == "SAVEAS")
                    {
                        string savePath = _inputBuffer.StartsWith("/") ? _inputBuffer : Shell.currentDirectory.TrimEnd('/') + "/" + _inputBuffer;
                        CurrentPath = savePath;
                        Save();
                    }

                    _isInputMode = false;
                    _statusTimer = 100;
                    return;
                }
                if (key.Key == ConsoleKey.Escape) { _isInputMode = false; return; }
                if (key.Key == ConsoleKey.Backspace && _inputBuffer.Length > 0)
                {
                    _inputBuffer = _inputBuffer.Remove(_inputBuffer.Length - 1);
                    return;
                }

                if (key.KeyChar >= 32 && key.KeyChar <= 126) { _inputBuffer += key.KeyChar; }
                return;
            }

            if (key.Modifiers == ConsoleModifiers.Control)
            {
                if (key.Key == ConsoleKey.S) { Save(); return; }
                if (key.Key == ConsoleKey.G)
                {
                    _isInputMode = true;
                    _inputPurpose = "GOTO";
                    _inputPrompt = "Go To Line: ";
                    _inputBuffer = "";
                    return;
                }
                if (key.Key == ConsoleKey.L)
                {
                    _isInputMode = true;
                    _inputPurpose = "LOAD";
                    _inputPrompt = "Open Path: ";
                    _inputBuffer = Shell.currentDirectory;
                    return;
                }
            }
            HandleStandardInput(key);
        }

        private void HandleStandardInput(ConsoleKeyInfo key)
        {
            string curLine = Lines[_cursorY];
            switch (key.Key)
            {
                case ConsoleKey.UpArrow: if (_cursorY > 0) { _cursorY--; _cursorX = Math.Min(_cursorX, Lines[_cursorY].Length); } break;
                case ConsoleKey.DownArrow: if (_cursorY < Lines.Count - 1) { _cursorY++; _cursorX = Math.Min(_cursorX, Lines[_cursorY].Length); } break;
                case ConsoleKey.LeftArrow: if (_cursorX > 0) _cursorX--; break;
                case ConsoleKey.RightArrow: if (_cursorX < curLine.Length) _cursorX++; break;
                case ConsoleKey.Enter:
                    Lines[_cursorY] = curLine.Substring(0, _cursorX);
                    Lines.Insert(_cursorY + 1, curLine.Substring(_cursorX));
                    _cursorY++; _cursorX = 0; break;
                case ConsoleKey.Backspace:
                    if (_cursorX > 0) { Lines[_cursorY] = curLine.Remove(_cursorX - 1, 1); _cursorX--; }
                    else if (_cursorY > 0) { _cursorX = Lines[_cursorY - 1].Length; Lines[_cursorY - 1] += curLine; Lines.RemoveAt(_cursorY); _cursorY--; }
                    break;
                default:
                    if (key.KeyChar >= 32 && key.KeyChar <= 126) { Lines[_cursorY] = curLine.Insert(_cursorX, key.KeyChar.ToString()); _cursorX++; }
                    break;
            }
        }

        private static void Save()
        {
            if (string.IsNullOrEmpty(CurrentPath))
            {
                _isInputMode = true;
                _inputPurpose = "SAVEAS";
                _inputPrompt = "Save As: ";
                _inputBuffer = Shell.currentDirectory;
                _status = "Enter path to save";
                _statusTimer = 150;
                return;
            }
            try
            {
                File.WriteAllLines(CurrentPath, Lines.ToArray());
                _status = "SAVED: " + Path.GetFileName(CurrentPath);
            }
            catch (Exception e)
            {
                _status = "SAVE ERROR: " + e.Message;
            }
            _statusTimer = 150;
        }
    }
}