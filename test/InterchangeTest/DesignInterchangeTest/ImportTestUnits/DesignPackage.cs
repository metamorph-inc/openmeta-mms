using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GME.MGA;
using Xunit;
using CyPhyGUIs;
using Ionic.Zip;
using GME.CSharp;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using CyPhyDesignImporter;
using META;

namespace DesignImporterTests
{
    public class DesignPackage
    {
        [Fact]
        public void DesignPackageImport()
        {
            // Clean the test directory
            var pathTest = Path.Combine(META.VersionInfo.MetaPath,
                                        "test",
                                        "InterchangeTest",
                                        "DesignInterchangeTest",
                                        "ImportTestUnits",
                                        "Package");
            foreach (var path in Directory.EnumerateDirectories(pathTest))
            {
                Directory.Delete(path, true);
            }
            foreach (var path in Directory.EnumerateFiles(pathTest, "*.*"))
            {
                if (!path.Contains("NestedFolders.adp"))
                {
                    File.Delete(path);
                }
            }

            // Create a test project
            var proj = new MgaProject();
            proj.Create("MGA=" + Path.Combine(pathTest,"testmodel.mga"), "CyPhyML");

            try
            {
                String pathCA = null;
                proj.PerformInTransaction(() =>
                {
                    // Instantiate the importer and import the package
                    var importer = new AVMDesignImporter(GMEConsole.CreateFromProject(proj), proj);

                    var mdlCA = importer.ImportFile(Path.Combine(pathTest, "NestedFolders.adp"), AVMDesignImporter.DesignImportMode.CREATE_CAS);
                    var ca = CyPhyClasses.ComponentAssembly.Cast(mdlCA.Impl);
                    pathCA = ca.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE);
                });

                // Check the design's backend folder for all files except for the ADM.
                var filesExpected = new List<String> 
                { 
                    "testObject.txt",
                    "dir1/testObject.txt",
                    "dir1/dir1a/testObject.txt",
                    "dir2/testObject.txt"
                };
                var filesInDir = Directory.GetFiles(pathCA, "*.*", SearchOption.AllDirectories);

                Assert.Equal(filesExpected.Count(), filesInDir.Count());
                foreach (var file in filesExpected)
                {
                    String fullPath = Path.Combine(pathCA, file);
                    Assert.True(File.Exists(fullPath));
                }
            }
            finally
            {
                proj.Save();
                proj.Close();
            }
        }
    }

    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del)
        {
            var mgaGateway = new MgaGateway(project);
            project.CreateTerritoryWithoutSink(out mgaGateway.territory);
            mgaGateway.PerformInTransaction(del);
        }
    }
}
