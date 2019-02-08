using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GhigoWeb.Extensions
{
    public static class DateTimeExt
    {
        public static string ToGhigoShortDateString(this System.DateTime dateTime)
        {
            return dateTime.Year == 1900 ? "-" : dateTime.ToShortDateString();
        }
    }
}