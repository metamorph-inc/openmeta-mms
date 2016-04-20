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
using CyPhyGUIs;
using System.Windows.Forms;
using Newtonsoft.Json;

using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using System.Diagnostics;
using META;

namespace CyPhy2CADPCB
{
    /// <summary>
    /// This class implements the necessary COM interfaces for a GME interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CyPhy2CADPCBInterpreter : IMgaComponentEx, IGMEVersionInfo, ICyPhyInterpreter
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
            {
                Logger = new GMELogger(project, this.ComponentName);
            }

            MgaGateway = new MgaGateway(project);
            project.CreateTerritoryWithoutSink(out MgaGateway.territory);
        }

        private CyPhy2CADPCB_Settings InitializeSettingsFromWorkflow(CyPhy2CADPCB_Settings settings)
        {
            // Seed with settings from workflow.
            String str_WorkflowParameters = "";
            try
            {
                MgaGateway.PerformInTransaction(delegate
                {
                    var testBench = CyPhyClasses.TestBench.Cast(this.mainParameters.CurrentFCO);
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
                    settings = new CyPhy2CADPCB_Settings();
                    foreach (var property in settings.GetType().GetProperties()
                                                               .Where(p => p.GetCustomAttributes(typeof(WorkflowConfigItemAttribute), false).Any())
                                                               .Where(p => dict_WorkflowParameters.ContainsKey(p.Name)))
                    {
                        if (dict_WorkflowParameters[property.Name] == null)
                        {
                            property.SetValue(settings, dict_WorkflowParameters[property.Name], null);
                        }
                        else
                        {
                            property.SetValue(settings, dict_WorkflowParameters[property.Name].ToLower(), null);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                Logger.WriteInfo("Could not find workflow object for CyPhy2CADPCB interpreter.");
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                Logger.WriteWarning("Workflow Parameter has invalid Json String: {0}", str_WorkflowParameters);
            }

            return settings;
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
            throw new NotImplementedException("Function Main(MgaProject, MgaFCO, MgaFCOs, ComponentStartMode) not implemented.");
        }


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

        private Queue<Tuple<string, TimeSpan>> runtime = new Queue<Tuple<string, TimeSpan>>();
        private DateTime startTime = DateTime.Now;

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

        public IInterpreterPreConfiguration PreConfig(IPreConfigParameters preConfigParameters)
        {
            return null;
        }

        public IInterpreterConfiguration DoGUIConfiguration(IInterpreterPreConfiguration preconfig, IInterpreterConfiguration previousConfig)
        {
            CyPhy2CADPCB_Settings settings = (previousConfig as CyPhy2CADPCB_Settings);

            if (settings.runLayout == "false" || String.IsNullOrWhiteSpace(settings.runLayout))
            {
                if (String.IsNullOrWhiteSpace(settings.layoutFilePath) && (settings.useSavedLayout == "false" || String.IsNullOrWhiteSpace(settings.useSavedLayout)))
                {
                    // Prompt the user for what layout JSON file they want to use.
                    DialogResult dr;
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.CheckFileExists = true;
                        ofd.DefaultExt = "*.json";
                        ofd.Multiselect = false;
                        ofd.Filter = "JSON file (*.json)|*.json";
                        dr = ofd.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            settings.layoutFilePath = ofd.FileName;
                        }
                        else
                        {
                            // User cancelled
                            return null;
                        }
                    }

                    if (String.IsNullOrWhiteSpace(settings.visualizerType))
                    {
                        settings.visualizerType = "STEP";
                    }
                }
            }

            return settings;
        }

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
            this.result.Labels = "Visualizer";

            try
            {
                MgaGateway.PerformInTransaction(delegate
                {
                    this.WorkInMainTransaction();
                },
                transactiontype_enum.TRANSACTION_NON_NESTED,
                abort: true
                );

                if (this.result.Success)
                {
                    Logger.WriteInfo("Generated files are here: <a href=\"file:///{0}\" target=\"_blank\">{0}</a>", this.mainParameters.OutputDirectory);
                    Logger.WriteInfo("CyPhy2CADPCB Interpreter has finished. [SUCCESS: {0}, Labels: {1}]", this.result.Success, this.result.Labels);
                }
                else
                {
                    Logger.WriteError("CyPhy2CADPCB Interpreter failed! See error messages above.");
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
                DisposeLogger();
                MgaGateway = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            META.Logger.RemoveFileListener(this.ComponentName);

            var SchematicSettings = this.mainParameters.config as CyPhy2CADPCB_Settings;

            return this.result;
        }

        public void DisposeLogger()
        {
            if (Logger != null)
                Logger.Dispose();
            Logger = null;
        }
        
        /// <summary>
        /// This function does the job.
        /// </summary>
        private void WorkInMainTransaction()
        {
            var config = (this.mainParameters.config as CyPhy2CADPCB_Settings);
            this.result.Success = true;

            // Call Elaborator
            var elaboratorSuccess = this.CallElaborator(this.mainParameters.Project,
                                                        this.mainParameters.CurrentFCO,
                                                        this.mainParameters.SelectedFCOs,
                                                        this.mainParameters.StartModeParam);
            this.UpdateSuccess("Elaborator", elaboratorSuccess);

            bool successGeneration = true;
            try
            {
                var testbench = ISIS.GME.Dsml.CyPhyML.Classes.TestBench.Cast(this.mainParameters.CurrentFCO);
                if (testbench == null)
                {
                    Logger.WriteError("Invalid context of invocation <{0}>, invoke the interpreter from a Testbench model",
                        this.mainParameters.CurrentFCO.Name);
                    return;
                }

                var ca = testbench.Children.ComponentAssemblyCollection.FirstOrDefault();
                if (ca == null)
                {
                    Logger.WriteFailed("No valid component assembly in testbench {0}", testbench.Name);
                    return;
                }

                config =    RetrieveWorkflowSettings(config, ca);
                if (config == null)
                {
                    this.result.Success = false;
                    return;
                }

                if (config.runLayout == "true")
                {
                    config.layoutFilePath = Path.Combine(this.mainParameters.OutputDirectory, config.layoutFile);
                }
                else
                {
                    // For runLayout == True, JSON won't exist until after this interpreter executes,
                    //      but if something causes it to not be created, the schematic test bench will
                    //      return an error.
                    if (!File.Exists(config.layoutFilePath))
                    {
                        Logger.WriteError("Unable to find PCB layout file at: {0}", config.layoutFilePath);
                        this.result.Success = false;
                        return;
                    }
                }
                
                // At this point, no matter the GME settings, the layoutFilePath should be specified.
                if (String.IsNullOrWhiteSpace(config.layoutFilePath))
                {
                    Logger.WriteError("Something has gone wrong in setting the PCB layout file path.");
                    this.result.Success = false;
                    return;
                }

                // File exists, so now copy to results directory.
                string outputLayoutPath = Path.Combine(this.mainParameters.OutputDirectory, config.layoutFile);
                if (config.layoutFilePath != outputLayoutPath)  // Will be equal in case of runLayout == True.
                {
                    if (File.Exists(outputLayoutPath))
                    {
                        File.Delete(outputLayoutPath);
                    }
                    File.Copy(config.layoutFilePath, outputLayoutPath);
                    if (!File.Exists(outputLayoutPath))
                    {
                        Logger.WriteError("PCB layout file was not copied from {0} to {1}.",
                                          config.layoutFilePath, outputLayoutPath);
                        this.result.Success = false;
                        return;
                    }
                }
                
                ///// Interpreter Logic here //////////////
                CyPhyParser parser = new CyPhyParser(Logger);
                var design = parser.ParseCyPhyDesign(ca);
                design.tb_parameters = new List<CyPhy.Parameter>(); 
                foreach (var tbparam in testbench.Children.ParameterCollection)
                {
                    design.tb_parameters.Add(tbparam);
                }
                CyPhy2CADPCB.CodeGenerator cg = new CyPhy2CADPCB.CodeGenerator();
                
                string ctString = cg.ProduceCTString(design, config.visualizerType.ToLower());
                if (String.IsNullOrWhiteSpace(ctString))
                {
                    Logger.WriteError("No EDAModels were found in component assembly. At least one component containing an EDAModel is required.");
                    this.result.Success = false;
                    return;
                }
                GenerateJson(ctString, "CT.json");

                // Ara template JSON file.
                string replaceFlag = "";
                string araTemplateString = "";
                int araTemplates = design.AllComponents.OfType<AbstractClasses.AraTemplateComponent>().Count();
                if (araTemplates > 1)
                {
                    Logger.WriteError("More than one Ara template module present in assembly.");
                    this.result.Success = false;
                    return;
                }
                else if (araTemplates == 1)
                {
                    replaceFlag = " -r";
                    araTemplateString = cg.ProduceAraTemplateJson(design);
                    if (String.IsNullOrWhiteSpace(araTemplateString))
                    {
                        Logger.WriteError("Something went wrong trying to generate the Ara Template String.");
                        this.result.Success = false;
                        return;
                    }
                    GenerateJson(araTemplateString, "AraTemplateParts.json");
                }
                


                // Check if interference analysis should be executed post-assembly.
                string interference_flag = "";
                int interference = testbench.Children.ParameterCollection.Where(p => p.Name == "INTERFERENCE_CHECK").Count();
                if (interference == 1)
                    interference_flag = " -i";

                // -a -> Run Assembler
                // -r -> Swap out stock components for specified parts.
                // -c -> Run Converter (STEP files only)
                // -v -> Run Visualizer
                string convertflag = "";
                if (config.visualizerType.ToLower() == "step" || config.visualizerType.ToLower() == "stp")
                {
                    convertflag = " -c";
                    // Create launch.js batch file
                    string launch_bat = Path.Combine(this.mainParameters.OutputDirectory, "launch_cadjs.bat");
                    File.WriteAllText(launch_bat, CyPhy2CADPCB.Properties.Resources.launch_cadjs);
                }
                else
                {
                    if (config.launchVisualizer == "true")
                    {
                        Logger.WriteWarning("launchVisualizer set to True, but a non-supported visualizer format was chosen (" +
                                            config.visualizerType + "). A non-STEP formatted assembly file will be created, but " +
                                            "it must viewed launched in an independent program.");
                    }
                }
                if (config.launchVisualizer == "true")
                {
                    this.result.RunCommand = string.Format("python.exe -E -m CADVisualizer " + 
                                                            ca.Name + " " + config.visualizerType.ToLower() +  
                                                            " -a" + replaceFlag + interference_flag + convertflag + " -v");
                }
                else
                {
                    this.result.RunCommand = string.Format("python.exe -E -m CADVisualizer " + 
                                                            ca.Name + " " + config.visualizerType.ToLower() + 
                                                            " -a" + replaceFlag + interference_flag + convertflag);
                }

                ///// End Interpreter Logic //////////////////////
                
                Logger.WriteDebug("Produced Design model: {0}", design.Name);

                successGeneration = true;
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.ToString());
                successGeneration = false;
            }
            this.UpdateSuccess("Code Generation", successGeneration);
        }

        public void GenerateJson( string ct, string filename )
        {
            string componentFile = Path.Combine(this.mainParameters.OutputDirectory, filename);
            StreamWriter writer = new StreamWriter(componentFile);
            writer.Write(ct);
            writer.Close();
        }

        public CyPhy2CADPCB_Settings RetrieveWorkflowSettings(CyPhy2CADPCB_Settings settings, CyPhy.ComponentAssembly ca)
        {
            var validvisualizers = new List<String> {"step", "stp", "stl", "mix"};
            if (String.IsNullOrWhiteSpace(settings.visualizerType))
            {
                Logger.WriteWarning("VisualizerType workflow parameter is not set. Defaulting to STEP models.");
                settings.visualizerType = "STEP";
            }
            if (!validvisualizers.Any(s => settings.visualizerType.ToLower().Contains(s)))
            {
                Logger.WriteError("CyPhy2CADPCB Visualizer Format Type not set to valid format. " +
                                  "The visualizer currently only supports STEP & STL files.");
                this.result.Success = false;
                return null;
            }
            if (settings.runLayout == "false" || String.IsNullOrWhiteSpace(settings.runLayout))
            {
                if (!String.IsNullOrWhiteSpace(settings.layoutFilePath) && settings.useSavedLayout == "true")
                {
                    Logger.WriteError("Conflicting interpreter settings: useSavedLayout is set to True, but " +
                                      "layoutFilePath is also specified.");
                    this.result.Success = false;
                    return null;
                }
                else if (String.IsNullOrWhiteSpace(settings.layoutFilePath) && settings.useSavedLayout == "true")
                {
                    string caDesignPath = ca.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE,
                                                              this.mainParameters.ProjectDirectory);
                    string layoutPath = Path.Combine(this.mainParameters.ProjectDirectory, 
                                                     ca.Attributes.Path, settings.layoutFile);
                    if (!File.Exists(layoutPath))
                    {
                        Logger.WriteError("Interpreter setting useSavedLayout is set to True, but no {0} " +
                                          "file was found in component assembly directory {1}.",
                                          settings.layoutFile, ca.Attributes.Path);
                        this.result.Success = false;
                        return null;
                    }
                    settings.layoutFilePath = layoutPath;
                }
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(settings.layoutFilePath) || settings.useSavedLayout == "true")
                {
                    Logger.WriteError("Conflicting interpreter settings: runLayout is set to True, but " +
                                      "either useSavedLayout == True or layoutFilePath is specified.");
                    this.result.Success = false;
                    return null;
                }
            }
            return settings;
        }

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
                var elaborator = new CyPhyElaborateCS.CyPhyElaborateCSInterpreter();
                elaborator.Initialize(project);
                int verbosity = 128;
                result = elaborator.RunInTransaction(project, currentobj, selectedobjs, verbosity);
                this.Logger.WriteDebug("Elaboration is done.");
            }
            catch (Exception ex)
            {
                this.Logger.WriteError("Exception occurred in Elaborator : {0}", ex.ToString());
                result = false;
            }

            return result;
        }

        /*
        private void GenerateRunBatFile(String OutDir)
        {
            this.Logger.WriteInfo("GenerateRunBatFile()  Generating: [{0}]...", Path.Combine(OutDir, "runAddComponentToPcbConstraints.bat"));
            StreamWriter file = new StreamWriter(Path.Combine(OutDir, "runAddComponentToPcbConstraints.bat"));
            //file.WriteLine("python Synthesize_PCB_CAD_connections.py {0}", layoutFilePath);  // replaced by fancy META python path code below

            var pcb_bat = CyPhy2CADPCB.Properties.Resources.runCreateCADAssembly;
            var pcb_bat_toOutput = String.Format(pcb_bat, 
                                                 (this.mainParameters.config as CyPhy2CADPCB_Settings).GetLayoutPath);

            file.Write(pcb_bat_toOutput);
            file.Close();
        }
        */
        /*
        public void GenerateScriptFiles(String OutDir)
        {
            this.Logger.WriteInfo("GenerateScriptFiles()  Generating: [{0}]...", Path.Combine(OutDir, "Synthesize_PCB_CAD_connections.py"));

            var pcb_python = CyPhy2CADPCB.Properties.Resources.Synthesize_PCB_CAD_connections;
            using (StreamWriter writer = new StreamWriter(Path.Combine(OutDir, "Synthesize_PCB_CAD_connections.py")))
            {
                writer.Write(Encoding.UTF8.GetString(pcb_python));
            }
        }
        */

        /// <summary>
        /// ProgId of the configuration class of this interpreter.
        /// </summary>
        public string InterpreterConfigurationProgId
        {
            get
            {
                return (typeof(CyPhy2CADPCB_Settings).GetCustomAttributes(typeof(ProgIdAttribute), false)[0] as ProgIdAttribute).Value;
            }
        }

        #endregion

        #region IMgaComponentEx Members

        MgaGateway MgaGateway { get; set; }
        GMELogger Logger { get; set; }

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
                                                                           typeof(CyPhy2CADPCB_Settings),
                                                                           this.ComponentProgID);
                CyPhy2CADPCB_Settings settings = (settings_ != null) ? settings_ as CyPhy2CADPCB_Settings : new CyPhy2CADPCB_Settings();
                
                // Set configuration based on Workflow Parameters. This will override all [WorkflowConfigItem] members.
                settings = InitializeSettingsFromWorkflow(settings);

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
                if (MgaGateway != null &&
                    MgaGateway.territory != null)
                {
                    MgaGateway.territory.Destroy();
                }
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

        [Serializable]
        [ComVisible(true)]
        [ProgId("ISIS.META.CADPCBConfig")]
        [Guid("98347693-FC33-4F1E-A2C0-8E97C41B23D4")]
        public class CADPCBConfig : IInterpreterConfiguration, IInterpreterPreConfiguration
        {
            public string ProjectDirectory { get; set; }
            public string AuxiliaryDirectory { get; set; }
            public bool UseProjectManifest { get; set; }
            public List<string> StepFormats { get; set; }
        }

    }
}
