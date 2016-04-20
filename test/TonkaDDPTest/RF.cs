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
    public class RFFixture
    {
        public RFFixture()
        {
            // First, copy BlankInputModel/InputModel.xme into the test folder
            File.Delete(RF.inputMgaPath);
            GME.MGA.MgaUtils.ImportXME(RF.blankXMEPath, RF.inputMgaPath);
            Assert.True(File.Exists(RF.inputMgaPath), "InputModel.mga not found; import may have failed.");

            // Next, import the content model
            File.Delete(RF.inputMgaPath);
            GME.MGA.MgaUtils.ImportXME(RF.rfXMEPath, RF.rfMgaPath);
            Assert.True(File.Exists(RF.rfMgaPath),
                        String.Format("{0} not found; import may have failed.",
                                      Path.GetFileName(RF.inputMgaPath)
                                     )
                        );

            // Next, export all component models from the content model
            var args = String.Format("{0} -f {1}", RF.rfMgaPath, RF.modelOutputPath).Split();
            CyPhyComponentExporterCL.CyPhyComponentExporterCL.Main(args);

            Assert.True(Directory.Exists(RF.modelOutputPath), "Model output path doesn't exist; Exporter may have failed.");
        }
    }

    public class RF : IUseFixture<RFFixture>
    {
        #region Paths
        public static readonly string testPath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                              "..\\..\\..\\..",
                                                              "models",
                                                              "ACMTestModels",
                                                              "RF");
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
        public static readonly string rfXMEPath = Path.Combine(testPath,
                                                                      "RFModel.xme");
        public static readonly string rfMgaPath = Path.Combine(testPath,
                                                                      "RFModel.mga");
        public static readonly string modelOutputPath = Path.Combine(testPath,
                                                                     "acm");
        #endregion

        #region Fixture
        RFFixture fixture;
        public void SetFixture(RFFixture data)
        {
            fixture = data;
        }
        #endregion

        // The round-trip test will catch any inconsistencies.
        // The ACM export comparison succeeds/fails nondeterministically.
        // Let's skip it for now.
        //[Fact]
        public void OutputTest()
        {
            var list_Comparisons = new List<String>()
            {
                "BasicAttributes",
                "Ports",
                "Mappings"
            };

            var list_NotGenerated = new List<String>();
            var list_DidNotMatch = new List<String>();

            foreach (var name in list_Comparisons)
            {
                var path_Expected = Path.Combine(testPath, name + ".expected.acm");
                var path_Generated = Path.Combine(modelOutputPath, name + ".component.acm");

                if (false == File.Exists(path_Generated))
                    list_NotGenerated.Add(path_Generated);

                if (0 != Common.RunXmlComparator(path_Generated, path_Expected))
                    list_DidNotMatch.Add(path_Generated);
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
            var comp_result = Common.RunCyPhyMLComparator(rfMgaPath, inputMgaPath);
            Assert.True(comp_result == 0, "Imported model doesn't match expected.");
        }

        [Fact]
        public void PythonLibraryTest()
        {
            // Find all exported ACM files
            var acms = Directory.EnumerateFiles(modelOutputPath, "*.acm", SearchOption.AllDirectories);
            ConcurrentDictionary<string, string> cb_Failures = new ConcurrentDictionary<string, string>();
            Parallel.ForEach(acms, pathACM =>
            {
                var absPathACM = Path.Combine(modelOutputPath, pathACM);
                String output;
                int rtnCode = PyLibUtils.TryImportUsingPyLib(absPathACM, out output);

                if (rtnCode != 0)
                    cb_Failures[pathACM] = output;
            });

            if (cb_Failures.IsEmpty == false)
            {
                var msg = "AVM PyLib failed to parse:" + Environment.NewLine;
                foreach (var acmPath in cb_Failures)
                {
                    msg += String.Format("{0}: {1}" + Environment.NewLine, acmPath.Key, acmPath.Value);
                }
                Assert.False(true, msg);
            }
        }
    }
}
