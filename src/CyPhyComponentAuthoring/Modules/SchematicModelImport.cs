using GME.MGA;
using META;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace CyPhyComponentAuthoring.Modules
{
    [CyPhyComponentAuthoringInterpreter.IsCATModule(ContainsCATmethod = true)]
    public class EDAModelImport : CATModule
    {
        #region layout constants

        // constants for adding new parameters to the GME display at the right coordinates
        private const int PARAMETER_START_X = 20;
        private const int PARAMETER_START_Y = 200;
        private const int PARAMETER_ADJUST_Y = 40;
        
        // constants for adding new pins to the GME display at the right coordinates
        private const int PIN_START_X = 450;
        private const int PIN_START_Y = 0;
        private const int PIN_ADJUST_Y = 40;

        // constants for adding new resources to the GME display at the right coordinates
        private const int RESOURCE_START_X = 650;
        private const int RESOURCE_START_Y = 200;

        // constants for adding new CAD models to the GME display at the right coordinates
        private const int MODEL_START_X = 270;
        private const int MODEL_START_Y = 200;

        /// <summary>
        /// A variable for layout control. Reset it to zero each time.
        /// </summary>
        private int greatest_current_y;

        #endregion

        private CyPhyGUIs.GMELogger Logger { get; set; }

        [CyPhyComponentAuthoringInterpreter.CATName(
                NameVal = "Add Eagle Schematic",
                DescriptionVal = "An existing Eagle Schematic model gets imported and associated with this CyPhy component.",
                RoleVal = CyPhyComponentAuthoringInterpreter.Role.Construct,
                IconResourceKey = "EagleOfficialIcon",
                SupportedDesignEntityTypes = CyPhyComponentAuthoringInterpreter.SupportedDesignEntityType.Component
           )
        ]
        public void ImportEagleModel_Delegate(object sender, EventArgs e)
        {
            ImportEagleModel((CyPhy.Component) this.GetCurrentDesignElement(), ((System.Windows.Forms.Control)sender).FindForm());
        }

        private void LogMessage(String message, CyPhyGUIs.GMELogger.MessageType_enum type)
        {
            if (this.Logger == null)
                this.Logger = new CyPhyGUIs.GMELogger(CurrentProj, this.GetType().Name);

            switch (type)
            {
                case CyPhyGUIs.SmartLogger.MessageType_enum.Error:
                    this.Logger.WriteError(message);
                    break;
                case CyPhyGUIs.SmartLogger.MessageType_enum.Warning:
                    this.Logger.WriteWarning(message);
                    break;
                default:
                    this.Logger.WriteInfo(message);
                    break;
            }
        }

        // MOT-347 Method to dispose of the Logger when done with it.
        private void disposeLogger()
        {
            if (null != this.Logger)
            {
                this.Logger.Dispose();
                this.Logger = null;
            }
        }

        [CyPhyComponentAuthoringInterpreter.CATDnD(Extension = ".lbr")]
        [CyPhyComponentAuthoringInterpreter.CATDnD(Extension = ".sch")]
        public void ImportDroppedFile(string filename)
        {
            var deviceList = GetDevicesInEagleModel(filename);

            string device;
            if (deviceList.Count == 1)
            {
                device = deviceList.First();
            }
            else
            {
                var dp = new CyPhyComponentAuthoring.GUIs.EagleDevicePicker(deviceList);
                dp.ShowDialog();

                if (String.IsNullOrWhiteSpace(dp.selectedDevice))
                {
                    LogMessage("Eagle device selection was cancelled.", CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                    disposeLogger();
                    return;
                }
                device = dp.selectedDevice;
            }

            ImportSelectedEagleDevice(device, filename);
        }


        public void ImportEagleModel(CyPhy.Component component, IWin32Window owner=null)
        {
            String eagleFilePath = "";

            // Open file dialog box
            DialogResult dr;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.DefaultExt = "test.mdl";
                ofd.Multiselect = false;
                ofd.Filter = "SCH and LBR files (*.sch, *.lbr)|*.sch;*.lbr|All files (*.*)|*.*";
                dr = ofd.ShowDialog(owner);
                if (dr == DialogResult.OK)
                {
                    eagleFilePath = ofd.FileName;
                }
                else
                {
                    LogMessage("No file was selected. Eagle Schematic Import will not complete.", CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                    disposeLogger();
                    return;
                }
            }

            var deviceList = GetDevicesInEagleModel(eagleFilePath);

            var dp = new CyPhyComponentAuthoring.GUIs.EagleDevicePicker(deviceList);
            dp.ShowDialog();

            if (String.IsNullOrWhiteSpace(dp.selectedDevice))
            {
                LogMessage("Eagle device selection was cancelled.", CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                disposeLogger();
                return;
            }

            ImportSelectedEagleDevice(dp.selectedDevice, eagleFilePath);
        }

        /// <summary>
        /// Gets Eagle component information from an Eagle library
        /// </summary>
        /// <param name="selected">Formatted device name within the Eagle library, such as \\LM139_COMPARATOR\\</param>
        /// <param name="eagleFilePath">Path to the Eagle library (.lbr) file containing the device</param>
        public void ImportSelectedEagleDevice(string selected, string eagleFilePath, CyPhy.Component comp = null)
        {
            if (comp == null) {
                comp = (CyPhy.Component) GetCurrentDesignElement();
            }
            //var selected = dp.selectedDevice;
            var splitSelected = selected.Split('\\');
            var libraryName = splitSelected[0];
            var deviceSetName = splitSelected[1];
            var deviceName = splitSelected[2];
            var deviceXML = GetEagleDevice(eagleFilePath, libraryName, deviceSetName, deviceName);

            var edaModel = ConvertEagleDeviceToAvmEdaModel(deviceXML);

            var cyphyEdaModel = BuildCyPhyEDAModel(edaModel, comp);                  


            #region dump and link EAGLE extract

            // Dump the EAGLE extract
            var compFolderPath = comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE);
            
            var schematicFolderPath = Path.Combine(compFolderPath, "Schematic");
            if (Directory.Exists(schematicFolderPath) == false)
                Directory.CreateDirectory(schematicFolderPath);

            var lbrFileRelPath = Path.Combine("Schematic",
                                              "ecad.lbr");
            int cnt = 1;
            while (File.Exists(Path.Combine(compFolderPath, lbrFileRelPath)))
            {
                lbrFileRelPath = Path.Combine("Schematic",
                                              String.Format("ecad({0}).lbr", cnt++));
            }
            var lbrFileAbsPath = Path.Combine(compFolderPath, lbrFileRelPath);

            var xmlExtract = ExtractStandaloneLibrary(deviceXML);
            xmlExtract.Save(lbrFileAbsPath);

            // Link the EAGLE extract to the CyPhy Component using a Resource.
            var resource = CyPhyClasses.Resource.Create(comp);
            resource.Name = Path.GetFileNameWithoutExtension(lbrFileRelPath);
            resource.Attributes.Path = lbrFileRelPath;
            resource.Attributes.Notes = "Standalone EAGLE library for this device";

            CyPhyClasses.UsesResource.Connect(cyphyEdaModel, resource, null, null, comp);

            // layout Resource just to the side of the Schematic model
            foreach (MgaPart item in (resource.Impl as MgaFCO).Parts)
            {
                item.SetGmeAttrs(null, RESOURCE_START_X, greatest_current_y + RESOURCE_START_Y);
            }

            #endregion

            // Finally, check for blank "library" field.
            // If blank, set it to the basename of the source file.
            // Don't worry; it doesn't really matter.
            if (String.IsNullOrWhiteSpace(cyphyEdaModel.Attributes.Library))
            {
                if (String.IsNullOrWhiteSpace(eagleFilePath))
                {
                    LogMessage("No Eagle library path was selected. Eagle Schematic Import will not complete.", CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                }
                else
                {
                    cyphyEdaModel.Attributes.Library = Path.GetFileNameWithoutExtension(eagleFilePath);
                }
            }
            disposeLogger();
        }

        public CyPhy.EDAModel BuildCyPhyEDAModel(avm.schematic.eda.EDAModel edaModel, CyPhy.Component comp)
        {
            var rf = CyPhyClasses.RootFolder.GetRootFolder(CurrentProj);
            var builder = new AVM2CyPhyML.CyPhyMLComponentBuilder(rf);
            CyPhy.EDAModel cyPhyEDAModel = builder.process(edaModel, comp);


            #region layout

            // find the largest current Y value so our new elements are added below the existing design elements
            greatest_current_y = 0;
            foreach (var child in GetCurrentDesignElement().AllChildren)
            {
                foreach (MgaPart item in (child.Impl as MgaFCO).Parts)
                {
                    int read_x, read_y;
                    string read_str;
                    item.GetGmeAttrs(out read_str, out read_x, out read_y);
                    greatest_current_y = (read_y > greatest_current_y) ? read_y : greatest_current_y;
                }
            }

            // layout CAD model to the "south" of existing elements
            foreach (MgaPart item in (cyPhyEDAModel.Impl as MgaFCO).Parts)
            {
                item.SetGmeAttrs(null, MODEL_START_X, greatest_current_y + MODEL_START_Y);
            }

            #endregion


            ExtendDevicePorts(cyPhyEDAModel);

            return cyPhyEDAModel;
        }

        /// <summary>
        /// Loads an EAGLE document into an XmlDocument structure.
        /// Will throw ArgumentNullException if path is null.
        /// Will throw FileNotFoundException if a file cannot be found at path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static XmlDocument LoadEagleDocument(String path)
        {
            #region Preconditions
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path", "Eagle Schematic Model path cannot be null.");
            }
            if (false == File.Exists(path))
            {
                throw new FileNotFoundException("Eagle Schematic Model cannot be found.", path);
            }
            #endregion

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Ignore
            };
            using (XmlReader reader = XmlReader.Create(path, settings))
            {
                var doc = new XmlDocument();
                doc.Load(reader);

                return doc;
            }
        }

        /// <summary>
        /// Given the path of an EAGLE document, returns a list of devices defined in the file.
        /// Each device will have the form: "library\deviceset\device"
        /// </summary>
        /// <param name="eagleDocPath"></param>
        /// <returns>A list of devices within the EAGLE document</returns>
        public static List<String> GetDevicesInEagleModel(String eagleDocPath)
        {
            var doc = LoadEagleDocument(eagleDocPath);
            var rtn = new List<String>();

            foreach (XmlNode device in doc.SelectNodes("//*[local-name()='device']"))
            {
                var deviceName = (device.Attributes["name"] == null)
                                    ? ""
                                    : device.Attributes["name"].Value;
                var deviceSetName = device.ParentNode.ParentNode.Attributes["name"].Value;

                // Skip elements without package (probably just symbols, like 5V, GND, etc)
                if (device.Attributes["package"] == null)
                    continue;

                var libraryMatches = device.SelectNodes("ancestor::*[local-name()='library']");
                if (libraryMatches == null)
                    continue;
                var library = libraryMatches.Item(0);

                String libraryName = "";
                if (library.Attributes["name"] != null)
                    libraryName = library.Attributes["name"].Value;

                String entry = String.Format("{0}\\{1}\\{2}", libraryName, deviceSetName, deviceName);
                rtn.Add(entry);
            }

            return rtn;
        }

        public static string CreateXpathLiteral(string attributeValue)
        {
            if (!attributeValue.Contains("\""))
            {
                // if we don't have any quotes, then wrap string in quotes...
                return string.Format("\"{0}\"", attributeValue);
            }
            else if (!attributeValue.Contains("'"))
            {
                // if we have some quotes, but no apostrophes, then wrap in apostrophes...
                return string.Format("'{0}'", attributeValue);
            }
            else
            {
                // must use concat so the literal in the XPath will find a match...
                return string.Format("concat(\"{0}\")", attributeValue.Replace("\"", "\",'\"',\""));
            }
        }

        /// <summary>
        /// Given an EAGLE doc path, device set name, and device name,
        /// loads the EAGLE document and returns an XmlNode element corresponding
        /// to the selected device.
        /// </summary>
        /// <param name="eagleDocPath"></param>
        /// <param name="deviceSetName"></param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static XmlNode GetEagleDevice(String eagleDocPath, String libraryName, String deviceSetName, String deviceName)
        {
            var doc = LoadEagleDocument(eagleDocPath);

            var query = String.Format("//*[local-name()='device' {0} and ancestor::*[local-name()='deviceset' and @name = '{1}'] and ancestor::*[local-name()='library' {2}] ]",
                                       String.IsNullOrEmpty(deviceName) 
                                            ? "and (not(@name) or @name='')"
                                            : String.Format("and @name={0}", CreateXpathLiteral(deviceName)),
                                       deviceSetName,
                                       String.IsNullOrEmpty(libraryName)
                                            ? "and (not(@name) or @name='')"
                                            : String.Format("and @name={0}", CreateXpathLiteral(libraryName)));
            var matches = doc.SelectNodes(query);

            if (matches.Count == 0)
                return null;
            if (matches.Count > 1)
                return null;

            return matches.Item(0);
        }

        /// <summary>
        /// Given an EAGLE device, generate an avm.schematic.eda.EDAModel object.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static avm.schematic.eda.EDAModel ConvertEagleDeviceToAvmEdaModel(XmlNode device)
        {
            XmlDocument doc;
            Dictionary<string, XmlNode> packages_Dict;
            Dictionary<string, XmlNode> symbols_Dict;
            XmlNode deviceSet;
            XmlNode library;
            Dictionary<string, XmlNode> gates_Dict;
            String devicePackageName;

            GatherDataFromDocument(device, out doc, out packages_Dict, out symbols_Dict, out deviceSet, out library, out gates_Dict, out devicePackageName);

            // Create a new EDAModel object
            var edaModel = new avm.schematic.eda.EDAModel()
            {
                Device = (device.Attributes["name"] == null)
                            ? ""
                            : device.Attributes["name"].Value,
                Package = device.Attributes["package"].Value,
                DeviceSet = deviceSet.Attributes["name"].Value,
                Library = (library.Attributes["name"] == null)
                            ? ""
                            : library.Attributes["name"].Value
            };

            // check for UserValue.
            // If it exists, create a "value" Parameter object inside the schematic model.
            if (deviceSet.Attributes["uservalue"] != null &&
                deviceSet.Attributes["uservalue"].Value == "yes")
            {
                edaModel.Parameter.Add(
                    new avm.schematic.eda.Parameter()
                    {
                        Locator = "value",
                        Value = new avm.Value()
                        {
                            DataType = avm.DataTypeEnum.String
                        }
                    }
                );
            }

            /* Let's paraphrase the Python script.
             *  
             * foreach (var conn in device.xpath('connects/connect')
             *   // "gate" attribute of <connect> object ("G$1")
             *   var gate = conn.get('gate')
             *   
             *   // "pin" attribute of <connect> object ("P$1")
             *   var pin = conn.get('pin')
             *   
             *   // "pad" attribute of <connect> object ("CATHODE")
             *   var pad = conn.get('pad')
             *   
             *   // A dictionary with gate name as key, then pin & pad pair as the value
             *   my_gates[gate] = [pin, pad]
             *   
             * // Iterate only over the keys of the dict.
             * // That is, only names like "G$1".
             * // (of which there are two)
             * foreach (g in my_gates.keys)
             *   // This will be <gate name="G$1" y="0" x="0" symbol="LED"/>
             *   var gate = gate element from ("gates/gate")
             *   
             *   // This will be <symbol name="LED"> and children
             *   var sym = look up "gate.symbol" in ('symbols/symbol'):name
             *   
             *   // This will be:
             *      <pin visible="off" name="P$1" y="0" x="0" rot="R90" length="short" direction="pas"/>
             *      <pin visible="off" name="P$2" y="7.62" x="0" rot="R270" length="short" direction="pas"/>
             *   var pins = sym.xpath('pin')
             *   
             *   ... layout stuff ...
             *   
             *   for pin in pins:
             *     create some pin objects 
             */

            // Discover what gates are used
            List<String> gatesUsed = new List<String>();
            foreach (XmlNode conn in device.SelectNodes("*[local-name()='connects']/*[local-name()='connect']"))
            {
                gatesUsed.Add(conn.Attributes["gate"].Value);
            }

            // Iterate over the distinct gates used
            foreach (var connGateName in gatesUsed.Distinct())
            {
                var gateDefinition = gates_Dict[connGateName];
                var gateSymbolName = gateDefinition.Attributes["symbol"].Value;
                var gateSymbol = symbols_Dict[gateSymbolName];

                foreach (XmlNode pin in gateSymbol.SelectNodes("*[local-name()='pin']"))
                {
                    edaModel.Pin.Add(
                        new avm.schematic.Pin()
                        {
                            EDAGate = connGateName,
                            EDASymbolLocationX = pin.Attributes["x"].Value,
                            EDASymbolLocationY = pin.Attributes["y"].Value,
                            EDASymbolRotation = (pin.Attributes["rot"] == null)
                                                ? "0"
                                                : pin.Attributes["rot"].Value,
                            Name = pin.Attributes["name"].Value
                        }
                    );
                }
            }

            return edaModel;
        }

        /// <summary>
        /// Given an EAGLE device, generate a standalone LBR XML file that captures
        /// only the data needed to represent this device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static XmlDocument ExtractStandaloneLibrary(XmlNode device)
        {
            XmlDocument doc;
            Dictionary<string, XmlNode> packages_Dict;
            Dictionary<string, XmlNode> symbols_Dict;
            XmlNode deviceSet;
            XmlNode library;
            Dictionary<string, XmlNode> gates_Dict;
            String devicePackageName;

            GatherDataFromDocument(device, out doc, out packages_Dict, out symbols_Dict, out deviceSet, out library, out gates_Dict, out devicePackageName);

            var outDoc = new XmlDocument();
            var root = (XmlElement)outDoc.AppendChild(outDoc.CreateElement("eagle"));
            root.SetAttribute("version", "6.5.0");
            root.SetAttribute("xmlns", "eagle");

            var resdraw = (XmlElement)root.AppendChild(outDoc.CreateElement("drawing"));
            var reslib = (XmlElement)resdraw.AppendChild(outDoc.CreateElement("library"));

            var respkgs = (XmlElement)reslib.AppendChild(outDoc.CreateElement("packages"));
            respkgs.InnerXml = packages_Dict[devicePackageName].OuterXml;

            var ressyms = (XmlElement)reslib.AppendChild(outDoc.CreateElement("symbols"));
            var resdsets = (XmlElement)reslib.AppendChild(outDoc.CreateElement("devicesets"));

            var resdset = (XmlElement)resdsets.AppendChild(outDoc.CreateElement("deviceset"));
            resdset.SetAttribute("name", deviceSet.Attributes["name"].Value);

            if (deviceSet.Attributes["prefix"] != null
                && !String.IsNullOrWhiteSpace(deviceSet.Attributes["prefix"].Value))
            {
                resdset.SetAttribute("prefix", deviceSet.Attributes["prefix"].Value);
            }

            var uv = (deviceSet.Attributes["uservalue"] == null)
                        ? ""
                        : deviceSet.Attributes["uservalue"].Value;
            if (!String.IsNullOrWhiteSpace(uv))
                resdset.SetAttribute("uservalue", uv);

            var resgates = (XmlElement)resdset.AppendChild(outDoc.CreateElement("gates"));
            var resdevs = (XmlElement)resdset.AppendChild(outDoc.CreateElement("devices"));
            resdevs.InnerXml = device.OuterXml;

            // Discover what gates are used
            List<String> gatesUsed = new List<String>();
            foreach (XmlNode conn in device.SelectNodes("*[local-name()='connects']/*[local-name()='connect']"))
            {
                gatesUsed.Add(conn.Attributes["gate"].Value);
            }

            var alreadyUsedSymbolNames = new List<string>();    // MOT-549

            // Iterate over the distinct gates used
            foreach (var connGateName in gatesUsed.Distinct())
            {
                var gateName = connGateName;
                var gate = gates_Dict[gateName];
                resgates.AppendChild(CloneElementToNewDoc((XmlElement)gate, outDoc));

                var symName = gate.Attributes["symbol"].Value;
                
                // Only add symbols if their name hasn't already been added.  MOT-549
                if (!alreadyUsedSymbolNames.Contains(symName))
                {
                    var sym = symbols_Dict[symName];
                    ressyms.AppendChild(CloneElementToNewDoc((XmlElement)sym, outDoc));
                    alreadyUsedSymbolNames.Add(symName);
                }
            }

            return outDoc;
        }

        private static XmlElement CloneElementToNewDoc(XmlElement element, XmlDocument newDoc)
        {
            var rtn = newDoc.CreateElement(element.Name);
            rtn.InnerXml = element.InnerXml;

            foreach (XmlAttribute attr in element.Attributes)
            {
                rtn.SetAttribute(attr.Name, attr.Value);
            }

            return rtn;
        }

        /// <summary>
        /// Given an EAGLE device, mine important data fields from its XML document.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="doc"></param>
        /// <param name="packages_Dict"></param>
        /// <param name="symbols_Dict"></param>
        /// <param name="deviceSet"></param>
        /// <param name="library"></param>
        /// <param name="gates_Dict"></param>
        /// <param name="devicePackageName"></param>
        private static void GatherDataFromDocument(XmlNode device, out XmlDocument doc, out Dictionary<string, XmlNode> packages_Dict, out Dictionary<string, XmlNode> symbols_Dict, out XmlNode deviceSet, out XmlNode library, out Dictionary<string, XmlNode> gates_Dict, out String devicePackageName)
        {
            // Get document-wide stuff that we'll need.
            doc = device.OwnerDocument;

            var packages = doc.SelectNodes("//*[local-name()='packages']/*[local-name()='package']");
            packages_Dict = new Dictionary<String, XmlNode>();
            foreach (XmlNode pkg in packages)
            {
                packages_Dict[pkg.Attributes["name"].Value] = pkg;
            }

            symbols_Dict = new Dictionary<String, XmlNode>();
            foreach (XmlNode symbol in doc.SelectNodes("//*[local-name()='symbols']/*[local-name()='symbol']"))
            {
                symbols_Dict[symbol.Attributes["name"].Value] = symbol;
            }

            // Get this device's ancestors.
            deviceSet = device.ParentNode.ParentNode;
            library = deviceSet.ParentNode.ParentNode;

            // Get gates that are part of this deviceSet.
            gates_Dict = new Dictionary<String, XmlNode>();
            foreach (XmlNode gate in deviceSet.SelectNodes("*[local-name()='gates']/*[local-name()='gate']"))
            {
                gates_Dict[gate.Attributes["name"].Value] = gate;
            }

            devicePackageName = device.Attributes["package"].Value;
        }


        /// <summary>
        /// Given a CyPhy EDAModel, create copies of its ports at the
        /// Component level, and connect them to the EDAModel's ports.
        /// Do the same for the EDAModel's parameters.
        /// </summary>
        /// <param name="model"></param>
        public void ExtendDevicePorts(CyPhy.EDAModel model)
        {
            var component = CyPhyClasses.Component.Cast(model.ParentContainer.Impl);

            int num_pins_in_schematic_model = model.Children.SchematicModelPortCollection.Count();
            int num_rows_in_schematic_model = num_pins_in_schematic_model / 2;
            int pad_to_get_under_schematic_model = 100 + num_rows_in_schematic_model * 15;

            int num_parsed_pins = 0;

            // Use a dictionary to track duplicate pin names, MOT-514
            Dictionary<string,List<CyPhy.SchematicModelPort>> pinDict = new Dictionary<string, List<CyPhy.SchematicModelPort>>();

            foreach (var pin in model.Children.SchematicModelPortCollection)
            {
                var newPin = CyPhyClasses.SchematicModelPort.Create(component);
                newPin.Name = pin.Name;

                newPin.Attributes.Definition = pin.Attributes.Definition;
                newPin.Attributes.DefinitionNotes = pin.Attributes.DefinitionNotes;
                newPin.Attributes.EDAGate = pin.Attributes.EDAGate;
                newPin.Attributes.InstanceNotes = pin.Attributes.InstanceNotes;
                newPin.Attributes.EDASymbolLocationX = pin.Attributes.EDASymbolLocationX;
                newPin.Attributes.EDASymbolLocationY = pin.Attributes.EDASymbolLocationY;
                newPin.Attributes.EDASymbolRotation = pin.Attributes.EDASymbolRotation;

                CyPhyClasses.PortComposition.Connect(pin,
                                                     newPin,
                                                     null,
                                                     null,
                                                     component);

                // Add the pin to the dictionary's list, MOT-514
                if( !pinDict.ContainsKey( newPin.Name ) )
                {
                    List<CyPhy.SchematicModelPort> tmpList = new List<CyPhy.SchematicModelPort>();
                    pinDict.Add(newPin.Name, tmpList);
                }
                pinDict[newPin.Name].Add(newPin);

                foreach (MgaPart item in (newPin.Impl as MgaFCO).Parts)
                {
                    int original_x = 0;
                    int original_y = 0;
                    foreach (MgaPart item2 in (pin.Impl as MgaFCO).Parts)
                    {
                        String icon;
                        item2.GetGmeAttrs(out icon, out original_x, out original_y);
                        break;
                    }

                    int new_x = original_x;
                    int new_y = greatest_current_y + MODEL_START_Y + pad_to_get_under_schematic_model + original_y;

                    item.SetGmeAttrs(null, new_x, new_y);
                }
                num_parsed_pins++;
            }

            // Try to disambiguate any duplicate pin names, MOT-514.
            foreach (KeyValuePair<string, List<CyPhy.SchematicModelPort>> entry in pinDict)
            {
                if (entry.Value.Count > 1)
                {
                    foreach (CyPhy.SchematicModelPort pin in entry.Value)
                    {
                        pin.Name += ("_" + pin.Attributes.EDAGate);
                    }
                }
            }

            int num_parsed_params = 0;
            foreach (var param in model.Children.EDAModelParameterCollection)
            {
                var newProp = CyPhyClasses.Property.Create(component);
                newProp.Name = param.Name;
                newProp.Attributes.Value = param.Attributes.Value;

                CyPhyClasses.EDAModelParameterMap.Connect(newProp,
                                                          param,
                                                          null,
                                                          null,
                                                          component);

                // - Perform some layout "niceification" on the resulting objects. 
                foreach (MgaPart item in (newProp.Impl as MgaFCO).Parts)
                {
                    item.SetGmeAttrs(null, PARAMETER_START_X, greatest_current_y + PARAMETER_START_Y + (num_parsed_params * PARAMETER_ADJUST_Y));
                }
                num_parsed_params++;
            }
        }
    }
}
