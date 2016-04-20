using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GME.MGA;
using Xunit;
using CyPhyGUIs;
using Ionic.Zip;

namespace DesignExporterUnitTests
{
    public class DesignPackageFixture : IDisposable
    {
        public String PathTest
        {
            get
            {
                return Path.GetDirectoryName(pathXME);
            }
        }

        private String pathXME
        {
            get
            {
                return Path.Combine(META.VersionInfo.MetaPath,
                                    "test",
                                    "InterchangeTest",
                                    "DesignInterchangeTest",
                                    "ExportTestModels",
                                    "DesignPackage",
                                    "model.xme");
            }
        }

        public DesignPackageFixture()
        {
            // Delete files from past runs
            var adm = Directory.EnumerateFiles(PathTest, "*.adm", SearchOption.AllDirectories);
            var adp = Directory.EnumerateFiles(PathTest, "*.adp", SearchOption.AllDirectories);
            foreach (var file in adm.Union(adp))
            {
                File.Delete(file);
            }

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
    }

    public class DesignPackage : IUseFixture<DesignPackageFixture>
    {
        #region Fixture
        DesignPackageFixture fixture;
        public void SetFixture(DesignPackageFixture data)
        {
            fixture = data;
        }
        #endregion

        private MgaProject proj { get { return fixture.proj; } }
        
        private static void VerifyZipContents(String PathZip, IEnumerable<String> ExpectedFiles)
        {
            Assert.True(File.Exists(PathZip));

            using (var zip = ZipFile.Read(PathZip))
            {
                Assert.Equal(ExpectedFiles.Count(), zip.Count);
                foreach (var entry in ExpectedFiles)
                {
                    Assert.True(1 == zip.Count(ze => ze.FileName.Equals(entry)), "Missing " + entry);
                }
            }
        }

        [Fact]
        public void NoArtifacts()
        {
            ExportDesignPackage("NoArtifacts");
            String pathADP = Path.Combine(fixture.PathTest, "NoArtifacts.adp");   
            VerifyZipContents(pathADP, 
                              new List<String> { 
                                  "NoArtifacts.adm" 
                              });
        }

        [Fact]
        public void OneArtifact()
        {
            ExportDesignPackage("OneArtifact");
            String pathADP = Path.Combine(fixture.PathTest, "OneArtifact.adp");
            VerifyZipContents(pathADP, 
                              new List<String> { 
                                  "OneArtifact.adm", 
                                  "2/testObject.txt" 
                              });
        }

        [Fact]
        public void NestedFolders()
        {
            ExportDesignPackage("NestedFolders");
            String pathADP = Path.Combine(fixture.PathTest, "NestedFolders.adp");
            VerifyZipContents(pathADP,
                              new List<String> { 
                                  "NestedFolders.adm", 
                                  "3/testObject.txt",
                                  "3/dir1/testObject.txt",
                                  "3/dir1/dir1a/testObject.txt",
                                  "3/dir2/testObject.txt"
                              });
        }

        [Fact]
        public void ExportAll()
        {
            String pathTest = Path.Combine(fixture.PathTest, "all");
            Directory.CreateDirectory(pathTest);

            var interp = new CyPhyDesignExporter.CyPhyDesignExporterInterpreter()
            {
                OutputDir = pathTest
            };

            proj.PerformInTransaction(delegate
            {
                interp.Main(proj, null, null, 0);
            });
            interp.DisposeLogger();

            // Make sure files exist
            var expected = new List<String> 
            {
                "NestedFolders.adp",
                "NoArtifacts.adp",
                "OneArtifact.adp"
            };

            foreach (var path in expected)
            {
                var fullPath = Path.Combine(pathTest, path);
                Assert.True(File.Exists(fullPath));
            }
        }

        private void ExportDesignPackage(String name)
        {
            // Get all designs in project
            var filter = proj.CreateFilter();
            filter.Kind = "ComponentAssembly";
            filter.Name = name;

            MgaFCOs assemblies = null;
            proj.PerformInTransaction(delegate
            {
                assemblies = proj.AllFCOs(filter);
            });

            Assert.NotNull(assemblies);
            Assert.Equal(1, assemblies.Count);

            var interp = new CyPhyDesignExporter.CyPhyDesignExporterInterpreter()
            {
                OutputDir = fixture.PathTest
            };

            foreach (var fco in assemblies)
            {
                proj.PerformInTransaction(delegate
                {
                    interp.Main(proj, fco as MgaFCO, null, 0);
                });
                interp.DisposeLogger();
            }
        }
    }
}
