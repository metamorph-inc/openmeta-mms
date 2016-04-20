using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using SpiceLib;
using System.IO;
using System.Reflection;

namespace SpiceLibTest
{
    public class SpiceParsingTests
    {
        private static String Path_TestModels = Path.Combine(
            Path.GetDirectoryName(Assembly.GetAssembly(typeof(SpiceParsingTests)).CodeBase.Substring("file:///".Length)),
                                                             "..", "..", "..", "..",
                                                             "models",
                                                             "SpiceTestModels");

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// testComponentInfoEquals
        /// Checks that the ComponentInfo-class "Equals" function is working normally.
        /// </summary>
        [Fact]
        public void test00()
        {
            // Create a ComponentInfo class and set initial values.
            ComponentInfo ci = new ComponentInfo()
            {
                name = "RES_0603",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                    {"RVAL", "1.0k"},
                    {"Bogus", "Yeah"}
                },
                pins = new List<string>()
                {
                    "PinOne",
                    "PinTwo"
                }
            };

            Assert.False(ci.Equals(null));
            Assert.True(ci.Equals(ci));

            // Create a ComponentInfo class and set initial values the same as before.
            ComponentInfo ci2 = new ComponentInfo()
            {
                name = "RES_0603",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                    {"RVAL", "1.0k"},
                    {"Bogus", "Yeah"}
                },
                pins = new List<string>()
                {
                    "PinOne",
                    "PinTwo"
                }
            };
            Assert.True(ci.Equals(ci2));
            Assert.True(ci2.Equals(ci2));
            Assert.True(ci2.Equals(ci));

            // Create a ComponentInfo class but don't set initial values.
            ComponentInfo ci3 = new ComponentInfo();
            Assert.True(ci3.Equals(ci3));
            Assert.False(ci2.Equals(ci3));
            Assert.False(ci3.Equals(ci));

