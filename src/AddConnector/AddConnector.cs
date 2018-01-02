using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GME.CSharp;
using GME;
using GME.MGA;
using GME.MGA.Core;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using InterfaceFCO = ISIS.GME.Common.Interfaces.FCO;
using InterfaceReference = ISIS.GME.Common.Interfaces.Reference;

///
/// AddConnector -- A connector toolbar utility.
/// 
/// See also MOT-564: Connector utilities for component builders.
/// 
/// Initial version created using the GME C# Interpreter Wizard,
/// described in the shared "Our Codebase" document under IT & Developer Docs.
/// 
/// 16-DEC-2012 Henry Forson
/// 

namespace AddConnector
{
    /// <summary>
    /// This class implements the necessary COM interfaces for a GME interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class AddConnectorInterpreter : IMgaComponentEx, IGMEVersionInfo
    {
        /// <summary>
        /// Contains information about the GUI event that initiated the invocation.
        /// </summary>
        public enum ComponentStartMode
        {
            GME_MAIN_START = 0, 		// Not used by GME
            GME_BROWSER_START = 1,      // Right click in the GME Tree Browser window
            GME_CONTEXT_START = 2,		// Using the context menu by right clicking a model element in the GME modeling window
            GME_EMBEDDED_START = 3,		// Not used by GME
            GME_MENU_START = 16,		// Clicking on the toolbar icon, or using the main menu
            GME_BGCONTEXT_START = 18,	// Using the context menu by right clicking the background of the GME modeling window
            GME_ICON_START = 32,		// Not used by GME
            GME_SILENT_MODE = 128 		// Not used by GME, available to testers not using GME
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
        public int getGreatestCurrentY(CyPhy.Component component)
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

        public int getGreatestCurrentConnectorY(CyPhy.Connector conn)
        {
            int rVal = 0;
            // The children of the component may be things like schematic models, ports, etc.
            foreach (var child in conn.AllChildren)
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


        private CyPhyGUIs.GMELogger Logger { get; set; }

        /// <summary>
        /// This function is called for each interpreter invocation before Main.
        /// Don't perform MGA operations here unless you open a transaction.
        /// </summary>
        /// <param name="project">The handle of the project opened in GME, for which the interpreter was called.</param>
        public void Initialize(MgaProject project)
        {
            // TODO: Add your initialization code here...            
        }

        /// <summary>
        /// Copies the connections in the original port/pin to the duplicate port/pin.
        /// </summary>
        /// <param name="component">The parent component of the ports/pins and connections.</param>
        /// <param name="original">The original, possibly connected, port/pin.</param>
        /// <param name="duplicate">The port/pin that will receive the copied connections.</param>
        /// <remarks>The original port/pin should typically be deleted after this method.</remarks>
        private void copyPinConnections(CyPhy.Component component, MgaFCO original, MgaFCO duplicate)
        {
            // Get the ID of the original pin
            string originalPinId = original.ID;

            // Check the component's connector IDs for any that connect to the original pin.
            foreach (CyPhy.PortComposition connector in component.Children.PortCompositionCollection)
            {
                CyPhy.SchematicModelPort dstPort = (CyPhy.SchematicModelPort)connector.DstEnd;
                CyPhy.SchematicModelPort srcPort = (CyPhy.SchematicModelPort)connector.SrcEnd;
                string dstId = dstPort.ID;
                string srcId = srcPort.ID;
                if (originalPinId == dstId)
                {
                    // Logger.WriteInfo("Found a matching connector dstPort.ID.");

                    // A matching schematic pin already exists, so just make the connection.
                    CyPhy.PortComposition composition = CyPhyClasses.PortComposition.Connect(
                        srcPort,
                        CyPhyClasses.Port.Cast(duplicate),
                        null,
                        null,
                        component);
                }
                if (originalPinId == srcId)
                {
                    // Logger.WriteInfo("Found a matching connector srcPort.ID.");

                    // A matching schematic pin already exists, so just make the connection.
                    CyPhy.PortComposition composition = CyPhyClasses.PortComposition.Connect(
                        CyPhyClasses.Port.Cast(duplicate),
                        dstPort,
                        null,
                        null,
                        component);
                }
            }
        }

        /// <summary>
        /// Generic port "clone" function that creates and copies a port/pin or pin. 
        /// </summary>
        /// <param name="parent">The MgaModel that will contain newly-created clone</param>
        /// <param name="oldPort">The original port or pin to be cloned</param>
        /// <returns>newPortFCO -- The cloned port/pin created in the parent.</returns>
        private MgaFCO ClonePort(MgaModel parent, MgaFCO oldPort)
        {
            Logger.WriteDebug("ClonePort: {0}", oldPort.AbsPath);

            GME.MGA.Meta.MgaMetaRole role = null;

            foreach (GME.MGA.Meta.MgaMetaRole roleItem in (parent.Meta as GME.MGA.Meta.MgaMetaModel).Roles)
            {
                if (roleItem.Kind.MetaRef == oldPort.MetaBase.MetaRef)
                {
                    role = roleItem;
                    break;
                }
            }

            var newPortFCO = parent.CopyFCODisp(oldPort, role);
            //newPortFCO.SetAttributeByNameDisp("ID", null);
            return newPortFCO;
        }
 

        /// <summary>
        /// Creates a connector for each selected port or pin.
        /// </summary>
        /// <param name="currentobj">Component that holds the ports/pins to be wrapped.</param>
        /// <param name="portList">List of all the selected ports/pins to be wrapped with connectors.</param>
        private void handleNoConnectorsSelected(MgaFCO currentobj, List<MgaFCO> portList)
        {
            int popCount = 0;

            // Get the component
            var component = ISIS.GME.Dsml.CyPhyML.Classes.Component.Cast(currentobj);
            int startY = getGreatestCurrentY(component);

            foreach (MgaFCO portOrPin in portList)
            {
                // Get the name of the selected port or pin
                string popName = portOrPin.Name;

                // Add a connector
                CyPhy.Connector newConnector = CyPhyClasses.Connector.Create(component);

                // Name it
                newConnector.Name = popName;

                // Give it X and Y coordinates
                // Figure out where to place the newly-created schematic pin based on what side
                // of the SPICE model the SPICE model pin is on.

                int pinX = 100;
                int pinY = startY + (125 * ++popCount);

                // Position the newly-created connector
                // GUI coordinates in all aspects.
                setFCOPosition(newConnector.Impl as MgaFCO, pinX, pinY);

                // Copy fields into a cloned port or pin
                MgaFCO clonedPortOrPin = ClonePort(newConnector.Impl as MgaModel, portOrPin);

                // Name it
                clonedPortOrPin.Name = popName;

                // Set coordinates
                setFCOPosition(clonedPortOrPin, 100, 100);

                // Copy connections
                copyPinConnections(component, portOrPin, clonedPortOrPin);

                // Delete the original port or pin
                portOrPin.DestroyObject();
            }
        }

        /// <summary>
        /// Adds selected ports or pins to the selected connector.
        /// </summary>
        /// <param name="currentobj">Component that holds the ports/pins to be wrapped.</param>
        /// <param name="portList">List of all the selected ports to be moved into the selected connector.</param>
        /// <param name="conn">Connector where the selected pins are to be moved.</param>
        private void handleOneConnectorSelected(MgaFCO currentobj, List<MgaFCO> portList, CyPhy.Connector conn)
        {
            int popCount = 0;
            int startY = getGreatestCurrentConnectorY(conn);

            // Get the component
            var component = ISIS.GME.Dsml.CyPhyML.Classes.Component.Cast(currentobj);

            foreach (MgaFCO portOrPin in portList)
            {
                // Get the name of the selected port or pin
                string popName = portOrPin.Name;

                int pinX = 100;
                int pinY = startY + (125 * ++popCount);

                // Copy fields into a cloned port or pin
                MgaFCO clonedPortOrPin = ClonePort(conn.Impl as MgaModel, portOrPin);

                // Name it
                clonedPortOrPin.Name = popName;

                // Set coordinates
                setFCOPosition(clonedPortOrPin, pinX, pinY);

                // Copy connections
                copyPinConnections(component, portOrPin, clonedPortOrPin);

                // Delete the original port or pin
                portOrPin.DestroyObject();
            }
        }

        /// <summary>
        /// Intelligently adds selected ports or pins to the selected connectors.
        /// </summary>
        /// <param name="currentobj">Component that holds the ports/pins to be wrapped.</param>
        /// <param name="portList">List of all the selected ports to be moved into the selected connector.</param>
        /// <param name="conn">Connector where the selected pins are to be moved.</param>
        private void handleMultipleConnectorsSelected(MgaFCO currentobj, List<MgaFCO> portList, List<CyPhy.Connector> connList)
        {
            // TODO(tthomas): Add "intelligence"
            int popCount = 0;
            CyPhy.Connector conn = connList[0];
            int startY = getGreatestCurrentConnectorY(conn);

            // Get the component
            var component = ISIS.GME.Dsml.CyPhyML.Classes.Component.Cast(currentobj);

            foreach (MgaFCO portOrPin in portList)
            {
                // Get the name of the selected port or pin
                string popName = portOrPin.Name;

                int pinX = 100;
                int pinY = startY + (125 * ++popCount);

                // Copy fields into a cloned port or pin
                MgaFCO clonedPortOrPin = ClonePort(conn.Impl as MgaModel, portOrPin);

                // Name it
                clonedPortOrPin.Name = popName;

                // Set coordinates
                setFCOPosition(clonedPortOrPin, pinX, pinY);

                // Copy connections
                copyPinConnections(component, portOrPin, clonedPortOrPin);

                // Delete the original port or pin
                portOrPin.DestroyObject();
            }
        }

        /// <summary>
        /// Checks an MgaFCO to see if it is a port.
        /// </summary>
        /// <param name="selectedObj"></param>
        /// <returns></returns>
        private bool isPort(MgaFCO selectedObj)
        {
            bool rVal = false;
            if( selectedObj != null )
            {
                switch (selectedObj.Meta.Name)
                {
                    case "SchematicModelPort":
                    case "RFPort":
                    case "SystemCPort":
                        rVal = true;
                        break;
                    default:
                        break;
                }
            }
            return rVal;
        }

        /// <summary>
        /// The main entry point of the interpreter. A transaction is already open,
        /// GMEConsole is available. A general try-catch block catches all the exceptions
        /// coming from this function, you don't need to add it. For more information, see InvokeEx.
        /// </summary>
        /// <param name="project">The handle of the project opened in GME, for which the interpreter was called.</param>
        /// <param name="currentobj">The model open in the active tab in GME. Its value is null if no model is open (no GME modeling windows open). </param>
        /// <param name="selectedobjs">
        /// A collection for the selected model elements. It is never null.
        /// If the interpreter is invoked by the context menu of the GME Tree Browser, then the selected items in the tree browser. Folders
        /// are never passed (they are not FCOs).
        /// If the interpreter is invoked by clicking on the toolbar icon or the context menu of the modeling window, then the selected items 
        /// in the active GME modeling window. If nothing is selected, the collection is empty (contains zero elements).
        /// </param>
        /// <param name="startMode">Contains information about the GUI event that initiated the invocation.</param>
        [ComVisible(false)]
        public void Main(MgaProject project, MgaFCO currentobj, MgaFCOs selectedobjs, ComponentStartMode startMode)
        {
            GMEConsole.Out.WriteLine("Running the AddConnector interpreter...");

            Boolean ownLogger = false;
            if (Logger == null)
            {
                ownLogger = true;
                Logger = new CyPhyGUIs.GMELogger(project, "AddConnector");
            }

            GMEConsole.Out.WriteLine("\n======================================================================================================================================\n");

            using (Logger)  // Ensure Logger is disposed if there is an unexpected exception. MOT-84 
            {

                Logger.WriteInfo("Starting AddConnector.");

                AddConnector(currentobj, selectedobjs);

                Logger.WriteInfo("The AddConnector interpreter has finished.");
            }

            if (ownLogger)
            {
                Logger.Dispose();
                Logger = null;
            }
        }

        /// <summary>
        /// Based on the selected objects, either adds connectors for ports/pins, or moves additional ports/pins into a connector.
        /// </summary>
        /// <param name="currentobj">The component model being modified.</param>
        /// <param name="selectedobjs">The selected ports/pins (and perhaps connector).</param>
        /// <seealso cref="https://metamorphsoftware.atlassian.net/browse/MOT-564"/>
        private void AddConnector(MgaFCO currentobj, MgaFCOs selectedobjs)
        {
            if (null == currentobj)
            {
                Logger.WriteError("The current object is null.  Please select a Component object.");
                return;
            }

            if (currentobj.Meta.Name != "Component")
            {
                Logger.WriteError("AddConnector only works on Component objects.");
                Logger.WriteError("But, {1} is neither; it is a {0}.", currentobj.Meta.Name, currentobj.Name);
                return;
            }

            // The current object is a component.
            // Check for selected objects.
            if (selectedobjs.Count < 1)
            {
                Logger.WriteError("At least one pin or port must be selected.");
                return;
            }

            // Make three lists classifying all the selected objects as either ports, connectors, or other,
            // so we can tell how many of each type have been selected.
            List<MgaFCO> portList = new List<MgaFCO>();
            List<CyPhy.Connector> connectorList = new List<CyPhy.Connector>();
            List<MgaFCO> otherList = new List<MgaFCO>();

            foreach (MgaFCO selectedObj in selectedobjs)
            {
                if (selectedObj.MetaRole.Name == "Connector")
                {
                    connectorList.Add(CyPhyClasses.Connector.Cast(selectedObj));
                }
                else if (isPort(selectedObj))
                {
                    Logger.WriteDebug("Found port {0}.", selectedObj.Name);
                    portList.Add(selectedObj);
                }
                else
                {
                    otherList.Add(selectedObj);
                }
            }

            if (portList.Count == 0)
            {
                Logger.WriteError("At least one pin/port object must be selected.");
                return;
            }

            if (otherList.Count > 0)
            {
                if (otherList.Count == 1)
                {
                    Logger.WriteWarning("AddConnector only operates on pins/ports and/or connectors; {0} was also selected but is being ignored.", otherList[0].MetaRole.Name);
                }
                else
                {
                    Logger.WriteWarning("AddConnector only operates on pins/ports and/or connectors; {0} objects of other types were also selected but are being ignored.", otherList.Count);
                }
            }

            Logger.WriteInfo("Found {0} ports, and {1} connectors OK.", portList.Count, connectorList.Count);

            // Choose the mode of operation based on how many connectors were selected.  See MOT-564.
            if (connectorList.Count == 0)
            {
                Logger.WriteInfo("About to create connectors for the selected ports.");
                handleNoConnectorsSelected(currentobj, portList);
            }
            else if (connectorList.Count == 1)
            {
                Logger.WriteInfo("About to move selected ports into the selected connector.");
                handleOneConnectorSelected(currentobj, portList, connectorList[0]);
            }
            else
            {
                Logger.WriteInfo("About to run in \"intelligent\" mode.");
                handleMultipleConnectorsSelected(currentobj, portList, connectorList);
            }

        }


        #region IMgaComponentEx Members

        MgaGateway MgaGateway { get; set; }
        GMEConsole GMEConsole { get; set; }

        public void InvokeEx(MgaProject project, MgaFCO currentobj, MgaFCOs selectedobjs, int param)
        {
            if (!enabled)
            {
                return;
            }

            try
            {
                GMEConsole = GMEConsole.CreateFromProject(project);
                MgaGateway = new MgaGateway(project);

                MgaGateway.PerformInTransaction(delegate
                {
                    Main(project, currentobj, selectedobjs, Convert(param));
                });
            }
            finally
            {
                MgaGateway = null;
                project = null;
                currentobj = null;
                selectedobjs = null;
                GMEConsole = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private ComponentStartMode Convert(int param)
        {
            switch (param)
            {
                case (int)ComponentStartMode.GME_BGCONTEXT_START:
                    return ComponentStartMode.GME_BGCONTEXT_START;
                case (int)ComponentStartMode.GME_BROWSER_START:
                    return ComponentStartMode.GME_BROWSER_START;

                case (int)ComponentStartMode.GME_CONTEXT_START:
                    return ComponentStartMode.GME_CONTEXT_START;

                case (int)ComponentStartMode.GME_EMBEDDED_START:
                    return ComponentStartMode.GME_EMBEDDED_START;

                case (int)ComponentStartMode.GME_ICON_START:
                    return ComponentStartMode.GME_ICON_START;

                case (int)ComponentStartMode.GME_MAIN_START:
                    return ComponentStartMode.GME_MAIN_START;

                case (int)ComponentStartMode.GME_MENU_START:
                    return ComponentStartMode.GME_MENU_START;
                case (int)ComponentStartMode.GME_SILENT_MODE:
                    return ComponentStartMode.GME_SILENT_MODE;
            }

            return ComponentStartMode.GME_SILENT_MODE;
        }

        #region Component Information
        public string ComponentName
        {
            get { return GetType().Name; }
        }

        public string ComponentProgID
        {
            get
            {
                return ComponentConfig.progID;
            }
        }

        public componenttype_enum ComponentType
        {
            get { return ComponentConfig.componentType; }
        }
        public string Paradigm
        {
            get { return ComponentConfig.paradigmName; }
        }
        #endregion

        #region Enabling
        bool enabled = true;
        public void Enable(bool newval)
        {
            enabled = newval;
        }
        #endregion

        #region Interactive Mode
        protected bool interactiveMode = true;
        public bool InteractiveMode
        {
            get
            {
                return interactiveMode;
            }
            set
            {
                interactiveMode = value;
            }
        }
        #endregion

        #region Custom Parameters
        SortedDictionary<string, object> componentParameters = null;

        public object get_ComponentParameter(string Name)
        {
            if (Name == "type")
                return "csharp";

            if (Name == "path")
                return GetType().Assembly.Location;

            if (Name == "fullname")
                return GetType().FullName;

            object value;
            if (componentParameters != null && componentParameters.TryGetValue(Name, out value))
            {
                return value;
            }

            return null;
        }

        public void set_ComponentParameter(string Name, object pVal)
        {
            if (componentParameters == null)
            {
                componentParameters = new SortedDictionary<string, object>();
            }

            componentParameters[Name] = pVal;
        }
        #endregion

        #region Unused Methods
        // Old interface, it is never called for MgaComponentEx interfaces
        public void Invoke(MgaProject Project, MgaFCOs selectedobjs, int param)
        {
            throw new NotImplementedException();
        }

        // Not used by GME
        public void ObjectsInvokeEx(MgaProject Project, MgaObject currentobj, MgaObjects selectedobjs, int param)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region IMgaVersionInfo Members

        public GMEInterfaceVersion_enum version
        {
            get { return GMEInterfaceVersion_enum.GMEInterfaceVersion_Current; }
        }

        #endregion

        #region Registration Helpers

        [ComRegisterFunctionAttribute]
        public static void GMERegister(Type t)
        {
            Registrar.RegisterComponentsInGMERegistry();

        }

        [ComUnregisterFunctionAttribute]
        public static void GMEUnRegister(Type t)
        {
            Registrar.UnregisterComponentsInGMERegistry();
        }

        #endregion


    }
}
