using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using META;
using System.Globalization;
using CyPhy2Schematic.Layout;
using GME.MGA;
using CyPhyComponentFidelitySelector;

namespace CyPhy2Schematic.Schematic
{

    public class CyPhyBuildVisitor : Visitor
    {
        //
        // This Class contains visitor methods to build the lightweight Component Object network from CyPhy Models
        //
        public static Dictionary<string, Component> ComponentInstanceGUIDs { get; set; }
        public static Dictionary<string, Component> Components { get; set; }
        public static Dictionary<string, Port> Ports { get; set; }
        public string ProjectDirectory { get; set; }
        public CodeGenerator.Mode mode { get; private set; }
        private MgaTraceability Traceability;
        private Dictionary<string, CyPhy2SchematicInterpreter.IDs> mgaIdToDomainIDs;
        private ISet<IMgaObject> selectedSpiceModels;
        private ComponentAssembly systemUnderTest;

        public CyPhyBuildVisitor(string projectDirectory, CodeGenerator.Mode mode, MgaTraceability traceability,
            Dictionary<string, CyPhy2Schematic.CyPhy2SchematicInterpreter.IDs> mgaIdToDomainIDs, ISet<IMgaObject> selectedSpiceModels)  // this is a singleton object and the constructor will be called once
        {
            Components = new Dictionary<string, Component>();
            ComponentInstanceGUIDs = new Dictionary<string, Component>();
            Ports = new Dictionary<string, Port>();
            this.ProjectDirectory = projectDirectory;
            this.mode = mode;
            this.Traceability = traceability;
            this.mgaIdToDomainIDs = mgaIdToDomainIDs;
            this.selectedSpiceModels = selectedSpiceModels;
        }

        public override void visit(TestBench obj)
        {
            Logger.WriteDebug("CyPhyBuildVisitor::visit({0})", obj.Impl.Path);

            var testBench = obj.Impl;
            var ca = testBench.Children.ComponentAssemblyCollection.FirstOrDefault();
            if (ca == null)
            {
                Logger.WriteFailed("No valid component assembly in testbench {0}", obj.Name);
                return;
            }
            var componentAssembly_obj = new ComponentAssembly(ca);
            obj.ComponentAssemblies.Add(componentAssembly_obj);
            this.systemUnderTest = componentAssembly_obj.SystemUnderTest = componentAssembly_obj;
            componentAssembly_obj.selectedSpiceModels = selectedSpiceModels;

            var tcs = testBench.Children.TestComponentCollection;
            foreach (var tc in tcs)
            {
                var component_obj = new Component(tc);
                component_obj.SystemUnderTest = this.systemUnderTest;
                obj.TestComponents.Add(component_obj);
                CyPhyBuildVisitor.Components.Add(tc.ID, component_obj);   // Add to global component list, are these instance ID-s or component type ID-s?
            }

            foreach (var param in testBench.Children.ParameterCollection)
            {
                var param_obj = new Parameter()
                {
                    Name = param.Name,
                    Value = param.Attributes.Value
                };
                obj.Parameters.Add(param_obj);
            }

            // solver parameters - currently using Dymola Solver object
            var solver = testBench.Children.SolverSettingsCollection.FirstOrDefault();
            if (solver != null)
                obj.SolverParameters.Add("SpiceAnalysis", solver.Attributes.ToolSpecificAnnotations);

            Dictionary<string, string> properties = testBench.Children.PropertyCollection.ToDictionary(param => param.Name, param => param.Attributes.Value);
            string spiceAnalysisType;
            if (properties.TryGetValue("Spice Analysis Type", out spiceAnalysisType))
            {
                if (spiceAnalysisType == "Transient Analysis")
                {
                    string stepSize = "0.0001";
                    properties.TryGetValue("Spice Step Size", out stepSize);
                    string endTime = "1";
                    properties.TryGetValue("Spice End Time", out endTime);
                    string startTime = "0";
                    properties.TryGetValue("Spice Start Time", out startTime);
                    obj.SolverParameters["SpiceAnalysis"] = String.Format(".TRAN {0} {1} {2}", stepSize, endTime, startTime);
                }
                else
                {
                    throw new ApplicationException(string.Format("Unsupported Spice Analysis Type '{0}'", spiceAnalysisType));
                }
            }
        }

