using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using META;


namespace CyPhy2SystemC.SystemC
{

    public class CyPhyBuildVisitor : Visitor
    {
        //
        // This Class contains visitor methods to build the lightweight Component Object network from CyPhy Models
        //

        public static Dictionary<string, Component> Components { get; set; }
        public static Dictionary<string, Port> Ports { get; set; }
        public string ProjectDirectory { get; set; }

        public CyPhyBuildVisitor(string projectDirectory)  // this is a singleton object and the constructor will be called once
        {
            Components = new Dictionary<string, Component>();
            Ports = new Dictionary<string, Port>();
            this.ProjectDirectory = projectDirectory;
        }

        public override void visit(TestBench obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("CyPhyBuildVisitor::visit TestBench: {0}", obj.Name);

            var testBench = obj.Impl;
            
            if (testBench.Children.ComponentAssemblyCollection.Count() == 0)
            {
                CodeGenerator.GMEConsole.Error.WriteLine("No valid component assembly in testbench {0}", obj.Name);
                return;
            }

            foreach (var ca in testBench.Children.ComponentAssemblyCollection)
            {
                var componentAssembly_obj = new ComponentAssembly(ca);
                obj.ComponentAssemblies.Add(componentAssembly_obj);
            }

            foreach (var tc in testBench.Children.TestComponentCollection)
            {
                var testComponent_obj = new Component(tc);
                obj.TestComponents.Add(testComponent_obj);
            }

            // Save CyPhy TestBench parameters, such as "simulationTime", in the CyPhy2SystemC testbench object.  MOT-516
            foreach (var p in testBench.Children.ParameterCollection)
            {
                string paramName = p.Name;
                if (!obj.TestParameters.ContainsKey(paramName))
                {
                    obj.TestParameters[paramName] = p;
                }
            }
        }

        public override void visit(ComponentAssembly obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("CyPhyBuildVisitor::visit ComponentAssembly: {0}", obj.Name);

            var ca = obj.Impl;

            // ------- ComponentAssemblies -------
            foreach (var innerComponentAssembly in ca.Children.ComponentAssemblyCollection)
            {
                var innerComponentAssembly_obj = new ComponentAssembly(innerComponentAssembly)
                {
                    Parent = obj,
                };
                obj.ComponentAssemblyInstances.Add(innerComponentAssembly_obj);
            }

            // ------- Components -------
            Component component_obj;
            foreach (var component in ca.Children.ComponentCollection)
            {
                // heuristics (nasty lookahead), will need to handle this properly with changes in CyPhyML
                var SystemCModel = component.Children.SystemCModelCollection.FirstOrDefault();
                if (SystemCModel != null && !String.IsNullOrWhiteSpace(SystemCModel.Attributes.ModuleName) &&
                            SystemCModel.Attributes.ModuleName.ToLower().StartsWith("arduino"))
                {
                    component_obj = new ArduinoComponent(component)
                    {
                        Parent = obj,
                    };
                }
                else
                {
                    component_obj = new Component(component)
                    {
                        Parent = obj,
                    };
                }

                obj.ComponentInstances.Add(component_obj);
                CyPhyBuildVisitor.Components.Add(component.ID, component_obj);   // Add to global component list, are these instance ID-s or component type ID-s?
            }

        }

