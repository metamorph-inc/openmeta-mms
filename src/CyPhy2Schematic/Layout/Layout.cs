using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using META;

using CyPhyGUIs;
using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;

using Eagle = CyPhy2Schematic.Schematic.Eagle;
using CyPhy2Schematic.Schematic;
using LayoutJson;
using System.Globalization;
using System.Xml.Linq;
using GME.MGA;
using System.Diagnostics;

namespace CyPhy2Schematic.Layout
{
    class ExactConstraint : Constraint
    {
        public double? x { get; set; }
        public double? y { get; set; }
        public int? layer { get; set; }
        public int? rotation { get; set; }
    }

    class GlobalLayoutConstraintException : Constraint  // MOT-728
    {
        // Exceptions just use the "name" and "type" from the base class.
    }
        
    class RangeConstraint : Constraint
    {
        public string x { get; set; }
        public string y { get; set; }
        public string layer { get; set; }
    }

    class RelativeConstraint : Constraint
    {
        public double? x { get; set; }
        public double? y { get; set; }
        public double? x1 { get; set; } //Relative constraint coordinates translated assuming origin is rotated
        public double? y1 { get; set; }
        public double? x2 { get; set; }
        public double? y2 { get; set; }
        public double? x3 { get; set; }
        public double? y3 { get; set; }

        public int pkg_idx { get; set; }
        public int? layer { get; set; }

        /*
         * not present = don't care; 0 = 0 degrees, 1 = 90 degrees...
         * Values here correspond to the same rotation as the translated x1,y1/x2,y2/etc. above.
         */
        public int? relativeRotation { get; set; }
    }

    class RelativeRangeConstraint : Constraint
    {
        public string x { get; set; }
        public string y { get; set; }
        public int pkg_idx { get; set; }
        public int? layer { get; set; }
    }

