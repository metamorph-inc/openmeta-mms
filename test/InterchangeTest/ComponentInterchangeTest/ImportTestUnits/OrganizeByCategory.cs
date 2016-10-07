using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using GME.MGA;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using GME.CSharp;

namespace ComponentImporterUnitTests
{
    public class OrganizeByCategoryFixture : IDisposable
    {
        public static String PathTest = Path.Combine(META.VersionInfo.MetaPath,
                                                     "test",
                                                     "InterchangeTest",
                                                     "ComponentInterchangeTest",
                                                     "ImportTestModels",
                                                     "OrganizeByCategory");

        public static String PathOriginalXME = Path.Combine(META.VersionInfo.MetaPath,
                                                    "test",
                                                    "InterchangeTest",
                                                    "ComponentInterchangeTest",
                                                    "SharedModels",
                                                    "BlankInputModel",
                                                    "InputModel.xme");

        public static String PathTestXME = Path.Combine(PathTest,
                                                        Path.GetFileName(PathOriginalXME));

        public OrganizeByCategoryFixture()
        {
            #region Set up test model and folder

            if (!Directory.Exists(PathTest))
            {
                Directory.CreateDirectory(PathTest);
            }
            if (File.Exists(PathTestXME))
            {
                File.Delete(PathTestXME);
            }
            File.Copy(PathOriginalXME, PathTestXME);

            #endregion

            String mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(PathTestXME,
                                              out mgaConnectionString);

            proj = new MgaProject();
            bool ro_mode;
            proj.Open(mgaConnectionString, out ro_mode);
            proj.EnableAutoAddOns(true);

            importer = new CyPhyComponentImporter.CyPhyComponentImporterInterpreter();
            importer.Initialize(proj);
        }
        
        public void Dispose()
        {
            proj.Save();
            proj.Close();
            importer.DisposeLogger();
        }

        public MgaProject proj { get; private set; }

        public CyPhyComponentImporter.CyPhyComponentImporterInterpreter importer;            
    }

    public class OrganizeByCategory : IUseFixture<OrganizeByCategoryFixture>
    {
        #region fixture
        OrganizeByCategoryFixture fixture;
        public void SetFixture(OrganizeByCategoryFixture data)
        {
            fixture = data;
        }
        #endregion

        MgaProject proj 
        { 
            get { return fixture.proj; } 
        }

        String pathTest
        {
            get { return OrganizeByCategoryFixture.PathTest; }
        }

        CyPhyComponentImporter.CyPhyComponentImporterInterpreter importer
        {
            get { return fixture.importer; }
        }

        private static String GetSimplePath(MgaObject obj)
        {
            var hierarchy = new List<MgaObject>();
            
            var iter = obj;
            while (iter != null)
            {
                hierarchy.Add(iter);

                GME.MGA.Meta.objtype_enum ot;
                iter.GetParent(out iter, out ot);
            }

            hierarchy.Reverse();
            return String.Join("/", hierarchy.Select(o => o.Name));
        }

        [Fact]
        public void NormalCase()
        {
            var category = new List<String> {
                "Cat1",
                "Cat2",
                "Cat3"
            };
            String compName = GetCurrentMethodName();
            CreateComponent_ImportComponent_CheckPath(category, compName);
        }

        [Fact]
        public void SingleDepthCategory()
        {
            var category = new List<String> {
                "SoloCat"
            };
            String compName = GetCurrentMethodName();
            CreateComponent_ImportComponent_CheckPath(category, compName);
        }

        [Fact]
        public void CategoryHasBlankToken()
        {
            var category = new List<String> {
                "WillHaveBlank",
                "",
                "ThirdThing"
            };
            String compName = GetCurrentMethodName();
            CreateComponent_ImportComponent_CheckPath(category, compName);
        }
        
