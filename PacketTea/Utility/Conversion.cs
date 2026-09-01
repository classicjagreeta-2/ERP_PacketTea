using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PacketTea.Utility
{
    public class Conversion 
    {
        public static string NumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[]
                {
                    "Zero", "One", "Two", "Three", "Four", "Five", "Six",
                    "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve",
                    "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen",
                    "Eighteen", "Nineteen"
                };

                var tensMap = new[]
                {
                    "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty",
                    "Sixty", "Seventy", "Eighty", "Ninety"
                };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dt = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
                dt.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                    values[i] = props[i].GetValue(item, null);

                dt.Rows.Add(values);
            }

            return dt;
        }
    }

}