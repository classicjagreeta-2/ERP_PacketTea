using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PacketTea.Utility
{
    public class DateHelper
    {
        public static string DateFormatter(DateTime? date)
        {
            if (date!=null)
            {
                if (date.Value.ToString("dd/MM/yyyy") == "01/01/0001")
                {
                    return "dd/MM/yyyy"; // Placeholder for null
                }
            }           
            else if (date == null)
            {
                return "dd/MM/yyyy"; // Placeholder for null
            }
            var inputString = date.Value.ToString("dd/MM/yyyy");
            var dateString = Regex.Replace(inputString, "(th|st|rd|,|'|\\|\")", " ", RegexOptions.IgnoreCase);
            Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None); // replace 1+ space into one
            dateString = regex.Replace(dateString, " ");

            var formats = new[] {
            "yyyyMMdd",
            "d/M/yyyy",
            "dd/M/yyyy",
            "d/MM/yyyy",
            "dd/MM/yyyy",
            "d/M/yy",
            "dd/M/yy",
            "d/MM/yy",
            "dd/MM/yy",
            "d.M.yyyy",
            "dd.M.yyyy",
            "d.MM.yyyy",
            "dd.MM.yyyy",
            "d.M.yy",
            "dd.M.yy",
            "d.MM.yy",
            "dd.MM.yy",
            "d M yyyy",
            "dd M yyyy",
            "d MM yyyy",
            "dd MM yyyy",
            "d M yy",
            "dd M yy",
            "d MM yy",
            "dd MM yy",
            "d MMM yy",
            "d MMMM yy",
            "dd MMM yyyy",
            "dd MMMM yyyy",
            "dd MMM yyyy",
            "dMMMyy",
            "ddMMMyy",
            "dMMMyyyy",
            "ddMMMyyyy",
            "dMMMMyy",
            "ddMMMMyy",
            "dMMMMyyyy",
            "ddMMMMyyyy",
            "d MMMyy",
            "dd MMMyy",
            "d MMMyyyy",
            "dd MMMyyyy",
            "d MMMMyy",
            "dd MMMMyy",
            "d MMMMyyyy",
            "dd MMMMyyyy"
        };

            return date.Value.ToString("dd/MM/yyyy"); // Placeholder logic for demonstration
        }
    }
}

