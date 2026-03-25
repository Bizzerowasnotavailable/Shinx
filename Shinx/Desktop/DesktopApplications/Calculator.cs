using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;

namespace Shinx.GUI
{
    public static class Calculator
    {
        public static string calcInput = "0";
        public static int storedNumber = 0;
        public static string currentOperation = "";
        public static bool wasCalcClickedLastFrame = false;

        public static void Draw(VBECanvas vbe)
        {
            if (Booleans.calc_opened)
            {
                Window.Draw(vbe, ref Int_Manager.calc_x, ref Int_Manager.calc_y, 220, 280, "Calculator", ref Booleans.calc_opened);

                if (Booleans.calc_opened)
                {
                    int x = Int_Manager.calc_x;
                    int y = Int_Manager.calc_y;

                    vbe.DrawFilledRectangle(new Pen(Color.White), x + 10, y + 30, 200, 40);
                    vbe.DrawRectangle(new Pen(Color.DarkGray), x + 10, y + 30, 200, 40);

                    uint textX = (uint)(x + 200 - (calcInput.Length * 8));
                    ASC16.DrawACSIIString(vbe, calcInput, Color.Black, textX, (uint)y + 42);

                    string[] buttons = { "7", "8", "9", "/", "4", "5", "6", "*", "1", "2", "3", "-", "C", "0", "=", "+" };

                    bool isClickedNow = Mouse.Click();
                    int mouseX = (int)MouseManager.X;
                    int mouseY = (int)MouseManager.Y;

                    int btnIndex = 0;
                    for (int row = 0; row < 4; row++)
                    {
                        for (int col = 0; col < 4; col++)
                        {
                            int btnX = x + 10 + (col * 50);
                            int btnY = y + 80 + (row * 45);
                            string btnText = buttons[btnIndex];

                            vbe.DrawFilledRectangle(new Pen(Color.LightGray), btnX, btnY, 45, 40);
                            vbe.DrawRectangle(new Pen(Color.Gray), btnX, btnY, 45, 40);

                            uint btnTextX = (uint)(btnX + 22 - (btnText.Length * 4));
                            ASC16.DrawACSIIString(vbe, btnText, Color.Black, btnTextX, (uint)(btnY + 12));

                            if (isClickedNow && !wasCalcClickedLastFrame)
                            {
                                if (mouseX >= btnX && mouseX <= btnX + 45 && mouseY >= btnY && mouseY <= btnY + 40)
                                {
                                    HandleCalcClick(btnText);
                                }
                            }
                            btnIndex++;
                        }
                    }
                    wasCalcClickedLastFrame = isClickedNow;
                }
            }
        }

        private static void HandleCalcClick(string btn)
        {
            if (btn == "C")
            {
                calcInput = "0"; storedNumber = 0; currentOperation = "";
            }
            else if (btn == "+" || btn == "-" || btn == "*" || btn == "/")
            {
                int.TryParse(calcInput, out storedNumber);
                currentOperation = btn; calcInput = "0";
            }
            else if (btn == "=")
            {
                int currentNumber; int.TryParse(calcInput, out currentNumber); int result = 0;
                if (currentOperation == "+") result = storedNumber + currentNumber;
                if (currentOperation == "-") result = storedNumber - currentNumber;
                if (currentOperation == "*") result = storedNumber * currentNumber;
                if (currentOperation == "/") { if (currentNumber != 0) result = storedNumber / currentNumber; else calcInput = "ERR"; }
                if (calcInput != "ERR") calcInput = result.ToString(); currentOperation = "";
            }
            else
            {
                if (calcInput == "0" || calcInput == "ERR") calcInput = btn; else calcInput += btn;
            }
        }
    }
}