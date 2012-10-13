using System;
using Microsoft.SPOT;

namespace JayDayService
{
    public static class Utilities
    {
        public static string Replace(string inString, string searchFor, string replaceWith)
        {
            string str = inString;
            int index;
            do
            {
                index = str.LastIndexOf(searchFor);
                if (index >= 0)
                {
                    str = str.Substring(0, index) + replaceWith + str.Substring(index + searchFor.Length);
                }
            } while (index > 0); return str;
        }

        public static void DisplayOnLine1(string msg, Gadgeteer.Modules.GHIElectronics.Display_HD44780 lcd)
        {
            Display(0, msg, lcd);
        }

        public static void DisplayBothLines(string line1, string line2, Gadgeteer.Modules.GHIElectronics.Display_HD44780 lcd)
        {
            Display(0, line1, lcd);
            Display(1, line2, lcd);
        }

        public static void DisplayOnLine2(string msg, Gadgeteer.Modules.GHIElectronics.Display_HD44780 lcd)
        {
            Display(1, msg, lcd);
        }

        private static void Display(byte line, string msg, Gadgeteer.Modules.GHIElectronics.Display_HD44780 lcd)
        {
            if (msg.Length > 16)
                msg = msg.Substring(0, 16);

            if (msg.Length < 16)
                msg += new string(' ', 16 - msg.Length);

            if (lcd != null)
            {
                lcd.SetCursor(line, 0);
                lcd.PrintString(msg);
            }            
        }

        public static void WriteDebug(string msg, Gadgeteer.Modules.GHIElectronics.Display_HD44780 lcd)
        {
            Debug.Print(msg);
            Display(1, msg, lcd);
        }

        public static void QuickSort(int left, int right, ref float[] array2Sort)
        {
            int leftHolder = left;
            int rightHolder = right;
            float pivot = array2Sort[left];

            while (left < right)
            {
                while ((array2Sort[right] >= pivot) && (left < right))
                    right--;

                if (left != right)
                {
                    array2Sort[left] = array2Sort[right];
                    left++;
                }

                while ((array2Sort[left] <= pivot) && (left < right))
                    left++;

                if (left != right)
                {
                    array2Sort[right] = array2Sort[left];
                    right--;
                }
            }

            array2Sort[left] = pivot;
            pivot = left;
            left = leftHolder;
            right = rightHolder;

            if (left < pivot) QuickSort(left, (int)pivot - 1, ref array2Sort);
            if (right > pivot) QuickSort((int)pivot + 1, right, ref array2Sort);
        }


    }

}
