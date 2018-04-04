using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using CyPhyComponentAuthoring;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Windows.Forms;
using META;
using System.Xml.Serialization;
using CyPhyComponentAuthoring.GUIs;
using GME.MGA;



namespace CyPhyComponentAuthoring.Modules
{
    [CyPhyComponentAuthoringInterpreter.IsCATModule(ContainsCATmethod=true)]
    public class SimulinkModelImport : CATModule
    {
        // Module-level variables
        private int m_startX;     // Visual X coordinate for the new SystemC model, relative to the left of the component window.
        private int m_startY;     // Visual Y coordinate for the new SystemC model, relative to the top of the component window.

        private CyPhyGUIs.GMELogger Logger { get; set; }
        
        [CyPhyComponentAuthoringInterpreter.CATName(
            NameVal = "Add Simulink Model",
            DescriptionVal = "An existing Simulink model gets imported and associated with this CyPhy component.",
            RoleVal = CyPhyComponentAuthoringInterpreter.Role.Construct,
            IconResourceKey = "CyPhy2Simulink"
           )
        ]
        public void ImportSimulinkModel_Delegate(object sender, EventArgs e)
        {
            ImportSimulinkModel(this.GetCurrentComp(), sender);
        }

        public void ImportSimulinkModel(CyPhy.Component component, object sender)
        {
            Boolean ownLogger = false;

            if (Logger == null)
            {
                ownLogger = true;
                Logger = new CyPhyGUIs.GMELogger(component.Impl.Project, "SimulinkModelImport");
            }

            // Check that the selected files are OK.
            bool needExit = false;

            if( needExit )
            {
                if (ownLogger)
                {
                    Logger.Dispose();
                    Logger = null;
                }

                return;
            }

            Form senderParentForm = null;
            if (sender is Control)
            {
                var senderControl = (Control)sender;
                senderParentForm = senderControl.FindForm();

                if (senderParentForm != null)
                {
                    senderParentForm.UseWaitCursor = true;
                }
            }

            try
            {
                using (var browser = new SimulinkLibraryBrowser())
                {
                    using (var simulinkConnector = new SimulinkConnector(Logger))
                    {
                        browser.BlockNames = simulinkConnector.ListSystemObjects("simulink").ToList();

                        var result = browser.ShowDialog(senderParentForm);

                        if (result == DialogResult.OK)
                        {
                            Logger.WriteInfo("Selected Block: {0}", browser.SelectedBlockName);

                            var paramNames = simulinkConnector.ListBlockParameters(browser.SelectedBlockName);

                            using (var paramPicker = new SimulinkParameterPicker())
                            {
                                paramPicker.ParamNames = paramNames.ToList();

                                var result2 = paramPicker.ShowDialog(senderParentForm);

                                if (result2 == DialogResult.OK)
                                {
                                    foreach (var param in paramPicker.SelectedParams)
                                    {
                                        Logger.WriteInfo(param);
                                    }

                                    IDictionary<int, string> inPorts;
                                    IDictionary<int, string> outPorts;

                                    simulinkConnector.ListPorts(browser.SelectedBlockName, out inPorts, out outPorts);

                                    AddSimulinkObjectToModel(component, browser.SelectedBlockName,
                                        paramPicker.SelectedParams,
                                        inPorts, outPorts);
                                }
                                else
                                {
                                    Logger.WriteInfo("Simulink import cancelled");
                                }
                            }
                        }
                        else
                        {
                            Logger.WriteInfo("Simulink import cancelled");
                        }

                        Logger.WriteInfo("Complete");
                    }
                }


            }
            catch (Exception e)
            {
                Logger.WriteError("Error occurred: {0}", e.Message);

                if (ownLogger)
                {
                    Logger.Dispose();
                    Logger = null;
                }

                return;
            }
            finally
            {
                if (senderParentForm != null)
                {
                    senderParentForm.UseWaitCursor = false;
                }
            }

            // Find the visual coordinates of where the new SystemC model should be placed.
            getNewModelInitialCoordinates(component, out m_startX, out m_startY);

            if (ownLogger)
            {
                Logger.Dispose();
                Logger = null;
            }
        }

