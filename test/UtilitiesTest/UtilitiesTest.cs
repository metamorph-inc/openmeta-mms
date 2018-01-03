using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GME.MGA;
using GME.Util;
using GME.MGA.Meta;
using Xunit;
using System.Reflection;
using System.IO;

namespace UtilitiesTest
{
    public class UtilitiesTestFixture : InterpreterFixtureBaseClass
    {
        public override String path_XME
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                    "..\\..\\..\\..",
                    "models",
                    "UtilitiesTest",
                    "UtilitiesTest.xme");
            }
        }
    }

    public class UtilitiesTest : IUseFixture<UtilitiesTestFixture>
    {
        #region Fixture
        UtilitiesTestFixture fixture;
        public void SetFixture(UtilitiesTestFixture data)
        {
            fixture = data;
        }

        /*public override MgaProject project
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
        }*/
        #endregion

        #region AddConnector Tests

        [Fact]
        private void TestSinglePin()
        {
            
        }
        #endregion
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            int ret = Xunit.ConsoleClient.Program.Main(new string[]
            {
                Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length),
                //"/noshadow",
            });
            Console.In.ReadLine();
        }
    }
}
