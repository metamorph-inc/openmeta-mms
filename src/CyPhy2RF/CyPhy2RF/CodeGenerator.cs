using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using META;
using CyPhyGUIs;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using CyPhyInterfaces = ISIS.GME.Dsml.CyPhyML.Interfaces;

using System.IO;
using CSXCAD;
using Postprocess;
using System.Globalization;

namespace CyPhy2RF.RF
{
    public class CodeGenerator
    {
        private GMELogger Logger { get; set; }
        public static bool verbose { get; set; }

        public enum Mode
        {
            SAR,
            DIRECTIVITY
        }

        public Mode mode { get; private set; }

        static CodeGenerator()
        {
            verbose = false;
        }

        private CyPhyGUIs.IInterpreterMainParameters mainParameters { get; set; }

        private CSXCAD.Ara.Endo m_endo;
        private bool m_rfModelFound = false;
        private uint m_slotIndex;
        private double m_dutResolution;
        private double m_sarResolution = 5;
        private double m_f0;
        private const double m_fc = 500e6;
        private XmlCompound m_antenna;
        private bool m_excludeEndo = true;

        public CodeGenerator(CyPhyGUIs.IInterpreterMainParameters parameters, Mode mode, GMELogger Logger)
        {
            this.Logger = Logger;
            this.mainParameters = parameters;
            CodeGenerator.verbose = ((CyPhy2RF.CyPhy2RF_Settings)parameters.config).Verbose;
            this.mode = mode;
        }

        public void GenerateCode()
        {
            // Map the root testbench object
            var testbench = CyPhyClasses.TestBench.Cast(this.mainParameters.CurrentFCO);
            if (testbench == null)
            {
                Logger.WriteError("Invalid context of invocation <{0}>, invoke the interpreter from a Testbench model",
                    this.mainParameters.CurrentFCO.Name);
                return;
            }

            // Parameters
            var slotParameter = testbench.Children.ParameterCollection.FirstOrDefault(x => x.Name == "Slot");
            if (slotParameter == null)
            {
                throw new ApplicationException("Missing required Testbench Parameter 'Slot'");
            }
            if (UInt32.TryParse(slotParameter.Attributes.Value, out m_slotIndex) == false)
            {
                throw new ApplicationException("Testbench Parameter 'Slot' must be an integer");
            }
            var resolutionParameter = testbench.Children.ParameterCollection.FirstOrDefault(x => x.Name == "Resolution");
            if (resolutionParameter == null)
            {
                throw new ApplicationException("Missing required Testbench Parameter 'Resolution'");
            }
            if (Double.TryParse(resolutionParameter.Attributes.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out m_dutResolution) == false)
            {
                throw new ApplicationException("Testbench Parameter 'Resolution' must be a real number");
            }
            var frequencyParameter = testbench.Children.ParameterCollection.FirstOrDefault(x => x.Name == "Frequency");
            if (frequencyParameter == null)
            {
                throw new ApplicationException("Missing required Testbench Parameter 'Frequency'");
            }
            if (Double.TryParse(frequencyParameter.Attributes.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out m_f0) == false)
            {
                throw new ApplicationException("Testbench Parameter 'Frequency' must be a real number");
            }

            // Set up simulation space
            Logger.WriteInfo("Constructing Ara Phone model...");

            m_endo = new CSXCAD.Ara.Endo();
            Visit(testbench);

            switch (mode)
            {
                case Mode.DIRECTIVITY:
                    GenerateDirectivitySimulationInput();
                    break;
                case Mode.SAR:
                    GenerateSarSimulationInput();
                    break;
            };
        }