            // Create a ComponentInfo class and set initial values slightly differently.
            ComponentInfo ci4 = new ComponentInfo()
            {
                name = "RES_0603",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                    {"RVAL", "1.0k"}
                },
                pins = new List<string>()
                {
                    "PinOne",
                    "PinTwo"
                }
            };

            Assert.True(ci4.Equals(ci4));
            Assert.False(ci2.Equals(ci4));

            // Create a ComponentInfo class with the pins in a different order.
            ComponentInfo ci5 = new ComponentInfo()
            {
                name = "RES_0603",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                    {"RVAL", "1.0k"}
                },
                pins = new List<string>()
                {
                     "PinTwo",
                     "PinOne",
                }
            };


            Assert.True(ci5.Equals(ci5));
            Assert.False(ci5.Equals(ci4));

        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Check an inductor subcircuit.
        /// </summary>
        [Fact]
        public void test01()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "L_LINEAR.cir");
            ComponentInfo result = myParser.ParseFile(filename);

            ComponentInfo expected = new ComponentInfo()
            {
                name = "L_LINEAR",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                    {"Inductance", "10e-6"},
                    {"Rs", "0.01"},
                    {"Cp", "2e-12"},
                    {"Rp", "300e3"},
                },
                pins = new List<string>()
                {
                    "1",
                    "2"
                }
            };

            Assert.True(result.Equals(expected));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Check a resistor subcircuit.
        /// </summary>
        [Fact]
        public void test1()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "test1.cir");
            ComponentInfo result = myParser.ParseFile(filename);

            ComponentInfo expected = new ComponentInfo()
            {
                name = "RES_0603",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                    {"RVAL", "1.0k"}
                },
                pins = new List<string>()
                {
                    "1",
                    "2"
                }
            };

            Assert.True(result.Equals(expected));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Check a DB9 connector
        /// </summary>
        [Fact]
        public void test2()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "test2.cir");
            ComponentInfo result = myParser.ParseFile(filename);

            ComponentInfo expected = new ComponentInfo()
            {
                name = "DB9",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                },
                pins = new List<string>()
                {
                    "PIN1",
                    "PIN2",
                    "PIN3",
                    "PIN4",
                    "PIN5",
                    "PIN6",
                    "PIN7",
                    "PIN8",
                    "PIN9"
                }
            };

            Assert.True(result.Equals(expected));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Check a resistor model.
        /// </summary>
        [Fact]
        public void test3()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "test3.cir");
            ComponentInfo result = myParser.ParseFile(filename);

            ComponentInfo expected = new ComponentInfo()
            {
                name = "RES_0603_330",
                elementType = 'R',
                parameters = new Dictionary<String, String>()
                {
                },
                pins = new List<string>()
                {
                    "PIN1",
                    "PIN2"
                }
            };

            Assert.True(result.Equals(expected));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Check a resistor subcircuit with two parameters
        /// </summary>
        [Fact]
        public void multiparams()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "multiparams.cir");
            ComponentInfo result = myParser.ParseFile(filename);

            ComponentInfo expected = new ComponentInfo()
            {
                name = "RES_MP",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                    {"RVAL", "1.0k"},
                    {"LVAL", "800pH"}
                },
                pins = new List<string>()
                {
                    "1",
                    "2"
                }
            };

            Assert.True(result.Equals(expected));
        }


        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Check a transistor model
        /// </summary>
        [Fact]
        public void q2n222a()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "q2n222a.cir");
            ComponentInfo result = myParser.ParseFile(filename);

            ComponentInfo expected = new ComponentInfo()
            {
                name = "Q2N222A",
                elementType = 'Q',
                parameters = new Dictionary<String, String>()
                {
                },
                pins = new List<string>()
                {
                    "COLLECTOR",
                    "BASE",
                    "EMITTER"
                }
            };

            Assert.True(result.Equals(expected));
        }


        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Check that the first line is treated as a comment
        /// </summary>
        [Fact]
        public void firstLineCommentTest()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "firstLineCommentTest.cir");
            ComponentInfo result = myParser.ParseFile(filename);

            ComponentInfo expected = new ComponentInfo()
            {
                name = "RES_2ndLineComponent",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                },
                pins = new List<string>()
                {
                    "1",
                    "2"
                }
            };

            Assert.True(result.Equals(expected));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// valid name characters test
        /// </summary>
        [Fact]
        public void nameCharsTestPass()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "nameCharsTestPass.cir");
            ComponentInfo result = myParser.ParseFile(filename);

            ComponentInfo expected = new ComponentInfo()
            {
                name = "AaBbCXxYyZz!#$%[0189]_",
                elementType = 'X',
                parameters = new Dictionary<String, String>()
                {
                },
                pins = new List<string>()
                {
                    "1",
                    "2"
                }
            };

            Assert.True(result.Equals(expected));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// invalid name characters test
        /// </summary>
        [Fact]
        public void nameCharsTestFail()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "nameCharsTestFail.cir");
            ComponentInfo result = new ComponentInfo();
            var exception = Assert.Throws<Exception>(delegate
            {
                result = myParser.ParseFile(filename);
            });
            Assert.True(exception.Message.Contains("not a valid subcircuit name"));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// File does not exist test
        /// </summary>
        [Fact]
        public void missing()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "missing.cir");
            ComponentInfo result = new ComponentInfo();
            var exception = Assert.Throws<Exception>(delegate
            {
                result = myParser.ParseFile(filename);
            });
            Assert.True(exception.Message.Contains("does not exist"));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// File does not contain a circuit test
        /// </summary>
        [Fact]
        public void nocircuit()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "nocircuit.cir");
            ComponentInfo result = new ComponentInfo();
            var exception = Assert.Throws<Exception>(delegate
            {
                result = myParser.ParseFile(filename);
            });
            Assert.True(exception.Message.Contains("No subcircuit"));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Subcircuit has no pins
        /// </summary>
        [Fact]
        public void shortSubcircuit()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "shortSubcircuit.cir");
            ComponentInfo result = new ComponentInfo();
            var exception = Assert.Throws<Exception>(delegate
            {
                result = myParser.ParseFile(filename);
            });
            Assert.True(exception.Message.Contains("Not enough fields"));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// hyphen in model name
        /// </summary>
        [Fact]
        public void bogusName()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "bogusName.cir");
            ComponentInfo result = new ComponentInfo();
            var exception = Assert.Throws<Exception>(delegate
            {
                result = myParser.ParseFile(filename);
            });
            Assert.True(exception.Message.Contains("not a valid model name"));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// hyphen in model name
        /// </summary>
        [Fact]
        public void bogusparams1()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "bogusparams1.cir");
            ComponentInfo result = new ComponentInfo();
            var exception = Assert.Throws<Exception>(delegate
            {
                result = myParser.ParseFile(filename);
            });
            Assert.True(exception.Message.Contains("needs an equals sign"));
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// brace-expression parameter
        /// </summary>
        [Fact]
        public void braceTest()
        {
            Parse myParser = new Parse();
            string filename = Path.Combine(Path_TestModels, "braceTest.cir");
            ComponentInfo result = new ComponentInfo();
            var exception = Assert.Throws<Exception>(delegate
            {
                result = myParser.ParseFile(filename);
            });
            Assert.True(exception.Message.Contains("brace-expression parameter"));
        }

    }
}
