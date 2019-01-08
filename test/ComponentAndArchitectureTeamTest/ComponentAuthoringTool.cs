using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Xunit;
using System.IO;
using GME.MGA;
using GME.CSharp;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using META;
using SystemCParser;
using SystemCAttributesClass = ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass;

namespace ComponentAndArchitectureTeamTest
{
    public class ComponentAuthoringFixture : IDisposable
    {
        public string mgaPath;

        public ComponentAuthoringFixture()
        {
            String mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(ComponentAuthoring.existingxmePath, out mgaConnectionString);
            mgaPath = mgaConnectionString.Substring("MGA=".Length);

            Assert.True(File.Exists(Path.GetFullPath(mgaPath)),
                        String.Format("{0} not found. Model import may have failed.", mgaPath));

            proj = new MgaProject();
            bool ro_mode;
            proj.Open("MGA=" + Path.GetFullPath(mgaPath), out ro_mode);
            proj.EnableAutoAddOns(true);
        }

        public void Dispose()
        {
            proj.Save();
            proj.Close();
            proj = null;
        }

        public MgaProject proj { get; private set; }
    }

    public class ComponentAuthoring : IUseFixture<ComponentAuthoringFixture>
    {
        #region Path Variables
        // this first set is for the created and deleted model
        public static string testcreatepath = Path.Combine(
            META.VersionInfo.MetaPath,
            "test",
            "ComponentAuthoringToolTests");

        public static string testiconpath = Path.Combine(
            META.VersionInfo.MetaPath,
            "meta",
            "CyPhyML",
            "icons",
            "AcousticPowerPort.png");

        public static string testmfgmdlpath = Path.Combine(
            META.VersionInfo.MetaPath,
            "models",
            "Box",
            "components",
            "BottomPlate120x100",
            "generic_cots_mfg.xml");

        public static string RenamedFileName = "FileNameChanged.prt";
        public static string RenamedFileNameWithoutExtension = Path.GetFileNameWithoutExtension(RenamedFileName);
        public static string RenamedFileNameRelativePath = Path.Combine("CAD", RenamedFileName);

        // this set is the saved and reused model
        public static readonly string testPath = Path.Combine(
            META.VersionInfo.MetaPath,
            "models",
            "ComponentsAndArchitectureTeam",
            "ComponentAuthoringTool"
            );
        public static readonly string existingxmePath = Path.Combine(
            testPath,
            "ComponentAuthoringTool.xme"
            );

        // This is the path to the input multigate-test Eagle library, MOT-549:
        public static readonly string multigateLbrPath = Path.Combine(
            META.VersionInfo.MetaPath,
            "models",
            "ComponentsAndArchitectureTeam",
            "ComponentAuthoringTool",
            "eagle-lbr",
            "multigate.lbr"
            );

        // This is the path to a known-good multigate-test result, MOT-549:
        public static readonly string multigateKnownGoodResultPath = Path.Combine(
            META.VersionInfo.MetaPath,
            "models",
            "ComponentsAndArchitectureTeam",
            "ComponentAuthoringTool",
            "eagle-lbr",
            "goldenMultigateOutput.lbr"
            );

        // This is the path to a known-bad multigate-test result, with duplicate symbol names. MOT-549:
        public static readonly string multigateKnownBadResultPath = Path.Combine(
            META.VersionInfo.MetaPath,
            "models",
            "ComponentsAndArchitectureTeam",
            "ComponentAuthoringTool",
            "eagle-lbr",
            "duplicatedSymbols.lbr"
            );

        // This is the path to a simple generator component for testing the SPICE Import with PinMatcher. OPENMETA-357:
        public static readonly string generatorLbrPath = Path.Combine(
            META.VersionInfo.MetaPath,
            "models",
            "ComponentsAndArchitectureTeam",
            "ComponentAuthoringTool",
            "eagle-lbr",
            "generator.lbr"
            );

        public static string testpartpath = Path.Combine(
            testPath,
            "damper.prt.36");
        #endregion

        #region Fixture
        ComponentAuthoringFixture fixture;
        public void SetFixture(ComponentAuthoringFixture data)
        {
            fixture = data;
        }
        #endregion

        [Fact]
        public void TestComponentInstance()
        {
            // these are the actual steps of the test
            fixture.proj.PerformInTransaction(delegate
            {
                // create the environment for the Component authoring class
                CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder(fixture.proj);
                CyPhy.Component testcomp = fixture.proj.GetComponentsByName("JustAComponent").First();
                string ret_msg;

                // new instance of the class to test
                CyPhyComponentAuthoring.CyPhyComponentAuthoringInterpreter testcai = new CyPhyComponentAuthoring.CyPhyComponentAuthoringInterpreter();
                // these class variables need to be set to avoid NULL references
                var CurrentObj = testcomp.Impl as MgaFCO;

                // We are in a Component, check valid preconditions
                Assert.True(testcai.CheckPreConditions(CurrentObj, out ret_msg),
                            String.Format("{0} should allow CAT to run in it, but it is not. Err=({1})", testcomp.Name, ret_msg)
                            );

                // create a subcomponent and set it as current 
                CyPhy.Component testsubcomp = fixture.proj.GetComponentsByName("ComponentSubtype").First();
                CurrentObj = testsubcomp.Impl as MgaFCO;
                // We are in a sub-Component, check valid preconditions
                Assert.True(testcai.CheckPreConditions(CurrentObj, out ret_msg),
                            String.Format("{0} should allow CAT to run in it, but it is not. Err=({1})", testsubcomp.Name, ret_msg)
                            );

                // create a component instance and set it as current 
                CyPhy.Component testcompinst = fixture.proj.GetComponentsByName("ComponentInstance").First();
                CurrentObj = testcompinst.Impl as MgaFCO;
                // We are in a Component instance, check valid preconditions
                Assert.False(testcai.CheckPreConditions(CurrentObj, out ret_msg),
                            String.Format("{0} should NOT allow CAT to run in it, yet it is. Err=({1})", testcompinst.Name, ret_msg)
                            );

                // create a library object and set it as current 
                CyPhy.Component testlibcomp = fixture.proj.GetComponentsByName("LibComponent").First();
                CurrentObj = testlibcomp.Impl as MgaFCO;
                // We are in a library object, check valid preconditions
                Assert.False(testcai.CheckPreConditions(CurrentObj, out ret_msg),
                            String.Format("{0} should NOT allow CAT to run in it, yet it is. Err=({1})", testlibcomp.Name, ret_msg)
                            );
            });
        }

