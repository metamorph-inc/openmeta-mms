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
using CyPhyGUIs;
using System.Windows.Forms;

using System.Collections;

namespace CyPhy2RF
{
    /// <summary>
    /// This class implements the necessary COM interfaces for a GME interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CyPhy2RFInterpreter : IMgaComponentEx, IGMEVersionInfo, ICyPhyInterpreter
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
        /// Don't perform MGA operations here unless you open a tansaction.
        /// </summary>
        /// <param name="project">The handle of the project opened in GME, for which the interpreter was called.</param>
        public void Initialize(MgaProject project)
        {
            if (Logger == null)
                Logger = new CyPhyGUIs.GMELogger(project, ComponentName);
            MgaGateway = new MgaGateway(project);
            project.CreateTerritoryWithoutSink(out MgaGateway.territory);       
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
            // TODO: Add your interpreter code
            Logger.WriteDebug("Running interpreter...");
			
			// Get RootFolder
			IMgaFolder rootFolder = project.RootFolder;
			Logger.WriteDebug(rootFolder.Name);
            
            // To use the domain-specific API:
            //  Create another project with the same name as the paradigm name
            //  Copy the paradigm .mga file to the directory containing the new project
            //  In the new project, install the GME DSMLGenerator NuGet package (search for DSMLGenerator)
            //  Add a Reference in this project to the other project
            //  Add "using [ParadigmName] = ISIS.GME.Dsml.[ParadigmName].Classes.Interfaces;" to the top of this file
            // if (currentobj.Meta.Name == "KindName")
            // [ParadigmName].[KindName] dsCurrentObj = ISIS.GME.Dsml.[ParadigmName].Classes.[KindName].Cast(currentobj);			
                        
        }

        #region IMgaComponentEx Members

        MgaGateway MgaGateway { get; set; }
        CyPhyGUIs.GMELogger Logger { get; set; }

