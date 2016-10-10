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


namespace ShowNet
{
    /// <summary>
    /// Provides info about an endpoint. 
    /// Examples of endpoints are a connection's DstEnd and SrcEnd.
    /// </summary>
    public class Endpoint
    {
        public string Name;         // Name of this endpoint.
        public string Kind;         // The type of this endpoint.
        public string ID;           // The project-unique ID of this endpoint.
        public string ParentName;   // The name of the endpoint's container.
        public string ParentKind;   // The type of the endpoint's container.
        public string ParentID;     // The project-unique ID of the endpoint's container.

        /// <summary>
        /// Initialize Endpoint info from an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint of interest.</param>
        public Endpoint(InterfaceFCO endpoint, InterfaceReference epReference)
        {
            const string defaultString = "unknown";
            Name = defaultString;
            Kind = defaultString;
            ID = defaultString;
            ParentName = defaultString;
            ParentKind = defaultString;
            ParentID = defaultString;

            if (endpoint != null)
            {
                if (endpoint.Name != null)
                {
                    Name = endpoint.Name;
                }

                if (endpoint.Kind != null)
                {
                    Kind = endpoint.Kind;
                }

                if (endpoint.ID != null)
                {
                    ID = endpoint.ID;
                }

                if (endpoint.ParentContainer != null)
                {
                    if (endpoint.ParentContainer.Name != null)
                    {
                        ParentName = endpoint.ParentContainer.Name;
                    }

                    if (endpoint.ParentContainer.Kind != null)
                    {
                        ParentKind = endpoint.ParentContainer.Kind;
                    }

                    if (endpoint.ParentContainer.ID != null)
                    {
                        ParentID = endpoint.ParentContainer.ID;
                    }
                }

                // If the endpoint reference exists, use it to get the parent name.
                if (epReference != null)
                {
                    ParentName = epReference.Name;
                    ID += "/" + epReference.ID;
                    ParentID = epReference.ID;
                }
            }
        }
    }

    /// <summary>
    /// A named collection of connected endpoints.
    /// </summary>
    public class Network
    {
        public string SignalName;
        public List<Endpoint> Nodes;

        public Network(List<Endpoint> nodes)
        {
            SignalName = Signal.GetSignalNameFromNodes(nodes);
            Nodes = nodes;
        }

        public string ToString()
        {
            string rVal = SignalName + ":\n";
            foreach (Endpoint ep in Nodes)
            {
                rVal += string.Format("..... <a href=\"mga:{0}\">{1}</a> {2} of the <a href=\"mga:{3}\">{4}</a> {5}\n",
                    ep.ID,
                    ep.Name,
                    ep.Kind,
                    ep.ParentID,
                    ep.ParentName,
                    ep.ParentKind);
            }
            return rVal;
        }
    }

    /// <summary>
    /// Class to determine a signal name from a list of endpoints.
    /// </summary>
    static public class Signal
    {
        /// <summary>
        /// Check if string contains only digits
        /// </summary>
        /// <param name="str"></param>
        /// <returns>true of the string contains only digits, otherwise false.</returns>
        /// <seealso>
        /// http://stackoverflow.com/questions/7461080/fastest-way-to-check-if-string-contains-only-digits
        /// </seealso>
        static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts a plausible signal name for a collection of endpoints.
        /// </summary>
        /// <remarks>
        /// The endpoints may be component pins.  So, if the endpoint's name is
        /// just digits, it is probably meaningless, unless combined with the container's
        /// name.  But, that would not be good as a universal signal name.
        /// 
        /// Instead, preference should be given to the longest endpoint name that isn't all digits.
        /// If we don't have any, we just leave the signal name as a null string,
        /// and let something else that checks for unique network names fix it.
        /// </remarks>
        /// <param name="nodes"></param>
        /// <returns></returns>
        static public string GetSignalNameFromNodes(List<Endpoint> nodes)
        {
            string rVal = "";

            // Look at the endpoint names to get a signal name.
            foreach (Endpoint ep in nodes)
            {
                string candidate = ep.Name;
                if ((!IsDigitsOnly(candidate)) && 
                    (candidate.Length > rVal.Length) &&
                    (candidate.Length > 1) )
                {
                    rVal = candidate;
                }
                else if ( (candidate.ToUpper() == "GND") &&
                    (candidate.Length == rVal.Length))
                {
                    rVal = candidate;
                }
            }

            return rVal;
        }
    }

    /// <summary>
    /// Class to manage a list of networks
    /// </summary>
    public class NetworkManager
    {
        public List<Network> NetworkList;

        public NetworkManager()
        {
            NetworkList = new List<Network>();
        }

        // Add a network to the list of networks, coalescing those with common connections.
        public void Add(Network newNetwork)
        {
            // Iterate list in reverse order, see http://stackoverflow.com/questions/1582285/how-to-remove-elements-from-a-generic-list-while-iterating-over-it
            for (int i = NetworkList.Count - 1; i >= 0; i--)
            {
                if (IsConnectionShared(NetworkList[i], newNetwork))
                {
                    MergeSrcIntoDst(NetworkList[i], ref newNetwork);
                    NetworkList.RemoveAt(i);
                }
            }

            NetworkList.Add(newNetwork);
        }

