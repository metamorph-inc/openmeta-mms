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
            RoleVal = CyPhyComponentAuthoringInterpreter.Role.Construct
           )
        ]
        public void ImportSimulinkModel_Delegate(object sender, EventArgs e)
        {
            ImportSimulinkModel(this.GetCurrentComp());
        }

        public void ImportSimulinkModel(CyPhy.Component component)
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

            try
            {
                var browser = new SimulinkLibraryBrowser();

                browser.DebugTextBox.Text += "Hello World!\r\n";

                var matlabType = Type.GetTypeFromProgID("Matlab.Application");

                dynamic matlab = Activator.CreateInstance(matlabType);
                matlab.Execute("load_system('simulink');");

                object result;

                matlab.Feval("find_system", 1, out result, "simulink");

                browser.DebugTextBox.Text += result.GetType().ToString();
                browser.DebugTextBox.Text += "\r\n\r\n";
                browser.DebugTextBox.Text += result.ToString();
                browser.DebugTextBox.Text += "\r\n\r\n";

                var resultArray = (Object[]) result;
                foreach(var obj in resultArray)
                {
                    browser.DebugTextBox.Text += obj.GetType().ToString();
                    browser.DebugTextBox.Text += "\r\n";
                    browser.DebugTextBox.Text += obj.ToString();
                    browser.DebugTextBox.Text += "\r\n";

                    if (obj is Object[,])
                    {
                        var objArray = (Object[,]) obj;

                        foreach (var obj2 in objArray)
                        {
                            browser.DebugTextBox.Text += "Subobject: ";
                            browser.DebugTextBox.Text += obj2.GetType().ToString();
                            browser.DebugTextBox.Text += "\r\n";
                            browser.DebugTextBox.Text += obj2.ToString();
                            browser.DebugTextBox.Text += "\r\n";
                        }
                    }
                }

                browser.ShowDialog();
                Logger.WriteInfo("Complete");

                matlab.Quit();
            }
            catch( Exception e )
            {
                Logger.WriteError("Error parsing: {0}", e.Message);

                if (ownLogger)
                {
                    Logger.Dispose();
                    Logger = null;
                }

                return;
            }

            // Find the visual coordinates of where the new SystemC model should be placed.
            getNewModelInitialCoordinates(component, out m_startX, out m_startY);

            if (ownLogger)
            {
                Logger.Dispose();
                Logger = null;
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
        
    }
}

