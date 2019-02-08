using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GhigoWeb.Extensions
{
    public static class DecimalExt
    {
        public static string ToSmartInt(this decimal dec, string Zero = "")
        {
            if (dec == 0) return Zero;

            bool isDecimalNecessary = Convert.ToInt32(dec) != dec;
            return isDecimalNecessary ? dec.ToString("N") : dec.ToString("N0");
            
        }
    }
}