        public void AddSimulinkObjectToModel(CyPhy.Component component, string blockPath, IEnumerable<string> selectedParams,
            IDictionary<int, string> inPorts, IDictionary<int, string> outPorts)
        {
            const int INNER_LEFT_COLUMN_X = 50;
            const int INNER_RIGHT_COLUMN_X = 500;
            const int INNER_VERTICAL_OFFSET = 25;
            const int INNER_VERTICAL_PARAM_SPACING = 100;
            const int INNER_VERTICAL_PORT_SPACING = 175;

            const int OUTER_LEFT_COLUMN_HORIZONTAL_OFFSET = -300;
            const int OUTER_RIGHT_COLUMN_HORIZONTAL_OFFSET = 300;
            const int OUTER_VERTICAL_PARAM_SPACING = 40;
            const int OUTER_VERTICAL_PORT_SPACING = 100;

            int baseXPosition, baseYPosition;
            getNewModelInitialCoordinates(component, out baseXPosition, out baseYPosition);

            int nextInnerLeftYPosition = 0;
            int nextInnerRightYPosition = 0;

            int nextOuterLeftYPosition = baseYPosition;
            int nextOuterRightYPosition = baseYPosition;

            CyPhy.SimulinkModel newSimulinkModel = CyPhyClasses.SimulinkModel.Create(component);
            newSimulinkModel.Name = blockPath;
            newSimulinkModel.Attributes.BlockType = blockPath;
            newSimulinkModel.Preferences.PortLabelLength = 0;
            setFCOPosition(newSimulinkModel.Impl as MgaFCO, baseXPosition, baseYPosition);
            

            foreach (var param in selectedParams)
            {
                CyPhy.SimulinkParameter newParam = CyPhyClasses.SimulinkParameter.Create(newSimulinkModel);
                newParam.Name = param;
                setFCOPosition(newParam.Impl as MgaFCO, INNER_LEFT_COLUMN_X, INNER_VERTICAL_OFFSET + nextInnerLeftYPosition);

                CyPhy.Property newProperty = CyPhyClasses.Property.Create(component);
                newProperty.Name = param;
                CyPhyClasses.SimulinkParameterPortMap.Connect(newProperty, newParam);
                setFCOPosition(newProperty.Impl as MgaFCO, baseXPosition + OUTER_LEFT_COLUMN_HORIZONTAL_OFFSET, nextOuterLeftYPosition);

                nextInnerLeftYPosition += INNER_VERTICAL_PARAM_SPACING;
                nextOuterLeftYPosition += OUTER_VERTICAL_PARAM_SPACING;
            }

            foreach (var inPort in inPorts)
            {
                CyPhy.SimulinkPort newPort = CyPhyClasses.SimulinkPort.Create(newSimulinkModel);

                var portName = inPort.Value;
                if (string.IsNullOrWhiteSpace(portName))
                {
                    portName = string.Format("in-{0}", inPort.Key);
                }
                newPort.Name = portName;
                newPort.Attributes.SimulinkPortDirection = CyPhyClasses.SimulinkPort.AttributesClass.SimulinkPortDirection_enum.@in;
                newPort.Attributes.SimulinkPortID = inPort.Key.ToString();
                setFCOPosition(newPort.Impl as MgaFCO, INNER_LEFT_COLUMN_X, INNER_VERTICAL_OFFSET + nextInnerLeftYPosition);

                CyPhy.Connector newConnector = CyPhyClasses.Connector.Create(component);
                newConnector.Name = portName;
                CyPhy.SimulinkPort connectorPort = CyPhyClasses.SimulinkPort.Create(newConnector);
                connectorPort.Name = portName;
                connectorPort.Attributes.SimulinkPortDirection = CyPhyClasses.SimulinkPort.AttributesClass.SimulinkPortDirection_enum.@in;
                connectorPort.Attributes.SimulinkPortID = inPort.Key.ToString();
                CyPhyClasses.PortComposition.Connect(connectorPort, newPort, null, null, component);
                setFCOPosition(newConnector.Impl as MgaFCO, baseXPosition + OUTER_LEFT_COLUMN_HORIZONTAL_OFFSET, nextOuterLeftYPosition);

                nextInnerLeftYPosition += INNER_VERTICAL_PORT_SPACING;
                nextOuterLeftYPosition += OUTER_VERTICAL_PORT_SPACING;
            }

            foreach (var outPort in outPorts)
            {
                CyPhy.SimulinkPort newPort = CyPhyClasses.SimulinkPort.Create(newSimulinkModel);

                var portName = outPort.Value;
                if (string.IsNullOrWhiteSpace(portName))
                {
                    portName = string.Format("out-{0}", outPort.Key);
                }
                newPort.Name = portName;
                newPort.Attributes.SimulinkPortDirection = CyPhyClasses.SimulinkPort.AttributesClass.SimulinkPortDirection_enum.@out;
                newPort.Attributes.SimulinkPortID = outPort.Key.ToString();
                setFCOPosition(newPort.Impl as MgaFCO, INNER_RIGHT_COLUMN_X, INNER_VERTICAL_OFFSET + nextInnerRightYPosition);

                CyPhy.Connector newConnector = CyPhyClasses.Connector.Create(component);
                newConnector.Name = portName;
                CyPhy.SimulinkPort connectorPort = CyPhyClasses.SimulinkPort.Create(newConnector);
                connectorPort.Name = portName;
                connectorPort.Attributes.SimulinkPortDirection = CyPhyClasses.SimulinkPort.AttributesClass.SimulinkPortDirection_enum.@out;
                connectorPort.Attributes.SimulinkPortID = outPort.Key.ToString();
                CyPhyClasses.PortComposition.Connect(newPort, connectorPort, null, null, component);
                setFCOPosition(newConnector.Impl as MgaFCO, baseXPosition + OUTER_RIGHT_COLUMN_HORIZONTAL_OFFSET, nextOuterRightYPosition);

                nextInnerRightYPosition += INNER_VERTICAL_PORT_SPACING;
                nextOuterRightYPosition += OUTER_VERTICAL_PORT_SPACING;
            }
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

        public class SimulinkConnector : IDisposable
        {
            private dynamic _matlabInstance;

            private CyPhyGUIs.SmartLogger _logger;

            public SimulinkConnector(CyPhyGUIs.SmartLogger logger)
            {
                _logger = logger;
                _matlabInstance = null;

                int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);
                try
                {
                    var matlabType = Type.GetTypeFromProgID("Matlab.Application");
                    if (matlabType == null)
                    {
                        throw new COMException("No type Matlab.Application", REGDB_E_CLASSNOTREG);
                    }
                    _matlabInstance = Activator.CreateInstance(matlabType);
                }
                catch (COMException e)
                {
                    if (e.ErrorCode == REGDB_E_CLASSNOTREG)
                    {
                        throw new ApplicationException("Matlab is not installed or registered");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            public IEnumerable<string> ListSystemObjects(string systemName)
            {
                _matlabInstance.Execute("load_system('simulink');");

                object result;

                _matlabInstance.Feval("find_system", 1, out result, "simulink");

                //We're going to make some assumptions about the format of this output
                //'result' is a single-element array, containing an Object[,] which is
                //an n*1 array containing strings with block names
                var blockNameArray = ((Object[,]) (((Object[]) result)[0])).Cast<string>().Select(s => s.Replace("\n", " "));

                return blockNameArray;
            }

            public IEnumerable<string> ListBlockParameters(string blockName)
            {
                object result;
                _matlabInstance.Execute(
                    string.Format("objParams = get_param('{0}', 'ObjectParameters')", blockName));
                _matlabInstance.Execute("paramNames = fieldnames(objParams)");

                _matlabInstance.GetWorkspaceData("paramNames", "base", out result);

                object[,] resultArray = (object[,]) result;

                return resultArray.Cast<string>();
            }

            public void ListPorts(string blockName, out IDictionary<int,string> inPorts, out IDictionary<int, string> outPorts)
            {
                inPorts = new SortedDictionary<int, string>();
                outPorts= new SortedDictionary<int, string>();

                var parameters = ListBlockParameters(blockName);

                if (!parameters.Contains("PortHandles"))
                {
                    //Selected block doesn't have ports and we shouldn't
                    //try to enumerate them
                }
                else
                {
                    DebugExecute("portHandles = get_param('{0}', 'PortHandles')", blockName);
                    DebugExecute("inPortCount = length(portHandles.Inport)");
                    DebugExecute("outPortCount = length(portHandles.Outport)");

                    object inPortCountObj, outPortCountObj;
                    _matlabInstance.GetWorkspaceData("inPortCount", "base", out inPortCountObj);
                    _matlabInstance.GetWorkspaceData("outPortCount", "base", out outPortCountObj);

                    int inPortCount = (int) ((double) inPortCountObj); //inPortCountObj is actually a Double; can't cast directly to int from object
                    int outPortCount = (int) ((double) outPortCountObj);

                    _logger.WriteDebug("{0} in, {1} out", inPortCount, outPortCount);

                    var inPortNumberObjs = new object[inPortCount];
                    var inPortNameObjs = new object[inPortCount];

                    for (int i = 1; i <= inPortCount; i++)
                    {
                        _logger.WriteDebug("Getting Port {0} Metadata:", i);
                        DebugExecute("portHandle = portHandles.Inport({0})", i);
                        DebugExecute("portMeta = get(portHandle)");
                        DebugExecute("portNumber = portMeta.PortNumber");
                        DebugExecute("portName = portMeta.Name");

                        //Declaring portNumberObj/portNameObj here only works on one iteration through the loop--
                        //Matlab COM bug? (We use the arrays above instead)
                        //object portNumberObj = null, portNameObj = null;
                        
                        _matlabInstance.GetWorkspaceData("portNumber", "base", out inPortNumberObjs[i-1]);
                        _matlabInstance.GetWorkspaceData("portName", "base", out inPortNameObjs[i-1]);

                        int portNumber = (int)((double)inPortNumberObjs[i - 1]);
                        string portName = (string)inPortNameObjs[i - 1];

                        _logger.WriteDebug("In Port {0} ({1})", portNumber, portName);

                        if (portName == null)
                        {
                            portName = "";
                        }

                        inPorts[portNumber] = portName;
                    }

                    var outPortNumberObjs = new object[outPortCount];
                    var outPortNameObjs = new object[outPortCount];

                    for (int i = 1; i <= outPortCount; i++)
                    {
                        _logger.WriteDebug("Getting Port {0} Metadata:", i);
                        DebugExecute("portHandle = portHandles.Outport({0})", i);
                        DebugExecute("portMeta = get(portHandle)");
                        DebugExecute("portNumber = portMeta.PortNumber");
                        DebugExecute("portName = portMeta.Name");

                        //Declaring portNumberObj/portNameObj here only works on one iteration through the loop--
                        //Matlab COM bug? (We use the arrays above instead)
                        //object portNumberObj = null, portNameObj = null;

                        _matlabInstance.GetWorkspaceData("portNumber", "base", out outPortNumberObjs[i - 1]);
                        _matlabInstance.GetWorkspaceData("portName", "base", out outPortNameObjs[i - 1]);

                        int portNumber = (int)((double)outPortNumberObjs[i - 1]);
                        string portName = (string)outPortNameObjs[i - 1];

                        _logger.WriteDebug("Out Port {0} ({1})", portNumber, portName);

                        if (portName == null)
                        {
                            portName = "";
                        }

                        outPorts[portNumber] = portName;
                    }
                }
            }

            public void DebugExecute(string format, params object[] args)
            {
                string output = _matlabInstance.Execute(string.Format(format, args));
                _logger.WriteDebug(output);
            }

            public void Dispose()
            {
                if (_matlabInstance != null)
                {
                    _matlabInstance.Quit();

                    _matlabInstance = null;
                }
            }
        }
        
    }
}



