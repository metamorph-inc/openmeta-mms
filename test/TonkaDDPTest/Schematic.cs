using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;
using GME.CSharp;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ComponentImporterUnitTests;

namespace TonkaACMTest
{
    public class SchematicFixture
    {
        public SchematicFixture()
        {
            // First, copy BlankInputModel/InputModel.xme into the test folder
            File.Delete(Schematic.inputMgaPath);
            GME.MGA.MgaUtils.ImportXME(Schematic.blankXMEPath, Schematic.inputMgaPath);
            Assert.True(File.Exists(Schematic.inputMgaPath), "InputModel.mga not found; import may have failed.");

            // Next, import the content model
            File.Delete(Schematic.schematicMgaPath);
            GME.MGA.MgaUtils.ImportXME(Schematic.schematicXMEPath, Schematic.schematicMgaPath);
            Assert.True(File.Exists(Schematic.schematicMgaPath), "SchematicModel.mga not found; import may have failed.");

            // Delete the ACM output path.
            if (Directory.Exists(Schematic.modelOutputPath))
                Directory.Delete(Schematic.modelOutputPath, true);

            // Next, export all component models from the content model
            var args = new String[] { Schematic.schematicMgaPath, "-f", Schematic.modelOutputPath };
            CyPhyComponentExporterCL.CyPhyComponentExporterCL.Main(args);

            Assert.True(Directory.Exists(Schematic.modelOutputPath), "Model output path doesn't exist; Exporter may have failed.");
            success = true;
        }

        public Boolean success = false;
    }
    public class Schematic : IUseFixture<SchematicFixture>
    {
        #region Paths
        public static readonly string testPath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                              "..\\..\\..\\..",
                                                              "models",
                                                              "ACMTestModels",
                                                              "Schematic");
        public static readonly string blankXMEPath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                                  "..\\..\\..\\..",
                                                                  "test",
                                                                  "InterchangeTest",
                                                                  "ComponentInterchangeTest",
                                                                  "SharedModels",
                                                                  "BlankInputModel",
                                                                  "InputModel.xme");
        public static readonly string inputMgaPath = Path.Combine(testPath,
                                                                  "InputModel.mga");
        public static readonly string schematicXMEPath = Path.Combine(testPath,
                                                                      "SchematicModel.xme");
        public static readonly string schematicMgaPath = Path.Combine(testPath,
                                                                      "SchematicModel.mga");
        public static readonly string modelOutputPath = Path.Combine(testPath,
                                                                     "acm");
        #endregion

        #region Fixture
        SchematicFixture fixture;
        public void SetFixture(SchematicFixture data)
        {
            fixture = data;
        }
        #endregion
        
        [Fact]
        public void OutputTest()
        {
            var list_Comparisons = new List<String>()
            {
                "EDA_BasicAttributes",
                "EDA_PinsAndParameters",
                "EDA_Mappings",
                "SPICE_BasicAttributes",
                "SPICE_PinsAndParameters",
                "SPICE_Mappings",
                "EDA2CAD_Mapping"
            };

            var list_NotGenerated = new List<String>();
            var list_DidNotMatch = new List<String>();

            foreach (var name in list_Comparisons)
            {
                var path_Expected = Path.Combine(testPath, name + ".expected.acm");
                var path_Generated = Path.Combine(modelOutputPath, name + ".component.acm");

                if (false == File.Exists(path_Generated))
                {
                    list_NotGenerated.Add(path_Generated);
                    continue;
                }

                if (0 != Common.RunXmlComparator(path_Generated, path_Expected))
                {
                    list_DidNotMatch.Add(path_Generated);
                }
            }

            if (list_NotGenerated.Any())
            {
                String failed = "";
                list_NotGenerated.ForEach(x => failed += "\n" + x);
                Assert.True(false, "These expected files weren't generated: " + failed);
            }
            if (list_DidNotMatch.Any())
            {
                String failed = "";
                list_DidNotMatch.ForEach(x => failed += "\n" + x);
                Assert.True(false, "These generated files didn't match expected: " + failed);
            }
        }
        
        [Fact]
        public void RoundTripTest()
        {
            // Run Importer
            var args = String.Format("-r {0} {1}", modelOutputPath, inputMgaPath).Split();
            var importer_result = CyPhyComponentImporterCL.CyPhyComponentImporterCL.Main(args);
            Assert.True(0 == importer_result, "Importer had non-zero return code.");

            // Compare
            var comp_result = Common.RunCyPhyMLComparator(schematicMgaPath, inputMgaPath);
            Assert.True(comp_result == 0, "Imported model doesn't match expected.");
        }

        [Fact]
        public void PythonLibraryTest()
        {
            // Find all exported ACM files
            var acms = Directory.EnumerateFiles(modelOutputPath, "*.acm", SearchOption.AllDirectories);
            ConcurrentBag<String> cb_Failures = new ConcurrentBag<String>();
            Parallel.ForEach(acms, pathACM =>
            {
                var absPathACM = Path.Combine(modelOutputPath, pathACM);
                String output;
                int rtnCode = PyLibUtils.TryImportUsingPyLib(absPathACM, out output);

                if (rtnCode != 0)
                {
                    cb_Failures.Add(String.Format("{1}: {0}{2}", Environment.NewLine, pathACM, output));
                }
            });

            if (cb_Failures.Any())
            {
                var msg = "AVM PyLib failed to parse:" + Environment.NewLine;
                msg += String.Join(Environment.NewLine + Environment.NewLine, cb_Failures);
                Assert.False(true, msg);
            }
        }
    }
}
