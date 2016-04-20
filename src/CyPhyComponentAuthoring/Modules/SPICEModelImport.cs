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
using SpiceLib;
using PinMatcher;
using UnitsManager;

// using domain specific interfaces
using CyPhyML = ISIS.GME.Dsml.CyPhyML.Interfaces;

namespace CyPhyComponentAuthoring.Modules
{
    [CyPhyComponentAuthoringInterpreter.IsCATModule(ContainsCATmethod=true)]
    public class SpiceModelImport : CATModule
    {
        // Pixel spacings for pins outside the SPICE model
        private const int SCHEMATIC_PIN_X_OFFSET = 100;
        private const int SCHEMATIC_PIN_X_SPACING = 700;
        private const int SCHEMATIC_PIN_Y_OFFSET = -75;
        private const int SCHEMATIC_PIN_Y_SPACING = 75;

        // Pixel spacings for pins within the SPICE model
        private const int PIN_X_OFFSET = 50;
        private const int PIN_X_SPACING = 300;
        private const int PIN_Y_OFFSET = 50;
        private const int PIN_Y_SPACING = 100;

        // Module-level variables
        private int m_startX;     // Visual X coordinate for the new SPICE model, relative to the left of the component window.
        private int m_startY;     // Visual Y coordinate for the new SPICE model, relative to the top of the component window.
        private int m_numberOfPinsPerColumn;  // The maximum number of pins placed in a vertical column.
        List<string> m_unconnectedSpicePinNames;  // Keeps track of any SPICE pins that don't get connected.


        private CyPhyGUIs.GMELogger Logger { get; set; }
        
        [CyPhyComponentAuthoringInterpreter.CATName(
            NameVal = "Add SPICE Model",
            DescriptionVal = "An existing NGSpice model gets imported and associated with this CyPhy component.",
            RoleVal = CyPhyComponentAuthoringInterpreter.Role.Construct
           )
        ]
        public void ImportSpiceModel_Delegate(object sender, EventArgs e)
        {
            ImportSpiceModel(this.GetCurrentComp());
        }

