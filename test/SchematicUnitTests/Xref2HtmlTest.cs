using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using CyPhy2Schematic;
using System.IO;

namespace SchematicUnitTests
{
    /// <summary>
    /// Unit tests
    /// </summary>
    public class Xref2HtmlTests
    {
        [Fact]
        public void FakeXrefCreation()
        {
            string designName = "Unit Test Fake Design";
            string OutputFileName = "xrefUnitTest.html";
            File.Delete(OutputFileName);
            Assert.False(File.Exists(OutputFileName));

            List<XrefItem> fakeTable = new List<XrefItem>() {
                { new XrefItem() { ReferenceDesignator = "R1", GmePath = @"path\To\R100-3"} },
                { new XrefItem() { ReferenceDesignator = "R2", GmePath = @"path\To\R100-2"} },
                { new XrefItem() { ReferenceDesignator = "R3", GmePath = @"path\To\R100-1"} },
                { new XrefItem() { ReferenceDesignator = "C1", GmePath = @"path\To\C9$1"} },
                { new XrefItem() { ReferenceDesignator = "C2", GmePath = @"path\To\C9-2"} },
                { new XrefItem() { ReferenceDesignator = "C3", GmePath = @"path\To\C9"} },
                { new XrefItem() { ReferenceDesignator = "T1", GmePath = @"apath\To\<b>TVS-1</b>"} },
                { new XrefItem() { ReferenceDesignator = "U1", GmePath = @"upath\To\74SN00-07"} },
                { new XrefItem() { ReferenceDesignator = "U2", GmePath = @"upath\To\74SN00-05"} },
                { new XrefItem() { ReferenceDesignator = "U3", GmePath = @"upath\To\74SN00-03"} },
                { new XrefItem() { ReferenceDesignator = "U4", GmePath = @"upath\To\74SN00-21"} },
                { new XrefItem() { ReferenceDesignator = "U5", GmePath = @"upath\To\74SN00-18"} },
            };

            string result = Xref2Html.makeHtmlFile(
                designName,
                fakeTable,
                OutputFileName);

            Assert.False(String.IsNullOrWhiteSpace(result));
            Assert.True(File.Exists(OutputFileName));
            int numLines = result.Split('\n').Length;
            Assert.InRange(numLines, 40, 60);
            Console.WriteLine("Output contains {0} lines:", numLines);
            Console.WriteLine("{0}", result);
        }
    }
}
