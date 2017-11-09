using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using META;

using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using ISIS.GME.Common.Interfaces;

namespace CyPhy2Schematic.Schematic
{
    class SpiceVisitor : Visitor
    {
        private int netCount = 0;
        private Dictionary<Port, string> PortNetMap;
        private Dictionary<DesignEntity, Spice.Node> ComponentNodeMap = new Dictionary<DesignEntity, Spice.Node>();
        private Dictionary<Object, Spice.SignalBase> ObjectSiginfoMap;

        private string[] Grounds = new string[] { "gnd", "Gnd", "GND", "ground", "Ground" };
        private string[] SigIntegrityTraces;
        private Dictionary<string, CyPhy2SchematicInterpreter.IDs> mgaIdToDominIDs;
        private MgaTraceability traceability;
        private CodeGenerator CodeGenerator;

        public Spice.Circuit circuit_obj { get; set; }
        public Spice.SignalContainer siginfo_obj { get; set; }

        public CodeGenerator.Mode mode { get; set; }

        Dictionary<CyPhy2SchematicInterpreter.IDs, string> objectToNetId
        {
            get
            {
                return siginfo_obj.objectToNetId;
            }
        }

        public SpiceVisitor(MgaTraceability traceability, Dictionary<string, CyPhy2SchematicInterpreter.IDs> mgaIdToDomainIDs, CodeGenerator CodeGenerator)
        {
            this.traceability = traceability;
            this.mgaIdToDominIDs = mgaIdToDomainIDs;
            this.CodeGenerator = CodeGenerator;
            PortNetMap = new Dictionary<Port, string>();
            ObjectSiginfoMap = new Dictionary<object, Spice.SignalBase>();
            netCount = 0;
        }

        private Tuple<char, string> GetSpiceType(DesignEntity obj)
        {
            var spiceObjs = (obj.Impl is Tonka.ComponentType) ?
                (obj.Impl as Tonka.ComponentType).Children.SPICEModelCollection :
                (obj.Impl as Tonka.ComponentAssembly).Children.SPICEModelCollection;
            Tonka.SPICEModel spiceObj = CyPhyBuildVisitor.GetSpiceModel(obj, spiceObjs);

            string[] spiceType = (spiceObj != null) ? spiceObj.Attributes.Class.Split(new char[] { ':', ' ', '.' }) : null;

            char baseType = (spiceType != null && spiceType.Count() > 0 && spiceType[0].Length > 0) ? spiceType[0][0] : (char)0;
            string classType = (spiceType != null && spiceType.Count() > 1) ? spiceType[1] : "";

            var tuple = new Tuple<char, string>(baseType, classType);

            return tuple;
        }

        private List<Component> CollectGroundNodes(ComponentAssembly obj)
        {
            var gnds = obj.ComponentInstances.Where(t =>
                Grounds.Any(GetSpiceType(t).Item2.Contains)
                ).ToList();
            var cgnds = obj.ComponentAssemblyInstances.SelectMany(ca => CollectGroundNodes(ca)).ToList();
            return gnds.Union(cgnds).ToList();
        }

        private List<Component> CollectGroundNodes(TestBench obj)
        {
            var gnds = obj.TestComponents.Where(t =>
                Grounds.Any(GetSpiceType(t).Item2.Contains)
                ).ToList();
            var cgnds = obj.ComponentAssemblies.SelectMany(ca => CollectGroundNodes(ca)).ToList();
            return gnds.Union(cgnds).ToList();
        }

        private void GenerateTraceSubckt(LayoutJson.Signal trace)
        {
            // this is where the smarts of what type of trace subcircuit to generate will reside
            // reference hspice98 - chapter21, pp 21-2: Selecting Wire Models

            // choices for wire models:
            // 1. no model
            // 2. lumped models with RLC - simple R, shunt cap C, series inductor and resistor RL, series resistor and shunt cap RC
            // 3. ideal - lossless transmission line
            // 4. lossy transmission line

            // decision based on : 
            // 1. source properties: rise time - trise, source resistance - Rsource
            // 2. connector properties: characteristic impedance - Z0, time delay - TD (function of length/frequency)
            //                  OR    : equiv resistance - R, equiv inductance - L, equiv capacitance - C
 
            // decision tree: Figure 21-1, Wire Model Selection Chart (wire_select.jpg)
            // a) trise > 5TD (low frequency) --- use lumped model with RLC (criteria for R / RL / RC in wire_select.jpg)
            // b) trise < 5TD (high frequence) --- use lossy or lossless transmission line
            StringWriter subckt = new StringWriter();
            subckt.WriteLine(".subckt Trace_{0} 1 2", trace.name);
            subckt.WriteLine("* Simple Transmission Line Model ");
            subckt.WriteLine("TL 1 0 2 0 Z0=50 TD=10ns");
            subckt.WriteLine(".ends Trace_{0}", trace.name);
            circuit_obj.subcircuits.Add(string.Format("Trace_{0}", trace.name), subckt.ToString());
        }

