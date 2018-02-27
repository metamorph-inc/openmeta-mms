using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using GME.MGA;
using GME.CSharp;
using META;
using CyPhyGUIs;
using System.Diagnostics;


namespace CyPhy2MfgBomTest
{
    public class InterpreterFixture : IDisposable
    {
        public static String path_Test = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                                      "..\\..\\..",
                                                      "CyPhy2MfgBomTest",
                                                      "model");

        private static String path_XME = Path.Combine(path_Test,
                                                      "testmodel.xme");

        public MgaProject proj { get; private set; }

        public InterpreterFixture()
        {
            String mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(path_XME, out mgaConnectionString);
            var path_MGA = mgaConnectionString.Substring("MGA=".Length);

            Assert.True(File.Exists(Path.GetFullPath(path_MGA)),
                        String.Format("{0} not found. Model import may have failed.", path_MGA));

            if (Directory.Exists(Path.Combine(path_Test, "output")))
            {
                Directory.Delete(Path.Combine(path_Test, "output"), true);
            }

            proj = new MgaProject();
            bool ro_mode;
            proj.Open(mgaConnectionString, out ro_mode);
            proj.EnableAutoAddOns(true);
        }

        public void Dispose()
        {
            proj.Save();
            proj.Close();
            proj = null;
        }
    }

    public class Interpreter : IUseFixture<InterpreterFixture>
    {
        #region Fixture
        InterpreterFixture fixture;
        public void SetFixture(InterpreterFixture data)
        {
            fixture = data;
        }
        #endregion

        private MgaProject project
        {
            get
            {
                return fixture.proj;
            }
        }

        private String TestPath
        {
            get
            {
                return InterpreterFixture.path_Test;
            }
        }

        [Fact]
        public void EndToEnd_SingleLevel()
        {
            /* What do we want to do?
             * Let's load the model and run the test bench.
             * Then check the output folder, and load the manifest.
             * From the manifest, find the BOM and Request.
             * Verify that each contains the stuff that we expect.
             * Finally, run the batch file and check that we get a metric.
             */

            var pathTestbench = "/@Testing|kind=Testing|relpos=0/SingleLevel|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 1;

            var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);

            Assert.Equal(3, bom.Parts.Count);
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "SN74S74N"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "2-406549-1"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "ERJ-2GE0R00X"));

            var partLed = bom.Parts.FirstOrDefault(p => p.octopart_mpn == "2-406549-1");
            Assert.Equal(2, partLed.quantity);
            Assert.Equal(2, partLed.instances_in_design.Count);

            var pathBatchFile = Path.Combine(pathOutput,
                                             "runBomCostAnalysis.bat");
            Assert.True(File.Exists(pathBatchFile));

            ////////////// Run the batch file
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + pathBatchFile + "\"");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;
            process.Close();

            Assert.Equal(0, exitCode);

            var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(pathOutput);
            Assert.True(manifest.Metrics
                                .First(m => m.Name == "part_cost_per_design")
                                .Value != null);

            
            ///////// Check for the cost estimation results
            var pathResults = Path.Combine(pathOutput,
                                           manifest.Artifacts
                                                   .First(m => m.Tag == "BomCostAnalysis::CostEstimationResults")
                                                   .Location);
            Assert.True(File.Exists(pathResults));
            var jsonResults = File.ReadAllText(pathResults);
            Assert.False(String.IsNullOrWhiteSpace(jsonResults));

            var results = MfgBom.CostEstimation.CostEstimationResult.Deserialize(jsonResults);
            Assert.False(results.per_design_parts_cost == 0);


            /////////// Check for BOM Table
            var pathBomTable = Path.Combine(pathOutput,
                                            manifest.Artifacts
                                                    .First(m => m.Tag == "BomCostAnalysis::BomTable")
                                                    .Location);
            // pathBomTable is now the full path to the generated BomTable.csv file.
            Assert.True(File.Exists(pathBomTable));

            string[] strBomTable = File.ReadAllLines(pathBomTable);

            // strBomTable is now an array of rows of the CSV table, one array entry per line of text in the CSV file.
            // Check that some changes for MOT-256 seem to be working.

            Assert.Equal(9, strBomTable.Count());    // There should be 10 rows total.
 
            Assert.Equal(",,Design Name:,SingleLevel", strBomTable[0]);  // The title of this BOM CSV, appearing in the third column,  is "SingleLevel".

            // This content will be followed by the actual price.
            // Since that changes over time, we won't check the price itself.
            var expectedContent = ",,Cost per Unit (based on 1 unit):,$";
            Assert.True(strBomTable[2].StartsWith(expectedContent));
        }

        [Fact]
        public void PartPath_MultiLevel()
        {
            var pathTestbench = "/@Testing|kind=Testing|relpos=0/MultiLevel|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 1;

            var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);

            Assert.Equal(1,
                         bom.Parts
                            .Where(p => p.octopart_mpn == "SN74S74N")
                            .SelectMany(p => p.instances_in_design)
                            .Count(ci => ci.path == "flipflop"));
            Assert.Equal(1,
                         bom.Parts
                            .Where(p => p.octopart_mpn == "SN74S74N")
                            .SelectMany(p => p.instances_in_design)
                            .Count(ci => ci.path == "SingleLevel/flipflop"));
            Assert.Equal(1,
                         bom.Parts
                            .Where(p => p.octopart_mpn == "ERJ-2GE0R00X")
                            .SelectMany(p => p.instances_in_design)
                            .Count(ci => ci.path == "SingleLevel/resistor"));
        }

        [Fact]
        public void MultiLevel()
        {
            /* What do we want to do?
             * Let's load the model and run the test bench.
             * Then check the output folder, and load the manifest.
             * From the manifest, find the BOM and Request.
             * Verify that each contains the stuff that we expect.
             */
            
            var pathTestbench = "/@Testing|kind=Testing|relpos=0/MultiLevel|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 1;

            var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);

            Assert.Equal(3, bom.Parts.Count);
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "SN74S74N"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "2-406549-1"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "ERJ-2GE0R00X"));

            var partLed = bom.Parts.FirstOrDefault(p => p.octopart_mpn == "2-406549-1");
            Assert.Equal(2, partLed.quantity);
            Assert.Equal(2, partLed.instances_in_design.Count);

            var partFlipFlop = bom.Parts.FirstOrDefault(p => p.octopart_mpn == "SN74S74N");
            Assert.Equal(2, partFlipFlop.quantity);
            Assert.Equal(2, partFlipFlop.instances_in_design.Count);
        }

        [Fact]
        public void OneMissingMPN()
        {
            var pathTestbench = "/@Testing|kind=Testing|relpos=0/OneMissingMPN|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 1;

            var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);

            Assert.Equal(4, bom.Parts.Count);
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "SN74S74N"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "2-406549-1"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "ERJ-2GE0R00X"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == null));

            var partNoMpm = bom.Parts.FirstOrDefault(p => p.octopart_mpn == null);
            Assert.Equal(1, partNoMpm.quantity);
            Assert.Equal(1, partNoMpm.instances_in_design.Count);
        }

        [Fact]
        public void TwoMissingMPN()
        {
            var pathTestbench = "/@Testing|kind=Testing|relpos=0/TwoMissingMPN|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 1;

            var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);

            Assert.Equal(5, bom.Parts.Count);
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "SN74S74N"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "2-406549-1"));
            Assert.Equal(1, bom.Parts.Count(p => p.octopart_mpn == "ERJ-2GE0R00X"));
            Assert.Equal(2, bom.Parts.Count(p => p.octopart_mpn == null));
        }

        [Fact]
        public void DesignQuantitySetTo9()
        {
            var pathTestbench = "/@Testing|kind=Testing|relpos=0/DesignQuantity9|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 9;

            var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);
        }

        [Fact]
        public void DesignQuantityMissing()
        {
            var pathTestbench = "/@Testing|kind=Testing|relpos=0/DesignQuantityMissing|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 1;

            var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);
        }

        [Fact]
        public void SupplierAffinity()
        {
            var pathTestbench = "/@Testing|kind=Testing|relpos=0/SupplierAffinity|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 1;

            var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);

            // Open request and check for supplier affinity
            var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(pathOutput);
            var pathRequest = Path.Combine(pathOutput,
                                           manifest.Artifacts
                                                   .First(a => a.Tag == "CyPhy2MfgBom::CostEstimationRequest")
                                                   .Location);
            var jsonRequest = File.ReadAllText(pathRequest);
            var request = MfgBom.CostEstimation.CostEstimationRequest.Deserialize(jsonRequest);

            var expectedAffinity = new List<String>()
            {
                "Digi-Key",
                "Mouser",
                "Some Other Co"
            };
            bool somethingMissing = false;
            somethingMissing = somethingMissing || expectedAffinity.Except(request.supplier_affinity).Any();
            somethingMissing = somethingMissing || request.supplier_affinity.Except(expectedAffinity).Any();
            Assert.False(somethingMissing, "The supplier affinity list doesn't match the expected");
        }

        //[Fact]
        public void MetricProvided()
        {
            var pathTestbench = "/@Testing|kind=Testing|relpos=0/MetricProvided|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            int design_quantity = 1;

            // This one needs to run the MasterInterpreter, instead of the usual,
            // since the MasterInterpreter is the one that generates the Metrics
            // in the TB Manifest.

            //var bom = RunInterpreter(pathTestbench, pathOutput, design_quantity);
        }

        private MfgBom.Bom.MfgBom RunInterpreter(string pathTestbench, String pathOutput, int design_quantity)
        {
            if (Directory.Exists(pathOutput))
            {
                Directory.Delete(pathOutput);
            }
            Directory.CreateDirectory(pathOutput);

            CyPhy2MfgBom.CyPhy2MfgBomInterpreter interpreter = null;
            InterpreterMainParameters parameters = null;
            project.PerformInTransaction(delegate
            {
                var objTestbench = project.get_ObjectByPath(pathTestbench);
                Assert.NotNull(objTestbench);

                var testbench = ISIS.GME.Dsml.CyPhyML.Classes.TestBench.Cast(objTestbench);
                Assert.NotNull(testbench);

                interpreter = new CyPhy2MfgBom.CyPhy2MfgBomInterpreter();
                interpreter.Initialize(project);
                parameters = new InterpreterMainParameters()
                {
                    CurrentFCO = testbench.Impl as MgaFCO,
                    SelectedFCOs = null,
                    Project = project,
                    OutputDirectory = pathOutput,
                    ProjectDirectory = project.GetRootDirectoryPath()
                };
            });

            interpreter.Main(parameters);

            // Load manifest
            string pathManifest = Path.Combine(pathOutput, "testbench_manifest.json");
            Assert.True(File.Exists(pathManifest));
            var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(pathOutput);
            Assert.Equal(2, manifest.Artifacts.Count);

            // This metric should be created, whether it's provided or not
            Assert.Equal(1, manifest.Metrics.Count(m => m.Name == "part_cost_per_design"));
            

            // Check batch file
            var pathBatchFile = Path.Combine(pathOutput, "runBomCostAnalysis.bat");
            Assert.True(File.Exists(pathBatchFile));
            var batchContents = File.ReadAllText(pathBatchFile);
            Assert.False(String.IsNullOrWhiteSpace(batchContents));


            // Check request
            var artifactRequest = manifest.Artifacts.FirstOrDefault(a => a.Tag == "CyPhy2MfgBom::CostEstimationRequest");
            Assert.NotNull(artifactRequest);
            var pathRequest = Path.Combine(pathOutput, artifactRequest.Location);
            Assert.True(File.Exists(pathRequest));

            string strRequest = System.IO.File.ReadAllText(pathRequest);
            var request = MfgBom.CostEstimation.CostEstimationRequest.Deserialize(strRequest);
            Assert.NotNull(request);
            Assert.NotNull(request.bom);
            Assert.Equal(design_quantity, request.design_quantity);


            // Check BOM
            var artifactBom = manifest.Artifacts.FirstOrDefault(a => a.Tag == "CyPhy2MfgBom::BOM");
            Assert.NotNull(artifactBom);
            var pathBom = Path.Combine(pathOutput, artifactBom.Location);
            Assert.True(File.Exists(pathBom));

            string strBom = System.IO.File.ReadAllText(pathBom);
            var bom = MfgBom.Bom.MfgBom.Deserialize(strBom);
            Assert.NotNull(bom);
            return bom;
        }
    }

    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del)
        {
            var mgaGateway = new MgaGateway(project);
            mgaGateway.PerformInTransaction(del);
        }
    }
}
