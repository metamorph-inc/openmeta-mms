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
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;

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
                var component = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@SinglePin"];
                Assert.Null((MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@SinglePin/@Pin|kind=Connector"]);
                var pin = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@SinglePin/@Pin|kind=SchematicModelPort"];
                var interpreter = new AddConnector.AddConnectorInterpreter();
                var SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs"));
                SelectedFCOs.Append(pin);

                interpreter.Main(fixture.proj, component, SelectedFCOs, AddConnector.AddConnectorInterpreter.ComponentStartMode.GME_SILENT_MODE);

                var connector = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@SinglePin/@Pin|kind=Connector"];
                Assert.Equal("Pin", connector.Name);
                Assert.Null((MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@SinglePin/@Pin|kind=SchematicModelPort"]);
                Assert.Equal(1, component.ChildObjects.Count);
            }
            finally
            {
                fixture.proj.AbortTransaction();
            }
        }

        [Fact]
        private void TestFivePinsNoConnector()
        {
            fixture.proj.BeginTransactionInNewTerr();

            try
            {
                var component = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@FivePins"];
                var SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs"));
                foreach (MgaFCO child in component.ChildObjects)
                {
                    Assert.Equal("SchematicModelPort", child.Meta.Name);
                    SelectedFCOs.Append(child);
                }

                var interpreter = new AddConnector.AddConnectorInterpreter();
                interpreter.Main(fixture.proj, component, SelectedFCOs, AddConnector.AddConnectorInterpreter.ComponentStartMode.GME_SILENT_MODE);

                foreach (MgaFCO child in component.ChildObjects)
                {
                    Assert.Equal("Connector", child.Meta.Name);
                }
            }
            finally
            {
                fixture.proj.AbortTransaction();
            }
        }

        [Fact]
        private void TestConnectorWithPins()
        {
            fixture.proj.BeginTransactionInNewTerr();

            try
            {
                var component = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@ConnectorWithPins"];
                var SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs"));
                foreach (MgaFCO child in component.ChildObjects)
                {
                    SelectedFCOs.Append(child);
                }
                Assert.Equal(4, component.ChildObjects.Count);

                var interpreter = new AddConnector.AddConnectorInterpreter();
                interpreter.Main(fixture.proj, component, SelectedFCOs, AddConnector.AddConnectorInterpreter.ComponentStartMode.GME_SILENT_MODE);

                Assert.Equal(1, component.ChildObjects.Count);
                foreach (MgaFCO child in component.ChildObjects)
                {
                    Assert.Equal("Connector", child.Meta.Name);
                }
            }
            finally
            {
                fixture.proj.AbortTransaction();
            }
        }

        [Fact]
        private void TestNoPinsTwoConnectors()
        {
            fixture.proj.BeginTransactionInNewTerr();

            try
            {
                var component = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@TwoConnectors"];
                var SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs"));
                foreach (MgaFCO child in component.ChildObjects)
                {
                    Assert.Equal(child.Meta.Name, "Connector");
                    SelectedFCOs.Append(child);
                }

                var interpreter = new AddConnector.AddConnectorInterpreter();
                interpreter.Main(fixture.proj, component, SelectedFCOs, AddConnector.AddConnectorInterpreter.ComponentStartMode.GME_SILENT_MODE);

                Assert.Equal(1, component.ChildObjects.Count);
                Assert.Equal("SCL_SDA", ((MgaFCO)component.ChildObjects[1]).Name); // FIX(tthomas): Why object [1]?
            }
            finally
            {
                fixture.proj.AbortTransaction();
            }
        }

        [Fact]
        private void TestMerge()
        {
            fixture.proj.BeginTransactionInNewTerr();

            try
            {
                var component = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@Merge"];
                var SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs"));
                foreach (MgaFCO child in component.ChildObjects)
                {
                    SelectedFCOs.Append(child);
                }
                CyPhy.Component comp = CyPhyClasses.Component.Cast(component);
                Assert.Equal(3, comp.Children.SchematicModelPortCollection.Count());
                Assert.Equal(4, comp.Children.ConnectorCollection.Count());

                var interpreter = new AddConnector.AddConnectorInterpreter();
                interpreter.Main(fixture.proj, component, SelectedFCOs, AddConnector.AddConnectorInterpreter.ComponentStartMode.GME_SILENT_MODE);
                
                Assert.Equal(0, comp.Children.SchematicModelPortCollection.Count());
                Assert.Equal(5, comp.Children.ConnectorCollection.Count());

                // Check GND Connector to confirm ports were merged correctly.
                var gnd_connector = comp.Children.ConnectorCollection.Where(a => a.Name == "GND").First();
                Assert.Equal(1, gnd_connector.AllChildren.Count());
                Assert.Equal(1, gnd_connector.Children.SchematicModelPortCollection.Count());

                // Make sure GND port in GND connector connects to both the EDA and SPICE models.
                var gnd_port = gnd_connector.AllChildren.First();
                var gnd_connections = comp.Children.PortCompositionCollection.Where(a => a.SrcEnd.Name == "GND" || a.DstEnd.Name == "GND");
                Assert.Equal(2, gnd_connections.Count());

                var ends_pairs = gnd_connections.Select(a => new string [] { a.SrcEnd.ParentContainer.Kind, a.DstEnd.ParentContainer.Kind });
                foreach (var ends_pair in ends_pairs)
                {
                    Assert.True(ends_pair.Contains("Connector"));
                    Assert.True(ends_pair.Contains("SPICEModel") || ends_pair.Contains("EDAModel"));
                }

            }
            finally
            {
                fixture.proj.AbortTransaction();
            }
        }

        [Fact]
        private void TestEagleImport()
        {
            fixture.proj.BeginTransactionInNewTerr();

            try
            {
                var component = (MgaFCO)fixture.proj.RootFolder.ObjectByPath["/@Components/@EagleImport"];
                var SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs"));
                var pin_count = 0;
                foreach (MgaFCO child in component.ChildObjects)
                {
                    if (child.Meta.Name == "SchematicModelPort")
                        pin_count++;
                    Assert.False(child.Meta.Name == "Connector");
                    SelectedFCOs.Append(child);
                }
                Assert.Equal(4, pin_count);

                var interpreter = new AddConnector.AddConnectorInterpreter();
                interpreter.Main(fixture.proj, component, SelectedFCOs, AddConnector.AddConnectorInterpreter.ComponentStartMode.GME_SILENT_MODE);

                var connector_count = 0;
                foreach (MgaFCO child in component.ChildObjects)
                {
                    if (child.Meta.Name == "Connector")
                        connector_count++;
                    Assert.False(child.Meta.Name == "SchematicModelPort");
                }
                Assert.Equal(4, connector_count);
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
