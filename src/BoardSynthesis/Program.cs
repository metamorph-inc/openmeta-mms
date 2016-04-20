using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Eagle = CyPhy2Schematic.Schematic.Eagle;
using Newtonsoft.Json;
using LayoutJson;


namespace BoardSynthesis
{

    class Program
    {
        const string ATTRIB_LAYER = "27";
        const string BOARD_LAYER = "20";
        const string DEBUG_LAYER = "80";
        const string DEBUG_LAYER_2 = "81";
        const int USER_LAYOUT_SIGNIFICANT_DECIMAL_PLACES = 4;   // Allows component-origin-location resolution of 0.0001 mm in board files generated from user-placed layouts.
        const int AUTO_LAYOUT_SIGNIFICANT_DECIMAL_PLACES = 1;   // Allows component-origin-location resolution of 0.1 mm in board files generated from auto-placed layouts.
        const double EPSILON = 1.0e-6;

        // Rotates the vector <x,y> by a multiple of 90 degrees.
        public static Tuple<double, double> rotateVector( double x, double y, int rot )
        {
            rot = rot % 4;

            for (int i = 0; i < rot; i++)
            {
                // Rotate the  vector (x, y) by 90 degrees.
                // This is just a simplification of the math of a matrix multiplication of a 90-degree rotation matrix times the vector.
                // It allows us to avoid creating many special cases when computing rotated offsets.
                // See also: https://en.wikipedia.org/wiki/Rotation_matrix
                double rotX = -y;
                y = x;
                x = rotX;
            }
            return Tuple.Create(x, y);
        }

