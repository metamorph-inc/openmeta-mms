using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using GME.CSharp;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ComponentImporterUnitTests;
using GME.MGA;
using GME.Util;
using CyPhyComponentExporter;

namespace TonkaACMTest
{

    public static class Extension
    {
        public static IEnumerable<CyPhy.Component> GetComponentsByName(this MgaProject project, String name)
        {
            MgaFilter filter = project.CreateFilter();
            filter.Kind = "Component";
            filter.Name = name;

            return project.AllFCOs(filter)
                          .Cast<MgaFCO>()
                          .Select(x => CyPhyClasses.Component.Cast(x))
                          .Cast<CyPhy.Component>()
                          .Where(c => c.ParentContainer.Kind == "Components");

        }
        public static IEnumerable<CyPhy.ComponentAssembly> GetComponentAssemblyByName(this MgaProject project, String name)
        {
            MgaFilter filter = project.CreateFilter();
            filter.Kind = "ComponentAssembly";
            filter.Name = name;

            return project.AllFCOs(filter)
                          .Cast<MgaFCO>()
                          .Select(x => CyPhyClasses.ComponentAssembly.Cast(x))
                          .Cast<CyPhy.ComponentAssembly>();

        }
    }
    public class GenericModelFixture
    {
        public GenericModelFixture()
        {
            // First, copy BlankInputModel/InputModel.xme into the test folder
            File.Delete(GenericModelTest.inputMgaPath);
            GME.MGA.MgaUtils.ImportXME(GenericModelTest.blankXMEPath, GenericModelTest.inputMgaPath);
            Assert.True(File.Exists(GenericModelTest.inputMgaPath), "InputModel.mga not found; import may have failed.");

            // Next, import the content model
            File.Delete(GenericModelTest.inputMgaPath);
            GME.MGA.MgaUtils.ImportXME(GenericModelTest.genericModelXMEPath, GenericModelTest.genericModelMgaPath);
            Assert.True(File.Exists(GenericModelTest.genericModelMgaPath),
                        String.Format("{0} not found; import may have failed.",
                                      Path.GetFileName(GenericModelTest.inputMgaPath)
                                     )
                        );
        }
    }

    public class GenericModelTest : IUseFixture<GenericModelFixture>
    {
        #region Paths
        public static readonly string testPath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                              "..\\..\\..\\..",
                                                              "models",
                                                              "ACMTestModels",
                                                              "GenericModel");
        public static readonly string blankXMEPath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                                  "..\\..\\..\\..",
                                                                  "test",
                                                                  "InterchangeTest",
                                                                  "ComponentInterchangeTest",
                                                                  "SharedModels",
                                                                  "BlankInputModel",
                                                                  "InputModel.xme");
        public static readonly string inputMgaPath = Path.Combine(testPath,
                                                                  "InputModel.mga");
        public static readonly string genericModelXMEPath = Path.Combine(testPath,
                                                                      "GenericModel.xme");
        public static readonly string genericModelMgaPath = Path.Combine(testPath,
                                                                      "GenericModel-test.mga");
        public static readonly string modelOutputPath = Path.Combine(testPath,
                                                                     "acm");
        #endregion

        #region Fixture
        GenericModelFixture fixture;
        public void SetFixture(GenericModelFixture data)
        {
            fixture = data;
        }
        #endregion

        [Trait("ModelType", "GenericModel")]
        [Fact]
        public void TestExport()
        {
            // Next, export all component models from the content model
            var args = String.Format("{0} -f {1}", GenericModelTest.genericModelMgaPath, GenericModelTest.modelOutputPath).Split();
            CyPhyComponentExporterCL.CyPhyComponentExporterCL.Main(args);

            Assert.True(Directory.Exists(GenericModelTest.modelOutputPath), "Model output path doesn't exist; Exporter may have failed.");


            MgaProject project = new MgaProject();
            MgaResolver resolver = new StrictMgaResolver();
            resolver.IsInteractive = false;
            bool ro_mode;
            project.Open("MGA=" + Path.GetFullPath(GenericModelTest.genericModelMgaPath), out ro_mode);
            try
            {
                project.BeginTransactionInNewTerr();

                var cyphyComp = project.GetComponentsByName("Component1").First();
                var comp = CyPhy2ComponentModel.Convert.CyPhyML2AVMComponent(cyphyComp);
                CyPhyComponentExporterInterpreter.SerializeAvmComponent(comp, 
                    Path.Combine(Path.GetDirectoryName(GenericModelTest.genericModelMgaPath), "Component1_test.acm"));

                avm.GenericDomainModel genericDomainModel = (avm.GenericDomainModel)comp.DomainModel.First();
                Assert.Equal("domain0", genericDomainModel.Domain);
                Assert.Equal("mms", genericDomainModel.Author);
                Assert.Equal("type0", genericDomainModel.Type);
                Assert.Equal("attr5", genericDomainModel.GenericAttribute5);

                avm.GenericDomainModelParameter dmParameter = genericDomainModel.GenericDomainModelParameter.First();
                Assert.Equal("attr4", dmParameter.GenericAttribute4);
                Assert.Equal(((avm.PrimitiveProperty)comp.Property.Single()).Value.ID, ((avm.DerivedValue)dmParameter.Value.ValueExpression).ValueSource);
                Assert.Equal(comp.Connector.Where(c => c.Name == "Connector").Single().Role.Single().PortMap.Single(),
                    genericDomainModel.GenericDomainModelPort.Single().ID);
                Assert.Equal(comp.Connector.Where(c => c.Name == "Connector2").Single().Role.Single().ID,
                    genericDomainModel.GenericDomainModelPort.Single().PortMap.Single());
                Assert.Equal("GenericDomainModelParameter_InDomainModel", dmParameter.Name);

                avm.GenericDomainModelPort dmPort = genericDomainModel.GenericDomainModelPort.First();
                Assert.Equal("GenericDomainModelPort_InDomainModel", dmPort.Name);
                Assert.Equal("type0", dmPort.Type);
                Assert.Equal("attr1", dmPort.GenericAttribute1);


                var componentAssembly = project.GetComponentAssemblyByName("ToplevelComponentAssembly").Single();
                var dm = CyPhy2DesignInterchange.CyPhy2DesignInterchange.Convert(componentAssembly);
                Assert.Equal("ToplevelComponentAssembly", dm.Name);

                var lowestContainer = dm.RootContainer.Container1.Single().Container1.Single();
                // lowest CA has GenericDomainModelPort that connects to the Component's port
                Assert.True(lowestContainer.Connector.Single().Role.Single().PortMap.Contains(
                    lowestContainer.ComponentInstance.First().PortInstance.First().ID));
                // lowest CA has Connector that connects to Component's Connector
                Assert.True(lowestContainer.Connector.Single().ConnectorComposition.Intersect(
                    lowestContainer.ComponentInstance.Single().ConnectorInstance.Select(ci => ci.ID)).Any());

                var toplevelConnector1 = dm.RootContainer.Connector.First();
                var toplevelConnector2 = dm.RootContainer.Connector.Skip(1).First();
                Assert.True(toplevelConnector1.Role.Single().PortMap.Contains(
                    toplevelConnector2.Role.Single().ID));
                Assert.Equal(typeof(avm.GenericDomainModelPort), toplevelConnector1.Role.Single().GetType());

                project.AbortTransaction();
            }
            finally
            {
                project.Close(true);
            }
        }
    }
}