        public void InvokeEx(MgaProject project, MgaFCO currentobj, MgaFCOs selectedobjs, int param)
        {
            if (!enabled)
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
                parameters.ProjectDirectory = Path.GetDirectoryName(currentobj.Project.ProjectConnStr.Substring("MGA=".Length));

                // set up the output directory
                MgaGateway.PerformInTransaction(delegate
                {
                    string outputDirName = project.Name;
                    if (currentobj != null)
                    {
                        outputDirName = currentobj.Name;
                    }

                    parameters.OutputDirectory = Path.GetFullPath(Path.Combine(
                        parameters.ProjectDirectory,
                        "results",
                        outputDirName));

                    //this.Parameters.PackageName = SystemC.Factory.GetModifiedName(currentobj.Name);
                });

                PreConfigArgs preConfigArgs = new PreConfigArgs();
                preConfigArgs.ProjectDirectory = parameters.ProjectDirectory;

                // call the preconfiguration with no parameters and get preconfig
                var preConfig = this.PreConfig(preConfigArgs);

                // get previous GUI config
                var previousConfig = META.ComComponent.DeserializeConfiguration(
                    parameters.ProjectDirectory,
                    typeof(CyPhy2RF_Settings),
                    this.ComponentProgID);

                // get interpreter config through GUI
                var config = this.DoGUIConfiguration(preConfig, previousConfig);
                if (config == null)
                {
                    Logger.WriteWarning("Operation cancelled by the user.");
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
                if (MgaGateway != null && MgaGateway.territory != null)
                {
                    MgaGateway.territory.Destroy();
                }
                MgaGateway = null;
                project = null;
                currentobj = null;
                selectedobjs = null;
                if (Logger != null)
                {
                    Logger.Dispose();
                    Logger = null;
                }
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
        /// Full path to the log file.
        /// </summary>
        private string LogFilePath
        {
            get
            {
                return Path.Combine(this.result.LogFileDirectory, this.LogFileFilename);
            }
        }

        /// <summary>
        /// ProgId of the configuration class of this interpreter.
        /// </summary>
        public string InterpreterConfigurationProgId
        {
            get
            {
                return (typeof(CyPhy2RF_Settings).GetCustomAttributes(typeof(ProgIdAttribute), false)[0] as ProgIdAttribute).Value;
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
            //var preConfig = new CyPhy2RF_v2PreConfiguration()
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
        /// <returns>Null if operation is cancelled by the user. Otherwise returns with a new
        /// configuration object.</returns>
        public IInterpreterConfiguration DoGUIConfiguration(
            IInterpreterPreConfiguration preConfig,
            IInterpreterConfiguration previousConfig)
        {
            #pragma warning disable 0219
            DialogResult ok = DialogResult.Cancel;
            #pragma warning restore 0219
            
            var settings = previousConfig as CyPhy2RF_Settings;

            if (settings == null)
            {
                settings = new CyPhy2RF_Settings();
            }

            //using (MainForm mf = new MainForm(settings, (preConfig as CyPhy2RF_v2PreConfiguration).ProjectDirectory))
            //{
            // show main form
            //    ok = mf.ShowDialog();
            //}

            //if (ok == DialogResult.OK)
            {
                return settings;
            }

            //return null;
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
            this.result.Labels = "RF";

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
                    Logger.WriteInfo("RF simulation has been created. [SUCCESS: {0}, Labels: {1}]", this.result.Success, this.result.Labels);
                }
                else
                {
                    Logger.WriteError("RF simulation cannot be created! See error messages above.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("Exception: {0}<br> {1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (MgaGateway != null &&
                    MgaGateway.territory != null)
                {
                    MgaGateway.territory.Destroy();
                }
                MgaGateway = null;
                if (Logger != null)
                {
                    Logger.Dispose();
                    Logger = null;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            META.Logger.RemoveFileListener(this.ComponentName);

            return this.result;
        }

        private void PrintRuntimeStatistics()
        {
            Logger.WriteInfo("======================================================");
            Logger.WriteInfo("Start time: {0}", this.startTime);
            foreach (var time in this.runtime)
            {
                Logger.WriteInfo("{0} = {1}", time.Item1, time.Item2);
            }
            Logger.WriteInfo("======================================================");
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
                var elaborator = new CyPhyElaborateCS.CyPhyElaborateCSInterpreter();
                elaborator.Initialize(project);

                Logger.WriteInfo("Elaborating model...");
                System.Windows.Forms.Application.DoEvents();

                int verbosity = 128;
                result = elaborator.RunInTransaction(project, currentobj, selectedobjs, verbosity);

                if (this.result.Traceability == null)
                {
                    this.result.Traceability = new META.MgaTraceability();
                }

                if (elaborator.Traceability != null)
                {
                    elaborator.Traceability.CopyTo(this.result.Traceability);
                }
                //this.Logger.WriteDebug("Elaboration is done.");
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
                return false;
            }

            return true;
        }

        private MgaFCO SurrogateMaster(
            MgaProject project,
            MgaFCO currentobj,
            MgaFCOs selectedobjs,
            int param,
            bool expand = true)
        {
            MgaFolder f = currentobj.ParentFolder;

            if (f == null)
            {
                throw new Exception("Testbench does not have a TestFolder parent!");
            }

            MgaFolder fNew = f.CreateFolder(f.MetaFolder);

            fNew.Name = currentobj.Name + DateTime.Now.ToString(" (MM-dd-yyyy HH:mm:ss)");

            MgaFCO newTestbench = fNew.CopyFCODisp(currentobj);
            //newTestbench.Name += " TestName";

            return newTestbench;
        }

        #endregion

        #region CyPhy2RF Specific code

        /// <summary>
        /// This function does the job. CyPhy2RF translation.
        /// </summary>
        private void WorkInMainTransaction()
        {
            this.result.Success = true;

            var config = (this.mainParameters.config as CyPhy2RF_Settings);
            RF.CodeGenerator.Mode mode = RF.CodeGenerator.Mode.DIRECTIVITY;
            if (config.doDirectivity != null)
            {
                Logger.WriteInfo("CyPhy2RF mode selected : directivity");
                mode = RF.CodeGenerator.Mode.DIRECTIVITY;
            }

            if (config.doSAR != null)
            {
                Logger.WriteInfo("CyPhy2RF mode selected : SAR");
                mode = RF.CodeGenerator.Mode.SAR;
            }

            // Elaborator
            var elaboratorSuccess = this.CallElaborator(
                this.mainParameters.Project,
                this.mainParameters.CurrentFCO,
                this.mainParameters.SelectedFCOs,
                this.mainParameters.StartModeParam);
            this.UpdateSuccess("Elaborator", elaboratorSuccess);

            bool successTranslation = true;
            try
            {
                Directory.CreateDirectory(OutputDirectory);

                // 1. Generate OpenEMS simulation model
                var rfCodeGenerator = new RF.CodeGenerator(this.mainParameters, mode, this.Logger);
                rfCodeGenerator.GenerateCode();
                successTranslation = true;

                // 2. Prepare batch file to be invoked by the job manager
                string cmdFileName = "run_dir_simulation.cmd";
                switch (mode)
                {
                    case RF.CodeGenerator.Mode.DIRECTIVITY:
                        cmdFileName = "run_dir_simulation.cmd";
                        File.WriteAllText(Path.Combine(OutputDirectory, cmdFileName), CyPhy2RF.Properties.Resources.run_dir_simulation);
                        break;
                    case RF.CodeGenerator.Mode.SAR:
                        cmdFileName = "run_sar_simulation.cmd";
                        File.WriteAllText(Path.Combine(OutputDirectory, cmdFileName), CyPhy2RF.Properties.Resources.run_sar_simulation);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Mode", "CyPhy2RF simulation mode not supported");
                }
                this.result.RunCommand = cmdFileName;
            }
            catch (ApplicationException ex)
            {
                Logger.WriteError(ex.Message);
                successTranslation = false;
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
                successTranslation = false;
            }
            this.UpdateSuccess("RF simulation translation", successTranslation);
        }

        #endregion
    }
}