        /// <summary>
        /// Check if there is a connection between two networks.
        /// </summary>
        /// <param name="net1">The first network.</param>
        /// <param name="net2">The second network.</param>
        /// <exception>
        /// Throws an exception if the first network has internal duplicate endpoints.
        /// </exception>
        /// <returns>true if there is a connection, otherwise false.</returns>
        bool IsConnectionShared(Network net1, Network net2)
        {
            bool rVal = false;

            // We use the Endpoint ID as the key to determine endpoint uniqueness.
            Dictionary<string, Endpoint> epDictionary = new Dictionary<string, Endpoint>();

            foreach (Endpoint ep in net1.Nodes)
            {
                if (!epDictionary.ContainsKey(ep.ID))
                {
                    epDictionary.Add(ep.ID, ep);
                }
                else
                {
                    throw new Exception("Network contains duplicate endpoints.");
                }
            };
            foreach (Endpoint ep in net2.Nodes)
            {
                if (!epDictionary.ContainsKey(ep.ID))
                {
                    epDictionary.Add(ep.ID, ep);
                }
                else
                {
                    // Found a connection between the two networks.
                    rVal = true;
                }
            };

            return rVal;
        }

        /// <summary>
        /// Merge the source netlist into the destination netlist.
        /// </summary>
        /// <param name="src">The source netlist.</param>
        /// <param name="dst">The destination netlist.</param>
        void MergeSrcIntoDst(Network src, ref Network dst)
        {
            // Build a dictionary with all the unique endpoints.
            // We use the Endpoint ID as the key to determine endpoint uniqueness.
            Dictionary<string, Endpoint> epDictionary = new Dictionary<string, Endpoint>();

            foreach (Endpoint ep in src.Nodes)
            {
                if (!epDictionary.ContainsKey(ep.ID))
                {
                    epDictionary.Add(ep.ID, ep);
                }
            };

            foreach (Endpoint ep in dst.Nodes)
            {
                if (!epDictionary.ContainsKey(ep.ID))
                {
                    epDictionary.Add(ep.ID, ep);
                }
            };

            // Get the endpoint list from the dictionary.
            List<Endpoint> nodes = epDictionary.Values.ToList();

            // Set the destination netlist to the merged network.
            dst = new Network(nodes);
        }
    }

    /// <summary>
    /// Class to ensure network names are unique.
    /// </summary>
    public static class NetworkNameChecker
    {
        static Dictionary<string, Network> netNameDict;

        /// <summary>
        /// Initialize NetworkNameChecker
        /// </summary>
        public static void Init()
        {
            netNameDict = new Dictionary<string, Network>();
        }

        /// <summary>
        /// Check each network in the list for a unique name,
        /// and give it one if needed.
        /// </summary>
        /// <param name="networkList">The list of networks to check.</param>
        public static void Update(ref List<Network> networkList)
        {
            foreach (Network net in networkList)
            {
                string netName = net.SignalName;

                if ((netName.Length > 0) && !netNameDict.ContainsKey(netName))
                {
                    // We found a unique network name.
                    netNameDict.Add(netName, net);
                }
                else
                {
                    // The existing network name is missing or not unique.
                    // So, find a unique name for it.
                    string nameBase = "NET_";
                    if (netName.Length > 0)
                    {
                        nameBase = netName + "_";
                    }
                    int count = 1;
                    netName = nameBase + (count++).ToString();
                    while (netNameDict.ContainsKey(netName))
                    {
                        netName = nameBase + (count++).ToString();
                    }
                    // Now we have found a new network name.
                    net.SignalName = netName;
                    netNameDict.Add(netName, net);
                }
                // Sort the endpoints in the network alphabetically.
                net.Nodes.Sort((ep1, ep2) =>
                    {
                        int x = ep1.Name.CompareTo(ep2.Name);
                        if (0 == x)
                        {
                            x = ep1.Kind.CompareTo(ep2.Kind);
                            if (0 == x)
                            {
                                x = ep1.ParentName.CompareTo(ep2.ParentName);
                                if (0 == x)
                                {
                                    x = ep1.ParentKind.CompareTo(ep2.ParentKind);
                                }
                            }
                        }
                        return x;
                    }
                 );
            }

            // Now, all the networks have unique names.
            // Sort the network list alphabetically by the network names.

            networkList.Sort((net1, net2) =>
                {
                    return net1.SignalName.CompareTo(net2.SignalName);
                }
            );            
        }
    }

    /// <summary>
    /// This class implements the necessary COM interfaces for the ShowNet GME interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ShowNetInterpreter : IMgaComponentEx, IGMEVersionInfo
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
            // This is the main ShowNet interpreter code
            Boolean ownLogger = false;
            if (Logger == null)
            {
                ownLogger = true;
                Logger = new CyPhyGUIs.GMELogger(project, "ShowNet");
            }
            GMEConsole.Out.WriteLine("\n======================================================================================================================================\n");
            Logger.WriteInfo("Starting ShowNet.");