        static void Main(string[] args)
        {
            #region ProcessArguments
            // need schematic input file / layout json input file name / output board file name as command line args
            if (args.Length < 2)
            {
                Console.WriteLine("usage: BoardSynthesis <schema.sch> <layout.json> [-r] [-v]");
                Console.WriteLine("     Input: <schema.sch>, <layout.json>;  Output: <schema.brd>");
                Console.WriteLine("[-r] Input: <schema.sch> (implicit schema.brd), <layout.json>; Output: <layout.json>");
                Console.WriteLine("[-v] Show verbose debug messages.");
                return;
            }
            var schematicFile = args[0];
            var layoutFile = args[1];
            var boardFile = schematicFile.Replace(".sch", ".brd");
            bool reverseBoardSynthesis = false;   // default to forward board synthesis
            bool showVerboseMessages = false;   // default to hiding verbose messages

            // Check for command-line argument switches
            for (int argIndex = 1; argIndex < args.Length; argIndex++)
            {
                if (args[argIndex].CompareTo("-r") == 0)
                {
                    reverseBoardSynthesis = true;
                }
                if (args[argIndex].CompareTo("-v") == 0)
                {
                    showVerboseMessages = true;
                }
            }
            #endregion

            #region LoadSchematic
            // load schematic file
            Eagle.eagle schematic = null;
            try
            {
                schematic = Eagle.eagle.LoadFromFile(schematicFile);
                Console.WriteLine("Parsed Schematic File : " + schematicFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR Reading Schematic XML: " + e.Message);
                // Console.WriteLine("ERROR Reading Schematic XML: " + e.Message +" " + e.StackTrace + " " + (e.InnerException != null ? e.InnerException.ToString() : ""));
                System.Environment.Exit(-1);
            }
            #endregion

            #region LoadLayout
            // load layout file
            Layout boardLayout = null;
            Dictionary<string, Package> packageMap = new Dictionary<string, Package>
                (StringComparer.InvariantCultureIgnoreCase);
            Dictionary<string, Signal> signalMap = new Dictionary<string, Signal>
                (StringComparer.InvariantCultureIgnoreCase);

            try
            {
                string sjson = File.ReadAllText(layoutFile, Encoding.UTF8);
                boardLayout = JsonConvert.DeserializeObject<Layout>(sjson);
                foreach (var pkg in boardLayout.packages)
                {
                    if ((pkg.name != null) && (pkg.ComponentID != null))    // Avoid using a null keys.  See MOT-611.
                    {
                        packageMap[pkg.name] = pkg;
                        packageMap[pkg.ComponentID] = pkg;  // for ID search
                    }
                }
                if (boardLayout.signals == null) boardLayout.signals = new List<Signal>();
                foreach (var sig in boardLayout.signals)
                {
                    if (showVerboseMessages)
                    {
                        Console.WriteLine("Found signal {0} in boardLayout:", sig.name);
                        foreach (var pin in sig.pins)
                        {
                            Console.WriteLine("   + pin {0} gate {1} of {2}.", pin.name, pin.gate, pin.package);
                        }
                    }
                    signalMap[sig.name] = sig;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR Reading Layout File: " + e.Message);
                System.Environment.Exit(-1);
            }
            #endregion

            Eagle.eagle eagle = null;

            #region LoadBoardTemplate
            // load board template file 
            // if there is a user-defined load that one, 
            // otherwise, load the one bundled as a project resource
            if (boardLayout.boardTemplate != null && boardLayout.boardTemplate != "")
            {
                try
                {
                    eagle = Eagle.eagle.LoadFromFile(boardLayout.boardTemplate);
                    Console.WriteLine("Parsed Eagle Board Template File {0}: " + eagle.version, boardLayout.boardTemplate);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Unable to load Board Template XML: " + e.Message);
                }
            }

            if (eagle == null)  // unable 
            {
                try
                {
                    var brdTempl = Properties.Resources.boardTemplate;
                    eagle = Eagle.eagle.Deserialize(brdTempl);
                    Console.WriteLine("Parsed Eagle Library schema version: " + eagle.version);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Failed parsing default board Template XML: " + e.Message);
                    System.Environment.Exit(-1);
                }
            }
            #endregion

            #region Layout2Board
            if (!reverseBoardSynthesis)   // forward mode: layout --> board
            {
                // Create Board File
                Console.WriteLine("Board synthesis forward mode; layout --> board.");
                /*
                 * Here's where we convert data from a layout.json file to the data of an Eagle board file.
                 */
                {
                    var plain = (eagle.drawing.Item as Eagle.board).plain;

                    // skip boundary generation if - omitBoundary is set, and has a boardTemplate file
                    bool skipBoundary =
                        (boardLayout.omitBoundary.HasValue &&
                        boardLayout.omitBoundary.Value &&
                        boardLayout.boardTemplate != null && 
                        boardLayout.boardTemplate != "");
                    if (!skipBoundary)
                    {
                        // Mark board boundaries
                        AddBoundary(boardLayout, plain);
                    }
					
                    // Add Layers for bounding box
                    AddBoundingLayers(eagle.drawing.layers);                    

                    // Design Rules
                    if (!String.IsNullOrWhiteSpace(boardLayout.designRules))
                    {
                        CopyDesignRules(boardLayout.designRules, (eagle.drawing.Item as Eagle.board).designrules);
                    }

                    // Copy Libraries
                    var srcLib = (schematic.drawing.Item as Eagle.schematic).libraries;
                    var dstLib = (eagle.drawing.Item as Eagle.board).libraries;
                    foreach (var lib in srcLib.library)
                    {
                        dstLib.library.Add(lib);
                    }

                    // Add Elements with Position
                    var parts = (schematic.drawing.Item as Eagle.schematic).parts;
                    var elements = new Eagle.elements(); 
                    AddElements(parts, packageMap, dstLib, elements, plain);
                    (eagle.drawing.Item as Eagle.board).elements = elements;

                    // Add Signals 
                    var nets = (schematic.drawing.Item as Eagle.schematic).sheets.sheet.Select(s => s.nets).FirstOrDefault();
                    var signals = new Eagle.signals();
                    AddSignals(nets, parts, signalMap, packageMap, dstLib, signals);
                    (eagle.drawing.Item as Eagle.board).signals = signals;
                }

                eagle.SaveToFile(boardFile);
            }
            #endregion

            #region Board2Layout
            else if (reverseBoardSynthesis)  // reverse mode: board --> layout
            {
                Console.WriteLine("Board synthesis reverse mode; board --> layout.");
                // Here's where we use a board file, plus a layout.json file, to create a new layout.json file.
                // The board file has the user-placed component origins and rotations.
                // The layout file has the unrotated bounding box's height and width, as well as 
                // the offset from the unrotated part's origin to its bounding box centroid.
                try
                {
                    eagle = Eagle.eagle.LoadFromFile(boardFile);
                    Console.WriteLine("Parsed Board File : " + boardFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR Reading Board XML: " + e.Message);
                    System.Environment.Exit(-1);
                }

                /*
                 * Here's where we get the board's width and height into the new layout data, from the EAGLE-board outline. MOT-788
                 */
                {
                    var plain = (eagle.drawing.Item as Eagle.board).plain;
                    var xList = new List<double>();
                    var yList = new List<double>();

                    foreach (var item in plain.Items)
                    {
                        var wire = item as Eagle.wire;

                        if (wire.layer == BOARD_LAYER)  // Check layer 20 ("Dimension") for board outline
                        {

                            double dx1 = Convert.ToDouble(wire.x1);
                            xList.Add(dx1);
                            double dx2 = Convert.ToDouble(wire.x2);
                            xList.Add(dx2);
                            double dy1 = Convert.ToDouble(wire.y1);
                            yList.Add(dy1);
                            double dy2 = Convert.ToDouble(wire.y2);
                            yList.Add(dy2);
                        }
                    }

                    double maxX = xList.Max();
                    double maxY = yList.Max();

                    if ((maxX > 0) && (maxY > 0))
                    {
                        boardLayout.boardWidth = maxX;
                        boardLayout.boardHeight = maxY;
                        Console.WriteLine("Setting boardWidth to " + maxX.ToString("0.##") + " mm, and boardHeight to " + maxY.ToString("0.##") + " mm.");
                    }
                }

                Dictionary<string, Eagle.element> elementMap = new Dictionary<string, Eagle.element>();

                var elements = (eagle.drawing.Item as Eagle.board).elements;
                foreach (var element in elements.element)
                {
                    // For each part
                    var key = packageMap.Keys.Where(name => (name.Replace("&", "_").ToUpperInvariant() == element.name.ToUpperInvariant())).FirstOrDefault();
                    if (key != null && packageMap.ContainsKey(key))
                    {
                        var pkg = packageMap[key];

                        // Convert the angular rotation from the board file to its corresponding integer multiple of 90 degrees.
                        if (element.rot.Contains("R90"))
                            pkg.rotation = 1;
                        else if (element.rot.Contains("R180"))
                            pkg.rotation = 2;
                        else if (element.rot.Contains("R270"))
                            pkg.rotation = 3;
                        else
                            pkg.rotation = 0;

                        // If the component is on the bottom of the board, we need to change its layout layer to "1".
                        if (element.rot.Contains("M"))
                            pkg.layer = 1;

                        int rot = (int)pkg.rotation;

                        // Now get the part's origin from the board file.
                        double originX = Convert.ToDouble(element.x);
                        double originY = Convert.ToDouble(element.y);

                        // Now get the vector from the unrotated part's origin to its centroid.
                        double unrotatedOriginToCentroidOffsetX = pkg.originX.HasValue ? pkg.originX.Value : 0.0;
                        if (pkg.layer == 1)
                        {
                            // Flip the x offset if it's on the bottom.
                            unrotatedOriginToCentroidOffsetX = -unrotatedOriginToCentroidOffsetX;
                        }

                        double unrotatedOriginToCentroidOffsetY = pkg.originY.HasValue ? pkg.originY.Value : 0.0;

                        // Now rotate the vector.
                        Tuple<double, double> rotatedOriginToCentroid = rotateVector(unrotatedOriginToCentroidOffsetX, unrotatedOriginToCentroidOffsetY, rot);

                        // Add the rotated offsets to the origin to compute the centroid's location
                        double centroidX = originX + rotatedOriginToCentroid.Item1;
                        double centroidY = originY + rotatedOriginToCentroid.Item2;

                        // Get unrotated offsets from the bounding box centroid to the lower left corner.
                        double CentroidToLowerLeftOffsetX = -pkg.width / 2.0;
                        double CentroidToLowerLeftOffsetY = -pkg.height / 2.0;

                        if ((rot % 2) == 1)
                        {
                            // Correct the offsets for odd rotations by swapping height and width.
                            CentroidToLowerLeftOffsetX = -pkg.height / 2.0;
                            CentroidToLowerLeftOffsetY = -pkg.width / 2.0;
                        }

                        // Compute the location of the lower-left corner
                        double lowerLeftCornerX = centroidX + CentroidToLowerLeftOffsetX;
                        double lowerLeftCornerY = centroidY + CentroidToLowerLeftOffsetY;

                        // Round the lower-left coordinates for use in the generated layout file.
                        pkg.x = Math.Round(lowerLeftCornerX, USER_LAYOUT_SIGNIFICANT_DECIMAL_PLACES);
                        pkg.y = Math.Round(lowerLeftCornerY, USER_LAYOUT_SIGNIFICANT_DECIMAL_PLACES);
                    }
                    else
                    {
                        Console.WriteLine("WARNING: part {0} not found in layout file", element.name);
                    }
                    elementMap.Add(element.name, element);
                }

                var signals = (eagle.drawing.Item as Eagle.board).signals;
                foreach (var signal in signals.signal)
                {
                    if (showVerboseMessages)
                    {
                        Console.WriteLine("Found signal {0} in Eagle.board:", signal.name);
                    }

                    Signal jsig = null;
                    bool update = true;
                    if (signalMap.ContainsKey(signal.name))
                        jsig = signalMap[signal.name];
                    else
                    {
                        jsig = new Signal()
                        {
                            name = signal.name
                        };
                        update = false;
                    }
                    jsig.pins = new List<Pin>();
                    jsig.wires = new List<Wire>();
                    jsig.vias = new List<Via>();
                    jsig.polygons = new List<Polygon>();
                    double traceLength = 0.0;
                    Dictionary<string, Pin> pinMap = new Dictionary<string, Pin>();
                    foreach (var item in signal.Items)
                    {
                        if (item is Eagle.contactref)
                        {
                            var cref = item as Eagle.contactref;
                            var pin = new Pin();
                            var package = packageMap.Keys.Where(name => (name.Replace("&", "_").ToUpperInvariant() == cref.element.ToUpperInvariant())).FirstOrDefault();
                            pin.package = package ?? cref.element;
                            var element = elementMap.ContainsKey(cref.element) ? elementMap[cref.element] : null;
                            if (element != null && schematic.drawing.Item is Eagle.schematic) // FIXME CT-151 need to review this "is" test. Sometimes schematic.drawing.Item is Eagle.board
                            {
                                var lib = (schematic.drawing.Item as Eagle.schematic).libraries.library.Where(l => l.name.Equals(element.library)).FirstOrDefault();
                                var namedDs = (lib != null) ? lib.devicesets.deviceset.Where(ds => ds.name.Equals(element.value)).FirstOrDefault() : null;
                                var namedDev = (lib != null) ? lib.devicesets.deviceset.Where(ds => ds.name.Equals(element.value)).
                                    SelectMany(ds => ds.devices.device).Where(d => d.package.Equals(element.package)).FirstOrDefault() : null;
                                var namedConnect = (namedDev != null) ? namedDev.connects.connect.Where(c => c.pad.Equals(cref.pad)).FirstOrDefault() : null;

                                if (namedConnect != null)
                                {
                                    pin.name = namedConnect.pin;
                                    pin.pad = namedConnect.pad;
                                    pin.gate = namedConnect.gate;
                                    if (showVerboseMessages)
                                    {
                                        Console.WriteLine("    + connection {0} pin {1} pad {2} gate {3} of {4}.", element.value, pin.name, pin.pad, pin.gate, pin.package);
                                    }
                                }
                                else
                                {
                                    var dev = (lib != null) ? lib.devicesets.deviceset.SelectMany(ds => ds.devices.device).Where(d => d.package.Equals(element.package)).FirstOrDefault() : null;
                                    var connect = (dev != null) ? dev.connects.connect.Where(c => c.pad.Equals(cref.pad)).FirstOrDefault() : null;
                                    pin.name = (connect != null) ? connect.pin : cref.pad;

                                    pin.pad = (connect != null) ? connect.pad : cref.pad;
                                    pin.gate = (connect != null) ? connect.gate : null;
                                    if (showVerboseMessages)
                                    {
                                        Console.WriteLine("    + unnamed connection pin {0} pad {1} gate {2} of {3}.", pin.name, pin.pad, pin.gate, pin.package);
                                    }
                                }
                                if (showVerboseMessages && (null == pin.gate))
                                {
                                    Console.WriteLine("Found a null pin.gate.");
                                }
                            }
                            //var package = packageMap.Keys.Where(name => (name.Replace("&", "_").ToUpperInvariant() == cref.element.ToUpperInvariant())).FirstOrDefault();
                            //pin.package = package ?? cref.element;
                            var pinName = string.Format("{0}.{1}", pin.package, pin.name);
                            // prevent creation of duplicate pins from the same package
                            if (!pinMap.ContainsKey(pinName))
                            {
                                jsig.pins.Add(pin);
                                pinMap[pinName] = pin;
                            }
                        }
                        else if (item is Eagle.wire)
                        {
                            var wire = item as Eagle.wire;
                            var jwire = new Wire()
                            {
                                x1 = Convert.ToDouble(wire.x1),
                                x2 = Convert.ToDouble(wire.x2),
                                y1 = Convert.ToDouble(wire.y1),
                                y2 = Convert.ToDouble(wire.y2),
                                width = Convert.ToDouble(wire.width),
                                layer = Convert.ToInt32(wire.layer)
                            };
                            jsig.wires.Add(jwire);
                            double w = jwire.x2 - jwire.x1;
                            double h = jwire.y2 - jwire.y1;
                            double l = Math.Sqrt(w * w + h * h);
                            traceLength += l;
                        }
                        else if (item is Eagle.via)
                        {
                            var via = item as Eagle.via;
                            var jvia = new Via()
                            {
                                x = Convert.ToDouble(via.x),
                                y = Convert.ToDouble(via.y),
                                drill = Convert.ToDouble(via.drill)
                            };
                            string[] layerRange = via.extent.Split('-');
                            if (layerRange.Count() >= 2)
                            {
                                jvia.layerBegin = Convert.ToInt32(layerRange[0]);
                                jvia.layerEnd = Convert.ToInt32(layerRange[1]);
                            }
                            jsig.vias.Add(jvia);
                        }
                        else if (item is Eagle.polygon)
                        {
                            var poly = item as Eagle.polygon;
                            var jpoly = new Polygon()
                            {
                                layer = Convert.ToInt32(poly.layer),
                                isolate = Convert.ToDouble(poly.isolate),
                                pour = poly.pour.ToString(),
                                rank = Convert.ToInt32(poly.rank),
                                thermals = (poly.thermals == Eagle.polygonThermals.yes),
                                width = Convert.ToDouble(poly.width),
                                vertices = new List<Vertex>()
                            };

                            foreach (var ver in poly.vertex)
                            {
                                var jver = new Vertex();
                                jver.x = Convert.ToDouble(ver.x);
                                jver.y = Convert.ToDouble(ver.y);
                                jver.curve = Convert.ToDouble(ver.curve);
                                jpoly.vertices.Add(jver);
                            }
                            jsig.polygons.Add(jpoly);
                        }
                        jsig.length = traceLength;
                        jsig.bends = jsig.wires.Count + jsig.vias.Count - 1;
                    }
                    if (!update)
                    {
                        if (showVerboseMessages)
                        {
                            foreach (var pin in jsig.pins)
                            {
                                Console.WriteLine("   + jsig {3} pin {0} gate {1} of {2}.", pin.name, pin.gate, pin.package, jsig.name);
                            }
                        }
                        boardLayout.signals.Add(jsig);
                    }
                }
                Console.WriteLine("Writing Output to {0}", layoutFile);
                var sjson = JsonConvert.SerializeObject(boardLayout, Formatting.Indented);
                File.WriteAllText(layoutFile, sjson);
            }
            #endregion
        }

        // This function converts parts from the layout.json data format to the Eagle board format.
        static void AddElements(Eagle.parts parts, Dictionary<string, Package> packageMap, 
            Eagle.libraries libs, Eagle.elements elements, Eagle.plain plain)
        {
            foreach (var part in parts.part)
            {
                Eagle.element element = new Eagle.element()
                {
                    name = part.name,
                    library = part.library,
                    value = part.value  // MOT-743
                };

                var devset =
                    libs.library.Where(l => l.name.Equals(part.library)).                     // find library
                    SelectMany(l => l.devicesets.deviceset).Where(ds => ds.name.Equals(part.deviceset)).    // find deviceset
                    FirstOrDefault();
                var device = devset != null ?
                    devset.devices.device.Where(d => d.name.Equals(part.device)).             // find device
                    FirstOrDefault() : null;
                var tech = device != null ?
                    device.technologies.technology.Select(t => t.name).FirstOrDefault() : null;

                if (devset == null || device == null)
                {
                    Console.WriteLine("ERROR: part {0} does not have device or deviceset information in schema file", part.name); 
                    continue;
                }
                element.package = device.package;

                if (packageMap.ContainsKey(part.name))
                {
                    // The layout file stores coordinates for the part's bounding box lower left corner, as pkg.x and pkg.y.
                    // For Eagle, we need to convert that to the location of the "origin" of the part, which is normally near
                    // the centroid of the part's pads.
                    //
                    // To change the coordinates, we have several clues:
                    // 1. The width and height of an unrotated bounding box for the part, namely pkg.width and pkg.height.
                    // 2. The counterclockwise rotation angle of the part, as an integer multiple of 90 degrees, namely pkg.rotation.
                    // 3. the offset from an unrotated part's origin to its bounding-box centroid, namely pkg.originX and pkg.originY.
                    //
                    var pkg = packageMap[part.name];
                    double pkgx = (pkg.x != null) ? (double)pkg.x : 0.0;
                    double pkgy = (pkg.y != null) ? (double)pkg.y : 0.0;
                    int prot = (pkg.rotation != null) ? (int)pkg.rotation : 0;

                    // Now (pkgx, pkgy) is the lower left corner of the possibly rotated bounding box.
                    // We next compute the centroid of the bounding box.

                    // Get unrotated offsets from lower left corner to the bounding box centroid.
                    double lowerLeftToCentroidOffsetX = pkg.width / 2.0;
                    double lowerLeftToCentroidOffsetY = pkg.height / 2.0;

                    if ((prot % 2) == 1)
                    {
                        // Correct the offsets for some rotations by swapping height and width.
                        lowerLeftToCentroidOffsetX = pkg.height / 2.0;
                        lowerLeftToCentroidOffsetY = pkg.width / 2.0;
                    }

                    double centroidX = Convert.ToDouble(pkgx) + lowerLeftToCentroidOffsetX;
                    double centroidY = Convert.ToDouble(pkgy) + lowerLeftToCentroidOffsetY;

                    // Now we have the location of the bounding box's centroid.
                    // Get the offset from the bounding-box centroid to the EAGLE origin, for an unrotated part.

                    double xorg = pkg.originX != null ? (double)pkg.originX : 0.0;
                    double yorg = pkg.originY != null ? (double)pkg.originY : 0.0;

                    double unrotatedCentroidToOriginX = -xorg;
                    if (1 == pkg.layer) // Component on board bottom flips the x offset.
                    {
                        unrotatedCentroidToOriginX = xorg;
                    }
                    double unrotatedCentroidToOriginY = -yorg;

                    // Compute the offset from the bounding-box centroid to the component origin of the possibly rotated part.
                    Tuple<double, double> rotatedCentroidToOrigin = rotateVector(unrotatedCentroidToOriginX, unrotatedCentroidToOriginY, prot);  

                    // Add the offset to compute the location of the origin of the part.
                    double OriginX = centroidX + rotatedCentroidToOrigin.Item1;
                    double OriginY = centroidY + rotatedCentroidToOrigin.Item2;

                    // Now that we have the location of the part's origin, we can round it to a desired precision.
                    // Our automatic layout uses a 0.1mm grid to place lower-left corners of bounding boxes, but
                    // because of the added offsets, the part's origin can be off grid.  To prevent many insignificant
                    // digits appearing in the part origin locations, we want to round those based on automatically-generated
                    // layout.json files to 0.1 mm.
                    //
                    // Some layout.json files, however, are generated from an Eagle board file that has had components manually positioned.
                    // These manually-positioned parts may be on a fractional-inch-based grid, so their origins should not be rounded
                    // to the nearest 0.1 mm. Any fractional-inch grid that's based on multiples of half a mil can be represented exactly
                    // using ten-thousandths of a millimeter, so manually-routed boards will have their origins rounded to the nearest
                    // ten-thousandth of a millimeter, which is 0.0001 mm, or four digits.

                    double x = Math.Round( OriginX, USER_LAYOUT_SIGNIFICANT_DECIMAL_PLACES );
                    double y = Math.Round( OriginY, USER_LAYOUT_SIGNIFICANT_DECIMAL_PLACES );

                    // Check the lower-left corner position to see if we should round to 1 decimal places.
                    double lowerLeftGridOffsetX = Math.Round(pkgx, AUTO_LAYOUT_SIGNIFICANT_DECIMAL_PLACES) - Math.Round(pkgx, USER_LAYOUT_SIGNIFICANT_DECIMAL_PLACES);
                    double lowerLeftGridOffsetY = Math.Round(pkgy, AUTO_LAYOUT_SIGNIFICANT_DECIMAL_PLACES) - Math.Round(pkgy, USER_LAYOUT_SIGNIFICANT_DECIMAL_PLACES);

                    double maxGridOffset = Math.Max(Math.Abs(lowerLeftGridOffsetX), Math.Abs(lowerLeftGridOffsetY));

                    if (maxGridOffset < EPSILON)
                    {
                        // Layout seems auto-placed.
                        // Our auto placement rounded up any lower-left-corner location to the next 0.1 mm.
                        // To avoid accumulated errors causing an incorrect exact layout constraint position,
                        // we need to truncate the auto-placed board origin to the next lower 0.1 mm here.
                        x = 0.1 * Math.Floor(10.0 * OriginX + EPSILON);
                        y = 0.1 * Math.Floor(10.0 * OriginY + EPSILON);
                    }

                    // Now x and y are component-origin coordinates, relative to the frame of reference.
                    // (The EAGLE board's origin will be the frame of reference origin, unless the part is relatively placed.)
                    // Continue with preexisting code ...

                    double rx = x;
                    double ry = y;

                    if (pkg.RelComponentID != null &&       // Check if the part has a relative placement.
                        packageMap.ContainsKey(pkg.RelComponentID))
                    {
                        // Yes, we need to reposition it based on the orientation of the reference package.
                        // We do this in two steps:
                        // 1. We figure out the total rotation angle of the component by adding the rotation of the reference package.
                        // 2. We rotate the part's origin relative to the reference frame, and translate the reference frame to the origin of the reference package.
                        var refPkg = packageMap[pkg.RelComponentID];
                        // rotate the contained parts according to refPkg rotation
                        if (refPkg.rotation != null) prot += (int)refPkg.rotation;
                        if (prot > 3) prot = prot % 4;

                        RotateAndTranslate(refPkg, x, y, out rx, out ry);
                    }
                    element.x = rx.ToString();
                    element.y = ry.ToString();
                    string layer = ((pkg.layer == 1) ? "M" : "");
                    string rot = (90 * prot).ToString();
                    element.rot = layer + "R" + rot;

                    // add bounding box
                    AddBoundingBox(pkg, plain, packageMap);

                }
                else
                {
                    Console.WriteLine("WARNING: part {0} not found in layout file", part.name);
                }

                foreach (var sattrib in device.technologies.technology.SelectMany(t => t.attribute))
                {
                    Eagle.attribute dattrib = new Eagle.attribute()
                    {
                        name = sattrib.name,
                        value = sattrib.value,
                        layer = ATTRIB_LAYER,
                        display = Eagle.attributeDisplay.off
                    };

                    element.attribute.Add(dattrib);
                }


                elements.element.Add(element);
            }
        }
        
        static void AddSignals(Eagle.nets nets, Eagle.parts parts, 
            Dictionary<string, Signal> signalMap, Dictionary<string, Package> packageMap,
            Eagle.libraries libs, Eagle.signals signals)
        {
            foreach (var net in nets.net)
            {
                var signal = new Eagle.signal();
                signal.name = net.name;
                var pinrefs = net.segment.SelectMany(s => s.Items).
                    Where(i => i is Eagle.pinref).
                    Select(i => i as Eagle.pinref );

                foreach (var pinref in pinrefs )
                {
                    var part = parts.part.Where(p => p.name.Equals(pinref.part)).FirstOrDefault();
                    var device =
                        libs.library.Where(l => l.name.Equals(part.library)).                                   // find library
                        SelectMany(l => l.devicesets.deviceset).Where(ds => ds.name.Equals(part.deviceset)).    // find deviceset
                        SelectMany(ds => ds.devices.device).Where(d => d.name.Equals(part.device)).             // find device
                        FirstOrDefault();

                    if (device == null)
                    {
                        // same error as reported above 
                        continue;
                    }

                    var connect = device.connects.connect.
                        Where(c => c.pin.Equals(pinref.pin) && c.gate.Equals(pinref.gate)).
                        FirstOrDefault();

                    if (connect == null)
                    {
                        Console.WriteLine("ERROR: Device {0} in library is missing referenced pin ({1}) or gate ({2}) from schematic.", part.name, pinref.pin, pinref.gate);
                        continue;
                    }

                    var pads = connect.pad.Split(' ');
                    foreach (var pad in pads)
                    {
                        var contactref = new Eagle.contactref()
                        {
                            element = pinref.part,
                            pad = pad
                        };
                        if (connect.route != CyPhy2Schematic.Schematic.Eagle.connectRoute.all)
                            contactref.route = (CyPhy2Schematic.Schematic.Eagle.contactrefRoute)connect.route;
                        signal.Items.Add(contactref);
                    }
                }

                if (signalMap.ContainsKey(net.name))        // add pre-route if we already have it 
                {
                    var jsig = signalMap[net.name];
                    Package refPkg = null;
                    if (jsig.RelComponentID != null && packageMap.ContainsKey(jsig.RelComponentID))
                        refPkg = packageMap[jsig.RelComponentID];

                    if (jsig.wires != null)
                    {
                        foreach (var jwire in jsig.wires)
                        {
                            double rx1 = jwire.x1;
                            double rx2 = jwire.x2;
                            double ry1 = jwire.y1;
                            double ry2 = jwire.y2;

                            if (refPkg != null)
                            {
                                RotateAndTranslate(refPkg, jwire.x1, jwire.y1, out rx1, out ry1);
                                RotateAndTranslate(refPkg, jwire.x2, jwire.y2, out rx2, out ry2);
                            }

                            var wire = new Eagle.wire()
                            {
                                x1 = rx1.ToString(),
                                x2 = rx2.ToString(),
                                y1 = ry1.ToString(),
                                y2 = ry2.ToString(),
                                width = jwire.width.ToString(),
                                layer = jwire.layer.ToString()
                            };
                            signal.Items.Add(wire);
                        }
                    }
                    if (jsig.vias != null)
                    {
                        foreach (var jvia in jsig.vias)
                        {
                            double rx = jvia.x;
                            double ry = jvia.y;
                            if (refPkg != null)
                            {
                                RotateAndTranslate(refPkg, jvia.x, jvia.y, out rx, out ry);
                            }

                            var via = new Eagle.via()
                            {
                                x = rx.ToString(),
                                y = ry.ToString(),
                                extent = jvia.layerBegin.ToString() + '-' + jvia.layerEnd.ToString(),
                                drill = jvia.drill.ToString()
                            };
                            signal.Items.Add(via);
                        }
                    }
                    if (jsig.polygons != null)
                    {
                        foreach (var jpoly in jsig.polygons)
                        {
                            var poly = new Eagle.polygon()
                            {
                                layer = Convert.ToString(jpoly.layer),
                                width = Convert.ToString(jpoly.width),
                                rank = Convert.ToString(jpoly.rank),
                                isolate = Convert.ToString(jpoly.isolate),
                                thermals = jpoly.thermals ? Eagle.polygonThermals.yes : Eagle.polygonThermals.no
                            };

                            if (jpoly.pour.Equals(Eagle.polygonPour.solid.ToString())) 
                                poly.pour = Eagle.polygonPour.solid;
                            else if (jpoly.pour.Equals(Eagle.polygonPour.hatch.ToString()))
                                poly.pour = Eagle.polygonPour.hatch;
                            else if (jpoly.pour.Equals(Eagle.polygonPour.cutout.ToString()))
                                poly.pour = Eagle.polygonPour.cutout;
                            foreach (var jpv in jpoly.vertices)
                            {
                                double rx = jpv.x;
                                double ry = jpv.y;
                                if (refPkg != null)
                                {
                                    RotateAndTranslate(refPkg, jpv.x, jpv.y, out rx, out ry);
                                }
                                var ver = new Eagle.vertex()
                                {
                                    x = Convert.ToString(rx),
                                    y = Convert.ToString(ry),
                                    curve = Convert.ToString(jpv.curve)
                                };
                                poly.vertex.Add(ver);
                            }
                            signal.Items.Add(poly);
                        }
                    }
                }

                signals.signal.Add(signal);
            }
        }
 
        // This adds board-outline dimension-layer wires for a rectangle based on the board width and height.
        // These wires will eventually end up in an EAGLE-board-file XML subsection named "plain".
        static void AddBoundary(Layout boardLayout, Eagle.plain plain)
        {
            {
                // Bottom wire
                var w = new Eagle.wire()
                {
                    x1 = Convert.ToString(0),
                    x2 = Convert.ToString(boardLayout.boardWidth),
                    y1 = Convert.ToString(0),
                    y2 = Convert.ToString(0),
                    width = Convert.ToString(0),
                    layer = BOARD_LAYER
                };
                plain.Items.Add(w);
            }
            {
                // Top wire
                var w = new Eagle.wire()
                {
                    x1 = Convert.ToString(0),
                    x2 = Convert.ToString(boardLayout.boardWidth),
                    y1 = Convert.ToString(boardLayout.boardHeight),
                    y2 = Convert.ToString(boardLayout.boardHeight),
                    width = Convert.ToString(0),
                    layer = BOARD_LAYER
                };
                plain.Items.Add(w);
            }
            {
                // Left side
                var w = new Eagle.wire()
                {
                    x1 = Convert.ToString(0),
                    x2 = Convert.ToString(0),
                    y1 = Convert.ToString(0),
                    y2 = Convert.ToString(boardLayout.boardHeight),
                    width = Convert.ToString(0),
                    layer = BOARD_LAYER
                };
                plain.Items.Add(w);
            }
            {
                // Right side
                var w = new Eagle.wire()
                {
                    x1 = Convert.ToString(boardLayout.boardWidth),
                    x2 = Convert.ToString(boardLayout.boardWidth),
                    y1 = Convert.ToString(0),
                    y2 = Convert.ToString(boardLayout.boardHeight),
                    width = Convert.ToString(0),
                    layer = BOARD_LAYER
                };
                plain.Items.Add(w);
            }
        }

        static void RotateAndTranslate(Package refPkg, double x, double y, 
            out double rx, out double ry)
        {
            // point-wise translate the center of the part  
            // corresponding to frame rotation
            double theta = (double)refPkg.rotation * Math.PI / 2.0;
            rx = x * Math.Cos(theta) - y * Math.Sin(theta);
            ry = x * Math.Sin(theta) + y * Math.Cos(theta);

            if (refPkg.rotation == 1) rx += refPkg.height;
            else if (refPkg.rotation == 2) { rx += refPkg.width; ry += refPkg.height; }
            else if (refPkg.rotation == 3) ry += refPkg.width;

            // now translate
            rx += (double)refPkg.x;
            ry += (double)refPkg.y;
        }

        static void CopyDesignRules(string designRuleFile, Eagle.designrules designRules)
        {
            try
            {
                using (StreamReader druIn = new StreamReader(designRuleFile))
                {

                    // initialize the design rules section
                    designRules.name = Path.GetFileNameWithoutExtension(designRuleFile);
                    designRules.description.Clear();
                    designRules.param.Clear();

                    var line = druIn.ReadLine();
                    var regex = new Regex("[a-zA-Z]*\\[(.*)\\]");
                    while (line != null)
                    {
                        string[] nameValues = line.Split('=');
                        if (nameValues.Count() < 2)
                        {
                            // error 
                            continue;
                        }
                        var name = nameValues[0].Trim();
                        var value = nameValues[1].Trim();

                        if (name.Contains("description"))
                        {
                            var desc = new Eagle.description();
                            desc.Text = new string[]{ value };
                            Console.WriteLine(name);
                            desc.language = regex.Replace(name, "$1");
                            Console.WriteLine(desc.language);
                            designRules.description.Add(desc);
                        }
                        else
                        {
                            var param = new Eagle.param();
                            param.name = name;
                            param.value = value;
                            designRules.param.Add(param);
                        }

                        // read the next line
                        line = druIn.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading design rules file {0}", e.Message);
            }

        }

        static void AddBoundingBox(Package pkg, Eagle.plain plain, Dictionary<string, Package> packageMap)
        {
            double x = pkg.x.Value;
            double y = pkg.y.Value;
            int prot = (pkg.rotation != null) ? (int)pkg.rotation : 0;
            int refProt = 0;

            if (pkg.RelComponentID != null &&
                packageMap.ContainsKey(pkg.RelComponentID))
            // part has a relative placement
            {
                double rx = x;
                double ry = y;
                var refPkg = packageMap[pkg.RelComponentID];
                refProt = (refPkg.rotation != null) ? (int)refPkg.rotation : 0;
                 // rotate the contained parts according to refPkg rotation
                prot += refProt;
                if (prot > 3) prot = prot % 4;

                RotateAndTranslate(refPkg, x, y, out rx, out ry);
                x = rx;
                y = ry;
            }

            double width = (prot == 0 || prot == 2) ? pkg.width : pkg.height;
            double height = (prot == 0 || prot == 2) ? pkg.height : pkg.width;

            // (x,y) is where the original lower-left corner of the package would have ended up, based on the reference package's rotation and translation.
            // But, if the package was rotated due to the reference package's rotation, that lower-left corner is now some other corner.
            // So, we need to add in a correction to find the new lower-left corner.
            switch (refProt)
            {
                case 1:
                    // The bounding box was rotated 90 degrees ccw, so the lower left corner is now the lower right corner.
                    // Translate from the lower right corner back to the lower-left corner.
                    x -= width;
                    break;
                case 2:
                    // The bounding box was rotated 180 degrees ccw, so the lower left corner is now the upper right corner.
                    // Translate from the upper right corner back to the lower-left corner.
                    x -= width;
                    y -= height;
                    break;
                case 3:
                    // The bounding box was rotated 270 degrees ccw, so the lower left corner is now the upper left corner.
                    // Translate from the upper left corner back to the lower-left corner.
                    y -= height;
                    break;
            }

            // Now, (x,y) should be the lower-left corner of the rotated and translated bounding box.
            // Add all the wires of the bounding box.

            {
                var w = new Eagle.wire()
                {
                    x1 = Convert.ToString(x),
                    x2 = Convert.ToString(x + width),
                    y1 = Convert.ToString(y),
                    y2 = Convert.ToString(y),
                    width = Convert.ToString(0.1),
                    layer = pkg.layer == 0 ? DEBUG_LAYER : DEBUG_LAYER_2
                };
                plain.Items.Add(w);
            }
            {
                var w = new Eagle.wire()
                {
                    x1 = Convert.ToString(x + width),
                    x2 = Convert.ToString(x + width),
                    y1 = Convert.ToString(y),
                    y2 = Convert.ToString(y + height),
                    width = Convert.ToString(0.1),
                    layer = pkg.layer == 0 ? DEBUG_LAYER : DEBUG_LAYER_2
                };
                plain.Items.Add(w);
            }
            {
                var w = new Eagle.wire()
                {
                    x1 = Convert.ToString(x),
                    x2 = Convert.ToString(x + width),
                    y1 = Convert.ToString(y + height),
                    y2 = Convert.ToString(y + height),
                    width = Convert.ToString(0.1),
                    layer = pkg.layer == 0 ? DEBUG_LAYER : DEBUG_LAYER_2
                };
                plain.Items.Add(w);
            }
            {
                var w = new Eagle.wire()
                {
                    x1 = Convert.ToString(x),
                    x2 = Convert.ToString(x),
                    y1 = Convert.ToString(y),
                    y2 = Convert.ToString(y + height),
                    width = Convert.ToString(0.1),
                    layer = pkg.layer == 0 ? DEBUG_LAYER : DEBUG_LAYER_2
                };
                plain.Items.Add(w);
            }


        }

        static void AddBoundingLayers(Eagle.layers layers)
        {
            {
                Eagle.layer l = new Eagle.layer()
                {
                    number = DEBUG_LAYER,
                    name = "tBoundingBox",
                    fill = "1",
                    color = "12",
                    active = Eagle.layerActive.yes,
                    visible = Eagle.layerVisible.no
                };
                layers.layer.Add(l);
            }
            {
                Eagle.layer l = new Eagle.layer()
                {
                    number = DEBUG_LAYER_2,
                    name = "bBoundingBox",
                    fill = "1",
                    color = "14",
                    active = Eagle.layerActive.yes,
                    visible = Eagle.layerVisible.no
                };
                layers.layer.Add(l);
            }

        }
    }
}
