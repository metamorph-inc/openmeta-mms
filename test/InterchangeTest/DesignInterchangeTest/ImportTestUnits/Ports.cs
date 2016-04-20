using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using GME.MGA;
using System.Diagnostics;
using System.IO;

namespace DesignImporterTests
{
    public class TonkaPortsFixture : DesignImporterTestFixtureBase
    {
        public override string pathXME
        {
            get { return "Ports\\TonkaPorts.xme"; }
        }
    }

    public class TonkaPortsRoundTrip : PortsRoundTripBase<TonkaPortsFixture>, IUseFixture<TonkaPortsFixture>
    {
        [Fact]
        public void ComponentAssembly_Tonka()
        {
            string asmName = "TonkaComponentAssembly";
            RunRoundTrip(asmName);
        }
    }

    public class PortsFixture : DesignImporterTestFixtureBase
    {
        public override String pathXME
        {
            get { return "Ports\\Ports.xme"; }
        }
    }

    public class PortsRoundTrip : PortsRoundTripBase<PortsFixture>, IUseFixture<PortsFixture>
    {
        [Fact]
        public void ComponentAssembly()
        {
            string asmName = "ComponentAssembly";
            RunRoundTrip(asmName);
        }

        [Fact]
        public void ComponentAssemblyWithCAD()
        {
            string asmName = "ComponentAssemblyWithCAD";
            RunRoundTrip(asmName);
        }
    }

    public abstract class PortsRoundTripBase<T> where T : DesignImporterTestFixtureBase
    {
        protected void RunRoundTrip(string asmName, Action<ISIS.GME.Dsml.CyPhyML.Interfaces.ComponentAssembly> caTest=null)
        {
            File.Delete(Path.Combine(fixture.AdmPath, asmName + ".adm"));
            File.Delete(Path.Combine(fixture.AdmPath, asmName + ".adp"));
            RunDesignExporter(asmName);
            var importedFilePath = CopyMgaAndRunDesignImporter(asmName, caTest);
            ComponentExporterUnitTests.Tests.runCyPhyMLComparator(proj.ProjectConnStr.Substring("MGA=".Length),
                importedFilePath, Environment.CurrentDirectory);
        }

        protected string CopyMgaAndRunDesignImporter(string asmName, Action<ISIS.GME.Dsml.CyPhyML.Interfaces.ComponentAssembly> caTest)
        {
            string testrunDir = Path.Combine(Path.GetDirectoryName(proj.ProjectConnStr.Substring("MGA=".Length)), "testrun");
            try
            {
                Directory.Delete(testrunDir, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            Directory.CreateDirectory(testrunDir);
            string importMgaPath = Path.Combine(testrunDir, Path.GetFileNameWithoutExtension(proj.ProjectConnStr.Substring("MGA=".Length)) + asmName + ".mga");
            //proj.Save(proj.ProjectConnStr + asmName + ".mga", true);
            File.Copy(proj.ProjectConnStr.Substring("MGA=".Length), importMgaPath, true);

            MgaProject proj2 = (MgaProject)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaProject"));
            proj2.OpenEx("MGA=" + importMgaPath, "CyPhyML", null);
            proj2.BeginTransactionInNewTerr();
            try
            {
                MgaFCO componentAssembly = (MgaFCO)proj2.RootFolder.GetObjectByPathDisp("/@" + FolderName + "/@" + asmName);
                Assert.NotNull(componentAssembly);
                componentAssembly.DestroyObject();
                var importer = new CyPhyDesignImporter.AVMDesignImporter(null, proj2);
                avm.Design design;
                var adpPath = Path.Combine(fixture.AdmPath, asmName + ("ComponentAssemblies" == FolderName ? ".adp" : ".adm"));
                var ret = importer.ImportFile(adpPath,
                    "ComponentAssemblies" == FolderName ? CyPhyDesignImporter.AVMDesignImporter.DesignImportMode.CREATE_CAS : CyPhyDesignImporter.AVMDesignImporter.DesignImportMode.CREATE_DS);
                if (caTest != null)
                {
                    caTest((ISIS.GME.Dsml.CyPhyML.Interfaces.ComponentAssembly)ret);
                }
            }
            finally
            {
                proj2.CommitTransaction();
                if (Debugger.IsAttached)
                {
                    proj2.Save(null, true);
                }
                proj2.Close(true);
            }
            return importMgaPath;
        }

        public MgaFCO RunDesignExporter(string asmName)
        {
            MgaFCO componentAssembly;
            proj.BeginTransactionInNewTerr();
            string fileExtension = ".adp";
            try
            {
                componentAssembly = (MgaFCO)proj.RootFolder.GetObjectByPathDisp("/@" + FolderName + "/@" + asmName);
                if (componentAssembly.Meta.Name == "DesignContainer")
                {
                    fileExtension = ".adm";
                }
                Assert.NotNull(componentAssembly);
                var designExporter = new CyPhyDesignExporter.CyPhyDesignExporterInterpreter();
                designExporter.Initialize(proj);

                var parameters = new CyPhyGUIs.InterpreterMainParameters()
                {
                    CurrentFCO = componentAssembly,
                    Project = proj,
                    OutputDirectory = AdmPath
                };

                designExporter.MainInTransaction(parameters, true);

            }
            finally
            {
                proj.AbortTransaction();
            }

            Assert.True(File.Exists(Path.Combine(fixture.AdmPath, asmName + fileExtension)));
            return componentAssembly;
        }

        MgaProject proj { get { return fixture.proj; } }
        protected T fixture;
        public void SetFixture(T data)
        {
            fixture = data;
        }
        public virtual string FolderName { get { return "ComponentAssemblies"; } }
        public virtual string AdmPath { get { return fixture.AdmPath; } }
    }
}