            // Get RootFolder
            IMgaFolder rootFolder = project.RootFolder;
            //GMEConsole.Out.WriteLine(rootFolder.Name);

            // To use the domain-specific API:
            //  Create another project with the same name as the paradigm name
            //  Copy the paradigm .mga file to the directory containing the new project
            //  In the new project, install the GME DSMLGenerator NuGet package (search for DSMLGenerator)
            //  Add a Reference in this project to the other project
            //  Add "using [ParadigmName] = ISIS.GME.Dsml.[ParadigmName].Classes.Interfaces;" to the top of this file
            bool done = false;

            if ((!done) && (null == currentobj))
            {
                done = true;
                Logger.WriteError("The current object is null.  Please select a ComponentAssembly or DesignContainer object.");
            }

            if ((!done) && (currentobj.Meta.Name != "ComponentAssembly" &&
                			currentobj.Meta.Name != "DesignContainer"))
            {
                done = true;
                Logger.WriteError("ShowNet only works on ComponentAssembly and DesignContainer objects.");
                Logger.WriteError("But, {1} is neither; it is a {0}.", currentobj.Meta.Name, currentobj.Name);
            }

            if (!done)
            {
				IEnumerable<CyPhy.PortComposition> portCompositionChildren = null;
	            IEnumerable<CyPhy.ConnectorComposition> connectorCompositionChildren = null;

	            if (currentobj.Meta.Name == "ComponentAssembly")
	            {
	                var componentAssembly = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(currentobj);
	                portCompositionChildren = componentAssembly.Children.PortCompositionCollection;
	                connectorCompositionChildren = componentAssembly.Children.ConnectorCompositionCollection;                    
	            }
	            else if (currentobj.Meta.Name == "DesignContainer")
	            {
	                var designContainer = ISIS.GME.Dsml.CyPhyML.Classes.DesignContainer.Cast(currentobj);
	                portCompositionChildren = designContainer.Children.PortCompositionCollection;
	                connectorCompositionChildren = designContainer.Children.ConnectorCompositionCollection;   
	            }

                //=================================================================
                // Process the port connections
                //=================================================================

                NetworkManager portNetworkManager = new NetworkManager();

            	foreach (CyPhy.PortComposition port in portCompositionChildren)
                {
                    Endpoint dstEndPoint = new Endpoint(port.DstEnd, port.GenericDstEndRef);
                    Endpoint srcEndPoint = new Endpoint(port.SrcEnd, port.GenericSrcEndRef);

                    //GMEConsole.Out.WriteLine("Found port: dst = '{0}' in the '{1}' {2}.",
                    //    dstEndPoint.Name,
                    //    dstEndPoint.ParentName,
                    //    dstEndPoint.ParentKind);

                    //GMEConsole.Out.WriteLine("            src = '{0}' in the '{1}' {2}.",
                    //    srcEndPoint.Name,
                    //    srcEndPoint.ParentName,
                    //    srcEndPoint.ParentKind);

                    // Create a network from the endpoints
                    List<Endpoint> nodes = new List<Endpoint>() { srcEndPoint, dstEndPoint };
                    Network newNetwork = new Network(nodes);
                    portNetworkManager.Add(newNetwork);
                }

                //=================================================================
                // Process the connector connections
                //=================================================================

                NetworkManager connectorNetworkManager = new NetworkManager();

            	foreach (CyPhy.ConnectorComposition connector in connectorCompositionChildren)
                {
                    Endpoint dstEndPoint = new Endpoint(connector.DstEnd, connector.GenericDstEndRef);
                    Endpoint srcEndPoint = new Endpoint(connector.SrcEnd, connector.GenericSrcEndRef);

                    // Create a network from the endpoints
                    List<Endpoint> nodes = new List<Endpoint>() { srcEndPoint, dstEndPoint };
                    Network newNetwork = new Network(nodes);
                    connectorNetworkManager.Add(newNetwork);
                }

                // Make sure the network names are unique.
                NetworkNameChecker.Init();
                NetworkNameChecker.Update(ref portNetworkManager.NetworkList);
                NetworkNameChecker.Update(ref connectorNetworkManager.NetworkList);

                // Display all the networks.

                Logger.WriteInfo(string.Format("===== Found {0} port networks on {1}: =====\n", 
                    portNetworkManager.NetworkList.Count,
                    currentobj.Name));

                foreach (Network mergedNet in portNetworkManager.NetworkList)
                {
                    using (StringReader sr = new StringReader(mergedNet.ToString()))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            GMEConsole.Out.WriteLine(line);
                        }
                    }
                }

                Logger.WriteInfo(string.Format("===== Found {0} connector networks on {1}: =====\n", 
                    connectorNetworkManager.NetworkList.Count,
                    currentobj.Name));

                foreach (Network mergedNet in connectorNetworkManager.NetworkList)
                {
                    using (StringReader sr = new StringReader(mergedNet.ToString()))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            GMEConsole.Out.WriteLine(line);
                        }
                    }
                }
            }

            Logger.WriteInfo("The ShowNet interpreter has finished.");

            if (ownLogger)
            {
                Logger.Dispose();
                Logger = null;
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
