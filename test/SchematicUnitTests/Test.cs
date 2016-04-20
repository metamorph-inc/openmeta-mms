using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using GME.MGA;

namespace SchematicUnitTests
{
    public class Test
    {
        private static void ImportTest(String p_test, String p_xme, out String p_mga)
        {
            try
            {
                String xmeName = Path.GetFileName(p_xme);
                Assert.True(File.Exists(p_xme), String.Format("{0} is missing", xmeName));

                p_mga = p_xme.Replace(".xme", "_test.mga");
                MgaUtils.ImportXME(p_xme, p_mga);

                // Assert that MGA file exists, and is relatively new
                String mgaName = Path.GetFileName(p_mga);
                Assert.True(File.Exists(p_mga), String.Format("{0} does not exist. It probably didn't import successfully.", mgaName));

                DateTime dt_mgaLastWrite = File.GetLastWriteTime(p_mga);
                var threshold = DateTime.Now.AddSeconds(-10.0);
                Assert.True(dt_mgaLastWrite > threshold, String.Format("{0} is older than 10 seconds. It probably didn't import successfully, and this is an old copy.", mgaName));
                
                // Delete temp file
                File.Delete(p_mga);
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
        
        /*
        [Fact]
        public static void SomeModel_LoadTest()
        {
            String p_test = "../../../../../models/Eagle/Circuit2";
            String p_xme = Path.Combine(p_test, "Circuit2.xme");
            String p_mga;
            ImportTest(p_test, p_xme, out p_mga);
        }
        */

    }
}
