using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace DesignImporterTests
{
    public class DesignWithFilesFixture : DesignImporterTestFixtureBase
    {
        public override string pathXME
        {
            get { return "DesignWithFiles\\DesignWithFiles.xme"; }
        }
    }

    public class DesignWithFilesRoundTrip : PortsRoundTripBase<DesignWithFilesFixture>, IUseFixture<DesignWithFilesFixture>
    {
        [Fact]
        public void ComponentAssembly_Tonka()
        {
            string asmName = "ComponentAssembly";
            RunRoundTrip(asmName);

            var testrunPath = Path.GetFullPath(Path.Combine(DesignImporterTestFixtureBase.PathTest, "DesignWithFiles\\testrun"));
            var files = Directory.EnumerateFiles(testrunPath, "*.*", SearchOption.AllDirectories)
                .Select(path => path.Substring(testrunPath.Length + 1))
                .OrderBy(path => path)
                .ToArray();
            Xunit.Assert.True(files.Length == 4, "Expected 4 files, but got these instead: " + String.Join(" ", files));
        }
    }
}
