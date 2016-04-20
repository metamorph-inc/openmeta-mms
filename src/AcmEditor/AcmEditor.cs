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
using CyPhyML = ISIS.GME.Dsml.CyPhyML.Interfaces;
using System.Windows.Forms;
using System.Diagnostics;

namespace AcmEditor
{
    /// <summary>
    /// This class implements the necessary COM interfaces for a GME interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class AcmEditorInterpreter : IMgaComponentEx, IGMEVersionInfo, IMgaEventSink
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
        public MgaFCO Main(MgaProject project, MgaFCO currentobj, MgaFCOs selectedobjs, ComponentStartMode startMode)
        {
            IMgaFolder rootFolder = project.RootFolder;

            acmFilename = project.ProjectConnStr.Substring("MGA=".Length, project.ProjectConnStr.Length - ".mga".Length - "MGA=".Length);
            var component = new CyPhyComponentImporter.CyPhyComponentImporterInterpreter().CreateComponentForAcm(project, acmFilename);
            component.StrAttrByName["Path"] = "components/../";


            return (MgaFCO)component;
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
                this.project = project;
                GMEConsole = GMEConsole.CreateFromProject(project);
                MgaGateway = new MgaGateway(project);
                project.CreateTerritoryWithoutSink(out MgaGateway.territory);

                project.CreateAddOn(this, out addon);
                // addOn->put_EventMask(OBJEVENT_ATTR | OBJEVENT_CONNECTED));

                MgaGateway.PerformInTransaction(delegate
                {
                    component = Main(project, currentobj, selectedobjs, Convert(param));
                });

                if (GMEConsole.gme != null && component != null)
                {
                    GMEConsole.gme.ShowFCO(component, false);
                }
            }
            finally
            {
                if (MgaGateway.territory != null)
                {
                    MgaGateway.territory.Destroy();
                }
                MgaGateway = null;
                project = null;
                currentobj = null;
                selectedobjs = null;
                // GMEConsole = null;
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


        string acmFilename;
        MgaFCO component = null;
        MgaProject project;
        bool loggedActiveMessage = false;
        MgaAddOn addon;
        public void GlobalEvent(globalevent_enum @event)
        {
            if (GMEConsole != null)
            {
                if (!loggedActiveMessage)
                {
                    GMEConsole.Info.WriteLine("Changes to this component will be saved to {0} when the file is saved", acmFilename);
                    loggedActiveMessage = true;
                }
            }
            if (@event == globalevent_enum.GLOBALEVENT_SAVE_PROJECT)
            {
                if (GMEConsole != null)
                {
                    GMEConsole.Info.WriteLine("Saving to {0}...", acmFilename);
                }
                CyPhyComponentExporter.CyPhyComponentExporterInterpreter.ExportToFile(ISIS.GME.Dsml.CyPhyML.Classes.Component.Cast(component), acmFilename);
                if (GMEConsole != null)
                {
                    GMEConsole.Info.WriteLine("Saved {0}", acmFilename);
                }
            }
            if (@event == globalevent_enum.GLOBALEVENT_CLOSE_PROJECT)
            {
                GMEConsole = null;
                addon = null;
            }
        }

        public void ObjectEvent(MgaObject obj, uint EventMask, object v)
        {
        }


        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                MessageBox.Show(String.Format("Usage: {0} {1}", Process.GetCurrentProcess().MainModule.FileName, "input.acm"));
                return 1;
            }
            try
            {
                string empty_mga = Path.Combine(META.VersionInfo.MetaPath, "meta", "EmptyCyPhyML.mga");
                string inputFilename = args[0];

                var gme = (IGMEOLEApp)Activator.CreateInstance(Type.GetTypeFromProgID("GME.Application"));
                
                File.Copy(empty_mga, inputFilename + ".mga", true);

                gme.Visible = true;
                gme.OpenProject("MGA=" + Path.GetFullPath(inputFilename + ".mga"));

                gme.MgaProject.BeginTransactionInNewTerr();
                try
                {
                    gme.MgaProject.RootFolder.Name = gme.MgaProject.Name = Path.GetFileNameWithoutExtension(inputFilename);
                }
                finally
                {
                    gme.MgaProject.CommitTransaction();
                }


                gme.RunComponent("MGA.Interpreter.AcmEditor");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return 1;
            }
            return 0;
        }
    }
}