    /**
     * Defines the bounding box of a package, using the lower left and upper
     * right coordinates of the box relative to the package's origin
     */
    public class BoundingBox
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
    }

    // Class to handle placement of subcircuit components that were added to a pre-routed subcircuit
    // after the subcircuit was pre-routed.  The extra components (bones) are arranged in a "boneyard"
    // to the left of the board's origin. MOT-782
    public class Boneyard
    {
        private List<Package> bones;    // The bones are extra component packages.

        // The yard is a rectangular area on the left of the EAGLE board where bones will be placed.
        public double yardHeight { set; get; }
        public double yardWidth { set; get; }
        public double maxYardHeight { set; get; }
        private int boneCount;


        // Distance between the yard and the origin of the board:
        public double boardMargin { set; get; }

        // Horizontal distance between parts in the yard:
        public double horizontalSeparation { set; get; }

        // Vertical distance between parts in the yard:
        public double verticalSeparation { set; get; }
        
        public Boneyard()
        {
            bones = new List<Package>(); // Internal bones
            boneCount = 0;
            yardHeight = 0;
            yardWidth = 0;
            maxYardHeight = 20;
            boardMargin = 2;
            horizontalSeparation = 1;
            verticalSeparation = 1;
        }
        public void Add(Package item)
        {
            bones.Add(item); // add to the internal bones
            // Sort them by width, to help the eventual arrangement of bones to appear balanced.
            List<Package> sortedBones = bones.OrderBy(x => x.koWidth).ThenBy(x => x.koHeight).ThenBy(x => x.name).ToList();
            bones = sortedBones;
            boneCount = bones.Count();
        }
        public List<Package> GetBones()
        {
            List<Package> bonesCopy = new List<Package>(bones);
            return bonesCopy;
        }

        public void ShowBones()
        {
            foreach (Package extraPkg in bones)
            {
                Debug.WriteLine("Package '{0}': ({3}, {4}) width = {1}, height = {2}", extraPkg.name, extraPkg.koWidth, extraPkg.koHeight, extraPkg.x, extraPkg.y );
            }
        }

        public void PlaceBones()
        {
            if( boneCount > 0 )
            {
                // Get the width of the widest bone
                double widestBoneWidth = Math.Ceiling( bones[boneCount - 1].koWidth.GetValueOrDefault(0.0) );
                bool placedOK = false;
                for (double testYardWidth = widestBoneWidth; (testYardWidth < (5 * (widestBoneWidth + horizontalSeparation))) && (!placedOK); testYardWidth += (widestBoneWidth + horizontalSeparation))
                {
                    placedOK = (checkYardPlacement(testYardWidth) < maxYardHeight);
                }
            }
         }

        public double checkYardPlacement(double testYardWidth)
        {
            // Puts the bones in the yard, starting at the bottom.
            // Returns the resulting height of the yard.
            yardHeight = 0;
            yardWidth = testYardWidth;
            double rowWidthUsed = 0;
            double rowHeight = 0;
            double rowBaseHeight = 0;

            List<Package> orderedBones = new List<Package>(bones);
            orderedBones.Reverse();

            foreach (Package bone in orderedBones)
            {
                double remainingWidthInRow = yardWidth - rowWidthUsed;
                double width =  bone.koWidth.GetValueOrDefault( 0.0 );
                double height = bone.koHeight.GetValueOrDefault( 0.0 );

                // Check if the bone's width will fit in the current row.
                if (remainingWidthInRow < width)
                {
                    // No, move up a row
                    rowBaseHeight += rowHeight + verticalSeparation;
                    rowHeight = 0;
                    rowWidthUsed = 0;
                    remainingWidthInRow = yardWidth;
                }

                // Place the bone in the row.
                bone.x = -(remainingWidthInRow + boardMargin);
                bone.y = rowBaseHeight;
                rowWidthUsed += width + horizontalSeparation;
                rowHeight = Math.Max(rowHeight, height);
            }

            yardHeight = rowBaseHeight + rowHeight;
            return yardHeight;
        }
    }

    public class LayoutGenerator
    {
        public static IEnumerable<IMgaModel> getAncestorModels(IMgaFCO fco, IMgaModel stopAt = null)
        {
            IMgaModel parent = fco.ParentModel;
            while (parent != null && (stopAt == null || parent.ID != stopAt.ID))
            {
                yield return parent;
                parent = parent.ParentModel;
            }
        }
        public static IEnumerable<IMgaModel> getSelfAndAncestorModels(IMgaModel model, IMgaModel stopAt = null)
        {
            yield return model;
            if (model.ParentModel != null)
            {
                foreach (IMgaModel ancestor in getAncestorModels(model.ParentModel))
                {
                    yield return ancestor;
                }
            }
        }

        public static string GetComponentAssemblyID(IMgaModel componentAssembly)
        {
            var managedGuid = GetComponentAssemblyManagedGuid(componentAssembly);
            if (string.IsNullOrEmpty(managedGuid) == false)
            {
                return managedGuid;
            }
            var guidChain = GetComponentAssemblyChainGuid(componentAssembly);
            if (guidChain != null)
            {
                return guidChain;
            }
            return new Guid(componentAssembly.GetGuidDisp()).ToString("D");
        }

        public static string GetComponentAssemblyChainGuid(IMgaModel componentAssembly)
        {
	        return componentAssembly.RegistryValue["Elaborator/InstanceGUID_Chain"];
        }
        
        public static string GetComponentAssemblyManagedGuid(IMgaModel componentAssembly)
        {
            return componentAssembly.StrAttrByName["ManagedGUID"];
        }

        public bool bonesFound { get; set; }    // MOT-782
        public LayoutJson.Layout boardLayout { get; set; }
        public GMELogger Logger { get; set; }
        public Dictionary<Package, Eagle.part> pkgPartMap { get; set; }
        public Dictionary<Eagle.part, Package> partPkgMap { get; set; }

        public Dictionary<ComponentAssembly, Package> preroutedPkgMap { get; set; }

        public static string[] LayersForOuterBoundingBoxComputation = new string[]
        {
            "17",  // Pads
            "21",  // tPlace  <-- top silkscreen:  may be too conservative 
            "22",  // bPlace  <-- bot silkscreen:  may be too conservative
            "29",  // tStop
            "30",  // bStop
            "31",  // tCream
            "32",  // bCream
            "39",  // tKeepout
            "40",  // bKeepout
            "41",  // tRestrict
            "42",  // bRestrict
            "43",  // vRestrict
            "44",  // Drills
            "45"   // Holes
        };

        public static string[] LayersForInnerBoundingBoxComputation = new string[]
        {
            "17",  // Pads
            "21",  // tPlace  <-- top silkscreen:  may be too conservative 
            "22",  // bPlace  <-- bot silkscreen:  may be too conservative
            //"29",  // tStop
            //"30",  // bStop
            "31",  // tCream
            "32",  // bCream
            "39",  // tKeepout
            "40",  // bKeepout
            "44",  // Drills
            "45"   // Holes
        };

        // Function to quantize a layout-box relative constraint coordinate, for MOT-779.
        // See also: ConvertRelativeConstraintFromOrigin().
        public static double quantize(double x)
        {
            return (0.1) * Math.Ceiling(10.0 * x);
        }

        private CodeGenerator CodeGenerator;

        public LayoutGenerator(Eagle.schematic sch_obj, TestBench tb_obj, GMELogger inLogger, String pathOutput, CodeGenerator CodeGenerator)
        {
            this.Logger = inLogger;
            this.pkgPartMap = new Dictionary<Package, Eagle.part>();
            this.partPkgMap = new Dictionary<Eagle.part, Package>();
            this.preroutedPkgMap = new Dictionary<ComponentAssembly, Package>();
            this.CodeGenerator = CodeGenerator;

            this.bonesFound = false;    // MOT-782.

            boardLayout = new LayoutJson.Layout();
            boardLayout.numLayers = 2;

            // we currently support the following testbench parameters related to layout
            // boardWidth
            // boardHeight
            // boardCutouts - array of exclusion rectangles (x,y,w,h; ...) for non-rectangular boards
            // boardTemplate
            // designRules
            // autoRouterConfig

            // all of these parameters can be supplied either as part of a pcb_component model
            // or as parameters in testbench
            // the file parameters in testbench are specified as resources in pcb_component model
            string pcbBW, tbBW, pcbBH, tbBH, pcbBES, tbBES, pcbICS, tbICS, pcbBC, tbBC;
            GetLayoutParametersFromPcbComponent(tb_obj, boardLayout,
                out pcbBW, out pcbBH, out pcbBES, out pcbICS, out pcbBC);
            GetLayoutParametersFromTestBench(tb_obj, boardLayout,
                out tbBW, out tbBH, out tbBES, out tbICS, out tbBC);

            string boardWidth = (tbBW != null) ? tbBW : pcbBW;
            string boardHeight = (tbBH != null) ? tbBH : pcbBH;
            string boardEdgeSpace = (tbBES != null) ? tbBES : pcbBES;
            string interChipSpace = (tbICS != null) ? tbICS : pcbICS;
            string boardCutouts = (tbBC != null) ? tbBC : pcbBC;

            Double dBoardWidth;
            if (false == Double.TryParse(boardWidth, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out dBoardWidth))
            {
                Logger.WriteWarning("Exception while reading board width: {0}", boardWidth);
                dBoardWidth = 40.0;
            }
            boardLayout.boardWidth = dBoardWidth;

            Double dBoardHeight;
            if (false == Double.TryParse(boardHeight, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out dBoardHeight))
            {
                Logger.WriteWarning("Exception while reading board height: {0}", boardHeight);
                dBoardHeight = 40.0;
            }
            boardLayout.boardHeight = dBoardHeight;

            Double dBoardEdgeSpace;
            if (false == Double.TryParse(boardEdgeSpace, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out dBoardEdgeSpace))
            {
                Logger.WriteWarning("Exception while reading board edge space: {0}", dBoardEdgeSpace);
                dBoardEdgeSpace = 0.5;
            }
            boardLayout.boardEdgeSpace = dBoardEdgeSpace;

            Double dInterChipSpace;
            if (false == Double.TryParse(interChipSpace, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out dInterChipSpace))
            {
                Logger.WriteWarning("Exception while reading board height: {0}", boardHeight);
                dInterChipSpace = 0.2;
            }
            boardLayout.interChipSpace = dInterChipSpace;

            if (boardCutouts != null && boardLayout.boardTemplate != null)
                GenerateBoardCutoutConstraints(boardCutouts);
            else if (boardCutouts != null)
                Logger.WriteWarning("Please specify a board template when defining board cutouts");

            boardLayout.packages = new List<Package>();
            int pkg_idx = 0;

            // we want to handle prerouted assemblies as follows
            foreach (var ca in tb_obj.ComponentAssemblies)
            {
                pkg_idx = HandlePreRoutedAsm(ca, pkg_idx);
            }

            #region Add Packages to Layout Json
            Boneyard myBoneyard = new Boneyard();

            // compute part dimensions from 
            foreach (var part in sch_obj.parts.part)
            {
                var dev = sch_obj.libraries.library.Where(l => l.name.Equals(part.library)).
                    SelectMany(l => l.devicesets.deviceset).Where(ds => ds.name.Equals(part.deviceset)).
                    SelectMany(ds => ds.devices.device).Where(d => d.name.Equals(part.device)).FirstOrDefault();
                var spkg = (dev != null)
                           ? sch_obj.libraries.library.Where(l => l.name.Equals(part.library))
                                    .SelectMany(l => l.packages.package).Where(p => p.name.Equals(dev.package))
                                    .FirstOrDefault()
                           : null;

                Package pkg = new Package();
                pkg.name = part.name;
                pkg.pkg_idx = pkg_idx++;

                if (dev == null || spkg == null)
                {
                    // emit warning
                    Logger.WriteWarning("Unable to get package size for part - layout/chipfit results may be inaccurate: {0}", part.name);
                }
                else
                {
                    pkg.package = spkg.name;        // note that the eagle package information may be incomplete - should really have curated version from CyPhy

                    #region ComputePackageSize

                    BoundingBox outerBoundingBox = ComputeBoundingBox(spkg, LayersForOuterBoundingBoxComputation);
                    BoundingBox innerBoundingBox = ComputeBoundingBox(spkg, LayersForInnerBoundingBoxComputation);

                    pkg.width = innerBoundingBox.MaxX - innerBoundingBox.MinX;
                    pkg.height = innerBoundingBox.MaxY - innerBoundingBox.MinY;
                    //TODO: this should probably be the package's origin, not the package's center
                    //Also, why is this being truncated to nearest 0.1?
                    pkg.originX = Math.Floor(10.0 * (innerBoundingBox.MaxX + innerBoundingBox.MinX) / 2.0) / 10.0;
                    pkg.originY = Math.Floor(10.0 * (innerBoundingBox.MaxY + innerBoundingBox.MinY) / 2.0) / 10.0;

                    double packageCenterX = (innerBoundingBox.MinX + innerBoundingBox.MaxX) / 2.0;
                    double packageCenterY = (innerBoundingBox.MinY + innerBoundingBox.MaxY) / 2.0;

                    //outer bounding box as sent to layout solver needs to be symmetrical about the package's
                    //center; calculate the largest dimensions in either direction to get a symmetrical
                    //box that contains the outer bounding box
                    double leftOuterWidth = packageCenterX - outerBoundingBox.MinX;
                    double rightOuterWidth = outerBoundingBox.MaxX - packageCenterX;

                    double bottomOuterHeight = packageCenterY - outerBoundingBox.MinY;
                    double topOuterHeight = outerBoundingBox.MaxY - packageCenterY;

                    double largestWidth = Math.Max(leftOuterWidth, rightOuterWidth);
                    double largestHeight = Math.Max(bottomOuterHeight, topOuterHeight);

                    pkg.koHeight = largestHeight * 2;
                    pkg.koWidth = largestWidth * 2;

                    #endregion

                    // emit component ID for locating components in CAD assembly
                    if (CodeGenerator.partComponentMap.ContainsKey(part))
                    {
                        var comp_obj = CodeGenerator.partComponentMap[part];
                        var comp = comp_obj.Impl as Tonka.Component;

                        // emit component ID for locating components in CAD assembly
                        pkg.ComponentID = GetComponentID(comp);

                        Boolean isMultiLayer = comp.Children.EDAModelCollection.Any(e => e.Attributes.HasMultiLayerFootprint);
                        if (isMultiLayer)
                            pkg.multiLayer = true;

                        #region GlobalConstraintExceptions
                        // Global-constraint exceptions related to this part, MOT-728
                        var exceptions =
                            from conn in comp.SrcConnections.ApplyGlobalLayoutConstraintExceptionCollection
                            select conn.SrcEnds.GlobalLayoutConstraintException;

                        foreach (var c in exceptions)
                        {
                            var pcons = ConvertGlobalLayoutConstraintException(c);
                            if (pkg.constraints == null)
                                pkg.constraints = new List<Constraint>();
                            pkg.constraints.Add(pcons);
                        }
                        #endregion

                        #region ExactConstraints
                        // exact constraints related to this part
                        var exactCons =
                            from conn in comp.SrcConnections.ApplyExactLayoutConstraintCollection
                            select conn.SrcEnds.ExactLayoutConstraint;

                        foreach (var c in exactCons)
                        {
                            var pcons = ConvertExactLayoutConstraint(c);
                            if (pkg.constraints == null)
                                pkg.constraints = new List<Constraint>();

                            ConvertExactConstraintFromOrigin(pcons, pkg.width, pkg.height, pkg.originX.Value, pkg.originY.Value);
                            pkg.constraints.Add(pcons);
                        }
                        #endregion

                        #region RangeConstraints
                        // range constraints related to this part
                        var rangeCons =
                            from conn in comp.SrcConnections.ApplyRangeLayoutConstraintCollection
                            select conn.SrcEnds.RangeLayoutConstraint;
                        foreach (var c in rangeCons)
                        {
                            var pcons = ConvertRangeLayoutConstraint(c);
                            if (pcons == null)
                                continue;

                            if (pkg.constraints == null)
                                pkg.constraints = new List<Constraint>();
                            pkg.constraints.Add(pcons);
                        }
                        #endregion

                        #region PreRoutedAssemblyPart
                        // now handle if this component is part of a pre-routed sub-ckt
                        LayoutParser layoutParser;
                        ComponentAssembly preRoutedAsm;
                        if (TryGetPrerouted(comp_obj.Parent, out layoutParser, out preRoutedAsm))
                        {
                            Package prePkg;
                            if (layoutParser.componentPackageMap.TryGetValue(comp_obj, out prePkg))
                            {
                                pkg.x = prePkg.x;
                                pkg.y = prePkg.y;
                                pkg.rotation = prePkg.rotation;
                                pkg.layer = prePkg.layer;
                                pkg.RelComponentID = GetComponentAssemblyID(preRoutedAsm.Impl.Impl as GME.MGA.MgaModel);  // MOT-782
                            }
                            else
                            {
                                // Found a bone, MOT-782.
                                Logger.WriteWarning("Component {0} in Pre-Placed Assembly {1} not found in layout.json. It will be generated in output but not placed...", comp_obj.Name, comp_obj.Parent.Name);
                                pkg.x = 0;
                                pkg.y = 0;
                                pkg.layer = 0;
                                pkg.rotation = 0;
                                myBoneyard.Add(pkg);    // MOT-782
                                bonesFound = true;
                            }
                            // MOT-782: Bones will be placed absolutely, *not* relative to a component assembly.
                            // pkg.RelComponentID = GetComponentAssemblyID(preRoutedAsm.Impl.Impl as GME.MGA.MgaModel);
                            pkg.doNotPlace = true;
                        }
                        #endregion
                    }
                }
                pkgPartMap.Add(pkg, part);  // add to map
                partPkgMap.Add(part, pkg);
                boardLayout.packages.Add(pkg);
            }

            // Set a maximum height for the boneyard.
            if (boardLayout.boardHeight > myBoneyard.maxYardHeight)
            {
                myBoneyard.maxYardHeight = boardLayout.boardHeight;
            }

            myBoneyard.PlaceBones();    // MOT-782.

            #endregion Add Packages to Layout

            #region AddSignalsToBoardLayout
            boardLayout.signals = new List<Signal>();
            foreach (var net in sch_obj.sheets.sheet.FirstOrDefault().nets.net)
            {
                // dump pre-routed signals to board file
                var sig = new Signal();
                sig.name = net.name;
                sig.pins = new List<Pin>();

                // prerouted business
                Signal preRouted = null;
                string preRoutedAsmID = null; // ID of the parent pre-routed assembly
                string preRoutedAsm = null; // name the parent pre-routed assembly
                bool onlyOnePreroute = true;

                // are there segments of this net that are prerouted?
                // determine the corresponding preroute 
                // RESTRICTION - all prerouted segments should belong to the same preRoute signal
                // it is okay to have some segments that are not prerouted - the autorouter should fill those

                Logger.WriteInfo("About to check the segments in Net {0}", net.name);

                foreach (var seg in net.segment.SelectMany(s => s.Items).Where(s => s is Eagle.pinref))
                {
                    bool isSegPrerouted = false;

                    var pr = seg as Eagle.pinref;
                    var pin = new Pin();
                    pin.name = pr.pin;
                    pin.gate = pr.gate;
                    pin.package = pr.part.ToUpper();

                    Logger.WriteInfo("Found a {0} segment for pin {1} of {2} gate {3}", net.name, pin.name, pin.package, pin.gate);

                    // find package and pad
                    var part = sch_obj.parts.part.Where(p => p.name.Equals(pr.part)).FirstOrDefault();

                    var dev = (part != null) ?
                        sch_obj.libraries.library.Where(l => l.name.Equals(part.library)).
                        SelectMany(l => l.devicesets.deviceset).Where(ds => ds.name.Equals(part.deviceset)).
                        SelectMany(ds => ds.devices.device).Where(d => d.name.Equals(part.device)).FirstOrDefault()
                        : null;
                    var pad = (dev != null) ?
                        dev.connects.connect.Where(c => c.gate.Equals(pr.gate) && c.pin.Equals(pr.pin)).
                        Select(c => c.pad).FirstOrDefault()
                        : null;
                    if (pad != null)
                        pin.pad = pad;

                    // check for preroutes 
                    if (part != null && CodeGenerator.partComponentMap.ContainsKey(part))
                    {
                        var comp_obj = CodeGenerator.partComponentMap[part];
                        var preRouteCA = Layout.LayoutGenerator.getAncestorModels((IMgaFCO)comp_obj.Impl.Impl).Where(m => m.Meta.Name == "ComponentAssembly")
                            .Select(ca => new ComponentAssembly(TonkaClasses.ComponentAssembly.Cast(ca)))
                            .Where(ca => CodeGenerator.preRouted.ContainsKey(ca))
                            .FirstOrDefault();
                        if (preRouteCA != null)
                        {
                            isSegPrerouted = true;     // net segment is prerouted - 
                            Logger.WriteInfo("Net {0} segment found in Prerouted Asm {1}", net.name, preRouteCA.Name);

                            var layoutParser = CodeGenerator.preRouted[preRouteCA];
                            // find the name/gate matching port in schematic domain model
                            var sch = comp_obj.Impl.Children.SchematicModelCollection.FirstOrDefault();

                            var port = (sch != null) ?
                                sch.Children.SchematicModelPortCollection.
                                Where(p => p.Attributes.EDAGate == pin.gate && p.Name == pin.name).
                                FirstOrDefault()
                                : null;

                            if (port != null)
                            {
                                Logger.WriteInfo("Port {0} has pin.package: {1} and grandparent name: {2}.", port.Name, pin.package,
                                port.ParentContainer.ParentContainer.Name);
                            }
                            else
                            {
                                Logger.WriteWarning("Nor port matches package: {0} gate: {1} name: {2}.", pin.package, pin.gate, pin.name );
                            }

                            // find the buildPort
                            var buildPort = port != null && CyPhyBuildVisitor.Ports.ContainsKey(port.ID) ?
                                CyPhyBuildVisitor.Ports[port.ID] : null;


                            if (buildPort != null && layoutParser.portTraceMap.ContainsKey(buildPort))
                            {
                                Logger.WriteInfo("Found build Port: {0} with hashCode {1} and it's Associated Trace, from port {2} with ID {3}.", buildPort.Name, buildPort.GetHashCode(), port.Name, port.ID);

                                if (preRouted == null)
                                {
                                    preRouted = layoutParser.portTraceMap[buildPort];
                                    preRoutedAsmID = GetComponentAssemblyID(preRouteCA.Impl.Impl as GME.MGA.MgaModel); // FIXME is this right
                                    preRoutedAsm = preRouteCA.Name;
                                    Logger.WriteInfo("Prerouted Asm Name: {0}, ID {1}",
                                        preRoutedAsm, preRoutedAsmID);
                                }
                                else
                                {
                                    var trace = layoutParser.portTraceMap[buildPort];
                                    if (preRouted != trace)
                                    {
                                        onlyOnePreroute = false;
                                        Logger.WriteWarning("BuildPort: {0}, preRoute = {1} different from a prior port = {2}, port.ID = {3}, buildPort hash code = {4}",
                                            buildPort.Name, preRouted.name, trace.name, port.ID, buildPort.GetHashCode() );
                                    }
                                }
                            }
                        }
                    }

                    Logger.WriteDebug("OnlyOnePreRoute: {0}, isPrerouted: {1}, pin: {2}",
                        onlyOnePreroute, isSegPrerouted, pin.name);

                    // add pin to list of pins for this signal
                    sig.pins.Add(pin);
                }

                // if net has connection to pcb "nets" - polygons - power/ground planes
                if (CodeGenerator.polyNetMap.ContainsValue(net))
                {
                    var port = CodeGenerator.polyNetMap.Where(pn => pn.Value == net).FirstOrDefault().Key;
                    if (port != null)
                    {
                        Logger.WriteInfo("Power/Ground Plane Port: {0}.{1} on net {2}", 
                            port.Parent.Name, port.Name, net.name);
                        // get the polygon corresponding to the PCB port and convert to Json
                        var brd = (port.ComponentParent.SchematicLib != null) ?
                            port.ComponentParent.SchematicLib.drawing.Item as Eagle.board :
                            null;
                        var bsigs = (brd != null) ? brd.signals : null;
                        var bsig = (bsigs != null) ? 
                            bsigs.signal.Where(s => s.name.Equals(port.Name)).FirstOrDefault() :
                            null;
                        if (bsig == null)
                            Logger.WriteError("Power/Ground Plane {0}.{1} not found in associated board resource", port.Parent.Name, port.Name);
                        else
                        {
                            foreach (var bsi in bsig.Items)
                            {
                                if (bsi is Eagle.polygon)
                                {
                                    var bp = bsi as Eagle.polygon;
                                    if (sig.polygons == null) sig.polygons = new List<Polygon>();
                                    var sp = new Polygon();
                                    sp.layer = Convert.ToInt32(bp.layer);
                                    sp.width = Convert.ToDouble(bp.width, CultureInfo.InvariantCulture);
                                    sp.pour = bp.pour.ToString();
                                    sp.rank = Convert.ToInt32(bp.rank);
                                    sp.isolate = Convert.ToDouble(bp.isolate, CultureInfo.InvariantCulture);
                                    sp.thermals = (bp.thermals == Eagle.polygonThermals.yes);

                                    sp.vertices = new List<Vertex>();
                                    foreach (var vx in bp.vertex)
                                    {
                                        var spv = new Vertex();
                                        spv.x = Convert.ToDouble(vx.x, CultureInfo.InvariantCulture);
                                        spv.y = Convert.ToDouble(vx.y, CultureInfo.InvariantCulture);
                                        spv.curve = Convert.ToDouble(vx.curve, CultureInfo.InvariantCulture);
                                        sp.vertices.Add(spv);
                                    }
                                    sig.polygons.Add(sp);
                                }
                            }
                        }
                    }
                }

                // if pre-routed then copy wires 
                if (onlyOnePreroute && preRouted != null)
                {
                    Logger.WriteInfo("Prerouted net {0}, from assembly {1}, originally as {2}", net.name, preRoutedAsm, preRouted.name);
                    sig.wires = new List<Wire>();
                    sig.vias = new List<Via>();
                    sig.polygons = new List<Polygon>();
                    sig.RelComponentID = preRoutedAsmID;
                    if (preRouted.wires != null)
                        foreach (var w in preRouted.wires)
                            sig.wires.Add(w);
                    if (preRouted.vias != null)
                        foreach (var v in preRouted.vias)
                            sig.vias.Add(v);
                    if (preRouted.polygons != null)
                        foreach (var p in preRouted.polygons)
                            sig.polygons.Add(p);
                }
                else
                {
                    Logger.WriteInfo("For net {0}, onlyOnePreroute = {1}, preRouted = {2}", net.name, onlyOnePreroute, preRouted != null);
                }

                boardLayout.signals.Add(sig);
            }
            #endregion

            #region AddRelativeConstraintsToBoardLayout
            // now process relative constraints - they require that all parts be mapped to packages already
            foreach (var pkg in boardLayout.packages)
            {
                if (!pkgPartMap.ContainsKey(pkg))
                    continue;

                var part = pkgPartMap[pkg];
                var comp = CodeGenerator.partComponentMap.ContainsKey(part) ?
                    CodeGenerator.partComponentMap[part] : null;
                var impl = comp != null ? comp.Impl as Tonka.Component : null;

                #region Relative Constraints
                var relCons =
                    from conn in impl.SrcConnections.ApplyRelativeLayoutConstraintCollection
                    select conn.SrcEnds.RelativeLayoutConstraint;

                foreach (var c in relCons)
                {
                    var pcons = new RelativeConstraint();
                    pcons.type = "relative-pkg";
                    try
                    {
                        if (!c.Attributes.XOffset.Equals(""))
                            pcons.x = Convert.ToDouble(c.Attributes.XOffset, CultureInfo.InvariantCulture);
                        if (!c.Attributes.YOffset.Equals(""))
                            pcons.y = Convert.ToDouble(c.Attributes.YOffset, CultureInfo.InvariantCulture);
                    }
                    catch (System.FormatException ex)
                    {
                        Logger.WriteError("Error in Offset attribute of Constraint: {0}, {1}", c.Name, ex.Message);
                    }
                    if (c.Attributes.RelativeLayer != TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.No_Restriction)
                    {
                        if (c.Attributes.RelativeLayer == TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.Same)
                            pcons.layer = 0;
                        else if (c.Attributes.RelativeLayer == TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.Opposite)
                            pcons.layer = 1;
                        else
                            throw new NotSupportedException("RelativeLayer value of " + c.Attributes.RelativeLayer.ToString() + " is not supported");
                    }

                    if (c.Attributes.RelativeRotation != TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum.No_Restriction)
                    {
                        switch (c.Attributes.RelativeRotation)
                        {
                            case TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._0:
                                pcons.relativeRotation = 0;
                                break;
                            case TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._90:
                                pcons.relativeRotation = 1;
                                break;
                            case TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._180:
                                pcons.relativeRotation = 2;
                                break;
                            case TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._270:
                                pcons.relativeRotation = 3;
                                break;
                        }
                    }

                    // find origin comp
                    var origCompImpl = (from conn in c.SrcConnections.RelativeLayoutConstraintOriginCollection
                                        select conn.SrcEnds.Component).FirstOrDefault();
                    var origComp =
                        ((origCompImpl != null) && CyPhyBuildVisitor.Components.ContainsKey(origCompImpl.ID)) ?
                        CyPhyBuildVisitor.Components[origCompImpl.ID] :
                        null;
                    var origPart =
                        ((origComp != null) && CodeGenerator.componentPartMap.ContainsKey(origComp)) ?
                        CodeGenerator.componentPartMap[origComp] :
                        null;
                    var origPkg = ((origPart != null) && partPkgMap.ContainsKey(origPart)) ?
                        partPkgMap[origPart] :
                        null;
                    pcons.pkg_idx = origPkg.pkg_idx.Value;

                    if (origPkg != null)
                    {
                        ConvertRelativeConstraintFromOrigin(pcons, origPkg.width, origPkg.height, origPkg.originX.Value, origPkg.originY.Value, pkg.width, pkg.height, pkg.originX.Value, pkg.originY.Value);
                    }
                    else
                    {
                        ConvertRelativeConstraintFromOrigin(pcons, 0, 0, 0, 0, pkg.width, pkg.height, pkg.originX.Value, pkg.originY.Value);
                    }

                    if (pkg.constraints == null)
                        pkg.constraints = new List<Constraint>();
                    pkg.constraints.Add(pcons);
                }
                #endregion

                #region Relative Range Constraints
                var relRangeCons =
                    from conn in impl.SrcConnections.ApplyRelativeRangeLayoutConstraintCollection
                    select conn.SrcEnds.RelativeRangeConstraint;

                foreach (var c in relRangeCons)
                {
                    var cons = ConvertRelativeRangeConstraint(c);
                    if (pkg.constraints == null)
                        pkg.constraints = new List<Constraint>();
                    pkg.constraints.Add(cons);
                }
                #endregion
            }
            #endregion

            #region Apply relative constraints to prerouted packages
            foreach (var assemblyPackagePair in preroutedPkgMap)
            {
                var componentAssembly = assemblyPackagePair.Key;
                var componentAssemblyImpl = componentAssembly.Impl;

                var pkg = assemblyPackagePair.Value;

                #region Relative Constraints
                var relCons =
                    from conn in componentAssemblyImpl.SrcConnections.ApplyRelativeLayoutConstraintCollection
                    select conn.SrcEnds.RelativeLayoutConstraint;

                foreach (var c in relCons)
                {
                    var pcons = new RelativeConstraint();
                    pcons.type = "relative-pkg";
                    try
                    {
                        if (!c.Attributes.XOffset.Equals(""))
                            pcons.x = Convert.ToDouble(c.Attributes.XOffset, CultureInfo.InvariantCulture);
                        if (!c.Attributes.YOffset.Equals(""))
                            pcons.y = Convert.ToDouble(c.Attributes.YOffset, CultureInfo.InvariantCulture);
                    }
                    catch (System.FormatException ex)
                    {
                        Logger.WriteError("Error in Offset attribute of Constraint: {0}, {1}", c.Name, ex.Message);
                    }
                    if (c.Attributes.RelativeLayer != TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.No_Restriction)
                    {
                        if (c.Attributes.RelativeLayer == TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.Same)
                            pcons.layer = 0;
                        else if (c.Attributes.RelativeLayer == TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.Opposite)
                            pcons.layer = 1;
                        else
                            throw new NotSupportedException("RelativeLayer value of " + c.Attributes.RelativeLayer.ToString() + " is not supported");
                    }

                    if (c.Attributes.RelativeRotation != TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum.No_Restriction)
                    {
                        switch (c.Attributes.RelativeRotation)
                        {
                            case TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._0:
                                pcons.relativeRotation = 0;
                                break;
                            case TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._90:
                                pcons.relativeRotation = 1;
                                break;
                            case TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._180:
                                pcons.relativeRotation = 2;
                                break;
                            case TonkaClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._270:
                                pcons.relativeRotation = 3;
                                break;
                        }
                    }

                    // find origin comp
                    var origCompImpl = (from conn in c.SrcConnections.RelativeLayoutConstraintOriginCollection
                                        select conn.SrcEnds.Component).FirstOrDefault();
                    var origComp =
                        ((origCompImpl != null) && CyPhyBuildVisitor.Components.ContainsKey(origCompImpl.ID)) ?
                        CyPhyBuildVisitor.Components[origCompImpl.ID] :
                        null;
                    var origPart =
                        ((origComp != null) && CodeGenerator.componentPartMap.ContainsKey(origComp)) ?
                        CodeGenerator.componentPartMap[origComp] :
                        null;
                    var origPkg = ((origPart != null) && partPkgMap.ContainsKey(origPart)) ?
                        partPkgMap[origPart] :
                        null;
                    pcons.pkg_idx = origPkg.pkg_idx.Value;

                    if (origPkg != null)
                    {
                        //The space claims for pre-routed subassemblies don't seem to have an originX/originY value; we'll default
                        //it to 0,0 (the center of the package) while respecting it if present (in case something changes later)
                        var destOriginX = pkg.originX.GetValueOrDefault(0.0);
                        var destOriginY = pkg.originY.GetValueOrDefault(0.0);
                        ConvertRelativeConstraintFromOrigin(pcons, origPkg.width, origPkg.height, origPkg.originX.Value, origPkg.originY.Value, pkg.width, pkg.height, destOriginX, destOriginY);
                    }
                    else
                    {
                        var destOriginX = pkg.originX.GetValueOrDefault(0.0);
                        var destOriginY = pkg.originY.GetValueOrDefault(0.0);
                        ConvertRelativeConstraintFromOrigin(pcons, 0, 0, 0, 0, pkg.width, pkg.height, destOriginX, destOriginY);
                    }

                    if (pkg.constraints == null)
                        pkg.constraints = new List<Constraint>();
                    pkg.constraints.Add(pcons);
                }
                #endregion

                #region Relative Range Constraints
                var relRangeCons =
                    from conn in componentAssemblyImpl.SrcConnections.ApplyRelativeRangeLayoutConstraintCollection
                    select conn.SrcEnds.RelativeRangeConstraint;

                foreach (var c in relRangeCons)
                {
                    var cons = ConvertRelativeRangeConstraint(c);
                    if (pkg.constraints == null)
                        pkg.constraints = new List<Constraint>();
                    pkg.constraints.Add(cons);
                }
                #endregion
            }


            #endregion

            #region Check Parent Containers for Constraints
            // now process relative constraints - they require that all parts be mapped to packages already
            foreach (var pkg in boardLayout.packages)
            {
                if (!pkgPartMap.ContainsKey(pkg))
                    continue;

                var part = pkgPartMap[pkg];
                var comp = CodeGenerator.partComponentMap.ContainsKey(part) ?
                    CodeGenerator.partComponentMap[part] : null;

                var parentConstraints = GetConstraintsOfParents(comp);
                if (parentConstraints != null)
                {
                    if (pkg.constraints == null)
                        pkg.constraints = new List<Constraint>();

                    foreach (Constraint parentConstraint in parentConstraints)
                    {
                        if (parentConstraint is ExactConstraint)
                        {
                            var exactConstraint = (ExactConstraint)parentConstraint;
                            ConvertExactConstraintFromOrigin(exactConstraint, pkg.width, pkg.height, pkg.originX.Value, pkg.originY.Value);
                        }
                    }

                    pkg.constraints.AddRange(parentConstraints);
                }
            }
            #endregion

            #region Add Template Border and Vias to Layout
            if (!String.IsNullOrWhiteSpace(boardLayout.boardTemplate))
            {
                boardLayout.border = new List<Border>();
                string templateFile = Path.Combine(pathOutput, boardLayout.boardTemplate);
                if (!File.Exists(templateFile))
                {
                    // TODO: This should return null to kill test bench.
                    Logger.WriteError(String.Format("boardTemplate not found! File searched for: {0}", templateFile));
                    return;
                }
                Eagle.eagle eagle = Eagle.eagle.LoadFromFile(templateFile);

                var plain = (eagle.drawing.Item as Eagle.board).plain;
                foreach (var border_seg in plain.Items)
                {
                    Border border_segment = new Border();
                    if (String.Compare(border_seg.GetType().Name, "wire") == 0)
                    {
                        border_segment.wire = new LayoutJson.Wire()
                        {
                            x1 = 0,
                            x2 = 0,
                            y1 = 0,
                            y2 = 0,
                            layer = 0,
                            width = 0,
                            curve = 0
                        };
                        Eagle.wire wire_seg = (Eagle.wire)border_seg;
                        
                        border_segment.wire.x1 = Convert.ToDouble(wire_seg.x1);
                        border_segment.wire.x2 = Convert.ToDouble(wire_seg.x2);
                        border_segment.wire.y1 = Convert.ToDouble(wire_seg.y1);
                        border_segment.wire.y2 = Convert.ToDouble(wire_seg.y2);
                        border_segment.wire.width = Convert.ToDouble(wire_seg.width);
                        border_segment.wire.layer = Convert.ToInt32(wire_seg.layer);
                        border_segment.wire.curve = Convert.ToDouble(wire_seg.curve);
                        boardLayout.border.Add(border_segment);
                    }
                    else if (String.Compare(border_seg.GetType().Name, "circle") == 0)
                    {
                        border_segment.circle = new LayoutJson.Circle()
                        {
                            x = 0,
                            y = 0,
                            radius = 0,
                            width = 0,
                            layer = 0
                        };
                        Eagle.circle circle_seg = (Eagle.circle)border_seg;
                        border_segment.circle.x = Convert.ToDouble(circle_seg.x);
                        border_segment.circle.y = Convert.ToDouble(circle_seg.y);
                        border_segment.circle.radius = Convert.ToDouble(circle_seg.radius);
                        border_segment.circle.width = Convert.ToDouble(circle_seg.width);
                        border_segment.circle.layer = Convert.ToInt32(circle_seg.layer);
                        boardLayout.border.Add(border_segment);
                    }
                }
            }
            #endregion
        }

        public static string GetComponentID(Tonka.Component comp)
        {
            var parent = comp.ParentContainer;
            string ComponentID = "";
            string preLayoutId = "";
            string parentId = "";
            var ancestors = getAncestorModels((MgaFCO)comp.Impl).Where(parentModel => parentModel.MetaBase.Name == typeof(Tonka.ComponentAssembly).Name).ToList();
            foreach (var parentModel in ancestors)
            {
                var caModel = TonkaClasses.ComponentAssembly.Cast(parentModel);
                if (((MgaModel)caModel.Impl).RegistryValue["layoutFile"] != null) //(preroutedPkgMap.ContainsKey(caModel))
                {
                    parentId = GetComponentAssemblyChainGuid((IMgaModel)caModel.Impl) ?? "";
                    if (parentModel == ancestors.Last())
                    {
                        // layout.json CA is the root; safe to assume InstanceGUID is unique
                        // this makes layout.json backwards-compatible, and alleviates some ID challenges
                        break;
                    }

                    preLayoutId = GetComponentAssemblyManagedGuid((IMgaModel)parentModel);
                    if (preLayoutId == "")
                    {
                        preLayoutId = GetComponentAssemblyChainGuid((IMgaModel)parentModel);
                    }
                    break;
                }
            }

            ComponentID = comp.Attributes.InstanceGUID;
            if (ComponentID.StartsWith(parentId))
            {
                ComponentID = ComponentID.Substring(parentId.Length);
                //pkg.ComponentID = preLayoutId + pkg.ComponentID;
            }
            ComponentID = preLayoutId + ComponentID;
            return ComponentID;
        }

        /// <summary>
        /// Given a component assembly checks if IT or any ancestor is prerouted (i.e. contains a parsed layout)
        /// </summary>
        /// <param name="comp_asm">The Assembly to be checked</param>
        /// <param name="parser">Output parameter that returns the layout parser object</param>
        /// <returns></returns>
        bool TryGetPrerouted(ComponentAssembly comp_asm, out LayoutParser parser, out ComponentAssembly preRoutedAsm)
        {
            preRoutedAsm = comp_asm;
            if (CodeGenerator.preRouted.TryGetValue(comp_asm, out parser))
            {
                return true;
            }
            else if (comp_asm.Parent != null)
                return TryGetPrerouted(comp_asm.Parent, out parser, out preRoutedAsm);

            return false;
        }

        /// <summary>
        /// Given a constrained package and a CyPhy RelativeRangeConstraint, generate a constraint object for the pkg.
        /// </summary>
        /// <param name="c">The CyPhy RelativeRangeConstraint object</param>
        /// <returns></returns>
        private RelativeRangeConstraint ConvertRelativeRangeConstraint(Tonka.RelativeRangeConstraint c)
        {
            var pcons = new RelativeRangeConstraint()
            {
                type = "relative-region"
            };

            if (!String.IsNullOrWhiteSpace(c.Attributes.XOffsetRange))
                pcons.x = c.Attributes.XOffsetRange;
            if (!String.IsNullOrWhiteSpace(c.Attributes.YOffsetRange))
                pcons.y = c.Attributes.YOffsetRange;
            if (c.Attributes.RelativeLayer != TonkaClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.No_Restriction)
            {
                if (c.Attributes.RelativeLayer == TonkaClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.Same)
                    pcons.layer = 0;
                else if (c.Attributes.RelativeLayer == TonkaClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.Opposite)
                    pcons.layer = 1;
                else
                    throw new NotSupportedException("RelativeLayer value of " + c.Attributes.RelativeLayer.ToString() + " is not supported");
            }

            // find origin comp
            var origCompImpl = (from conn in c.SrcConnections.RelativeRangeLayoutConstraintOriginCollection
                                select conn.SrcEnds.Component).FirstOrDefault();
            var origComp =
                ((origCompImpl != null) && CyPhyBuildVisitor.Components.ContainsKey(origCompImpl.ID)) ?
                CyPhyBuildVisitor.Components[origCompImpl.ID] :
                null;
            var origPart =
                ((origComp != null) && CodeGenerator.componentPartMap.ContainsKey(origComp)) ?
                CodeGenerator.componentPartMap[origComp] :
                null;
            var origPkg = ((origPart != null) && partPkgMap.ContainsKey(origPart)) ?
                partPkgMap[origPart] :
                null;
            pcons.pkg_idx = origPkg.pkg_idx.Value;

            return pcons;
        }

        /**
         * Converts exact layout constraint from being positioned relative to the package's origin (the
         * form they use in GME) to being relative to lower left corner of the inner bounding box
         * (the form they need to be in for the layout solver)
         */
        private void ConvertExactConstraintFromOrigin(ExactConstraint constraint, 
            double packageWidth, double packageHeight,
            double originX, double originY)
        {
            int rotation = constraint.rotation != null ? constraint.rotation.Value : 0;
            int layer = constraint.layer != null ? constraint.layer.Value : 0;  
            int mrot = (layer == 1) ? rotation + 2 : rotation;  // bottom layer needs mirroring
            mrot = mrot % 4;

            switch(mrot) {
                case 0:
                    if (constraint.x.HasValue) //constraint.x/constraint.y might not exist (they're nullable)--  if they don't, keep them null
                    {
                        constraint.x = 0.1 * Math.Ceiling(10.0 * (constraint.x.Value - packageWidth / 2.0)) + originX;
                    }
                    if (constraint.y.HasValue)
                    {
                        constraint.y = 0.1 * Math.Ceiling(10.0 * (constraint.y.Value - packageHeight / 2.0)) + originY;
                    }
                    break;
                case 2: //180 degrees clockwise
                    if (constraint.x.HasValue)
                    {
                        constraint.x = 0.1 * Math.Ceiling(10.0 * (constraint.x.Value - packageWidth / 2.0)) - originX;
                    }
                    if (constraint.y.HasValue)
                    {
                        constraint.y = 0.1 * Math.Ceiling(10.0 * (constraint.y.Value - packageHeight / 2.0)) - originY;
                    }
                    break;
                case 1: //90 degrees counterclockwise
                    if (constraint.x.HasValue)
                    {
                        constraint.x = 0.1 * Math.Ceiling(10.0 * (constraint.x.Value - packageHeight / 2.0)) - originY;
                    }
                    if (constraint.y.HasValue)
                    {
                        constraint.y = 0.1 * Math.Ceiling(10.0 * (constraint.y.Value - packageWidth / 2.0)) + originX;
                    }
                    break;
                case 3: //270 degrees counterclockwise
                    if (constraint.x.HasValue)
                    {
                        constraint.x = 0.1 * Math.Ceiling(10.0 * (constraint.x.Value - packageHeight / 2.0)) + originY;
                    }
                    if (constraint.y.HasValue)
                    {
                        constraint.y = 0.1 * Math.Ceiling(10.0 * (constraint.y.Value - packageWidth / 2.0)) - originX;
                    }
                    break;
            }
        }

        private void ConvertRelativeConstraintFromOrigin(RelativeConstraint constraint,
            double originPackageWidth,
            double originPackageHeight,
            double originPackageOriginX,
            double originPackageOriginY,
            double targetPackageWidth,
            double targetPackageHeight,
            double targetPackageOriginX,
            double targetPackageOriginY)
        {
            if (constraint.x.HasValue) //constraint.x/constraint.y might not exist (they're nullable)--  if they don't, keep them null
            {
                double constraintX = constraint.x.Value;
                //No rotation
                constraint.x  = (0.1) * Math.Ceiling(10.0 * (constraintX + (originPackageWidth / 2.0 - targetPackageWidth / 2.0) - (originPackageOriginX - targetPackageOriginX)));
                //90 degrees CCW
                constraint.x1 = (0.1) * Math.Ceiling(10.0 * (constraintX + (originPackageWidth / 2.0 - targetPackageHeight / 2.0) - (originPackageOriginX + targetPackageOriginY)));
                //180 degrees CCW
                constraint.x2 = (0.1) * Math.Ceiling(10.0 * (constraintX + (originPackageWidth / 2.0 - targetPackageWidth / 2.0) - (originPackageOriginX + targetPackageOriginX)));
                //270 degrees CCW
                constraint.x3 = (0.1) * Math.Ceiling(10.0 * (constraintX + (originPackageWidth / 2.0 - targetPackageHeight / 2.0) - (originPackageOriginX - targetPackageOriginY)));
            }
            if (constraint.y.HasValue)
            {
                double constraintY = constraint.y.Value;
                //No rotation
                constraint.y = 0.1 * Math.Ceiling(10.0 * (constraintY + (originPackageHeight / 2.0 - targetPackageHeight / 2.0) - (originPackageOriginY - targetPackageOriginY)));
                //90 degrees CCW
                constraint.y1 = 0.1 * Math.Ceiling(10.0 * (constraintY + (originPackageHeight / 2.0 - targetPackageWidth / 2.0) - (originPackageOriginY - targetPackageOriginX)));
                //180 degrees CCW
                constraint.y2 = 0.1 * Math.Ceiling(10.0 * (constraintY + (originPackageHeight / 2.0 - targetPackageHeight / 2.0) - (originPackageOriginY + targetPackageOriginY)));
                //270 degrees CCW
                constraint.y3 = 0.1 * Math.Ceiling(10.0 * (constraintY + (originPackageHeight / 2.0 - targetPackageWidth / 2.0) - (originPackageOriginY + targetPackageOriginX)));
            }
        }

        private void GetLayoutParametersFromTestBench( TestBench tb_obj, LayoutJson.Layout boardLayout,
            out string boardWidth,
            out string boardHeight,
            out string boardEdgeSpace,  // MOT-789
            out string interChipSpace,
            out string boardCutouts)
        {
            var bt = tb_obj.Parameters.FirstOrDefault(p => p.Name.Equals("boardTemplate"));
            if (bt != null)
            {
                try
                {
                    boardLayout.boardTemplate = Path.GetFileName(bt.Value);
                }
                catch (System.ArgumentException ex)
                {
                    this.CodeGenerator.Logger.WriteError("Error extracting boardTemplate filename: {0}", ex.Message);
                }
            }

            var dr = tb_obj.Parameters.FirstOrDefault(p => p.Name.Equals("designRules"));
            if (dr != null)
            {
                try
                {
                    boardLayout.designRules = Path.GetFileName(dr.Value);
                }
                catch (System.ArgumentException ex)
                {
                    CodeGenerator.Logger.WriteError("Error extracting designRules filename: {0}", ex.Message);
                }
            }

            var bw = tb_obj.Parameters.FirstOrDefault(p => p.Name.Equals("boardWidth"));
            var bh = tb_obj.Parameters.FirstOrDefault(p => p.Name.Equals("boardHeight"));
            var bes = tb_obj.Parameters.FirstOrDefault(p => p.Name.Equals("boardEdgeSpace"));
            var ics = tb_obj.Parameters.FirstOrDefault(p => p.Name.Equals("interChipSpace"));
            var bc = tb_obj.Parameters.FirstOrDefault(p => p.Name.Equals("boardCutouts"));
            
            boardWidth = (bw != null) ? bw.Value : null;
            boardHeight = (bh != null) ? bh.Value : null;
            boardEdgeSpace = (bes != null) ? bes.Value : null;
            interChipSpace = (ics != null) ? ics.Value : null;
            boardCutouts = (bc != null) ? bc.Value : null;

        }

  
        private void GetLayoutParametersFromPcbComponent(TestBench tb_obj, LayoutJson.Layout boardLayout,
            out string boardWidth, out string boardHeight, out string boardEdgeSpace, out string interChipSpace, out string boardCutouts)
        {
            // Look to see if there is a PCB component in the top level assembly 
            var compImpl = tb_obj.ComponentAssemblies.SelectMany(c => c.ComponentInstances)
                                                     .Select(i => i.Impl)
                                                     .FirstOrDefault(j => (j as Tonka.Component).Attributes
                                                                                                .Classifications
                                                                                                .Contains("pcb_board")
                                                                       || (j as Tonka.Component).Attributes
                                                                                                .Classifications
                                                                                                .Contains("template.pcb_template"));

            if (compImpl == null)
            {
                boardWidth = null;
                boardHeight = null;
                boardEdgeSpace = null;
                interChipSpace = null;
                boardCutouts = null;
                return;
            }
            
            var comp = compImpl as Tonka.Component;

            boardLayout.boardTemplate = GetResource(comp, "boardTemplate");
            boardLayout.designRules = GetResource(comp, "designRules");
            boardWidth = GetParameterValue(comp, "boardWidth");
            boardHeight = GetParameterValue(comp, "boardHeight");
            boardEdgeSpace = GetParameterValue(comp, "boardEdgeSpace");
            interChipSpace = GetParameterValue(comp, "interChipSpace");
            boardCutouts = GetParameterValue(comp, "boardCutouts");
        }

        private string GetResource(Tonka.Component comp, string resName)
        {
            string resFile = null;
            var res = (comp != null) ? comp.Children.ResourceCollection.FirstOrDefault(r =>
                                            r.Name.ToUpper().Contains(resName.ToUpper()))
                                            : null;            
            if (res != null)
            {
                // This file is being copied into the results folder elsewhere.
                // Here, we just need the filename so that we can find the copied
                // file in the output directory.
                resFile = Path.GetFileName(res.Attributes.Path);
            }
            return resFile;
        }

        private string GetParameterValue(Tonka.Component comp, string paramName)
        {
            String val = null;

            // Check for a Parameter with this value
            var par = comp.Children.ParameterCollection.FirstOrDefault(p => p.Name.Equals(paramName));
            if (par != null)
            {
                val = par.Attributes.Value;
            }
            else
            {
                // If null, check for a Property with this name            
                var prop = comp.Children.PropertyCollection.FirstOrDefault(p => p.Name.Equals(paramName));
                if (prop != null)
                {
                    val = prop.Attributes.Value;
                }
            }

            return val;
        }

        private void GenerateBoardCutoutConstraints(string boardCutouts)
        {
            boardLayout.omitBoundary = true;
            // create exclusion constraints for cutouts
            if (boardLayout.constraints == null)
                boardLayout.constraints = new List<Constraint>();
            var cutouts = boardCutouts.Split(';');
            foreach (var cutout in cutouts)
            {
                var pts = cutout.Split(',');
                if (pts.Length >= 4)
                {
                    // Create constraints for both top and bottom layers
                    RangeConstraint rcTop = new RangeConstraint()
                    {
                        type = "ex-region",
                        layer = "0"
                    };
                    RangeConstraint rcBottom = new RangeConstraint()
                    {
                        type = "ex-region",
                        layer = "1"
                    };

                    double d = Double.NaN;
                    var cbox = new BoundingBox();
                    if (Double.TryParse(pts[0], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                        cbox.MinX = d;
                    if (Double.TryParse(pts[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                        cbox.MinY = d;
                    if (Double.TryParse(pts[2], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                        cbox.MaxX = cbox.MinX + d;
                    if (Double.TryParse(pts[3], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                        cbox.MaxY = cbox.MinY + d;
                    rcBottom.x = rcTop.x = String.Format(CultureInfo.InvariantCulture, "{0}:{1}", cbox.MinX, cbox.MaxX);
                    rcBottom.y = rcTop.y = String.Format(CultureInfo.InvariantCulture, "{0}:{1}", cbox.MinY, cbox.MaxY);
                    boardLayout.constraints.Add(rcTop);
                    boardLayout.constraints.Add(rcBottom);
                    Logger.WriteInfo("Adding board cutout constraint: X {0}:{1}, Y {2}:{3}", 
                                     cbox.MinX, 
                                     cbox.MaxX,
                                     cbox.MinY, 
                                     cbox.MaxY);
                }
            }
        }

        public static BoundingBox ComputeBoundingBox(Eagle.package spkg, string[] includedLayers)
        {
            /**
             * The XSD element for an Eagle package, repeated here for convenience:
             * <xsd:element name='package'>
             *  <xsd:complexType>
             *   <xsd:sequence>
             *    <xsd:element ref='eagle:description' minOccurs='0' maxOccurs='1'/>
             *    <xsd:choice minOccurs='0' maxOccurs='unbounded'>
             *     <xsd:element ref='eagle:polygon'/>
             *     <xsd:element ref='eagle:wire'/>
             *     <xsd:element ref='eagle:text'/>
             *     <xsd:element ref='eagle:dimension'/>
             *     <xsd:element ref='eagle:circle'/>
             *     <xsd:element ref='eagle:rectangle'/>
             *     <xsd:element ref='eagle:frame'/>
             *     <xsd:element ref='eagle:hole'/>
             *     <xsd:element ref='eagle:pad'/>
             *     <xsd:element ref='eagle:smd'/>
             *    </xsd:choice>
             *   </xsd:sequence>
             *   <xsd:attribute name='name' type='xsd:string' use='required'/>
             *  </xsd:complexType>
             * </xsd:element>
             */
            BoundingBox boundingBox = new BoundingBox();
            boundingBox.MinX = Double.MaxValue;
            boundingBox.MinY = Double.MaxValue;
            boundingBox.MaxX = Double.MinValue;
            boundingBox.MaxY = Double.MinValue;

            foreach (Eagle.wire wire in spkg.Items.Where(s => s is Eagle.wire))
            {
                if (wire == null) continue;

                if (!includedLayers.Any(wire.layer.Trim().Equals)) //Was layer.Contains; would cause incorrect behavior for single-digit layer numbers in includedLayers
                    continue;

                var x1 = Convert.ToDouble(wire.x1, CultureInfo.InvariantCulture);
                var x2 = Convert.ToDouble(wire.x2, CultureInfo.InvariantCulture);
                var y1 = Convert.ToDouble(wire.y1, CultureInfo.InvariantCulture);
                var y2 = Convert.ToDouble(wire.y2, CultureInfo.InvariantCulture);
                if (x1 < boundingBox.MinX) boundingBox.MinX = x1;
                if (x2 < boundingBox.MinX) boundingBox.MinX = x2;
                if (x1 > boundingBox.MaxX) boundingBox.MaxX = x1;
                if (x2 > boundingBox.MaxX) boundingBox.MaxX = x2;
                if (y1 < boundingBox.MinY) boundingBox.MinY = y1;
                if (y2 < boundingBox.MinY) boundingBox.MinY = y2;
                if (y1 > boundingBox.MaxY) boundingBox.MaxY = y1;
                if (y2 > boundingBox.MaxY) boundingBox.MaxY = y2;
            }
            foreach (Eagle.pad pad in spkg.Items.Where(s => s is Eagle.pad))
            {
                if (pad == null) continue;

                var pad_num = pad.name;
                var x = Convert.ToDouble(pad.x, CultureInfo.InvariantCulture);
                var y = Convert.ToDouble(pad.y, CultureInfo.InvariantCulture);
                var drill = Convert.ToDouble(pad.drill, CultureInfo.InvariantCulture);
                var dia = Convert.ToDouble(pad.diameter, CultureInfo.InvariantCulture);
                var shape = pad.shape;  // enum padShape {round, octagon, @long, offset}
                var rot = pad.rot;      // { R90, R180, R270, ...}  
                var r = 0.0;
                if (dia == 0.0)  // JS: Workaround for no diameter present, estimate dia to be 2x drill size 
                {
                    dia = drill * 2.0;
                }

                if (shape == Eagle.padShape.@long)
                {
                    // TODO: consider PAD rotation; for now, consider long pads are 2x diameter.

                    dia *= 2.0;  // diameter value from package desc is for the "short" side, for max calc, consider long side is 2x (by inspection)
                }
                r = dia / 2.0;

                if ((x - r) < boundingBox.MinX) boundingBox.MinX = x - r;
                if ((x + r) > boundingBox.MaxX) boundingBox.MaxX = x + r;
                if ((y - r) < boundingBox.MinY) boundingBox.MinY = y - r;
                if ((y + r) > boundingBox.MaxY) boundingBox.MaxY = y + r;
            }
            foreach (Eagle.circle circle in spkg.Items.Where(s => s is Eagle.circle))
            {
                if (circle == null) continue;

                if (!includedLayers.Any(circle.layer.Trim().Equals))
                    continue;

                var x = Convert.ToDouble(circle.x, CultureInfo.InvariantCulture);
                var y = Convert.ToDouble(circle.y, CultureInfo.InvariantCulture);
                var r = Convert.ToDouble(circle.radius, CultureInfo.InvariantCulture);
                if ((x - r) < boundingBox.MinX) boundingBox.MinX = x - r;
                if ((x + r) > boundingBox.MaxX) boundingBox.MaxX = x + r;
                if ((y - r) < boundingBox.MinY) boundingBox.MinY = y - r;
                if ((y + r) > boundingBox.MaxY) boundingBox.MaxY = y + r;
            }
            foreach (Eagle.smd smd in spkg.Items.Where(s => s is Eagle.smd))
            {
                if (smd == null) continue;
                // SKN: after intense research on eagle, it seems that the way SMD pads are placed is as follows
                // dx - dy tells the length of the pad, while the x is the center point of the SMD pad

                //TODO: do we want to take into account which layer this is in?

                var x = Convert.ToDouble(smd.x, CultureInfo.InvariantCulture);
                var y = Convert.ToDouble(smd.y, CultureInfo.InvariantCulture);
                var ddx = Convert.ToDouble(smd.dx, CultureInfo.InvariantCulture);
                var ddy = Convert.ToDouble(smd.dy, CultureInfo.InvariantCulture);
                var dx = ddx;  // should be /2??
                var dy = ddy;
                if (smd.rot.Equals("R90") || smd.rot.Equals("R270"))
                {       // flip dx and dy if there is rotation
                    dx = ddy;
                    dy = ddx;
                }
                var x1 = x - dx / 2.0;
                var x2 = x + dx / 2.0;
                var y1 = y - dy / 2.0;
                var y2 = y + dy / 2.0;
                if (x1 < boundingBox.MinX) boundingBox.MinX = x1;
                if (x2 > boundingBox.MaxX) boundingBox.MaxX = x2;
                if (y1 < boundingBox.MinY) boundingBox.MinY = y1;
                if (y2 > boundingBox.MaxY) boundingBox.MaxY = y2;
            }
            foreach (Eagle.polygon polygon in spkg.Items.Where(s => s is Eagle.polygon))
            {
                if (polygon == null) continue;

                if (!includedLayers.Any(polygon.layer.Trim().Equals))
                    continue;

                //Calculate bounding box of polygon
                var minimumPolygonX = polygon.vertex.Min(v => Convert.ToDouble(v.x, CultureInfo.InvariantCulture));
                var minimumPolygonY = polygon.vertex.Min(v => Convert.ToDouble(v.y, CultureInfo.InvariantCulture));

                var maximumPolygonX = polygon.vertex.Max(v => Convert.ToDouble(v.x, CultureInfo.InvariantCulture));
                var maximumPolygonY = polygon.vertex.Max(v => Convert.ToDouble(v.y, CultureInfo.InvariantCulture));

                //Extend bounding box if any part of polygon's bounding box is smaller
                if (minimumPolygonX < boundingBox.MinX) boundingBox.MinX = minimumPolygonX;
                if (maximumPolygonX > boundingBox.MaxX) boundingBox.MaxX = maximumPolygonX;
                if (minimumPolygonY < boundingBox.MinY) boundingBox.MinY = minimumPolygonY;
                if (maximumPolygonY > boundingBox.MaxY) boundingBox.MaxY = maximumPolygonY;
            }
            foreach (Eagle.rectangle rectangle in spkg.Items.Where(s => s is Eagle.rectangle))
            {
                if (rectangle == null) continue;

                if (!includedLayers.Any(rectangle.layer.Trim().Equals))
                    continue;

                var x1 = Convert.ToDouble(rectangle.x1, CultureInfo.InvariantCulture);
                var y1 = Convert.ToDouble(rectangle.y1, CultureInfo.InvariantCulture);
                var x2 = Convert.ToDouble(rectangle.x2, CultureInfo.InvariantCulture);
                var y2 = Convert.ToDouble(rectangle.y2, CultureInfo.InvariantCulture);

                //TODO: Consider rotation?  What does rotation mean in this context?  (Rectangles' coordinates
                //      are absolute coordinates with respect to the origin; does rotation flip them?)

                //Figure out minimum/maximum coordinates (just in case the coordinates aren't in the order we expect)
                double minimumRectangleX, minimumRectangleY, maximumRectangleX, maximumRectangleY;
                if (x1 <= x2)
                {
                    minimumRectangleX = x1;
                    maximumRectangleX = x2;
                }
                else
                {
                    minimumRectangleX = x2;
                    maximumRectangleX = x1;
                }

                if (y1 <= y2)
                {
                    minimumRectangleY = y1;
                    maximumRectangleY = y2;
                }
                else
                {
                    minimumRectangleY = y2;
                    maximumRectangleY = y1;
                }

                if (minimumRectangleX < boundingBox.MinX) boundingBox.MinX = minimumRectangleX;
                if (maximumRectangleX > boundingBox.MaxX) boundingBox.MaxX = maximumRectangleX;
                if (minimumRectangleY < boundingBox.MinY) boundingBox.MinY = minimumRectangleY;
                if (maximumRectangleY > boundingBox.MaxY) boundingBox.MaxY = maximumRectangleY;
            } 
            foreach (Eagle.frame frame in spkg.Items.Where(s => s is Eagle.frame))
            {
                if (frame == null) continue;

                if (!includedLayers.Any(frame.layer.Trim().Equals))
                    continue;

                //Assumption: the coordinates of a frame are its outer corners
                var x1 = Convert.ToDouble(frame.x1, CultureInfo.InvariantCulture);
                var y1 = Convert.ToDouble(frame.y1, CultureInfo.InvariantCulture);
                var x2 = Convert.ToDouble(frame.x2, CultureInfo.InvariantCulture);
                var y2 = Convert.ToDouble(frame.y2, CultureInfo.InvariantCulture);

                //TODO: Consider rotation?  What does rotation mean in this context?  (Frames' coordinates
                //      are absolute coordinates with respect to the origin; does rotation flip them?)

                //Figure out minimum/maximum coordinates (just in case the coordinates aren't in the order we expect)
                double minimumFrameX, minimumFrameY, maximumFrameX, maximumFrameY;
                if (x1 <= x2)
                {
                    minimumFrameX = x1;
                    maximumFrameX = x2;
                }
                else
                {
                    minimumFrameX = x2;
                    maximumFrameX = x1;
                }

                if (y1 <= y2)
                {
                    minimumFrameY = y1;
                    maximumFrameY = y2;
                }
                else
                {
                    minimumFrameY = y2;
                    maximumFrameY = y1;
                }

                if (minimumFrameX < boundingBox.MinX) boundingBox.MinX = minimumFrameX;
                if (maximumFrameX > boundingBox.MaxX) boundingBox.MaxX = maximumFrameX;
                if (minimumFrameY < boundingBox.MinY) boundingBox.MinY = minimumFrameY;
                if (maximumFrameY > boundingBox.MaxY) boundingBox.MaxY = maximumFrameY;
            }
            foreach (Eagle.hole hole in spkg.Items.Where(s => s is Eagle.hole))
            {
                if (hole == null) continue;

                var x = Convert.ToDouble(hole.x, CultureInfo.InvariantCulture);
                var y = Convert.ToDouble(hole.y, CultureInfo.InvariantCulture);
                //eagle documentation indicates that hole.drill is the hole diameter
                //do we want more clearance around holes?
                var dia = Convert.ToDouble(hole.drill, CultureInfo.InvariantCulture);

                var r = dia / 2.0;

                if ((x - r) < boundingBox.MinX) boundingBox.MinX = x - r;
                if ((x + r) > boundingBox.MaxX) boundingBox.MaxX = x + r;
                if ((y - r) < boundingBox.MinY) boundingBox.MinY = y - r;
                if ((y + r) > boundingBox.MaxY) boundingBox.MaxY = y + r;
            }

            return boundingBox;
        }

        private RangeConstraint ConvertRangeLayoutConstraint(Tonka.RangeLayoutConstraint c)
        {
            var pcons = new RangeConstraint();
            
            var xRangeProvided = !String.IsNullOrWhiteSpace(c.Attributes.XRange);
            var yRangeProvided = !String.IsNullOrWhiteSpace(c.Attributes.YRange);
            var layerProvided = !String.IsNullOrWhiteSpace(c.Attributes.LayerRange);

            var rangeType = c.Attributes.Type;

            if (rangeType == TonkaClasses.RangeLayoutConstraint.AttributesClass.Type_enum.Inclusion
                && xRangeProvided
                && yRangeProvided
                && layerProvided)
            {
                pcons.type = "in-region";
            }
            else if (rangeType == TonkaClasses.RangeLayoutConstraint.AttributesClass.Type_enum.Exclusion)
            {
                if (xRangeProvided && yRangeProvided && layerProvided)
                {
                    pcons.type = "ex-region";
                }
                else
                {
                    this.Logger.WriteCheckFailed(
                        String.Format("Range Constraint <a href=\"mga:{0}\">{1}</a> is marked \"Exclusion\" but must have all other range values provided.",
                                      c.ID,
                                      c.Name));
                    return null;
                }
            }
            else
            {
                pcons.type = "range";
            }

            var regx = new Regex("[0-9. ]*-[0-9. ]*");

            if (xRangeProvided && c.Attributes.XRange.Contains('-'))
                Logger.WriteWarning("X Range using deprecated seperator '-', please update to ':'");
            if (yRangeProvided && c.Attributes.YRange.Contains('-'))
                Logger.WriteWarning("Y Range using deprecated seperator '-', please update to ':'");
            if (layerProvided && c.Attributes.LayerRange.Contains('-'))
                Logger.WriteWarning("Layer Range using deprecated seperator '-', please update to ':'");

            if (xRangeProvided)
                pcons.x = c.Attributes.XRange.Replace('-',':');
            if (yRangeProvided)
                pcons.y = c.Attributes.YRange.Replace('-',':');
            if (layerProvided)
                pcons.layer = c.Attributes.LayerRange.Replace('-',':');
            
            return pcons;
        }

        private static GlobalLayoutConstraintException ConvertGlobalLayoutConstraintException(Tonka.GlobalLayoutConstraintException c)
        {
            var pcons = new GlobalLayoutConstraintException();
            pcons.type = "except";
            if (!String.IsNullOrWhiteSpace(c.Attributes.Constraint.ToString()))
            {
                pcons.name = c.Attributes.Constraint.ToString();
            }
            return pcons;
        }

        private static ExactConstraint ConvertExactLayoutConstraint(Tonka.ExactLayoutConstraint c)
        {
            var pcons = new ExactConstraint();
            pcons.type = "exact";
            if (!String.IsNullOrWhiteSpace(c.Attributes.X))
                pcons.x = Convert.ToDouble(c.Attributes.X, CultureInfo.InvariantCulture);
            if (!String.IsNullOrWhiteSpace(c.Attributes.Y))
                pcons.y = Convert.ToDouble(c.Attributes.Y, CultureInfo.InvariantCulture);
            if (!String.IsNullOrWhiteSpace(c.Attributes.Layer))
                pcons.layer = Convert.ToInt32(c.Attributes.Layer);
            if (!String.IsNullOrWhiteSpace(c.Attributes.Rotation))
                pcons.rotation = Convert.ToInt32(c.Attributes.Rotation);
            return pcons;
        }

        private IEnumerable<Constraint> GetConstraintsOfParents(Component comp)
        {
            var rtn = new List<Constraint>();
            if (comp.Parent != null)
                rtn.AddRange(GetConstraintsOfParents(comp.Parent));
            return rtn;
        }

        private IEnumerable<Constraint> GetConstraintsOfParents(ComponentAssembly asm)
        {
            var rtn = new List<Constraint>();

            if (asm.Parent != null)
                rtn.AddRange(GetConstraintsOfParents(asm.Parent));

            #region ConstraintExceptions
            // Global-layout-constraint exceptions related to this assembly, MOT-728
            var glcException = from conn in asm.Impl.SrcConnections
                                                 .ApplyGlobalLayoutConstraintExceptionCollection
                               select ConvertGlobalLayoutConstraintException(conn.SrcEnds.GlobalLayoutConstraintException);

            if (glcException != null)
                rtn.AddRange(glcException);
            #endregion

            #region ExactConstraints
            // exact constraints related to this assembly
            var exactCons = from conn in asm.Impl.SrcConnections
                                                 .ApplyExactLayoutConstraintCollection
                            select ConvertExactLayoutConstraint(conn.SrcEnds.ExactLayoutConstraint);

            if (exactCons != null)
                rtn.AddRange(exactCons);
            #endregion

            #region RangeConstraints
            // range constraints related to this assembly
            var rangeCons = from conn in asm.Impl.SrcConnections
                                                 .ApplyRangeLayoutConstraintCollection
                                      select ConvertRangeLayoutConstraint(conn.SrcEnds.RangeLayoutConstraint);

            if (rangeCons != null)
                rtn.AddRange(rangeCons);
            #endregion

            
            #region RelativeRangeConstraints
            // range constraints related to this assembly
            var relativeRangeCons = from conn in asm.Impl.SrcConnections
                                                    .ApplyRelativeRangeLayoutConstraintCollection
                                        select ConvertRelativeRangeConstraint(conn.SrcEnds.RelativeRangeConstraint);

            if (relativeRangeCons != null)
                rtn.AddRange(relativeRangeCons);
            #endregion

            return rtn;
        }

        public int HandlePreRoutedAsm(ComponentAssembly obj, int pkg_idx)
        {
            // 1) all parts that are part of the pre-routed assembly are tagged with a 'virtual' spaceClaim part
            // 2) we add their absolute location (in the subckt frame of refernece) in the output json
            // 3) we create virtual spaceClaim parts for pre-routed subcircuits

            // 4) all nets belonging to the pre-routed subcircuit are tagged with the spaceClaim part
            // 5) these nets are added to json with absolute location (same as parts above)

            if (CodeGenerator.preRouted.ContainsKey(obj))
            {
                var layoutBox = (obj.Impl.Impl as GME.MGA.MgaFCO).RegistryValue["layoutBox"];
                // format x1,y1,w1,h1; x2,y2,w2,h2; x3,y3,w3,h3
                // NOTE: if there are multiple bounding boxes 
                // - we expect the first one to be at the origin of the reference frame
                // - this minor limitation would be easy to address later by adding code to identify 
                // - the bounding box at origin even if its not the first one

                var bBoxs = layoutBox.Split(';');
                int bbidx = 0;
                int origPkgIdx = 0;
                foreach (var bbox in bBoxs)
                {
                    Package pkg = new Package()
                    {
                        pkg_idx = pkg_idx++,
                        name = obj.Name,
                        package = "__spaceClaim__",
                        ComponentID = GetComponentAssemblyID(obj.Impl.Impl as GME.MGA.MgaModel)
                    };

                    var pts = bbox.Split(',');
                    if (pts.Length >= 4)
                    {
                        double d = Double.NaN;
                        if (Double.TryParse(pts[0], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                            pkg.x = d;
                        if (Double.TryParse(pts[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                            pkg.y = d;
                        if (Double.TryParse(pts[2], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                            pkg.width = d;
                        if (Double.TryParse(pts[3], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                            pkg.height = d;
                    }
                    if (pts.Length >= 5)
                    {
                        int d = int.MaxValue;
                        if (int.TryParse(pts[4], out d))
                        {
                            var econs = new ExactConstraint()
                            {
                                type = "exact",
                                layer = d
                            };

                            if (pkg.constraints == null)
                                pkg.constraints = new List<Constraint>();
                            pkg.constraints.Add(econs);
                        }
                    }
                    else // MOT-777 Force pre-routed subassemblies to the top layer
                    {
                        var econs = new ExactConstraint()
                        {
                            type = "exact",
                            layer = 0
                        };

                        if (pkg.constraints == null)
                            pkg.constraints = new List<Constraint>();
                        pkg.constraints.Add(econs);
                    }

                    if (bbidx == 0)
                    {                        
                        preroutedPkgMap.Add(obj, pkg);
                        origPkgIdx = (int)pkg.pkg_idx;

                        //NOTE: Relative constraints can't be computed until after all other components have
                        //      been enumerated; see LayoutGenerator constructor for the code that computes
                        //      those for pre-routed subassemblies
                        //Find and add exact layout constraints; these go on the first/origin bounding box
                        #region ExactConstraints
                        var exactCons =
                            from conn in obj.Impl.SrcConnections.ApplyExactLayoutConstraintCollection
                            select conn.SrcEnds.ExactLayoutConstraint;

                        foreach (var c in exactCons)
                        {
                            var pcons = ConvertExactLayoutConstraint(c);
                            if (pkg.constraints == null)
                                pkg.constraints = new List<Constraint>();

                            //The space claims for pre-routed subassemblies don't seem to have an originX/originY value; we'll default
                            //it to 0,0 (the center of the package) while respecting it if present (in case something changes later)
                            var originX = pkg.originX.GetValueOrDefault(0.0);
                            var originY = pkg.originY.GetValueOrDefault(0.0);

                            ConvertExactConstraintFromOrigin(pcons, pkg.width, pkg.height, originX, originY);

                            pkg.constraints.Add(pcons);
                        }
                        #endregion
                    }
                    else
                    {
                        pkg.ComponentID += "." + bbidx.ToString();

                        // This isn't the first layout box in the layoutBox registry value.
                        // So, we'll add a relative-pkg constraint to this layout box, relative to first one.

                        // The rotation offset is the X offset between the starting lower-left corner and the
                        // (different) ending lower-left corner, when the layout box is rotated 90 degrees 
                        // counter-clockwise about its center. MOT-779
                        double rotationOffset = (pkg.width - pkg.height) / 2;

                        double relX = pkg.x.GetValueOrDefault(0.0);
                        double relY = pkg.y.GetValueOrDefault(0.0);

                        var pcons = new RelativeConstraint()
                        {
                            type = "relative-pkg",
                            x = quantize(relX),
                            y = quantize(relY),
                            x1 = quantize(relX + rotationOffset),    // MOT-779
                            y1 = quantize(relY - rotationOffset),
                            x2 = quantize(relX),
                            y2 = quantize(relY),
                            x3 = quantize(relX + rotationOffset),
                            y3 = quantize(relY - rotationOffset),
                            pkg_idx = origPkgIdx,
                            relativeRotation = 0,
                        };

                        if (pkg.constraints == null)
                            pkg.constraints = new List<Constraint>();
                        pkg.constraints.Add(pcons);
                    }

                    //Find and add range constraints, these are applied to ALL bounding boxes
                    #region RangeConstraints
                    // range constraints related to this part
                    var rangeCons =
                        from conn in obj.Impl.SrcConnections.ApplyRangeLayoutConstraintCollection
                        select conn.SrcEnds.RangeLayoutConstraint;
                    foreach (var c in rangeCons)
                    {
                        var pcons = ConvertRangeLayoutConstraint(c);
                        if (pcons == null)
                            continue;

                        if (pkg.constraints == null)
                            pkg.constraints = new List<Constraint>();
                        pkg.constraints.Add(pcons);
                    }
                    #endregion

                    boardLayout.packages.Add(pkg);
                    bbidx++;
                }
                // TBD SKN figure out relative constraint between multiple bboxes

                // if this assembly is prelaid out, then don't go into its children
            }
            else
            {
                // this assembly is not prelaid out; check for descendants that are
                foreach (var ca in obj.ComponentAssemblyInstances)
                {
                    pkg_idx = HandlePreRoutedAsm(ca, pkg_idx);
                }
            }
            return pkg_idx;
        }

        public void Generate(string layoutFile)
        {
            StreamWriter writer = new StreamWriter(layoutFile);
            string sjson = JsonConvert.SerializeObject(boardLayout, Newtonsoft.Json.Formatting.Indented, 
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore }) ;
            writer.Write(sjson);
            writer.Close();
        }

    }

    public class LayoutParser
    {
        public Dictionary<Port, Signal> portTraceMap { get; set; }
        public Dictionary<Component, Package> componentPackageMap { get; set; }

        public CodeGenerator.Mode mode { get; set; }
        public string parentInstanceGUID { get; set; }
        public string parentGUID { get; set; }

        private GMELogger gmeLogger;
        private string layoutFile;
        private CodeGenerator CodeGenerator;

        public LayoutParser(string inFile, GMELogger inLogger, CodeGenerator CodeGenerator)
        {
            mode = CodeGenerator.Mode.EDA;      // default mode
            gmeLogger = inLogger;
            layoutFile = inFile;
            portTraceMap = new Dictionary<Port, Signal>();
            componentPackageMap = new Dictionary<Component, Package>();
            this.CodeGenerator = CodeGenerator;
        }

        public void BuildMaps()
        {
            var layout = Parse(layoutFile);
            var packageComponent = new Dictionary<string, Component>();

            // the parsing here basically maps components from the read in json file to Component 
            // objects in build network
            // and it maps the nets (signal) to:  a) schematic ports, and b) spice ports 

            foreach (var p in layout.packages)  // populate package name --> component map
            {
                if (String.Equals(p.package, "__spaceClaim__"))
                {
                    // Space claims don't correspond to components, and don't need to be matched as below.
                    continue;
                }

                var pguid = parentInstanceGUID == null ? p.ComponentID :
                    parentInstanceGUID + p.ComponentID;

                // A component with an empty GUID will not be matchable
                if (String.IsNullOrEmpty(pguid))
                {
                    continue;
                }

                Component comp;
                if (CyPhyBuildVisitor.ComponentInstanceGUIDs.TryGetValue(pguid, out comp) || (p.ComponentID != null && CyPhyBuildVisitor.ComponentInstanceGUIDs.TryGetValue(p.ComponentID, out comp)))
                {
                    packageComponent.Add(p.name.ToUpper(), comp);
                    packageComponent[p.name] = comp;
                    componentPackageMap.Add(comp, p);
                }
                else
                {
                    gmeLogger.WriteError("Parsing Layout Json: JSON component '{1}' has Instance GUID '{0}' but it was not found in model", pguid ?? "[null ComponentID]", p.name);
                }
            }

            // from nets --> pins --> packages --> ComponentID --> components --> ports
            foreach (var n in layout.signals)
            {
                foreach (var pin in n.pins)
                {
                    if (packageComponent.ContainsKey(pin.package))
                    {
                        var comp = packageComponent[pin.package];
                        var sch = comp.Impl.Children.SchematicModelCollection.FirstOrDefault();

                        // find the name/gate matching port in schematic domain model
                        var port = (sch != null) ?
                            sch.Children.SchematicModelPortCollection.
                            Where(p => p.Attributes.EDAGate == pin.gate && p.Name == pin.name).
                            FirstOrDefault()
                            : null;

                        if (mode == CodeGenerator.Mode.EDA) // if we are doing EDA then just map to sch domain ports
                            MapPortToNet(port, n);
                        else // SPICE_SI mode -- map to spice ports
                        {
                            // find the associated port in the component with the schematic port
                            foreach (var compPort in port.DstConnections.PortCompositionCollection.
                                Select(p => p.DstEnd).
                                Union(port.SrcConnections.PortCompositionCollection.Select(p => p.SrcEnd)))
                            {
                                // cast 
                                var compSchPort = compPort != null ? compPort as Tonka.SchematicModelPort : null;
                                // from the component port find the associated spice port
                                var spicePort = compSchPort != null ?
                                compSchPort.DstConnections.PortCompositionCollection.Select(p => p.DstEnd).
                                    Union(compSchPort.SrcConnections.PortCompositionCollection.Select(p => p.SrcEnd)).
                                    Where(q => q.ParentContainer is Tonka.SPICEModel).FirstOrDefault() : null;

                                MapPortToNet(spicePort as Tonka.SchematicModelPort, n);
                            }
                        }
                    }
                    else
                        gmeLogger.WriteError("Package {0} not found in layout.json file", pin.package);
                }
            }
        }

        public void MapPortToNet(Tonka.SchematicModelPort port, Signal n)
        {
            // get the build port for this schematic domain port & store the net with the build port
            var buildPort = (port != null) &&
                CyPhyBuildVisitor.Ports.ContainsKey(port.ID) ?
                CyPhyBuildVisitor.Ports[port.ID] : null;
            if (null == buildPort) 
            {
                if (null == port)
                {
                    gmeLogger.WriteInfo("Skipping a null port.");
                }
                else
                {
                    gmeLogger.WriteError("port {0} with ID {1} wasn't found in CyPhyBuildVisitor.Ports", port.Name, port.ID);
                }
            }
            else if (portTraceMap.ContainsKey(buildPort))
            {
                gmeLogger.WriteWarning("The buildPort {0} with hashCode {1} was already found in portTraceMap with net {2} instead of {3}.",
                    buildPort.Name, buildPort.GetHashCode(), portTraceMap[buildPort].name,  n.name);
            }

            // now remember this net with the spice-build port 
            // - we will use it when building the spice model
            if ((buildPort != null) && (!portTraceMap.ContainsKey(buildPort)))
            {
                portTraceMap.Add(buildPort, n);
                gmeLogger.WriteInfo("Mapping portTraceMap Build Port {0} with hashCode {1} to Net {2}", buildPort.Name, buildPort.GetHashCode(), n.name );
            }

            if (CodeGenerator.verbose)
            {
                if (buildPort == null)
                    gmeLogger.WriteWarning("No Matching Build Port  for {0}, ignoring trace", port.Name);
                else
                    gmeLogger.WriteInfo("Mapping Build Port {0} to Net {1}", buildPort.Name, n.name);
            }

        }
       

        public LayoutJson.Layout Parse(string layoutFile)
        {
            StreamReader reader = new StreamReader(layoutFile);
            string sjson = reader.ReadToEnd();
            LayoutJson.Layout layout = JsonConvert.DeserializeObject<LayoutJson.Layout>(sjson);
            return layout;
        }

    }
}
