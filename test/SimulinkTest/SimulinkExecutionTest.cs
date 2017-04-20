using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GME.MGA;
using SchematicUnitTests;
using Xunit;

namespace SimulinkTest
{
    public class SimulinkExecutionTest : InterpreterTestBaseClass, IUseFixture<InterpreterTestFixture>
    {
        #region Fixture
        InterpreterTestFixture fixture;
        public void SetFixture(InterpreterTestFixture data)
        {
            fixture = data;
        }

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
        #endregion

        [SkipWithoutMatlabFact]
        public void TestExecuteDuplicateComponentName()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("DuplicateComponentName");
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteHierarchy()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("Hierarchy");
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteHierarchyWithOutput()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("HierarchyWithOutput");

            AssertFileExists(outputDir, "convertmat.m"); //Created by CopyFile directive
            AssertFileExists(outputDir, "ComputeMetrics.py"); //Created by PostProcessing directive

            AssertFileExists(outputDir, "output.mat");
            AssertFileExists(outputDir, "output.csv");
            //TODO: verify that TB manifest has our result populated
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteHierarchyWithOutputForPet()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("HierarchyWithOutputForPET");

            AssertFileExists(outputDir, "convertmat.m"); //Created by CopyFile directive
            AssertFileExists(outputDir, "ComputeMetrics.py"); //Created by PostProcessing directive

            AssertFileExists(outputDir, "output.mat");
            AssertFileExists(outputDir, "output.csv");
            //TODO: verify that TB manifest has our result populated
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteMultiEndpoint()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("Multi-endpoint");
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteMultiInstance()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("Multi-instance");
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteMultipleHierarchy()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("MultipleHierarchy");
        }

        [SkipWithoutMatlabFact]
        public void TestExecutePIDControllerReference()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("PIDControllerReference");
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteUserComponent()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionSucceeds("UserComponent");
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteBadBlockParameter()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionFails("FAIL_BadBlockParameter");
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteBadBlockParameterValue()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionFails("FAIL_BadBlockParameterValue");
        }

        [SkipWithoutMatlabFact]
        public void TestExecuteBadModelParameter()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var outputDir = AssertSimulinkExecutionFails("FAIL_BadModelParameter");
        }

        private string AssertSimulinkExecutionSucceeds(string testBenchName)
        {
            string testBenchPath =
                string.Format("/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_{0}|kind=TestBench|relpos=0", testBenchName);
            string configuration =
                string.Format("/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@{0}|kind=ComponentAssembly|relpos=0", testBenchName);

            //We need the master interpreter here--  actually executing the generated Simulink script needs the 
            //TB manifest to be present, which is generated by the MI
            var runResult =
                MasterInterpreterTest.CyPhyMasterInterpreterRunner.RunMasterInterpreterAndReturnResults(fixture.path_MGA,
                    testBenchPath, configuration);

            var outputDir = runResult.OutputDirectory;

            Assert.True(runResult.Success);
            AssertCommonSimulinkFilesGenerated(outputDir);

            var returnCode = RunSimulinkGen(outputDir);
            Assert.Equal(0, returnCode);
            AssertSimulinkModelGenerated(outputDir);
            return outputDir;
        }

        private string AssertSimulinkExecutionFails(string testBenchName)
        {
            string testBenchPath =
                string.Format("/@Testing|kind=Testing|relpos=0/@Fail_Simulink|kind=Testing|relpos=0/TB_{0}|kind=TestBench|relpos=0", testBenchName);
            string configuration =
                string.Format("/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@{0}|kind=ComponentAssembly|relpos=0", testBenchName);

            //We need the master interpreter here--  actually executing the generated Simulink script needs the 
            //TB manifest to be present, which is generated by the MI
            var runResult =
                MasterInterpreterTest.CyPhyMasterInterpreterRunner.RunMasterInterpreterAndReturnResults(fixture.path_MGA,
                    testBenchPath, configuration);

            var outputDir = runResult.OutputDirectory;

            Assert.True(runResult.Success);
            AssertCommonSimulinkFilesGenerated(outputDir);

            var returnCode = RunSimulinkGen(outputDir);
            Assert.NotEqual(0, returnCode);
            return outputDir;
        }

        private void AssertSimulinkModelGenerated(string outputDir)
        {
            AssertFileExists(outputDir, "build_simulink.m");
            AssertFileExists(outputDir, "newModel.slx");
        }
    }
}