        public override void visit(TestBench obj)
        {
            if (obj.SolverParameters.ContainsKey("SpiceAnalysis"))
            {
                circuit_obj.analysis = obj.SolverParameters["SpiceAnalysis"];
            }
            var tracesParam = obj.Parameters.Where(p => p.Name.Equals("Traces")).FirstOrDefault();
            if (tracesParam != null)
                SigIntegrityTraces = tracesParam.Value.Split(new char[] { ' ', ',' });

            // process all ground nets first before they get assigned another number
            var gnds = CollectGroundNodes(obj);
            var gports = gnds.SelectMany(g => g.Ports).ToList();
            foreach (var gp in gports)
            {
                visit(gp, "0");
            }
        }

        public override void visit(ComponentAssembly obj)
        {
            CodeGenerator.Logger.WriteDebug(
                    "SpiceVisitor::visit({0})",
                    obj.Name);

            // create a signal container for this assembly
            var siginfo_obj = new Spice.SignalContainer()
            {
                name = obj.Name,
                gmeid = obj.Impl.ID
            };
            ObjectSiginfoMap.Add(obj, siginfo_obj);
            var siginfo_parent = this.siginfo_obj;
            if (obj.Parent != null && ObjectSiginfoMap.ContainsKey(obj.Parent))
            {
                siginfo_parent = ObjectSiginfoMap[obj.Parent] as Spice.SignalContainer;
            }
            siginfo_parent.signals.Add(siginfo_obj);

            var spiceObjs = obj.Impl.Children.SPICEModelCollection;
            var spiceObj = CyPhyBuildVisitor.GetSpiceModel(obj, spiceObjs);
            if (spiceObj == null) // no spice model in this component, skip from generating
            {
                return;
            }
            GenerateSpice(obj, spiceObj);
        }

        public override void visit(Component obj)
        {
            DesignEntity de = obj;
            CodeGenerator.Logger.WriteDebug(
                    "SpiceVisitor::visit({0})",
                    obj.Name);

            // create a signal container for this component
            var siginfo_obj = new Spice.SignalContainer()
            {
                name = de.Name,
                gmeid = de.Impl.ID
            };
            ObjectSiginfoMap.Add(de, siginfo_obj);
            var siginfo_parent = this.siginfo_obj;
            if (de.Parent != null && ObjectSiginfoMap.ContainsKey(de.Parent))
            {
                siginfo_parent = ObjectSiginfoMap[de.Parent] as Spice.SignalContainer;
            }
            siginfo_parent.signals.Add(siginfo_obj);

            var spiceObjs = obj.Impl.Children.SPICEModelCollection;
            var spiceObj = CyPhyBuildVisitor.GetSpiceModel(obj, spiceObjs);
            if (spiceObj == null) // no spice model in this component, skip from generating
            {
                return;
            }
            GenerateSpice(de, spiceObj);
        }

        private void GenerateSpice(DesignEntity de, Tonka.SPICEModel spiceObj)
        {
            var ancestor = de.Parent;
            while (ancestor != null)
            {
                if (ComponentNodeMap.ContainsKey(ancestor))
                {
                    return;
                }
                ancestor = ancestor.Parent;
            }

            var nodes = circuit_obj.nodes;
            var spiceType = GetSpiceType(de);

            if (Grounds.Any(spiceType.Item2.Contains))  // is a ground node skip it
            {
                return;
            }

            var node = new Spice.Node();
            node.name = de.Name;
            node.type = spiceType.Item1;
            node.classType = spiceType.Item2;

            // error checking 
            if (node.type == (char)0)
            {
                CodeGenerator.Logger.WriteWarning("Missing Spice Type for component {0}", de.Name);
            }
            if (node.type == 'X' && node.classType == "")
            {
                CodeGenerator.Logger.WriteWarning("Missing Subcircuit Type for component {0}, should be X.<subckt-type>", de.Name);
            }

            if (node.classType != "" && !circuit_obj.subcircuits.ContainsKey(node.classType))
            {
                circuit_obj.subcircuits.Add(node.classType, de.SpiceLib);
            }

            foreach (var par in spiceObj.Children.SPICEModelParameterCollection)
            {
                if (node.parameters.ContainsKey(par.Name))
                {
                    CodeGenerator.Logger.WriteError("Duplicate Parameter: {0}: in Component <a href=\"mga:{2}\">{1}</a>",
                        par.Name,
                        de.Name,
                        de.Impl.ID);
                }
                else
                {
                    node.parameters.Add(par.Name, FindTestBenchParameter(par) ?? par.Attributes.Value);
                }
            }

            nodes.Add(node);
            ComponentNodeMap[de] = node;
        }

