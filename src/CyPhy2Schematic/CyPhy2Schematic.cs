using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GME.CSharp;
using GME;
using GME.MGA;
using GME.MGA.Core;
using System.Linq;
using META;

using CyPhyGUIs;

using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using CyPhyComponentFidelitySelector;
using CyPhy2Schematic.Schematic;
using System.Xml;
using System.Xml.Linq;

namespace CyPhy2Schematic
{
    /// <summary>
    /// This class implements the necessary COM interfaces for a GME interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CyPhy2SchematicInterpreter : IMgaComponentEx, IGMEVersionInfo, ICyPhyInterpreter
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
        /// This function is called for each interpreter invocation before Main.
        /// Don't perform MGA operations here unless you open a transaction.
        /// </summary>
        /// <param name="project">The handle of the project opened in GME, for which the interpreter was called.</param>
        public void Initialize(MgaProject project)
        {
            if (Logger == null)
                Logger = new GMELogger(project, ComponentName);
            MgaGateway = new MgaGateway(project);
        }

        private CyPhy2Schematic_Settings InitializeSettingsFromWorkflow(CyPhy2Schematic_Settings settings)
        {
            // Seed with settings from workflow.
            String str_WorkflowParameters = "";
            try
            {
                MgaGateway.PerformInTransaction(delegate
                {
                    var testBench = TonkaClasses.TestBench.Cast(this.mainParameters.CurrentFCO);
                    var workflowRef = testBench.Children.WorkflowRefCollection.FirstOrDefault();
                    var workflow = workflowRef.Referred.Workflow;
                    var taskCollection = workflow.Children.TaskCollection;
                    var myTask = taskCollection.FirstOrDefault(t => t.Attributes.COMName == this.ComponentProgID);
                    str_WorkflowParameters = myTask.Attributes.Parameters;
                },
                transactiontype_enum.TRANSACTION_NON_NESTED,
                abort: true
                );

                Dictionary<string, string> dict_WorkflowParameters = (Dictionary<string, string>)
                    Newtonsoft.Json.JsonConvert.DeserializeObject(str_WorkflowParameters, typeof(Dictionary<string, string>));

                if (dict_WorkflowParameters != null)
                {
                    settings = new CyPhy2Schematic_Settings();
                    foreach (var property in settings.GetType().GetProperties()
                                                               .Where(p => p.GetCustomAttributes(typeof(WorkflowConfigItemAttribute), false).Any())
                                                               .Where(p => dict_WorkflowParameters.ContainsKey(p.Name)))
                    {
                        property.SetValue(settings, dict_WorkflowParameters[property.Name], null);
                    }
                }
            }
            catch (NullReferenceException)
            {
                Logger.WriteInfo("Could not find workflow object for CyPhy2Schematic interpreter.");
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                Logger.WriteWarning("Workflow Parameter has invalid Json String: {0}", str_WorkflowParameters);
            }

            return settings;
        }



        #region IMgaComponentEx Members

        private MgaGateway MgaGateway { get; set; }
        private GMELogger Logger { get; set; }

