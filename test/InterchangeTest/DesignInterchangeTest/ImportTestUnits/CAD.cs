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
    public class CADFixture : DesignImporterTestFixtureBase
    {
        public override String pathXME
        {
            get { return "CAD\\CAD.xme"; }
        }
    }

    public class CAD : PortsRoundTripBase<CADFixture>, IUseFixture<CADFixture>
    {
        [Fact]
        public void WheelParameter()
        {
            RunRoundTrip("ReferenceCoordinateSystem");
        }

        public override string AdmPath { get { return fixture.AdmPath; } }
    }
}
