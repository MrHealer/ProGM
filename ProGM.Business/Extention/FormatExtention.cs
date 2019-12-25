using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.Extention
{
   public class FormatExtention
    {
        public static string Money(string input)
        {
            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
            return double.Parse(input).ToString("#,### (vnđ)", cul.NumberFormat);
        }
        public static string FormartMinute(int minute)
        {
            int h = minute / 60;
            int s = minute % 60;
            if (h<1)
            {
                return s +" Phút";
            }
            else
            {
                return h+" giờ " +s + " phút";
            }
        }
    }
}
