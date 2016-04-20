using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MfgBom
{
    // See http://stackoverflow.com/questions/769621/dealing-with-commas-in-a-csv-file
    public static class Csv
    {
        public static string Escape(string s)
        {
            // If sting is empty/null, quit now.
            // Otherwise, the string has content, so proceed.
            if (String.IsNullOrWhiteSpace(s))
            {
                return "";
            }
            
            string t = s.Trim();
            if (t.Contains(QUOTE))
            {
                t = t.Replace(QUOTE, ESCAPED_QUOTE);
            }

            // Test if the string is a number (float or int)
            Boolean isNumber = false;
            float tmp;
            isNumber = float.TryParse(t, out tmp);
            int tmp2;
            isNumber = isNumber || int.TryParse(t, out tmp2);
            if (isNumber)
            {
                t = "'" + t;    // Prevent numeric strings from being turned into numbers, so the BOM
                                // won't drop leading 0's, such as in "0603" -> 603.
            }

            if (t.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
            {
                t = QUOTE + t + QUOTE;
            }

            return t;
        }

        public static string Escape(Nullable<float> f)
        {
            if (f.HasValue == false)
            {
                return "";      // MOT-338
            }

            return f.ToString();
        }

        public static string Unescape(string s)
        {
            if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(ESCAPED_QUOTE))
                    s = s.Replace(ESCAPED_QUOTE, QUOTE);
            }

            return s;
        }


        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ';', '"', ' ', ',', '\n' };
    }
}