        [Fact]
        public void TestCADImportResourceNaming()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string path_Test = Path.Combine(testcreatepath, TestName);
            string path_Mga = Path.Combine(path_Test, TestName + ".mga");

            // delete any previous test results
            if (Directory.Exists(path_Test))
            {
                Directory.Delete(path_Test, true);
            }

            // create a new blank project
            MgaProject proj = new MgaProject();
            proj.Create("MGA=" + path_Mga, "CyPhyML");

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                // create the environment for the Component authoring class
                CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder(proj);
                CyPhy.Components components = CyPhyClasses.Components.Create(rf);
                CyPhy.Component testcomp = CyPhyClasses.Component.Create(components);

                // new instance of the class to test
                CyPhyComponentAuthoring.Modules.CADModelImport testcam = new CyPhyComponentAuthoring.Modules.CADModelImport();
                // these class variables need to be set to avoid NULL references
                testcam.SetCurrentDesignElement(testcomp);
                testcam.CurrentObj = testcomp.Impl as MgaFCO;

                // call the module with a part file to skip the CREO steps
                testcam.ImportCADModel(testpartpath);

                // verify results
                // insure the resource path was created correctly
                var correct_name = testcomp.Children.ResourceCollection.Where(p => p.Name == "damper.prt").First();
                Assert.True(correct_name.Attributes.Path == "CAD\\damper.prt",
                            String.Format("{0} should have had value {1}; instead found {2}", correct_name.Name, "CAD\\damper.prt", correct_name.Attributes.Path)
                            );
                // insure the part file was copied to the back-end folder correctly
                var getcadmdl = testcomp.Children.CADModelCollection.First();
                string returnedpath;
                getcadmdl.TryGetResourcePath(out returnedpath, ComponentLibraryManager.PathConvention.ABSOLUTE);
                Assert.True(File.Exists(returnedpath + ".36"),
                    String.Format("Could not find the source file for the created resource, got {0}", returnedpath));
            });
            proj.Save();
            proj.Close();
        }

        [Fact]
        public void TestCATDialogBoxCentering()
        {
            // these are the actual steps of the test
            fixture.proj.PerformInTransaction(delegate
            {
                // create the environment for the Component authoring class
                CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder(fixture.proj);
                CyPhy.Component testcomp = fixture.proj.GetComponentsByName("JustAComponent").First();

                // new instance of the class to test
                CyPhyComponentAuthoring.CyPhyComponentAuthoringInterpreter testcai = new CyPhyComponentAuthoring.CyPhyComponentAuthoringInterpreter();

                // Call the create dialog box method
                testcai.PopulateDialogBox(CyPhyComponentAuthoring.CyPhyComponentAuthoringInterpreter.SupportedDesignEntityType.Component, true);
                // Get the dialog box location and verify it is in the center of the screen
                Assert.True(testcai.ThisDialogBox.StartPosition == FormStartPosition.CenterScreen,
                            String.Format("CAT dialog box is not in the center of the screen")
                            );
            });
        }

        /// <summary>
        /// Checks that CAT's "Add Eagle Schematic" with a component having multiple gates won't
        /// create a back-end "ecad.lbr" file containing duplicate symbols.
        /// 
        /// See also MOT-549: EAGLE CAT module doesn't correctly handle multi-gate devices
        /// </summary>

        [Fact]
        public void TestAddMultiGateComponent()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string path_Test = Path.Combine(testcreatepath, TestName);
            string path_Mga = Path.Combine(path_Test, TestName + ".mga");

            // delete any previous test results
            if (Directory.Exists(path_Test))
            {
                Directory.Delete(path_Test, true);
            }

            // create a new blank project
            MgaProject proj = new MgaProject();
            proj.Create("MGA=" + path_Mga, "CyPhyML");

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                // create the environment for the Component authoring class
                CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder(proj);
                CyPhy.Components components = CyPhyClasses.Components.Create(rf);
                CyPhy.Component testcomp = CyPhyClasses.Component.Create(components);

                // new instance of the class to test
                CyPhyComponentAuthoring.Modules.EDAModelImport CATModule = new CyPhyComponentAuthoring.Modules.EDAModelImport();

                //// these class variables need to be set to avoid NULL references
                CATModule.SetCurrentDesignElement(testcomp);
                CATModule.CurrentObj = testcomp.Impl as MgaFCO;

                // call the primary function directly
                CATModule.ImportSelectedEagleDevice( "\\LM139_COMPARATOR\\", multigateLbrPath );

                // verify results
                // 1. Ensure the ecad.lbr file was generated in the back-end folder correctly.
                string ecadAbsolutePath = Path.Combine(testcomp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), "Schematic", "ecad.lbr");
                Assert.True(File.Exists(ecadAbsolutePath),
                    String.Format("Could not find the source file for the created resource, got {0}", ecadAbsolutePath));
                Trace.TraceInformation("An ecad.lbr file was generated in the back-end folder, at:\n{0}", ecadAbsolutePath);
                
                // 2. Checking our test environment; verify that VerboseFileCompare() can detect unmatched test files.
                var resultString = VerboseFileCompare(multigateKnownGoodResultPath, multigateKnownBadResultPath);
                Assert.False(resultString == "",
                    String.Format("VerboseFileCompare() didn't detect any differences between {0} and {1}.\n",
                    multigateKnownGoodResultPath,
                    multigateKnownBadResultPath)
                    );

                // 3. Checking our test environment; verify that VerboseFileCompare() reports the expected difference between test files.
                Assert.True( resultString.Contains("14_PIN_COMPARATOR"),
                    String.Format("VerboseFileCompare() reported a different difference between {0} and {1} than expected.\n",
                    multigateKnownGoodResultPath,
                    multigateKnownBadResultPath)
                    );

                // 4. Verify the generated ecad.lbr file correctly matches a good one.
                resultString = VerboseFileCompare(multigateKnownGoodResultPath, ecadAbsolutePath);
                Assert.True(resultString == "",
                    String.Format("The generated ecad.lbr file at {0} didn't match the known-good result at {1}.\n{2}",
                    ecadAbsolutePath,
                    multigateKnownGoodResultPath,
                    resultString)
                    );
                Trace.TraceInformation("The generated ecad.lbr file matched OK the expected result at:\n{0}", multigateKnownGoodResultPath);
            });
            proj.Save();
            proj.Close();
        }

        [Fact]
        public void TestAddCustomIconTool()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string path_Test = Path.Combine(testcreatepath, TestName);
            string path_Mga = Path.Combine(path_Test, TestName + ".mga");

            // delete any previous test results
            if (Directory.Exists(path_Test))
            {
                Directory.Delete(path_Test, true);
            }

            // create a new blank project
            MgaProject proj = new MgaProject();
            proj.Create("MGA=" + path_Mga, "CyPhyML");

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                // create the environment for the Component authoring class
                CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder(proj);
                CyPhy.Components components = CyPhyClasses.Components.Create(rf);
                CyPhy.Component testcomp = CyPhyClasses.Component.Create(components);

                // new instance of the class to test
                CyPhyComponentAuthoring.Modules.CustomIconAdd CATModule = new CyPhyComponentAuthoring.Modules.CustomIconAdd();

                //// these class variables need to be set to avoid NULL references
                CATModule.SetCurrentDesignElement(testcomp);
                CATModule.CurrentObj = testcomp.Impl as MgaFCO;

                // call the primary function directly
                CATModule.AddCustomIcon(testiconpath);

                // verify results
                // 1. insure the icon file was copied to the back-end folder correctly
                string iconAbsolutePath = Path.Combine(testcomp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), "Icon.png");
                Assert.True(File.Exists(iconAbsolutePath),
                    String.Format("Could not find the source file for the created resource, got {0}", iconAbsolutePath));

                // 2. insure the resource path was created correctly
                var iconResource = testcomp.Children.ResourceCollection.Where(p => p.Name == "Icon.png").First();
                Assert.True(iconResource.Attributes.Path == "Icon.png",
                            String.Format("{0} Resource should have had value {1}; instead found {2}", iconResource.Name, "Icon.png", iconResource.Attributes.Path)
                            );

                // 3. Verify the registry entry exists
                string expected_registry_entry = Path.Combine(testcomp.GetDirectoryPath(ComponentLibraryManager.PathConvention.REL_TO_PROJ_ROOT), "Icon.png");
                string registry_entry = (testcomp.Impl as GME.MGA.IMgaFCO).get_RegistryValue("icon");
                Assert.True(registry_entry.Equals(expected_registry_entry),
                            String.Format("Registry should have had value {0}; instead found {1}", expected_registry_entry, registry_entry)
                            );
            });
            proj.Save();
            proj.Close();
        }

        [Fact]
        public void TestAddDocumentation()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string path_Test = Path.Combine(testcreatepath, TestName);
            string path_Mga = Path.Combine(path_Test, TestName + ".mga");

            // delete any previous test results
            if (Directory.Exists(path_Test))
            {
                Directory.Delete(path_Test, true);
            }

            // create a new blank project
            MgaProject proj = new MgaProject();
            proj.Create("MGA=" + path_Mga, "CyPhyML");

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                // create the environment for the Component authoring class
                CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder(proj);
                CyPhy.Components components = CyPhyClasses.Components.Create(rf);
                CyPhy.Component testcomp = CyPhyClasses.Component.Create(components);

                // new instance of the class to test
                var CATModule = new CyPhyComponentAuthoring.Modules.AddDocumentation()
                {
                    CurrentObj = testcomp.Impl as MgaFCO
                };
                CATModule.SetCurrentDesignElement(testcomp);
                CATModule.CurrentObj = (MgaFCO)testcomp.Impl;

                var path_DocToAdd = Path.Combine(META.VersionInfo.MetaPath,
                                                 "src",
                                                 "CyPhyComponentAuthoring",
                                                 "Modules",
                                                 "AddDocumentation.cs");

                // Import it once and check
                {
                    CATModule.AddDocument(path_DocToAdd);

                    var resource = testcomp.Children.ResourceCollection.First(r => r.Name.Equals("AddDocumentation.cs"));
                    Assert.Equal(Path.GetFileName(path_DocToAdd), resource.Name);
                    var relpath_Resource = resource.Attributes.Path;
                    var path_Resource = Path.Combine(testcomp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE),
                                                     relpath_Resource);
                    Assert.True(File.Exists(path_Resource));
                }

                // Import again and make sure the collision is prevented.
                {
                    CATModule.AddDocument(path_DocToAdd);

                    var resource = testcomp.Children.ResourceCollection.First(r => r.Name.Equals("AddDocumentation_(1).cs"));
                    Assert.Equal("AddDocumentation_(1).cs", resource.Name);
                    var relpath_Resource = resource.Attributes.Path;
                    var path_Resource = Path.Combine(testcomp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE),
                                                     relpath_Resource);
                    Assert.True(File.Exists(path_Resource));
                }
            });
            proj.Save();
            proj.Close();
        }
        
        [Fact]
        public void SpiceImport()
        {
            String nameTest = "SpiceImport";

            fixture.proj.PerformInTransaction(delegate
            {
                // Set the SPICE file name
                string shortSpiceFileName = "test1.cir";

                // Set the SPICE file path
                string spiceFilesDirectoryPath = getSourceSpiceFileDirectory();

                // Set the combined path to the SPICE file
                string fullSpiceFileName = Path.Combine(spiceFilesDirectoryPath, shortSpiceFileName);

                var rf = CyPhyClasses.RootFolder.GetRootFolder(fixture.proj);
                var cf = CyPhyClasses.Components.Create(rf);
                cf.Name = nameTest;
                CyPhy.Component component = CyPhyClasses.Component.Create(cf);
                component.Name = nameTest;

                var catModule = new CyPhyComponentAuthoring.Modules.SpiceModelImport();
                catModule.SetCurrentDesignElement(component);
                catModule.CurrentObj = (MgaFCO)component.Impl;

                catModule.ImportSpiceModel(component, fullSpiceFileName);

                // Check that one and only one CyPhy SPICE model exists in the component.
                Assert.Equal(1, component.Children.SPICEModelCollection.Count());

                // Check that the CyPhy SPICE model has the correct class.
                var newSpiceModel = component.Children.SPICEModelCollection.First();
                Assert.True(newSpiceModel.Attributes.Class == "X:RES_0603");

                // Check that the CyPhy SPICE model has two pins.
                Assert.Equal(2, newSpiceModel.Children.SchematicModelPortCollection.Count());

                // Check the spice model's pins names and numbers.
                Dictionary<string, int> pinList = new Dictionary<string, int>();
                foreach (var newPort in newSpiceModel.Children.SchematicModelPortCollection)
                {
                    pinList.Add(newPort.Name, newPort.Attributes.SPICEPortNumber);
                }
                Assert.Equal(0, pinList["1"]);  // Schematic pin 1 is SPICE pin 0
                Assert.Equal(1, pinList["2"]);  // Schematic pin 2 is SPICE pin 1

                // Check that the component has two schematic pins
                Assert.Equal(2, component.Children.SchematicModelPortCollection.Count());

                // Check that there is one resource.
                Assert.Equal(1, component.Children.ResourceCollection.Count());
                var newResource = component.Children.ResourceCollection.First();

                // Check that the resource is named correctly.
                Assert.Equal("SPICEModelFile", newResource.Name);

                // Check that the resource has the copied-SPICE-file's relative path.
                Assert.Equal(Path.Combine("Spice", "test1.cir"), newResource.Attributes.Path);

                // Check connection between the SPICE model and the resource
                var srcConnections = newSpiceModel.SrcConnections.UsesResourceCollection;
                var dstConnections = newSpiceModel.DstConnections.UsesResourceCollection;
                var connUnion = srcConnections.Union(dstConnections);
                int connectionCount = 0;
                foreach( var connection in connUnion )
                {
                    if ((connection.DstEnd.ID == newResource.ID) || 
                        (connection.SrcEnd.ID == newResource.ID))
                    {
                        connectionCount += 1;
                    }
                }
                Assert.Equal(1, connectionCount );

                // Check that there are two port compositions
                Assert.Equal(2, component.Children.PortCompositionCollection.Count());

                // Check the connections between the component schematic pins and the SPICE model pins
                Dictionary<string,string> pinMap = new Dictionary<string,string>();
                foreach (var newPortComp in component.Children.PortCompositionCollection)
                {
                    var dstId = newPortComp.DstEnd.ID;
                    var srcId = newPortComp.SrcEnd.ID;

                    // Prove that this port composition connects a schematic pin
                    bool foundSchematicPin = false;
                    foreach (var newSchematicPin in component.Children.SchematicModelPortCollection)
                    {
                        var pinId = newSchematicPin.ID;
                        if ((pinId == dstId) || (pinId == srcId))
                        {
                            foundSchematicPin = true;
                            break;
                        }
                    }
                    Assert.True(foundSchematicPin);

                    // Prove that it also connects to a SPICE model pin
                    bool foundSpicePin = false;
                    foreach (var newSpicePin in newSpiceModel.Children.SchematicModelPortCollection)
                    {
                        var pinId = newSpicePin.ID;
                        if ((pinId == dstId) || (pinId == srcId))
                        {
                            foundSpicePin = true;
                            break;
                        }
                    }
                    Assert.True(foundSpicePin);
                }

                // Check that there is one CyPhy SPICE parameter.
                Assert.Equal(1, component.Children.SPICEModelParameterMapCollection.Count());
                var newParamMap = component.Children.SPICEModelParameterMapCollection.First();

                Assert.Equal(1, newSpiceModel.Children.SPICEModelParameterCollection.Count());
                var newParam = newSpiceModel.Children.SPICEModelParameterCollection.First();

                // Check that there is one component property.
                Assert.Equal(1, component.Children.PropertyCollection.Count());
                var newProp = component.Children.PropertyCollection.First();

                // Check that the spice model parameter and the component property are connected.
                List<string> mapIds = new List<string> {
                    newParamMap.SrcEnd.ID,
                    newParamMap.DstEnd.ID};
                mapIds.Sort();
                List<string> endIds = new List<string> {
                    newParam.ID,
                    newProp.ID};
                endIds.Sort();
                Assert.Equal(mapIds, endIds);

                // Create a path to the current component folder
                string PathForComp = component.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE);

                // Verify that the SPICE file has been copied to its destination
                string destinationFilePath = Path.Combine( PathForComp, newResource.Attributes.Path );
                string sourceFilePath = fullSpiceFileName;
                Assert.True(FileCompare( sourceFilePath, destinationFilePath ));
            });
        }

        [Fact]
        public void SpiceImportWithPinMatch()
        {
            String nameTest = "SpiceImportWithPinMatch";

            fixture.proj.PerformInTransaction(delegate
            {
                // Set the SPICE file name
                string shortSpiceFileName = "test4.cir";

                // Set the SPICE file path
                string spiceFilesDirectoryPath = getSourceSpiceFileDirectory();

                // Set the combined path to the SPICE file
                string fullSpiceFileName = Path.Combine(spiceFilesDirectoryPath, shortSpiceFileName);

                var rf = CyPhyClasses.RootFolder.GetRootFolder(fixture.proj);
                var cf = CyPhyClasses.Components.Create(rf);
                cf.Name = nameTest;
                CyPhy.Component component = CyPhyClasses.Component.Create(cf);
                component.Name = nameTest;

                //var path = String.Format("/@{0}|kind=Components/@{0}|kind=Component", nameTest);
                //var component = CyPhyClasses.Component.Cast(fixture.proj.RootFolder.ObjectByPath[path]);
                
                // new instance of the class to test
                CyPhyComponentAuthoring.Modules.EDAModelImport SchematicCATModule = new CyPhyComponentAuthoring.Modules.EDAModelImport();

                //// these class variables need to be set to avoid NULL references
                SchematicCATModule.SetCurrentDesignElement(component);
                SchematicCATModule.CurrentObj = component.Impl as MgaFCO;

                // call the primary function directly
                SchematicCATModule.ImportSelectedEagleDevice("\\GENERATOR\\", generatorLbrPath);

                var SpiceCATModule = new CyPhyComponentAuthoring.Modules.SpiceModelImport();
                SpiceCATModule.SetCurrentDesignElement(component);
                SpiceCATModule.CurrentObj = (MgaFCO)component.Impl;

                SpiceCATModule.ImportSpiceModel(component, fullSpiceFileName);

                // Check that one and only one CyPhy SPICE model exists in the component.
                Assert.Equal(1, component.Children.SPICEModelCollection.Count());

                // Check that the CyPhy SPICE model has the correct class.
                var newSpiceModel = component.Children.SPICEModelCollection.First();
                Assert.True(newSpiceModel.Attributes.Class == "X:GENERATOR");

                // Check that the CyPhy SPICE model has two pins.
                Assert.Equal(3, newSpiceModel.Children.SchematicModelPortCollection.Count());

                // Check that the GND pin has a connection to both the SPICE Model and EDA Model.
                var gnd_connections = component.Children.PortCompositionCollection.Where(a => a.SrcEnd.ParentContainer.Name == "GND" || a.DstEnd.ParentContainer.Name == "GND");
                var end_points = gnd_connections.Select(a => a.SrcEnd.ParentContainer.Name == "GND" ? a.DstEnd.ParentContainer.Name : a.SrcEnd.ParentContainer.Name);
                foreach (var end_point in end_points)
                {
                    Assert.True(end_point == "GENERATOR_SPICEModel" || end_point == "EDAModel");
                }

                Assert.Equal(7, component.Children.PortCompositionCollection.Count());
                var differing_connections = component.Children.PortCompositionCollection
                                           .Where(a => a.SrcEnd.Name != a.DstEnd.Name)
                                           .Select(a => new List<string> { a.SrcEnd.Name, a.DstEnd.Name });
                Assert.Equal(1, differing_connections.Count());
                var diff = differing_connections.First();
                diff.Sort();
                Assert.Equal("SDA", diff[0]);
                Assert.Equal("SIGNAL", diff[1]);
                //}
            });
        }

        [Fact]
        public void CheckMissingSpiceFileException()
        {
            String nameTest = "CheckMissingSpiceFileException";

            fixture.proj.PerformInTransaction(delegate
            {
                // Set the SPICE file name
                string shortSpiceFileName = "this_file_should_not_exist.cir";

                // Set the SPICE file's path
                string spiceFilesDirectoryPath = getSourceSpiceFileDirectory();

                // Set the combined path to the missing SPICE file
                string fullSpiceFileName = Path.Combine(spiceFilesDirectoryPath, shortSpiceFileName);

                var rf = CyPhyClasses.RootFolder.GetRootFolder(fixture.proj);
                var cf = CyPhyClasses.Components.Create(rf);
                cf.Name = nameTest;
                CyPhy.Component component = CyPhyClasses.Component.Create(cf);
                component.Name = nameTest;

                var catModule = new CyPhyComponentAuthoring.Modules.SpiceModelImport();

                Assert.DoesNotThrow(delegate
                {
                    catModule.ImportSpiceModel(component, fullSpiceFileName);
                });
                // Assert.True(expectedException.Message.Contains("does not exist"));
            });
        }

        
        //--------------------------------------------------------------------------------------------------
        /// <summary>
        /// FileCompare -- compares the contents of two files for equality.
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns>true if the files are the same</returns>
        private bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }

        /// <summary>
        /// getSourceSpiceFileDirectory
        /// </summary>
        /// <returns>The path of the source SPICE-file directory.</returns>
        private string getSourceSpiceFileDirectory()
        {
            // Set the SPICE file path to \models\SpiceTestModels
            string part1 = Path.Combine(META.VersionInfo.MetaPath, "models");
            string rVal = Path.Combine(part1, "SpiceTestModels");

            return rVal;
        }

        /// <summary>
        /// getScTestFileDirectory
        /// </summary>
        /// <returns>The path of the SystemC test-file directory.</returns>
        private string getScTestFileDirectory()
        {
            // Set the SystemC test-file path to \models\SystemCTestModels
            string part1 = Path.Combine(META.VersionInfo.MetaPath, "models");
            string rVal = Path.Combine(part1, "SystemCTestModels");

            return rVal;
        }



        [Fact]
        public void TestAddMfgModelTool()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string path_Test = Path.Combine(testcreatepath, TestName);
            string path_Mga = Path.Combine(path_Test, TestName + ".mga");

            // delete any previous test results
            if (Directory.Exists(path_Test))
            {
                Directory.Delete(path_Test, true);
            }

            // create a new blank project
            MgaProject proj = new MgaProject();
            proj.Create("MGA=" + path_Mga, "CyPhyML");

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                // create the environment for the Component authoring class
                CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder(proj);
                CyPhy.Components components = CyPhyClasses.Components.Create(rf);
                CyPhy.Component testcomp = CyPhyClasses.Component.Create(components);

                // new instance of the class to test
                CyPhyComponentAuthoring.Modules.MfgModelImport CATModule = new CyPhyComponentAuthoring.Modules.MfgModelImport();

                //// these class variables need to be set to avoid NULL references
                CATModule.SetCurrentDesignElement(testcomp);
                CATModule.CurrentObj = testcomp.Impl as MgaFCO;

                // call the primary function directly
                CATModule.import_manufacturing_model(testmfgmdlpath);

                // verify results
                // 1. insure the mfg file was copied to the backend folder correctly
                // insure the part file was copied to the backend folder correctly
                var getmfgmdl = testcomp.Children.ManufacturingModelCollection.First();
                string returnedpath;
                getmfgmdl.TryGetResourcePath(out returnedpath, ComponentLibraryManager.PathConvention.ABSOLUTE);
                string demanglepathpath = returnedpath.Replace("\\", "/");
                Assert.True(File.Exists(demanglepathpath),
                    String.Format("Could not find the source file for the created resource, got {0}", demanglepathpath));

                // 2. insure the resource path was created correctly
                var mfgResource = testcomp.Children.ResourceCollection.Where(p => p.Name == "generic_cots_mfg.xml").First();
                Assert.True(mfgResource.Attributes.Path == Path.Combine("Manufacturing", "generic_cots_mfg.xml"),
                            String.Format("{0} Resource should have had value {1}; instead found {2}", mfgResource.Name, "generic_cots_mfg.xml", mfgResource.Attributes.Path)
                            );
            });
            proj.Save();
            proj.Close();
        }

        [Fact]
        public void TestCADFileReNaming()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string path_Test = Path.Combine(testcreatepath, TestName);
            string path_Mga = Path.Combine(path_Test, TestName + ".mga");

            // delete any previous test results
            if (Directory.Exists(path_Test))
            {
                Directory.Delete(path_Test, true);
            }

            // create a new blank project
            MgaProject proj = new MgaProject();
            proj.Create("MGA=" + path_Mga, "CyPhyML");

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                // create the environment for the Component authoring class
                CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder(proj);
                CyPhy.Components components = CyPhyClasses.Components.Create(rf);
                CyPhy.Component testcomp = CyPhyClasses.Component.Create(components);

                // Import a CAD file into the test project
                CyPhyComponentAuthoring.Modules.CADModelImport importcam = new CyPhyComponentAuthoring.Modules.CADModelImport();
                // these class variables need to be set to avoid NULL references
                importcam.SetCurrentDesignElement(testcomp);
                importcam.CurrentObj = testcomp.Impl as MgaFCO;

                // import the CAD file
                importcam.ImportCADModel(testpartpath);
                // Get a path to the imported Cad file
                var getcadmdl = testcomp.Children.CADModelCollection.First();
                string importedpath;
                getcadmdl.TryGetResourcePath(out importedpath, ComponentLibraryManager.PathConvention.ABSOLUTE);
                // add in Creo version number
                importedpath += Path.GetExtension(testpartpath);

                // Rename the CAD file
                CyPhyComponentAuthoring.Modules.CADFileRename renamecam = new CyPhyComponentAuthoring.Modules.CADFileRename();
                // these class variables need to be set to avoid NULL references
                renamecam.SetCurrentDesignElement(testcomp);
                renamecam.CurrentObj = testcomp.Impl as MgaFCO;

                // call the module with a part file and the new file name
                renamecam.RenameCADFile(importedpath, RenamedFileNameWithoutExtension);

                // verify results
                // 1. Verify the file was renamed
                var renamecadmdl = testcomp.Children.CADModelCollection.First();
                string renamedpath;
                renamecadmdl.TryGetResourcePath(out renamedpath, ComponentLibraryManager.PathConvention.ABSOLUTE);
                // add in Creo version
                renamedpath += ".1";
                Assert.True(File.Exists(renamedpath),
                    String.Format("Could not find the renamed CAD file, found {0}", renamedpath));

                // 2. Verify the Model was renamed
                bool model_not_found = false;
                try
                {
                    var model_name = testcomp.Children.CADModelCollection.Where(p => p.Name == RenamedFileName).First();
                }
                catch (Exception ex)
                {
                    model_not_found = true;
                }
                Assert.False(model_not_found,
                            String.Format("No CAD model found with the renamed name {0}", RenamedFileName)
                            );

                // 3. Verify the resource name and path were changed
                CyPhy.Resource resource_name = null;
                bool resource_not_found = false;
                try
                {
                    resource_name = testcomp.Children.ResourceCollection.Where(p => p.Name == RenamedFileName).First();
                }
                catch (Exception ex)
                {
                    resource_not_found = true;
                }
                Assert.False(resource_not_found,
                            String.Format("No resource found with the renamed name {0}", RenamedFileName)
                            );
                Assert.True(resource_name.Attributes.Path == RenamedFileNameRelativePath,
                            String.Format("{0} should have had value {1}; instead found {2}", resource_name.Name, RenamedFileNameRelativePath, resource_name.Attributes.Path)
                            );
            });
            proj.Save();
            proj.Close();
        }

        public struct portData_s
        {
            public string name;
            public SystemCAttributesClass.Directionality_enum direction;
            public SystemCAttributesClass.DataType_enum type;
            public int dimension;

            public portData_s(
                string name, 
                SystemCAttributesClass.Directionality_enum direction, 
                SystemCAttributesClass.DataType_enum type, 
                int dimension)
            {
                this.name = name;
                this.direction = direction;
                this.type = type;
                this.dimension = dimension;
            }
        };


        //--------------------------------------------------------------------------------------------------
        /// <summary>
        /// VerboseFileCompare -- compares the contents of two text files for equality, ignoring newlines.
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns>A non-empty string if the files are different</returns>
        private string VerboseFileCompare(string file1, string file2)
        {
            var rVal = "";
            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return a null string to indicate that the files are the same.
                return rVal;
            }

            // Open the two files.
            System.IO.StreamReader fs1 = new System.IO.StreamReader(file1);
            System.IO.StreamReader fs2 = new System.IO.StreamReader(file2);

            // Read and compare a line from each file until either a
            // non-matching set of lines is found or until the end of
            // file1 is reached.
            int linecount = 0;
            bool done = false;
            while( !done )
            {
                linecount += 1;

                // Read one line from each file.
                var line1 = fs1.ReadLine();
                var line2 = fs2.ReadLine();

                if( (line1 == null) || (line2 == null) )
                {
                    if( line1 != line2 )
                    {
                        if( line1 == null )
                        {
                            rVal = string.Format( "File {0} ended at line {1}.", file1, linecount );
                        }
                        else
                        {
                            rVal = string.Format( "File {0} ended at line {1}.", file2, linecount );
                        }
                    }

                    done = true;
                }
                else if( line1.CompareTo( line2 ) != 0 )
                {
                    rVal = string.Format( "At line {0}, file {1} =\n\"{2}\"\nBut, file {3} =\n\"{4}\".",
                        linecount,
                        file1,
                        line1,
                        file2,
                        line2 );
                    done = true;
                }
            }

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return rVal;
        }


        #region OctoPart Importer Test

        private void cleanComponent(string iconPath, string datasheetPath, CyPhy.Resource iconResource, CyPhy.Resource datasheetResource)
        {
            // Remove created artifacts
            if (File.Exists(iconPath))
            {
                File.Delete(iconPath);
            }
            if (Directory.Exists(datasheetPath))
            {
                Directory.Delete(datasheetPath, true);
            }
            if (iconResource != null)
            {
                iconResource.Delete();
            }
            if (datasheetResource != null)
            {
                datasheetResource.Delete();
            }
        }
        
        [Fact(Skip="Octopart API key needed")]
        public void OctopartImporter_TypicalCase()
        {            
            MgaProject proj = this.fixture.proj;

            var component = "/@OctopartTest|kind=Components|relpos=0/LMV324IDR_-1100030P1|kind=Component|relpos=0";

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                var compfco = proj.ObjectByPath[component] as MgaFCO;
                CyPhy.Component comp = proj.GetComponentsByName("LMV324IDR_-1100030P1").First();
                string iconAbsolutePath = Path.Combine(comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), "Icon.png");
                string datasheetAbsolutePath = Path.Combine(comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), "doc");
                CyPhy.Resource iconResource = null;
                CyPhy.Resource datasheetResource = null;

                try
                {
                    // new instance of the class to test
                    CyPhyComponentAuthoring.Modules.OctoPartDataImport CATModule = new CyPhyComponentAuthoring.Modules.OctoPartDataImport();

                    //// these class variables need to be set to avoid NULL references
                    CATModule.SetCurrentDesignElement(comp);
                    CATModule.CurrentObj = compfco;

                    // call the primary function directly
                    bool success = CATModule.GetOctoPartData(comp);

                    // verify results
                    Assert.True(success);

                    // 1. ensure the icon file was copied to the back-end folder correctly
                    iconResource = comp.Children.ResourceCollection.Where(p => p.Name == "Icon.png").First();
                    Assert.True(File.Exists(iconAbsolutePath),
                        String.Format("Could not find the source file for the created resource, got {0}", iconAbsolutePath));

                    // 2. ensure the datasheet path was created correctly
                    datasheetResource = comp.Children.ResourceCollection.Where(p => p.Name == "Datasheet.pdf").First();
                    Assert.True(datasheetResource.Attributes.Path == "doc\\Datasheet.pdf",
                                String.Format("{0} Resource should have had value {1}; instead found {2}", datasheetResource.Name, "doc\\Datasheet.pdf", datasheetResource.Attributes.Path)
                                );

                    // 3. Verify the registry entry exists
                    Assert.True(comp.Children.PropertyCollection.Count() == 16, String.Format("OctoPart query did not create the expected number of CyPhy properties (16). Properties created: {0}",
                                                                                              comp.Children.PropertyCollection.Count()));
                }
                finally
                {
                    cleanComponent(iconAbsolutePath, datasheetAbsolutePath, iconResource, datasheetResource);
                }

            });            
        }


        [Fact(Skip="Octopart API key needed")]
        public void OctopartImporter_EmptyReturnCategory()
        {
            MgaProject proj = this.fixture.proj;

            var component = "/@OctopartTest|kind=Components|relpos=0/IndexOutOfRange|kind=Component|relpos=0";

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                var compfco = proj.ObjectByPath[component] as MgaFCO;
                CyPhy.Component comp = proj.GetComponentsByName("IndexOutOfRange").First();
                string iconAbsolutePath = Path.Combine(comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), "Icon.png");
                string datasheetAbsolutePath = Path.Combine(comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), "doc");
                CyPhy.Resource iconResource = null;
                CyPhy.Resource datasheetResource = null;

                try
                {
                    // new instance of the class to test
                    CyPhyComponentAuthoring.Modules.OctoPartDataImport CATModule = new CyPhyComponentAuthoring.Modules.OctoPartDataImport();

                    //// these class variables need to be set to avoid NULL references
                    CATModule.SetCurrentDesignElement(comp);
                    CATModule.CurrentObj = compfco;

                    // call the primary function directly
                    bool success = CATModule.GetOctoPartData(comp);

                    // verify results
                    Assert.True(success);

                    // 1. insure the icon file was copied to the back-end folder correctly
                    iconResource = comp.Children.ResourceCollection.Where(p => p.Name == "Icon.png").First();
                    Assert.True(File.Exists(iconAbsolutePath),
                        String.Format("Could not find the source file for the created resource, got {0}", iconAbsolutePath));

                    // 2. insure the datasheet path was created correctly
                    datasheetResource = comp.Children.ResourceCollection.Where(p => p.Name == "Datasheet.pdf").First();
                    Assert.True(datasheetResource.Attributes.Path == "doc\\Datasheet.pdf",
                                String.Format("{0} Resource should have had value {1}; instead found {2}", datasheetResource.Name, "doc\\Datasheet.pdf", datasheetResource.Attributes.Path)
                                );

                    Assert.Equal(new string[] { "ram_bytes", "number_of_bits", "lifecycle_status", "pin_count", "rohs_status", "clock_speed", "reach_svhc_compliance", "octopart_mpn" }.OrderBy(n => n),
                        comp.Children.PropertyCollection.Select(p => p.Name).OrderBy(n => n));
                }
                finally
                {
                    cleanComponent(iconAbsolutePath, datasheetAbsolutePath, iconResource, datasheetResource);
                }
            });
        }


        [Fact]
        public void OctopartImporter_InvalidMPN()
        {
            MgaProject proj = this.fixture.proj;

            var component = "/@OctopartTest|kind=Components|relpos=0/ValueCannotBeNull|kind=Component|relpos=0";

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                var compfco = proj.ObjectByPath[component] as MgaFCO;
                CyPhy.Component comp = proj.GetComponentsByName("ValueCannotBeNull").First();

                // new instance of the class to test
                CyPhyComponentAuthoring.Modules.OctoPartDataImport CATModule = new CyPhyComponentAuthoring.Modules.OctoPartDataImport();

                //// these class variables need to be set to avoid NULL references
                CATModule.SetCurrentDesignElement(comp);
                CATModule.CurrentObj = compfco;

                // call the primary function directly
                bool success = CATModule.GetOctoPartData(comp);

                Assert.False(success);

            });
        }


        [Fact(Skip="Octopart API key needed")]
        public void Octopart_Importer_NoEDAModelButMPNProperty()
        {
            MgaProject proj = this.fixture.proj;

            var component = "/@OctopartTest|kind=Components|relpos=0/MPN_only|kind=Component|relpos=0";

            // these are the actual steps of the test
            proj.PerformInTransaction(delegate
            {
                var compfco = proj.ObjectByPath[component] as MgaFCO;
                CyPhy.Component comp = proj.GetComponentsByName("MPN_only").First();
                string iconAbsolutePath = Path.Combine(comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), "Icon.png");
                string datasheetAbsolutePath = Path.Combine(comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), "doc");
                CyPhy.Resource iconResource = null;
                CyPhy.Resource datasheetResource = null;

                try
                {
                    // new instance of the class to test
                    CyPhyComponentAuthoring.Modules.OctoPartDataImport CATModule = new CyPhyComponentAuthoring.Modules.OctoPartDataImport();

                    //// these class variables need to be set to avoid NULL references
                    CATModule.SetCurrentDesignElement(comp);
                    CATModule.CurrentObj = compfco;

                    // call the primary function directly
                    bool success = CATModule.GetOctoPartData(comp);

                    // verify results
                    Assert.True(success);

                    // 1. insure the icon file was copied to the back-end folder correctly
                    iconResource = comp.Children.ResourceCollection.Where(p => p.Name == "Icon.png").First();
                    Assert.True(File.Exists(iconAbsolutePath),
                        String.Format("Could not find the source file for the created resource, got {0}", iconAbsolutePath));

                    // 2. insure the datasheet path was created correctly
                    datasheetResource = comp.Children.ResourceCollection.Where(p => p.Name == "Datasheet.pdf").First();
                    Assert.True(datasheetResource.Attributes.Path == "doc\\Datasheet.pdf",
                                String.Format("{0} Resource should have had value {1}; instead found {2}", datasheetResource.Name, "doc\\Datasheet.pdf", datasheetResource.Attributes.Path)
                                );

                    // 3. Verify the registry entry exists
                    Assert.True(comp.Children.PropertyCollection.Count() == 16, String.Format("OctoPart query did not create the expected number of CyPhy properties (16). Properties created: {0}",
                                                                                              comp.Children.PropertyCollection.Count()));
                }
                finally
                {
                    cleanComponent(iconAbsolutePath, datasheetAbsolutePath, iconResource, datasheetResource);
                }
            });
        }
        
        #endregion
    }
}
