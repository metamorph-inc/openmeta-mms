/***********************************************************************
 * FILENAME:    InterpreterTest.cs
 *
 * DESCRIPTION:
 *       This file provides a test fixture and xunit tests using
 *       workbenches within the "SchematicTestModel.xme" file.
 *
 * NOTES:
 *      This test assembly has been added to /test/tests.xunit,
 *      so it will also run as part of Metamorph's test suite.
 *
 *      To manually run these XUNIT tests, you need to start the XUNIT GUI.
 *      It can be found at:
 *
 *          \META\3rdParty\xunit-1.9.1\xunit.gui.clr4.x86.exe
 *
 *      After launching the XUNIT GUI, under the assembly tab, choose
 *      the “SchematicUnitTests.dll” file to see a list tests.  The dll
 *      is at "\test\SchematicUnitTests\bin\Release".
 *
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GME.CSharp;
using GME.MGA;
using META;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using System.Text.RegularExpressions;
using CyPhyComponentFidelitySelector;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Runtime.InteropServices;

namespace SchematicUnitTests
{
    public class InterpreterTestFixture : InterpreterFixtureBaseClass
    {
        public override String path_XME
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                              "..\\..\\..\\..",
                                                              "models",
                                                              "SchematicTestModel",
                                                              "Schematic.xme");
            }
        }
    }

    public class InterpreterTest : InterpreterTestBaseClass, IUseFixture<InterpreterTestFixture>
    {
        public const string generatedSchemaFile = "schema.sch";
        public const string generatedSpiceTemplateFile = "schema.cir.template";
        public const string generatedSpiceFile = "schema.cir";
        public const string generatedLayoutFile = "layout-input.json";
        public const string generatedSpiceViewerLauncher = "LaunchSpiceViewer.bat";

        #region Fixture
        InterpreterTestFixture fixture;
        public void SetFixture(InterpreterTestFixture data)
        {
            fixture = data;
        }
        #endregion

        public override MgaProject project
        {
            get
            {
                return fixture.proj;
            }
        }

        public override String TestPath
        {
            get
            {
                return fixture.path_Test;
            }
        }

        //----------------------------------------------------------
        //--------      Start of unit test definitions      --------
        //----------------------------------------------------------

        /// <summary>
        /// Check that a board-edge exception can place parts near the board edge, MOT-728.
        /// </summary>
        [Fact]
        public void BoardEdgeExceptionTest()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            // MOT-728.
            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/PR_BoardEdgeConstraintException|kind=TestBench|relpos=0";

            var pathBoardFile = ExecuteAndCheckPreRouteTB(OutputDir, TestbenchPath);

            // Check component position. If different, subsequent regression tests will fail.
            var componentName = "R1";
            var expectedXPos = 7.8;
            var expectedYPos = 2.1;
            var expectedRotation = 0;
            var expectedLayer = 0;

            CheckComponentPosition(OutputDir, componentName, expectedXPos, expectedYPos, expectedRotation, expectedLayer);

        }


        /// <summary>
        /// Check for correct generation of Global Layout Constraint Exceptions, MOT-728.
        /// This test checks constraint exceptions connected to individual components.
        /// </summary>
        [Fact]
        public void GlobalConstraintExceptions()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/PR_GlobalConstraintException|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            JObject layout = null;

            using (StreamReader reader = File.OpenText(Path.Combine(OutputDir, generatedLayoutFile)))
            {
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    var serializer = new JsonSerializer();
                    layout = serializer.Deserialize<JObject>(jsonTextReader);
                }
            }

            Assert.NotNull(layout);

            var pkg = layout["packages"];

            // The R1 component should have a board-edge-spacing-constraint exception.
            var comp2 = pkg.First(p => p["name"].ToString() == "R1");
            var constr2 = comp2["constraints"].First(c => c["type"].ToString() == "except");
            Assert.Equal("Board_Edge_Spacing", constr2["name"].Value<string>());

        }

        /// <summary>
        /// Check Global Layout Constraint Exceptions on subassemblies, MOT-728.
        /// Here we check a constraint exception on a subassembly that hasn't been pre-routed.
        /// That exception should propagate to the subassembly's components.
        /// </summary>
        [Fact]
        public void GlobalConstraintExceptionsOnSubassemblies()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/PR_GlobalConstraintExceptionOnSubassembly|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            JObject layout = null;

            using (StreamReader reader = File.OpenText(Path.Combine(OutputDir, generatedLayoutFile)))
            {
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    var serializer = new JsonSerializer();
                    layout = serializer.Deserialize<JObject>(jsonTextReader);
                }
            }

            Assert.NotNull(layout);

            var pkg = layout["packages"];

            // The R1 component should have two constraint exceptions.
            var comp = pkg.First(p => p["name"].ToString() == "R1");
            var exceptList = comp["constraints"].Where(c => c["type"].ToString() == "except");
            List<string> exceptionNames = new List<string>();
            foreach (var exception in exceptList)
            {
                exceptionNames.Add(exception["name"].ToString());
            }
            Assert.True(exceptionNames.Contains("Board_Edge_Spacing"), "The R1 component is missing the Board_Edge_Spacing exception.");

            // The C1 component should have one constraint exception.
            comp = pkg.First(p => p["name"].ToString() == "C1");
            exceptList = comp["constraints"].Where(c => c["type"].ToString() == "except");
            exceptionNames = new List<string>();
            foreach (var exception in exceptList)
            {
                exceptionNames.Add(exception["name"].ToString());
            }
            Assert.True(exceptionNames.Contains("Board_Edge_Spacing"), "The C1 component is missing the Board_Edge_Spacing exception.");

            // The R2 component should have no constraint exceptions.
            comp = pkg.First(p => p["name"].ToString() == "R2");
            exceptList = comp["constraints"].Where(c => c["type"].ToString() == "except");
            exceptionNames = new List<string>();
            foreach (var exception in exceptList)
            {
                exceptionNames.Add(exception["name"].ToString());
            }
            Assert.False(exceptionNames.Contains("Board_Edge_Spacing"), "The R2 component has an unexpected Board_Edge_Spacing exception.");
        }


        /// <summary>
        /// Check Global Layout Constraint Exceptions on a Pre-Routed Subassemblies, MOT-728.
        /// Here we check a constraint exception on a pre-routed subassembly.
        /// </summary>
        [Fact]
        public void GlobalConstraintExceptionsOnPreRoutedSubassemblies()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/PR_GlobalConstraintExceptionOnPreRoutedSubassembly|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            JObject layout = null;

            using (StreamReader reader = File.OpenText(Path.Combine(OutputDir, generatedLayoutFile)))
            {
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    var serializer = new JsonSerializer();
                    layout = serializer.Deserialize<JObject>(jsonTextReader);
                }
            }

            Assert.NotNull(layout);

            var pkg = layout["packages"];

            // Check that all the components in the pre-routed subassembly only have a Board_Edge_Spacing constraint exception.
            List<string> preRoutedComponentNames = new List<string> { "RdvC", "TC", "RlimC" };

            foreach (var componentName in preRoutedComponentNames)
            {
                // Each component should have only a Board_Edge_Spacing constraint exception.
                var comp = pkg.First(p => p["name"].ToString() == componentName);
                var exceptList = comp["constraints"].Where(c => c["type"].ToString() == "except");
                List<string> exceptionNames = new List<string>();
                foreach (var exception in exceptList)
                {
                    exceptionNames.Add(exception["name"].ToString());
                }
                Assert.True(exceptionNames.Contains("Board_Edge_Spacing"), String.Format("The {0} component is missing the Board_Edge_Spacing exception.", componentName));
            }
        }


        /// <summary>
        /// Check that pre-routing can use blind, buried, and stacked vias, MOT-604.
        /// </summary>
        [Fact]
        public void PreRoutedBlindViasTest()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/Place_and_Route_Blind_and_Buried_Vias|kind=TestBench|relpos=0";

            var pathBoardFile = ExecuteAndCheckPreRouteTB(OutputDir, TestbenchPath);

            // Check ASIC position. If different, subsequent regression tests will fail.
            var expectedXPos = 0.0;
            var expectedYPos = 0.9;
            CheckAsicPosition(OutputDir, expectedXPos, expectedYPos);

            // Check that the contents of the schema.brd file seems OK.
            // See also: http://stackoverflow.com/questions/1427149/count-a-specifc-word-in-a-text-file-in-c-sharp
            //
            // Note: Normally, the GME "boardEdgeSpace" parameter affects component positioning, but not in this test.
            // This is because this test doesn't convert the GME "boardEdgeSpace" parameter to a batch-file command-line parameter
            // when invoking "placeonly.bat".

            Dictionary<string, string> testStrings = new Dictionary<string, string>()
            {
                //{"Layer 3 wire from pin G2 of GP ASIC to buried via", "<wire x1=\"1.6\" y1=\"2.8\" x2=\"2.4\" y2=\"2.8\" width=\"0.1\" layer=\"3\" />"},
                {"Layer 3 wire from pin G2 of GP ASIC to buried via", "<wire x1=\"8.4\" y1=\"12.1\" x2=\"9.2\" y2=\"12.1\" width=\"0.1\" layer=\"3\" />"},
                //{"Blind micro-via from Layer 1 to Layer 2 at pin G2 of GP ASIC", "<via x=\"1.6\" y=\"2.8\" extent=\"1-2\" drill=\"0.127\" />"},
                {"Blind micro-via from Layer 1 to Layer 2 at pin G2 of GP ASIC", "<via x=\"8.4\" y=\"12.1\" extent=\"1-2\" drill=\"0.127\" />"},
                //{"Stacked micro-via from Layer 2 to Layer 3 at pin G2 of GP ASIC", "<via x=\"1.6\" y=\"2.8\" extent=\"2-3\" drill=\"0.127\" />"},
                {"Stacked micro-via from Layer 2 to Layer 3 at pin G2 of GP ASIC", "<via x=\"8.4\" y=\"12.1\" extent=\"2-3\" drill=\"0.127\" />"},
                //{"Buried micro-via from Layer 2 to Layer 3", "<via x=\"2.4\" y=\"2.8\" extent=\"2-3\" drill=\"0.127\" />"}
                {"Buried micro-via from Layer 2 to Layer 3", "<via x=\"9.2\" y=\"12.1\" extent=\"2-3\" drill=\"0.127\" />"}
            };

            using (StreamReader reader = File.OpenText(pathBoardFile))
            {
                string contents = reader.ReadToEnd();
                foreach (string testStringName in testStrings.Keys)
                {
                    string testString = testStrings[testStringName];
                    bool testStringFound = contents.Contains(testString);
                    Assert.True(testStringFound, testStringName + " PreRoutedBlindViasTest string not found in generated board file.");
                }
            }
        }

        private string ExecuteAndCheckPreRouteTB(string OutputDir, string TestbenchPath)
        {
            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            // At this point, we've created a "layout-input.json" file, and we need to run the "placeonly.bat" file to
            // synthesize a "schema.brd" file.

            string batchFileName = "placeonly.bat";
            var pathBatchFile = Path.Combine(OutputDir,
                                             batchFileName);
            Assert.True(File.Exists(pathBatchFile));

            string generatedBoardFileName = "schema.brd";
            var pathBoardFile = Path.Combine(OutputDir,
                                             generatedBoardFileName);

            // Run the "placeonly.bat" batch file
            var processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + batchFileName + "\"")
            {
                WorkingDirectory = OutputDir,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit(10000);

                // Read the streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Assert.True(0 == process.ExitCode, output + Environment.NewLine + error);
            }

            // Check that the "schema.brd" file exists.
            Assert.True(File.Exists(pathBoardFile), "Failed to generate " + generatedBoardFileName);
            return pathBoardFile;
        }

        /// <summary>
        /// Check "layout.json" to see if the ASIC is placed where we expect.
        /// </summary>
        /// <param name="OutputDir"></param>
        /// <param name="expectedXPos"></param>
        /// <param name="expectedYPos"></param>
        private static void CheckAsicPosition(string OutputDir, double expectedXPos, double expectedYPos)
        {
            var pathLayoutFile = Path.Combine(OutputDir, "layout.json");
            var jsonLayoutFile = File.ReadAllText(pathLayoutFile);
            var jobjLayoutFile = Newtonsoft.Json.Linq.JObject.Parse(jsonLayoutFile);

            var pkgAsic = jobjLayoutFile["packages"].First(p => p["name"].ToString() == "GP_ASIC");

            var realX = pkgAsic["x"].Value<Double>();
            var realY = pkgAsic["y"].Value<Double>();

            Assert.True(realX == expectedXPos, "GP_ASIC not placed at the expected position.");

            // Have to use this comparison because it's a floating-point value
            Assert.True(Math.Abs(realY - expectedYPos) < 0.01, "GP_ASIC not placed at the expected position.");
        }

        /// <summary>
        /// Check "layout.json" to see if a component is placed where we expect.
        /// </summary>
        /// <param name="OutputDir"></param>
        /// <param name="componentName"></param>
        /// <param name="expectedXPos"></param>
        /// <param name="expectedYPos"></param>
        /// <param name="expectedRotation"></param>
        /// <param name="expectedLayer"></param>
        private static void CheckComponentPosition(string OutputDir, string componentName, double expectedXPos, double expectedYPos, int expectedRotation, int expectedLayer)
        {
            var pathLayoutFile = Path.Combine(OutputDir, "layout.json");
            var jsonLayoutFile = File.ReadAllText(pathLayoutFile);
            var jobjLayoutFile = Newtonsoft.Json.Linq.JObject.Parse(jsonLayoutFile);

            var pkg = jobjLayoutFile["packages"].First(p => p["name"].ToString() == componentName);

            var realX = pkg["x"].Value<Double>();
            var realY = pkg["y"].Value<Double>();
            var realRotation = pkg["rotation"].Value<Double>();
            var realLayer = pkg["layer"].Value<Double>();

            var msg = componentName + " not placed at the expected ";
            var epsilon = 0.01; // a generous error limit for floating-point math

            Assert.True(Math.Abs(realX - expectedXPos) < epsilon, msg + "X position.");
            Assert.True(Math.Abs(realY - expectedYPos) < epsilon, msg + "Y position.");
            Assert.True(Math.Abs(realRotation - expectedRotation) < epsilon, msg + "rotation.");
            Assert.True(Math.Abs(realLayer - expectedLayer) < epsilon, msg + "layer.");
        }


        /// <summary>
        /// Check pre-routing of a GP ASIC sub-assembly with a Via-In-Pad (VIP), MOT-618.
        /// </summary>
        [Fact]
        public void PreRoutedGpAsicVipTest()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/Place_and_Route_VIP|kind=TestBench|relpos=0";

            var pathBoardFile = ExecuteAndCheckPreRouteTB(OutputDir, TestbenchPath);

            // Check ASIC position. If different, subsequent regression tests will fail.
            var expectedXPos = 1.0;
            var expectedYPos = 1.9;
            CheckAsicPosition(OutputDir, expectedXPos, expectedYPos);

            // Check that the contents of the schema.brd file seems OK.
            // See also: http://stackoverflow.com/questions/1427149/count-a-specifc-word-in-a-text-file-in-c-sharp
            //
            // Note: Normally, the GME "boardEdgeSpace" parameter affects component positioning, but not in this test.
            // This is because this test doesn't convert the GME "boardEdgeSpace" parameter to a batch-file command-line parameter
            // when invoking "placeonly.bat".

            Dictionary<string, string> testStrings = new Dictionary<string, string>()
            {
                {"Pin A1 offset in GP ASIC", "<smd name=\"A1\" x=\"-2\" y=\"2\" dx=\"0.28\" dy=\"0.28\" layer=\"1\" roundness=\"100\" stop=\"no\" />"},
                //// {"GP ASIC location", "<element name=\"GP_ASIC\" library=\"henry_review\" package=\"BGA107\" value=\"T6WR6XBG\" x=\"4.5\" y=\"4.5\" />"},
                //{"GP ASIC location", "<element y=\"4.2\" x=\"4.2\" library=\"henry_review\" name=\"GP_ASIC\" value=\"T6WR6XBG\" package=\"BGA107\" />"},
                {"GP ASIC location", "<element y=\"12.5\" x=\"10\" library=\"henry_review\" name=\"GP_ASIC\" value=\"T6WR6XBG\" package=\"BGA107\" />"},
                //// {"micro-VIA location", "<via x=\"2.5\" y=\"6.5\" extent=\"1-16\" drill=\"0.08\" />"}
                //{"micro-VIA location", "<via x=\"2.2\" y=\"6.2\" extent=\"1-16\" drill=\"0.08\" />"}
                {"micro-VIA location", "<via x=\"12\" y=\"10.5\" extent=\"1-16\" drill=\"0.08\" />"}
            };

            using (StreamReader reader = File.OpenText(pathBoardFile))
            {
                string contents = reader.ReadToEnd();
                foreach (string testStringName in testStrings.Keys)
                {
                    string testString = testStrings[testStringName];
                    bool testStringFound = contents.Contains(testString);
                    Assert.True(testStringFound, testStringName + " VIP test string not found in generated board file.");
                }
            }
        }

        /// <summary>
        /// Check that place and route can generate a four-layer board. MOT-587
        /// </summary>
        [Fact]
        public void FourLayerBoardGeneration()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/PlaceAndRoute|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            // At this point, we've created a "layout-input.json" file, and we need to run the "placeonly.bat" file to
            // synthesize a "schema.brd" file.
            var pathBoardFile = RunPlaceOnly(OutputDir);

            // Check that the contents of the schema.brd file seems OK.
            // See also: http://stackoverflow.com/questions/1427149/count-a-specifc-word-in-a-text-file-in-c-sharp
            string layer2testString = "<layer number=\"2\" name=\"Route2\" color=\"1\" fill=\"3\" />";
            string layer15testString = "<layer number=\"15\" name=\"Route15\" color=\"4\" fill=\"6\" />";
            using (StreamReader reader = File.OpenText(pathBoardFile))
            {
                string contents = reader.ReadToEnd();
                bool layer2found = contents.Contains(layer2testString);
                bool layer15found = contents.Contains(layer15testString);
                Assert.True(layer2found, "Layer 2 test string not found in generated board file.");
                Assert.True(layer15found, "Layer 15 test string not found in generated board file.");
            }
        }

        /// <summary>
        /// Check for correct generation of Relative Layer constraints
        /// </summary>
        [Fact]
        public void RelativeLayerConstraints()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/PR_RelativeLayerConstraint|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            JObject layout = null;

            using (StreamReader reader = File.OpenText(Path.Combine(OutputDir, generatedLayoutFile)))
            {
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    var serializer = new JsonSerializer();
                    layout = serializer.Deserialize<JObject>(jsonTextReader);
                }
            }

            Assert.NotNull(layout);

            var pkg = layout["packages"];

            var comp2 = pkg.First(p => p["name"].ToString() == "Comp2");
            var constr2 = comp2["constraints"].First(c => c["type"].ToString() == "relative-pkg");
            Assert.Null(constr2["layer"]);

            var comp3 = pkg.First(p => p["name"].ToString() == "Comp3");
            var constr3 = comp3["constraints"].First(c => c["type"].ToString() == "relative-pkg");
            Assert.Equal(0, constr3["layer"].Value<int>());

            var comp4 = pkg.First(p => p["name"].ToString() == "Comp4");
            var constr4 = comp4["constraints"].First(c => c["type"].ToString() == "relative-pkg");
            Assert.Equal(1, constr4["layer"].Value<int>());
        }

        /// <summary>
        /// Check for correct generation of Relative Range constraints
        /// </summary>
        [Fact]
        public void RelativeRangeConstraints()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/PR_RelativeRangeConstraint|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            JObject layout = null;

            using (StreamReader reader = File.OpenText(Path.Combine(OutputDir, generatedLayoutFile)))
            {
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    var serializer = new JsonSerializer();
                    layout = serializer.Deserialize<JObject>(jsonTextReader);
                }
            }

            Assert.NotNull(layout);

            var pkg = layout["packages"];

            var comp1 = pkg.First(p => p["name"].ToString() == "Comp1");
            var anchor_idx = comp1["pkg_idx"].Value<int>();

            // Component at the top level, with positive offset values
            CheckRelativeRangeConstraint(pkg,
                                         anchor_idx,
                                         "5:5",
                                         "5:5",
                                         0,
                                         "Comp_5_5_offset");

            // Component at the top level, with negative offset values
            CheckRelativeRangeConstraint(pkg,
                                         anchor_idx,
                                         "-5:-5",
                                         "-5:-5",
                                         1,
                                         "Comp_minus_5_5_offset");

            // Component inside a Component Assembly, with positive offset values
            CheckRelativeRangeConstraint(pkg,
                                         anchor_idx,
                                         "5:5",
                                         "5:5",
                                         0,
                                         "CompCA");

            // Component inside a Component Assembly Ref, with positive offset values
            CheckRelativeRangeConstraint(pkg,
                                         anchor_idx,
                                         "5:5",
                                         "5:5",
                                         0,
                                         "R1");

            // Component inside a Component Assembly Ref, with positive offset values
            CheckRelativeRangeConstraint(pkg,
                                         anchor_idx,
                                         "5:5",
                                         "5:5",
                                         0,
                                         "R2");
        }

        private static void CheckRelativeRangeConstraint(JToken pkg, int anchor_idx, String expectedX, String expectedY, int expectedLayer, String pkgName)
        {
            var component = pkg.First(p => p["name"].ToString() == pkgName);
            var constraint = component["constraints"].First(c => c["type"].ToString() == "relative-region");
            Assert.Equal(anchor_idx, constraint["pkg_idx"].Value<int>());
            Assert.Equal(expectedX, constraint["x"].Value<String>());
            Assert.Equal(expectedY, constraint["y"].Value<String>());
            Assert.Equal(expectedLayer, constraint["layer"].Value<int>());
        }

        /// <summary>
        /// Check for correct generation of Region constraints.
        /// Regions include:
        /// - Range constraints with X, Y, and LAYER all constrained
        /// - Range constraints marked as "exclusion"
        /// </summary>
        [Fact]
        public void RegionConstraints()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/P_RegionConstraints|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            JObject layout = null;

            using (StreamReader reader = File.OpenText(Path.Combine(OutputDir, generatedLayoutFile)))
            {
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    var serializer = new JsonSerializer();
                    layout = serializer.Deserialize<JObject>(jsonTextReader);
                }
            }

            Assert.NotNull(layout);

            var pkg = layout["packages"];

            var compIncl = pkg.First(p => p["name"].ToString() == "InclusiveRegion");
            var constrIncl = compIncl["constraints"].First(c => c["type"].ToString() == "in-region");
            Assert.NotNull(constrIncl["x"]);
            Assert.NotNull(constrIncl["y"]);
            Assert.NotNull(constrIncl["layer"]);

            var compExcl = pkg.First(p => p["name"].ToString() == "ExclusiveRegion");
            var constrExcl = compExcl["constraints"].First(c => c["type"].ToString() == "ex-region");
            Assert.NotNull(constrExcl["x"]);
            Assert.NotNull(constrExcl["y"]);
            Assert.NotNull(constrExcl["layer"]);

            // If an Exclusion is partially-specified, it should be ignored
            var compIncompleteExcl = pkg.First(p => p["name"].ToString() == "IncompleteExclusiveRegion");
            var numExConstraints = compIncompleteExcl["constraints"].Count(c => c["type"].ToString() == "ex-region");
            Assert.Equal(0, numExConstraints);

        }

        /// <summary>
        /// Check that place and route can pre-route a sub-assembly with components on both sides. MOT-613
        /// </summary>
        [Fact]
        public void PreRoutedWithComponentsOnTopAndBottom()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@AraTestBenches|kind=Testing|relpos=0/Place_and_Route_for_MOT-613|kind=TestBench|relpos=0";

            var pathBoardFile = ExecuteAndCheckPreRouteTB(OutputDir, TestbenchPath);

            // Check that the contents of the schema.brd file seems OK.
            //string resistorString = @"<element y=""7.2"" rot=""MR270"" x=""2.2"" library=""chipResistors"" name=""R_0402_200_0.1W_1%_Thick_Film"" value=""RESISTOR_0402""";
            //string ledString = @"<element y=""3.2"" rot=""R270"" x=""2.2"" library=""Andres_Project"" name=""Red_LED"" value=""LED"" package=""LED""";
            //string resistorString = @"<element y=""13"" rot=""MR270"" x=""10"" library=""chipResistors"" name=""R_0402_200_0.1W_1%_Thick_Film"" value=""RESISTOR_0402""";
            //string ledString = @"<element y=""9"" rot=""R270"" x=""10"" library=""Andres_Project"" name=""Red_LED"" value=""LED"" package=""LED""";
            string resistorString = @"<element y=""13"" rot=""MR270"" x=""10"" library=""chipResistors"" name=""R_0402_200_0.1W_1%_Thick_Film"" value=""ERJ-2RKF2000X"" package=""R_0402""";
            string ledString = @"<element y=""9"" rot=""R270"" x=""10"" library=""Andres_Project"" name=""Red_LED"" value=""LTST-C191KRKT"" package=""LED""";

            using (StreamReader reader = File.OpenText(pathBoardFile))
            {
                string contents = reader.ReadToEnd();
                bool resistorOK = contents.Contains(resistorString);
                bool ledOK = contents.Contains(ledString);
                Assert.True(resistorOK, "Chip resistor test string not found in generated board file.");
                Assert.True(ledOK, "LED test string not found in generated board file.");
            }
        }


        [Fact]
        [Trait("Type", "SPICE")]
        public void SpiceGeneration()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/Spice_Test|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doSpice = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Failed to generate " + generatedSpiceTemplateFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Generated EAGLE schematic (" + generatedSchemaFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Generated layout file (" + generatedLayoutFile + "), but shouldn't have.");
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Failed to generate " + generatedSpiceViewerLauncher);


            string sch = File.ReadAllText(Path.Combine(OutputDir, generatedSpiceTemplateFile), System.Text.Encoding.UTF8);
            Assert.Contains("\nRRfb ", sch);
            Assert.Contains("\nRRin ", sch);
            Assert.Contains("\nXSpicySingleOpAmp", sch);
            Assert.Contains("\n.SUBCKT OPAMP1      1   2   6", sch);

            Assert.True(Regex.Match(sch, "RRfb \\d+ \\d+  10000").Success);
            Assert.True(Regex.Match(sch, "RRin \\d+ \\d+  10000").Success);
            Assert.True(Regex.Match(sch, "XSpicySingleOpAmp \\d+ \\d+ \\d+ OPAMP1").Success);

        }

        [Fact]
        [Trait("Type", "SPICE")]
        public void SpiceTemplateGeneration()
        {
            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/SpiceTemplateTest|kind=TestBench|relpos=0";

            var runResult = MasterInterpreterTest.CyPhyMasterInterpreterRunner.RunMasterInterpreterAndReturnResults(fixture.path_MGA,
                                                                        TestbenchPath,
                                                                        "/@A_SpiceTests|kind=ComponentAssemblies|relpos=0/@SpiceParametricModel|kind=ComponentAssembly|relpos=0");

            string OutputDir = runResult.OutputDirectory;

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Failed to generate " + generatedSpiceTemplateFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Generated EAGLE schematic (" + generatedSchemaFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Generated layout file (" + generatedLayoutFile + "), but shouldn't have.");
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Failed to generate " + generatedSpiceViewerLauncher);

            string sch = File.ReadAllText(Path.Combine(OutputDir, generatedSpiceTemplateFile), System.Text.Encoding.UTF8);
            Assert.Contains("\nXResistor ", sch);
            Assert.Contains("\nLInductor ", sch);
            Assert.True(Regex.Match(sch, "XResistor \\d+ \\d+ R_CHIP Cp=4e-14 Ls=5e-10 Resistance=180").Success);
            Assert.True(Regex.Match(sch, "VRFGen \\d+ \\d+  SINE \\$\\{Offset\\} \\$\\{Amplitude\\} \\$\\{Frequency\\} \\$\\{Delay\\}").Success);

            RunPopulateSchemaTemplate(OutputDir);
            sch = File.ReadAllText(Path.Combine(OutputDir, generatedSpiceFile), System.Text.Encoding.UTF8);
            Assert.Contains("\nXResistor ", sch);
            Assert.Contains("\nLInductor ", sch);
            Assert.True(Regex.Match(sch, "XResistor \\d+ \\d+ R_CHIP Cp=4e-14 Ls=5e-10 Resistance=180").Success);
            Assert.True(Regex.Match(sch, "VRFGen \\d+ \\d+  SINE 3.253e-12 1.001 1000 1.293e-13").Success);
        }

        //TODO: Add test for running in the context of a PET.

        [Fact]
        [Trait("Type", "SPICE")]
        public void SpiceTemplateGenerationFormula()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/SpiceTemplateTestFormula|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doSpice = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Failed to generate " + generatedSpiceTemplateFile);

            string sch = File.ReadAllText(Path.Combine(OutputDir, generatedSpiceTemplateFile), System.Text.Encoding.UTF8);
            Assert.True(Regex.Match(sch, "VRFGen \\d+ \\d+  SINE \\$\\{Offset\\} \\$\\{Amplitude\\} 1e\\+06 \\$\\{Delay\\}").Success);

            //Check for warning in log.
        }

        [Fact]
        [Trait("Type", "SPICE")]
        public void SpiceAsmModelGeneration()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing/@SpiceAsmModelGeneration";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doSpice = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Failed to generate " + generatedSpiceTemplateFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Generated EAGLE schematic (" + generatedSchemaFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Generated layout file (" + generatedLayoutFile + "), but shouldn't have.");
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Failed to generate " + generatedSpiceTemplateFile);

            string sch = File.ReadAllText(Path.Combine(OutputDir, generatedSpiceTemplateFile), System.Text.Encoding.UTF8);
            Assert.DoesNotContain("RRfb", sch);
            Assert.DoesNotContain("RRin", sch);
            Assert.DoesNotContain("XSpicySingleOpAmp", sch);
            Assert.DoesNotContain("SUBCKT OPAMP1", sch);
            Assert.Contains("\n* not a real opamp", sch);

            Assert.True(Regex.Match(sch, "\nXComponentAssembly \\d+ \\d+ OPAMPASM").Success);
        }

        [Fact]
        [Trait("Type", "SPICE")]
        public void SpiceAsmModelGeneration_Complex()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            project.PerformInTransaction(abort: true, del: delegate
            {
                var testObj = project.ObjectByPath["/@TestBenches|kind=Testing/@SpiceAsmModelGeneration_Complex"] as MgaFCO;
                Assert.NotNull(testObj);
                var tb = ISIS.GME.Dsml.CyPhyML.Classes.TestBench.Cast(testObj);
                testObj.SetRegistryValueDisp("SpiceFidelitySettings", null);

                string OutputDir = RunSpiceAsmModelGeneration_Complex(TestName);

                string sch = File.ReadAllText(Path.Combine(OutputDir, generatedSpiceTemplateFile), System.Text.Encoding.UTF8);
                Assert.DoesNotContain("RRfb", sch);
                Assert.DoesNotContain("RRin", sch);
                Assert.DoesNotContain("XSpicySingleOpAmp", sch);
                Assert.DoesNotContain("SUBCKT OPAMP1", sch);
                Assert.Contains("\n* not a real opamp", sch);

                Assert.True(Regex.Match(sch, "\nXLevel2_SPICE \\d+ \\d+ OPAMPASM1\\s").Success);
            });

        }

        [Fact]
        [Trait("Type", "SPICE")]
        public void SpiceAsmModelGeneration_Complex_2()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            project.PerformInTransaction(abort: true, del: delegate
            {
                var testObj = project.ObjectByPath["/@TestBenches|kind=Testing/@SpiceAsmModelGeneration_Complex"] as MgaFCO;
                Assert.NotNull(testObj);

                var tb = ISIS.GME.Dsml.CyPhyML.Classes.TestBench.Cast(testObj);
                XElement e = FidelitySelectionRules.CreateForAssembly(tb.Children.TopLevelSystemUnderTestCollection.First().Referred.ComponentAssembly);
                FidelitySelectionRules xpaths = new FidelitySelectionRules();
                xpaths.rules.Add(new FidelitySelectionRules.SelectionRule()
                {
                    xpath = "//ComponentAssembly[@Name='Level4_SPICE']/SpiceModel[@Name='SPICEModel2']",
                    lowest = true
                });
                var elements = FidelitySelectionRules.SelectElements(e, xpaths);
                Assert.Equal(1, elements.Count);

                FidelitySelectionRules.SerializeInRegistry(testObj, xpaths);

                string OutputDir = RunSpiceAsmModelGeneration_Complex(TestName);

                string sch = File.ReadAllText(Path.Combine(OutputDir, generatedSpiceTemplateFile), System.Text.Encoding.UTF8);
                Assert.DoesNotContain("RRfb", sch);
                Assert.DoesNotContain("RRin", sch);
                Assert.DoesNotContain("XSpicySingleOpAmp", sch);
                Assert.DoesNotContain("SUBCKT OPAMP1", sch);
                Assert.Contains("\n.SUBCKT LEVEL4ASM2", sch);

                Assert.True(Regex.Match(sch, "\nXLevel4_SPICE \\d+ \\d+ OPAMPASM2\\s").Success);
            });
        }

        [Fact]
        [Trait("Type", "SPICE")]
        public void SpiceAsmModelGeneration_Complex_SelectClassification()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            project.PerformInTransaction(abort: true, del: delegate
            {
                var testObj = project.ObjectByPath["/@TestBenches|kind=Testing/@SpiceAsmModelGeneration_Complex"] as MgaFCO;
                Assert.NotNull(testObj);

                var tb = ISIS.GME.Dsml.CyPhyML.Classes.TestBench.Cast(testObj);
                XElement e = FidelitySelectionRules.CreateForAssembly(tb.Children.TopLevelSystemUnderTestCollection.First().Referred.ComponentAssembly);
                FidelitySelectionRules xpaths = new FidelitySelectionRules();
                xpaths.rules.Add(new FidelitySelectionRules.SelectionRule()
                {
                    xpath = "//Component[contains(@Classifications, 'passive')]/SpiceModel",
                    lowest = true
                });
                xpaths.rules.Add(new FidelitySelectionRules.SelectionRule()
                {
                    xpath = "//Component[contains(@Classifications, 'active')]/SpiceModel",
                    lowest = true
                });
                var elements = FidelitySelectionRules.SelectElements(e, xpaths);
                Assert.Equal(3, elements.Count);
                /* TODO more test patterns:
                    "//SpiceModel"
                    "//Component[contains(@Classifications, 'a')]/SpiceModel"
                    "//Component/SpiceModel"
                    "//SpiceModel[@Name='SPICEModel1']"
                    "//SpiceModel[count(ancestor::*) > 3]"
                    "//*[@Name='GMEName']/SpiceModel"
                */

                FidelitySelectionRules.SerializeInRegistry(testObj, xpaths);

                string OutputDir = RunSpiceAsmModelGeneration_Complex(TestName);

                string sch = File.ReadAllText(Path.Combine(OutputDir, generatedSpiceTemplateFile), System.Text.Encoding.UTF8);
                Assert.Contains("RRfb", sch);
                Assert.Contains("RRin", sch);
                Assert.Contains("XSpicySingleOpAmp", sch);
                Assert.Contains("SUBCKT OPAMP1", sch);
                Assert.DoesNotContain("\n.SUBCKT LEVEL4ASM2", sch);
            });
        }

        private string RunSpiceAsmModelGeneration_Complex(string OutputDirName)
        {
            string OutputDir = Path.Combine(TestPath, "output", OutputDirName);

            string TestbenchPath = "/@TestBenches|kind=Testing/@SpiceAsmModelGeneration_Complex";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doSpice = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), String.Format("Failed to generate {0}/{1}", OutputDir, generatedSpiceTemplateFile));
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Generated EAGLE schematic (" + generatedSchemaFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Generated layout file (" + generatedLayoutFile + "), but shouldn't have.");
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Failed to generate " + generatedSpiceTemplateFile);
            return OutputDir;
        }

        [Fact]
        public void BasicSchematicGeneration()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/BasicSchematicGeneration|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir, TestbenchPath);

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");
        }

        [Fact]
        public void ChipFit()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/ChipFit|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doChipFit = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");
        }

        [Fact]
        public void PlaceRoute()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/PlaceRoute|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");
        }

        [Fact]
        public void SpacingParameters()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/SchematicSpacing|kind=TestBench|relpos=0";

            var runResult = MasterInterpreterTest.CyPhyMasterInterpreterRunner.RunMasterInterpreterAndReturnResults(fixture.path_MGA,
                                                                                    TestbenchPath,
                                                                                    "/@A_SpiceTests|kind=ComponentAssemblies|relpos=0/@Test1|kind=ComponentAssembly|relpos=0");

            var OutputDir = runResult.OutputDirectory;

            // Verify normal outputs
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), String.Format("Failed to generate {0}/{1}", OutputDir, generatedSchemaFile));
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), String.Format("Failed to generate {0}/{1}", OutputDir, generatedLayoutFile));
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            // Check for spacing parameters in manifest's first execution command.
            var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(OutputDir);
            var invocation = manifest.Steps.First().Invocation;

            Assert.Contains("-i 0.1", invocation);
            Assert.Contains("-e 0.2", invocation);
        }

        [Fact]
        public void MaxRetriesParameter()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string TestbenchPath = "/@TestBenches/@" + TestName + "|kind=TestBench";

            var runResult = MasterInterpreterTest.CyPhyMasterInterpreterRunner.RunMasterInterpreterAndReturnResults(fixture.path_MGA,
                                                                                    TestbenchPath,
                                                                                    "/@A_SpiceTests|kind=ComponentAssemblies|relpos=0/@Test1|kind=ComponentAssembly|relpos=0");

            var OutputDir = runResult.OutputDirectory;

            // Verify normal outputs
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            // Check for spacing parameters in manifest's first execution command.
            var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(OutputDir);
            var invocation = manifest.Steps.First().Invocation;

            Assert.Contains("-s 2000", invocation);
        }

        [Fact]
        public void MultiLayerAttribute()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/MultiLayerAttribute|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            String pathLayoutFile = Path.Combine(OutputDir, generatedLayoutFile);
            Assert.True(File.Exists(pathLayoutFile),
                        "Failed to generate " + generatedLayoutFile);

            // Check layout file for "HasMultiLayerFootprint" indicators
            var jsonLayoutFile = File.ReadAllText(pathLayoutFile);
            var jobjLayoutFile = Newtonsoft.Json.Linq.JObject.Parse(jsonLayoutFile);
            var pkg = jobjLayoutFile["packages"].First(p => p["name"].ToString() == "SpicySingleOpAmp_HasMultiLayer");
            Assert.NotNull(pkg["multiLayer"]);
            Assert.Equal(true, pkg["multiLayer"].ToObject<Boolean>());
        }

        [Fact]
        public void CompHasNoEDAModel()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/CornerCases|kind=Testing|relpos=0/CompHasNoEDAModel|kind=TestBench|relpos=0";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");
        }

        [Fact]
        public void Elaborator_RLC_RefAsOrigin()
        {
            project.PerformInTransaction(abort: true, del: delegate
            {
                string pathComponentAssembly = "/@A_ElaboratorTest|kind=ComponentAssemblies|relpos=0/CA_ElabTest_RLC_RefAsOrigin|kind=ComponentAssembly|relpos=0";
                var objComponentAssembly = project.get_ObjectByPath(pathComponentAssembly);
                Assert.NotNull(objComponentAssembly);

                var asm = CyPhyClasses.ComponentAssembly.Cast(objComponentAssembly);
                Assert.NotNull(asm);

                var relativeLayoutConstraint_RefAsOrigin = asm.Children.RelativeLayoutConstraintCollection.First(rlc => rlc.Name == "RLC__RefAsOrigin");
                Assert.NotNull(relativeLayoutConstraint_RefAsOrigin);

                var CInst__Resistor_R0603 = asm.Children.ComponentCollection.First(c => c.Name == "CInst__Resistor_R0603");
                Assert.NotNull(CInst__Resistor_R0603);


                // Run Elaborator
                var elaborator = new CyPhyElaborateCS.CyPhyElaborateCSInterpreter();
                bool result = elaborator.RunInTransaction(project, asm.Impl as MgaFCO, null, 128);
                Assert.True(result);


                // Formerly ComponentRefs, these objects should now be Component instances
                var CRef__Resistor_R0603 = asm.Children.ComponentCollection.First(c => c.Name == "CRef__Resistor_R0603");
                Assert.NotNull(CInst__Resistor_R0603);

                var CRef__SpicySingleOpAmp = asm.Children.ComponentCollection.First(c => c.Name == "CRef__SpicySingleOpAmp");
                Assert.NotNull(CRef__SpicySingleOpAmp);

                #region Check RLC__RefAsOrigin
                Assert.Equal(1,
                             relativeLayoutConstraint_RefAsOrigin.DstConnections
                                                                  .ApplyRelativeLayoutConstraintCollection
                                                                  .Where(c => c.DstEnd.ID == CInst__Resistor_R0603.ID)
                                                                  .Count());

                Assert.Equal(1,
                             relativeLayoutConstraint_RefAsOrigin.DstConnections
                                                                  .ApplyRelativeLayoutConstraintCollection
                                                                  .Where(c => c.DstEnd.ID == CRef__SpicySingleOpAmp.ID)
                                                                  .Count());

                Assert.Equal(1,
                             relativeLayoutConstraint_RefAsOrigin.SrcConnections
                                                                  .RelativeLayoutConstraintOriginCollection
                                                                  .Where(c => c.SrcEnd.ID == CRef__Resistor_R0603.ID)
                                                                  .Count());
                #endregion

                // Sanity check
                Assert.Equal(1, asm.Children.RelativeLayoutConstraintOriginCollection.Count());
                Assert.Equal(2, asm.Children.ApplyRelativeLayoutConstraintCollection.Count());
            });
        }

        [Fact]
        public void Elaborator_Exact()
        {
            project.PerformInTransaction(abort: true, del: delegate
            {
                string pathComponentAssembly = "/@A_ElaboratorTest|kind=ComponentAssemblies|relpos=0/CA_ElabTest_Exact|kind=ComponentAssembly|relpos=0";
                var objComponentAssembly = project.get_ObjectByPath(pathComponentAssembly);
                Assert.NotNull(objComponentAssembly);

                var asm = CyPhyClasses.ComponentAssembly.Cast(objComponentAssembly);
                Assert.NotNull(asm);

                var exactLayoutConstraint = asm.Children.ExactLayoutConstraintCollection.First();
                Assert.NotNull(exactLayoutConstraint);

                var CInst__Resistor_R0603 = asm.Children.ComponentCollection.First(c => c.Name == "CInst__Resistor_R0603");
                Assert.NotNull(CInst__Resistor_R0603);


                // Run Elaborator
                var elaborator = new CyPhyElaborateCS.CyPhyElaborateCSInterpreter();
                bool result = elaborator.RunInTransaction(project, asm.Impl as MgaFCO, null, 128);
                Assert.True(result);


                // Formerly ComponentRefs, these objects should now be Component instances
                var CRef__SpicySingleOpAmp = asm.Children.ComponentCollection.First(c => c.Name == "CRef__SpicySingleOpAmp");
                Assert.NotNull(CRef__SpicySingleOpAmp);

                #region Check ExactLayoutConstraint
                Assert.Equal(1,
                             exactLayoutConstraint.DstConnections
                                                  .ApplyExactLayoutConstraintCollection
                                                  .Where(c => c.DstEnd.ID == CInst__Resistor_R0603.ID)
                                                  .Count());

                Assert.Equal(1,
                             exactLayoutConstraint.DstConnections
                                                  .ApplyExactLayoutConstraintCollection
                                                  .Where(c => c.DstEnd.ID == CRef__SpicySingleOpAmp.ID)
                                                  .Count());
                #endregion

                // Sanity check
                Assert.Equal(2, asm.Children.ApplyExactLayoutConstraintCollection.Count());
            });
        }

        [Fact]
        public void Elaborator_Range()
        {
            project.PerformInTransaction(abort: true, del: delegate
            {
                string pathComponentAssembly = "/@A_ElaboratorTest|kind=ComponentAssemblies|relpos=0/CA_ElabTest_Range|kind=ComponentAssembly|relpos=0";
                var objComponentAssembly = project.get_ObjectByPath(pathComponentAssembly);
                Assert.NotNull(objComponentAssembly);

                var asm = CyPhyClasses.ComponentAssembly.Cast(objComponentAssembly);
                Assert.NotNull(asm);

                var rangeLayoutConstraint = asm.Children.RangeLayoutConstraintCollection.First();
                Assert.NotNull(rangeLayoutConstraint);

                var CInst__SpicySingleOpAmp = asm.Children.ComponentCollection.First(c => c.Name == "CInst__SpicySingleOpAmp");
                Assert.NotNull(CInst__SpicySingleOpAmp);


                // Run Elaborator
                var elaborator = new CyPhyElaborateCS.CyPhyElaborateCSInterpreter();
                bool result = elaborator.RunInTransaction(project, asm.Impl as MgaFCO, null, 128);
                Assert.True(result);


                // Formerly ComponentRefs, these objects should now be Component instances
                var CRef__Resistor_R0603 = asm.Children.ComponentCollection.First(c => c.Name == "CRef__Resistor_R0603");
                Assert.NotNull(CRef__Resistor_R0603);

                #region Check RangeLayoutConstraint
                Assert.Equal(1,
                             rangeLayoutConstraint.DstConnections
                                                  .ApplyRangeLayoutConstraintCollection
                                                  .Where(c => c.DstEnd.ID == CInst__SpicySingleOpAmp.ID)
                                                  .Count());

                Assert.Equal(1,
                             rangeLayoutConstraint.DstConnections
                                                  .ApplyRangeLayoutConstraintCollection
                                                  .Where(c => c.DstEnd.ID == CRef__Resistor_R0603.ID)
                                                  .Count());
                #endregion

                // Sanity check
                Assert.Equal(2, asm.Children.ApplyRangeLayoutConstraintCollection.Count());
            });
        }

        [Fact]
        public void Elaborator_RLC_InstAsOrigin()
        {
            project.PerformInTransaction(abort: true, del: delegate
            {
                string pathComponentAssembly = "/@A_ElaboratorTest|kind=ComponentAssemblies|relpos=0/CA_ElabTest_RLC_InstAsOrigin|kind=ComponentAssembly|relpos=0";
                var objComponentAssembly = project.get_ObjectByPath(pathComponentAssembly);
                Assert.NotNull(objComponentAssembly);

                var asm = CyPhyClasses.ComponentAssembly.Cast(objComponentAssembly);
                Assert.NotNull(asm);

                var relativeLayoutConstraint_InstAsOrigin = asm.Children.RelativeLayoutConstraintCollection.First(rlc => rlc.Name == "RLC__InstAsOrigin");
                Assert.NotNull(relativeLayoutConstraint_InstAsOrigin);

                var CInst__Resistor_R0603 = asm.Children.ComponentCollection.First(c => c.Name == "CInst__Resistor_R0603");
                Assert.NotNull(CInst__Resistor_R0603);

                var CInst__SpicySingleOpAmp = asm.Children.ComponentCollection.First(c => c.Name == "CInst__SpicySingleOpAmp");
                Assert.NotNull(CInst__SpicySingleOpAmp);


                // Run Elaborator
                var elaborator = new CyPhyElaborateCS.CyPhyElaborateCSInterpreter();
                bool result = elaborator.RunInTransaction(project, asm.Impl as MgaFCO, null, 128);
                Assert.True(result);


                // Formerly ComponentRefs, these objects should now be Component instances
                var CRef__SpicySingleOpAmp = asm.Children.ComponentCollection.First(c => c.Name == "CRef__SpicySingleOpAmp");
                Assert.NotNull(CRef__SpicySingleOpAmp);

                #region Check RLC__InstAsOrigin
                Assert.Equal(1,
                             relativeLayoutConstraint_InstAsOrigin.DstConnections
                                                                  .ApplyRelativeLayoutConstraintCollection
                                                                  .Where(c => c.DstEnd.ID == CInst__Resistor_R0603.ID)
                                                                  .Count());

                Assert.Equal(1,
                             relativeLayoutConstraint_InstAsOrigin.DstConnections
                                                                  .ApplyRelativeLayoutConstraintCollection
                                                                  .Where(c => c.DstEnd.ID == CRef__SpicySingleOpAmp.ID)
                                                                  .Count());

                Assert.Equal(1,
                             relativeLayoutConstraint_InstAsOrigin.SrcConnections
                                                                  .RelativeLayoutConstraintOriginCollection
                                                                  .Where(c => c.SrcEnd.ID == CInst__SpicySingleOpAmp.ID)
                                                                  .Count());
                #endregion

                // Sanity check
                Assert.Equal(1, asm.Children.RelativeLayoutConstraintOriginCollection.Count());
                Assert.Equal(2, asm.Children.ApplyRelativeLayoutConstraintCollection.Count());
            });
        }

        [Fact]
        [Trait("Schematic", "Pre-Routed")]
        public void UsePreRoutedLayout()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_PreRoute";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];
            var spaceClaims = packages.Where(p => p["name"].ToString().Equals("LedDriver")
                                                  && p["package"].ToString().Equals("__spaceClaim__"));
            Assert.Equal(2, spaceClaims.Count());

            var signals = json["signals"];
            var signalsWithWires = signals.Where(s => s["wires"] != null);
            Assert.Equal(2, signalsWithWires.Count());

            foreach (var wires in signalsWithWires.SelectMany(s => s["wires"]))
            {
                Assert.NotNull(wires["x1"]);
                Assert.NotNull(wires["y1"]);
                Assert.NotNull(wires["x2"]);
                Assert.NotNull(wires["y2"]);
                Assert.NotNull(wires["width"]);
                Assert.NotNull(wires["layer"]);
            }
        }

        [Fact]
        [Trait("Schematic", "Pre-Routed")]
        public void RangeSyntaxConversion()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing/AraTestBenches/PR_RangeSyntaxConversion";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];
            var constraints = packages.SelectMany(p => p["constraints"]);
            foreach (var constraint in constraints)
            {
                Assert.DoesNotContain("-", constraint["x"].ToString());
                Assert.DoesNotContain("-", constraint["y"].ToString());
                Assert.DoesNotContain("-", constraint["layer"].ToString());
                Assert.Contains(":", constraint["x"].ToString());
                Assert.Contains(":", constraint["y"].ToString());
                Assert.Contains(":", constraint["layer"].ToString());
            }
        }

        /// <summary>
        /// Test that the "cutouts"-related parameters of a Placement TB make it to layout-input.json
        /// </summary>
        [Fact]
        [Trait("PCB", "Cutouts")]
        public void PCB_Cutouts()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/P_Cutouts";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));
            Assert.NotNull(json["omitBoundary"]);
            Assert.True(json["omitBoundary"].Value<Boolean>());

            var constraints = json["constraints"];

            #region Checking functions for each constraint
            Func<JToken, bool> hasRegion1 = delegate (JToken arr)
            {
                return arr.Any(c => c["type"].ToString().Equals("ex-region") &&
                                    c["x"].Value<String>().Equals("0:2") &&
                                    c["y"].Value<String>().Equals("7:10") &&
                                    c["layer"].Value<int>().Equals(0));
            };
            Func<JToken, bool> hasRegion2 = delegate (JToken arr)
            {
                return arr.Any(c => c["type"].ToString().Equals("ex-region") &&
                                    c["x"].Value<String>().Equals("0:2") &&
                                    c["y"].Value<String>().Equals("7:10") &&
                                    c["layer"].Value<int>().Equals(1));
            };
            Func<JToken, bool> hasRegion3 = delegate (JToken arr)
            {
                return arr.Any(c => c["type"].ToString().Equals("ex-region") &&
                                    c["x"].Value<String>().Equals("18:20") &&
                                    c["y"].Value<String>().Equals("15:18") &&
                                    c["layer"].Value<int>().Equals(0));
            };
            Func<JToken, bool> hasRegion4 = delegate (JToken arr)
            {
                return arr.Any(c => c["type"].ToString().Equals("ex-region") &&
                                    c["x"].Value<String>().Equals("18:20") &&
                                    c["y"].Value<String>().Equals("15:18") &&
                                    c["layer"].Value<int>().Equals(1));
            };
            #endregion

            Assert.True(hasRegion1(constraints));
            Assert.True(hasRegion2(constraints));
            Assert.True(hasRegion3(constraints));
            Assert.True(hasRegion4(constraints));
        }

        [Fact]
        [Trait("Schematic", "Pre-Routed")]
        public void PreRouteTests_SimpleArchetypalHierarchy()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PreRouteTests/SimpleArchetypalHierarchy";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput)); ;

            var packages = json["packages"];
            var pkgIds = packages.Select(p => p["ComponentID"].ToString());
            Assert.True(pkgIds.Distinct().Count() == pkgIds.Count(), "Duplicate package IDs found");

            // LED2 is the south center. Use him as the reference starting point.
            // LED3 is north center
            // LED4 is north west
            // LED1 is north east
            var led2 = packages.First(p => p["name"].ToString().Equals("LED2"));
            var relativeXOrigin = led2["x"].Value<Decimal>();
            var relativeYOrigin = led2["y"].Value<Decimal>();

            // Ensure that the diagram hasn't been rotated
            var rotation = led2["rotation"].Value<int>();
            Assert.Equal(rotation, 0.0);

            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED1", (decimal)(6.7 - 3.5), (decimal)(2.9 - 1));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED3", (decimal)(3.5 - 3.5), (decimal)(4.0 - 1.0));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED4", (decimal)(0.2 - 3.5), (decimal)(4.0 - 1.0));

            // Test the wire too
            var xOffsetWirePoint = (Decimal)(8.6914 - 3.5);
            var yOffsetWirePoint = (Decimal)(5.8292 - 1.0);

            var xExpected = xOffsetWirePoint + relativeXOrigin;
            var yExpected = yOffsetWirePoint + relativeYOrigin;

            // Find a wire object that includes the expected XY point.
            var signals = json["signals"];
            Assert.True(signals.Any(s => s["wires"]
                               .Any(w => (w["x1"].Value<Decimal>() == xExpected && w["y1"].Value<Decimal>() == yExpected)
                                      || (w["x2"].Value<Decimal>() == xExpected && w["y2"].Value<Decimal>() == yExpected))));
        }

        [Fact]
        [Trait("Schematic", "Pre-Routed")]
        public void PreRouteTests_DoubleArchetypalHierarchy()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PreRouteTests/DoubleArchetypalHierarchy";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput)); ;

            var packages = json["packages"];
            var pkgIds = packages.Select(p => p["ComponentID"].ToString());
            Assert.True(pkgIds.Distinct().Count() == pkgIds.Count(), "Duplicate package IDs found");

            // LED2 is the south center. Use him as the reference starting point.
            // LED3 is north center
            // LED4 is north west
            // LED1 is north east
            var led2 = packages.First(p => p["name"].ToString().Equals("LED2"));
            var relativeXOrigin = led2["x"].Value<Decimal>();
            var relativeYOrigin = led2["y"].Value<Decimal>();

            // Ensure that the diagram hasn't been rotated
            var rotation = led2["rotation"].Value<int>();
            Assert.Equal(rotation, 0.0);

            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED1", (decimal)(7.6 - 4.0), (decimal)(2.8 - 1.2));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED3", (decimal)(4.0 - 4.0), (decimal)(4.5 - 1.2));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED4", (decimal)(0.5 - 4.0), (decimal)(4.5 - 1.2));

            // Test the wire too
            var xOffsetWirePoint = (Decimal)(9.5296 - 4.0);
            var yOffsetWirePoint = (Decimal)(6.3626 - 1.2);

            var xExpected = xOffsetWirePoint + relativeXOrigin;
            var yExpected = yOffsetWirePoint + relativeYOrigin;

            // Find a wire object that includes the expected XY point.
            var signals = json["signals"];
            Assert.True(signals.Any(s => s["wires"]
                               .Any(w => (w["x1"].Value<Decimal>() == xExpected && w["y1"].Value<Decimal>() == yExpected)
                                      || (w["x2"].Value<Decimal>() == xExpected && w["y2"].Value<Decimal>() == yExpected))));
        }

        [Fact]
        [Trait("Schematic", "Pre-Routed")]
        public void PreRouteTests_DuplicatedReferentialHierarchy()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PreRouteTests/DuplicatedReferentialHierarchy";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            // LED2 is the south center. Use him as the reference starting point.
            // LED3 is north center
            // LED4 is north west
            // LED1 is north east
            // We need to do this once for each of the duplicated subcircuits
            DuplicatedReferentialHierarchy_TestSubcircuitPositions(json, "{ac441a0e-f111-4ca8-ba4b-796b88eb8432}");
            DuplicatedReferentialHierarchy_TestSubcircuitPositions(json, "{dfd36830-7827-40a6-898f-76a015feb748}");
        }

        private void DuplicatedReferentialHierarchy_TestSubcircuitPositions(JObject json, string subcircuitGUID)
        {
            var packages = json["packages"];
            var pkgIds = packages.Select(p => p["ComponentID"].ToString());
            Assert.True(pkgIds.Distinct().Count() == pkgIds.Count(), "Duplicate package IDs found");

            var led2 = packages.First(p => p["name"].ToString().StartsWith("LED2")
                                        && p["ComponentID"].ToString().StartsWith(subcircuitGUID));
            var relativeXOrigin = led2["x"].Value<Decimal>();
            var relativeYOrigin = led2["y"].Value<Decimal>();

            // Ensure that the diagram hasn't been rotated
            var rotation = led2["rotation"].Value<int>();
            Assert.Equal(rotation, 0.0);

            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED1", (decimal)(6.7 - 3.5), (decimal)(2.9 - 1), subcircuitGUID);
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED3", (decimal)(3.5 - 3.5), (decimal)(4.0 - 1.0), subcircuitGUID);
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED4", (decimal)(0.2 - 3.5), (decimal)(4.0 - 1.0), subcircuitGUID);

            // Test the wire too
            var xOffsetWirePoint = (Decimal)(8.6914 - 3.5);
            var yOffsetWirePoint = (Decimal)(5.8292 - 1.0);

            var xExpected = xOffsetWirePoint + relativeXOrigin;
            var yExpected = yOffsetWirePoint + relativeYOrigin;

            // Find a wire object that includes the expected XY point.
            var signals = json["signals"];
            Assert.True(signals.Any(s => s["wires"]
                               .Any(w => (w["x1"].Value<Decimal>() == xExpected && w["y1"].Value<Decimal>() == yExpected)
                                      || (w["x2"].Value<Decimal>() == xExpected && w["y2"].Value<Decimal>() == yExpected))));
        }

        /* This design has a Component Assembly Ref and a lone Component at the top level.
         * The referenced Component Assembly has pre-layout data.
         * We've stored new layout data at the top level, which should supercede the lower data.
         * The wire between the Component Assembly and the standalone component should also be preserved.
         */
        [Fact]
        [Trait("Schematic", "Pre-Routed")]
        public void PreRouteTests_CARefAndComponent()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PreRouteTests/CARefAndComponent";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput)); ;

            var packages = json["packages"];
            var pkgIds = packages.Select(p => p["ComponentID"].ToString());
            Assert.True(pkgIds.Distinct().Count() == pkgIds.Count(), "Duplicate package IDs found");

            // Ensure that there's only one __spaceClaim__ object.
            Assert.Equal(1, packages.Count(p => p["package"].ToString().Equals("__spaceClaim__")));

            // LED2 is the south center. Use him as the reference starting point.
            // LED3 is north center
            // LED4 is north west
            // LED1 is north east
            var led2 = packages.First(p => p["name"].ToString().Equals("LED2"));
            var relativeXOrigin = led2["x"].Value<Decimal>();
            var relativeYOrigin = led2["y"].Value<Decimal>();

            // Ensure that the diagram hasn't been rotated
            var rotation = led2["rotation"].Value<int>();
            Assert.Equal(rotation, 0.0);

            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED1", (decimal)(7.2 - 3.7), (decimal)(1.4 - 1.2));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED3", (decimal)(3.7 - 3.7), (decimal)(4.2 - 1.2));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED4", (decimal)(0.4 - 3.7), (decimal)(1.8 - 1.2));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "Comp_SOT143", (decimal)(0.88 - 3.7), (decimal)(7.55 - 1.2));

            // Find the wire that joins the floating component to the component assembly.
            // Verify that it has "wire" geometric data.
            // Verify one point that's known from the pre-layout.
            var signals = json["signals"];
            var signal = signals.First(s => s["pins"].ToList<JToken>()
                                                     .Any(p => p["package"].ToString()
                                                                           .Equals("COMP_SOT143")));
            Assert.True(signal["wires"].Count() > 0);

            // Test the wire
            var xOffsetWirePoint = (Decimal)(10.6106 - 3.7);
            var yOffsetWirePoint = (Decimal)(0.8862 - 1.2);

            var xExpected = xOffsetWirePoint + relativeXOrigin;
            var yExpected = yOffsetWirePoint + relativeYOrigin;

            // Find a wire object that includes the expected XY point.
            Assert.True(signal["wires"].ToList().Any(w => (w["x1"].Value<Decimal>() == xExpected && w["y1"].Value<Decimal>() == yExpected)
                                                       || (w["x2"].Value<Decimal>() == xExpected && w["y2"].Value<Decimal>() == yExpected)));
        }

        /* This design has two archetypal Component Assemblies.
         * They each have the exact same layout data, including the same IDs for the elements in their subtrees.
         */
        [Fact]
        [Trait("Schematic", "Pre-Routed")]
        public void PreRouteTests_TwoArchetypalAssemblies()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PreRouteTests/TwoArchetypalAssemblies";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput)); ;

            var packages = json["packages"];
            var pkgIds = packages.Select(p => p["ComponentID"].ToString());
            Assert.True(pkgIds.Distinct().Count() == pkgIds.Count(), "Duplicate package IDs found");

            // Ensure that there's two __spaceClaim__ objects.
            var spaceClaims = packages.Where(p => p["package"].ToString().Equals("__spaceClaim__"));
            Assert.Equal(2, spaceClaims.Count());

            // For each space claim, there should be 4 packages
            foreach (var sc in spaceClaims)
            {
                var idPrefix = sc["ComponentID"] + "{";
                Assert.Equal(4, packages.Count(p => p["ComponentID"].ToString().StartsWith(idPrefix)));
            }
        }

        /* This design has two archetypal Component Assemblies.
         * They each have the exact same layout data, including the same IDs for the elements in their subtrees.
         * There is layout data stored at the top level that should supercede the lower-level layout data.
         */
        [Fact]
        [Trait("Schematic", "Pre-Routed")]
        public void PreRouteTests_TwoArchetypalAssemblies_WithPrelayout()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PreRouteTests/TwoArchetypalAssemblies_WithPrelayout";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput)); ;

            var packages = json["packages"];
            var pkgIds = packages.Select(p => p["ComponentID"].ToString());
            Assert.True(pkgIds.Distinct().Count() == pkgIds.Count(), "Duplicate package IDs found");

            // Ensure that there's two __spaceClaim__ objects.
            var spaceClaims = packages.Where(p => p["package"].ToString().Equals("__spaceClaim__"));
            Assert.Equal(1, spaceClaims.Count());
            var spaceClaim = spaceClaims.First();

            // There should be 8 packages in this spaceclaim
            var idSpaceClaim = spaceClaim["ComponentID"].ToString();
            Assert.Equal(8, packages.Count(p => p["RelComponentID"] != null
                                             && p["RelComponentID"].ToString().Equals(idSpaceClaim)));

            // Test component positions to make sure they're preserved from the top-level layout.
            // LED2 is the south center. Use him as the reference starting point.
            // LED3 is north center
            // LED4 is north west
            // LED1 is north east
            var led2 = packages.First(p => p["name"].ToString().Equals("LED2"));
            var relativeXOrigin = led2["x"].Value<Decimal>();
            var relativeYOrigin = led2["y"].Value<Decimal>();

            // Ensure that the diagram hasn't been rotated
            var rotation = led2["rotation"].Value<int>();
            Assert.Equal(rotation, 0.0);

            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED1", (decimal)(7.5 - 4.2), (decimal)(2.9 - 0.7));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED3", (decimal)(3.7 - 4.2), (decimal)(4.2 - 0.7));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED4", (decimal)(0.4 - 4.2), (decimal)(2.3 - 0.7));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED1$1", (decimal)(16.7 - 4.2), (decimal)(3.6 - 0.7));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED2$1", (decimal)(14.0 - 4.2), (decimal)(1.8 - 0.7));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED3$1", (decimal)(11.4 - 4.2), (decimal)(3.7 - 0.7));
            TestOffset(packages, relativeXOrigin, relativeYOrigin, "LED4$1", (decimal)(11.4 - 4.2), (decimal)(0.4 - 0.7));

            // Test the wire
            var xOffsetWirePoint = (Decimal)(11.5708 - 4.2);
            var yOffsetWirePoint = (Decimal)(8.2056 - 0.7);

            var xExpected = xOffsetWirePoint + relativeXOrigin;
            var yExpected = yOffsetWirePoint + relativeYOrigin;

            // Find a wire object that includes the expected XY point.
            var signals = json["signals"];
            Assert.True(signals.Any(s => s["wires"].Any(w => (w["x1"].Value<Decimal>() == xExpected && w["y1"].Value<Decimal>() == yExpected)
                                                          || (w["x2"].Value<Decimal>() == xExpected && w["y2"].Value<Decimal>() == yExpected))));
        }

        public void TestOffset(
            JToken packages,
            Decimal xOrigin,
            Decimal yOrigin,
            String pkgName,
            Decimal xOffset,
            Decimal yOffset,
            String GUIDPrefix = null)
        {
            JToken pkg;
            if (String.IsNullOrWhiteSpace(GUIDPrefix))
            {
                pkg = packages.First(p => p["name"].ToString().Equals(pkgName));
            }
            else
            {
                pkg = packages.First(p => p["name"].ToString().StartsWith(pkgName)
                                       && p["ComponentID"].ToString().StartsWith(GUIDPrefix));
            }

            var pkgX = pkg["x"].Value<Decimal>();
            var pkgY = pkg["y"].Value<Decimal>();

            Decimal targetX = xOrigin + xOffset;
            Decimal targetY = yOrigin + yOffset;

            Assert.Equal(pkgX, targetX);
            Assert.Equal(pkgY, targetY);
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void ConstrainedAssemblies()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/ConstrainedAssemblies";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            /*
             * The resistor used for these tests is 2.946mm*1.966mm, with its origin at its center point.
             * Therefore, these exact constraints are offset by 1.473mm*0.983mm.
             */
            double componentWidth = 2.8;
            double componentHeight = 1.8;

            #region Checking functions for each constraint
            Func<JToken, bool> hasExact1 = delegate (JToken arr)
            {
                Console.WriteLine("X: {0}, Y: {1}", 1.0 - componentHeight / 2.0, 1.0 - componentWidth / 2.0);
                return arr.Any(c => c["type"].ToString().Equals("exact") &&
                                    Math.Abs(c["y"].Value<Double>() - (1.0 - componentWidth / 2.0)) < 0.0001 && //intentionally flipped due to rotation
                                    Math.Abs(c["x"].Value<Double>() - (1.0 - componentHeight / 2.0)) < 0.0001 &&
                                    c["layer"].Value<int>().Equals(1) &&
                                    c["rotation"].Value<int>().Equals(1));
            };

            Func<JToken, bool> hasExact2 = delegate (JToken arr)
            {
                return arr.Any(c => c["type"].ToString().Equals("exact") &&
                                    c["x"].Value<Double>().Equals(2.0 - componentWidth / 2.0) &&
                                    c["y"].Value<Double>().Equals(2.0 - componentHeight / 2.0) &&
                                    c["layer"].Value<int>().Equals(2) &&
                                    c["rotation"].Value<int>().Equals(2));
            };

            Func<JToken, bool> hasExact3 = delegate (JToken arr)
            {
                return arr.Any(c => c["type"].ToString().Equals("exact") &&
                                    c["y"].Value<Double>().Equals(3.0 - componentWidth / 2.0) && //intentionally flipped due to rotation
                                    c["x"].Value<Double>().Equals(3.0 - componentHeight / 2.0) &&
                                    c["layer"].Value<int>().Equals(3) &&
                                    c["rotation"].Value<int>().Equals(3));
            };

            Func<JToken, bool> hasRange1 = delegate (JToken arr)
            {
                return arr.Any(c => c["type"].ToString().Equals("in-region") &&
                                    c["x"].ToString().Equals("1:1") &&
                                    c["y"].ToString().Equals("1:1") &&
                                    c["layer"].ToString().Equals("1:1"));
            };

            Func<JToken, bool> hasRange2 = delegate (JToken arr)
            {
                return arr.Any(c => c["type"].ToString().Equals("in-region") &&
                                    c["x"].ToString().Equals("2:2") &&
                                    c["y"].ToString().Equals("2:2") &&
                                    c["layer"].ToString().Equals("2:2"));
            };
            #endregion

            // Comp1 should have nothing
            var comp1 = packages.First(p => p["name"].ToString().Equals("Comp1"));
            Assert.False(comp1["constraints"].Any());

            // Comp2 should have Exact2 and Range1
            var comp2 = packages.First(p => p["name"].ToString().Equals("Comp2"));
            var comp2Constraints = comp2["constraints"];
            Assert.Equal(2, comp2Constraints.Count());
            Assert.True(hasExact2(comp2Constraints));
            Assert.True(hasRange1(comp2Constraints));

            // Comp3 should have Exact1
            var comp3 = packages.First(p => p["name"].ToString().Equals("Comp3"));
            var comp3Constraints = comp3["constraints"];
            Assert.Equal(1, comp3Constraints.Count());
            Assert.True(hasExact1(comp3Constraints));

            // Comp4 should have Exact1, Range2, and Exact3
            var comp4 = packages.First(p => p["name"].ToString().Equals("Comp4"));
            var comp4Constraints = comp4["constraints"];
            Assert.Equal(3, comp4Constraints.Count());
            Assert.True(hasExact1(comp4Constraints));
            Assert.True(hasExact3(comp4Constraints));
            Assert.True(hasRange2(comp4Constraints));
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void ConstraintsRelativeToOrigins()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_ConstraintOrigins";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            var fid1 = packages.First(p => p["name"].ToString().Equals("FID1"));
            var fid1XOrigin = fid1["originX"].Value<Double>(); //So we're not hard-coding bounding boxes
            var fid1YOrigin = fid1["originY"].Value<Double>();
            var fid1Width = fid1["width"].Value<Double>();
            var fid1Height = fid1["height"].Value<Double>();
            //Should have single exact constraint at (1, 1) in the GME model; need to adjust for the translation
            //between package origin and lower left corner here
            var fid1Constraints = fid1["constraints"];
            Assert.Equal(1, fid1Constraints.Count());
            var fid1Constraint = fid1Constraints[0];
            Assert.Equal("exact", fid1Constraint["type"].ToString());
            Assert.Equal(1.0 - (fid1Width / 2.0 - fid1XOrigin), fid1Constraint["x"].Value<Double>());
            Assert.Equal(1.0 - (fid1Height / 2.0 - fid1YOrigin), fid1Constraint["y"].Value<Double>());

            var header = packages.First(p => p["name"].ToString().Equals("HEADER"));
            //Get the package index for the HEADER component (just in case it changes)
            var headerIndex = header["pkg_idx"].Value<int>();
            var headerXOrigin = header["originX"].Value<double>();
            var headerYOrigin = header["originY"].Value<double>();
            var headerWidth = header["width"].Value<double>();
            var headerHeight = header["height"].Value<double>();

            var fid2 = packages.First(p => p["name"].ToString().Equals("FID2"));
            var fid2XOrigin = fid2["originX"].Value<Double>(); //So we're not hard-coding bounding boxes
            var fid2YOrigin = fid2["originY"].Value<Double>();
            var fid2Width = fid2["width"].Value<Double>();
            var fid2Height = fid2["height"].Value<Double>();
            //Should have single relative constraint, relative to HEADER
            var fid2Constraints = fid2["constraints"];
            Assert.Equal(1, fid2Constraints.Count());
            var fid2Constraint = fid2Constraints[0];
            Assert.Equal(headerIndex, fid2Constraint["pkg_idx"].Value<int>());
            Assert.Equal(0.1 * Math.Ceiling(10.0 * (0 + (headerWidth / 2.0 - headerXOrigin) - (fid2Width / 2.0 - fid2XOrigin))), fid2Constraint["x"].Value<Double>());
            Assert.Equal(0.1 * Math.Ceiling(10.0 * (-13 + (headerHeight / 2.0 - headerYOrigin) - (fid2Height / 2.0 - fid2YOrigin))), fid2Constraint["y"].Value<Double>());
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void ConstraintRotation()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_ConstraintRotation";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            var rot0 = packages.First(p => p["name"].ToString().Equals("ROT0"));
            var rot0Constraints = rot0["constraints"];
            Assert.Equal(1, rot0Constraints.Count());
            var rot0Constraint = rot0Constraints[0];
            Assert.Equal(22.1, rot0Constraint["x"].Value<double>(), 5);
            Assert.Equal(64.9, rot0Constraint["y"].Value<double>(), 5);

            var rot90 = packages.First(p => p["name"].ToString().Equals("ROT90"));
            var rot90Constraints = rot90["constraints"];
            Assert.Equal(1, rot90Constraints.Count());
            var rot90Constraint = rot90Constraints[0];
            Assert.Equal(64.9, rot90Constraint["x"].Value<double>(), 5);
            Assert.Equal(72.1, rot90Constraint["y"].Value<double>(), 5);

            var rot180 = packages.First(p => p["name"].ToString().Equals("ROT180"));
            var rot180Constraints = rot180["constraints"];
            Assert.Equal(1, rot180Constraints.Count());
            var rot180Constraint = rot180Constraints[0];
            Assert.Equal(73.9, rot180Constraint["x"].Value<double>(), 5);
            Assert.Equal(14.9, rot180Constraint["y"].Value<double>(), 5);

            var rot270 = packages.First(p => p["name"].ToString().Equals("ROT270"));
            var rot270Constraints = rot270["constraints"];
            Assert.Equal(1, rot270Constraints.Count());
            var rot270Constraint = rot270Constraints[0];
            Assert.Equal(14.9, rot270Constraint["x"].Value<double>(), 5);
            Assert.Equal(23.9, rot270Constraint["y"].Value<double>(), 5);

            var led0 = packages.First(p => p["name"].ToString().Equals("LED0"));
            var led0Constraints = led0["constraints"];
            Assert.Equal(1, led0Constraints.Count());
            var led0Constraint = led0Constraints[0];
            Assert.Equal(38.3, led0Constraint["x"].Value<double>(), 5);
            Assert.Equal(73.6, led0Constraint["y"].Value<double>(), 5);

            var led90 = packages.First(p => p["name"].ToString().Equals("LED90"));
            var led90Constraints = led90["constraints"];
            Assert.Equal(1, led90Constraints.Count());
            var led90Constraint = led90Constraints[0];
            Assert.Equal(59.2, led90Constraint["x"].Value<double>(), 5);
            Assert.Equal(73.3, led90Constraint["y"].Value<double>(), 5);

            var led180 = packages.First(p => p["name"].ToString().Equals("LED180"));
            var led180Constraints = led180["constraints"];
            Assert.Equal(1, led180Constraints.Count());
            var led180Constraint = led180Constraints[0];
            Assert.Equal(58.3, led180Constraint["x"].Value<double>(), 5);
            Assert.Equal(24.2, led180Constraint["y"].Value<double>(), 5);

            var led270 = packages.First(p => p["name"].ToString().Equals("LED270"));
            var led270Constraints = led270["constraints"];
            Assert.Equal(1, led270Constraints.Count());
            var led270Constraint = led270Constraints[0];
            Assert.Equal(38.6, led270Constraint["x"].Value<double>(), 5);
            Assert.Equal(23.3, led270Constraint["y"].Value<double>(), 5);
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void ConstraintOnPreRouteExact()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_ConstraintOnPreRouteExact";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            var spaceClaim = packages.First(p => p["pkg_idx"].ToString().Equals("0"));
            var spaceClaimConstraints = spaceClaim["constraints"];
            Assert.Equal(2, spaceClaimConstraints.Count()); //first constraint should be the space claim's layer constraint
            var spaceClaimConstraint = spaceClaimConstraints[1]; //second constraint should be the one we're interested in
            Assert.Equal("exact", spaceClaimConstraint["type"].Value<string>());
            Assert.Equal(6.0, spaceClaimConstraint["x"].Value<double>(), 5);
            Assert.Equal(17.8, spaceClaimConstraint["y"].Value<double>(), 5);
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void ConstraintOnPreRouteRange()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_ConstraintOnPreRouteRange";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            var spaceClaim = packages.First(p => p["pkg_idx"].ToString().Equals("0"));
            var spaceClaimConstraints = spaceClaim["constraints"];
            Assert.Equal(2, spaceClaimConstraints.Count()); //first constraint should be the space claim's layer constraint
            var spaceClaimConstraint = spaceClaimConstraints[1]; //second constraint should be the one we're interested in
            Assert.Equal("in-region", spaceClaimConstraint["type"].Value<string>());
            Assert.Equal("5:20", spaceClaimConstraint["x"].Value<string>());
            Assert.Equal("5:20", spaceClaimConstraint["y"].Value<string>());
            Assert.Equal("0:0", spaceClaimConstraint["layer"].Value<string>());
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void ConstraintOnPreRouteRelative()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_ConstraintOnPreRouteRelative";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            var spaceClaim = packages.First(p => p["pkg_idx"].ToString().Equals("0"));
            var spaceClaimConstraints = spaceClaim["constraints"];
            Assert.Equal(2, spaceClaimConstraints.Count()); //first constraint should be the space claim's layer constraint
            var spaceClaimConstraint = spaceClaimConstraints[1]; //second constraint should be the one we're interested in
            Assert.Equal("relative-pkg", spaceClaimConstraint["type"].Value<string>());
            Assert.Equal(2.5, spaceClaimConstraint["x"].Value<double>(), 5);
            Assert.Equal(3.8, spaceClaimConstraint["y"].Value<double>(), 5);
            Assert.Equal(8, spaceClaimConstraint["pkg_idx"].Value<int>());
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void ConstraintOnPreRouteRelRange()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_ConstraintOnPreRouteRelRange";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            var spaceClaim = packages.First(p => p["pkg_idx"].ToString().Equals("0"));
            var spaceClaimConstraints = spaceClaim["constraints"];
            Assert.Equal(2, spaceClaimConstraints.Count()); //first constraint should be the space claim's layer constraint
            var spaceClaimConstraint = spaceClaimConstraints[1]; //second constraint should be the one we're interested in
            Assert.Equal("relative-region", spaceClaimConstraint["type"].Value<string>());
            Assert.Equal("3:20", spaceClaimConstraint["x"].Value<string>());
            Assert.Equal("3:20", spaceClaimConstraint["y"].Value<string>());
            Assert.Equal(0, spaceClaimConstraint["layer"].Value<int>());
            Assert.Equal(8, spaceClaimConstraint["pkg_idx"].Value<int>());
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void RelativeConstraintRotationInputData()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_RelativeConstraintRotation";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            var pkg = packages.First(p => p["name"].ToString().Equals("TARGET"));
            var constraints = pkg["constraints"];
            Assert.Equal(1, constraints.Count());
            var constraint = constraints[0];
            Assert.Equal("relative-pkg", constraint["type"].Value<string>());
            Assert.Equal(13.8, constraint["x"].Value<double>(), 5);
            Assert.Equal(6.3, constraint["y"].Value<double>(), 5);
            Assert.Equal(6.6, constraint["x1"].Value<double>(), 5);
            Assert.Equal(13.5, constraint["y1"].Value<double>(), 5);
            Assert.Equal(15.6, constraint["x2"].Value<double>(), 5);
            Assert.Equal(6.3, constraint["y2"].Value<double>(), 5);
            Assert.Equal(6.6, constraint["x3"].Value<double>(), 5);
            Assert.Equal(15.3, constraint["y3"].Value<double>(), 5);
            Assert.Equal(0, constraint["pkg_idx"].Value<int>());
        }

        [Fact]
        [Trait("Schematic", "LayoutConstraints")]
        public void RelativeConstraintRelativeRotation()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_RelativeConstraintRelativeRotation";

            RunInterpreterMain(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Load outputs and check the dictionary for expected stuff.
            var nameLayoutInput = "layout-input.json";
            var pathLayoutInput = Path.Combine(OutputDir, nameLayoutInput);
            Assert.True(File.Exists(pathLayoutInput), "Failed to generate layout-input.json");

            var json = JObject.Parse(File.ReadAllText(pathLayoutInput));

            var packages = json["packages"];

            {
                var pkg = packages.First(p => p["name"].ToString().Equals("TARGET0"));
                var constraints = pkg["constraints"];
                Assert.Equal(1, constraints.Count());
                var constraint = constraints[0];
                Assert.Equal("relative-pkg", constraint["type"].Value<string>());
                Assert.Equal(0, constraint["relativeRotation"].Value<int>());
            }

            {
                var pkg = packages.First(p => p["name"].ToString().Equals("TARGET90"));
                var constraints = pkg["constraints"];
                Assert.Equal(1, constraints.Count());
                var constraint = constraints[0];
                Assert.Equal("relative-pkg", constraint["type"].Value<string>());
                Assert.Equal(1, constraint["relativeRotation"].Value<int>());
            }

            {
                var pkg = packages.First(p => p["name"].ToString().Equals("TARGET180"));
                var constraints = pkg["constraints"];
                Assert.Equal(1, constraints.Count());
                var constraint = constraints[0];
                Assert.Equal("relative-pkg", constraint["type"].Value<string>());
                Assert.Equal(2, constraint["relativeRotation"].Value<int>());
            }

            {
                var pkg = packages.First(p => p["name"].ToString().Equals("TARGET270"));
                var constraints = pkg["constraints"];
                Assert.Equal(1, constraints.Count());
                var constraint = constraints[0];
                Assert.Equal("relative-pkg", constraint["type"].Value<string>());
                Assert.Equal(3, constraint["relativeRotation"].Value<int>());
            }

            {
                var pkg = packages.First(p => p["name"].ToString().Equals("TARGETNR"));
                var constraints = pkg["constraints"];
                Assert.Equal(1, constraints.Count());
                var constraint = constraints[0];
                Assert.Equal("relative-pkg", constraint["type"].Value<string>());
                Assert.Null(constraint["relativeRotation"]);
            }
        }

        [Fact]
        public void InvalidAutorouterConfig()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_InvalidAutorouterConfig";

            var result = RunInterpreterMainAndReturnResult(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });
            Assert.False(result.Success); //This testbench should fail
        }

        [Fact]
        public void InvalidBoardTemplate()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_InvalidBoardTemplate";

            var result = RunInterpreterMainAndReturnResult(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });
            Assert.False(result.Success); //This testbench should fail
        }

        [Fact]
        public void InvalidDesignRules()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_InvalidDesignRules";

            var result = RunInterpreterMainAndReturnResult(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });
            Assert.False(result.Success); //This testbench should fail
        }

        [Fact]
        public void MissingComponentSchematic()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_MissingComponentSchematic";

            var result = RunInterpreterMainAndReturnResult(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });
            Assert.False(result.Success); //This testbench should fail
        }

        [Fact]
        public void UnparseableComponentSchematic()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_UnparseableComponentSchematic";

            var result = RunInterpreterMainAndReturnResult(OutputDir,
                                TestbenchPath,
                                new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });
            Assert.False(result.Success); //This testbench should fail
        }

        /// <summary>
        /// Check that place and route can generate a four-layer board. MOT-587
        /// </summary>
        [Fact]
        public void ModelLibraryMismatch()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches/PR_ModelLibraryMismatch";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            Assert.True(File.Exists(Path.Combine(OutputDir, generatedSchemaFile)), "Failed to generate " + generatedSchemaFile);
            Assert.True(File.Exists(Path.Combine(OutputDir, generatedLayoutFile)), "Failed to generate " + generatedLayoutFile);
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceTemplateFile)), "Generated SPICE model (" + generatedSpiceTemplateFile + "), but shouldn't have.");
            Assert.False(File.Exists(Path.Combine(OutputDir, generatedSpiceViewerLauncher)), "Generated " + generatedSpiceTemplateFile + ", but shouldn't have.");

            // At this point, we've created a "layout-input.json" file, and we need to run the "placeonly.bat" file to
            // synthesize a "schema.brd" file.
            var pathBoardFile = RunPlaceOnly(OutputDir);
            // We want BoardSynthesis to succeed; RunPlaceOnly checks for that itself, so we don't need to check again here
        }
    }



    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del, bool abort)
        {
            var mgaGateway = new MgaGateway(project);
            mgaGateway.PerformInTransaction(del, abort: abort);
        }
    }

    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            int ret = Xunit.ConsoleClient.Program.Main(new string[] {
                Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length),
                // [Trait("THIS", "ONE")]
                // "/trait", "THIS=ONE",
                //"/trait", "Type=SPICE", // Do SPICE Tests Only
                //"/noshadow",
            });
            Console.In.ReadLine();
            return ret;
        }
    }
}
