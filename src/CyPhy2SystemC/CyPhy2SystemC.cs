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
using Microsoft.Win32;

namespace CyPhy2SystemC
{
    /// <summary>
    /// This class implements the necessary COM interfaces for a GME interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CyPhy2SystemCInterpreter : IMgaComponentEx, IGMEVersionInfo, ICyPhyInterpreter
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
            GMEConsole = GMEConsole.CreateFromProject(project);
            MgaGateway = new MgaGateway(project);
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
            GMEConsole.Out.WriteLine("Running interpreter...");
			
			// Get RootFolder
			IMgaFolder rootFolder = project.RootFolder;
			GMEConsole.Out.WriteLine(rootFolder.Name);
            
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
        GMEConsole GMEConsole { get; set; }

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
                    typeof(CyPhy2SystemC_Settings),
                    this.ComponentProgID);

                // get interpreter config through GUI
                var config = this.DoGUIConfiguration(preConfig, previousConfig);
                if (config == null)
                {
                    GMEConsole.Warning.WriteLine("Operation cancelled by the user.");
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
                GMEConsole.Error.WriteLine("Interpretation failed {0}<br>{1}", ex.Message, ex.StackTrace);
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
                GMEConsole.Info.WriteLine("{0} : OK", message);
            }
            else
            {
                GMEConsole.Error.WriteLine("{0} : FAILED", message);
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
                return (typeof(CyPhy2SystemC_Settings).GetCustomAttributes(typeof(ProgIdAttribute), false)[0] as ProgIdAttribute).Value;
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
            //var preConfig = new CyPhy2SystemC_v2PreConfiguration()
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
            
            var settings = previousConfig as CyPhy2SystemC_Settings;

            if (settings == null)
            {
                settings = new CyPhy2SystemC_Settings();
            }

            RegistryKey vc_full;
            RegistryKey vc_express;
            using (var hklm_32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (vc_full = hklm_32.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\10.0\Setup\VC"))
            using (vc_express = hklm_32.OpenSubKey(@"SOFTWARE\Microsoft\VCExpress\10.0\Setup\VC"))
            {
                if (vc_full == null && vc_express == null)
                {
                    string msg = "Visual Studio 2010 or Visual C++ 2010 Express is not detected on your system. One of the two is required to run CyPhy2SystemC.\n\nAttempt to run CyPhy2SystemC anyway?";
                    if (MessageBox.Show(msg, "CyPhy2SystemC", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return null;
                    }
                }
            }

            //using (MainForm mf = new MainForm(settings, (preConfig as CyPhy2SystemC_v2PreConfiguration).ProjectDirectory))
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
            this.runtime.Clear();

            this.mainParameters = parameters;

            var configSuccess = this.mainParameters != null;

            this.UpdateSuccess("Configuration", configSuccess);

            this.result.Labels = "SystemC";
            this.result.LogFileDirectory = Path.Combine(this.mainParameters.ProjectDirectory, "log");
            this.LogFileFilename = this.ComponentName + "." + System.Diagnostics.Process.GetCurrentProcess().Id + ".log";

            try
            {
                META.Logger.AddFileListener(this.LogFilePath, this.ComponentName, this.mainParameters.Project);
            }
            catch (Exception ex) // logger construction may fail, which should be an ignorable exception
            {
                GMEConsole.Warning.WriteLine("Error in Logger construction: {0}", ex.Message);
            }

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
                    GMEConsole.Info.WriteLine("Generated files are here: <a href=\"file:///{0}\" target=\"_blank\">{0}</a>", this.mainParameters.OutputDirectory);
                    GMEConsole.Info.WriteLine("SystemC Composer has finished. [SUCCESS: {0}, Labels: {1}]", this.result.Success, this.result.Labels);
                }
                else
                {
                    GMEConsole.Error.WriteLine("SystemC Composer failed! See error messages above.");
                }
            }
            catch (Exception ex)
            {
                GMEConsole.Error.WriteLine("Exception: {0}<br> {1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                MgaGateway = null;
                GMEConsole = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            META.Logger.RemoveFileListener(this.ComponentName);

            //var SystemCSettings = this.mainParameters.config as CyPhy2SystemC_Settings;

            return this.result;
        }

        private void PrintRuntimeStatistics()
        {
            GMEConsole.Info.WriteLine("======================================================");
            GMEConsole.Info.WriteLine("Start time: {0}", this.startTime);
            foreach (var time in this.runtime)
            {
                GMEConsole.Info.WriteLine("{0} = {1}", time.Item1, time.Item2);
            }
            GMEConsole.Info.WriteLine("======================================================");
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
            try
            {

                // call elaborator and expand the references
                Type t = Type.GetTypeFromProgID("MGA.Interpreter.CyPhyElaborate");
                IMgaComponentEx elaborator = Activator.CreateInstance(t) as IMgaComponentEx;
                elaborator.Initialize(project);

                if (expand)
                {
                    elaborator.ComponentParameter["automated_expand"] = "true";
                }
                else
                {
                    elaborator.ComponentParameter["automated_collapse"] = "true";
                }

                GMEConsole.Info.WriteLine("Elaborating model...");
                System.Windows.Forms.Application.DoEvents();

                // elaborator.UnrollConnectors = true;
                elaborator.InvokeEx(project, currentobj, selectedobjs, param);

                CyPhyCOMInterfaces.IMgaTraceability traceability = elaborator.ComponentParameter["traceability"] as CyPhyCOMInterfaces.IMgaTraceability;

                this.result.Traceability = new META.MgaTraceability();

                if (traceability != null)
                {
                    traceability.CopyTo(this.result.Traceability);
                }

                GMEConsole.Info.WriteLine("Elaboration is done.");
                System.Windows.Forms.Application.DoEvents();

                // TODO: get exception message(s) from elaborator
                string msgs = elaborator.ComponentParameter["exception"] as string;
                if (string.IsNullOrWhiteSpace(msgs) == false)
                {
                    GMEConsole.Error.WriteLine("Elaborator exception occurred: {0}", msgs);
                    System.Windows.Forms.Application.DoEvents();
                    throw new Exception(msgs);
                }

            }
            catch (Exception ex)
            {
                GMEConsole.Error.WriteLine(ex);
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

        #region CyPhy2SystemC Specific code

        /// <summary>
        /// This function does the job. CyPhy2SystemC translation.
        /// </summary>
        private void WorkInMainTransaction()
        {
            this.result.Success = true;

            // 1) check model, if fails return success = false
            // Make attached library paths available for Checker
            //GMEConsole.Info.WriteLine("Checking rules...");
            //Rules.Checker.GMEConsole = GMEConsole;
            //var checker = new Rules.Checker(this.mainParameters);
            //var successRules = checker.Check(this.result.Traceability);
            //this.UpdateSuccess("Model check", successRules);

            //if (this.result.Success == false)
            //{
            //    checker.PrintDetails();
            //    return;
            //}

            // surrogate the master interpreter's role - till we figure out a way to hook into the master interp
            //{
            //    MgaFCO newCurrent = this.SurrogateMaster(this.mainParameters.Project, this.mainParameters.CurrentFCO, this.mainParameters.SelectedFCOs,
            //        this.mainParameters.StartModeParam);
            //    var newParameters = new InterpreterMainParameters()
            //        {
            //            Project = this.mainParameters.Project,
            //            CurrentFCO = newCurrent,
            //            SelectedFCOs = this.mainParameters.SelectedFCOs,
            //            StartModeParam = this.mainParameters.StartModeParam,
            //            config = this.mainParameters.config,
            //            OutputDirectory = this.mainParameters.OutputDirectory,
            //            ProjectDirectory = this.mainParameters.ProjectDirectory
            //        };
            //    this.mainParameters = newParameters;
            //}

            // 2) try to call dependencies - elaborate, cyber, Bond Graph interpreter
            var elaboratorSuccess = this.CallElaborator(this.mainParameters.Project, this.mainParameters.CurrentFCO, this.mainParameters.SelectedFCOs,
                this.mainParameters.StartModeParam);
            this.UpdateSuccess("Elaborator", elaboratorSuccess);

            bool successTranslation = true;
            try
            {
                // Generate SystemC source code and projects
                SystemC.CodeGenerator.GMEConsole = GMEConsole;
                var systemCCodeGenerator = new SystemC.CodeGenerator(this.mainParameters);
                systemCCodeGenerator.GenerateSystemCCode();
                successTranslation = true;


                // Generate build and test job
                String p_vcsln = Path.Combine(OutputDirectory, "SystemCTestBench.sln");
                String p_cmd = Path.Combine(OutputDirectory, "build.cmd");
                String sim_exe_name = "Release\\SystemCTestBench.exe";
                String p_exe = Path.Combine(OutputDirectory, sim_exe_name);

                // See if "msbuild" is in path.
                // If not, check the registry to try to get a path for it.
                // Used this technique to get registry key: http://stackoverflow.com/a/445323
                String s_RUNCMD = "@ECHO OFF" + Environment.NewLine +
                                  "setlocal ENABLEEXTENSIONS" + Environment.NewLine +
                                  "set KEY_NAME=\"HKLM\\SOFTWARE\\Microsoft\\MSBuild\\ToolsVersions\\4.0\"" + Environment.NewLine +
                                  "set VALUE_NAME=\"MSBuildToolsPath\"" + Environment.NewLine +
                                  "FOR /F \"usebackq skip=2 tokens=1-3\" %%A IN (`REG QUERY %KEY_NAME% /v %VALUE_NAME% 2^>nul`) DO (" + Environment.NewLine +
                                  "  set ValueName=%%A" + Environment.NewLine +
                                  "  set ValueType=%%B" + Environment.NewLine +
                                  "  set ValueValue=%%C" + Environment.NewLine +
                                  "  set _msbuild=%%Cmsbuild.exe" + Environment.NewLine +
                                  ")" + Environment.NewLine +
                                  String.Format("%_msbuild% {0} /p:Configuration=Release;VCTargetsPath=\"C:\\Program Files (x86)\\MSBuild\\Microsoft.Cpp\\v4.0\"\\ /p:Platform=Win32 /m", Path.GetFileName(p_vcsln)) + Environment.NewLine +
                                  "EXIT /b %errorlevel%" + Environment.NewLine;

                File.WriteAllText(p_cmd, s_RUNCMD);

                File.WriteAllText(Path.Combine(OutputDirectory, "run_simulation.cmd"),
                    "@ECHO OFF" + Environment.NewLine +
                    "PUSHD %~dp0" + Environment.NewLine +
                    sim_exe_name + Environment.NewLine +
                    "ECHO Press any key to exit" + Environment.NewLine +
                    "PAUSE >nul" + Environment.NewLine);
                
                this.result.RunCommand = Path.GetFileName(p_cmd);
            }
            catch (Exception ex)
            {
                GMEConsole.Error.WriteLine(ex);
                successTranslation = false;
            }
            this.UpdateSuccess("SystemC translation", successTranslation);
        }

        #endregion
    }
}