        private void GenerateDirectivitySimulationInput()
        {
            // Constants
            double unit = 1e-3;
            double c0 = 299792458.0;
            double lambda0 = c0 / m_f0;
            double lambdaMin = c0 / (m_f0 + m_fc);

            Compound simulationSpace = new Compound("space");
            Compound solidSpace = new Compound("solid-space");
            simulationSpace.Add(solidSpace);

            Compound dut;
            if (m_excludeEndo == true)
            {
                dut = m_endo.GetModule(m_slotIndex);
            }
            else
            {
                dut = m_endo;
            }
            solidSpace.Add(dut); // modifies dut parent (!)

            // Set up simulation grid, nf2ff and SAR
            Logger.WriteInfo("Constructing FDTD simulation grid...");
            double airBox = 40;
            double maxRes = Math.Round(lambdaMin / 20 / unit);
            double maxRatio = 1.5;

            RectilinearGrid grid = new RectilinearGrid();
            grid.Add(dut.BoundingBox.P1);
            grid.Add(dut.BoundingBox.P2);

            #region openems_workaround

            // openEMS v0.0.31 seems to handle transformations on excitation (lumped port),
            // SAR and NF2FF simulation components incorrectly.
            // Applied workarounds:
            // 1. The entire design is moved so that the antenna feedpoint is in the origin
            // 2. The SAR and NF2FF boxes are added late, w/o transformations

            Vector3D antennaPosition = new Vector3D(
                m_antenna.AbsoluteTransformation.X,
                m_antenna.AbsoluteTransformation.Y,
                m_antenna.AbsoluteTransformation.Z);

            solidSpace.Transformations.Add(new TTranslate(-antennaPosition));

            grid.Move(-antennaPosition);
            grid.Add(new Vector3D(0, 0, 0));
            grid.ZLines.Add(-(m_antenna.Parent as CSXCAD.Ara.PCB).Thickness);
            grid.Sort();

            grid.SmoothMesh(m_dutResolution, maxRatio);
            grid.AddAirbox(airBox);
            grid.SmoothMesh(maxRes, maxRatio);

            simulationSpace.Add(new NF2FFBox("nf2ff",
                new Vector3D(grid.XLines.First(), grid.YLines.First(), grid.ZLines.First()),
                new Vector3D(grid.XLines.Last(), grid.YLines.Last(), grid.ZLines.Last()),
                lambdaMin / 15 / unit));
            #endregion

            grid.AddPML(8);

            Simulation fdtd = new Simulation();
            fdtd.Excitation = new GaussExcitation(m_f0, m_fc);

            // Export
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("CyPhy generated openEMS simulation file"),
                new XElement("openEMS",
                    fdtd.ToXElement(),
                    new XElement("ContinuousStructure",
                        new XAttribute("CoordSystem", 0),
                        simulationSpace.ToXElement(),
                        grid.ToXElement()
                    )
                )
            );

            if (dut is CSXCAD.Ara.Module)
            {
                dut.Parent = m_endo;
            }

            string openEmsInput = Path.Combine(mainParameters.OutputDirectory, "openEMS_input.xml");
            doc.Save(openEmsInput);

            string nf2ffInput = Path.Combine(mainParameters.OutputDirectory, "nf2ff_input.xml");
            var nf2ff = new Postprocess.NF2FF(m_f0);
            nf2ff.ToXDocument().Save(nf2ffInput);
        }