        public override void visit(Component obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("CyPhyBuildVisitor::visit Component: {0}", obj.Name);

            var component = obj.Impl;
            var ca = obj.Parent;

            var SystemCModel = component.Children.SystemCModelCollection.FirstOrDefault();
            if (SystemCModel != null)
            {
                obj.HasSystemCModel = true;
                if (component is CyPhy.Component)
                {
                    obj.ComponentDirectory = ((CyPhy.Component)component).GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE);
                }
                
                // Try checking ModuleName attribute to get the name of the C++ class.
                // Since this is new, the fallback plan is to take the SystemCModel name (old style).
                if (!String.IsNullOrWhiteSpace(SystemCModel.Attributes.ModuleName))
                    obj.Name = SystemCModel.Attributes.ModuleName;
                else
                    obj.Name = SystemCModel.Name;

                // get all SystemC code resources
                foreach (var libRes in SystemCModel.SrcConnections.UsesResourceCollection.Select(c => c.SrcEnds.Resource).Union(
                    SystemCModel.DstConnections.UsesResourceCollection.Select(c => c.DstEnds.Resource)))
                {

                    // TBD SKN: respath relative or absolute 
                    // currently assume that its relative and later differentiate if its absolute
                    var resPath = ((libRes != null) ? libRes.Attributes.Path : "").Replace('/', '\\');
                    obj.Sources.Add(new SourceFile(resPath));
                }
                if (obj.Sources.Count == 0)
                {
                    CodeGenerator.GMEConsole.Warning.WriteLine("Error No SystemC source path is given to <a href=\"mga:{0}\">{1}</a>",
                        SystemCModel.ID, SystemCModel.Name);
                }

                foreach (var port in SystemCModel.Children.SystemCPortCollection)
                {
                    var port_obj = new Port(port)
                    {
                        Parent = obj
                    };
                    switch (port.Attributes.DataType)
                    {
                        case CyPhyClasses.SystemCPort.AttributesClass.DataType_enum.@bool:
                            if (port.Attributes.DataTypeDimension > 1)
                            {
                                CodeGenerator.GMEConsole.Warning.WriteLine("Array of bool is not supported for port <a href=\"mga:{0}\">{1}</a>", port.ID, port.Name);
                            }
                            else
                            {
                                port_obj.DataType = "bool";
                            }
                            break;
                        case CyPhyClasses.SystemCPort.AttributesClass.DataType_enum.sc_bit:
                            if (port.Attributes.DataTypeDimension > 1)
                            {
                                port_obj.DataType = String.Format("sc_bv<{0}>", port.Attributes.DataTypeDimension);
                            }
                            else
                            {
                                port_obj.DataType = "sc_bit";
                            }
                            break;
                        case CyPhyClasses.SystemCPort.AttributesClass.DataType_enum.sc_int:
                            if (port.Attributes.DataTypeDimension > 1)
                            {
                                port_obj.DataType = String.Format("sc_int<{0}>", port.Attributes.DataTypeDimension);
                            }
                            else
                            {
                                CodeGenerator.GMEConsole.Warning.WriteLine("Exact size of sc_int is not set for port <a href=\"mga:{0}\">{1}</a>", port.ID, port.Name);
                            }
                            break;
                        case CyPhyClasses.SystemCPort.AttributesClass.DataType_enum.sc_logic:
                            if (port.Attributes.DataTypeDimension > 1)
                            {
                                port_obj.DataType = String.Format("sc_lv<{0}>", port.Attributes.DataTypeDimension);
                            }
                            else
                            {
                                port_obj.DataType = "sc_logic";
                            }
                            break;
                        case CyPhyClasses.SystemCPort.AttributesClass.DataType_enum.sc_uint:
                            if (port.Attributes.DataTypeDimension > 1)
                            {
                                port_obj.DataType = String.Format("sc_uint<{0}>", port.Attributes.DataTypeDimension);
                            }
                            else
                            {
                                CodeGenerator.GMEConsole.Warning.WriteLine("Exact size of sc_uint is not set for port <a href=\"mga:{0}\">{1}</a>", port.ID, port.Name);
                            }
                            break;
                        default:
                            CodeGenerator.GMEConsole.Warning.WriteLine("Unkown datatype for port <a href=\"mga:{0}\">{1}</a>", port.ID, port.Name);
                            break;
                    }
                    obj.Ports.Add(port_obj);
                    CyPhyBuildVisitor.Ports.Add(port.ID, port_obj);
                    if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Mapping Port <a href=\"mga:{0}\">{1}</a>", port.ID, port.Name);
                }
            }