        private string FindTestBenchParameter(Tonka.SPICEModelParameter parameter)
        {
            if (parameter.AllSrcConnections.Count() == 0)
                return null;

            return FindTestBenchParameter(parameter, ((Tonka.SPICEModelParameterMap)parameter.AllSrcConnections.First()).SrcEnd);
        }

        private string FindTestBenchParameter(Tonka.SPICEModelParameter parameter, FCO element)
        {
            if (!(element is Tonka.Parameter || element is Tonka.Property))
            {
                //CodeGenerator.Logger.WriteWarning(String.Format("{0} depends on {1} in ValueFlow network.", parameter.Path, element.Path));
                CodeGenerator.Logger.WriteWarning("Root Source Obscured: <a href=\"mga:{1}\">{0}</a> in Valueflow path for {4} parameter <a href=\"mga:{3}\">{2}</a>",
                                                  element.Name, traceability.GetID(element.Impl),
                                                  parameter.Name, traceability.GetID(parameter.Impl), parameter.ParentContainer.ParentContainer.Name);
                return null;
            }
            if (element.AllSrcConnections.Count() == 0 || element.AllSrcConnections.First().Kind != "ValueFlow")
                return element.ParentContainer is Tonka.TestBench ? "${" + element.Name + "}" : null;
            return FindTestBenchParameter(parameter, ((Tonka.ValueFlow)element.AllSrcConnections.First()).SrcEnd);
        }

        public override void visit(Port obj)
        {
            CodeGenerator.Logger.WriteDebug(
                    "SpiceVisitor::visit({0}, dest connections: {1})",
                    obj.Name, obj.DstConnections.Count);

            if (!ComponentNodeMap.ContainsKey(obj.Parent)) // parent is a ground node
            {
                return;
            }

            var parentNode = ComponentNodeMap[obj.Parent];  // parent node
            var siginfo_parent = this.siginfo_obj;
            if (ObjectSiginfoMap.ContainsKey(obj.Parent))
            {
                siginfo_parent = ObjectSiginfoMap[obj.Parent] as Spice.SignalContainer;
            }

            int index = 0;
            try
            {
                index = obj.Impl.Attributes.SPICEPortNumber;
                if (index >= 0 && parentNode.nets.ContainsKey(index))
                {
                    CodeGenerator.Logger.WriteError("Duplicate SPICE Port Number: {0}: for Port <a href=\"mga:{2}\">{1}</a>",
                        index, obj.Name, obj.Impl.ID);
                    return;
                }
            }
            catch (System.FormatException ex)
            {
                index = -1;     // missing index
                CodeGenerator.Logger.WriteWarning("Invalid SPICE Port Number: {0}: for Port: <a href=\"mga:{2}\">{1}</a>",
                                                  obj.Impl.Attributes.SPICEPortNumber,
                                                  obj.Name, obj.Impl.ID);
            }

            if (PortNetMap.ContainsKey(obj))// port already mapped to a net object - no need to visit further
            {
                if (index >= 0) parentNode.nets.Add(index, PortNetMap[obj]);
                // spice signal info
                var siginfo_obj = new Spice.Signal()
                {
                    name = obj.Name,
                    gmeid = obj.Impl.ID,
                    spicePort = obj.Impl.Attributes.SPICEPortNumber,
                    net = PortNetMap[obj]
                };
                siginfo_parent.signals.Add(siginfo_obj);

                return;
            }

            // create a new net, 
            string net_obj = string.Format("{0}", ++netCount);
            if (!parentNode.nets.ContainsKey(index))
            {
                if (index >= 0) parentNode.nets.Add(index, net_obj);
                // spice signal info
                var siginfo_obj = new Spice.Signal()
                {
                    name = obj.Name,
                    gmeid = obj.Impl.ID,
                    spicePort = obj.Impl.Attributes.SPICEPortNumber,
                    net = net_obj
                };
                siginfo_parent.signals.Add(siginfo_obj);

                AddNetToSiginfoTraceability(obj, net_obj);
            }
            else
            {
                CodeGenerator.Logger.WriteWarning("Invalid SpiceOrder attribute for schematic port: <a href=\"mga:{0}\">{1}</a>",
                                                  obj.Impl.ID,
                                                  obj.Name);
            }

            // if we are in the signal integrity mode and the port has an associated parsed trace
            if (mode == CodeGenerator.Mode.SPICE_SI && 
                CodeGenerator.signalIntegrityLayout.portTraceMap.ContainsKey(obj))
            {
                var trace = CodeGenerator.signalIntegrityLayout.portTraceMap[obj];
                if (SigIntegrityTraces.Contains(trace.name))
                {
                    CodeGenerator.Logger.WriteInfo("Generating Trace Subcircuit for {0} on Port {1}.{2}", trace.name, obj.Parent.Name, obj.Name);
                    // insert a subckt to model a trace
                    var traceNode = new Spice.Node();
                    traceNode.name = trace.name;
                    traceNode.type = 'X';
                    traceNode.classType = string.Format("Trace_{0}", trace.name);
                    traceNode.nets.Add(0, net_obj);
                    // create a new net to replace the original net and carry that wire further
                    net_obj = string.Format("{0}", ++netCount);
                    traceNode.nets.Add(1, net_obj);
                    circuit_obj.nodes.Add(traceNode);
                    // generate the subckt
                    GenerateTraceSubckt(trace);
                }
            }
            else if (CodeGenerator.verbose)
                CodeGenerator.Logger.WriteWarning("Port {0} has no Trace", obj.Impl.Path); 



            // assign to all connected ports - some nets may not have any connection
            visit(obj, net_obj);
        }

