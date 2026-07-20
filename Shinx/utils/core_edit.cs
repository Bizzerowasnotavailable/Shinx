using Shinx.GUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx.Commands
{
    public class core_edit : ICommand
    {
        private List<string> lines = new List<string>();
        private int currentLine = 0;
        private int currentCol = 0;
        private int topLine = 0;
        private int leftChar = 0;

        private string path = "";
        private bool running = true;
        private string statusMessage = "";
        private bool _needsFullRedraw = true;

        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: edit <file>");
                return;
            }

            path = args[0].StartsWith("/") ? args[0] : Shell.currentDirectory.TrimEnd('/') + "/" + args[0];

            if (DesktopManager.Running)
            {
                TextEditor.Open(path);
                return;
            }

            var vc = VirtualConsole.Current;

            lines.Clear();
            if (File.Exists(path))
            {
                lines.AddRange(File.ReadAllLines(path));
            }
            if (lines.Count == 0) lines.Add("");

            currentLine = 0;
            currentCol = 0;
            topLine = 0;
            leftChar = 0;
            running = true;
            _needsFullRedraw = true;
            statusMessage = $"Editing {Path.GetFileName(path)}";

            while (running)
            {
                Render(vc);
                HandleInput(vc);
            }

            vc.Clear();
        }

        private void Render(VirtualConsole vc)
        {
            int screenWidth = vc.WindowWidth;
            int screenHeight = vc.WindowHeight;
            int viewHeight = screenHeight - 4;
            int viewWidth = screenWidth - 4;

            int oldTop = topLine;
            int oldLeft = leftChar;

            if (currentLine < topLine) topLine = currentLine;
            if (currentLine >= topLine + viewHeight) topLine = currentLine - viewHeight + 1;
            if (currentCol < leftChar) leftChar = currentCol;
            if (currentCol >= leftChar + viewWidth) leftChar = currentCol - viewWidth + 1;

            if (oldTop != topLine || oldLeft != leftChar) _needsFullRedraw = true;

            if (_needsFullRedraw)
            {
                vc.Clear();
                vc.SetCursorPosition(0, 0);
                vc.SetBackground(ConsoleColor.Gray);
                vc.SetForeground(ConsoleColor.Black);
                vc.Write($" SHINX EDIT | L: {currentLine + 1} C: {currentCol + 1} | {path}".PadRight(screenWidth - 1));
                vc.ResetColor();

                for (int i = 0; i < viewHeight; i++)
                {
                    int lineIndex = topLine + i;
                    vc.SetCursorPosition(0, i + 1);
                    DrawLine(vc, lineIndex, viewWidth);
                }

                vc.SetCursorPosition(0, screenHeight - 2);
                vc.SetForeground(ConsoleColor.Cyan);
                vc.Write(statusMessage.PadRight(screenWidth - 1));
                vc.SetCursorPosition(0, screenHeight - 1);
                vc.SetBackground(ConsoleColor.White);
                vc.SetForeground(ConsoleColor.Black);
                vc.Write(" ^O Save    ^X Exit    ^L GoTo Line ".PadRight(screenWidth - 1));
                vc.ResetColor();

                _needsFullRedraw = false;
            }
            else
            {
                vc.SetCursorPosition(0, 0);
                vc.SetBackground(ConsoleColor.Gray);
                vc.SetForeground(ConsoleColor.Black);
                vc.Write($" SHINX EDIT | L: {currentLine + 1} C: {currentCol + 1} | {path}".PadRight(screenWidth - 1));
                vc.ResetColor();

                vc.SetCursorPosition(0, (currentLine - topLine) + 1);
                DrawLine(vc, currentLine, viewWidth);
            }

            int cursorX = (currentCol - leftChar) + 2;
            int cursorY = (currentLine - topLine) + 1;
            vc.SetCursorPosition(
                Math.Clamp(cursorX, 0, screenWidth - 1),
                Math.Clamp(cursorY, 0, screenHeight - 1));
        }

        private void DrawLine(VirtualConsole vc, int lineIndex, int viewWidth)
        {
            if (lineIndex < lines.Count)
            {
                string fullLine = lines[lineIndex];
                string visibleText = (fullLine.Length > leftChar)
                    ? fullLine.Substring(leftChar, Math.Min(fullLine.Length - leftChar, viewWidth))
                    : "";

                if (lineIndex == currentLine)
                {
                    vc.SetBackground(ConsoleColor.DarkBlue);
                    vc.Write("> " + visibleText.PadRight(viewWidth));
                    vc.ResetColor();
                }
                else
                {
                    vc.Write("  " + visibleText.PadRight(viewWidth));
                }
            }
            else
            {
                vc.SetForeground(ConsoleColor.DarkGray);
                vc.Write("~".PadRight(viewWidth + 2));
                vc.ResetColor();
            }
        }

        private void HandleInput(VirtualConsole vc)
        {
            ConsoleKeyInfo key = vc.ReadKey(true);

            if ((key.Modifiers & ConsoleModifiers.Control) != 0)
            {
                switch (key.Key)
                {
                    case ConsoleKey.O: SaveFile(); _needsFullRedraw = true; break;
                    case ConsoleKey.X: running = false; break;
                    case ConsoleKey.L: ShowGoToLinePrompt(vc); _needsFullRedraw = true; break;
                }
                return;
            }

            string lineText = lines[currentLine];
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.DownArrow:
                    if (key.Key == ConsoleKey.UpArrow && currentLine > 0) currentLine--;
                    else if (key.Key == ConsoleKey.DownArrow && currentLine < lines.Count - 1) currentLine++;
                    if (currentCol > lines[currentLine].Length) currentCol = lines[currentLine].Length;
                    _needsFullRedraw = true;
                    break;

                case ConsoleKey.LeftArrow:
                    if (currentCol > 0) currentCol--;
                    break;

                case ConsoleKey.RightArrow:
                    if (currentCol < lineText.Length) currentCol++;
                    break;

                case ConsoleKey.Enter:
                    string rem = lineText.Substring(currentCol);
                    lines[currentLine] = lineText.Substring(0, currentCol);
                    lines.Insert(currentLine + 1, rem);
                    currentLine++;
                    currentCol = 0;
                    _needsFullRedraw = true;
                    break;

                case ConsoleKey.Backspace:
                    if (currentCol > 0)
                    {
                        lines[currentLine] = lineText.Remove(currentCol - 1, 1);
                        currentCol--;
                    }
                    else if (currentLine > 0)
                    {
                        int prevLen = lines[currentLine - 1].Length;
                        lines[currentLine - 1] += lines[currentLine];
                        lines.RemoveAt(currentLine);
                        currentLine--;
                        currentCol = prevLen;
                        _needsFullRedraw = true;
                    }
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        lines[currentLine] = lineText.Insert(currentCol, key.KeyChar.ToString());
                        currentCol++;
                    }
                    break;
            }
        }

        private void SaveFile()
        {
            try
            {
                File.WriteAllLines(path, lines.ToArray());
                statusMessage = "FILE SAVED";
            }
            catch (Exception e)
            {
                statusMessage = "SAVE ERROR: " + e.Message;
            }
        }

        private void ShowGoToLinePrompt(VirtualConsole vc)
        {
            statusMessage = "Line: ";
            Render(vc);
            string input = vc.ReadLine();
            if (int.TryParse(input, out int l) && l > 0 && l <= lines.Count)
            {
                currentLine = l - 1;
                currentCol = 0;
            }
        }
    }
}
