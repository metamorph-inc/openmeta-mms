using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GME.MGA;
using System.IO;
using Xunit;
using CyPhyGUIs;

namespace DesignExporterUnitTests
{
    public abstract class ExporterFixture : IDisposable
    {
        public String PathTest {
            get 
            {
                return Path.GetDirectoryName(pathXME);
            }
        }

        public abstract String pathXME { get; }

        public ExporterFixture()
        {
            String mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(pathXME, out mgaConnectionString);

            proj = new MgaProject();
            bool ro_mode;
            proj.Open(mgaConnectionString, out ro_mode);
            proj.EnableAutoAddOns(true);
        }

        public void Dispose()
        {
            proj.Save();
            proj.Close();
        }

        public MgaProject proj { get; private set; }

        public avm.Design Convert(String pathDE)
        {
            MgaObject objDE = null;
            proj.PerformInTransaction(delegate
            {
                objDE = proj.get_ObjectByPath(pathDE);
            });
            Assert.NotNull(objDE);

            var interp = new CyPhyDesignExporter.CyPhyDesignExporterInterpreter();
            interp.Initialize(proj);
            InterpreterMainParameters param = new InterpreterMainParameters()
            {
                OutputDirectory = PathTest,
                CurrentFCO = objDE as MgaFCO,
                Project = proj
            };
            var result = interp.Main(param);
            Assert.True(result.Success);

            // Load the new .adm file
            var pathAdm = Path.Combine(PathTest,
                                       pathDE.Split('/').Last() + ".adm");
            var xml = File.ReadAllText(pathAdm);
            var design = XSD2CSharp.AvmXmlSerializer.Deserialize<avm.Design>(xml);
            Assert.NotNull(design);

            return design;
        }
    }
}