        private void AddNetToSiginfoTraceability(Port obj, string net_obj)
        {
            //string id = traceability.GetID(obj.Parent.Impl.Impl);
            if (obj.Parent.Impl.Impl.MetaBase.Name != typeof(Tonka.TestComponent).Name)
            {
               // var ids = this.mgaIdToDominIDs[id];
                // CodeGenerator.Logger.WriteWarning("{0} {1} {2} {3} {4}", obj.Name, obj.Parent.Impl, ids.ID, ids.instanceGUID, ids.managedGUID);

                //id = traceability.GetID(obj.Impl.Impl);
                CyPhy2SchematicInterpreter.IDs ids;
                foreach (var connected in obj.connectedPorts.Values)
                {
                    if (this.mgaIdToDominIDs.TryGetValue(traceability.GetID(connected.Impl), out ids))
                    {
                        if (connected.ParentContainer is Tonka.Component)
                        {
                            CyPhy2SchematicInterpreter.IDs componentIDs;
                            CyPhy2SchematicInterpreter.IDs componentAssemblyIDs;
                            if (this.mgaIdToDominIDs.TryGetValue(traceability.GetID(connected.ParentContainer.Impl), out componentIDs) &&
                                this.mgaIdToDominIDs.TryGetValue(traceability.GetID(connected.ParentContainer.ParentContainer.Impl), out componentAssemblyIDs))
                            {
                                CodeGenerator.Logger.WriteDebug("Mapped Component port {0} {1} port ID:{2} ; component instanceGUID:{3} ; ca managedGUID:{4}", connected.ParentContainer.Name, connected.Name,
                                    ids.ID, componentIDs.instanceGUID, componentAssemblyIDs.managedGUID);

                                objectToNetId.Add(new CyPhy2SchematicInterpreter.IDs()
                                {
                                    ID = ids.ID,
                                    ConnectorID = ids.ConnectorID,
                                    instanceGUID = componentIDs.instanceGUID,
                                    managedGUID = componentAssemblyIDs.managedGUID
                                }, net_obj);

                            }
                            else
                            {
                                CodeGenerator.Logger.WriteDebug("Unmapped component port {0} {1} ", connected.Name, connected.ParentContainer.Name);
                            }
                        }
                        else if (connected.ParentContainer is Tonka.ComponentAssembly)
                        {
                            CyPhy2SchematicInterpreter.IDs componentAssemblyIDs;
                            if (this.mgaIdToDominIDs.TryGetValue(traceability.GetID(connected.ParentContainer.Impl), out componentAssemblyIDs))
                            {
                                objectToNetId.Add(new CyPhy2SchematicInterpreter.IDs()
                                {
                                    ID = ids.ID,
                                    ConnectorID = ids.ConnectorID,
                                    managedGUID = componentAssemblyIDs.managedGUID
                                }, net_obj);
                            }
                            else
                            {
                                CodeGenerator.Logger.WriteDebug("Unmapped ComponentAssembly port {0} {1} ", connected.Name, connected.ParentContainer.Name);
                            }
                        }
                    }
                    else
                    {
                        CodeGenerator.Logger.WriteWarning("Unmapped port <a href=\"mga:{0}\">{1}</a>", traceability.GetID(connected.Impl), connected.Impl.AbsPath);
                    }
                }
            }
        }

        private void visit(Port obj, string net_obj)
        {
            if (PortNetMap.ContainsKey(obj))
            {
                CodeGenerator.Logger.WriteDebug("Port {0}, already visited with net {1}, now {2}", obj.Name, PortNetMap[obj], net_obj);
                return;
            }
            PortNetMap[obj] = net_obj;  // add to map

            var allPorts =
                (from conn in obj.DstConnections select conn.DstPort).Union
                (from conn in obj.SrcConnections select conn.SrcPort);

            foreach (var port in allPorts) // visit sources
            {
                if (!PortNetMap.ContainsKey(port))
                    this.visit(port, net_obj);
            }
        }
    }
}
