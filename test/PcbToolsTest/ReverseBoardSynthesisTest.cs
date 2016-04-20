using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;
using Newtonsoft.Json;
using LayoutJson;


//----------------------------------------------------------------------------
// ReverseBoardSynthesisTest.cs -- checks reverse board synthesis.
//
// It was derived from BoundingBoxCalc.cs on 31-August-2015 to automatically 
// verify a workaround in CT-188 intended to fix pre-routed signal traces.
//----------------------------------------------------------------------------


namespace PcbToolsTest
{
    public class ReverseBoardSynthesis
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
        public void CheckThatLayoutHasTargetSignalPinCorrect() // Test inspired by CT-188
        {
            #region CheckThatLayoutHasTargetSignalPinCorrect
            var pathTest = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                        "..\\..\\..\\..",
                                        "test",
                                        "PcbToolsTest",
                                        "ReverseBoardSynthesis");

            var filenameLayout = "layout.json";
            var originalLayoutFilepath = Path.Combine(pathTest, "layout - Original Copy.json");
            var targetLayoutFilePath = Path.Combine(pathTest, filenameLayout);
            var filenameSch = "schema.sch";
            var pathBrd = Path.Combine(pathTest, "schema.brd");

            // Verify that the "BoardSynthesis.exe" file exists.
            Assert.True(File.Exists(pathBoardSynthesis), "Couldn't find file at " + pathBoardSynthesis);

            // Verify that the originalLayoutFilepath exists.
            Assert.True(File.Exists(originalLayoutFilepath), "Couldn't find file at " + pathBoardSynthesis);

            File.Copy(originalLayoutFilepath, targetLayoutFilePath, true);  // Overwrite the target "layout.json" file with the original version

            // Verify that the targetLayoutFilePath exists
            Assert.True(File.Exists(targetLayoutFilePath), "Couldn't find file at " + targetLayoutFilePath);

            string args = String.Join(" ", " ", filenameSch, filenameLayout, "-r");
            
            // Run reverse board synthesis to create a new layout.json file.
            using (var proc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                    FileName = pathBoardSynthesis,
                    Arguments = args,
                    WorkingDirectory = pathTest,
                    RedirectStandardError = true,
                    // RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
                proc.Start();
                if (proc.WaitForExit(15000))
                {
                    proc.WaitForExit();
                }
                int exitCode = proc.ExitCode;
                Assert.Equal(0, exitCode);
                Assert.True(0 == proc.ExitCode, proc.StandardError.ToString());
            }

            // Now there should be a new "layout.json" file.

            #region LoadLayout
            // load layout file
            Layout boardLayout = null;

            {
                string sjson = File.ReadAllText(targetLayoutFilePath, Encoding.UTF8);
                boardLayout = JsonConvert.DeserializeObject<Layout>(sjson);

                // Check that signal N$2 has a pin named "VDD" with package "U8" and pad "P$8"
                int targetPinCount = 0;
                foreach (var sig in boardLayout.signals)
                {
                    if( sig.name.Equals("N$2") )
                    {
                        foreach( var pin in sig.pins )
                        {
                            if( pin.package.Equals( "U8" ) && pin.pad.Equals( "P$8" ) && pin.name.Equals( "VDD" ) )
                            {
                                targetPinCount += 1;
                            }
                        }
                    }
                }

                Assert.True((1 == targetPinCount), "Found " + targetPinCount + " pins on signal N$2 named \"VDD\" with package \"U8\" and pad \"P$8\".");
            }
            #endregion            
            #endregion
        }
        [Fact]
        public void CheckThatLayoutHasBoardSizeCorrect()	// MOT-788
        {
            #region CheckThatLayoutHasBoardSizeCorrect
            var pathTest = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                        "..\\..\\..\\..",
                                        "test",
                                        "PcbToolsTest",
                                        "LayoutBoxSize");

            var filenameLayout = "layout.json";
            var originalLayoutFilepath = Path.Combine(pathTest, "layout - Original Copy.json");
            var targetLayoutFilePath = Path.Combine(pathTest, filenameLayout);
            var filenameSch = "schema.sch";
            var pathBrd = Path.Combine(pathTest, "schema.brd");

            // Verify that the "BoardSynthesis.exe" file exists.
            Assert.True(File.Exists(pathBoardSynthesis), "Couldn't find file at " + pathBoardSynthesis);

            // Verify that the originalLayoutFilepath exists.
            Assert.True(File.Exists(originalLayoutFilepath), "Couldn't find file at " + pathBoardSynthesis);

            File.Copy(originalLayoutFilepath, targetLayoutFilePath, true);  // Overwrite the target "layout.json" file with the original version

            // Verify that the targetLayoutFilePath exists
            Assert.True(File.Exists(targetLayoutFilePath), "Couldn't find file at " + targetLayoutFilePath);

            string args = String.Join(" ", " ", filenameSch, filenameLayout, "-r");

            // Run reverse board synthesis to create a new layout.json file.
            using (var proc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                    FileName = pathBoardSynthesis,
                    Arguments = args,
                    WorkingDirectory = pathTest,
                    RedirectStandardError = true,
                    // RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
                proc.Start();
                if (proc.WaitForExit(15000))
                {
                    proc.WaitForExit();
                }
                int exitCode = proc.ExitCode;
                Assert.Equal(0, exitCode);
                Assert.True(0 == proc.ExitCode, proc.StandardError.ToString());
            }

            // Now there should be a new "layout.json" file.

            #region LoadLayout
            // load layout file
            Layout boardLayout = null;

            {
                string sjson = File.ReadAllText(targetLayoutFilePath, Encoding.UTF8);
                boardLayout = JsonConvert.DeserializeObject<Layout>(sjson);

                // Check that in the layout file the board width is 3 mm, and the board height is 7 mm.

                Assert.True(3 == boardLayout.boardWidth);
                Assert.True(7 == boardLayout.boardHeight);
            }
            #endregion
            #endregion
        }

    }
}
