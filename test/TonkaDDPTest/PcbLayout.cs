using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using META;
using Xunit;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GME.MGA;
using GME.CSharp;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using CyPhy2DesignInterchange;
using Ionic.Zip;
using System.Globalization;

namespace TonkaDDPTest
{
    public class PcbLayoutFixture : IDisposable
    {
        public static String path_Test = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                      "..\\..\\..\\..",
                                                      "test",
                                                      "TonkaDDPTest",
                                                      "Model");

        private static String path_XME = Path.Combine(path_Test,
                                                      "testmodel.xme");

        private String path_MGA;
        public MgaProject proj { get; private set; }

        public PcbLayoutFixture()
        {
            String mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(path_XME, out mgaConnectionString);
            path_MGA = mgaConnectionString.Substring("MGA=".Length);

            Assert.True(File.Exists(Path.GetFullPath(path_MGA)),
                        String.Format("{0} not found. Model import may have failed.", path_MGA));

            proj = new MgaProject();
            bool ro_mode;
            proj.Open("MGA=" + Path.GetFullPath(path_MGA), out ro_mode);
            proj.EnableAutoAddOns(true);
            mgaGateway = new MgaGateway(proj);
            proj.CreateTerritoryWithoutSink(out mgaGateway.territory);

            try
            {
                PerformInTransaction(delegate
                {
                    var objAsm = proj.get_ObjectByPath("/@ComponentAssemblies/@Assembly");
                    Assert.NotNull(objAsm);
                    var ca = CyPhyClasses.ComponentAssembly.Cast(objAsm);
                    Assert.NotNull(ca);

                    var exporter = new CyPhyDesignExporter.CyPhyDesignExporterInterpreter();
                    var pathADP = exporter.ExportToPackage(ca, path_Test);

                    using (ZipFile zip = ZipFile.Read(pathADP))
                    {
                        var entryADM = zip.FirstOrDefault(ze => ze.FileName.Contains(".adm"));
                        Assert.True(entryADM != null, "No ADM file found in extracted ADP");

                        entryADM.Extract(path_Test, ExtractExistingFileAction.OverwriteSilently);

                        var xml = File.ReadAllText(Path.Combine(path_Test, entryADM.FileName));
                        exportedDesign = XSD2CSharp.AvmXmlSerializer.Deserialize<avm.Design>(xml);
                    }

                    var importer = new CyPhyDesignImporter.AVMDesignImporter(null, proj as IMgaProject, null);
                    var objImported = importer.ImportDesign(exportedDesign, CyPhyDesignImporter.AVMDesignImporter.DesignImportMode.CREATE_CAS);
                    Assert.NotNull(objImported);
                    importedDesign = CyPhyClasses.ComponentAssembly.Cast(objImported.Impl);
                });
            }
            catch (Exception)
            {
                proj.Close();
                throw;
            }
        }

        private MgaGateway mgaGateway;
        public void PerformInTransaction(Action del)
        {
            mgaGateway.PerformInTransaction(() => del(), abort: false);
        }

        public avm.Design exportedDesign;
        public CyPhy.ComponentAssembly importedDesign;

