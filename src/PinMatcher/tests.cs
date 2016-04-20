using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PinMatcher
{
    public class tests
    {
        [Fact]
        public void test00CheckArraysAreEqualMethod()
        {
            string[,] a1 = 
            {
                { "1", "Pin1" },
                { "2", "Pin2" },
                { "3", "Pin3" },
                { "GND", "0" },
                { "VCC", "V+" }
             };

            string[,] a2 = // Same as a1
            {
                { "1", "Pin1" },
                { "2", "Pin2" },
                { "3", "Pin3" },
                { "GND", "0" },
                { "VCC", "V+" }
             };

            string[,] b = // V+ -> V-
            {
                { "1", "Pin1" },
                { "2", "Pin2" },
                { "3", "Pin3" },
                { "GND", "0" },
                { "VCC", "V-" }
             };

            // Equality of identical arrays
            Assert.True(arraysAreEqual(a1, a1));
            Assert.True(arraysAreEqual(a2, a2));
            Assert.True(arraysAreEqual(b, b));

            // Equality of equal-valued arrays
            Assert.True(arraysAreEqual(a1, a2));
            Assert.True(arraysAreEqual(a2, a1));

            // Inequality of inequal-valued arrays
            Assert.False(arraysAreEqual(a1, b));
            Assert.False(arraysAreEqual(b, a1));
            Assert.False(arraysAreEqual(a2, b));
            Assert.False(arraysAreEqual(b, a2));
        }

        [Fact]
        public void test01PinMatching()
        {
            List<string> schematicPins = new List<string>(){
                "1", "2", "3", "VCC", "GND"
            };
            List<string> spicePins = new List<string>(){
                "0", "Pin1", "Pin2", "Pin3", "V+"
            };

            string[,] expected = {
                { "1", "Pin1" },
                { "2", "Pin2" },
                { "3", "Pin3" },
                { "GND", "0" },
                { "VCC", "V+" },
            };

            string[,] result = PinMatcher.GetPinMatches(schematicPins, spicePins);

            Assert.True(arraysAreEqual(expected, result));
        }

        [Fact]
        public void test02PinMatching()
        {
            List<string> schematicPins = new List<string>(){
                "CA", "AN", "PIN1", "PIN2", "PIN3", "s1", "s2", "s3"
            };
            List<string> spicePins = new List<string>(){
                "3", "Pin2", "1", "anode", "cathode"
            };

            string[,] expected = {
                { "AN", "anode" },
                { "CA", "cathode" },
                { "PIN1", "" },
                { "PIN2", "Pin2" },
                { "PIN3", "" },
                { "s1", "1" },
                { "s2", "" },
                { "s3", "3" },
            };

            string[,] result = PinMatcher.GetPinMatches(schematicPins, spicePins);

            Assert.True(arraysAreEqual(expected, result));
        }

        [Fact]
        public void test03PinMatching()
        {
            List<string> schematicPins = new List<string>(){
                "E", "B", "C", "S", "D",
            };
            List<string> spicePins = new List<string>(){
                "COLLECTOR", "BASE", "SOURCE", "DRAIN", "EMITTER"
            };

            string[,] expected = {
                { "B", "BASE" },
                { "C", "COLLECTOR" },
                { "D", "DRAIN" },
                { "E", "EMITTER" },
                { "S", "SOURCE" },
            };

            string[,] result = PinMatcher.GetPinMatches(schematicPins, spicePins);

            Assert.True(arraysAreEqual(expected, result));
        }

        [Fact]
        public void test04HungarianAlgorithmCheck()
        {
            /* Example from Layman's Explanation, http://en.wikipedia.org/wiki/Hungarian_algorithm
             
            	    Clean bathroom	Sweep floors	Wash windows
                Jim 	$1	            $3	            $3
                Steve	$3	            $2	            $3
                Alan	$3	            $3	            $2
             */

            int[,] costs = {
                { 1, 3, 3 },
                { 3, 2, 3 },
                { 3, 3, 2 }
            };

            int[] expected = { 0, 1, 2 }; // Jim cleans the bathroom, Steve sweeps the floors and Alan washes the windows.

            int[] results = HungarianAlgorithm.FindAssignments(costs);
            Assert.Equal(expected, results);

        }


        /// <summary>
        /// Deep compare two 2-dimensional arrays of strings for equality.
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns>True if the arrays have equal values, otherwise false.</returns>
        static bool arraysAreEqual(string[,] a1, string[,] a2)
        {
            bool rVal = true;
            int rows = a1.GetLength(0);
            int cols = a1.GetLength(1);

            if ((a2.GetLength(0) != rows) || (a2.GetLength(1) != cols))
            {
                rVal = false;
            }
            else
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (a1[i, j] != a2[i, j])
                        {
                            rVal = false;
                            break;
                        }
                    }
                }
            }

            return rVal;
        }       
    }

    /// <summary>
    /// Stub for a method that could test private methods of the partial-attributed class.
    /// </summary>
    static public partial class PinMatcher
    {
        [Fact]
        public static void PinMatchInternalTestFunction()
        {

        }
    }
}
