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
    public class UtilitiesTestFixture : SchematicUnitTests.InterpreterFixtureBaseClass
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
        #endregion

        #region AddConnector Tests

        [Fact]
        private void TestSinglePinNoConnector()
        {
            fixture.proj.BeginTransactionInNewTerr();

            try
            {
                var component = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@Component"];
                Assert.Null((MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@Component/@Pin|kind=Connector"]);
                var pin = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@Component/@Pin|kind=SchematicModelPort"];
                var interpreter = new AddConnector.AddConnectorInterpreter();
                var SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs"));
                SelectedFCOs.Append(pin);

                interpreter.Main(fixture.proj, component, SelectedFCOs, AddConnector.AddConnectorInterpreter.ComponentStartMode.GME_SILENT_MODE);

                var connector = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@Component/@Pin|kind=Connector"];
                Assert.Equal(connector.Name, "Pin");
                Assert.Null((MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@Component/@Pin|kind=SchematicModelPort"]);
            }
            finally
            {
                fixture.proj.AbortTransaction();
            }
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
