using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GME.MGA;
using Xunit;

namespace DesignImporterTests
{
    public class LayoutJsonFixture : DesignImporterTestFixtureBase
    {
        public override string pathXME
        {
            get { return "LayoutJson\\LayoutJson.xme"; }
        }
    }

    public class LayoutJsonRoundTrip : PortsRoundTripBase<LayoutJsonFixture>, IUseFixture<LayoutJsonFixture>
    {
        [Fact]
        public void ComponentAssembly_Tonka()
        {
            string asmName = "ComponentAssembly";
            RunRoundTrip(asmName, ac => {
                Assert.Equal("0,0,10,10", ((IMgaFCO)ac.Impl).RegistryValue["layoutBox"]);
                Assert.Equal("layout.json", ((IMgaFCO)ac.Impl).RegistryValue["layoutFile"]);
            });
        }
    }
}