            if (obj is ArduinoComponent)
            {
                ArduinoComponent arduinoObj = obj as ArduinoComponent;

                var sourceParameter = component.Children.PropertyCollection.Where(c => c.Name.ToLower() == "source").First();
                if (sourceParameter != null)
                {
                    arduinoObj.FirmwarePath = sourceParameter.Attributes.Value;
                }
                else
                {
                    CodeGenerator.GMEConsole.Warning.WriteLine("Error No firmware source path is given to Arduino component <a href=\"mga:{0}\">{1}</a>",
                       component.ID, component.Name);
                }

                obj.Sources.Add(new SourceFile(arduinoObj.FirmwarePath));
            }
        }

        public override void visit(Port obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("CyPhyBuildVisitor::visit Port: {0}", obj.Name);
        }
    }

    public class CyPhyConnectVisitor : Visitor
    {
        public CyPhyConnectVisitor()
        {
        }

        public override void visit(Port obj)
        {
 
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("CyPhyConnectVisitor::visit Port: <a href=\"mga:{0}\">{1}</a>", obj.Impl.ID, obj.Impl.Path);
            var elecPort = obj.Impl as CyPhy.SystemCPort;
            if (elecPort != null &&
                ((elecPort.Attributes.Directionality == CyPhyClasses.SystemCPort.AttributesClass.Directionality_enum.@out) || 
                (elecPort.Attributes.Directionality == CyPhyClasses.SystemCPort.AttributesClass.Directionality_enum.inout))
                )
            {
                obj.IsOutput = true;
                // from SystemC port navigate out to component port (or connector) in either connection direction
                var compPorts = elecPort.DstConnections.PortCompositionCollection.Select(p => p.DstEnd).
                    Union(elecPort.SrcConnections.PortCompositionCollection.Select(p => p.SrcEnd));
                foreach (var compPort in compPorts)
                {
                    Dictionary<string, object> visited = new Dictionary<string, object>();
                    visited.Clear();
                    if (compPort.ParentContainer is CyPhy.Connector)                     // traverse connector chain, carry port name in srcConnector
                        Traverse(compPort.Name, obj, obj.Parent.Impl, compPort.ParentContainer as CyPhy.Connector, visited);
                    else if (compPort.ParentContainer is CyPhy.DesignElement)
                        Traverse(obj, obj.Parent.Impl, compPort as CyPhy.Port, visited); // traverse port chain
                    else if ((compPort.ParentContainer is CyPhy.SystemCModel) && CyPhyBuildVisitor.Ports.ContainsKey(compPort.ID))
                        ConnectPorts(obj, CyPhyBuildVisitor.Ports[compPort.ID]);
                }
            }
        }

        private void Traverse(string srcConnectorName, Port srcPort_obj, CyPhy.DesignElement parent, CyPhy.Connector connector, Dictionary<string, object> visited)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Traverse Connector: {0}, Mapped-Port: {1}",
                connector.Path.Substring(CodeGenerator.BasePath.Length + 1), srcConnectorName);

            visited.Add(connector.ID, connector);

            // continue traversal as connector
            var remotes = connector.DstConnections.ConnectorCompositionCollection.Select(p => p.DstEnd).Union(
                    connector.SrcConnections.ConnectorCompositionCollection.Select(p => p.SrcEnd));
            foreach (var remote in remotes)
            {
                if (visited.ContainsKey(remote.ID)) // already visited
                    continue;
                if (remote.ParentContainer is CyPhy.DesignElement)
                    Traverse(srcConnectorName, srcPort_obj, remote.ParentContainer as CyPhy.DesignElement, remote as CyPhy.Connector, visited);
            }

            // continue traversal through named port
            var mappedPorts = connector.Children.SystemCPortCollection.Where(p => p.Name.Equals(srcConnectorName));
            foreach (var mappedPort in mappedPorts)
            {
                var remotePorts = mappedPort.DstConnections.PortCompositionCollection.Select(p => p.DstEnd).Union(
                    mappedPort.SrcConnections.PortCompositionCollection.Select(p => p.SrcEnd));
                foreach (var remotePort in remotePorts)
                {
                    if (visited.ContainsKey(remotePort.ID)) // already visited
                        continue;
                    if (remotePort.ParentContainer is CyPhy.DesignElement)
                        Traverse(srcPort_obj, remotePort.ParentContainer as CyPhy.DesignElement, remotePort as CyPhy.Port, visited);
                    else if ((remotePort.ParentContainer is CyPhy.SystemCModel) &&
                        CyPhyBuildVisitor.Ports.ContainsKey(remotePort.ID))
                        ConnectPorts(srcPort_obj, CyPhyBuildVisitor.Ports[remotePort.ID]);
                }
            }
        }

        private void Traverse(Port srcPort_obj, CyPhy.DesignElement parent, CyPhy.Port port, Dictionary<string, object> visited)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Traverse Port: port {0}",
                port.Path.Substring(CodeGenerator.BasePath.Length + 1));

            visited.Add(port.ID, port);

            // continue traversal
            var remotes = port.DstConnections.PortCompositionCollection.Select(p => p.DstEnd).Union(
                port.SrcConnections.PortCompositionCollection.Select(p => p.SrcEnd));

            foreach (var remote in remotes)
            {
                if (visited.ContainsKey(remote.ID))
                    continue;       // already visited continue

                if (remote.ParentContainer is CyPhy.DesignElement)  // remote is contained in a Component or ComponentAssembly
                    Traverse(srcPort_obj, remote.ParentContainer as CyPhy.DesignElement, remote as CyPhy.Port, visited);
                else if (remote.ParentContainer is CyPhy.Connector) // remote is contained in a Connector
                    Traverse(remote.Name, srcPort_obj, remote.ParentContainer.ParentContainer as CyPhy.DesignElement,
                        remote.ParentContainer as CyPhy.Connector, visited);
                else if ((remote.ParentContainer is CyPhy.SystemCModel) &&    // remote is contained in a SystemC Model
                    (CyPhyBuildVisitor.Ports.ContainsKey(remote.ID)))
                    ConnectPorts(srcPort_obj, CyPhyBuildVisitor.Ports[remote.ID]);
            }
        }

        private void ConnectPorts(Port srcPort_obj, Port dstPort_obj)
        {
            if (srcPort_obj.Impl.Equals(dstPort_obj.Impl))
                return;

            if ((dstPort_obj.Impl.Attributes.Directionality != CyPhyClasses.SystemCPort.AttributesClass.Directionality_enum.@in) &&
                (dstPort_obj.Impl.Attributes.Directionality != CyPhyClasses.SystemCPort.AttributesClass.Directionality_enum.inout))
            {
                CodeGenerator.GMEConsole.Warning.WriteLine("Multiple drivers on the same channel: <a href=\"mga:{0}\">{1}</a>, <a href=\"mga:{2}\">{3}</a>", 
                    srcPort_obj.Impl.ID, srcPort_obj.Impl.Path, dstPort_obj.Impl.ID, dstPort_obj.Impl.Path);
            }

            Connection conn_obj = new Connection(srcPort_obj, dstPort_obj);
            srcPort_obj.DstConnections.Add(conn_obj);
            dstPort_obj.SrcConnections.Add(conn_obj);
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Connecting Port {0} to {1}", srcPort_obj.Impl.Name,
                dstPort_obj.Impl.Name);
        }

    }
}
 