        public override void visit(ComponentAssembly obj)
        {
            Logger.WriteDebug("CyPhyBuildVisitor::visit({0})", obj.Impl.Path);

            var ca = obj.Impl;

            if (mode == CodeGenerator.Mode.SPICE || mode == CodeGenerator.Mode.SPICE_SI || mode == CodeGenerator.Mode.SCHEMATIC_ONLY)
            {
                var spiceModels = obj.Impl.Children.SPICEModelCollection;
                Tonka.SPICEModel spiceModel = GetSpiceModel(obj, spiceModels);

                if (spiceModel != null)
                {
                    GetSpiceModel(spiceModel, obj, isTestComponent: false);
                    foreach (var port in spiceModel.Children.SchematicModelPortCollection)
                    {
                        var port_obj = new Port(port)
                        {
                            Parent = obj,
                        };
                        obj.Ports.Add(port_obj);
                        CyPhyBuildVisitor.Ports.Add(port.ID, port_obj);
                        Logger.WriteInfo("Mapping Port <a href=\"mga:{0}\">{1}</a> with ID {2}", Traceability.GetID(port.Impl), port.Name, port.ID);
                    }
                }
            }


            // ------- ComponentAssemblies -------
            foreach (var innerComponentAssembly in ca.Children.ComponentAssemblyCollection)
            {
                var innerComponentAssembly_obj = new ComponentAssembly(innerComponentAssembly)
                {
                    Parent = obj,
                    SystemUnderTest = this.systemUnderTest
                };
                obj.ComponentAssemblyInstances.Add(innerComponentAssembly_obj);
            }

            // ------- Components -------
            foreach (var component in ca.Children.ComponentCollection)
            {
                var component_obj = new Component(component)
                {
                    Parent = obj,
                    SystemUnderTest = this.systemUnderTest
                };
                obj.ComponentInstances.Add(component_obj);
                CyPhyBuildVisitor.Components.Add(component.ID, component_obj);   // Add to global component list, component type ID-s?
                var componentInstanceGUID = Layout.LayoutGenerator.GetComponentID(component);
                //GME.CSharp.GMEConsole console = GME.CSharp.GMEConsole.CreateFromProject(component.Impl.Project);
                //console.Info.WriteLine(componentInstanceGUID);
                try
                {
                    CyPhyBuildVisitor.ComponentInstanceGUIDs.Add(componentInstanceGUID, component_obj);   // component instance guid-s?
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(e.Message + " " + componentInstanceGUID);
                }
            }
        }

        public static Tonka.SPICEModel GetSpiceModel(DesignEntity de, IEnumerable<Tonka.SPICEModel> spiceModels)
        {
            var spiceModelsList = spiceModels.ToList();
            if (spiceModelsList.Count == 0)
            {
                return null;
            }
            if (de.SystemUnderTest.selectedSpiceModels != null)
            {
                return spiceModels.Where(sm => de.SystemUnderTest.selectedSpiceModels.Contains(sm.Impl)).FirstOrDefault();
            }
            else
            {
                // TODO if (spiceModelsList.Count > 1) warn SpiceModel was picked arbitrarily
                return spiceModels.OrderBy(x => x.Name + "_" + x.Guid.ToString("D")).FirstOrDefault();
            }
        }

        private bool SpiceClassRequiresModel(String spiceClass)
        {
            if ((spiceClass.Length == 1 && spiceClass.ToUpper() != "X") || (spiceClass.Length == 0))
            {
                // It's a primitive
                return false;
            }
            else
            {
                // It's a sub-circuit or a refinement on a primitive.
                return true;
            }
        }

