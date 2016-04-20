using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ElaboratorTest
{
    public class CheckGMEVersion
    {
        [Fact(Skip="Rarely works on build machines")]
        void VersionCheck()
        {
            var gme = (GME.IGMEOLEApp)Activator.CreateInstance(Type.GetTypeFromProgID("GME.Application"));
            Assert.True(gme.VersionMajor >= 15, String.Format("GME version {0} is too old", gme.Version));
            Assert.True(gme.VersionMinor >= 4, String.Format("GME version {0} is too old", gme.Version));

        }
    }
}
