using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using META;

using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using System.Globalization;

namespace CyPhy2Schematic.Schematic
{
    public class EdaVisitor : Visitor
    {
        private int netCount = 0;
        const float netLength = 2.54f;  // 0.1 inch = 2.54 mm
        const string netLayer = "91";
        const string nameLayer = "95";
        const string wireWidth = "0.3";
        const string labelSize = "1.27";

        private Dictionary<Port, Eagle.net> PortNetMap = new Dictionary<Port, Eagle.net>();

        private Eagle.eagle _eagle_obj;
        public Eagle.eagle eagle_obj {
            get
            {
                return _eagle_obj;
            }
            set
            {
                this._eagle_obj = value;
                this.schematic_obj = value.drawing.Item as Eagle.schematic;
            } 
        }
        private Eagle.schematic schematic_obj { get; set; }
        private CodeGenerator CodeGenerator;

        public EdaVisitor(CodeGenerator CodeGenerator)
        {
            this.CodeGenerator = CodeGenerator;
        }

        public override void visit(TestBench obj)
        {
            // Create a sheet for the testbench
            var sheet_obj = new Eagle.sheet();
            schematic_obj.sheets.sheet.Add(sheet_obj);
        }

        public override void visit(ComponentAssembly obj)
        {
            var layoutFile = (obj.Impl.Impl as GME.MGA.MgaFCO).RegistryValue["layoutFile"];
            int ancestorsWithLayoutJson = Layout.LayoutGenerator.getAncestorModels((GME.MGA.MgaFCO)obj.Impl.Impl).Where(parent => parent.RegistryValue["layoutFile"] != null).Count();
	    // we only care about the top-most layout.json
            if (layoutFile != null && ancestorsWithLayoutJson == 0)
            {
                var pathLayoutFile = Path.Combine(obj.Impl.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE), layoutFile);
                var managedGUID = CyPhy2Schematic.Layout.LayoutGenerator.GetComponentAssemblyManagedGuid(obj.Impl.Impl as GME.MGA.MgaModel);
                var layoutParser = new Layout.LayoutParser(pathLayoutFile, CodeGenerator.Logger, CodeGenerator)
                    {
                        parentInstanceGUID = string.IsNullOrEmpty(managedGUID) == false ? managedGUID: CyPhy2Schematic.Layout.LayoutGenerator.GetComponentAssemblyChainGuid(obj.Impl.Impl as GME.MGA.MgaModel),
                        parentGUID = CyPhy2Schematic.Layout.LayoutGenerator.GetComponentAssemblyID(obj.Impl.Impl as GME.MGA.MgaModel)
                    };
                Logger.WriteDebug("Design \"{0}\" is using layoutFile \"{1}\". Parent GUID : {2}", obj.Name, layoutFile, layoutParser.parentInstanceGUID ?? "[null]");
                layoutParser.BuildMaps();
                CodeGenerator.preRouted.Add(obj, layoutParser);
            }
        }

        public override void visit(Component obj)
        {
            if (obj.Impl is Tonka.TestComponent)
                return;

            // TBD SKN - pcb components are not "real" schematic components
            //         - do not generate eagle schematic parts for them 
            if (CodeGenerator.polyComponentClasses
                             .Contains((obj.Impl as Tonka.Component).Attributes.Classifications))
            {
                return;
            }

            var parts = schematic_obj.parts;
            var instances = schematic_obj.sheets.sheet.FirstOrDefault().instances;
            var libraries = schematic_obj.libraries;

            var schObj = obj.Impl.Children.EDAModelCollection.FirstOrDefault();
            if (schObj == null) // no schematic model in this component, skip from generating 
                return;

            var part = new Eagle.part();
            part.name = obj.Name;
            var device = schObj.Attributes.Device;
            part.device = (device != null) ? device : "device-unknown";
            var deviceset = schObj.Attributes.DeviceSet;
            part.deviceset = (deviceset != null) ? deviceset : "deviceset-unknown";
            var libName = schObj.Attributes.Library;
            part.library = (String.IsNullOrWhiteSpace(libName) == false) ? libName : "library-noname";
            var parVal = obj.Parameters.FirstOrDefault(p => p.Name.Equals("value"));
            var partMpn = obj.Impl.Children.PropertyCollection.FirstOrDefault(p => p.Name.Equals("octopart_mpn"));

            if (partMpn != null)
            {
                part.value = partMpn.Attributes.Value;  // See also: MOT-743
            }
            else if (parVal != null)
            {
                part.value = parVal.Value;
            }
            else
            {
                part.value = "";
            }

            MergeLibrary(obj, obj.SchematicLib, libraries, libName);

            var devLib = (obj.SchematicLib != null) ? obj.SchematicLib.drawing.Item as Eagle.library : null;
            if (devLib != null)
            {
                var techs = devLib.devicesets.deviceset.SelectMany(p => p.devices.device).SelectMany(q => q.technologies.technology).Select(t => t.name);
                part.technology = techs.FirstOrDefault();
            }

            parts.part.Add(part);
            CodeGenerator.partComponentMap[part] = obj; // add to map
            CodeGenerator.componentPartMap[obj] = part; // add to reverse map

            if (devLib != null)
            {
                var gates = devLib.devicesets.deviceset.SelectMany(p => p.gates.gate);
                foreach (var gate in gates)
                {
                    var instance = new Eagle.instance();
                    instance.part = part.name;
                    instance.gate = gate.name;
                    double x = float.Parse(gate.x, CultureInfo.InvariantCulture) + obj.CenterX;
                    double y = float.Parse(gate.y, CultureInfo.InvariantCulture) + obj.CenterY;

                    instance.x = x.ToString("F2");
                    instance.y = y.ToString("F2");

                    instances.instance.Add(instance);
                }
            }
        }

