using System;
using System.IO;
using System.Linq;
using GME.MGA;
using Xunit;
using Eagle = CyPhy2Schematic.Schematic.Eagle;

namespace SchematicUnitTests
{
    public class PwrGndPlaneFixture : InterpreterFixtureBaseClass
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
                                    "PwrGndPlane",
                                    "PwrGndPlane.xme");
            }
        }
    }

    public class PwrGndPlane : InterpreterTestBaseClass, IUseFixture<PwrGndPlaneFixture>
    {
        #region Fixture
        PwrGndPlaneFixture fixture;
        public void SetFixture(PwrGndPlaneFixture data)
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

        [Fact]
        public void EndToEnd()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing/@AraTestBenches/@PlaceAndRoute_1x2_4layer";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            var pathBoardFile = RunPlaceOnly(OutputDir);

            var xml = File.ReadAllText(pathBoardFile);
            var eagle = CyPhy2Schematic.Schematic.Eagle.eagle.Deserialize(xml);

            var board = (eagle.drawing.Item as Eagle.board);
            var signals = board.signals.signal;
            Assert.Equal(3, signals.Count);

            Func<String, String, String, String, int, int, bool> verifySignal = 
                delegate(String elementCr1, 
                         String padCr1, 
                         String elementCr2,
                         String padCr2, 
                         int layoutPolygon, 
                         int numVertices)
            {
                return signals.Any(s =>    s.Items.OfType<Eagle.contactref>()
                                                .Any(cr => cr.element.Equals(elementCr1) 
                                                        && cr.pad.Equals(padCr1))
                                        && s.Items.OfType<Eagle.contactref>()
                                                .Any(cr => cr.element.Equals(elementCr2) 
                                                        && cr.pad.Equals(padCr2))
                                        && s.Items.OfType<Eagle.polygon>()
                                                .Any(p => p.layer.Equals(layoutPolygon.ToString())
                                                       && p.vertex.Count.Equals(numVertices)));
            };

            Assert.True(verifySignal("C1", "NEG", "U1", "P$1", 2, 18));
            Assert.True(verifySignal("C1", "POS", "U1", "P$16", 15, 12));
        }

        private void CheckFile(String OutputDir, String Filename)
        {
            Assert.True(File.Exists(Path.Combine(OutputDir, Filename)));
        }

        [Fact]
        public void ReadParamsFromPcbComp()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string TestbenchPath = "/@Testing/@AraTestBenches/@PlaceAndRoute_PcbCompParams";

            CheckResultsUsingPcbTemplate(TestName, TestbenchPath);
        }

        [Fact]
        public void ReadParamsFromPcbComp_AlternateCategory()
        {            
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string TestbenchPath = "/@Testing/@AraTestBenches/@PR_PcbCompParams_AltCategory";
            
            CheckResultsUsingPcbTemplate(TestName, TestbenchPath);
        }

        [Fact]
        public void ReadParamsFromPcbComp_Props()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string TestbenchPath = "/@Testing/@AraTestBenches/@PlaceAndRoute_PcbCompProps";

            CheckResultsUsingPcbTemplate(TestName, TestbenchPath);
        }

        private void CheckResultsUsingPcbTemplate(string TestName, string TestbenchPath)
        {
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            // Check for files specified in PCB Component
            Assert.True(File.Exists(Path.Combine(OutputDir, "4layer_InnerPowerPlanes.brd")));
            var pathAutoRouteFile = Path.Combine(OutputDir, "autoroute.ctl");
            Assert.True(File.Exists(pathAutoRouteFile));
            Assert.True(File.ReadAllText(pathAutoRouteFile).Contains("; TEST-STRING"));
            Assert.True(File.Exists(Path.Combine(OutputDir, "OSHPark-4layer.dru")));

            // Check layout file            
            var pathLayoutFile = Path.Combine(OutputDir, "layout-input.json");
            var jsonLayoutFile = File.ReadAllText(pathLayoutFile);
            var jobjLayoutFile = Newtonsoft.Json.Linq.JObject.Parse(jsonLayoutFile);
            Assert.Equal(40.0, jobjLayoutFile["boardWidth"]);
            Assert.Equal(40.0, jobjLayoutFile["boardHeight"]);
            Assert.Equal("4layer_InnerPowerPlanes.brd", jobjLayoutFile["boardTemplate"]);
            Assert.Equal("OSHPark-4layer.dru", jobjLayoutFile["designRules"]);
        }
    }
}
