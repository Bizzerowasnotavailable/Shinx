using Cosmos.System.Graphics;
using Cosmos.System;
using System;
using System.Drawing;

namespace Shinx.GUI
{
    public static class Calendar
    {
        private static int viewMonth = -1;
        private static int viewYear = -1;

        public static void Draw(VBECanvas vbe)
        {
            if (!Booleans.calendar_opened) return;

            if (viewMonth == -1)
            {
                viewMonth = DateTime.Now.Month;
                viewYear = DateTime.Now.Year;
            }

            Window.Draw(vbe, ref Int_Manager.calendar_x, ref Int_Manager.calendar_y, 260, 280, "Calendar", ref Booleans.calendar_opened);

            int x = Int_Manager.calendar_x;
            int y = Int_Manager.calendar_y;

            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            int[] daysInMonths = { 31, (DateTime.IsLeapYear(viewYear) ? 29 : 28), 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            vbe.DrawFilledRectangle(new Pen(Color.DarkBlue), x + 10, y + 30, 240, 30);

            vbe.DrawFilledRectangle(new Pen(Color.SteelBlue), x + 15, y + 35, 20, 20);
            ASC16.DrawACSIIString(vbe, "<", Color.White, (uint)x + 21, (uint)y + 37);

            vbe.DrawFilledRectangle(new Pen(Color.SteelBlue), x + 225, y + 35, 20, 20);
            ASC16.DrawACSIIString(vbe, ">", Color.White, (uint)x + 231, (uint)y + 37);

            string header = monthNames[viewMonth - 1] + " " + viewYear;
            ASC16.DrawACSIIString(vbe, header, Color.White, (uint)x + 85, (uint)y + 37);

            if (Mouse.Click())
            {
                if (MouseManager.X > x + 15 && MouseManager.X < x + 35 && MouseManager.Y > y + 35 && MouseManager.Y < y + 55)
                {
                    viewMonth--;
                    if (viewMonth < 1) { viewMonth = 12; viewYear--; }
                }
                if (MouseManager.X > x + 225 && MouseManager.X < x + 245 && MouseManager.Y > y + 35 && MouseManager.Y < y + 55)
                {
                    viewMonth++;
                    if (viewMonth > 12) { viewMonth = 1; viewYear++; }
                }
            }

            ASC16.DrawACSIIString(vbe, "Su Mo Tu We Th Fr Sa", Color.Black, (uint)x + 15, (uint)y + 70);

            DateTime firstOfMonth = new DateTime(viewYear, viewMonth, 1);
            int startDay = (int)firstOfMonth.DayOfWeek;
            int currentDay = 1;
            int row = 0;
            int col = startDay;

            while (currentDay <= daysInMonths[viewMonth - 1])
            {
                int drawX = x + 15 + (col * 33);
                int drawY = y + 95 + (row * 25);

                if (currentDay == DateTime.Now.Day && viewMonth == DateTime.Now.Month && viewYear == DateTime.Now.Year)
                {
                    vbe.DrawFilledRectangle(new Pen(Color.LightBlue), drawX - 2, drawY - 2, 25, 20);
                }

                string dayStr = currentDay < 10 ? " " + currentDay : currentDay.ToString();
                ASC16.DrawACSIIString(vbe, dayStr, Color.Black, (uint)drawX, (uint)drawY);

                currentDay++;
                col++;
                if (col > 6) { col = 0; row++; }
            }

            vbe.DrawFilledRectangle(new Pen(Color.LightGray), x + 90, y + 250, 80, 20);
            vbe.DrawRectangle(new Pen(Color.Black), x + 90, y + 250, 80, 20);
            ASC16.DrawACSIIString(vbe, "Today", Color.Black, (uint)x + 110, (uint)y + 253);

            if (Mouse.Click() && MouseManager.X > x + 90 && MouseManager.X < x + 170 && MouseManager.Y > y + 250 && MouseManager.Y < y + 270)
            {
                viewMonth = DateTime.Now.Month;
                viewYear = DateTime.Now.Year;
            }
        }
    }
}