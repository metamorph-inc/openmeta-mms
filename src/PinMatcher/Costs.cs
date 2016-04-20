using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

// Includes classes and methods used to figure out near-matches of schematic and SPICE pin names,
// to help connect them.  Based on a modified Levenstein algorithm and the Hungarian algorithm.
namespace PinMatcher
{
    public class Costs
    {
        /// <summary>
        /// Compute the distance between two pins.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int CostBetweenTwoPins( string s, string t )
        {
            int rVal = 1000;    // If either string is null, we use a large cost independent of how well it matches,
                                // because a null string is a wildcard representing a missing pin, not a pin with a null name.
            if( (s != "") && (t != "" ) )
            {
                rVal = modifiedLevenshteinDistance( s, t ); // Otherwise, use the Levenstein distance.
            }
            return rVal;
        }

        /// <summary>
        /// Compute a distance between two strings.
        /// </summary>
        public static int modifiedLevenshteinDistance(string t, string s)
        {
            const int scale = 10;   // A scale factor is added to give a fixed-point fractional precision
            int notSubstringPenalty = 30;  // The penalty for not being a substring is about 3 scaled mismatched characters.
            int notAbbreviationPenalty = 30; // The penalty for not starting with the same first letter.
            int mismatchedNumberPenalty = 50;   // The penalty for not containing matching numeric substrings.

            // degenerate cases
            if (s == t) return 0;
            if (s.Length == 0) return scale * t.Length;
            if (t.Length == 0) return scale * s.Length;

            // Avoid substring penalty if one string is a subset of the other
            if ((s.ToLower().IndexOf(t.ToLower()) > 0) || (t.ToLower().IndexOf(s.ToLower()) > 0))
            {
                notSubstringPenalty = 0;     // Being substrings removes a non-substring penalty
            }

            // Avoid abbreviation penalty if the first letters match.
            if (s.ToLower()[0] == t.ToLower()[0])
            {
                notAbbreviationPenalty = 0;
            }

            // Avoid the mismatched number penaly if each string contains the same numeric substring.
            Regex pattern = new Regex(@"^(?<prefix>[^0-9]*)(?<digits>[0-9]+)(?<suffix>[^0-9]*)$");
            Match matchS = pattern.Match(s);
            Match matchT = pattern.Match(t);

            if (matchS.Success && matchT.Success)
            {
                // Each string contains a numeric substring.
                // Compare the numeric part.
                int sDigits = int.Parse(matchS.Groups["digits"].Value);
                int tDigits = int.Parse(matchT.Groups["digits"].Value);
                if (sDigits == tDigits)
                {
                    mismatchedNumberPenalty = 0;
                }
            }

            // create two work vectors of integer distances
            int[] v0 = new int[t.Length + 1];
            int[] v1 = new int[t.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = scale * i;

            for (int i = 0; i < s.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = scale * (i + 1);

                // use formula to fill in the rest of the row
                for (int j = 0; j < t.Length; j++)
                {
                    // var cost = (s[i] == t[j]) ? 0 : scale;

                    // Consider letters that only differ in case as nearly equal.
                    var cost = (s[i] == t[j]) ? 0 : 
                        (char.IsLetter(s[i]) && (char.ToLower(s[i]) == char.ToLower(t[j])) ? 2 : scale);

                    var min1 = Math.Min( v1[j] + scale, v0[j + 1] + scale);
                    v1[j + 1] = Math.Min( min1, v0[j] + cost);
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[t.Length] + notSubstringPenalty + notAbbreviationPenalty + mismatchedNumberPenalty;
        }
    }
}
