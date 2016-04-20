using System;
using System.IO;
using System.Linq;
using GME.MGA;
using Xunit;
using Eagle = CyPhy2Schematic.Schematic.Eagle;

namespace SchematicUnitTests
{
    
    public class PositionOverlapFixture : InterpreterFixtureBaseClass
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
                                    "PositionOverlap",
                                    "positionoverlap.xme");
            }
        }
    }

    public class PositionOverlap : InterpreterTestBaseClass, IUseFixture<PositionOverlapFixture>
    {
        #region Fixture
        PositionOverlapFixture fixture;
        public void SetFixture(PositionOverlapFixture data)
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
        public void OverlappingPositions()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@Testing/@PlaceAndRoute_1x2";

            RunInterpreterMain(OutputDir,
                               TestbenchPath,
                               new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            /*
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
             * */
        }
    }
}
