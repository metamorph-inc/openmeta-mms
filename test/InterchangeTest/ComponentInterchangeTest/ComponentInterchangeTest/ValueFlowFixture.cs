using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;

namespace ComponentInterchangeTest
{
    public class ValueFlowFixture : IDisposable
    {
        public ValueFlowFixture()
        {
            // Clear the Components folder
            try
            {
                var compDir = Path.Combine(ValueFlow.testPath, "Exported");
                if (Directory.Exists(compDir))
                    Directory.Delete(compDir, true);
                Directory.CreateDirectory(compDir);
            }
            catch (Exception ex)
            {
                // Results will be unreliable. Might as well quit now.
                throw ex;
            }

            // Import the model.
            File.Delete(ValueFlow.mgaPath);
            GME.MGA.MgaUtils.ImportXME(ValueFlow.xmePath, ValueFlow.mgaPath);
            Assert.True(File.Exists(ValueFlow.mgaPath), "MGA file not found. Model import may have failed.");

            // Export the components
            var returnCode = CommonFunctions.runCyPhyComponentExporterCL(ValueFlow.mgaPath, "Exported");
            Assert.True(0 == returnCode, "Exporter had non-zero return code of " + returnCode);
        }

        public void Dispose()
        {
            // No state, so nothing to do here.
        }
    }
}
