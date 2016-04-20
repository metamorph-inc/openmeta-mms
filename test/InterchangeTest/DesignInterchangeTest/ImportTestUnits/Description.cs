using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace DesignImporterTests
{
    public class DescriptionFixture : DesignImporterTestFixtureBase
    {
        public override string pathXME
        {
            get { return "Description\\Description.xme"; }
        }
    }

    public class DescriptionRoundTrip : PortsRoundTripBase<DescriptionFixture>, IUseFixture<DescriptionFixture>
    {
        [Fact]
        public void ComponentAssembly_Tonka()
        {
            string asmName = "ComponentAssembly";
            RunRoundTrip(asmName);
        }
    }
}
