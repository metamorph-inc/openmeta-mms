using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using CyPhyComponentAuthoring;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using META;
using System.Xml.Serialization;
using GME.MGA;
using SystemCParser;
using SystemCAttributesClass = ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass;



namespace CyPhyComponentAuthoring.Modules
{
    [CyPhyComponentAuthoringInterpreter.IsCATModule(ContainsCATmethod=true)]
    public class SystemCModelImport : CATModule
    {
        ScParse parsedInfo;

        // Pixel spacings for pins outside the SystemC model
        private const int SCHEMATIC_PIN_X_OFFSET = 100;
        private const int SCHEMATIC_PIN_X_SPACING = 700;
        private const int SCHEMATIC_PIN_Y_OFFSET = -75;
        private const int SCHEMATIC_PIN_Y_SPACING = 75;

        // Pixel spacings for pins within the SystemC model
        private const int PIN_X_OFFSET = 50;
        private const int PIN_X_SPACING = 300;
        private const int PIN_Y_OFFSET = 50;
        private const int PIN_Y_SPACING = 100;

        // Module-level variables
        private int m_startX;     // Visual X coordinate for the new SystemC model, relative to the left of the component window.
        private int m_startY;     // Visual Y coordinate for the new SystemC model, relative to the top of the component window.
        private int m_numberOfPinsPerColumn;  // The maximum number of pins placed in a vertical column.

        private CyPhyGUIs.GMELogger Logger { get; set; }
        
        [CyPhyComponentAuthoringInterpreter.CATName(
                NameVal = "Add SystemC Model",
                DescriptionVal = "An existing SystemC model gets imported and associated with this CyPhy component.",
                RoleVal = CyPhyComponentAuthoringInterpreter.Role.Construct,
                SupportedDesignEntityTypes = CyPhyComponentAuthoringInterpreter.SupportedDesignEntityType.Component
           )
        ]
        public void ImportSystemCModel_Delegate(object sender, EventArgs e)
        {
            ImportSystemCModel((CyPhy.Component) this.GetCurrentDesignElement());
        }

