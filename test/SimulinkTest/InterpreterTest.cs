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
        public void TestHierarchy()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                "output",
                TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@Success|kind=Testing|relpos=0/TB_Hierarchy|kind=TestBench|relpos=0";

            var result = RunInterpreterMainAndReturnResult(OutputDir, TestbenchPath);

            Assert.True(result.Success);

            //TODO: Verify that we created all the files we were supposed to
        }

        [Fact]
        public void TestEmptyComponentName()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string OutputDir = Path.Combine(TestPath,
                "output",
                TestName);

            string TestbenchPath = "/@Testing|kind=Testing|relpos=0/@Fail_Interpreter|kind=Testing|relpos=0/TB_FAIL_EmptyComponentName|kind=TestBench|relpos=0";

            var result = RunInterpreterMainAndReturnResult(OutputDir, TestbenchPath);

            Assert.False(result.Success);
        }
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
