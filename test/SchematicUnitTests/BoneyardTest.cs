using System;
using System.IO;
using System.Linq;
using GME.MGA;
using Xunit;
using Eagle = CyPhy2Schematic.Schematic.Eagle;

namespace SchematicUnitTests
{
    public class BoneyardFixture : InterpreterFixtureBaseClass
    {
        public override string path_XME
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                    "..\\..\\..\\..",
                                    "test",
                                    "SchematicUnitTests",
                                    "Models",
                                    "Boneyard",
                                    "Boneyard.xme");
            }
        }
    }

    public class Boneyard : InterpreterTestBaseClass, IUseFixture<BoneyardFixture>
    {
        #region Fixture
        BoneyardFixture fixture;
        public void SetFixture(BoneyardFixture data)
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

        private void CheckFile(String OutputDir, String Filename)
        {
            Assert.True(File.Exists(Path.Combine(OutputDir, Filename)), "Didn't find file '" + Filename + "' in directory: " + OutputDir);
        }

        [Fact]
        public void EndToEnd()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.GetFullPath( Path.Combine(TestPath, "output", TestName) );

            string TestbenchPath = "/@TestBenches_MacroFab_2_Layer/@1_PlaceAndRoute_V2";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            var pathBoardFile = RunPlaceOnly(OutputDir);

            // Check that the _partialLayout.txt file exists and contains the expected messages, namely:
            //      Layout was unable to automatically place 'Astable_555_Assembly'.
            //      The best partial layout file was '.\partialLayouts\input-layout009.json'.
            CheckFile(OutputDir, "_partialLayout.txt");
            string pathPartialLayoutTxt = Path.Combine(OutputDir, "_partialLayout.txt");
            string partialLayoutTxt = File.ReadAllText(pathPartialLayoutTxt);
            Assert.Contains("Astable_555_Assembly", partialLayoutTxt);
            Assert.Contains("input-layout009.json", partialLayoutTxt);

            string partialLayoutsDir = Path.Combine(OutputDir, "partialLayouts" );
            CheckFile(partialLayoutsDir, "input-layout000.json");
            CheckFile(partialLayoutsDir, "input-layout001.json");
            CheckFile(partialLayoutsDir, "input-layout015.json");
            CheckFile(partialLayoutsDir, "input-layout016.json");

            // Check that a board file was generated and contains some expected signals
            Assert.Contains(".brd", pathBoardFile);
            var xml = File.ReadAllText(pathBoardFile);
            var eagle = CyPhy2Schematic.Schematic.Eagle.eagle.Deserialize(xml);

            var board = (eagle.drawing.Item as Eagle.board);
            var signals = board.signals.signal;
            Assert.Equal(8, signals.Count);

            Func<String, String, String, String, bool> verifySignal = 
                delegate(String elementCr1, 
                         String padCr1, 
                         String elementCr2,
                         String padCr2)
            {
                bool found1 = signals.Any(
                    // Find a signal in the board where a contact reference matches the first element and pad parameters.
                    s => s.Items.OfType<Eagle.contactref>().Any(cr => cr.element.Equals(elementCr1) && cr.pad.Equals(padCr1))
                );
                Assert.True(found1, "Unable to find a signal in the generated board containing " + elementCr1 + " pin " + padCr1);
                bool found2 = signals.Any(
                    // Find a signal in the board where a contact reference matches the second element and pad parameters.
                    s => s.Items.OfType<Eagle.contactref>().Any(cr => cr.element.Equals(elementCr2) && cr.pad.Equals(padCr2))
                );

                Assert.True(found2, "Unable to find a signal in the generated board containing " + elementCr2 + " pin " + padCr2);

                return signals.Any(
                    // Find a signal in the board where a contact reference matches the first element and pad parameters, and
                    s => s.Items.OfType<Eagle.contactref>().Any( cr => cr.element.Equals(elementCr1) && cr.pad.Equals(padCr1)) &&
                    // a contact reference matches the second element and pad parameters
                    s.Items.OfType<Eagle.contactref>().Any( cr => cr.element.Equals(elementCr2) && cr.pad.Equals(padCr2) )
                );
            };

            Assert.True(verifySignal("CT", "1", "RB", "P$2"), "Missing a signal in board file: " + pathBoardFile);
            Assert.True(verifySignal("LM555D", "P$2", "LM555D", "P$6"), "Missing a signal in board file: " + pathBoardFile);
            Assert.True(verifySignal("LED_G1", "2", "R_1K", "P$2"), "Missing a signal in board file: " + pathBoardFile);
            Assert.True(verifySignal("SW_TOG", "1", "J1_9V", "P$1"), "Missing a signal in board file: " + pathBoardFile);

            // Verify that a PNG file was also created
            CheckFile(OutputDir, "schema.png");
        }
    }
}
