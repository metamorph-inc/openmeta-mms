﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using GME.MGA;
using System.IO;
using System.Reflection;
using GME.CSharp;
using ISIS.GME.Dsml.CyPhyML.Interfaces;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using File = ISIS.GME.Dsml.CyPhyML.Interfaces.File;

namespace DesignSpaceTest
{
    public class DSFixtureBase : IDisposable
    {
        public DSFixtureBase(string xmePath)
        {
            try
            {
                string connection;
                MgaUtils.ImportXMEForTest(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).CodeBase.Substring("file:///".Length)),
                    xmePath),
                    out connection);

                Type type = Type.GetTypeFromProgID("Mga.MgaProject");
                proj_ = Activator.CreateInstance(type) as MgaProject;
                proj_.OpenEx(connection, "CyPhyML", null);
            }
            catch (Exception e)
            {
                this.exception = e;
            }
        }

        private Exception exception;
        private IMgaProject proj_;
        public IMgaProject proj {
            get {
                if (exception != null)
                {
                    throw exception;
                }
                return proj_;
            }
        }

        public void Dispose()
        {
            if (exception == null)
            {
                proj_.Save();
                proj_.Close(true);
            }
        }
    }

    public class ToyDSFixture : DSFixtureBase, IDisposable
    {
        public ToyDSFixture() :
            base(@"..\..\..\..\models\DesignSpace\ToyDS.xme")
        {
        }
    }

    public class DomainModelDSFixture : DSFixtureBase, IDisposable
    {
        public DomainModelDSFixture() :
            base(@"..\..\..\..\models\DesignSpace\DomainModel.xme")
        {
        }
    }

    public abstract class DesertTestBaseClass
    {
        public void DesertTestBase(MgaProject project, string dsPath, Action<IEnumerable<Configurations>> helperTest, Action<Configurations> exporterTest)
        {
            var gateway = new MgaGateway(project);
            Type desertType = Type.GetTypeFromProgID("MGA.Interpreter.DesignSpaceHelper");
            var desert = (IMgaComponentEx)Activator.CreateInstance(desertType);

            MgaFCO currentobj = null;
            gateway.PerformInTransaction(() =>
            {
                currentobj = (MgaFCO)project.RootFolder.ObjectByPath[dsPath];
                var configurations = ISIS.GME.Dsml.CyPhyML.Classes.DesignContainer.Cast(currentobj).Children.ConfigurationsCollection;
                foreach (var configuration in configurations)
                {
                    configuration.Delete();
                }
            }, abort: false);
            Xunit.Assert.True(currentobj != null, string.Format("'{0}' does not exist in model", dsPath));

            desert.Initialize(project);
            desert.InvokeEx(project, currentobj, null, 128);
            Configurations configs = null;
            gateway.PerformInTransaction(() =>
            {
                var configurations = ISIS.GME.Dsml.CyPhyML.Classes.DesignContainer.Cast(currentobj).Children.ConfigurationsCollection;
                configs = configurations.First();
                helperTest(configurations);
            });

            if (exporterTest != null)
            {
                Type caExporterType = Type.GetTypeFromProgID("MGA.Interpreter.CyPhyCAExporter");
                var caExporter = (IMgaComponentEx)Activator.CreateInstance(caExporterType);

                gateway.PerformInTransaction(() =>
                {
                    MgaFCOs selected = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs"));
                    foreach (MgaFCO cwc in configs.Children.CWCCollection.Select(x => (MgaFCO)x.Impl))
                    {
                        selected.Append(cwc);
                    }

                    caExporter.Initialize(project);
                    caExporter.InvokeEx(project, selected[1].ParentModel as MgaFCO, selected, 128);
                    exporterTest(configs);
                });
            }
        }
    }

    public class DomainModelDesertTest : DesertTestBaseClass, IUseFixture<DomainModelDSFixture>
    {

        [Fact]
        void TestCAExport_DomainModel()
        {
            DesertTestBase((MgaProject)fixture.proj, "/@DesignSpaces/@DesignContainer_DomainModel", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(2, configurations.First().Children.CWCCollection.Count());
            }, configurations =>
            {
                Assert.Equal(2, configurations.Children.CWCCollection.Count());

                foreach (var cwc in configurations.Children.CWCCollection)
                {
                    //Verify that we generated both ComponentAssemblies
                    Assert.Equal(1, cwc.DstConnections.Config2CACollection.Count());
                    var caConn = cwc.DstConnections.Config2CACollection.First();
                    // ((MgaModel)ca.Impl).GetDescendantFCOs(project.CreateFilter()).Count
                    var ca = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssemblyRef.Cast(caConn.DstEnd.Impl).Referred.ComponentAssembly;
                    Assert.Equal(1, ca.Children.ConnectorCollection.Count());

                    //Now look inside the CA and see if we generated the GenericDomainModel
                    //We should have exactly one
                    var genericDomainModels = ca.Children.GenericDomainModelCollection;
                    Assert.Equal(1, genericDomainModels.Count());
                    //Now validate its metadata
                    var genericDomainModel = genericDomainModels.First();
                    Assert.Equal("OutsideGenericDomainModel", genericDomainModel.Name);
                    Assert.Equal("ArbitraryDomain", genericDomainModel.Attributes.Domain);
                    Assert.Equal("ArbitraryType", genericDomainModel.Attributes.Type);

                    //Verify that the generic domain model's ports and connections were copied
                    var ports = genericDomainModel.Children.GenericDomainModelPortCollection;
                    Assert.Equal(1, ports.Count());
                    Assert.Equal(1, ports.First().AllSrcConnections.Count());
                }
            });
        }

        private DomainModelDSFixture fixture;
        public void SetFixture(DomainModelDSFixture data)
        {
            this.fixture = data;
        }
    }

    public class DesertTest : DesertTestBaseClass, IUseFixture<ToyDSFixture>
    {
        [Fact]
        void TestDesertAutomation()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainer", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(2, configurations.First().Children.CWCCollection.Count());
            }, (configs) =>
            {
                foreach (var cwc in configs.Children.CWCCollection)
                {
                    Assert.Equal(1, cwc.DstConnections.Config2CACollection.Count());
                    var caConn = cwc.DstConnections.Config2CACollection.First();
                    // ((MgaModel)ca.Impl).GetDescendantFCOs(project.CreateFilter()).Count
                    var ca = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssemblyRef.Cast(caConn.DstEnd.Impl).Referred.ComponentAssembly;
                    Assert.Equal(1, ca.Children.ConnectorCollection.Count());
                }
            });

        }

        [Fact]
        void TestDesert_DesignContainer_SimpleProp()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainer_SimpleProp", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(2, configurations.First().Children.CWCCollection.Count());
            }, null);

        }

        [Fact(Skip = "Fails due to desert bug")]
        void TestDesert_DesignContainer_Alt_SimpleProp()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainer_Alt_SimpleProp", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(2, configurations.First().Children.CWCCollection.Count());
            }, null);

        }

        [Fact]
        void TestDesert_DesignContainer_DupPropName()
        {
            // bug was fixed where desert would stack overflow. Now the model is rejected
            Assert.Throws(typeof(COMException), () =>
                DesertTestBase(project, "/@DesignSpaces/@DesignContainer_DupPropName", (configurations) =>
                {
                }, null));

        }

        [Fact]
        void TestDesert_DesignContainer_Formula()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainer_Formula", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(1, configurations.First().Children.CWCCollection.Count());
            }, null);
        }

        [Fact]
        void TestDesert_DesignContainerParamConstraint()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainerParamConstraint", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(1, configurations.First().Children.CWCCollection.Count());
            }, null);
        }
        [Fact]
        void TestDesert_DesignContainer_Opt_Constraint()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainer_Opt_Constraint", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(3, configurations.First().Children.CWCCollection.Count());
            }, null);
        }

        [Fact]
        void TestDesert_DesignContainer_Opt_Constraint2()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainer_Opt_Constraint2", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(2, configurations.First().Children.CWCCollection.Count());
            }, null);
        }

        [Fact]
        void TestDesert_DesignContainer_Opt_Constraint3()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainer_Opt_Constraint3", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(3, configurations.First().Children.CWCCollection.Count());
            }, null);
        }

        [Fact]
        void TestDesert_DesignContainer_Formula_Dups()
        {
            DesertTestBase(project, "/@DesignSpaces/@DesignContainer_Formula_Dups", (configurations) =>
            {
                Assert.Equal(1, configurations.Count());
                Assert.Equal(1, configurations.First().Children.CWCCollection.Count());
            }, null);
        }

        [Fact]
        void TestDesignSpaceHelper_ExportXML()
        {
            var targetFile = "test__export.xml";
            // Make sure target export file doesn't exist when we begin
            if (System.IO.File.Exists(targetFile))
            {
                System.IO.File.Delete(targetFile);
            }

            var dsPath = "/@DesignSpaces/@DesignContainer";

            var gateway = new MgaGateway(project);
            Type desertType = Type.GetTypeFromProgID("MGA.Interpreter.DesignSpaceHelper");
            dynamic desert = Activator.CreateInstance(desertType);

            MgaFCO currentobj = null;
            gateway.PerformInTransaction(() =>
            {
                currentobj = (MgaFCO)project.RootFolder.ObjectByPath[dsPath];
                var configurations = ISIS.GME.Dsml.CyPhyML.Classes.DesignContainer.Cast(currentobj).Children.ConfigurationsCollection;
                foreach (var configuration in configurations)
                {
                    configuration.Delete();
                }
            }, abort: false);
            Xunit.Assert.True(currentobj != null, string.Format("'{0}' does not exist in model", dsPath));

            desert.Initialize(project);
            desert.ExportDesertXML(project, currentobj, targetFile);
            Xunit.Assert.True(System.IO.File.Exists(targetFile));
            
            var desertRoot = XElement.Load(Path.Combine(Directory.GetCurrentDirectory(), targetFile));

            var spaces = desertRoot.Elements("Space");
            Xunit.Assert.Equal(1, spaces.Count());

            // Validate that we have the root space
            Xunit.Assert.Equal("DesignSpace", spaces.First().Attribute("name").Value);

            // Now look for a leaf-node component: CompA
            Xunit.Assert.True(
                spaces.First().Descendants("Element").Any(element => element.Attribute("name").Value == "CompA"),
                "Generated DESERT XML is missing components"
            );
        }

        [Fact]
        void TestDesignSpaceHelper_ExportAndImportXML()
        {
            var dsPath = "/@DesignSpaces/@DesignContainer";

            var gateway = new MgaGateway(project);
            Type desertType = Type.GetTypeFromProgID("MGA.Interpreter.DesignSpaceHelper");
            dynamic desert = Activator.CreateInstance(desertType);

            MgaFCO currentobj = null;
            gateway.PerformInTransaction(() =>
            {
                currentobj = (MgaFCO)project.RootFolder.ObjectByPath[dsPath];
                var configurations = ISIS.GME.Dsml.CyPhyML.Classes.DesignContainer.Cast(currentobj).Children.ConfigurationsCollection;
                foreach (var configuration in configurations)
                {
                    configuration.Delete();
                }
            }, abort: false);
            Xunit.Assert.True(currentobj != null, string.Format("'{0}' does not exist in model", dsPath));

            desert.Initialize(project);

            var modelDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).CodeBase.Substring("file:///".Length)),
                @"..\..\..\..\models\DesignSpace");

            string exportedConfigsObjectName = desert.ImportConfigsFromXml(
                project,
                currentobj,
                Path.Combine(modelDirectory, "ToyDS_export.xml"),
                Path.Combine(modelDirectory, "ToyDS_export_back.xml"));

            gateway.PerformInTransaction(() =>
            {
                var configurations = ISIS.GME.Dsml.CyPhyML.Classes.DesignContainer.Cast(currentobj).Children.ConfigurationsCollection;

                Assert.Equal(1, configurations.Count());
                Assert.Equal(2, configurations.First().Children.CWCCollection.Count());
            });
        }

        private MgaProject project { get { return (MgaProject)fixture.proj; } }

        ToyDSFixture fixture;
        public void SetFixture(ToyDSFixture data)
        {
            fixture = data;
        }
    }

    public class DesertUnitTest
    {
        [DllImport("DesignSpaceHelper.dll", CallingConvention=CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.BStr)]
        private static extern string Exponentiate([MarshalAs(UnmanagedType.LPWStr)] string input);

        [Fact]
        void TestDesert_Exponentiation()
        {
            Type DesignSpaceHelperType = Type.GetTypeFromProgID("MGA.Interpreter.DesignSpaceHelper");
            var dsh = (IMgaComponentEx)Activator.CreateInstance(DesignSpaceHelperType);
            string input;
            string output;
            input = "(5)^6 / 7";
            output = Exponentiate(input);
            Assert.Equal("(exp(6*ln((5)))) / 7", output);

            string input2 = "(a/0.75)^(2.7+0.7*(b/c))";
            output = Exponentiate(input2);
            string expected2 = "(exp((2.7+0.7*(b/c))*ln((a/0.75))))";
            Assert.Equal(expected2, output);

            string input3 = "(a/0.75)^(2.7+0.7*(b/d))";
            output = Exponentiate(input3);
            string expected3 = "(exp((2.7+0.7*(b/d))*ln((a/0.75))))";
            Assert.Equal(expected3, output);

            string input4 = input2 + " / " + input3;
            output = Exponentiate(input4);
            string expected4 = expected2 + " / " + expected3;
            Assert.Equal(expected4, output);
            GC.KeepAlive(dsh);
        }

    }
}
