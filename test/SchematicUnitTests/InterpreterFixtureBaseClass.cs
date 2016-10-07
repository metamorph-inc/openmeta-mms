using System;
using System.IO;
using GME.MGA;
using Xunit;

namespace SchematicUnitTests
{
    public abstract class InterpreterFixtureBaseClass : IDisposable
    {
        public String path_Test
        {
            get
            {
                return Path.GetDirectoryName(path_XME);
            }
        }

        public abstract String path_XME { get; }

        public readonly String path_MGA;
        public MgaProject proj { get; private set; }

        public InterpreterFixtureBaseClass()
        {
            String mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(path_XME, out mgaConnectionString);
            path_MGA = mgaConnectionString.Substring("MGA=".Length);

            Assert.True(File.Exists(Path.GetFullPath(path_MGA)),
                        String.Format("{0} not found. Model import may have failed.", path_MGA));

            foreach (string dirname in new string[] { Path.Combine(path_Test, "output"), Path.Combine(path_Test, "results") }) {
                if (Directory.Exists(dirname))
                {
                    foreach (string filename in Directory.GetFiles(dirname, "*", SearchOption.AllDirectories))
                    {
                        File.Delete(Path.Combine(dirname, filename));
                    }
                }
            }

            proj = new MgaProject();
            bool ro_mode;
            proj.Open("MGA=" + Path.GetFullPath(path_MGA), out ro_mode);
            proj.EnableAutoAddOns(true);

            // Ensure "~/Documents/eagle" exists
            var pathDocEagle = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                            "eagle");
            Directory.CreateDirectory(pathDocEagle);
        }

        public void Dispose()
        {
            proj.Close(abort: true);
            proj = null;
        }
    }
}
