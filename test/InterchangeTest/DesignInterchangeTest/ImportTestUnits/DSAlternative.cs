using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GME.MGA;
using Xunit;

namespace DesignImporterTests
{
    public class DSAlternativeFixture : DesignImporterTestFixtureBase
    {
        public override String pathXME
        {
            get { return "DSAlternative\\DSAlternative.xme"; }
        }
    }

    public class DSAlternative : PortsRoundTripBase<DSAlternativeFixture>, IUseFixture<DSAlternativeFixture>
    {
        //[Fact]
        public void WheelParameter()
        {
            string asmName = "WheelParameter";
            RunRoundTrip(asmName);
        }

        [Fact]
        public void WheelProperty()
        {
            string asmName = "WheelProperty";
            RunRoundTrip(asmName);
        }

        public override string AdmPath { get { return fixture.AdmPath; } }
        public override string FolderName { get { return "DesignSpaces"; } }
    }
}
