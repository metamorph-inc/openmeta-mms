using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;
using Eagle = CyPhy2Schematic.Schematic.Eagle;

namespace PcbToolsTest
{
    public class BoundingBoxCalc
    {
        public static String pathBoardSynthesis
        {
            get
            {
                string path = Path.Combine(META.VersionInfo.MetaPath,
                                                                    "src",
                                                                    "BoardSynthesis",
                                                                    "bin",
                                                                    "Release",
                                                                    "BoardSynthesis.exe");
                if (File.Exists(path))
                {
                    return path;
                }
                return Path.Combine(META.VersionInfo.MetaPath, "bin", "BoardSynthesis.exe");
            }
        }

        [Fact]
        public void BoundingBoxLayer()
        {
            var pathTest = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                        "..\\..\\..\\..",
                                        "test",
                                        "PcbToolsTest",
                                        "BoundingBox");

            var filenameLayout = "layout.json";
            var filenameSch = "schema.sch";
            var pathBrd = Path.Combine(pathTest, "schema.brd");
            
            using (var proc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = pathBoardSynthesis,
                    Arguments = String.Join(" ", filenameSch, filenameLayout),
                    WorkingDirectory = pathTest,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
                proc.Start();
                Assert.True(proc.WaitForExit(5000));
                Assert.Equal(0, proc.ExitCode);
                Assert.True(0 == proc.ExitCode, proc.StandardError.ToString());
            }

            var xml = File.ReadAllText(pathBrd);
            var eagle = CyPhy2Schematic.Schematic.Eagle.eagle.Deserialize(xml);
            
            // Check that bounding box layers are defined
            Assert.Equal(1, eagle.drawing.layers.layer.Count(l => l.number.Equals("80")
                                                               && l.name.Equals("tBoundingBox")));
            Assert.Equal(1, eagle.drawing.layers.layer.Count(l => l.number.Equals("81")
                                                               && l.name.Equals("bBoundingBox")));

            Func<double, double, double, double, int, bool> verifyWire = delegate(double x1, double y1, double x2, double y2, int layer)
            {
                var matches = (eagle.drawing.Item as Eagle.board)
                                 .plain
                                 .Items
                                 .OfType<Eagle.wire>()
                                 .Where(w => w.x1.Equals(x1.ToString())
                                          && w.y1.Equals(y1.ToString())
                                          && w.x2.Equals(x2.ToString())
                                          && w.y2.Equals(y2.ToString())
                                          && w.layer.Equals(layer.ToString()));
                return matches.Count() == 1;
            };

            Assert.True(verifyWire(0.2, 0.2, 1.2, 0.2, 80));
            Assert.True(verifyWire(1.2, 0.2, 1.2, 1.2, 80));
            Assert.True(verifyWire(0.2, 1.2, 1.2, 1.2, 80));
            Assert.True(verifyWire(0.2, 0.2, 0.2, 1.2, 80));

            Assert.True(verifyWire(3.2, 0.2, 4.2, 0.2, 81));
            Assert.True(verifyWire(4.2, 0.2, 4.2, 1.2, 81));
            Assert.True(verifyWire(3.2, 1.2, 4.2, 1.2, 81));
            Assert.True(verifyWire(3.2, 0.2, 3.2, 1.2, 81));

        }
    }
}
