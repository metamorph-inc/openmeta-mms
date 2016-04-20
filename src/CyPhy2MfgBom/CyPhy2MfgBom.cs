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
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace CyPhy2MfgBom
{
    /// <summary>
    /// This class implements the necessary COM interfaces for a GME interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CyPhy2MfgBomInterpreter : IMgaComponentEx, IGMEVersionInfo, ICyPhyInterpreter
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
            project.CreateTerritoryWithoutSink(out MgaGateway.territory);
        }

        #region CyPhyGUIs
        /// <summary>
        /// ProgId of the configuration class of this interpreter.
        /// </summary>
        public string InterpreterConfigurationProgId
        {
            get
            {
                return (typeof(CyPhyGUIs.NullInterpreterConfiguration).GetCustomAttributes(typeof(ProgIdAttribute), false)[0] as ProgIdAttribute).Value;
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
            return new NullInterpreterConfiguration();
        }

        /// <summary>
        /// No GUI and interactive elements are allowed within this function.
        /// </summary>
        /// <param name="parameters">Main parameters for this run and GUI configuration.</param>
        /// <returns>Result of the run, which contains a success flag.</returns>
        public IInterpreterResult Main(IInterpreterMainParameters parameters)
        {
            this.mainParameters = parameters;

            //this.runtime = new Queue<Tuple<string, TimeSpan>>();
            //this.runtime.Clear();
            this.result = new InterpreterResult()
            {
                Success = true
            };

            try
            {
                BOMVisitor visitor = new BOMVisitor()
                {
                    Logger = Logger,
                    Traceability = this.result.Traceability
                };

                String nameFCO = null;
                int designQuantity = 1;
                List<String> listSupplierAffinity = null;
                MgaGateway.PerformInTransaction(delegate
                {
                    // Call Elaborator
                    var elaboratorSuccess = this.CallElaborator(this.mainParameters.Project,
                                                                this.mainParameters.CurrentFCO,
                                                                this.mainParameters.SelectedFCOs,
                                                                this.mainParameters.StartModeParam);
                    this.UpdateSuccess("Elaborator", elaboratorSuccess);

                    // Parse design
                    var tb = CyPhyClasses.TestBench.Cast(parameters.CurrentFCO);
                    visitor.visit(tb);

                    nameFCO = this.mainParameters.CurrentFCO.Name;

                    var propDesignQuantity = tb.Children
                                               .PropertyCollection
                                               .FirstOrDefault(p => p.Name == "design_quantity");
                    if (propDesignQuantity != null)
                    {
                        int val;
                        if (int.TryParse(propDesignQuantity.Attributes.Value, out val))
                        {
                            designQuantity = val;
                        }
                        else
                        {
                            Logger.WriteWarning("The property <a href=\"mga:{0}\">{1}</a> has a non-integer value of \"{2}\". Setting to default of 1.",
                                                propDesignQuantity.ID,
                                                propDesignQuantity.Name,
                                                propDesignQuantity.Attributes.Value);
                        }
                    }
                    else
                    {
                        Logger.WriteWarning("No property named \"design_quantity\" found. Assuming quantity of 1.");
                    }

                    var metricPartCostPerDesign = tb.Children
                                                    .MetricCollection
                                                    .FirstOrDefault(m => m.Name == "part_cost_per_design");
                    if (metricPartCostPerDesign == null)
                    {
                        var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(this.mainParameters.OutputDirectory);
                        var metric = new AVM.DDP.MetaTBManifest.Metric()
                        {
                            Name = "part_cost_per_design",
                            DisplayedName = "Part Cost Per Design",
                            Unit = "USD",
                            GMEID = null
                        };
                        if (manifest.Metrics == null)
                        {
                            manifest.Metrics = new List<AVM.DDP.MetaTBManifest.Metric>();
                        }
                        manifest.Metrics.Add(metric);

                        manifest.Serialize(this.mainParameters.OutputDirectory);
                    }

                    var propSupplierAffinity = tb.Children
                                                 .PropertyCollection
                                                 .FirstOrDefault(p => p.Name == "supplier_affinity");
                    if (propSupplierAffinity != null &&
                        !String.IsNullOrWhiteSpace(propSupplierAffinity.Attributes.Value))
                    {
                        listSupplierAffinity = propSupplierAffinity.Attributes
                                                                   .Value
                                                                   .Split(',')
                                                                   .Select(s => s.Trim())
                                                                   .ToList();
                    }
                },
                transactiontype_enum.TRANSACTION_NON_NESTED,
                abort: true);

                // Serialize BOM to file
                var bom = visitor.bom;
                var filenameBom = nameFCO + "_bom.json";
                var pathBom = Path.Combine(this.mainParameters.OutputDirectory, 
                                           filenameBom);
                using (StreamWriter outfile = new StreamWriter(pathBom))
                {
                    outfile.Write(bom.Serialize());
                }

                // Create CostEstimationRequest
                var request = new MfgBom.CostEstimation.CostEstimationRequest()
                {
                    bom = bom,
                    design_quantity = designQuantity,
                    supplier_affinity = listSupplierAffinity
                };
                var filenameRequest = nameFCO + "_cost_estimate_request.json";
                var pathRequest = Path.Combine(this.mainParameters.OutputDirectory,
                                               filenameRequest);
                using (StreamWriter outfile = new StreamWriter(pathRequest))
                {
                    outfile.Write(request.Serialize());
                }

                var tbManifest = AVM.DDP.MetaTBManifest.OpenForUpdate(this.mainParameters.OutputDirectory);
                tbManifest.AddArtifact(filenameBom, "CyPhy2MfgBom::BOM");
                tbManifest.AddArtifact(filenameRequest, "CyPhy2MfgBom::CostEstimationRequest");
                tbManifest.Serialize(this.mainParameters.OutputDirectory);
                
                using (var batRunBomCostAnalysis = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, 
                                                                                 "runBomCostAnalysis.bat")))
                {
                    batRunBomCostAnalysis.Write(CyPhy2MfgBom.Properties.Resources.runBomCostAnalysis);
                }
                this.result.RunCommand = "runBomCostAnalysis.bat";
                

                if (this.result.Success)
                {
                    Logger.WriteInfo("Generated files are here: <a href=\"file:///{0}\" target=\"_blank\">{0}</a>", this.mainParameters.OutputDirectory);
                    Logger.WriteInfo("CyPhy2MfgBom has finished. [SUCCESS: {0}, Labels: {1}]", this.result.Success, this.result.Labels);
                }
                else
                {
                    Logger.WriteError("CyPhy2MfgBom failed! See error messages above.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("Exception: {0}<br> {1}", ex.Message, ex.StackTrace);
            }
            return this.result;
        }
        #endregion

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

                if (this.result.Traceability == null)
                {
                    this.result.Traceability = new META.MgaTraceability();
                }

                if (elaborator.Traceability != null)
                {
                    elaborator.Traceability.CopyTo(this.result.Traceability);
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

        #region IMgaComponentEx Members

        MgaGateway MgaGateway { get; set; }
        GMELogger Logger { get; set; }

        public void InvokeEx(MgaProject project, MgaFCO currentobj, MgaFCOs selectedobjs, int param)
        {
            if (!enabled)
            {
                return;
            }

            try
            {
                if (Logger == null)
                {
                    Logger = new GMELogger(project, ComponentName);
                }
                MgaGateway = new MgaGateway(project);
                project.CreateTerritoryWithoutSink(out MgaGateway.territory);

                this.mainParameters = new InterpreterMainParameters()
                {
                    Project = project,
                    CurrentFCO = currentobj as MgaFCO,
                    SelectedFCOs = selectedobjs,
                    StartModeParam = param
                };

                Main(this.mainParameters);

                //this.PrintRuntimeStatistics();
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
                project = null;
                currentobj = null;
                selectedobjs = null; 
                DisposeLogger();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public void DisposeLogger()
        {

            if (Logger != null)
            {
                Logger.Dispose();
                Logger = null;
            }
        }

        //private Queue<Tuple<string, TimeSpan>> runtime = new Queue<Tuple<string, TimeSpan>>();
        private DateTime startTime = DateTime.Now;
        /*private void PrintRuntimeStatistics()
        {
            Logger.WriteDebug("======================================================");
            Logger.WriteDebug("Start time: {0}", this.startTime);
            foreach (var time in this.runtime)
            {
                Logger.WriteDebug("{0} = {1}", time.Item1, time.Item2);
            }
            Logger.WriteDebug("======================================================");
        }*/

        /// <summary>
        /// Parameter of this run.
        /// </summary>
        private IInterpreterMainParameters mainParameters { get; set; }

        /// <summary>
        /// Result of the latest run of this interpreter.
        /// </summary>
        private InterpreterResult result = new InterpreterResult();
        private void UpdateSuccess(string message, bool success)
        {
            this.result.Success = this.result.Success && success;

            //this.runtime.Enqueue(new Tuple<string, TimeSpan>(message, DateTime.Now - this.startTime));
            if (success)
            {
                Logger.WriteInfo("{0} : OK", message);
            }
            else
            {
                Logger.WriteError("{0} : FAILED", message);
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