        public override void visit(Port obj)
        {
            if (obj.Parent.Impl is Tonka.TestComponent)
                return;

            if (CodeGenerator.polyComponentClasses
                             .Contains((obj.Parent.Impl as Tonka.Component).Attributes.Classifications))
            {
                return;
            }

            Logger.WriteDebug("CyPhySchematicVisitor::visit({0}, dest connections: {1})",
                              obj.Name,
                              obj.DstConnections.Count);

            if (PortNetMap.ContainsKey(obj))// port already mapped to a net object - no need to visit further
                return;
            if (obj.DstConnections.Count <= 0 && obj.SrcConnections.Count <= 0)  // no source and dest connections - skip this port
                return;

            var net_obj = new Eagle.net();
            net_obj.name = string.Format("N${0}", netCount++);
            visit(obj, net_obj);
            schematic_obj.sheets.sheet.FirstOrDefault().nets.net.Add(net_obj);    // relying that a sheet has been created already
        }

        private void visit(Port obj, Eagle.net net_obj)
        {
            if (obj.Parent.Impl is Tonka.TestComponent)
            {
            
            }
            else if (CodeGenerator.polyComponentClasses
                                  .Contains((obj.Parent.Impl as Tonka.Component).Attributes.Classifications))
            {
                // remember this 'pcb' net for later processing in generating layout
                if (!CodeGenerator.polyNetMap.ContainsKey(obj))
                {
                    CodeGenerator.polyNetMap.Add(obj, net_obj);
                }
            }
            else
            {
                // create a segment for this object
                var segment_obj = new Eagle.segment();
                CreateWireSegment(obj, segment_obj);  // simple routing
                CreatePinRef(obj, segment_obj); // destination pin
                net_obj.segment.Add(segment_obj);
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

        private void CreatePinRef(Port obj, Eagle.segment segment_obj)
        {
            var gate = obj.Impl.Attributes.EDAGate;
            var pinref_obj = new Eagle.pinref()
            {
                gate = (String.IsNullOrWhiteSpace(gate) == false) ? gate : "gate-unknown",
                part = obj.Parent.Name,
                pin = obj.Name,
            };
            segment_obj.Items.Add(pinref_obj);
        }

        private void CreateWireSegment(Port port, Eagle.segment segment_obj)
        {
            // create two short wire segments: 1. from src pin to, and 2. to dst pin
            // TODO: create vertical segments for vertical pins (or rotated symbols)
            var rot = port.Impl.Attributes.EDASymbolRotation;
            double x1 = port.CanvasX;
            double y1 = port.CanvasY;
            double x2 = x1;
            double y2 = y1;
            if (rot.Equals("R90") || rot.Equals("90"))
                y2 -= netLength; // 90 pointing down
            else if (rot.Equals("R270") || rot.Equals("270"))
                y2 += netLength; // 270 pointing up
            else if (rot.Equals("R180") || rot.Equals("180"))
                x2 += netLength; // 180 going right
            else
                x2 -= netLength; // 0 going left

            var wire_obj = new Eagle.wire()
            {
                x1 = x1.ToString("F2"),
                y1 = y1.ToString("F2"),
                x2 = x2.ToString("F2"),
                y2 = y2.ToString("F2"),
                layer = netLayer,
                width = wireWidth
            };
            
            segment_obj.Items.Add(wire_obj);

            var label_obj = new Eagle.label()
            {
                x = wire_obj.x2,
                y = wire_obj.y2,
                size = labelSize,
                layer = nameLayer
            };
            
            segment_obj.Items.Add(label_obj);
        }

        private void MergeLibrary(Component obj, Eagle.eagle eagleSrc, Eagle.libraries libsDst, string libName)
        {
            var libSrc = eagleSrc.drawing.Item as Eagle.library;
            if (libSrc == null)
            {
                Logger.WriteWarning("No Schematic Library for Component: <a href=\"MGA:{0}\">{1}</a>", obj.Impl.ID, obj.Impl.Name);
                return;
            }
            if (String.IsNullOrWhiteSpace(libName))
                libName = "library-noname";
            var libDst = libsDst.library.Where(l => l.name.Equals(libName)).FirstOrDefault();
            if (libDst == null)
            {
                libSrc.name = libName;
                libsDst.library.Add(libSrc);
            }
            else
            {
                // keep the description
                // add to packages, add to symbols, add to device-sets/devices
                foreach (Eagle.package pkg in libSrc.packages.package)
                {
                    if (libDst.packages.package.Where(p => p.name.Equals(pkg.name)).Count() == 0)
                        libDst.packages.package.Add(pkg);
                }
                foreach (Eagle.symbol sym in libSrc.symbols.symbol)
                {
                    if (libDst.symbols.symbol.Where(s => s.name.Equals(sym.name)).Count() == 0)
                        libDst.symbols.symbol.Add(sym);
                }
                foreach (Eagle.deviceset dset in libSrc.devicesets.deviceset)
                {
                    if (libDst.devicesets.deviceset.Where(d => d.name.Equals(dset.name)).Count() == 0)
                        libDst.devicesets.deviceset.Add(dset);
                    else
                    {
                        var dstDset = libDst.devicesets.deviceset.Where(d => d.name.Equals(dset.name)).FirstOrDefault();
                        foreach (Eagle.device dev in dset.devices.device)
                        {
                            if (dstDset.devices.device.Where(dd => dd.name.Equals(dev.name)).Count() == 0)
                                dstDset.devices.device.Add(dev);
                        }
                    }
                }

            }

        }
    }
}