        public override void visit(Component obj)
        {
            Logger.WriteDebug("CyPhyBuildVisitor::visit({0})", obj.Impl.Path);

            var component = obj.Impl;
            var ca = obj.Parent;

            var compBase = obj.Impl.ArcheType;
            bool isTestComponent = component is Tonka.TestComponent;

            Tonka.SchematicModel schematicModel = null;

            //////////// EDA /////////////
            if (mode == CodeGenerator.Mode.EDA)
            {
                var edaModel = component.Children.EDAModelCollection.FirstOrDefault();

                if (edaModel == null)
                {
                    Logger.WriteInfo("Skipping Component <a href=\"mga:{0}\">{1}</a> (no EDA model)",
                                     Traceability.GetID(obj.Impl.Impl), obj.Impl.Name);
                    return;
                }
                schematicModel = edaModel;

                // try and load the resource

                bool hasResource = false;
                String schAbsPath = "";
                try
                {
                    hasResource = edaModel.TryGetResourcePath(out schAbsPath, ComponentLibraryManager.PathConvention.REL_TO_PROJ_ROOT);
                    schAbsPath = Path.Combine(this.ProjectDirectory, schAbsPath);
                }
                catch (NotSupportedException ex)
                {
                    // test component will not have / should not have resource
                    hasResource = false;
                }
                if (!hasResource && !isTestComponent)
                {
                    Logger.WriteError("Couldn't determine path of schematic file for component <a href=\"mga:{0}\">{1}</a>. It may not be linked to a Resource object.",
                        Traceability.GetID(obj.Impl.Impl), obj.Impl.Name);
                    return;
                }

                bool failedToLoadSchematicLib = false;
                try
                {
                    obj.SchematicLib = Eagle.eagle.LoadFromFile(schAbsPath);
                }
                catch (Exception e)
                {
                    failedToLoadSchematicLib = true;
                    if (!isTestComponent) // test components don't need to have schematic
                    {
                        // Try to get the ID of the pre-Elaborated object so that the error message
                        // will be more meaningful.
                        String compOriginalID = null;
                        Logger.WriteError("Error Loading Schematic Library of <a href=\"mga:{0}\">{1}</a>: {2}",
                                            Traceability.GetID(obj.Impl.Impl), obj.Impl.Name, e.Message);
                        throw;
                    }
                }

                if (failedToLoadSchematicLib)
                    obj.SchematicLib = new Eagle.eagle();

                foreach (var param in edaModel.Children.EDAModelParameterCollection)
                {
                    var val = param.Attributes.Value;
                    var valSrc = param.SrcConnections
                                      .EDAModelParameterMapCollection
                                      .Select(p => p.SrcEnd)
                                      .FirstOrDefault();

                    if (valSrc != null)
                    {
                        var valProp = valSrc as Tonka.Property;
                        if (valProp != null)
                            val = valProp.Attributes.Value;
                        var valParam = valSrc as Tonka.Parameter;
                        if (valParam != null)
                            val = valParam.Attributes.Value;
                    }

                    var param_obj = new Parameter()
                    {
                        Name = param.Name,
                        Value = val
                    };
                    obj.Parameters.Add(param_obj);
                }
            }
            ///////////// SPICE //////////////
            else if (mode == CodeGenerator.Mode.SPICE || mode == CodeGenerator.Mode.SPICE_SI || mode == CodeGenerator.Mode.SCHEMATIC_ONLY)
            {
                var spiceModels = obj.Impl.Children.SPICEModelCollection;
                Tonka.SPICEModel spiceModel = GetSpiceModel(obj, spiceModels);

                if (spiceModel == null)
                {
                    if (spiceModels.Count() == 0)
                    {
                        Logger.WriteInfo("Skipping Component <a href=\"mga:{0}\">{1}</a> (no SPICE model)",
                                         Traceability.GetID(obj.Impl.Impl), obj.Impl.Name);
                    }
                    return;
                }
                schematicModel = spiceModel;
                GetSpiceModel(spiceModel, obj, isTestComponent);

                foreach (var param in spiceModel.Children.SPICEModelParameterCollection)
                {
                    var val = param.Attributes.Value;
                    var valSrc = param.SrcConnections
                                      .SPICEModelParameterMapCollection
                                      .Select(p => p.SrcEnd)
                                      .FirstOrDefault();

                    if (valSrc != null)
                    {
                        var valProp = valSrc as Tonka.Property;
                        if (valProp != null)
                            val = valProp.Attributes.Value;
                        var valParam = valSrc as Tonka.Parameter;
                        if (valParam != null)
                            val = valParam.Attributes.Value;
                    }

                    var param_obj = new Parameter()
                    {
                        Name = param.Name,
                        Value = val
                    };
                    obj.Parameters.Add(param_obj);
                }
            }
            else
            {
                throw new NotSupportedException(String.Format("Mode {0} is not supported by visit(Component)", mode.ToString()));
            }

            // Both EDA and SPICE models have these ports in common.
            if (schematicModel != null)
            {
                foreach (var port in schematicModel.Children.SchematicModelPortCollection)
                {
                    var port_obj = new Port(port)
                    {
                        Parent = obj,
                    };
                    obj.Ports.Add(port_obj);
                    CyPhyBuildVisitor.Ports.Add(port.ID, port_obj);
                    Logger.WriteInfo("Mapping Port <a href=\"mga:{0}\">{1}</a> with ID {2}", Traceability.GetID(port.Impl), port.Name, port.ID );
                }
            }
        }