        [Fact]
        public void CategoryIsBlankString()
        {
            var category = new List<String> {
                ""
            };
            String compName = GetCurrentMethodName();

            // Create the test component and export it to disk.
            avm.Component comp = new avm.Component()
            {
                Classifications = new List<String>() { "" },
                Name = compName
            };
            String pathComp = Path.Combine(pathTest, comp.Name + ".acm");
            comp.SaveToFile(pathComp);

            // Import the component
            IMgaFCO cyphyComp = null;
            proj.PerformInTransaction(delegate
            {
                cyphyComp = importer.ImportFile(proj, pathTest, pathComp);
            });

            // Check that its path is what we expected
            var expectedPath = String.Join("/",
                                           new List<String>() {
                                               "RootFolder",
                                               "Components",                                               
                                               compName
                                           });

            proj.PerformInTransaction(delegate
            {
                Assert.NotNull(cyphyComp);
                String pathCyPhyComp = GetSimplePath(cyphyComp as MgaObject);
                Assert.Equal(expectedPath, pathCyPhyComp);
            });
        }

        [Fact]
        public void CategoryFolderAlreadyExists()
        {
            var category = new List<String> {
                "AlreadyExists",
                "SecondPart"
            };
            String compName = GetCurrentMethodName();

            ISIS.GME.Dsml.CyPhyML.Interfaces.Components cf = null;
            proj.PerformInTransaction(delegate
            {
                var rf = ISIS.GME.Dsml.CyPhyML.Classes.RootFolder.GetRootFolder(proj);
                cf = rf.Children.ComponentsCollection.FirstOrDefault(c => c.Name.Equals("Components"));
                if (cf == null)
                {
                    cf = ISIS.GME.Dsml.CyPhyML.Classes.Components.Create(rf);
                }

                var cf2 = cf.Children.ComponentsCollection.FirstOrDefault(c => c.Name.Equals(category.First()));
                if (cf2 == null)
                {
                    cf2 = ISIS.GME.Dsml.CyPhyML.Classes.Components.Create(cf);
                    cf2.Name = category.First();
                }
            });
            
            CreateComponent_ImportComponent_CheckPath(category, compName);

            proj.PerformInTransaction(delegate
            {
                // Check that the folder is not duplicated.
                Assert.Equal(1, cf.Children.ComponentsCollection.Count(c => c.Name.Equals(category.First())));
            });
        }

        private void CreateComponent_ImportComponent_CheckPath(List<string> category, String compName)
        {
            // Create the test component and export it to disk.
            avm.Component comp = new avm.Component()
            {
                Classifications = new List<String>()
                {
                    String.Join(".", category)
                },
                Name = compName
            };
            String pathComp = Path.Combine(pathTest, comp.Name + ".acm");
            comp.SaveToFile(pathComp);
            
            // Import the component
            IMgaFCO cyphyComp = null;
            proj.PerformInTransaction(delegate
            {
                cyphyComp = importer.ImportFile(proj, pathTest, pathComp);
            });
                        
            // Check that its path is what we expected
            var expectedPath = String.Join("/",
                                           new List<String>() {
                                               "RootFolder",
                                               "Components", 
                                               String.Join("/", category),
                                               compName
                                           });

            proj.PerformInTransaction(delegate
            {
                Assert.NotNull(cyphyComp);
                String pathCyPhyComp = GetSimplePath(cyphyComp as MgaObject);
                Assert.Equal(expectedPath, pathCyPhyComp);
            });
        }

        [Fact]
        public void NoCategory()
        {
            String compName = GetCurrentMethodName();

            avm.Component comp = new avm.Component()
            {
                Classifications = null,
                Name = compName
            };
            String pathComp = Path.Combine(pathTest, comp.Name + ".acm");
            comp.SaveToFile(pathComp);

            IMgaFCO cyphyComp = null;
            proj.PerformInTransaction(delegate
            {
                cyphyComp = importer.ImportFile(proj, pathTest, pathComp);
            });

            String expectedPath = "RootFolder/Components/" + compName;

            proj.PerformInTransaction(delegate
            {
                Assert.NotNull(cyphyComp);
                String pathCyPhyComp = GetSimplePath(cyphyComp as MgaObject);
                Assert.Equal(expectedPath, pathCyPhyComp);
            });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethodName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }

    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del)
        {
            var mgaGateway = new MgaGateway(project);
            mgaGateway.PerformInTransaction(del);
        }
    }
}
