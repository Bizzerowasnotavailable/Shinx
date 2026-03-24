using System;
using System.IO;
using System.Collections.Generic;

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

            path = args[0].StartsWith(@"0:\") ? args[0] : Shell.currentDirectory + args[0];

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
                Render();
                HandleInput();
            }

            Console.Clear();
        }

        private void Render()
        {
            int screenWidth = Console.WindowWidth;
            int screenHeight = Console.WindowHeight;
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
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($" SHINX EDIT | L: {currentLine + 1} C: {currentCol + 1} | {path}".PadRight(screenWidth - 1));
                Console.ResetColor();

                for (int i = 0; i < viewHeight; i++)
                {
                    int lineIndex = topLine + i;
                    Console.SetCursorPosition(0, i + 1);
                    DrawLine(lineIndex, viewWidth);
                }

                Console.SetCursorPosition(0, screenHeight - 2);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(statusMessage.PadRight(screenWidth - 1));
                Console.SetCursorPosition(0, screenHeight - 1);
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(" ^O Save    ^X Exit    ^L GoTo Line ".PadRight(screenWidth - 1));
                Console.ResetColor();

                _needsFullRedraw = false;
            }
            else
            {
                Console.SetCursorPosition(0, 0);
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($" SHINX EDIT | L: {currentLine + 1} C: {currentCol + 1} | {path}".PadRight(screenWidth - 1));
                Console.ResetColor();

                Console.SetCursorPosition(0, (currentLine - topLine) + 1);
                DrawLine(currentLine, viewWidth);
            }

            int cursorX = (currentCol - leftChar) + 2;
            int cursorY = (currentLine - topLine) + 1;
            Console.SetCursorPosition(Math.Clamp(cursorX, 0, screenWidth - 1), Math.Clamp(cursorY, 0, screenHeight - 1));
        }

        private void DrawLine(int lineIndex, int viewWidth)
        {
            if (lineIndex < lines.Count)
            {
                string fullLine = lines[lineIndex];
                string visibleText = (fullLine.Length > leftChar)
                    ? fullLine.Substring(leftChar, Math.Min(fullLine.Length - leftChar, viewWidth))
                    : "";

                if (lineIndex == currentLine)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write("> " + visibleText.PadRight(viewWidth));
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("  " + visibleText.PadRight(viewWidth));
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("~".PadRight(viewWidth + 2));
                Console.ResetColor();
            }
        }

        private void HandleInput()
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            if ((key.Modifiers & ConsoleModifiers.Control) != 0)
            {
                switch (key.Key)
                {
                    case ConsoleKey.O: SaveFile(); _needsFullRedraw = true; break;
                    case ConsoleKey.X: running = false; break;
                    case ConsoleKey.L: ShowGoToLinePrompt(); _needsFullRedraw = true; break;
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

        private void ShowGoToLinePrompt()
        {
            statusMessage = "Line: ";
            Render();
            string input = Console.ReadLine();
            if (int.TryParse(input, out int l) && l > 0 && l <= lines.Count)
            {
                currentLine = l - 1;
                currentCol = 0;
            }
        }
    }
}