        public void ImportSpiceModel(CyPhy.Component component, String path_SpiceFile = null)
        {
            ComponentInfo ci = new ComponentInfo();

            Boolean ownLogger = false;
            if (Logger == null)
            {
                ownLogger = true;
                Logger = new CyPhyGUIs.GMELogger(component.Impl.Project, "SpiceModelImport");
            }

            if (path_SpiceFile == null)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.CheckFileExists = true;
                    ofd.DefaultExt = "*.cir";
                    ofd.Multiselect = false;
                    ofd.Filter = "CIR (*.cir)|*.cir|All files (*.*)|*.*";
                    DialogResult dr = ofd.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        path_SpiceFile = ofd.FileName;
                    }
                    else
                    {
                        Logger.WriteError("No file was selected. SPICE Model Import will not complete.");

                        if (ownLogger)
                        {
                            Logger.Dispose();
                            Logger = null;
                        }

                        return;
                    }
                }
                if (String.IsNullOrWhiteSpace(path_SpiceFile))
                {
                    Logger.WriteError("SPICE Model Path of \"{0}\" isn't valid.", path_SpiceFile);

                    if (ownLogger)
                    {
                        Logger.Dispose();
                        Logger = null;
                    }

                    return;
                }
            }

            try
            {
                Parse myParse = new Parse();
                ci = myParse.ParseFile(path_SpiceFile);
            }
            catch( Exception e )
            {
                Logger.WriteError("Error parsing '{0}': {1}", path_SpiceFile, e.Message);

                if (ownLogger)
                {
                    Logger.Dispose();
                    Logger = null;
                }

                return;
            }

            //-----------------------------------------------------------------------------------
            //
            //  At this point, the SPICE model file has been parsed and component info extracted.
            //
            //  'ci' has the SPICE component information.
            //  'component' is the GME component that the SPICE info will be added to.
            //  'path_SpiceFile' is the complete path and filename of the ".CIR" file.
            //
            //------------------------------------------------------------------------------------

            //// Clean up the classifications.
            //// This doesn't really belong here; it's just temporary to fix components on 4/1/2014.
            //string[] sArray = component.Attributes.Classifications.Replace( "/", ".").Split( '.' );
            //int sArrayLength = sArray.GetLength(0);
            //string s = "";
            //for (int i = 0; i < sArrayLength; i++)
            //{
            //    string ss = sArray[i].ToLower().Trim().Replace( ' ', '_');
            //    ss = ss.Replace("(", "").Replace(")", "");
            //    s += ss.Replace( "_&_", "_and_" );
            //    if (i + 1 < sArrayLength)
            //    {
            //        s += ".";
            //    }
            //}
            //component.Attributes.Classifications = s;



            // Find the visual coordinates of where the new SPICE model should be placed.
            getNewModelInitialCoordinates(component, out m_startX, out m_startY);

            //------------- Setup the pins
            // Make a list of the preexisting schematic pins (ports) in the component, if any,
            // as well as a dictionary mapping pin names to the ports,
            // for future use when creating SPICE pins that may need to connect to the
            // schematic pins.
            List<string> schematicPins = new List<string>();
            Dictionary<string, CyPhy.Port> schematicPinNameDictionary = new Dictionary<string, CyPhy.Port>();

            // Iterate over child (schematic) ports of the component, to fill in the pin list and dictionary.
            foreach (CyPhy.Port port in component.Children.PortCollection)
            {
                Logger.WriteDebug("Child Port: {0}", GetHyperlinkStringFromComponent(port.Name, port.Impl as IMgaFCO));    // MOT-228
                schematicPins.Add(port.Name);
                try
                {
                    schematicPinNameDictionary.Add( port.Name, port );
                }
                catch( Exception e )
                {
                    // Logger.WriteDebug("Exception adding pin '{0}' to schematicPinNameDictionary: {1}", port.Name, e.Message);
                    // Include GME hyperlink in the debug message for MOT-228:
                    
                    Logger.WriteDebug("Exception adding pin '{0}' to schematicPinNameDictionary: {1}",
                        GetHyperlinkStringFromComponent(port.Name, port.Impl as IMgaFCO),
                        e.Message);
                    // Cleanup
                    Logger.WriteWarning("Unable to match any schematic pins with SPICE pins; new pins will be created.");
                    schematicPins = new List<string>();
                    schematicPinNameDictionary = new Dictionary<string, CyPhy.Port>();
                    break;
                }
            }

            // Find a mapping between the schematic pins and the SPICE pins
            string[,] matchedPins = PinMatcher.PinMatcher.GetPinMatches( schematicPins, ci.pins);

            // Show the pin matches
            if (matchedPins.GetLength(0) > 0)
            {
                Logger.WriteDebug("Pin Matches:");

                for (int row = 0; row < matchedPins.GetLength(0); row++)
                {
                    // Print out the schematic pin name and the SPICE-model pin name, for each pin match.
                    string p0 = matchedPins[row, 0];
                    string p1 = matchedPins[row, 1];

                    // Substitute hyperlinks for the schematic pin names, for MOT-228.
                    if (schematicPinNameDictionary.ContainsKey(p0))
                    {
                        p0 = GetHyperlinkStringFromComponent(p0, schematicPinNameDictionary[p0].Impl as IMgaFCO);
                    }

                    Logger.WriteDebug(@"  {0} [{1}, {2}]", row + 1, p0, p1 );
                }
            }


            //------------------ Setup the Properties
            // Make a list of the preexisting schematic properties in the component, if any,
            // as well as a dictionary mapping property names to the properties,
            // for future use when creating SPICE parameters that may need to connect to the
            // properties.
            List<string> schematicProperties = new List<string>();
            Dictionary<string, CyPhy.Property> schematicPropertyNameDictionary = new Dictionary<string, CyPhy.Property>();

            // Iterate over child (schematic) properties of the component, to fill in the schematicProperties list and dictionary.
            foreach (CyPhy.Property property in component.Children.PropertyCollection)
            {
                Logger.WriteDebug("Child Property: {0} = {1}",
                    GetHyperlinkStringFromComponent(property.Name, property.Impl as IMgaFCO),
                    property.Attributes.Value);

                schematicProperties.Add(property.Name);

                try
                {
                    schematicPropertyNameDictionary.Add(property.Name, property);
                }
                catch (Exception e)
                {
                    Logger.WriteDebug("Exception adding property '{0}' to schematicPropertyNameDictionary: {1}",
                        GetHyperlinkStringFromComponent(property.Name, property.Impl as IMgaFCO),
                        e.Message);
                    // Cleanup
                    Logger.WriteWarning("Unable to match any schematic properties with SPICE parameters; new properties will be created.");
                    schematicProperties = new List<string>();
                    schematicPropertyNameDictionary = new Dictionary<string, CyPhy.Property>();
                    break;
                }
            }

            // Get a list of the SPICE parameter names
            List<string> spiceParameters = new List<string>( ci.parameters.Keys );

            // Find a mapping between the schematic properties and the SPICE parameters
            string[,] matchedProperties = PinMatcher.PinMatcher.GetPinMatches(schematicProperties, spiceParameters);

            // Show the property matches
            if (matchedProperties.GetLength(0) > 0)
            {
                Logger.WriteDebug("Property Matches:");

                for (int row = 0; row < matchedProperties.GetLength(0); row++)
                {
                    // Print out the schematic property name and the SPICE parameter name, for each mapping.
                    string p0 = matchedProperties[row, 0];
                    string p1 = matchedProperties[row, 1];

                    // Substitute hyperlinks for the schematic property names, for MOT-228.
                    if (schematicPropertyNameDictionary.ContainsKey(p0))
                    {
                        p0 = GetHyperlinkStringFromComponent(p0, schematicPropertyNameDictionary[p0].Impl as IMgaFCO);
                    }

                    Logger.WriteDebug(@"  {0} [{1}, {2}]", row + 1, p0, p1);
                }
            }


            //------------------------------------------------------------
            // Create a SPICEModel child
            CyPhy.SPICEModel newModel = CyPhyClasses.SPICEModel.Create(component);
            newModel.Name = ci.name +"_SPICEModel";
            newModel.Attributes.Notes = "Created using SPICE Model CAT Module";
            newModel.Attributes.Class = string.Format("{0}:{1}", ci.elementType, ci.name);

            Logger.WriteInfo("Created a new SPICEModel: \"{0}\"", GetHyperlinkStringFromComponent(newModel.Name, newModel.Impl as IMgaFCO));

            // Adjust the SpiceLib model's label length to allow complete pin and property names to be seen.
            newModel.Preferences.PortLabelLength = 0;

            // Set the SPICE model's visual position.
            setFCOPosition(newModel.Impl as MgaFCO, m_startX, m_startY);

            // Create new ports within the SPICEModel, and add them to a SPICE-pin dictionary
            Dictionary<string, CyPhy.SchematicModelPort> spicePinNameDictionary = CreatePortsWithinTheSpiceModelForEachSpicePin(ci, newModel);

            // Arrange the SPICE model's pins into columns.
            Dictionary<string, int[]> spicePinRowColDictionary = ArrangeSpiceModelPinsIntoRowsAndCols(ci, spicePinNameDictionary);

            // Now the SPICE-model pins (ports) have been added to the SPICE model.
            // We need to connect them to existing schematic pins, if possible,
            // based on how the SPICE pin names matched with the schematic pin names.
            m_unconnectedSpicePinNames = new List<string>();  // Keep track of any SPICE pins that don't get connected.

            ConnectSpiceModelPortsToExistingSchematicPinsIfPossible(component, schematicPinNameDictionary, matchedPins, spicePinNameDictionary);

            CreateAndConnectSchematicPinsForUnconnectedSpicePins(component, spicePinNameDictionary, spicePinRowColDictionary, schematicPinNameDictionary);

            // Create new parameters within the SPICEModel, and add them to a SPICE parameter-name dictionary
            Dictionary<string, CyPhy.SPICEModelParameter> spiceParameterNameDictionary;
            CreateNewParametersWithinSpiceModelAndPopulateDictionary(ci, newModel, out spiceParameterNameDictionary);

            // Now the SPICE-model parameters (properties) have been added to the SPICE model.
            // We need to connect them to existing properties, if possible,
            // based on how the SPICE parameter names matched with the schematic property names.
            List<string> unconnectedSpiceParameterNames;  // Keeps track of any SPICE parameters that don't get connected.
            ConnectSpiceModelParametersToExistingPropertiesIfPossible(
                component, 
                schematicPropertyNameDictionary, 
                matchedProperties, 
                spiceParameterNameDictionary, 
                out unconnectedSpiceParameterNames);


            // Now we need to create and connect schematic properties for any SPICE parameters that are still unconnected.
            CreateAndConnectSchematicPropertiesForUnconnectedSpiceParameters(
                component, 
                ci, 
                spiceParameterNameDictionary, 
                schematicPropertyNameDictionary,
                unconnectedSpiceParameterNames);

            // Get the visual location to place the SPICE-model-file resource.
            int spiceModelfileResourceX = m_startX + 200;
            int spiceModelFileResourceY = m_startY - 100;

            string subdirectory = "Spice";  // The subdirectory where the SPICE file will be copied.

            //  - Copy the SPICE Model files into the component's back-end folder

            string verbose = "";    // String used for exception debugging.
            string destFileName = "";   // Name of the destination SPICE file after possible modification during copying.
            try
            {
                CopySpiceFile(component, path_SpiceFile, subdirectory, out destFileName, out verbose);

            }
            catch (Exception e)
            {
                Logger.WriteError("{1}Exception copying SPICE file: {0}", e.Message, verbose);
                return;
            }

            // Create the SPICE-model-file resource
            CreateSpiceModelFileResource(
                component,
                subdirectory,
                destFileName,
                path_SpiceFile,
                spiceModelfileResourceX,
                spiceModelFileResourceY,
                newModel);


            if (ownLogger)
            {
                Logger.Dispose();
                Logger = null;
            }
        }

        /// <summary>
        /// Creates a SchematicModelPort (pin) within the SPICE-model element for each SPICE pin.
        /// </summary>
        /// <param name="ci">Parsed SPICE-file info about the SPICE subcircuit or model.</param>
        /// <param name="newModel">The SPICE-model element.</param>
        /// <returns>A mapping of SPICE pin names to SchematicModelPorts internal to the SPICE-model element.</returns>
        private Dictionary<string, CyPhy.SchematicModelPort> CreatePortsWithinTheSpiceModelForEachSpicePin(ComponentInfo ci, CyPhy.SPICEModel newModel)
        {
            Dictionary<string, CyPhy.SchematicModelPort> spicePinNameDictionary = new Dictionary<string, CyPhy.SchematicModelPort>();

            int spicePinNumber = 0;
            foreach (string portName in ci.pins)
            {
                // Create a new port within the SPICEModel.
                CyPhy.SchematicModelPort newPort = CyPhyClasses.SchematicModelPort.Create(newModel);
                newPort.Name = portName;
                newPort.Attributes.SPICEPortNumber = spicePinNumber++;

                try
                {
                    // Add the new port to the SPICE pin-name dictionary.
                    spicePinNameDictionary.Add(newPort.Name, newPort);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("WARNING! Exception adding pin '{0}' to spicePinNameDictionary: {1}",
                        GetHyperlinkStringFromComponent(newPort.Name, newPort.Impl as IMgaFCO),
                        e.Message);
                }
            }
            return spicePinNameDictionary;
        }

        /// <summary>
        /// Arrange SPICE-model pins into rows and columns.
        /// </summary>
        /// <param name="ci">The parsed SPICE subcircuit or model info.</param>
        /// <param name="spicePinNameDictionary">Maps SPICE pin names to SPICE pins.</param>
        /// <returns>A dictionary mapping spice pin names to an array containing their row number and column number.</returns>
        private Dictionary<string, int[]> ArrangeSpiceModelPinsIntoRowsAndCols(ComponentInfo ci, Dictionary<string, CyPhy.SchematicModelPort> spicePinNameDictionary)
        {
            List<string> spicePinNames = new List<string>(ci.pins);
            spicePinNames.Sort(ComparePinsUsingNaturalNumericOrder);
            // If more than 5 pins, arrange in two columns.
            int numberOfPinColumns = (spicePinNames.Count() > 5 ? 2 : 1);
            m_numberOfPinsPerColumn = spicePinNames.Count() / numberOfPinColumns;
            // Keep track of the rows and columns associated with the SPICE pins, for later schematic pin positioning.
            Dictionary<string, int[]> spicePinRowColDictionary = new Dictionary<string, int[]>();
            int pinIndex = 0;
            foreach (string pinName in spicePinNames)
            {
                var pinPort = spicePinNameDictionary[pinName];

                // Figure out what row and column this pin is in.
                int pinCol = pinIndex / m_numberOfPinsPerColumn;
                int pinRow = pinIndex % m_numberOfPinsPerColumn;
                pinIndex += 1;

                // Figure out the visual positioning of this pin within the SPICE model window.
                int pinX = PIN_X_OFFSET + (pinCol * PIN_X_SPACING);
                int pinY = PIN_Y_OFFSET + (pinRow * PIN_Y_SPACING);

                // Set the pin's GUI coordinates in all aspects.
                setFCOPosition(pinPort.Impl as MgaFCO, pinX, pinY);

                // Keep track of the row and column for later schematic pin positioning.
                int[] rowCol = { pinRow, pinCol };
                try
                {
                    spicePinRowColDictionary.Add(pinName, rowCol);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("WARNING! Exception adding pin '{0}' to spicePinRowColDictionary: {1}",
                        GetHyperlinkStringFromComponent(pinName, pinPort.Impl as IMgaFCO),
                        e.Message);
                }
            }

            // Show the rows and columns of the pins
            Logger.WriteDebug("Spice pin names (row,col):");
            foreach (var entry in spicePinRowColDictionary)
            {
                Logger.WriteDebug("   '{0}' ({1}, {2})",
                    GetHyperlinkStringFromComponent(entry.Key, spicePinNameDictionary[entry.Key].Impl as IMgaFCO),
                    entry.Value[0],
                    entry.Value[1]);
            }
            return spicePinRowColDictionary;
        }

        /// <summary>
        /// Connect SPICE-model ports to existing schematic pins, if possible.
        /// </summary>
        /// <param name="component">The component containing the SPICE model.</param>
        /// <param name="schematicPinNameDictionary">Maps schematic-pin names to schematic pins.</param>
        /// <param name="matchedPins">Table of strings with schematic pin to SPICE-model port matches in rows.</param>
        /// <param name="spicePinNameDictionary">Maps SPICE-pin names to SPICE pins.</param>
        private void ConnectSpiceModelPortsToExistingSchematicPinsIfPossible(
            CyPhy.Component component, 
            Dictionary<string, CyPhy.Port> schematicPinNameDictionary, 
            string[,] matchedPins, Dictionary<string, 
            CyPhy.SchematicModelPort> spicePinNameDictionary)
        {
            for (int rows = 0; rows < matchedPins.GetLength(0); rows++)
            {
                string schematicPinName = matchedPins[rows, 0];
                string spicePinName = matchedPins[rows, 1];
                if (schematicPinName == "")
                {
                    // We found no match for this SPICE pin
                    m_unconnectedSpicePinNames.Add(spicePinName);
                }
                else if (spicePinName != "")
                {
                    Logger.WriteInfo("Connecting SPICE model pin \"{0}\" to component pin \"{1}\".",
                        GetHyperlinkStringFromComponent(spicePinName, spicePinNameDictionary[spicePinName].Impl as IMgaFCO),
                        GetHyperlinkStringFromComponent(schematicPinName, schematicPinNameDictionary[schematicPinName].Impl as IMgaFCO));

                    // A matching schematic pin already exists, so just make the connection.
                    CyPhy.PortComposition composition = CyPhyClasses.PortComposition.Connect(
                        schematicPinNameDictionary[schematicPinName],
                        spicePinNameDictionary[spicePinName],
                        null,
                        null,
                        component);
                }
            }
        }

        /// <summary>
        /// Create and connect schematic pins for unconnected SPICE-model pins.
        /// </summary>
        /// <param name="component">The component containing the SPICE model, which will get the new schematic pins.</param>
        /// <param name="spicePinNameDictionary">Maps SPICE-model pin names to SchematicModelPorts.</param>
        /// <param name="spicePinRowColDictionary">Maps pins to an array of integers containing [rowNumber, colNumber].</param>
        /// <param name="schematicPinNameDictionary">Map of schematic-pin names to schematic pins. This gets updated if new pins are created.</param>

        private void CreateAndConnectSchematicPinsForUnconnectedSpicePins(
            CyPhy.Component component, 
            Dictionary<string, CyPhy.SchematicModelPort> spicePinNameDictionary, 
            Dictionary<string, int[]> spicePinRowColDictionary,
            Dictionary<string, CyPhy.Port> schematicPinNameDictionary )
        {
            // Sort the unconnected SPICE pins so there is a chance of them being placed in order.
            m_unconnectedSpicePinNames.Sort(ComparePinsUsingNaturalNumericOrder);

            int leftRowCount = 0;
            int rightRowCount = 0;


            // Now we need to create and connect schematic pins for any SPICE pins that are still unconnected.
            foreach (string spicePinName in m_unconnectedSpicePinNames)
            {
                // Get the spice pin
                CyPhy.SchematicModelPort spicePin = spicePinNameDictionary[spicePinName];

                // Create the schematic pin
                CyPhy.SchematicModelPort schematicPin = CyPhyClasses.SchematicModelPort.Create(component);
                schematicPin.Name = spicePinName;
                try
                {
                    schematicPinNameDictionary.Add(spicePinName, schematicPin);
                    Logger.WriteInfo("Created a new pin called \"{0}\".",
                        GetHyperlinkStringFromComponent(schematicPin.Name, schematicPinNameDictionary[schematicPin.Name].Impl as IMgaFCO));
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("Unable to add new schematic pin \"{0}\" to the schematicPinNameDictionary: {1}.",
                        GetHyperlinkStringFromComponent(schematicPin.Name, schematicPinNameDictionary[schematicPin.Name].Impl as IMgaFCO),
                        e);
                }

                // Figure out where to place the newly-created schematic pin based on what side
                // of the SPICE model the SPICE model pin is on.
                int col = spicePinRowColDictionary[spicePinName][1];
                int pinX = SCHEMATIC_PIN_X_OFFSET + SCHEMATIC_PIN_X_SPACING * col;
                int pinY = m_startY + SCHEMATIC_PIN_Y_OFFSET;

                if (col == 0)
                {
                    // put the pin on the left
                    pinY += SCHEMATIC_PIN_Y_SPACING * leftRowCount++;
                }
                else
                {
                    // put the pin on the right
                    pinY += SCHEMATIC_PIN_Y_SPACING * rightRowCount++;
                }

                // Position the newly-created schematic pin
                // Set the schematicPin's GUI coordinates in all aspects.
                setFCOPosition(schematicPin.Impl as MgaFCO, pinX, pinY);

                // Create a connection between the schematic and spice pins.
                CyPhy.PortComposition composition = CyPhyClasses.PortComposition.Connect(
                    schematicPin,
                    spicePin,
                    null,
                    null,
                    component);
            }
        }

        /// <summary>
        /// Creates new parameters within the SPICE model element, and also populates a parameter-name-to-SPICEModelParameter dictionary.
        /// </summary>
        /// <param name="ci">The parsed SPICE subcircuit or model info.</param>
        /// <param name="myModel">The SPICE model element that will have parameters added.</param>
        /// <param name="spiceParameterNameDictionary">A mapping between parameter names and SPICEModelParameters that gets populated.</param>
        private void CreateNewParametersWithinSpiceModelAndPopulateDictionary(
            ComponentInfo ci, 
            CyPhy.SPICEModel myModel, 
            out Dictionary<string, CyPhy.SPICEModelParameter> spiceParameterNameDictionary)
        {
            int paramRow = 0;
            spiceParameterNameDictionary = new Dictionary<string, CyPhy.SPICEModelParameter>();

            foreach (var param in ci.parameters)
            {
                // Create parameters within the Spice model
                CyPhy.SPICEModelParameter newParam = CyPhyClasses.SPICEModelParameter.Create(myModel);
                newParam.Name = param.Key;
                newParam.Attributes.Value = param.Value;

                // Place the parameter at the bottom left of the component, so even numbers of pins remain symmetric.
                int paramX = PIN_X_OFFSET;  // use the same X offset as the left pins.
                int paramY = PIN_Y_OFFSET + ((++paramRow + m_numberOfPinsPerColumn) * PIN_Y_SPACING);

                // Set the parameter's GUI coordinates in all aspects.
                setFCOPosition(newParam.Impl as MgaFCO, paramX, paramY);

                try
                {
                    // Add the new parameter to the SPICE parameter-name dictionary.
                    spiceParameterNameDictionary.Add(newParam.Name, newParam);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("Exception adding parameter '{0}' to spiceParameterNameDictionary: {1}",
                        GetHyperlinkStringFromComponent(newParam.Name, newParam.Impl as IMgaFCO),
                        e.Message);
                }
            }
        }

        /// <summary>
        /// Connect SPICE model parameters to existing properties, if possible.
        /// </summary>
        /// <param name="component">The component containing the SPICE model and any existing properties.</param>
        /// <param name="schematicPropertyNameDictionary">A mapping of property names to properties.</param>
        /// <param name="matchedProperties">A table of strings with property name to parameter name matches in the rows.</param>
        /// <param name="spiceParameterNameDictionary">A mapping of parameter names to SPICEModelParameters.</param>
        /// <param name="unconnectedSpiceParameterNames">A list that will be populated with SPICE parameter names that had no match with existing proerties.</param>
        private void ConnectSpiceModelParametersToExistingPropertiesIfPossible(
            CyPhy.Component component, 
            Dictionary<string, 
            CyPhy.Property> schematicPropertyNameDictionary, 
            string[,] matchedProperties, 
            Dictionary<string, 
            CyPhy.SPICEModelParameter> spiceParameterNameDictionary, 
            out List<string> unconnectedSpiceParameterNames)
        {
           unconnectedSpiceParameterNames = new List<string>();  // Keep track of any SPICE parameters that don't get connected.
            for (int rows = 0; rows < matchedProperties.GetLength(0); rows++)
            {
                string schematicPropertyName = matchedProperties[rows, 0];
                string spiceParameterName = matchedProperties[rows, 1];
                if (schematicPropertyName == "")
                {
                    // We found no match for this SPICE parameter
                    unconnectedSpiceParameterNames.Add(spiceParameterName);
                }
                else if (spiceParameterName != "")
                {

                    // A matching schematic property already exists, so just make the connection.
                    var spiceParameter = spiceParameterNameDictionary[spiceParameterName];
                    var schematicProperty = schematicPropertyNameDictionary[schematicPropertyName];
                    Logger.WriteInfo("Connecting SPICE parameter \"{0}\" to component property \"{1}\".",
                        GetHyperlinkStringFromComponent(spiceParameterName, spiceParameter.Impl as IMgaFCO),
                        GetHyperlinkStringFromComponent(schematicPropertyName, schematicProperty.Impl as IMgaFCO));

                    CyPhyClasses.SPICEModelParameterMap.Connect(
                        schematicProperty,
                        spiceParameter,
                        null,
                        null,
                        component);
                }
            }
        }

        /// <summary>
        /// Create and connect schematic properties for unconnected SPICE parameters
        /// </summary>
        /// <param name="component">The component that will have property elements added.</param>
        /// <param name="ci">Parsed info about the subcircuit/model.</param>
        /// <param name="spiceParameterNameDictionary">A mapping of parameter names to SPICEModelParameters.</param>
        /// <param name="schematicPropertyNameDictionary">A mapping of property names to schematic properties.  Updated if properties are created.</param>
        /// <param name="unconnectedSpiceParameterNames">A list of the unconnected SPICE property names.</param>
        /// <remarks>The new properties will be placed in the component's window below the pins on the left of the SPICE model.</remarks>
        private void CreateAndConnectSchematicPropertiesForUnconnectedSpiceParameters(
            CyPhy.Component component, 
            ComponentInfo ci, 
            Dictionary<string, CyPhy.SPICEModelParameter> spiceParameterNameDictionary, 
            Dictionary<string, CyPhy.Property> schematicPropertyNameDictionary,
            List<string> unconnectedSpiceParameterNames)
        {
            unconnectedSpiceParameterNames.Sort(ComparePinsUsingNaturalNumericOrder);    // Sort the unconnected SPICE parameters so there is a chance of them being placed in order.
            int propertyRow = 0;
            // int propertyOffsetY = 200 + 15 * numberOfPinsPerColumn;
            foreach (string spiceParameterName in unconnectedSpiceParameterNames)
            {
 
                // Get the spice parameter
                var spiceParameter = spiceParameterNameDictionary[spiceParameterName];

                // Create the schematic property
                CyPhy.Property schematicProperty = CyPhyClasses.Property.Create(component);
                schematicProperty.Name = spiceParameterName;
                string valueWithOptionalNgspiceScaleFactors = ci.parameters[spiceParameterName];
                // See MOT-398 and section 2.1.3 of the Ngspice User's Manual Version 26plus.
                string unitString = "";
                CyPhyML.unit cyPhyMLUnit = null;
                schematicProperty.Attributes.Value = GetNumericStringFromNgspiceNumberField( valueWithOptionalNgspiceScaleFactors,
                    out unitString);
                CyPhyML.RootFolder rootFolder = CyPhyClasses.RootFolder.GetRootFolder((MgaProject)this.CurrentProj); 
                    
                // Get the units reference from the units string
                //cyPhyMLUnit = getCyPhyMLUnitFromString(unitString);
                cyPhyMLUnit = UnitsMap.getCyPhyMLUnitFromString(unitString, rootFolder);
                // Set the units reference in the property.
                schematicProperty.Referred.unit = cyPhyMLUnit;

                try
                {
                    schematicPropertyNameDictionary.Add(schematicProperty.Name, schematicProperty);
                    Logger.WriteInfo("Created a new property called \"{0}\".",
                        GetHyperlinkStringFromComponent(schematicProperty.Name, schematicProperty.Impl as IMgaFCO));
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("Exception adding schematicProperty '{0}' to schematicPropertyNameDictionary: {1}",
                        GetHyperlinkStringFromComponent(schematicProperty.Name, schematicProperty.Impl as IMgaFCO),
                        e.Message);
                }


                // Compute the position of the newly-created property.
                int propertyX = SCHEMATIC_PIN_X_OFFSET;  // Position the parameters below the left pins
                int propertyY = m_startY + SCHEMATIC_PIN_Y_OFFSET + ((++propertyRow + m_numberOfPinsPerColumn) * SCHEMATIC_PIN_Y_SPACING);

                // Set the property's GUI coordinates in all aspects.
                setFCOPosition(schematicProperty.Impl as MgaFCO, propertyX, propertyY);

                // Create a connection between the schematic property and spice parameter.
                CyPhyClasses.SPICEModelParameterMap.Connect(
                    schematicProperty,
                    spiceParameter,
                    null,
                    null,
                    component);
            }
        }

        /// <summary>
        /// Converts an Ngspice numeric field to a number string, handling Ngspice scale factors.
        /// </summary>
        /// <param name="valueWithOptionalNgspiceScaleFactors">The string to be converted, possibly including a scale factor suffix.</param>
        /// <param name="units">Units symbol from the CIR file.</param>
        /// <returns>An equivalent numeric string without scale factors.</returns>
        /// <seealso>MOT-398 and section 2.1.3 of the Ngspice User's Manual.</seealso>
        /// suffix value
        /// g       1e9
        /// meg     1e6
        /// k       1e3
        /// m       1e-3
        /// u       1e-6
        /// n       1e-9
        /// p       1e-12
        /// f       1e-15
        private string GetNumericStringFromNgspiceNumberField(string valueWithOptionalNgspiceScaleFactors, out string units)
        {
            string rVal = valueWithOptionalNgspiceScaleFactors;
            string trimmed = valueWithOptionalNgspiceScaleFactors.Trim().ToLower();
            string noAlphaSuffix = trimmed.TrimEnd( "abcdefghijklmnopqrstuvwxyz".ToCharArray() );
            string suffix = trimmed.Substring( noAlphaSuffix.Length );
            int unitsIndex = 0;

            double unscaledValue;

            if( double.TryParse( noAlphaSuffix , out unscaledValue ) )
            {
                // Now unscaledValue has the unscaled value.
                double scaleFactor = 1.0;

                // Check for a scale factor in the suffix.
                if( suffix.Length >= 1 )
                {
                    char firstCharacter = suffix[0];
                    switch( firstCharacter )
                    {
                        case 'g':
                            scaleFactor = 1e9;
                            unitsIndex = 1;
                            break;
                        case 'k':
                            scaleFactor = 1e3;
                            unitsIndex = 1;
                            break;
                        case 'u':
                            scaleFactor = 1e-6;
                            break;
                        case 'n':
                            scaleFactor = 1e-9;
                            unitsIndex = 1;
                            break;
                        case 'p':
                            scaleFactor = 1e-12;
                            unitsIndex = 1;
                            break;
                        case 'f':
                            scaleFactor = 1e-15;
                            unitsIndex = 1;
                            break;
                        case 'm':
                            scaleFactor = 1e-3;
                            unitsIndex = 1;
                            if( suffix.Length >= 3 )
                            {
                                if( ('e' == suffix[ 1 ]) && ('g' == suffix[ 2 ]) ){
                                    scaleFactor = 1e6;
                                    unitsIndex = 3;
                                }
                            }
                            break;
                    }
                    rVal = (unscaledValue * scaleFactor).ToString("G8");
                }
            }

            units = "";

            if (suffix.Length >= unitsIndex + 1)
            {
                string spiceUnits = suffix.Substring(unitsIndex).ToUpper();
                switch (spiceUnits)
                {
                    case "A":
                    case "AMP":
                    case "AMPS":
                    case "AMPERE":
                        units = "Ampere";
                        break;

                    case "V":
                    case "VOLT":
                    case "VOLTS":
                        units = "Volt";
                        break;

                    case "F":
                    case "FARAD":
                    case "FARADS":
                        units = "Farad";
                        break;

                    case "H":
                    case "HENRY":
                    case "HENRIES":
                        units = "Henry";
                        break;

                    case "Ω":
                    case "OHM":
                    case "OHMS":
                        units = "Ohm";
                        break;

                    case "HZ":
                    case "HERTZ":
                        units = "Hertz";
                        break;

                    case "W":
                    case "WATT":
                    case "WATTS":
                        units = "Watt";
                        break;
                }
            }

            return rVal;
        }


        /// <summary>
        /// Creates a SPICE model file resource (SMFR) and connects it to the SPICE model element.
        /// </summary>
        /// <param name="component">The component that will contain the SMFR.</param>
        /// <param name="subdirectory">The subdirectory the SPICE file has been copied into.</param>
        /// <param name="destFileName">The file name of the copied SPICE file.</param>
        /// <param name="path_SpiceFile">Path and file name of the source SPICE file.</param>
        /// <param name="startX">The X position to place the SMFR<./param>
        /// <param name="startY">The Y position to place the SMFR.</param>
        /// <param name="newModel">The SPICE model the SMFR will be connected to.</param>
        /// <returns></returns>
        private string CreateSpiceModelFileResource(
            CyPhy.Component component, 
            string subdirectory,
            string destFileName,
            string path_SpiceFile,
            int spiceModelfileResourceX, 
            int spiceModelFileResourceY, 
            CyPhy.SPICEModel newModel)
        {
            CyPhy.Resource newResource = CyPhyClasses.Resource.Create(component);
            newResource.Name = "SPICEModelFile";
            newResource.Attributes.Notes = string.Format("Derived from '{0}'.\n", path_SpiceFile);
            newResource.Attributes.Path = Path.Combine( subdirectory, destFileName );

            // Set the SPICE-model-file resource's GUI coordinates in all aspects.
            setFCOPosition(newResource.Impl as MgaFCO, spiceModelfileResourceX, spiceModelFileResourceY);

            // Create a connection between the SPICEModelFile resource and the SpiceModel.
            CyPhyClasses.UsesResource.Connect(newResource, newModel, null, null, component);
            return subdirectory;
        }

        /// <summary>
        /// Copy a SPICE file into a component's subdirectory.
        /// </summary>
        /// <param name="component">The component that is getting the SPICE file.</param>
        /// <param name="path_SpiceFile">The full path to the input SPICE (".cir") file, including the file name.</param>
        /// <param name="subdirectory">The subdirectory where the SPICE file will be copied, typically "Spice".</param>
        /// <param name="destFileName">The name of the SPICE file after it has been uniquely copied.  (See MOT-221).</param>
        /// <param name="verbose">String used for exception debugging.</param>
        /// <remarks>May throw an exception from the file system.</remarks>
        private void CopySpiceFile(
            CyPhy.Component component, 
            String path_SpiceFile, 
            string subdirectory, 
            out string destFileName, 
            out string verbose)
        {
            // used in creating the resource object below
            string PathforComp = null;
            verbose = "Begin CopySpiceFile()\n"; // Used for debugging.

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


            destFileName = Path.GetFileName(path_SpiceFile);

            // Find a file name that doesn't already exist, MOT-221.
            string destinationSpiceFilePathAndName = Path.Combine(finalPathName, destFileName);
            int cnt = 1;

            // Make sure the named file doesn't already exist for MOT-221.
            while (File.Exists(destinationSpiceFilePathAndName))
            {
                Logger.WriteInfo("File {0} already exists.", destinationSpiceFilePathAndName);
                destFileName = Path.GetFileNameWithoutExtension(path_SpiceFile) + "_" + (cnt++) + Path.GetExtension(path_SpiceFile);
                destinationSpiceFilePathAndName = Path.Combine(finalPathName, destFileName);
            }
 
            verbose += string.Format("About to copy file '{0}' to '{1}'.\n",
               path_SpiceFile, destinationSpiceFilePathAndName);
            System.IO.File.Copy(path_SpiceFile, destinationSpiceFilePathAndName, true);
            Logger.WriteInfo("Copied file \"{0}\" to \"{1}\".", path_SpiceFile, destinationSpiceFilePathAndName);
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
                    // That's why we chack all aspects to get a single maximum Y, so
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
        /// <remarks>Added for MOT-228: Modify SPICE CAT Module messages to use GME-hyperlinks.
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
