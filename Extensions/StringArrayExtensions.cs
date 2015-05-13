using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonymousFerretTwitchLogger.Extensions
{
    public static class StringArrayExtensions
    {

        public static string Join(this string[] arr, string joinStr)
        {
            string a = "";
            foreach (string s in arr)
                a += (a.Length > 0 ? joinStr : "")+s;
            return a;
        }

    }
}