        private void GenerateSarSimulationInput()
        {
            // Constants
            double unit = 1e-3;
            double c0 = 299792458.0;
            double lambda0 = c0 / m_f0;
            double f_c = 500e6;
            double lambdaMin = c0 / (m_f0 + f_c);

            Compound simulationSpace = new Compound("space");
            Compound solidSpace = new Compound("solid-space");
            simulationSpace.Add(solidSpace);
            solidSpace.Add(m_endo);

            var headPhantom = new CSXCAD.Ara.HeadPhantom();
            headPhantom.Transformations.Add(new TRotateX(Math.PI / 2));
            headPhantom.Transformations.Add(new TTranslate(32.0, 80.0, -headPhantom.Width / 2 - 7.0)); // TODO: Make endo width/height accessibles
            solidSpace.Add(headPhantom);

            // Set up simulation grid, nf2ff and SAR
            Logger.WriteInfo("Constructing FDTD simulation grid...");
            double airBox = 40;
            double envResolution = Math.Round(lambdaMin / 20 / unit);
            double maxRatio = 1.5;

            RectilinearGrid grid = new BoundingGrid_6x3();

            #region openems_workaround

            // openEMS v0.0.31 seems to handle transformations on excitation (lumped port),
            // SAR and NF2FF simulation components incorrectly.
            // Applied workarounds:
            // 1. The entire design is moved so that the antenna feedpoint is in the origin
            // 2. The SAR and NF2FF boxes are added late, w/o transformations

            Vector3D dutPosition = new Vector3D(
                m_antenna.AbsoluteTransformation.X,
                m_antenna.AbsoluteTransformation.Y,
                m_antenna.AbsoluteTransformation.Z);

            solidSpace.Transformations.Add(new TTranslate(-dutPosition));

            grid.Move(-dutPosition);
            grid.Add(new Vector3D(0, 0, 0));
            grid.ZLines.Add(-(m_antenna.Parent as CSXCAD.Ara.PCB).Thickness);
            grid.Sort();
            grid.SmoothMesh(m_dutResolution, maxRatio);

            grid.Add(headPhantom.BoundingBox.P1);
            grid.Add(headPhantom.BoundingBox.P2);
            grid.SmoothMesh(m_sarResolution, maxRatio);

            grid.AddAirbox(airBox);
            grid.SmoothMesh(envResolution, maxRatio);

            simulationSpace.Add(new SARBox("SAR", m_f0,
                new Vector3D(headPhantom.XGridPoints.First(), headPhantom.YGridPoints.First(), headPhantom.ZGridPoints.First()),
                new Vector3D(headPhantom.XGridPoints.Last(), headPhantom.YGridPoints.Last(), headPhantom.ZGridPoints.Last())));
            simulationSpace.Add(new NF2FFBox("nf2ff",
                new Vector3D(grid.XLines.First(), grid.YLines.First(), grid.ZLines.First()),
                new Vector3D(grid.XLines.Last(), grid.YLines.Last(), grid.ZLines.Last()),
                lambdaMin / 15 / unit));

            #endregion

            grid.AddPML(8);

            Simulation fdtd = new Simulation();
            fdtd.Excitation = new GaussExcitation(m_f0, m_fc);

            // Export
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("CyPhy generated openEMS simulation file"),
                new XElement("openEMS",
                    fdtd.ToXElement(),
                    new XElement("ContinuousStructure",
                        new XAttribute("CoordSystem", 0),
                        simulationSpace.ToXElement(),
                        grid.ToXElement()
                    )
                )
            );

            string openEmsInput = Path.Combine(mainParameters.OutputDirectory, "openEMS_input.xml");
            doc.Save(openEmsInput);

