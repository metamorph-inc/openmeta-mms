using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GME.MGA;
using SchematicUnitTests;
using Xunit;

namespace SimulinkTest
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
                    "SimulinkTestModel",
                    "SimulinkTestModel.xme");
            }
        }
    }

    public class InterpreterTest : InterpreterTestBaseClass, IUseFixture<InterpreterTestFixture>
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

        #region Testbenches where interpreter should succeed

        [Fact]
        public void TestHierarchy()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            AssertTestBenchSucceeds(testName, "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_Hierarchy|kind=TestBench|relpos=0");
        }

        [Fact]
        public void TestHierarchyWithOutput()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string outputDir;

            AssertTestBenchSucceeds(testName, "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_HierarchyWithOutput|kind=TestBench|relpos=0", out outputDir);
            AssertFileExists(outputDir, "convertmat.m"); //Created by CopyFile directive
            AssertFileExists(outputDir, "ComputeMetrics.py"); //Created by PostProcessing directive
        }

        [Fact]
        public void TestHierarchyWithOutputForPet()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string outputDir;

            AssertTestBenchSucceeds(testName, "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_HierarchyWithOutputForPET|kind=TestBench|relpos=0", out outputDir);
            AssertFileExists(outputDir, "convertmat.m"); //Created by CopyFile directive
            AssertFileExists(outputDir, "ComputeMetrics.py"); //Created by PostProcessing directive
        }

        [Fact]
        public void TestMultiEndpoint()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            AssertTestBenchSucceeds(testName, "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_Multi-endpoint|kind=TestBench|relpos=0");
        }

        [Fact]
        public void TestMultiInstance()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            AssertTestBenchSucceeds(testName, "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_Multi-instance|kind=TestBench|relpos=0");
        }

        [Fact]
        public void TestMultipleHierarchy()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            AssertTestBenchSucceeds(testName, "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_MultipleHierarchy|kind=TestBench|relpos=0");
        }

        [Fact]
        public void TestPidControllerReference()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string outputDir;

            AssertTestBenchSucceeds(testName, "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_PIDControllerReference|kind=TestBench|relpos=0", out outputDir);
            AssertFileExists(outputDir, "my_library.slx"); //Created by CopyFile directive
        }

        [Fact]
        public void TestUserComponent()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string outputDir;

            AssertTestBenchSucceeds(testName, "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_UserComponent|kind=TestBench|relpos=0", out outputDir);
            AssertFileExists(outputDir, "my_library.slx"); //Created by UserLibrary directive
        }

        #endregion

        #region Testbenches where interpeter should fail

        [Fact]
        public void TestEmptyComponentName()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            AssertTestBenchFails(testName, "/@Testing|kind=Testing|relpos=0/@Fail_Interpreter|kind=Testing|relpos=0/TB_FAIL_EmptyComponentName|kind=TestBench|relpos=0");
        }

        [Fact]
        public void TestEmptyPortId()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            AssertTestBenchFails(testName, "/@Testing|kind=Testing|relpos=0/@Fail_Interpreter|kind=Testing|relpos=0/TB_FAIL_EmptyPortID|kind=TestBench|relpos=0");
        }

        [Fact]
        public void TestWhitespaceComponentName()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            AssertTestBenchFails(testName, "/@Testing|kind=Testing|relpos=0/@Fail_Interpreter|kind=Testing|relpos=0/TB_FAIL_WhitespaceComponentName|kind=TestBench|relpos=0");
        }

        [Fact]
        public void TestWhitespacePortId()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            AssertTestBenchFails(testName, "/@Testing|kind=Testing|relpos=0/@Fail_Interpreter|kind=Testing|relpos=0/TB_FAIL_WhitespacePortID|kind=TestBench|relpos=0");
        }

        #endregion
    }

    public class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            int ret = Xunit.ConsoleClient.Program.Main(new string[] {
                Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length),
                // [Trait("THIS", "ONE")]
                // "/trait", "THIS=ONE",
                //"/noshadow",
            });
            Console.In.ReadLine();
            return ret;
        }
    }
}