        private void GetSpiceModel(Tonka.SPICEModel spiceModel, DesignEntity de, bool isTestComponent)
        {
            String spiceClass = spiceModel.Attributes.Class;
            if (String.IsNullOrWhiteSpace(spiceClass))
            {
                Logger.WriteWarning("Component <a href=\"mga:{0}\">{1}</a> has a SPICE model, but its class is not specified",
                                    Traceability.GetID(de.Impl.Impl),
                                    de.Impl.Name);
            }

            // If this SPICE class requires a model file to be provided, try to find it and load it.
            if (SpiceClassRequiresModel(spiceClass))
            {
                try
                {
                    String spiceAbsPath;
                    if (spiceModel.TryGetResourcePath(out spiceAbsPath, ComponentLibraryManager.PathConvention.REL_TO_PROJ_ROOT))
                    {
                        spiceAbsPath = Path.Combine(this.ProjectDirectory, spiceAbsPath);
                        de.SpiceLib = File.ReadAllText(spiceAbsPath);
                    }
                    else
                    {
                        throw new ApplicationException("SPICE model has no associated Resource");
                    }
                }
                catch (Exception e)
                {
                    de.SpiceLib = "";
                    if (!isTestComponent)
                        Logger.WriteError("Error Loading Spice Library of <a href=\"mga:{0}\">{1}</a>: {2}",
                            Traceability.GetID(de.Impl.Impl), de.Impl.Name, e.Message);
                }
            }
        }
    }

    public class CyPhyLayoutVisitor : Visitor
    {
        public const float gridSize = 2.54f;    // grid size is 0.1inch or 2.54mm
        // These constants are multiples of 2.54 for the inch to mm conversion
        // Eagle prefers 0.1 inch grid - though in theory anything can be set, but symbol pins have 0.1 inch spacing
        const float symSpace = 7.62f;
        const float gme2Eagle = 20.0f;
        const float defSize = 10.16f;

        // This class computes the layout of the flat schematic diagram starting from GME hierarchical diagrams
        public override void upVisit(ComponentAssembly obj)
        {
            SortedList<Tuple<float, float>, object> nodes = new SortedList<Tuple<float, float>, object>();

            // we have already visited all child nodes, and know their extents (canvasWidth and canvasHeight)
            // now sort all child objects by their GME object location, and use the GME location as their initial position
            foreach (var ca in obj.ComponentAssemblyInstances)
            {
                var allAspect = ca.Impl.Aspects.Where(a => a.Name.Equals("All")).FirstOrDefault();
                float x = (allAspect != null) ? ((float)allAspect.X) / gme2Eagle : defSize;
                float y = (allAspect != null) ? ((float)allAspect.Y) / gme2Eagle : defSize;
                ca.CanvasX = (float)Math.Round(x / gridSize) * gridSize;
                ca.CanvasY = (float)Math.Round(y / gridSize) * gridSize;
                Tuple<float, float> location = new Tuple<float, float>(ca.CanvasX, ca.CanvasY);
                if (nodes.ContainsKey(location))
                {
                    Logger.WriteWarning("Component Assembly {1}.{0} overlaps with another in model",
                        ca.Name, ca.Parent.Name);
                    var xOry = (new Random()).Next(0, 1);
                    location = new Tuple<float, float>(ca.CanvasX + xOry*(gridSize/2),
                        ca.CanvasY + (gridSize/2)*(1-xOry));
                }
                nodes.Add(location, ca);

            }
            foreach (var ca in obj.ComponentInstances)
            {
                var allAspect = ca.Impl.Aspects.Where(a => a.Name.Equals("All")).FirstOrDefault();
                float x = allAspect != null ? ((float)allAspect.X) / gme2Eagle : defSize;
                float y = allAspect != null ? ((float)allAspect.Y) / gme2Eagle : defSize;
                ca.CanvasX = (float)Math.Round(x / gridSize) * gridSize;
                ca.CanvasY = (float)Math.Round(y / gridSize) * gridSize;
                Tuple<float, float> location = new Tuple<float, float>(ca.CanvasX, ca.CanvasY);
                if (nodes.ContainsKey(location))
                {
                    Logger.WriteWarning("Component {1}.{0} overlaps with another in model",
                        ca.Name, ca.Parent.Name);
                    var xOry = (new Random()).Next(0, 1);
                    location = new Tuple<float, float>(ca.CanvasX + xOry * (gridSize / 2),
                        ca.CanvasY + (gridSize / 2) * (1 - xOry));
                }

                nodes.Add(location, ca);
            }

            object[] nodeArr = nodes.Values.ToArray();
            float maxWidth = 0;
            float maxHeight = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodeArr[i];
                var nx = node is Component ? (node as Component).CanvasX : (node as ComponentAssembly).CanvasX;
                var ny = node is Component ? (node as Component).CanvasY : (node as ComponentAssembly).CanvasY;

                for (int j = 0; j < i; j++)
                {
                    var nodePre = nodeArr[j];
                    float npxw, npyh;
                    if (nodePre is Component)
                    {
                        var c = nodePre as Component;
                        npxw = c.CanvasX + c.CanvasWidth;
                        npyh = c.CanvasY + c.CanvasHeight;
                    }
                    else
                    {
                        var c = nodePre as ComponentAssembly;
                        npxw = c.CanvasX + c.CanvasWidth;
                        npyh = c.CanvasY + c.CanvasHeight;
                    }
                    if ( (nx - symSpace) <= npxw && (ny - symSpace) <= npyh)       // overlap
                    {
                        if (j % 2 == 0)
                            nx = npxw + symSpace;
                        else
                            ny = npyh + symSpace;
                    }
                }
                float nxw, nyh;
                if (node is Component)
                {
                    var c = node as Component;
                    c.CanvasX = nx;
                    c.CanvasY = ny;
                    nxw = nx + c.CanvasWidth;
                    nyh = ny + c.CanvasHeight;
                    Logger.WriteDebug("Placing Node {0} in Assembly {1}: @ {2},{3}",
                                      c.Name, obj.Name, nx, ny);
                }
                else
                {
                    var c = node as ComponentAssembly;
                    c.CanvasX = nx;
                    c.CanvasY = ny;
                    nxw = nx + c.CanvasWidth;
                    nyh = ny + c.CanvasHeight;
                }
                maxWidth = Math.Max(maxWidth, nxw);
                maxHeight = Math.Max(maxHeight, nyh);
            }
            // set the size of this container
            obj.CanvasWidth = (float)Math.Round(maxWidth/(2.0f*gridSize))*2.0f*gridSize;
            obj.CanvasHeight = (float)Math.Round(maxHeight/(2.0f*gridSize))*2.0f*gridSize;
        }