            string nf2ffInput = Path.Combine(mainParameters.OutputDirectory, "nf2ff_input.xml");
            var nf2ff = new Postprocess.NF2FF(m_f0);
            nf2ff.ToXDocument().Save(nf2ffInput);
        }

        private void Visit(CyPhyInterfaces.TestBench testBench)
        {
            if (testBench.Children.ComponentAssemblyCollection.Count() == 0)
            {
                Logger.WriteError("No valid component assembly in testbench {0}", testBench.Name);
                return;
            }

            foreach (var componentAssembly in testBench.Children.ComponentAssemblyCollection)
            {
                Visit(componentAssembly);
            }

            foreach (var testComponent in testBench.Children.TestComponentCollection)
            {
                Visit(testComponent);
            }
        }

        private void Visit(CyPhyInterfaces.ComponentAssembly componentAssembly)
        {
            foreach (var innerComponentAssembly in componentAssembly.Children.ComponentAssemblyCollection)
            {
                Visit(componentAssembly);
            }

            foreach (var component in componentAssembly.Children.ComponentCollection)
            {
                Visit(component);
            }
        }

        private void Visit(CyPhyInterfaces.Component component)
        {
            var rfModel = component.Children.RFModelCollection.FirstOrDefault();
            if (rfModel != null)
            {
                string resourcePath = "";

                // Get all resources
                foreach (var resource in rfModel.SrcConnections.UsesResourceCollection.Select(c => c.SrcEnds.Resource).Union(
                    rfModel.DstConnections.UsesResourceCollection.Select(c => c.DstEnds.Resource)))
                {
                    if (resource != null && resource.Attributes.Path != String.Empty)
                    {
                        rfModel.TryGetResourcePath(out resourcePath, ComponentLibraryManager.PathConvention.ABSOLUTE);
                    }

                    break;  // max. one CSXCAD file supported
                }

                if (resourcePath.Length == 0)
                {
                    Logger.WriteError("No resource file specified for component {0}.", component.Name);
                    return;
                }

                if (!System.IO.File.Exists(resourcePath))
                {
                    Logger.WriteError("Resource file {0} not found for component {1}.", resourcePath, component.Name);
                    return;
                }

                var araModule = m_endo.GetModule(m_slotIndex);
                if (araModule == null)
                {
                    if (m_endo.Slots[m_slotIndex].Size[0] == 2 && m_endo.Slots[m_slotIndex].Size[1] == 2)
                    {
                        araModule = new CSXCAD.Ara.Module_2x2(component.Name);
                    }
                    else
                    {
                        araModule = new CSXCAD.Ara.Module_1x2(component.Name);
                    }
                }

                m_antenna = new CSXCAD.XmlCompound(
                    null,
                    component.Name,
                    new Vector3D(rfModel.Attributes.X, rfModel.Attributes.Y, araModule.PCB.Thickness),
                    (double)rfModel.Attributes.Rotation * Math.PI / 2
                 );
                m_antenna.Parse(XElement.Load(resourcePath));

                araModule.PCB.Add(m_antenna);
                m_endo.AddModule(m_slotIndex, araModule);
            }
        }

        private void Visit(CyPhyInterfaces.TestComponent component)
        {
            m_rfModelFound = false;

            if (CodeGenerator.verbose) Logger.WriteInfo("CodeGenerator::Visit TestComponent: {0}", component.Name);

            foreach (var port in component.Children.RFPortCollection)
            {
                if (m_rfModelFound)
                {
                    return;
                }

                VisitPorts(port, null);
            }

        }

        private void VisitPorts(CyPhyInterfaces.RFPort currentPort, CyPhyInterfaces.RFPort prevPort)
        {
            if (m_rfModelFound)
            {
                return;
            }

            if (CodeGenerator.verbose) Logger.WriteInfo("Current port: " + currentPort.Name
                + " (" + currentPort.ID + ") parent: " + currentPort.ParentContainer.Name);

            if (currentPort.ParentContainer.Kind == "RFModel")
            {
                m_rfModelFound = true;

                // Add excitation
                CyPhyInterfaces.RFModel rfModel = CyPhyClasses.RFModel.Cast(currentPort.ParentContainer.Impl);

                CSXCAD.Ara.Module m = m_endo.GetModule(m_slotIndex);
                CSXCAD.Compound excitation = new Compound(null, "Excitation",
                    new Vector3D(rfModel.Attributes.X, rfModel.Attributes.Y, m.PCB.Thickness),
                    (double)rfModel.Attributes.Rotation * Math.PI / 2);
                excitation.Add(new LumpedPort(100, 1, 50.0,
                    new Vector3D(0, 0, -m.PCB.Thickness),
                    new Vector3D(0, 0, 0),
                    ENormDir.Z, true));
                m.PCB.Add(excitation);

                return;
            }

            var portQuery = (prevPort == null) ?
                (from p in currentPort.SrcConnections.PortCompositionCollection
                 select p.SrcEnd).Union(
                 from p in currentPort.DstConnections.PortCompositionCollection
                 select p.DstEnd) :
                (from p in currentPort.SrcConnections.PortCompositionCollection
                 where p.SrcEnd.ID != prevPort.ID
                 select p.SrcEnd).Union(
                 from p in currentPort.DstConnections.PortCompositionCollection
                 where p.DstEnd.ID != prevPort.ID
                 select p.DstEnd);

            foreach (CyPhyClasses.RFPort nextPort in portQuery)
            {
                VisitPorts(nextPort, currentPort);
            }
        }
    }
}