        public void ImportSystemCModel(CyPhy.Component component, String[] path_SystemCFiles = null)
        {
            Boolean ownLogger = false;
            String path_systemCHeaderFile = null;
            String path_systemCSourceFile = null;

            if (Logger == null)
            {
                ownLogger = true;
                Logger = new CyPhyGUIs.GMELogger(component.Impl.Project, "SystemCModelImport");
            }

            if (path_SystemCFiles == null)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.CheckFileExists = true;
                    ofd.DefaultExt = "*.h";
                    ofd.Multiselect = true;
                    //ofd.Filter = "AVM design files and packages (*.adm;*.adp)|*.adm;*.adp|All files (*.*)|*.*";

                    ofd.Filter = "SystemC (*.h;*.cpp)|*.h;*.cpp|All files (*.*)|*.*";
                    DialogResult dr = ofd.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        path_SystemCFiles = ofd.FileNames;
                    }
                    else
                    {
                        Logger.WriteError("No file was selected. SystemC Model Import will not complete.");

                        if (ownLogger)
                        {
                            Logger.Dispose();
                            Logger = null;
                        }

                        return;
                    }
                }
            }

            // Check that the selected files are OK.
            bool needExit = false;

            if (path_SystemCFiles.Length != 2)
            {
                Logger.WriteError("Two SystemC files need to be selected: a \".h\" file, and a \".cpp\" file." );
                needExit = true;
            }

            foreach( var fileName in path_SystemCFiles )
            {
                if (String.IsNullOrWhiteSpace(fileName))
                {
                    Logger.WriteError("SystemC Model Path of \"{0}\" isn't valid.", fileName);
                    needExit = true;
                }
                    
                switch( Path.GetExtension( fileName ) )
                {
                    case ".h":
                        path_systemCHeaderFile = fileName;
                        break;

                    case ".cpp":
                        path_systemCSourceFile = fileName;
                        break;

                    default:
                        Logger.WriteError("SystemC Model Path of \"{0}\" has an unexpected file extension.", fileName);
                        needExit = true;
                        break;
                }
            }

            if( null == path_systemCHeaderFile )
            {
                Logger.WriteError("Missing the SystemC Model \".h\" file." );
                needExit = true;
            }

            if( null == path_systemCSourceFile )
            {
                Logger.WriteError("Missing the SystemC Model \".cpp\" file." );
                needExit = true;
            }

            if ((!needExit) &&
                (Path.GetFileNameWithoutExtension(path_systemCHeaderFile) != Path.GetFileNameWithoutExtension(path_systemCSourceFile)))
            {
                Logger.WriteWarning("The \".h\" and \".cpp\" file names don't match.");
            }

            if( needExit )
            {
                if (ownLogger)
                {
                    Logger.Dispose();
                    Logger = null;
                }

                return;
            }

            try
            {
                Uncomment nocomment = new Uncomment(path_systemCHeaderFile);
                parsedInfo = new ScParse( nocomment.result );
            }
            catch( Exception e )
            {
                Logger.WriteError("Error parsing '{0}': {1}", path_systemCHeaderFile, e.Message);

                if (ownLogger)
                {
                    Logger.Dispose();
                    Logger = null;
                }

                return;
            }

            //-----------------------------------------------------------------------------------
            //
            //  At this point, the SystemC model header file has been parsed and module info extracted.
            //
            //  'parsedInfo' has the SystemC module information.
            //  'component' is the GME component that the SystemC info will be added to.
            //  'path_SystemCFile' is the complete path and filename of the SystemC module's ".h" file.
            //
            //------------------------------------------------------------------------------------


            // Find the visual coordinates of where the new SystemC model should be placed.
            getNewModelInitialCoordinates(component, out m_startX, out m_startY);

            //------------- Setup the ports

            //------------------------------------------------------------
            // Create a SystemCModel child
            CyPhy.SystemCModel newModel = CyPhyClasses.SystemCModel.Create(component);
            newModel.Name = parsedInfo.scModuleName + "_SPICEModel";
            newModel.Attributes.Notes = "Created using SystemC-Model CAT Module";
            newModel.Attributes.ModuleName = parsedInfo.scModuleName;

            Logger.WriteInfo("Created a new SystemCModel: \"{0}\"", GetHyperlinkStringFromComponent(newModel.Name, newModel.Impl as IMgaFCO));

            // Adjust the SystemC model's label length to allow complete port names to be seen.
            newModel.Preferences.PortLabelLength = 0;

            // Set the SystemC model's visual position.
            setFCOPosition(newModel.Impl as MgaFCO, m_startX, m_startY);

            // Create new ports within the SystemC model, and add them to a SystemC-port dictionary
            Dictionary<string, CyPhy.SystemCPort> SystemCPortNameDictionary = CreatePortsWithinTheSystemCModelForEachSystemCPort(parsedInfo, newModel);

            // Arrange the SystemC model's pins into columns.
            Dictionary<string, int[]> SystemCPortRowColDictionary = ArrangeSystemCModelPinsIntoRowsAndCols(parsedInfo, SystemCPortNameDictionary);

            // Get the visual location to place the SystemC-model header-file resource.
            int SystemCHeaderFileResourceX = m_startX + 200;
            int SystemCHeaderFileResourceY = m_startY - 100;

            // Get the visual location to place the SystemC-model source-file resource.
            int SystemCSourceFileResourceX = SystemCHeaderFileResourceX + 100;
            int SystemCSourceFileResourceY = SystemCHeaderFileResourceY;

            string subdirectory = "SystemC";  // The subdirectory where the SystemC file will be copied.

            //  - Copy the SystemC Model files into the component's back-end folder

            string verbose = "";    // String used for exception debugging.
            string destSourceFileName = "";   // Name of the destination SystemC source file after possible modification during copying.
            string destHeaderFileName = "";   // Name of the destination SystemC header file after possible modification during copying.
            try
            {
                CopySystemCFile(component, path_systemCHeaderFile, subdirectory, out destHeaderFileName, out verbose);
                CopySystemCFile(component, path_systemCSourceFile, subdirectory, out destSourceFileName, out verbose);
            }
            catch (Exception e)
            {
                Logger.WriteError("{1}Exception copying SystemC file: {0}", e.Message, verbose);
                return;
            }

            // Create the SystemC-model-file resources
            CreateSystemCModelFileResources(
                component,
                subdirectory,

                destHeaderFileName,
                destSourceFileName,

                path_systemCHeaderFile,
                path_systemCSourceFile,

                SystemCHeaderFileResourceX,
                SystemCHeaderFileResourceY,

                SystemCSourceFileResourceX,
                SystemCSourceFileResourceY,
                newModel);


            if (ownLogger)
            {
                Logger.Dispose();
                Logger = null;
            }
        }

        /// <summary>
        /// Gets a SystemC DataTypeEnum from a string
        /// </summary>
        /// <param name="dataTypeString">the string to convert</param>
        /// <returns></returns>
        private SystemCAttributesClass.DataType_enum getSystemCDataTypeEnumFromString(string dataTypeString)
        {
            SystemCAttributesClass.DataType_enum rVal = SystemCAttributesClass.DataType_enum.@bool;
            switch (dataTypeString)
            {
                case "sc_bit":
                    rVal = SystemCAttributesClass.DataType_enum.sc_bit;
                    break;
                case "sc_int":
                    rVal = SystemCAttributesClass.DataType_enum.sc_int;
                    break;
                case "sc_logic":
                    rVal = SystemCAttributesClass.DataType_enum.sc_logic;
                    break;
                case "sc_uint":
                    rVal = SystemCAttributesClass.DataType_enum.sc_uint;
                    break;
                case "bool":
                    rVal = SystemCAttributesClass.DataType_enum.@bool;
                    break;
                default:
                    Logger.WriteInfo("dataTypeString: \"{0}\" didn't match.", dataTypeString );
                    break;
            }
            return rVal;
        }


        /// <summary>
        /// Gets a SystemC DirectionalityEnum from a string
        /// </summary>
        /// <param name="dataTypeString">the string to convert</param>
        /// <returns></returns>
        private SystemCAttributesClass.Directionality_enum getSystemCDirectionalityEnumFromString(string dataDirectionString)
        {
            SystemCAttributesClass.Directionality_enum rVal = SystemCAttributesClass.Directionality_enum.not_applicable;
            switch (dataDirectionString)
            {
                case "sc_in":
                    rVal = SystemCAttributesClass.Directionality_enum.@in;
                    break;
                case "sc_inout":
                    rVal = SystemCAttributesClass.Directionality_enum.inout;
                    break;
                case "not_applicable":
                    rVal = SystemCAttributesClass.Directionality_enum.not_applicable;
                    break;
                case "sc_out":
                    rVal = SystemCAttributesClass.Directionality_enum.@out;
                    break;

                default:
                    Logger.WriteInfo("dataDirectionString: \"{0}\" didn't match.", dataDirectionString);
                    break;
            }
            return rVal;
        }

        /// <summary>
        /// Creates a SystemCPort within the SystemC-model element for each SystemC pin.
        /// </summary>
        /// <param name="ci">Parsed SystemC-file info about the SystemC subcircuit or model.</param>
        /// <param name="newModel">The SystemC-model element.</param>
        /// <returns>A mapping of SystemC pin names to SchematicModelPorts internal to the SystemC-model element.</returns>
        private Dictionary<string, CyPhy.SystemCPort> CreatePortsWithinTheSystemCModelForEachSystemCPort(ScParse ci, CyPhy.SystemCModel newModel)
        {
            Dictionary<string, CyPhy.SystemCPort> SystemCPortNameDictionary = new Dictionary<string, CyPhy.SystemCPort>();

            foreach (var listElement in parsedInfo.pinList)
            {
                // Create a new port within the SystemC Model.
                CyPhy.SystemCPort newPort = CyPhyClasses.SystemCPort.Create(newModel);
                newPort.Name = listElement.name;
                newPort.Attributes.DataType = getSystemCDataTypeEnumFromString(listElement.type);
                newPort.Attributes.DataTypeDimension = listElement.dimension;
                newPort.Attributes.Directionality = getSystemCDirectionalityEnumFromString( listElement.direction );

                try
                {
                    // Add the new port to the SystemC port-name dictionary.
                    SystemCPortNameDictionary.Add(newPort.Name, newPort);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("WARNING! Exception adding port '{0}' to SystemCPortNameDictionary: {1}",
                        GetHyperlinkStringFromComponent(newPort.Name, newPort.Impl as IMgaFCO),
                        e.Message);
                }
            }
            return SystemCPortNameDictionary;
        }


        /// <summary>
        /// Arrange SystemC-model pins into rows and columns.
        /// </summary>
        /// <param name="ci">The parsed SystemC subcircuit or model info.</param>
        /// <param name="SystemCPortNameDictionary">Maps SystemC pin names to SystemC pins.</param>
        /// <returns>A dictionary mapping pin names to an array containing their row number and column number.</returns>
        ///


        private Dictionary<string, int[]> ArrangeSystemCModelPinsIntoRowsAndCols(ScParse parsedInfo, Dictionary<string, CyPhy.SystemCPort> SystemCPortNameDictionary)
        {
            /// We want the pure input pins to end up on the left, and the pure output pins to end up on the right.
            /// Any other pins should go on the side with the least pins.
            List<string> scInPortNames = new List<string>();
            List<string> scOutPortNames = new List<string>();
            List<string> scMiscPortNames = new List<string>();

            foreach (var port in parsedInfo.pinList)
            {
                if (port.direction == "sc_in")
                {
                    scInPortNames.Add(port.name);
                }
                else if (port.direction == "sc_out")
                {
                    scOutPortNames.Add(port.name);
                }
                else
                {
                    scMiscPortNames.Add(port.name);
                }
            }

            scInPortNames.Sort(ComparePinsUsingNaturalNumericOrder);
            scOutPortNames.Sort(ComparePinsUsingNaturalNumericOrder);
            scMiscPortNames.Sort(ComparePinsUsingNaturalNumericOrder);

            // Get the starting row, col for each list of port names.
            int[] scInPortNamesOffset = { 0, 0 }; // The start of the left column
            int[] scOutPortNamesOffset = { 0, 1 };  // the start of the right column
            int miscRow = 0;
            int miscCol = 0;
            if( scInPortNames.Count < scOutPortNames.Count )
            {
                // Put misc on the left;
                miscCol = 0;
                miscRow = scInPortNames.Count + 1;
            }
            else
            {
                // Put misc on the right;
                miscCol = 1;
                miscRow = scOutPortNames.Count + 1;

            }
            int[] scMiscPortNamesOffset = { miscCol, miscRow };

            // Keep track of the rows and columns associated with each of the SystemC ports, for positioning.
            Dictionary<string, int[]> scPortRowColDictionary = new Dictionary<string, int[]>();
            int rowCount = 0;
            foreach (string portName in scInPortNames)
            {
                var port = SystemCPortNameDictionary[portName];

                // Figure out what row and column this port is in.
                int portCol = 0;
                int portRow = rowCount++;

                // Figure out the visual positioning of this pin within the SystemC model window.
                int pinX = PIN_X_OFFSET + (portCol * PIN_X_SPACING);
                int pinY = PIN_Y_OFFSET + (portRow * PIN_Y_SPACING);

                // Set the pin's GUI coordinates in all aspects.
                setFCOPosition(port.Impl as MgaFCO, pinX, pinY);

                // Keep track of the row and column for later positioning.
                int[] rowCol = { portRow, portCol };
                try
                {
                    scPortRowColDictionary.Add(portName, rowCol);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("WARNING! Exception adding port '{0}' to SystemCPortRowColDictionary: {1}",
                        GetHyperlinkStringFromComponent(portName, port.Impl as IMgaFCO),
                        e.Message);
                }
            }

            rowCount = 0;
            foreach (string portName in scOutPortNames)
            {
                var port = SystemCPortNameDictionary[portName];

                // Figure out what row and column this port is in.
                int portCol = 1;
                int portRow = rowCount++;

                // Figure out the visual positioning of this pin within the SystemC model window.
                int pinX = PIN_X_OFFSET + (portCol * PIN_X_SPACING);
                int pinY = PIN_Y_OFFSET + (portRow * PIN_Y_SPACING);

                // Set the pin's GUI coordinates in all aspects.
                setFCOPosition(port.Impl as MgaFCO, pinX, pinY);

                // Keep track of the row and column for later positioning.
                int[] rowCol = { portRow, portCol };
                try
                {
                    scPortRowColDictionary.Add(portName, rowCol);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("WARNING! Exception adding port '{0}' to SystemCPortRowColDictionary: {1}",
                        GetHyperlinkStringFromComponent(portName, port.Impl as IMgaFCO),
                        e.Message);
                }
            }

            rowCount = 0;
            foreach (string portName in scMiscPortNames)
            {
                var port = SystemCPortNameDictionary[portName];

                // Figure out what row and column this port is in.
                int portCol = miscCol;
                int portRow = miscRow + rowCount++;

                // Figure out the visual positioning of this pin within the SystemC model window.
                int pinX = PIN_X_OFFSET + (portCol * PIN_X_SPACING);
                int pinY = PIN_Y_OFFSET + (portRow * PIN_Y_SPACING);

                // Set the pin's GUI coordinates in all aspects.
                setFCOPosition(port.Impl as MgaFCO, pinX, pinY);

                // Keep track of the row and column for later positioning.
                int[] rowCol = { portRow, portCol };
                try
                {
                    scPortRowColDictionary.Add(portName, rowCol);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("WARNING! Exception adding port '{0}' to SystemCPortRowColDictionary: {1}",
                        GetHyperlinkStringFromComponent(portName, port.Impl as IMgaFCO),
                        e.Message);
                }
            }

            // Show the rows and columns of the pins
            Logger.WriteDebug("Port names (row,col):");
            foreach (var entry in scPortRowColDictionary)
            {
                Logger.WriteDebug("   '{0}' ({1}, {2})",
                    GetHyperlinkStringFromComponent(entry.Key, SystemCPortNameDictionary[entry.Key].Impl as IMgaFCO),
                    entry.Value[0],
                    entry.Value[1]);
            }
            return scPortRowColDictionary;
        }


        /// <summary>
        /// Creates a SystemC model file resource (SMFR) and connects it to the SystemC model element.
        /// </summary>
        /// <param name="component">The component that will contain the SMFR.</param>
        /// <param name="subdirectory">The subdirectory the SystemC file has been copied into.</param>
        /// <param name="destFileName">The file name of the copied SystemC file.</param>
        /// <param name="path_SystemCFile">Path and file name of the source SystemC file.</param>
        /// <param name="startX">The X position to place the SMFR<./param>
        /// <param name="startY">The Y position to place the SMFR.</param>
        /// <param name="newModel">The SystemC model the SMFR will be connected to.</param>
        /// <returns></returns>
        /// 

        private string CreateSystemCModelFileResources(
            CyPhy.Component component, 
            string subdirectory,
            string destHeaderFileName,
            string destSourceFileName,
            string path_systemCHeaderFile,
            string path_systemCSourceFile,
            int SystemCHeaderFileResourceX, 
            int SystemCHeaderFileResourceY, 
            int SystemCSourceFileResourceX, 
            int SystemCSourceFileResourceY, 
            CyPhy.SystemCModel newModel)
        {
            // First, do the SystemC header file.
            CyPhy.Resource scHeaderResource = CyPhyClasses.Resource.Create(component);
            scHeaderResource.Name = "SystemCModelHeaderFile";
            scHeaderResource.Attributes.Notes = string.Format("Derived from '{0}'.\n", path_systemCHeaderFile);
            scHeaderResource.Attributes.Path = Path.Combine(subdirectory, destHeaderFileName);

            // Set the SystemC-model-header-file resource's GUI coordinates in all aspects.
            setFCOPosition(scHeaderResource.Impl as MgaFCO, SystemCHeaderFileResourceX, SystemCHeaderFileResourceY);

            // Create a connection between the SystemC model-header file resource and the SystemCModel.
            CyPhyClasses.UsesResource.Connect(scHeaderResource, newModel, null, null, component);

            //////////////////////////////////////
            //
            //  Next, do the SystemC source file.
            //
            //////////////////////////////////////

            CyPhy.Resource scSourceResource = CyPhyClasses.Resource.Create(component);
            scSourceResource.Name = "SystemCModelSourceFile";
            scSourceResource.Attributes.Notes = string.Format("Derived from '{0}'.\n", path_systemCSourceFile);
            scSourceResource.Attributes.Path = Path.Combine(subdirectory, destSourceFileName);

            // Set the SystemC-model-source-file resource's GUI coordinates in all aspects.
            setFCOPosition(scSourceResource.Impl as MgaFCO, SystemCSourceFileResourceX + 200, SystemCSourceFileResourceY);

            // Create a connection between the SystemC model-source file resource and the SystemCModel.
            CyPhyClasses.UsesResource.Connect(scSourceResource, newModel, null, null, component);
            return subdirectory;
        }

        /// <summary>
        /// Copy a SystemC file into a component's subdirectory.
        /// </summary>
        /// <param name="component">The component that is getting the SystemC file.</param>
        /// <param name="path_SystemCFile">The full path to the input SystemC (".cir") file, including the file name.</param>
        /// <param name="subdirectory">The subdirectory where the SystemC file will be copied, typically "SystemC".</param>
        /// <param name="destFileName">The name of the SystemC file after it has been uniquely copied.  (See MOT-221).</param>
        /// <param name="verbose">String used for exception debugging.</param>
        /// <remarks>May throw an exception from the file system.</remarks>
        private void CopySystemCFile(
            CyPhy.Component component, 
            String path_scFile, 
            string subdirectory, 
            out string destFileName, 
            out string verbose)
        {
            // used in creating the resource object below
            string PathforComp = null;
            verbose = "Begin CopySystemCFile()\n"; // Used for debugging.

            // create the destination path
            verbose += "step 1\n";
            PathforComp = META.ComponentLibraryManager.EnsureComponentFolder(component);
            verbose += "step 2\n";

            PathforComp = component.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE);
            verbose += "step 3\n";

            PathforComp = PathforComp.Replace('/', Path.DirectorySeparatorChar);

            string finalPathName = Path.Combine(PathforComp, subdirectory);
            verbose += "step 4\n";

            if (Directory.Exists(finalPathName) == false)
            {
                Directory.CreateDirectory(finalPathName);
            }

            verbose += string.Format("Created directory '{0}' OK.\n", finalPathName);


            destFileName = Path.GetFileName(path_scFile);

            // Find a file name that doesn't already exist, MOT-221.
            string destinationSpiceFilePathAndName = Path.Combine(finalPathName, destFileName);
            int cnt = 1;

            // Make sure the named file doesn't already exist for MOT-221.
            while (File.Exists(destinationSpiceFilePathAndName))
            {
                Logger.WriteInfo("File {0} already exists.", destinationSpiceFilePathAndName);
                destFileName = Path.GetFileNameWithoutExtension(path_scFile) + "_" + (cnt++) + Path.GetExtension(path_scFile);
                destinationSpiceFilePathAndName = Path.Combine(finalPathName, destFileName);
            }
 
            verbose += string.Format("About to copy file '{0}' to '{1}'.\n",
               path_scFile, destinationSpiceFilePathAndName);
            System.IO.File.Copy(path_scFile, destinationSpiceFilePathAndName, true);
            Logger.WriteInfo("Copied file \"{0}\" to \"{1}\".", path_scFile, destinationSpiceFilePathAndName);
        }

        /// <summary>
        /// Set a First Class Object's visual position in all aspects.
        /// </summary>
        /// <param name="myFCO">The FCO whose position will be set. </param>
        /// <param name="x">The X coordinate to be set. (0 on the left, increasing to the right)</param>
        /// <param name="y">The Y coordinate. (0 at the top, increasing down)</param>

        public void setFCOPosition(MgaFCO myFCO, int x, int y)
        {
            // Set the FCO's coordinates in all aspects.
            foreach (MgaPart item in (myFCO).Parts)
            {
                item.SetGmeAttrs(null, x, y);   // The icon string is null.
            }
        }


        /// <summary>
        /// Returns the initial (x,y) coordinates for a new model that will be added to a component.
        /// </summary>
        /// <param name="component">The component the new model willbe added to</param>
        /// <param name="x">The X coordinate the new model should use.</param>
        /// <param name="y">The Y coordinate the new model should use.</param>
        public void getNewModelInitialCoordinates(CyPhy.Component component, out int x, out int y)
        {
            const int MODEL_START_X = 350;  // Always start new models at x = 650.
            const int MODEL_START_Y = 200;  // Y offset visually below the lowest element already in the component.
            x = MODEL_START_X;
            y = getGreatestCurrentY(component) + MODEL_START_Y;
        }

        /// <summary>
        /// Finds the largest current GUI-positioning Y value currently used within a component.
        /// </summary>
        /// <remarks>
        /// We use this as part of the calculation of where to put our newly-created elements on screen.
        /// We want the new ones to be added below the existing design elements, so they won't
        /// appear to be overlapping other stuff.
        /// Y is zero at the top of the window, and increases in the downward direction.
        /// </remarks>
        /// <param name="component">The CyPhy.Component that will be analyzed.</param>
        /// <returns>The largest current Y value used by the component.</returns>
        /// <see>Based on code in SchematicModelImport.cs</see>
        public int getGreatestCurrentY( CyPhy.Component component )
        {
            int rVal = 0;
            // The children of the component may be things like schematic models, ports, etc.
            foreach (var child in component.AllChildren)
            {
                foreach (MgaPart item in (child.Impl as MgaFCO).Parts)
                {
                    // Each Parts/item has info corresponding to placement on a GME aspect,
                    // where each aspect is like a separate, but overlapping canvas.
                    // Although components may be moved to different places in each aspect,
                    // we'd like to create them so they start off at the same place in
                    // each aspect.  Otherwise, it disorients the user when they switch aspects.
                    // That's why we check all aspects to get a single maximum Y, so
                    // the newly created stuff won't be overlapping in any of its aspects.

                    int x, y;
                    string read_str;
                    item.GetGmeAttrs(out read_str, out x, out y);
                    rVal = (y > rVal) ? y : rVal;
                }
            }
            return rVal;
        }
        /// <summary>
        /// Compares two pin-name strings using natural numeric order, so 'Pin10' comes after 'Pin9'.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The seconds string.</param>
        /// <returns>
        /// 0, if both strings match;
        /// a number greater than 0, if the first string is greater than the second;
        /// a number less than 0, if the first string is less than the second.
        /// </returns>
        private static int ComparePinsUsingNaturalNumericOrder(string x, string y)
        {
            // Corner cases with null strings:
            if ((x == null) && (y == null))
            {
                return 0;
            }
            if (x == null)
            {
                // If x is null and y is not null, y 
                // is greater.  
                return -1;
            }
            if (y == null)
            {
                // If y is null and x is not null, x 
                // is greater.  
                return 1;
            }

            // Now, both X and Y are not null.
            // Check for a simple number within each string,
            Regex pattern = new Regex(@"^(?<prefix>[^0-9]*)(?<digits>[0-9]+)(?<suffix>[^0-9]*)$");
            Match matchX = pattern.Match(x);
            Match matchY = pattern.Match(y);

            if (!matchX.Success || !matchY.Success)
            {
                // pattern was not found in a string
                return x.CompareTo(y);
            }

            // Now, both matchX and matchY were successful.
            string xPrefix = matchX.Groups["prefix"].Value;
            string yPrefix = matchY.Groups["prefix"].Value;

            if (xPrefix != yPrefix)
            {
                return xPrefix.CompareTo(yPrefix);
            }

            // The prefixes are the same.  Compare the numeric part.
            int xDigits = int.Parse(matchX.Groups["digits"].Value);
            int yDigits = int.Parse(matchY.Groups["digits"].Value);
            if (xDigits != yDigits)
            {
                return xDigits - yDigits;
            }

            // The numbers are the same also.  Compare the suffixes.
            string xSuffix = matchX.Groups["suffix"].Value;
            string ySuffix = matchY.Groups["suffix"].Value;

            return xSuffix.CompareTo(ySuffix);
        }

        /// <summary>
        /// Gets a component's hyperlinked name string for use in GME Logger messages, based on its IMgaFCO object.
        /// </summary>
        /// <param name="defaultString">A default text string that will be shown if the component is null or has no ID.</param>
        /// <param name="myComponent">The IMgaFCO object of the component to be referenced.</param>
        /// <returns>The hyperlinked text, if all is OK; otherwise the defaultString.</returns>
        /// <remarks>Added for MOT-228: Modify SystemC CAT Module messages to use GME-hyperlinks.
        /// 
        ///  Instead of accepting CyPhy.Port or CyPhy.Property as an argument, this method accepts an IMgaFCO.
        ///  IMgaFCO objects have the same ID and Name fields. An example of an IMgaFCO object would be a "CyPhy.Port.Impl as IMgaFCO".
        ///  
        ///  When calling this function, you can use this code snippet to get an MgaFCO version of a CyPhy object:
        ///         GetHyperlinkStringFromComponent("default", myComponent.Impl as IMgaFCO);
        /// 
        /// </remarks>
        /// <see cref="https://metamorphsoftware.atlassian.net/browse/MOT-228"/>

        private static string GetHyperlinkStringFromComponent(string defaultString, IMgaFCO myComponent)
        {
            string rVal = defaultString;
            if ((null != myComponent) && (myComponent.ID.Length > 0) && (myComponent.Name.Length > 0))
            {
                rVal = string.Format("<a href=\"mga:{0}\">{1}</a>",
                    myComponent.ID,
                    myComponent.Name);
            }
            return rVal;
        }
        
    }
}