        public override void visit(Component obj)
        {
            Logger.WriteDebug("CyPhyLayoutVisitor::visit({0})", obj.Impl.Path);
            // compute device dimension
            // need gate objects + their locations within device
            // need an overall device schematic symbol dimension
            var compLib = (obj.SchematicLib != null) ? obj.SchematicLib.drawing.Item as Eagle.library : null;
            if (compLib != null)
            {
                var gates = compLib.devicesets.deviceset.SelectMany(p => p.gates.gate);
                float minx = float.MaxValue;
                float maxx = float.MinValue;
                float miny = float.MaxValue;
                float maxy = float.MinValue;
                foreach (var gate in gates)
                {
                    var symbol = compLib.symbols.symbol.Where(q => q.name.Equals(gate.symbol)).FirstOrDefault();
                    float gx = float.Parse(gate.x, CultureInfo.InvariantCulture);
                    float gy = float.Parse(gate.y, CultureInfo.InvariantCulture);
                    var pins = symbol.Items.Where(p => p is Eagle.pin).Select(p => p as Eagle.pin);
                    foreach (var pin in pins)
                    {
                        float x = gx + float.Parse(pin.x, CultureInfo.InvariantCulture);
                        float y = gy + float.Parse(pin.y, CultureInfo.InvariantCulture);
                        minx = Math.Min(x, minx);
                        miny = Math.Min(y, miny);
                        maxx = Math.Max(x, maxx);
                        maxy = Math.Max(y, maxy);
                    }
                    var wires = symbol.Items.Where(p => p is Eagle.wire).Select(p => p as Eagle.wire);
                    foreach (var w in wires)
                    {
                        float x1 = float.Parse(w.x1, CultureInfo.InvariantCulture);
                        float x2 = float.Parse(w.x2, CultureInfo.InvariantCulture);
                        float y1 = float.Parse(w.y1, CultureInfo.InvariantCulture);
                        float y2 = float.Parse(w.y2, CultureInfo.InvariantCulture);
                        minx = Math.Min(minx, Math.Min(x1, x2));
                        miny = Math.Min(miny, Math.Min(y1, y2));
                        maxx = Math.Max(maxx, Math.Min(x1, x2));
                        maxy = Math.Max(maxy, Math.Min(y1, y2));
                    }
                    var rectangles = symbol.Items.Where(p => p is Eagle.rectangle).Select(p => p as Eagle.rectangle);
                    foreach (var w in rectangles)
                    {
                        float x1 = float.Parse(w.x1, CultureInfo.InvariantCulture);
                        float x2 = float.Parse(w.x2, CultureInfo.InvariantCulture);
                        float y1 = float.Parse(w.y1, CultureInfo.InvariantCulture);
                        float y2 = float.Parse(w.y2, CultureInfo.InvariantCulture);
                        minx = Math.Min(minx, Math.Min(x1, x2));
                        miny = Math.Min(miny, Math.Min(y1, y2));
                        maxx = Math.Max(maxx, Math.Min(x1, x2));
                        maxy = Math.Max(maxy, Math.Min(y1, y2));
                    }
                    var circles = symbol.Items.Where(p => p is Eagle.circle).Select(p => p as Eagle.circle);
                    foreach (var w in circles)
                    {
                        float x1 = float.Parse(w.x, CultureInfo.InvariantCulture) - float.Parse(w.radius, CultureInfo.InvariantCulture);
                        float x2 = float.Parse(w.x, CultureInfo.InvariantCulture) + float.Parse(w.radius, CultureInfo.InvariantCulture);
                        float y1 = float.Parse(w.y, CultureInfo.InvariantCulture) - float.Parse(w.radius, CultureInfo.InvariantCulture);
                        float y2 = float.Parse(w.y, CultureInfo.InvariantCulture) + float.Parse(w.radius, CultureInfo.InvariantCulture);
                        minx = Math.Min(minx, Math.Min(x1, x2));
                        miny = Math.Min(miny, Math.Min(y1, y2));
                        maxx = Math.Max(maxx, Math.Min(x1, x2));
                        maxy = Math.Max(maxy, Math.Min(y1, y2));
                    }
                }
                obj.CanvasWidth = (float) Math.Round((maxx - minx)/(2.0*gridSize)) * 2.0f * gridSize;
                obj.CanvasHeight = (float) Math.Round((maxy - miny)/(2.0*gridSize)) * 2.0f * gridSize;
            }
            else
            {
                obj.CanvasWidth = defSize;
                obj.CanvasHeight = defSize;
            }

        }
    }

    public class CyPhyLayout2Visitor : Visitor
    {
        public override void visit(ComponentAssembly obj)
        {
            if (obj.Parent != null)
            {
                obj.CanvasX += obj.Parent.CanvasX;
                obj.CanvasY += obj.Parent.CanvasY;
            }
        }

        public override void visit(Component obj)
        {
            if (obj.Parent != null)     // test components may have null parent
            {
                obj.CanvasX += obj.Parent.CanvasX;
                obj.CanvasY += obj.Parent.CanvasY;
            }
            // round off center to align with schematic grid based on gridSize parameter
            double cx = (obj.CanvasX + (obj.CanvasWidth / 2.0f));
            double cy = (obj.CanvasY + (obj.CanvasHeight / 2.0f));

            // snap to grid
            obj.CenterX = (float)Math.Round(cx / CyPhyLayoutVisitor.gridSize) * CyPhyLayoutVisitor.gridSize;
            obj.CenterY = (float)Math.Round(cy / CyPhyLayoutVisitor.gridSize) * CyPhyLayoutVisitor.gridSize;

            Logger.WriteDebug("Component {0}: Center {1},{2}, Size {3},{4}",
                              obj.Name,
                              obj.CenterX,
                              obj.CenterY, obj.CanvasWidth, obj.CanvasHeight);
        }

        public override void visit(Port obj)
        {
            float portX = 0;
            // FIXME should this be a hard error if this doesn't parse? The default value is the empty string, which doesn't parse
            float.TryParse(obj.Impl.Attributes.EDASymbolLocationX, NumberStyles.Float, CultureInfo.InvariantCulture, out portX);
            float portY = 0;
            float.TryParse(obj.Impl.Attributes.EDASymbolLocationY, NumberStyles.Float, CultureInfo.InvariantCulture, out portY);
            var compLib = (obj.ComponentParent.SchematicLib != null) ? obj.ComponentParent.SchematicLib.drawing.Item as Eagle.library : null;
            if (compLib != null)
            {
                var gate = compLib.devicesets.deviceset.
                    SelectMany(p => p.gates.gate).
                    Where(g => g.name.Equals(obj.Impl.Attributes.EDAGate)).
                    FirstOrDefault();
                float gateX = gate != null ? float.Parse(gate.x, CultureInfo.InvariantCulture) : 0.0f;
                float gateY = gate != null ? float.Parse(gate.y, CultureInfo.InvariantCulture) : 0.0f;
                portX += gateX;
                portY += gateY;
            }
            obj.CanvasX = obj.ComponentParent.CenterX + portX;
            obj.CanvasY = obj.ComponentParent.CenterY + portY;

            Logger.WriteDebug("Port {0}: Location {1},{2} Parent Center: {3},{4}",
                              obj.Name,
                              obj.CanvasX,
                              obj.CanvasY,
                              obj.ComponentParent.CenterX,
                              obj.ComponentParent.CenterY);
        }


    }

    public class CyPhyConnectVisitor : Visitor
    {
        public Dictionary<string, Port> VisitedPorts { get; set; }
        public CodeGenerator.Mode mode { get; private set; }
        private Type SchematicModelType;

        public CyPhyConnectVisitor(CodeGenerator.Mode mode)
        {
            VisitedPorts = new Dictionary<string, Port>();

            this.mode = mode;
            switch (mode)
            {
                case CodeGenerator.Mode.EDA:
                    SchematicModelType = typeof(Tonka.EDAModel);
                    break;
                case CodeGenerator.Mode.SCHEMATIC_ONLY:
                case CodeGenerator.Mode.SPICE:
                case CodeGenerator.Mode.SPICE_SI:
                    SchematicModelType = typeof(Tonka.SPICEModel);
                    break;
                default:
                    throw new NotSupportedException(String.Format("Mode {0} is not supported.", mode.ToString()));
            }
        }

        private bool portHasCorrectParentType(ISIS.GME.Common.Interfaces.FCO port)
        {
            ISIS.GME.Common.Interfaces.Container parent = port.ParentContainer;
            Type parentType = parent.GetType();
            return this.SchematicModelType.IsAssignableFrom(parentType);
        }

        public override void visit(Port obj)
        {
            Logger.WriteDebug("CyPhyConnectVisitor::visit({0})", obj.Impl.Path);

            if (VisitedPorts.ContainsKey(obj.Impl.ID))  // this port was already visited in a connection traversal - no need to explore its connections again
                return;
            VisitedPorts.Add(obj.Impl.ID, obj);

            Logger.WriteDebug("Connect Visit: port {0}", obj.Impl.Path);
            var elecPort = obj.Impl as Tonka.SchematicModelPort;
            if (elecPort != null)
            {
                // from schematic port navigate out to component port (or connector) in either connection direction
                var compPorts = elecPort.DstConnections.PortCompositionCollection.Select(p => p.DstEnd).
                    Union(elecPort.SrcConnections.PortCompositionCollection.Select(p => p.SrcEnd));
                foreach (var compPort in compPorts)
                {
                    Dictionary<string, object> visited = new Dictionary<string, object>();
                    if (compPort.ParentContainer is Tonka.Connector)                     // traverse connector chain, carry port name in srcConnector
                    {
                        obj.connectedPorts[compPort.Impl.AbsPath] = compPort;
                        Traverse(compPort.Name, obj, obj.ComponentParent.Impl, compPort.ParentContainer as Tonka.Connector, visited);
                    }
                    else if (compPort.ParentContainer is Tonka.DesignElement)
                    {
                        obj.connectedPorts[compPort.Impl.AbsPath] = compPort;
                        Traverse(obj, (Tonka.DesignElement)obj.Parent.Impl, compPort as Tonka.Port, visited); // traverse port chain
                    }
                    else if (portHasCorrectParentType(compPort)
                             && CyPhyBuildVisitor.Ports.ContainsKey(compPort.ID))
                    {
                        obj.connectedPorts[compPort.Impl.AbsPath] = compPort;
                        ConnectPorts(obj, CyPhyBuildVisitor.Ports[compPort.ID]);
                    }
                }
            }
        }

        private void Traverse(string srcConnectorName, Port srcPort_obj, Tonka.DesignElement parent, Tonka.Connector connector, Dictionary<string, object> visited)
        {
            // XXX this is dead code; the elaborator removes Connectors
            Logger.WriteDebug("Traverse Connector: {0}, Mapped-Pin: {1}",
                              connector.Path,
                              srcConnectorName);
            if (visited.ContainsKey(connector.ID))
            {
                Logger.WriteWarning("Traverse Connector Revisit: {0}, Mapped-Pin: {1}", connector.Path, srcConnectorName);
                return;
            }
            visited.Add(connector.ID, connector);

            // continue traversal as connector
            var remotes = connector.DstConnections.ConnectorCompositionCollection.Select(p => p.DstEnd).Union(
                    connector.SrcConnections.ConnectorCompositionCollection.Select(p => p.SrcEnd));
            foreach (var remote in remotes)
            {
                if (visited.ContainsKey(remote.ID)) // already visited
                    continue;
                if (remote.ParentContainer is Tonka.DesignElement)
                    Traverse(srcConnectorName, srcPort_obj, remote.ParentContainer as Tonka.DesignElement, remote as Tonka.Connector, visited);
            }

            // continue traversal through named port
            // XXX FIXME p.Name.Equals(srcConnectorName) is a bug (why should it matter what the names are)
            var mappedPorts = connector.Children.SchematicModelPortCollection.Where(p => p.Name.Equals(srcConnectorName));
            foreach (var mappedPort in mappedPorts)
            {
                var remotePorts = mappedPort.DstConnections.PortCompositionCollection.Select(p => p.DstEnd).Union(
                    mappedPort.SrcConnections.PortCompositionCollection.Select(p => p.SrcEnd));
                foreach (var remotePort in remotePorts)
                {
                    if (visited.ContainsKey(remotePort.ID)) // already visited
                    {
                        continue;
                    }
                    if (remotePort.ParentContainer is Tonka.Connector &&
                        !visited.ContainsKey(remotePort.ParentContainer.ID))
                        // traverse connector chain, carry port name in srcConnector
                    {
                        Traverse(remotePort.Name, srcPort_obj,
                            remotePort.ParentContainer.ParentContainer as Tonka.DesignElement,
                            remotePort.ParentContainer as Tonka.Connector, visited);
                    }
                    else if (remotePort.ParentContainer is Tonka.DesignElement)
                    {
                        Traverse(srcPort_obj, remotePort.ParentContainer as Tonka.DesignElement, remotePort as Tonka.Port, visited);
                    }
                    else if (portHasCorrectParentType(remotePort)
                             && CyPhyBuildVisitor.Ports.ContainsKey(remotePort.ID))
                    {
                        ConnectPorts(srcPort_obj, CyPhyBuildVisitor.Ports[remotePort.ID]);
                    }
                }
            }
        }

        private void Traverse(Port srcPort_obj, Tonka.DesignElement parent, Tonka.Port port, Dictionary<string, object> visited)
        {
            Logger.WriteDebug("Traverse Port: port {0}",
                              port.Path);

            if (visited.ContainsKey(port.ID))
            {
                Logger.WriteWarning("Traverse Port Revisit: {0}", port.Path);
                return;
            }
            visited.Add(port.ID, port);

            // continue traversal
            var remotes = port.DstConnections.PortCompositionCollection.Select(p => p.DstEnd).Union(
                port.SrcConnections.PortCompositionCollection.Select(p => p.SrcEnd));

            foreach (var remote in remotes)
            {
                if (visited.ContainsKey(remote.ID))
                    continue;       // already visited continue

                if (remote.ParentContainer is Tonka.DesignElement)  // remote is contained in a Component or ComponentAssembly
                {
                    srcPort_obj.connectedPorts[remote.Impl.AbsPath] = remote;
                    Traverse(srcPort_obj,
                             remote.ParentContainer as Tonka.DesignElement,
                             remote as Tonka.Port,
                             visited);
                }
                else if (remote.ParentContainer is Tonka.Connector) // remote is contained in a Connector
                {
                    srcPort_obj.connectedPorts[remote.Impl.AbsPath] = remote;
                    Traverse(remote.Name,
                             srcPort_obj,
                             remote.ParentContainer.ParentContainer as Tonka.DesignElement,
                             remote.ParentContainer as Tonka.Connector,
                             visited);
                }
                else if (portHasCorrectParentType(remote)    // remote is contained in a SchematicModel
                         && CyPhyBuildVisitor.Ports.ContainsKey(remote.ID))
                {
                    ConnectPorts(srcPort_obj, CyPhyBuildVisitor.Ports[remote.ID]);
                }
            }
        }

        private void ConnectPorts(Port srcPort_obj, Port dstPort_obj)
        {
            if (srcPort_obj.Impl.Equals(dstPort_obj.Impl))
                return;

            Connection conn_obj = new Connection(srcPort_obj, dstPort_obj);
            srcPort_obj.DstConnections.Add(conn_obj);
            dstPort_obj.SrcConnections.Add(conn_obj);
            Logger.WriteDebug("Connecting Port {0} to {1}", srcPort_obj.Impl.Name,
                              dstPort_obj.Impl.Name);

            // mark the dstPort visited
            if (!VisitedPorts.ContainsKey(dstPort_obj.Impl.ID))
            {
                VisitedPorts.Add(dstPort_obj.Impl.ID, dstPort_obj);
                dstPort_obj.connectedPorts = srcPort_obj.connectedPorts;
            }
            else
                Logger.WriteDebug("Port {0} already in visited ports, don't add",
                                  dstPort_obj.Impl.Path);
        }


    }
}
