using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpiceLib;
using Xunit;

namespace SpiceLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Parse myParser = new Parse();
            string[] filenames = { "test1.cir", "multiparams.cir", "test2.cir", "test3.cir", "q2n222a.cir", "missing.cir", "nocircuit.cir",
                                 "shortSubcircuit.cir", "bogusName.cir", "bogusparams1.cir", "firstLineCommentTest.cir", 
                                 "nameCharsTestFail.cir", "nameCharsTestPass.cir", "braceTest.cir" };
            foreach( string filename in filenames )
            {
                try
                {
                    ComponentInfo result = myParser.ParseFile(filename );
                    Console.WriteLine("Parsing file '{0}' found {1}",
                        filename, result.ToString() );
                }
                catch (Exception e)
                {
                    // Let the user know what went wrong.
                    Console.WriteLine("ParseFile( {0} ) error:", filename);
                    Console.WriteLine(e.Message);
                }
            }
            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

        }
    }
}
