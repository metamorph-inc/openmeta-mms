using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;
using GME.MGA;

namespace DesignExporterUnitTests
{
    public class TonkaFixture : ExporterFixture
    {
        public override String pathXME
        {
            get
            {
                return Path.Combine(META.VersionInfo.MetaPath,
                                    "test",
                                    "InterchangeTest",
                                    "DesignInterchangeTest",
                                    "ExportTestModels",
                                    "Tonka",
                                    "Tonka.xme");
            }
        }
    }

    public class Tonka : IUseFixture<TonkaFixture>
    {
        #region Fixture
        TonkaFixture fixture;
        public void SetFixture(TonkaFixture data)
        {
            fixture = data;
        }
        #endregion

        private MgaProject proj { get { return fixture.proj; } }

        [Fact]
        [Trait("Interchange", "Tonka")]
        public void SystemC()
        {
            TestPortsAndConnections<avm.systemc.SystemCPort>();
        }

        [Fact]
        [Trait("Interchange", "Tonka")]
        public void SchematicPin()
        {
            TestPortsAndConnections<avm.schematic.Pin>();
        }

        
        [Fact]
        [Trait("Interchange", "Tonka")]
        public void RFPort()
        {
            TestPortsAndConnections<avm.rf.RFPort>();
        }        

        private void TestPortsAndConnections<T>() where T : avm.Port
        {
            String pathCA = "ca/Tonka";
            avm.Design design = fixture.Convert(pathCA);

            var rc = design.RootContainer;
            Assert.NotNull(rc);

            #region get components and containers
            var topconnector = rc.Connector.First(c => c.Name.Equals("topconnector"));
            var comp1 = rc.ComponentInstance.First(ci => ci.Name.Equals("comp1"));
            var comp2 = rc.ComponentInstance.First(ci => ci.Name.Equals("comp2"));
            var subasm = rc.Container1.OfType<avm.Compound>().First();
            var comp3 = subasm.ComponentInstance.First();
            #endregion

            var domainPortTypeName = typeof(T).Name;

            #region get ports
            var top_domainport = rc.Port.OfType<T>().First();
            var topconnector_domainport = topconnector.Role.OfType<T>().First();
            var comp1_domainport = comp1.PortInstance.First(pi => pi.IDinComponentModel.Equals(domainPortTypeName));
            var comp2_domainport = comp2.PortInstance.First(pi => pi.IDinComponentModel.Equals(domainPortTypeName));
            var subasm_domainport = subasm.Port.OfType<T>().First();
            var comp3_domainport = comp3.PortInstance.First(pi => pi.IDinComponentModel.Equals(domainPortTypeName));
            #endregion
            
            #region assert connections
            top_domainport.IsConnectedTo(topconnector_domainport);
            top_domainport.IsConnectedTo(subasm_domainport);

            topconnector_domainport.IsConnectedTo(comp1_domainport);

            comp1_domainport.IsConnectedTo(comp2_domainport);
            comp1_domainport.IsConnectedTo(subasm_domainport);

            subasm_domainport.IsConnectedTo(comp3_domainport);
            #endregion
        }
    }
    
    static class Extensions
    {
        public static void IsConnectedTo(this avm.PortMapTarget p1, avm.PortMapTarget p2)
        {
            Assert.True(p1.PortMap.Contains(p2.ID) || p2.PortMap.Contains(p1.ID),
                        String.Format("Couldn't find expected PortMap relation between ID {0} and {1}", p1.ID, p2.ID));
        }
    }
}
