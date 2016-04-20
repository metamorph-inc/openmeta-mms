using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

// Includes classes and methods used to figure out near-matches of schematic and SPICE pin names,
// to help connect them.  Based on a modified Levenstein algorithm and the Hungarian algorithm.
namespace PinMatcher
{
    static public partial class PinMatcher
    {
        static readonly int index1 = 0;
        static readonly int index2 = 1;

        /// <summary>
        /// Produces an N x 2 array of strings showing the best match-up of two lists of strings.
        /// Used to match schematic pins with SPICE pins, for example.
        /// </summary>
        /// <param name="firstPinNames">The first list of strings, typically schematic pin names.</param>
        /// <param name="secondPinNames">The second list of strings, typically SPICE pin names.</param>
        /// <returns>N x 2 array of strings, showing the best match-up between two lists of strings.
        /// The first column of the array has strings from the first list in a sorted order.
        /// The second column of the array has a permutation of strings from the second list, so that
        /// strings on the same row will tend to be close matches.
        /// If one list is longer than the other, the shorter one will be extended with "" strings.</returns>
        /// <remarks>This method uses a modified Levenshtein algorithm to tell how close two strings
        /// are to matching, and the Hungarian algorithm to optimize the overall matchup.</remarks>
        /// <seealso cref="http://en.wikipedia.org/wiki/Levenshtein_distance"/>
        /// <seealso cref="http://en.wikipedia.org/wiki/Hungarian_algorithm"/>
        static public string[,] GetPinMatches(List<string> firstPinNames, List<string> secondPinNames)
        {
            int len1 = firstPinNames.Count;
            int len2 = secondPinNames.Count;

            int pinListSize = Math.Max(len1, len2);

            // Copy the pin lists so the new lists can be extended, if needed, to the same size with empty strings.
            List<string> newS1 = new List<string>(firstPinNames);
            newS1.Sort();
            while (newS1.Count < pinListSize)
            {
                newS1.Add("");
            }

            List<string> newS2 = new List<string>(secondPinNames);
            newS2.Sort();
            while (newS2.Count < pinListSize)
            {
                newS2.Add("");
            }

            // Now, both new lists of pins have the same length.
            // Compute a square cost matrix for the pins based on their distances.

            int[,] costMatrix = new int[pinListSize, pinListSize];

            for (int rows = 0; rows < pinListSize; rows++)
            {
                for (int cols = 0; cols < pinListSize; cols++)
                {
                    costMatrix[rows, cols] = Costs.CostBetweenTwoPins(newS1[rows], newS2[cols]);
                }
            }

            // Find the least cost assignment of pins to pins
            int[] pinMap = HungarianAlgorithm.FindAssignments(costMatrix);

            Debug.Assert(pinMap.Count() == pinListSize);

            // Create the output list of matched pin strings

            string[,] rVal = new string[pinListSize, 2];

            for (int i = 0; i < pinListSize; i++)
            {
                rVal[i, index1] = newS1[i];
                rVal[i, index2] = newS2[pinMap[i]];
            }
            return rVal;
        }
    }
}