        public void InvokeEx(MgaProject project, MgaFCO currentobj, MgaFCOs selectedobjs, int param)
        {
            if (this.enabled == false)
            {
                return;
            }

            try
            {
                // Need to call this interpreter in the same way as the MasterInterpreter will call it.
                // initialize main parameters
                var parameters = new InterpreterMainParameters()
                {
                    Project = project,
                    CurrentFCO = currentobj,
                    SelectedFCOs = selectedobjs,
                    StartModeParam = param
                };

                this.mainParameters = parameters;
                parameters.ProjectDirectory = project.GetRootDirectoryPath();

                // set up the output directory
                MgaGateway.PerformInTransaction(delegate
                {
                    string outputDirName = project.Name;
                    if (currentobj != null)
                    {
                        outputDirName = currentobj.Name;
                    }

                    var outputDirAbsPath = Path.GetFullPath(Path.Combine(
                                                            parameters.ProjectDirectory,
                                                            "results",
                                                            outputDirName));

                    parameters.OutputDirectory = outputDirAbsPath;

                    if (Directory.Exists(outputDirAbsPath))
                    {
                        Logger.WriteWarning("Output directory {0} already exists. Unexpected behavior may result.", outputDirAbsPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(outputDirAbsPath);
                    }

                    //this.Parameters.PackageName = Schematic.Factory.GetModifiedName(currentobj.Name);
                });

                PreConfigArgs preConfigArgs = new PreConfigArgs()
                {
                    ProjectDirectory = parameters.ProjectDirectory,
                    Project = parameters.Project
                };

                // call the preconfiguration with no parameters and get preconfig
                var preConfig = this.PreConfig(preConfigArgs);

                // get previous GUI config
                var settings_ = META.ComComponent.DeserializeConfiguration(parameters.ProjectDirectory,
                                                                           typeof(CyPhy2Schematic_Settings),
                                                                           this.ComponentProgID);
                CyPhy2Schematic_Settings settings = (settings_ != null) ? settings_ as CyPhy2Schematic_Settings : new CyPhy2Schematic_Settings();

                // Set configuration based on Workflow Parameters. This will override all [WorkflowConfigItem] members.
                settings = InitializeSettingsFromWorkflow(settings);

                // Don't skip GUI -- we've been invoked directly here.
                settings.skipGUI = null;

                // get interpreter config through GUI
                var config = this.DoGUIConfiguration(preConfig, settings);
                if (config == null)
                {
                    Logger.WriteWarning("Operation canceled by the user.");
                    return;
                }

                // if config is valid save it and update it on the file system
                META.ComComponent.SerializeConfiguration(parameters.ProjectDirectory, config, this.ComponentProgID);

                // assign the new configuration to mainParameters
                parameters.config = config;

                // call the main (ICyPhyComponent) function
                this.Main(parameters);
            }
            catch (Exception ex)
            {
                Logger.WriteError("Interpretation failed {0}<br>{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                DisposeLogger();
                MgaGateway = null;
                project = null;
                currentobj = null;
                selectedobjs = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
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

        #region Dependent Interpreters

        private bool CallElaborator(
            MgaProject project,
            MgaFCO currentobj,
            MgaFCOs selectedobjs,
            int param,
            bool expand = true)
        {
            bool result = false;
            try
            {
                this.Logger.WriteDebug("Elaborating model...");
                var elaborator = new CyPhyElaborateCS.CyPhyElaborateCSInterpreter()
                {
                    Logger = Logger
                };
                elaborator.Initialize(project);
                int verbosity = 128;
                result = elaborator.RunInTransaction(project, currentobj, selectedobjs, verbosity, mainParameters.OutputDirectory);

                if (this.result.Traceability == null)
                {
                    this.result.Traceability = new META.MgaTraceability();
                }

                if (elaborator.Traceability != null)
                {
                    elaborator.Traceability.CopyTo(this.result.Traceability);
                    elaborator.Traceability.CopyTo(Logger.Traceability);
                }
                this.Logger.WriteDebug("Elaboration is done.");
            }
            catch (Exception ex)
            {
                this.Logger.WriteError("Exception occurred in Elaborator : {0}", ex.ToString());
                result = false;
            }

            return result;
        }

        #endregion

        #region CyPhyGUIs

        /// <summary>
        /// Result of the latest run of this interpreter.
        /// </summary>
        private InterpreterResult result = new InterpreterResult();

        /// <summary>
        /// Parameter of this run.
        /// </summary>
        private IInterpreterMainParameters mainParameters { get; set; }

        /// <summary>
        /// Output directory where all files must be generated
        /// </summary>
        private string OutputDirectory
        {
            get
            {
                return this.mainParameters.OutputDirectory;
            }
        }

        private void UpdateSuccess(string message, bool success)
        {
            this.result.Success = this.result.Success && success;

            this.runtime.Enqueue(new Tuple<string, TimeSpan>(message, DateTime.Now - this.startTime));
            if (success)
            {
                Logger.WriteInfo("{0} : OK", message);
            }
            else
            {
                Logger.WriteError("{0} : FAILED", message);
            }
        }

        /// <summary>
        /// Name of the log file. (It is not a full path)
        /// </summary>
        private string LogFileFilename { get; set; }


        /// <summary>
        /// ProgId of the configuration class of this interpreter.
        /// </summary>
        public string InterpreterConfigurationProgId
        {
            get
            {
                return (typeof(CyPhy2Schematic_Settings).GetCustomAttributes(typeof(ProgIdAttribute), false)[0] as ProgIdAttribute).Value;
            }
        }

        /// <summary>
        /// Preconfig gets called first. No transaction is open, but one may be opened.
        /// In this function model may be processed and some object ids get serialized
        /// and returned as preconfiguration (project-wise configuration).
        /// </summary>
        /// <param name="preConfigParameters"></param>
        /// <returns>Null if no configuration is required by the DoGUIConfig.</returns>
        public IInterpreterPreConfiguration PreConfig(IPreConfigParameters preConfigParameters)
        {
            //var preConfig = new CyPhy2Schematic_v2PreConfiguration()
            //{
            //    ProjectDirectory = preConfigParameters.ProjectDirectory
            //};

            //return preConfig;
            return null;
        }

        /// <summary>
        /// Shows a form for the user to select/change settings through a GUI. All interactive
        /// GUI operations MUST happen within this function scope.
        /// </summary>
        /// <param name="preConfig">Result of PreConfig</param>
        /// <param name="previousConfig">Previous configuration to initialize the GUI.</param>
        /// <returns>Null if operation is canceled by the user. Otherwise returns with a new
        /// configuration object.</returns>
        public IInterpreterConfiguration DoGUIConfiguration(
            IInterpreterPreConfiguration preConfig,
            IInterpreterConfiguration previousConfig)
        {
            CyPhy2Schematic_Settings settings = (previousConfig as CyPhy2Schematic_Settings);

            // If none found, we should do GUI.
            // If available, seed the GUI with the previous settings.
            if (settings == null || settings.skipGUI == null)
            {
                // Do GUI
                var gui = new CyPhy2Schematic.GUI.CyPhy2Schematic_GUI();
                gui.settings = settings;

                var result = gui.ShowDialog();

                if (result == DialogResult.OK)
                {
                    return gui.settings;
                }
                else
                {
                    // USER CANCELED.
                    return null;
                }
            }

            return settings;
        }

        private Queue<Tuple<string, TimeSpan>> runtime = new Queue<Tuple<string, TimeSpan>>();
        private DateTime startTime = DateTime.Now;

        /// <summary>
        /// No GUI and interactive elements are allowed within this function.
        /// </summary>
        /// <param name="parameters">Main parameters for this run and GUI configuration.</param>
        /// <returns>Result of the run, which contains a success flag.</returns>
        public IInterpreterResult Main(IInterpreterMainParameters parameters)
        {
            if (Logger == null)
                Logger = new GMELogger(parameters.Project, ComponentName);

            this.runtime.Clear();
            this.mainParameters = parameters;
            var configSuccess = this.mainParameters != null;
            this.UpdateSuccess("Configuration", configSuccess);
            this.result.Labels = "Schematic";

            try
            {
                MgaGateway.PerformInTransaction(delegate
                {
                    this.WorkInMainTransaction();
                },
                transactiontype_enum.TRANSACTION_NON_NESTED,
                abort: true
                );

                this.PrintRuntimeStatistics();
                if (this.result.Success)
                {
                    Logger.WriteInfo("Generated files are here: <a href=\"file:///{0}\" target=\"_blank\">{0}</a>", this.mainParameters.OutputDirectory);
                    Logger.WriteInfo("Schematic Interpreter has finished. [SUCCESS: {0}, Labels: {1}]", this.result.Success, this.result.Labels);
                }
                else
                {
                    Logger.WriteError("Schematic Interpreter failed! See error messages above.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("Exception: {0}<br> {1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                DisposeLogger();
                MgaGateway = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            META.Logger.RemoveFileListener(this.ComponentName);

            var SchematicSettings = this.mainParameters.config as CyPhy2Schematic_Settings;

            return this.result;
        }

        public void DisposeLogger()
        {
            if (Logger != null)
                Logger.Dispose();
            Logger = null;
        }

        private void PrintRuntimeStatistics()
        {
            Logger.WriteDebug("======================================================");
            Logger.WriteDebug("Start time: {0}", this.startTime);
            foreach (var time in this.runtime)
            {
                Logger.WriteDebug("{0} = {1}", time.Item1, time.Item2);
            }
            Logger.WriteDebug("======================================================");
        }

        #endregion

        public class MgaObjectCompararer : IEqualityComparer<IMgaObject>
        {
            public bool Equals(IMgaObject x, IMgaObject y)
            {
                return x.Project == y.Project && x.ID == y.ID;
            }

            public int GetHashCode(IMgaObject obj)
            {
                return obj.ID.GetHashCode();
            }
        }

        #region CyPhy2Schematic Specific code

        /// <summary>
        /// This function does the job. CyPhy2Schematic translation.
        /// </summary>
        private void WorkInMainTransaction()
        {
            var config = (this.mainParameters.config as CyPhy2Schematic_Settings);
            string placementDotBat = "placement.bat";
            string placeonlyDotBat = "placeonly.bat";

            this.result.Success = true;

            GetIDsOfEverything(this.mainParameters.CurrentFCO);

            // Call Elaborator
            var elaboratorSuccess = this.CallElaborator(this.mainParameters.Project,
                                                        this.mainParameters.CurrentFCO,
                                                        this.mainParameters.SelectedFCOs,
                                                        this.mainParameters.StartModeParam);

            this.UpdateSuccess("Elaborator", elaboratorSuccess);

            Schematic.CodeGenerator.Mode mode = Schematic.CodeGenerator.Mode.EDA;

            ISet<IMgaObject> selectedSpiceModels = null;
            if (config.doSpice != null)
            {
                FidelitySelectionRules xpaths = FidelitySelectionRules.DeserializeSpiceFidelitySelection(this.mainParameters.CurrentFCO);
                if (xpaths != null && xpaths.rules.Count > 0)
                {
                    var testBench = TonkaClasses.TestBench.Cast(this.mainParameters.CurrentFCO);
                    var ca = testBench.Children.ComponentAssemblyCollection.FirstOrDefault();
                    if (ca == null)
                    {
                        Logger.WriteFailed("No valid component assembly in testbench {0}", testBench.Name);
                        return;
                    }
                    selectedSpiceModels = new HashSet<IMgaObject>(new MgaObjectCompararer());
                    Dictionary<XElement, Tonka.SPICEModel> map = new Dictionary<XElement, Tonka.SPICEModel>();
                    foreach (var e in FidelitySelectionRules.SelectElements(FidelitySelectionRules.CreateForAssembly(ca, map), xpaths))
                    {
                        selectedSpiceModels.Add(map[e].Impl);
                    }
                }


                this.result.RunCommand = "runspice.bat";
                mode = Schematic.CodeGenerator.Mode.SPICE;
            }
            else if (config.doSpiceForSI != null)
            {
                this.result.RunCommand = "runspice.bat";
                mode = Schematic.CodeGenerator.Mode.SPICE_SI;
            }
            else
            {
                mode = Schematic.CodeGenerator.Mode.EDA;
                if (config.doChipFit != null)
                {
                    Boolean chipFitViz = false;
                    if (Boolean.TryParse(config.showChipFitVisualizer, out chipFitViz)
                        && chipFitViz)
                    {
                        this.result.RunCommand = "chipfit.bat chipfit_display";
                    }
                    else
                    {
                        this.result.RunCommand = "chipFit.bat";
                    }
                }
                else if (config.doPlaceRoute != null)
                {
                    this.result.RunCommand = placementDotBat;
                }
                else if (config.doPlaceOnly != null)
                {
                    this.result.RunCommand = placeonlyDotBat;
                }
                else
                {
                    this.result.RunCommand = "cmd /c dir";
                }
            }

            bool successTranslation = true;
            try
            {
                var schematicCodeGenerator = new Schematic.CodeGenerator(this.mainParameters, mode, (MgaTraceability)result.Traceability, mgaIdToDomainIDs, selectedSpiceModels);
                schematicCodeGenerator.Logger = Logger;
                this.schematicCodeGenerator = schematicCodeGenerator;

                var gcResult = schematicCodeGenerator.GenerateCode();

                // MOT-782: Prevent autorouting if we've placed components off the board.
                if ((gcResult.bonesFound) && (this.result.RunCommand == placementDotBat))
                {
                    // Found a bone, MOT-782.
                    Logger.WriteWarning("Skipping EAGLE autorouting, since components not found in layout.json were placed off the board.");

                    this.result.RunCommand = placeonlyDotBat;
                    config.doPlaceOnly = config.doPlaceRoute;
                    config.doPlaceRoute = null;
                }

                if (mode == Schematic.CodeGenerator.Mode.EDA &&
                    (config.doPlaceRoute != null || config.doPlaceOnly != null))
                {
                    this.result.RunCommand += gcResult.runCommandArgs;
                }

                successTranslation = true;
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
                Logger.WriteDebug(ex.ToString());
                successTranslation = false;
            }
            finally
            {
                CyPhyBuildVisitor.ComponentInstanceGUIDs = null;
                CyPhyBuildVisitor.Components = null;
                CyPhyBuildVisitor.Ports = null;
                CodeGenerator.partNames = null;
            }
            this.UpdateSuccess("Schematic translation", successTranslation);
        }

        public class IDs
        {
            public string instanceGUID;
            public string managedGUID;
            public string ID;
            public string ConnectorID;

            public override string ToString()
            {
                return managedGUID + "/" + (String.IsNullOrEmpty(instanceGUID) ? "" : (instanceGUID + "/")) + ConnectorID + "/" + ID;
            }
        }
        public Dictionary<string, IDs> mgaIdToDomainIDs = new Dictionary<string, IDs>();
        private CodeGenerator schematicCodeGenerator;

        private void GetIDsOfEverything(MgaFCO testbench)
        {
            Queue<MgaFCO> q = new Queue<MgaFCO>();
            q.Enqueue(testbench);
            var metaProject = testbench.Meta.MetaProject;
            int componentRefId = metaProject.RootFolder.GetDefinedFCOByNameDisp("ComponentRef", true).MetaRef;
            int componentId = metaProject.RootFolder.GetDefinedFCOByNameDisp("Component", true).MetaRef;
            int testComponentId = -1; // TODO //metaProject.RootFolder.GetDefinedFCOByNameDisp("TestComponent", true).MetaRef;
            int testBenchId = metaProject.RootFolder.GetDefinedFCOByNameDisp("TestBench", true).MetaRef;
            int componentAssemblyId = metaProject.RootFolder.GetDefinedFCOByNameDisp("ComponentAssembly", true).MetaRef;
            int connectorId = metaProject.RootFolder.GetDefinedFCOByNameDisp("Connector", true).MetaRef;

            Action<IMgaFCO> addComponentID = (fco) =>
            {
                mgaIdToDomainIDs[fco.ID] = new IDs()
                {
                    instanceGUID = (fco.Meta.Name == typeof(Tonka.Component).Name) ? fco.GetStrAttrByNameDisp("InstanceGUID") : null, // TestComponents do not have InstanceGUIDs
                    managedGUID = fco.GetStrAttrByNameDisp("ManagedGUID")
                    // ID = fco.GetIntAttrByNameDisp("ID").ToString()
                };
            };
            Action<IMgaFCO> addComponentRefID = (fco) =>
            {
                mgaIdToDomainIDs[fco.ID] = new IDs()
                {
                    instanceGUID = fco.GetStrAttrByNameDisp("InstanceGUID"),
                    // managedGUID
                    ID = fco.GetIntAttrByNameDisp("ID").ToString()
                };
            };
            Action<IMgaFCO> addComponentAssemblyID = (fco) =>
            {
                mgaIdToDomainIDs[fco.ID] = new IDs()
                {
                    // instanceGUID =
                    managedGUID = fco.GetStrAttrByNameDisp("ManagedGUID"),
                    ID = fco.GetIntAttrByNameDisp("ID").ToString()
                };
            };

            while (q.Count > 0)
            {
                MgaFCO fco = q.Dequeue();
                if (fco.Meta.ObjType == GME.MGA.Meta.objtype_enum.OBJTYPE_REFERENCE)
                {
                    var reference = (MgaReference)fco;
                    var referred = reference.Referred;
                    if (referred != null)
                    {
                        if (referred.Meta.MetaRef == componentAssemblyId)
                        {
                            if (reference.Meta.MetaRef == componentRefId)
                            {
                                addComponentRefID(reference);
                            }
                            else
                            {
                                addComponentAssemblyID(referred);
                            }
                            foreach (MgaFCO child in ((MgaModel)referred).ChildFCOs)
                            {
                                q.Enqueue(child);
                            }
                        }
                        else if (referred.Meta.MetaRef == componentId || referred.Meta.MetaRef == testComponentId)
                        {
                            addComponentRefID(reference);
                            foreach (MgaFCO child in ((MgaModel)referred).ChildFCOs)
                            {
                                q.Enqueue(child);
                            }
                        }
                    }
                }
                else if (fco.Meta.MetaRef == componentAssemblyId)
                {
                    addComponentAssemblyID(fco);
                    foreach (MgaFCO child in ((MgaModel)fco).ChildFCOs)
                    {
                        q.Enqueue(child);
                    }
                }
                else if (fco.Meta.MetaRef == testBenchId)
                {
                    foreach (MgaFCO child in ((MgaModel)fco).ChildFCOs)
                    {
                        q.Enqueue(child);
                    }
                }
                else if (fco.Meta.MetaRef == connectorId)
                {
                    foreach (MgaFCO child in ((MgaModel)fco).ChildFCOs)
                    {
                        if (child.Meta.Name == "SchematicModelPort")
                        {
                            mgaIdToDomainIDs[child.ID] = new IDs()
                            {
                                // instanceGUID =
                                // managedGUID =
                                ID = child.GetStrAttrByNameDisp("ID"),
                                ConnectorID = fco.GetStrAttrByNameDisp("ID")
                            };
                        }
                    }
                }
                else if (fco.Meta.MetaRef == componentId || fco.Meta.MetaRef == testComponentId)
                {
                    addComponentID(fco);
                }
            }
        }

        #endregion

    }

    public static class TraceabilityExtension
    {
        public static string GetID(this MgaTraceability t, IMgaObject obj)
        {
            String compOriginalID = null;
            if (t.TryGetMappedObject(obj.ID, out compOriginalID))
            {
            }
            else
            {
                compOriginalID = obj.ID;
            }
            return compOriginalID;

        }
    }
}