        public void Dispose()
        {
            proj.Save();
            proj.Close();
            proj = null;
        }
    }

    public class PcbLayoutFixtureTest
    {
        [Fact]
        [Trait("Schematic", "Layout")]
        public void TestFixture()
        {
            using (var fixture = new PcbLayoutFixture()) { }
        }
    }

    public class PcbLayout : IUseFixture<PcbLayoutFixture>
    {
        #region Fixture
        PcbLayoutFixture fixture;
        public void SetFixture(PcbLayoutFixture data)
        {
            fixture = data;
        }
        #endregion

        #region Convenience Functions
        private MgaProject project
        {
            get
            {
                return fixture.proj;
            }
        }

        private String TestPath
        {
            get
            {
                return PcbLayoutFixture.path_Test;
            }
        }

        private avm.Design design
        {
            get
            {
                return fixture.exportedDesign;
            }
        }

        private avm.Container container
        {
            get
            {
                return design.RootContainer;
            }
        }

        private CyPhy.ComponentAssembly componentAssembly
        {
            get
            {
                return fixture.importedDesign;
            }
        }
        #endregion
        
        private const String DefaultNote = "Note";

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_HasExactConstraints()
        {
            var constraints = container.ContainerFeature.OfType<avm.schematic.eda.ExactLayoutConstraint>();
            Assert.Equal(10, constraints.Count());
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Exact1()
        {
            String nameComp = "C1a";
            double x = 1.1;
            double y = 2.2;
            var layer = avm.schematic.eda.LayerEnum.Top;
            var rot = avm.schematic.eda.RotationEnum.r0;

            Export_TestExactLayoutConstraint(nameComp, x, y, layer, rot);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Exact2()
        {
            String nameComp = "C1b";
            double x = 1.1;
            double y = 2.2;
            var layer = avm.schematic.eda.LayerEnum.Bottom;
            var rot = avm.schematic.eda.RotationEnum.r90;

            Export_TestExactLayoutConstraint(nameComp, x, y, layer, rot);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Exact3()
        {
            String nameComp = "C1c";
            double x = 1.1;
            double y = 2.2;
            var layer = avm.schematic.eda.LayerEnum.Bottom;
            var rot = avm.schematic.eda.RotationEnum.r180;

            Export_TestExactLayoutConstraint(nameComp, x, y, layer, rot);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Exact4()
        {
            String nameComp = "C1d";
            double x = 1.1;
            double y = 2.2;
            var layer = avm.schematic.eda.LayerEnum.Bottom;
            var rot = avm.schematic.eda.RotationEnum.r270;

            Export_TestExactLayoutConstraint(nameComp, x, y, layer, rot);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        [Trait("Schematic", "ConstraintConversion")]
        public void Export_Exact_Partial1()
        {
            String nameComp = "C4a";
            double x = 5;

            var result = Export_ExactLayoutConstraint(nameComp);

            Assert.True(result.XSpecified);
            Assert.Equal(x, result.X);
            Assert.Equal(DefaultNote, result.Notes);

            Assert.False(result.YSpecified);
            Assert.False(result.RotationSpecified);
            Assert.False(result.LayerSpecified);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        [Trait("Schematic", "ConstraintConversion")]
        public void Export_Exact_Partial2()
        {
            String nameComp = "C4b";
            double y = 5;

            var result = Export_ExactLayoutConstraint(nameComp);

            Assert.True(result.YSpecified);
            Assert.Equal(y, result.Y);

            Assert.True(String.IsNullOrWhiteSpace(result.Notes));
            Assert.False(result.XSpecified);
            Assert.False(result.RotationSpecified);
            Assert.False(result.LayerSpecified);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        [Trait("Schematic", "ConstraintConversion")]
        public void Export_Exact_Partial3()
        {
            String nameComp = "C4c";

            var result = Export_ExactLayoutConstraint(nameComp);

            Assert.True(result.LayerSpecified);
            Assert.Equal(avm.schematic.eda.LayerEnum.Bottom, result.Layer);

            Assert.True(String.IsNullOrWhiteSpace(result.Notes));
            Assert.False(result.XSpecified);
            Assert.False(result.YSpecified);
            Assert.False(result.RotationSpecified);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        [Trait("Schematic", "ConstraintConversion")]
        public void Export_Exact_Partial4()
        {
            String nameComp = "C4d";

            var result = Export_ExactLayoutConstraint(nameComp);

            Assert.True(result.RotationSpecified);
            Assert.Equal(avm.schematic.eda.RotationEnum.r90, result.Rotation);

            Assert.True(String.IsNullOrWhiteSpace(result.Notes));
            Assert.False(result.XSpecified);
            Assert.False(result.YSpecified);
            Assert.False(result.LayerSpecified);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_CA_Inst_Exact()
        {
            String nameCont = "CAI1";
            double x = 1.0;
            double y = 1.0;
            var layer = avm.schematic.eda.LayerEnum.Bottom;
            var rot = avm.schematic.eda.RotationEnum.r90;

            Export_TestExactLayoutConstraint(nameCont, x, y, layer, rot);
        }

        private avm.schematic.eda.ExactLayoutConstraint Export_ExactLayoutConstraint(String nameTarget)
        {
            avm.schematic.eda.ExactLayoutConstraint rtn = null;
            fixture.PerformInTransaction(delegate
            {
                var comp = container.ComponentInstance.FirstOrDefault(c => c.Name.Equals(nameTarget));
                var cont = container.Container1.FirstOrDefault(c => c.Name.Equals(nameTarget));

                IEnumerable<avm.schematic.eda.ExactLayoutConstraint> constraints = null;
                if (comp != null)
                {
                    constraints = container.ContainerFeature.OfType<avm.schematic.eda.ExactLayoutConstraint>()
                                                           .Where(c => c.ConstraintTarget.Contains(comp.ID));
                }
                else if (cont != null)
                {
                    constraints = container.ContainerFeature.OfType<avm.schematic.eda.ExactLayoutConstraint>()
                                                           .Where(c => c.ContainerConstraintTarget.Contains(cont.ID));
                }
                else
                {
                    Assert.True(false, String.Format("No object found with the name {0}", nameTarget));
                }

                Assert.Equal(1, constraints.Count());

                rtn = constraints.First();
            });

            return rtn;
        }

        private void Export_TestExactLayoutConstraint(
            String nameTarget, 
            double x, 
            double y, 
            avm.schematic.eda.LayerEnum layer, 
            avm.schematic.eda.RotationEnum rot)
        {
            fixture.PerformInTransaction(delegate
            {
                var comp = container.ComponentInstance.FirstOrDefault(c => c.Name.Equals(nameTarget));
                var cont = container.Container1.FirstOrDefault(c => c.Name.Equals(nameTarget));

                IEnumerable<avm.schematic.eda.ExactLayoutConstraint> constraints = null;
                if (comp != null)
                {
                    constraints = container.ContainerFeature.OfType<avm.schematic.eda.ExactLayoutConstraint>()
                                                           .Where(c => c.ConstraintTarget.Contains(comp.ID));
                }
                else if (cont != null)
                {
                    constraints = container.ContainerFeature.OfType<avm.schematic.eda.ExactLayoutConstraint>()
                                                           .Where(c => c.ContainerConstraintTarget.Contains(cont.ID));
                }
                else
                {
                    Assert.True(false, String.Format("No object found with the name {0}", nameTarget));
                }

                Assert.Equal(1, constraints.Count());

                var constraint = constraints.First();
                Assert.Equal(x, constraint.X);
                Assert.Equal(y, constraint.Y);
                Assert.Equal(layer, constraint.Layer);
                Assert.Equal(rot, constraint.Rotation);

                Assert.Equal(DefaultNote, constraint.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_HasRangeConstraints()
        {
            var constraints = container.ContainerFeature.OfType<avm.schematic.eda.RangeLayoutConstraint>();
            Assert.Equal(6, constraints.Count());
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Range1()
        {
            var nameComp = "C2a";

            bool hasXRange = true;
            var xMin = 1.1;
            var xMax = 5.5;

            bool hasYRange = true;
            var yMin = 5.5;
            var yMax = 10.1;

            var layerRange = avm.schematic.eda.LayerRangeEnum.Either;
            var type = avm.schematic.eda.RangeConstraintTypeEnum.Inclusion;

            Export_TestRangeLayoutConstraint(nameComp, hasXRange, xMin, xMax, hasYRange, yMin, yMax, layerRange, type);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Range2()
        {
            var nameComp = "C2b";

            bool hasXRange = true;
            var xMin = 1.1;
            var xMax = 5.5;

            bool hasYRange = false;
            var yMin = 5.5;
            var yMax = 10.1;

            var layerRange = avm.schematic.eda.LayerRangeEnum.Top;
            var type = avm.schematic.eda.RangeConstraintTypeEnum.Exclusion;

            Export_TestRangeLayoutConstraint(nameComp, hasXRange, xMin, xMax, hasYRange, yMin, yMax, layerRange, type);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Range3()
        {
            var nameComp = "C2c";

            bool hasXRange = false;
            var xMin = 1.1;
            var xMax = 5.5;

            bool hasYRange = true;
            var yMin = 5.5;
            var yMax = 10.1;

            var layerRange = avm.schematic.eda.LayerRangeEnum.Bottom;

            Export_TestRangeLayoutConstraint(nameComp, hasXRange, xMin, xMax, hasYRange, yMin, yMax, layerRange);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Range4()
        {
            var nameComp = "C2d";

            bool hasXRange = false;
            var xMin = 0.0;
            var xMax = 0.0;

            bool hasYRange = false;
            var yMin = 0.0;
            var yMax = 0.0;

            var layerRange = avm.schematic.eda.LayerRangeEnum.Either;

            Export_TestRangeLayoutConstraint(nameComp, hasXRange, xMin, xMax, hasYRange, yMin, yMax, layerRange);
        }

        private void Export_TestRangeLayoutConstraint(
            string nameTarget,
            bool hasXRange,
            double xMin,
            double xMax,
            bool hasYRange,
            double yMin,
            double yMax,
            avm.schematic.eda.LayerRangeEnum layerRange,
            avm.schematic.eda.RangeConstraintTypeEnum type = avm.schematic.eda.RangeConstraintTypeEnum.Inclusion)
        {
            fixture.PerformInTransaction(delegate
            {
                var comp = container.ComponentInstance.FirstOrDefault(c => c.Name.Equals(nameTarget));
                var cont = container.Container1.FirstOrDefault(c => c.Name.Equals(nameTarget));

                IEnumerable<avm.schematic.eda.RangeLayoutConstraint> constraints = null;
                if (comp != null)
                {
                    constraints = container.ContainerFeature.OfType<avm.schematic.eda.RangeLayoutConstraint>()
                                                            .Where(c => c.ConstraintTarget.Contains(comp.ID));
                }
                else if (cont != null)
                {
                    constraints = container.ContainerFeature.OfType<avm.schematic.eda.RangeLayoutConstraint>()
                                                            .Where(c => c.ContainerConstraintTarget.Contains(cont.ID));
                }
                else
                {
                    Assert.True(false, String.Format("No object found with the name {0}", nameTarget));
                }

                Assert.Equal(1, constraints.Count());
                var constraint = constraints.First();

                if (hasXRange)
                {
                    Assert.Equal(xMin, constraint.XRangeMin);
                    Assert.Equal(xMax, constraint.XRangeMax);
                }

                if (hasYRange)
                {
                    Assert.Equal(yMin, constraint.YRangeMin);
                    Assert.Equal(yMax, constraint.YRangeMax);
                }

                Assert.Equal(layerRange, constraint.LayerRange);
                Assert.Equal(type, constraint.Type);

                Assert.Equal(DefaultNote, constraint.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_HasRelativeConstraints()
        {
            var constraints = container.ContainerFeature.OfType<avm.schematic.eda.RelativeLayoutConstraint>();
            Assert.Equal(3, constraints.Count());
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Relative1()
        {
            var nameCompConstrained = "C3a1";
            var nameCompOrigin = "C3a2";
            double? xOffset = 5.1;
            double? yOffset = 10.2;
            avm.schematic.eda.RelativeLayerEnum? relLayer = null;
            avm.schematic.eda.RelativeRotationEnum? rotation = avm.schematic.eda.RelativeRotationEnum.NoRestriction;

            Export_TestRelativeLayoutConstraint(nameCompConstrained, nameCompOrigin, xOffset, yOffset, relLayer, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Relative2()
        {
            var nameCompConstrained = "C3b1";
            var nameCompOrigin = "C3b2";
            double? xOffset = -5.1;
            double? yOffset = -10.2;
            avm.schematic.eda.RelativeLayerEnum? relLayer = avm.schematic.eda.RelativeLayerEnum.Same;
            avm.schematic.eda.RelativeRotationEnum? rotation = avm.schematic.eda.RelativeRotationEnum.r0;

            Export_TestRelativeLayoutConstraint(nameCompConstrained, nameCompOrigin, xOffset, yOffset, relLayer, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Relative3()
        {
            var nameCompConstrained = "C3c1";
            var nameCompOrigin = "C3c2";
            double? xOffset = null;
            double? yOffset = null;
            avm.schematic.eda.RelativeLayerEnum? relLayer = avm.schematic.eda.RelativeLayerEnum.Opposite;
            avm.schematic.eda.RelativeRotationEnum? rotation = avm.schematic.eda.RelativeRotationEnum.r270;

            Export_TestRelativeLayoutConstraint(nameCompConstrained, nameCompOrigin, xOffset, yOffset, relLayer, rotation);
        }

        private void Export_TestRelativeLayoutConstraint(
            string nameCompConstrained, 
            string nameCompOrigin, 
            double? xOffset, 
            double? yOffset, 
            avm.schematic.eda.RelativeLayerEnum? relLayer,
            avm.schematic.eda.RelativeRotationEnum? rotation)
        {
            var compConstrained = container.ComponentInstance.First(c => c.Name.Equals(nameCompConstrained));
            Assert.NotNull(compConstrained);

            var constraints = container.ContainerFeature.OfType<avm.schematic.eda.RelativeLayoutConstraint>()
                                                        .Where(c => c.ConstraintTarget.Contains(compConstrained.ID));
            Assert.Equal(1, constraints.Count());

            var constraint = constraints.First();

            var compOrigin = container.ComponentInstance.FirstOrDefault(ci => ci.ID.Equals(constraint.Origin));
            Assert.NotNull(compOrigin);
            Assert.Equal(nameCompOrigin, compOrigin.Name);

            Assert.Equal(xOffset.HasValue, constraint.XOffsetSpecified);
            if (xOffset.HasValue)
            {
                Assert.Equal(xOffset.Value, constraint.XOffset);
            }

            Assert.Equal(yOffset.HasValue, constraint.YOffsetSpecified);
            if (yOffset.HasValue)
            {
                Assert.Equal(yOffset.Value, constraint.YOffset);
            }

            Assert.Equal(relLayer.HasValue, constraint.RelativeLayerSpecified);
            if (relLayer.HasValue)
            {
                Assert.Equal(relLayer.Value, constraint.RelativeLayer);
            }

            Assert.Equal(rotation.HasValue, constraint.RelativeRotationSpecified);
            if (rotation.HasValue)
            {
                Assert.Equal(rotation.Value, constraint.RelativeRotation);
            }

            Assert.Equal(DefaultNote, constraint.Notes);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_RelativeRange1()
        {
            var nameCompConstrained = "C5b";
            var nameCompOrigin = "C5a";
            double? xMinOffset = 0;
            double? xMaxOffset = 5;
            double? yMinOffset = -1;
            double? yMaxOffset = 2;
            avm.schematic.eda.RelativeLayerEnum? relLayer = null;

            Export_TestRelativeRangeLayoutConstraint(nameCompConstrained, nameCompOrigin, xMinOffset, xMaxOffset, yMinOffset, yMaxOffset, relLayer);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_RelativeRange2()
        {
            var nameCompConstrained = "C5c";
            var nameCompOrigin = "C5b";
            double? xMinOffset = -6;
            double? xMaxOffset = -2;
            double? yMinOffset = -5;
            double? yMaxOffset = -1;
            avm.schematic.eda.RelativeLayerEnum? relLayer = avm.schematic.eda.RelativeLayerEnum.Same;

            Export_TestRelativeRangeLayoutConstraint(nameCompConstrained, nameCompOrigin, xMinOffset, xMaxOffset, yMinOffset, yMaxOffset, relLayer);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_RelativeRange3()
        {
            var nameCompConstrained = "C5d";
            var nameCompOrigin = "C5c";
            double? xMinOffset = -3;
            double? xMaxOffset = 3;
            double? yMinOffset = -2;
            double? yMaxOffset = 2;
            avm.schematic.eda.RelativeLayerEnum? relLayer = avm.schematic.eda.RelativeLayerEnum.Opposite;

            Export_TestRelativeRangeLayoutConstraint(nameCompConstrained, nameCompOrigin, xMinOffset, xMaxOffset, yMinOffset, yMaxOffset, relLayer);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_RelativeRange_CA()
        {
            var nameCompConstrained = "Ca5";
            var nameCompOrigin = "C5c";
            double? xMinOffset = -3;
            double? xMaxOffset = 3;
            double? yMinOffset = -2;
            double? yMaxOffset = 2;
            avm.schematic.eda.RelativeLayerEnum? relLayer = avm.schematic.eda.RelativeLayerEnum.Opposite;

            fixture.PerformInTransaction(delegate
            {
                var containerConstrained = container.Container1.First(c => c.Name.Equals(nameCompConstrained));
                Assert.NotNull(containerConstrained);

                var constraints = container.ContainerFeature.OfType<avm.schematic.eda.RelativeRangeLayoutConstraint>()
                                                            .Where(c => c.ContainerConstraintTarget.Contains(containerConstrained.ID));
                Assert.Equal(1, constraints.Count());

                var constraint = constraints.First();

                var compOrigin = container.ComponentInstance.FirstOrDefault(ci => ci.ID.Equals(constraint.Origin));
                Assert.NotNull(compOrigin);
                Assert.Equal(nameCompOrigin, compOrigin.Name);

                Assert.Equal(xMinOffset.HasValue, constraint.XRelativeRangeMinSpecified);
                if (xMinOffset.HasValue)
                {
                    Assert.Equal(xMinOffset.Value, constraint.XRelativeRangeMin);
                }

                Assert.Equal(xMaxOffset.HasValue, constraint.XRelativeRangeMaxSpecified);
                if (xMaxOffset.HasValue)
                {
                    Assert.Equal(xMaxOffset.Value, constraint.XRelativeRangeMax);
                }

                Assert.Equal(yMinOffset.HasValue, constraint.YRelativeRangeMinSpecified);
                if (yMinOffset.HasValue)
                {
                    Assert.Equal(yMinOffset.Value, constraint.YRelativeRangeMin);
                }

                Assert.Equal(yMaxOffset.HasValue, constraint.YRelativeRangeMaxSpecified);
                if (yMaxOffset.HasValue)
                {
                    Assert.Equal(yMaxOffset.Value, constraint.YRelativeRangeMax);
                }

                Assert.Equal(relLayer.HasValue, constraint.RelativeLayerSpecified);
                if (relLayer.HasValue)
                {
                    Assert.Equal(relLayer.Value, constraint.RelativeLayer);
                }
            });
        }

        private void Export_TestRelativeRangeLayoutConstraint(
            string nameCompConstrained, 
            string nameCompOrigin, 
            double? xMinOffset, 
            double? xMaxOffset, 
            double? yMinOffset, 
            double? yMaxOffset, 
            avm.schematic.eda.RelativeLayerEnum? relLayer)
        {
            var compConstrained = container.ComponentInstance.First(c => c.Name.Equals(nameCompConstrained));
            Assert.NotNull(compConstrained);

            var constraints = container.ContainerFeature.OfType<avm.schematic.eda.RelativeRangeLayoutConstraint>()
                                                        .Where(c => c.ConstraintTarget.Contains(compConstrained.ID));
            Assert.Equal(1, constraints.Count());

            var constraint = constraints.First();

            var compOrigin = container.ComponentInstance.FirstOrDefault(ci => ci.ID.Equals(constraint.Origin));
            Assert.NotNull(compOrigin);
            Assert.Equal(nameCompOrigin, compOrigin.Name);

            Assert.Equal(xMinOffset.HasValue, constraint.XRelativeRangeMinSpecified);
            if (xMinOffset.HasValue)
            {
                Assert.Equal(xMinOffset.Value, constraint.XRelativeRangeMin);
            }

            Assert.Equal(xMaxOffset.HasValue, constraint.XRelativeRangeMaxSpecified);
            if (xMaxOffset.HasValue)
            {
                Assert.Equal(xMaxOffset.Value, constraint.XRelativeRangeMax);
            }

            Assert.Equal(yMinOffset.HasValue, constraint.YRelativeRangeMinSpecified);
            if (yMinOffset.HasValue)
            {
                Assert.Equal(yMinOffset.Value, constraint.YRelativeRangeMin);
            }

            Assert.Equal(yMaxOffset.HasValue, constraint.YRelativeRangeMaxSpecified);
            if (yMaxOffset.HasValue)
            {
                Assert.Equal(yMaxOffset.Value, constraint.YRelativeRangeMax);
            }

            Assert.Equal(relLayer.HasValue, constraint.RelativeLayerSpecified);
            if (relLayer.HasValue)
            {
                Assert.Equal(relLayer.Value, constraint.RelativeLayer);
            }

            Assert.Equal(DefaultNote, constraint.Notes);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Exact_ComponentAssemblyInstance()
        {
            var nameCA = "CAI1";
            var x = 1.0;
            var y = 1.0;
            var Layer = avm.schematic.eda.LayerEnum.Bottom;
            var Rotation = avm.schematic.eda.RotationEnum.r90;

            Test_Exact_OnContainer(nameCA, x, y, Layer, Rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Exact_ComponentAssemblyReference()
        {
            var nameCA = "CAR1";
            var x = 1.0;
            var y = 1.0;
            var Layer = avm.schematic.eda.LayerEnum.Bottom;
            var Rotation = avm.schematic.eda.RotationEnum.r90;

            Test_Exact_OnContainer(nameCA, x, y, Layer, Rotation);
        }

        private void Test_Exact_OnContainer(string nameContainer, double x, double y, avm.schematic.eda.LayerEnum Layer, avm.schematic.eda.RotationEnum Rotation)
        {
            fixture.PerformInTransaction(delegate
            {
                var caConstrained = container.Container1.First(c => c.Name.Equals(nameContainer));
                Assert.NotNull(caConstrained);

                var constraints = container.ContainerFeature.OfType<avm.schematic.eda.ExactLayoutConstraint>()
                                                            .Where(c => c.ContainerConstraintTarget.Contains(caConstrained.ID));
                Assert.Equal(1, constraints.Count());

                var constraint = constraints.First();
                Assert.Equal(x, constraint.X);
                Assert.Equal(y, constraint.Y);
                Assert.Equal(Layer, constraint.Layer);
                Assert.Equal(Rotation, constraint.Rotation);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Range_ComponentAssemblyInstance()
        {
            var nameCA = "CAI1";
            var xMin = 0.0;
            var xMax = 1.0;
            var yMin = 2.0;
            var yMax = 3.0;
            var layer = avm.schematic.eda.LayerRangeEnum.Bottom;

            Test_Range_OnContainer(nameCA, xMin, xMax, yMin, yMax, layer);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_Range_ComponentAssemblyReference()
        {
            var nameCA = "CAR1";
            var xMin = 0.0;
            var xMax = 1.0;
            var yMin = 2.0;
            var yMax = 3.0;
            var layer = avm.schematic.eda.LayerRangeEnum.Bottom;

            Test_Range_OnContainer(nameCA, xMin, xMax, yMin, yMax, layer);
        }

        private void Test_Range_OnContainer(string nameContainer, double xMin, double xMax, double yMin, double yMax, avm.schematic.eda.LayerRangeEnum layer)
        {
            fixture.PerformInTransaction(delegate
            {
                var caConstrained = container.Container1.First(c => c.Name.Equals(nameContainer));
                Assert.NotNull(caConstrained);

                var constraints = container.ContainerFeature.OfType<avm.schematic.eda.RangeLayoutConstraint>()
                                                            .Where(c => c.ContainerConstraintTarget.Contains(caConstrained.ID));
                Assert.Equal(1, constraints.Count());

                var constraint = constraints.First();
                Assert.Equal(xMin, constraint.XRangeMin);
                Assert.Equal(xMax, constraint.XRangeMax);
                Assert.Equal(yMin, constraint.YRangeMin);
                Assert.Equal(yMax, constraint.YRangeMax);
                Assert.Equal(layer, constraint.LayerRange);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_GlobalConstraintException_Component()
        {
            var nameComp = "C6";
            
            Test_Global_Exception_Component(nameComp, avm.schematic.eda.GlobalConstraintTypeEnum.BoardEdgeSpacing);
        }

        private void Test_Global_Exception_Component(string nameComp, avm.schematic.eda.GlobalConstraintTypeEnum excludedType)
        {
            fixture.PerformInTransaction(delegate
            {
                var compConstrained = container.ComponentInstance.First(c => c.Name.Equals(nameComp));
                Assert.NotNull(compConstrained);

                var constraints = container.ContainerFeature.OfType<avm.schematic.eda.GlobalLayoutConstraintException>()
                                                            .Where(c => c.ConstraintTarget.Contains(compConstrained.ID));
                var constraint = constraints.First(c => c.Constraint == excludedType);

                Assert.Equal(DefaultNote, constraint.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Export_GlobalConstraintException_ComponentAssembly()
        {
            var nameContainer = "Ca6";
            
            Test_GlobalException_Container(nameContainer, avm.schematic.eda.GlobalConstraintTypeEnum.BoardEdgeSpacing);
        }
        
        private void Test_GlobalException_Container(string nameContainer, avm.schematic.eda.GlobalConstraintTypeEnum excludedType)
        {
            fixture.PerformInTransaction(delegate
            {
                var contConstrained = container.Container1.First(c => c.Name.Equals(nameContainer));
                Assert.NotNull(contConstrained);

                var constraints = container.ContainerFeature.OfType<avm.schematic.eda.GlobalLayoutConstraintException>()
                                                            .Where(c => c.ContainerConstraintTarget.Contains(contConstrained.ID));
                var constraint = constraints.First(c => c.Constraint == excludedType);

                Assert.Equal(DefaultNote, constraint.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact1()
        {
            var nameComp = "C1a";
            var x = 1.1;
            var y = 2.2;
            var layer = 0;
            var rotation = 0;

            Import_TestExactLayoutConstraint(nameComp, x, y, layer, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact2()
        {
            var nameComp = "C1b";
            double x = 1.1;
            double y = 2.2;
            var layer = 1;
            var rotation = 1;

            Import_TestExactLayoutConstraint(nameComp, x, y, layer, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact3()
        {
            var nameComp = "C1c";
            var x = 1.1;
            var y = 2.2;
            var layer = 1;
            var rotation = 2;

            Import_TestExactLayoutConstraint(nameComp, x, y, layer, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact4()
        {
            var nameComp = "C1d";
            var x = 1.1;
            var y = 2.2;
            var layer = 1;
            var rotation = 3;

            Import_TestExactLayoutConstraint(nameComp, x, y, layer, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact_ComponentAssemblyInstance()
        {
            var nameCA = "CAI1";
            var x = 1.0;
            var y = 1.0;
            var layer = 1;
            var rotation = 1;

            Import_TestExactLayoutConstraint(nameCA, x, y, layer, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact_ComponentAssemblyReference()
        {
            var nameCA = "CAR1";
            var x = 1.0;
            var y = 1.0;
            var layer = 1;
            var rotation = 1;

            Import_TestExactLayoutConstraint(nameCA, x, y, layer, rotation);
        }

        private void Import_TestExactLayoutConstraint(string nameTarget, double x, double y, int layer, int rotation)
        {
            fixture.PerformInTransaction(delegate
            {
                var comp = componentAssembly.Children.ComponentRefCollection.FirstOrDefault(c => c.Name.Equals(nameTarget));
                var compAsm = componentAssembly.Children.ComponentAssemblyCollection.FirstOrDefault(c => c.Name.Equals(nameTarget));

                IEnumerable<CyPhy.ExactLayoutConstraint> constraints = null;
                if (comp != null)
                {
                    constraints = comp.SrcConnections.ApplyExactLayoutConstraintCollection.Select(c => c.SrcEnds.ExactLayoutConstraint);
                }
                else if (compAsm != null)
                {
                    constraints = compAsm.SrcConnections.ApplyExactLayoutConstraintCollection.Select(ca => ca.SrcEnds.ExactLayoutConstraint);
                }
                else
                {
                    Assert.True(false, String.Format("No component or component assembly found with name {0}", nameTarget));
                }

                Assert.Equal(1, constraints.Count());
                var constraint = constraints.First();

                Assert.Equal(x, double.Parse(constraint.Attributes.X, CultureInfo.InvariantCulture));
                Assert.Equal(y, double.Parse(constraint.Attributes.Y, CultureInfo.InvariantCulture));
                Assert.Equal(layer.ToString(CultureInfo.InvariantCulture), constraint.Attributes.Layer);
                Assert.Equal(rotation.ToString(CultureInfo.InvariantCulture), constraint.Attributes.Rotation);

                Assert.Equal(DefaultNote, constraint.Attributes.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact_Partial1()
        {
            String nameComp = "C4a";

            var result = Import_ExactLayoutConstraint(nameComp);

            Assert.Equal("5", result.x);
            Assert.Equal(DefaultNote, result.notes);

            Assert.True(String.IsNullOrWhiteSpace(result.y));
            Assert.True(String.IsNullOrWhiteSpace(result.layer));
            Assert.True(String.IsNullOrWhiteSpace(result.rotation));
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact_Partial2()
        {
            String nameComp = "C4b";

            var result = Import_ExactLayoutConstraint(nameComp);

            Assert.Equal("5", result.y);

            Assert.True(String.IsNullOrWhiteSpace(result.notes));
            Assert.True(String.IsNullOrWhiteSpace(result.x));
            Assert.True(String.IsNullOrWhiteSpace(result.layer));
            Assert.True(String.IsNullOrWhiteSpace(result.rotation));
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact_Partial3()
        {
            String nameComp = "C4c";

            var result = Import_ExactLayoutConstraint(nameComp);

            Assert.Equal("1", result.layer);

            Assert.True(String.IsNullOrWhiteSpace(result.notes));
            Assert.True(String.IsNullOrWhiteSpace(result.x));
            Assert.True(String.IsNullOrWhiteSpace(result.y));
            Assert.True(String.IsNullOrWhiteSpace(result.rotation));
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Exact_Partial4()
        {
            String nameComp = "C4d";

            var result = Import_ExactLayoutConstraint(nameComp);

            Assert.Equal("1", result.rotation);

            Assert.True(String.IsNullOrWhiteSpace(result.notes));
            Assert.True(String.IsNullOrWhiteSpace(result.x));
            Assert.True(String.IsNullOrWhiteSpace(result.y));
            Assert.True(String.IsNullOrWhiteSpace(result.layer));
        }

        private class Exact
        {
            public String x;
            public String y;
            public String layer;
            public String rotation;
            public String notes;
        }

        private Exact Import_ExactLayoutConstraint(string nameTarget)
        {
            Exact rtn = null;

            fixture.PerformInTransaction(delegate
            {
                var comp = componentAssembly.Children.ComponentRefCollection.FirstOrDefault(c => c.Name.Equals(nameTarget));
                var compAsm = componentAssembly.Children.ComponentAssemblyCollection.FirstOrDefault(c => c.Name.Equals(nameTarget));

                IEnumerable<CyPhy.ExactLayoutConstraint> constraints = null;
                if (comp != null)
                {
                    constraints = comp.SrcConnections.ApplyExactLayoutConstraintCollection.Select(c => c.SrcEnds.ExactLayoutConstraint);
                }
                else if (compAsm != null)
                {
                    constraints = compAsm.SrcConnections.ApplyExactLayoutConstraintCollection.Select(ca => ca.SrcEnds.ExactLayoutConstraint);
                }
                else
                {
                    Assert.True(false, String.Format("No component or component assembly found with name {0}", nameTarget));
                }

                Assert.Equal(1, constraints.Count());
                var constraint = constraints.First();

                rtn = new Exact()
                {
                    x = constraint.Attributes.X,
                    y = constraint.Attributes.Y,
                    layer = constraint.Attributes.Layer,
                    rotation = constraint.Attributes.Rotation,
                    notes = constraint.Attributes.Notes                    
                };
            });

            return rtn;
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Range1()
        {
            var nameComp = "C2a";

            var xRange = String.Format("{0}:{1}", 1.1f, 5.5f);
            var yRange = String.Format("{0}:{1}", 5.5f, 10.1f);

            var layerRange = "0:1";
            var type = CyPhyClasses.RangeLayoutConstraint.AttributesClass.Type_enum.Inclusion;

            Import_TestRangeConstraint(nameComp, xRange, yRange, layerRange, type);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Range2()
        {
            var nameComp = "C2b";

            var xRange = String.Format("{0}:{1}", 1.1f, 5.5f);
            var yRange = "";

            var layerRange = "0";
            var type = CyPhyClasses.RangeLayoutConstraint.AttributesClass.Type_enum.Exclusion;

            Import_TestRangeConstraint(nameComp, xRange, yRange, layerRange, type);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Range3()
        {
            var nameComp = "C2c";

            var xRange = "";
            var yRange = String.Format("{0}:{1}", 5.5f, 10.1f);

            var layerRange = "1";

            Import_TestRangeConstraint(nameComp, xRange, yRange, layerRange);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Range_ComponentAssemblyInstance()
        {
            var nameComp = "CAI1";

            var xRange = String.Format("{0}:{1}", 0.0f, 1.0f);
            var yRange = String.Format("{0}:{1}", 2.0f, 3.0f);

            var layerRange = "1";

            Import_TestRangeConstraint(nameComp, xRange, yRange, layerRange);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Range_ComponentAssemblyReference()
        {
            var nameComp = "CAR1";

            var xRange = String.Format("{0}:{1}", 0.0f, 1.0f);
            var yRange = String.Format("{0}:{1}", 2.0f, 3.0f);

            var layerRange = "1";

            Import_TestRangeConstraint(nameComp, xRange, yRange, layerRange);
        }

        private void Import_TestRangeConstraint(
            string nameTarget, 
            string xRange, 
            string yRange, 
            string layerRange,
            CyPhyClasses.RangeLayoutConstraint.AttributesClass.Type_enum type = CyPhyClasses.RangeLayoutConstraint.AttributesClass.Type_enum.Inclusion)
        {
            fixture.PerformInTransaction(delegate
            {
                var comp = componentAssembly.Children.ComponentRefCollection.FirstOrDefault(c => c.Name.Equals(nameTarget));
                var compAsm = componentAssembly.Children.ComponentAssemblyCollection.FirstOrDefault(c => c.Name.Equals(nameTarget));

                IEnumerable<CyPhy.RangeLayoutConstraint> constraints = null;
                if (comp != null)
                {
                    constraints = comp.SrcConnections.ApplyRangeLayoutConstraintCollection.Select(c => c.SrcEnds.RangeLayoutConstraint);
                }
                else if (compAsm != null)
                {
                    constraints = compAsm.SrcConnections.ApplyRangeLayoutConstraintCollection.Select(ca => ca.SrcEnds.RangeLayoutConstraint);
                }
                else
                {
                    Assert.True(false, String.Format("No component or component assembly found with name {0}", nameTarget));
                }

                Assert.Equal(1, constraints.Count());
                var constraint = constraints.First();

                Assert.Equal(xRange, constraint.Attributes.XRange);
                Assert.Equal(yRange, constraint.Attributes.YRange);

                Assert.Equal(layerRange, constraint.Attributes.LayerRange);
                Assert.Equal(type, constraint.Attributes.Type);

                Assert.Equal(DefaultNote, constraint.Attributes.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Relative1()
        {
            var nameComp = "C3a1";
            var nameCompOrigin = "C3a2";
            String xOffset = "5.1";
            String yOffset = "10.2";
            var layerRange = ISIS.GME.Dsml.CyPhyML.Classes.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.No_Restriction;
            var rotation = ISIS.GME.Dsml.CyPhyML.Classes.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum.No_Restriction;

            Import_TestRelativeLayoutConstraint(nameComp, nameCompOrigin, xOffset, yOffset, layerRange, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Relative2()
        {
            var nameComp = "C3b1";
            var nameCompOrigin = "C3b2";
            String xOffset = "-5.1";
            String yOffset = "-10.2";
            var layerRange = ISIS.GME.Dsml.CyPhyML.Classes.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.Same;
            var rotation = ISIS.GME.Dsml.CyPhyML.Classes.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._0;

            Import_TestRelativeLayoutConstraint(nameComp, nameCompOrigin, xOffset, yOffset, layerRange, rotation);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_Relative3()
        {
            var nameComp = "C3c1";
            var nameCompOrigin = "C3c2";
            String xOffset = "";
            String yOffset = "";
            var layerRange = ISIS.GME.Dsml.CyPhyML.Classes.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.Opposite;
            var rotation = ISIS.GME.Dsml.CyPhyML.Classes.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._270;

            Import_TestRelativeLayoutConstraint(nameComp, nameCompOrigin, xOffset, yOffset, layerRange, rotation);
        }

        private void Import_TestRelativeLayoutConstraint(
            string nameComp, 
            string nameCompOrigin, 
            String xOffset, 
            String yOffset, 
            ISIS.GME.Dsml.CyPhyML.Classes.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum layerRange,
            ISIS.GME.Dsml.CyPhyML.Classes.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum rotation)
        {
            fixture.PerformInTransaction(delegate
            {
                var compTarget = componentAssembly.Children.ComponentRefCollection.FirstOrDefault(c => c.Name.Equals(nameComp));
                Assert.NotNull(compTarget);

                var constraints = compTarget.SrcConnections.ApplyRelativeLayoutConstraintCollection.Select(c => c.SrcEnds.RelativeLayoutConstraint);
                Assert.Equal(1, constraints.Count());
                var constraint = constraints.First();

                var compOrigin = constraint.SrcConnections.RelativeLayoutConstraintOriginCollection.First().SrcEnds.ComponentRef;
                Assert.Equal(nameCompOrigin, compOrigin.Name);

                Assert.Equal(xOffset, constraint.Attributes.XOffset);
                Assert.Equal(yOffset, constraint.Attributes.YOffset);
                Assert.Equal(layerRange, constraint.Attributes.RelativeLayer);
                Assert.Equal(rotation, constraint.Attributes.RelativeRotation);

                Assert.Equal(DefaultNote, constraint.Attributes.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_RelativeRange1()
        {
            var nameComp = "C5b";
            var nameCompOrigin = "C5a";
            String xRangeOffset = "0:5";
            String yRangeOffset = "-1:2";
            var layerRange = CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.No_Restriction;

            Import_TestRelativeRangeLayoutConstraint(nameComp, nameCompOrigin, xRangeOffset, yRangeOffset, layerRange);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_RelativeRange2()
        {
            var nameComp = "C5c";
            var nameCompOrigin = "C5b";
            String xRangeOffset = "-6:-2";
            String yRangeOffset = "-5:-1";
            var layerRange = CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.Same;

            Import_TestRelativeRangeLayoutConstraint(nameComp, nameCompOrigin, xRangeOffset, yRangeOffset, layerRange);
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_RelativeRange3()
        {
            var nameComp = "C5d";
            var nameCompOrigin = "C5c";
            String xRangeOffset = "-3:3";
            String yRangeOffset = "-2:2";
            var layerRange = CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.Opposite;

            Import_TestRelativeRangeLayoutConstraint(nameComp, nameCompOrigin, xRangeOffset, yRangeOffset, layerRange);
        }

        private void Import_TestRelativeRangeLayoutConstraint(
            string nameComp,
            string nameCompOrigin,
            String xRangeOffset,
            String yRangeOffset,
            CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum layerRange)
        {
            fixture.PerformInTransaction(delegate
            {
                var compTarget = componentAssembly.Children.ComponentRefCollection.FirstOrDefault(c => c.Name.Equals(nameComp));
                Assert.NotNull(compTarget);

                var constraints = compTarget.SrcConnections
                                            .ApplyRelativeRangeLayoutConstraintCollection
                                            .Select(c => c.SrcEnds.RelativeRangeConstraint);
                Assert.Equal(1, constraints.Count());
                var constraint = constraints.First();

                var compOrigin = constraint.SrcConnections
                                           .RelativeRangeLayoutConstraintOriginCollection
                                           .First()
                                           .SrcEnds
                                           .ComponentRef;
                Assert.Equal(nameCompOrigin, compOrigin.Name);

                Assert.Equal(xRangeOffset, constraint.Attributes.XOffsetRange);
                Assert.Equal(yRangeOffset, constraint.Attributes.YOffsetRange);
                Assert.Equal(layerRange, constraint.Attributes.RelativeLayer);

                Assert.Equal(DefaultNote, constraint.Attributes.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_RelativeRange_CA()
        {
            var nameCA = "Ca5";
            var nameCompOrigin = "C5c";
            String xRangeOffset = "-3:3";
            String yRangeOffset = "-2:2";
            var layerRange = CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.Opposite;

            fixture.PerformInTransaction(delegate
            {
                var compTarget = componentAssembly.Children.ComponentAssemblyCollection
                                                           .FirstOrDefault(c => c.Name.Equals(nameCA));
                Assert.NotNull(compTarget);

                var constraints = compTarget.SrcConnections
                                            .ApplyRelativeRangeLayoutConstraintCollection
                                            .Select(c => c.SrcEnds.RelativeRangeConstraint);
                Assert.Equal(1, constraints.Count());
                var constraint = constraints.First();

                var compOrigin = constraint.SrcConnections
                                           .RelativeRangeLayoutConstraintOriginCollection
                                           .First()
                                           .SrcEnds
                                           .ComponentRef;
                Assert.Equal(nameCompOrigin, compOrigin.Name);

                Assert.Equal(xRangeOffset, constraint.Attributes.XOffsetRange);
                Assert.Equal(yRangeOffset, constraint.Attributes.YOffsetRange);
                Assert.Equal(layerRange, constraint.Attributes.RelativeLayer);

                Assert.Equal(DefaultNote, constraint.Attributes.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_GlobalException_Comp()
        {
            var nameComp = "C6";

            Import_TestGlobalConstraintException_Comp(nameComp, CyPhyClasses.GlobalLayoutConstraintException.AttributesClass.Constraint_enum.Board_Edge_Spacing);
        }

        private void Import_TestGlobalConstraintException_Comp(string nameComp, CyPhyClasses.GlobalLayoutConstraintException.AttributesClass.Constraint_enum type)
        {
            fixture.PerformInTransaction(delegate
            {
                var compTarget = componentAssembly.Children.ComponentRefCollection
                                                           .FirstOrDefault(c => c.Name.Equals(nameComp));
                Assert.NotNull(compTarget);

                var constraints = compTarget.SrcConnections
                                            .ApplyGlobalLayoutConstraintExceptionCollection
                                            .Select(c => c.SrcEnds.GlobalLayoutConstraintException);
                var constraint = constraints.First(c => c.Attributes.Constraint == type);

                Assert.Equal(DefaultNote, constraint.Attributes.Notes);
            });
        }

        [Fact]
        [Trait("Schematic", "Layout")]
        public void Import_GlobalException_CA()
        {
            var nameCA = "Ca6";

            Import_TestGlobalConstraintException_CA(nameCA, CyPhyClasses.GlobalLayoutConstraintException.AttributesClass.Constraint_enum.Board_Edge_Spacing);
        }

        private void Import_TestGlobalConstraintException_CA(string nameCA, CyPhyClasses.GlobalLayoutConstraintException.AttributesClass.Constraint_enum type)
        {
            fixture.PerformInTransaction(delegate
            {
                var compTarget = componentAssembly.Children.ComponentAssemblyCollection
                                                           .FirstOrDefault(c => c.Name.Equals(nameCA));
                Assert.NotNull(compTarget);

                var constraints = compTarget.SrcConnections
                                            .ApplyGlobalLayoutConstraintExceptionCollection
                                            .Select(c => c.SrcEnds.GlobalLayoutConstraintException);
                var constraint = constraints.First(c => c.Attributes.Constraint == type);

                Assert.Equal(DefaultNote, constraint.Attributes.Notes);
            });
        }
    }
}
