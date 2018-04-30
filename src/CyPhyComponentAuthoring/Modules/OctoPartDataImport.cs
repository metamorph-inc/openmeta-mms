using GME.MGA;
using META;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using OctoPart = MfgBom.OctoPart;


namespace CyPhyComponentAuthoring.Modules
{
    [CyPhyComponentAuthoringInterpreter.IsCATModule(ContainsCATmethod = true)]
    public class OctoPartDataImport : CATModule
    {
        #region layout constants

        // constants for adding new parameters to the GME display at the right coordinates
        private const int PARAMETER_START_X = 1000;
        private const int PARAMETER_START_Y = 40;
        private const int PARAMETER_ADJUST_Y = 40;

        private const int RESOURCE_START_X = 900;
        private const int RESOURCE_START_Y = 40;
        private const int RESOURCE_ADJUST_Y = 100;

        /// <summary>
        /// A variable for layout control. Reset it to zero each time.
        /// </summary>
        private bool Close_Dlg;
        private int greatest_current_x;
        private int resources_created = 0;  // Only move resources that are created by this module.

        #endregion

        private CyPhyGUIs.GMELogger Logger { get; set; }

        [CyPhyComponentAuthoringInterpreter.CATName(
                NameVal = "Add OctoPart Information",
                DescriptionVal = "An existing EDAModel's device name is queried with OctoPart and important " +
                                 "component properties are populated, as well as an image and datasheet.",
                RoleVal = CyPhyComponentAuthoringInterpreter.Role.Construct,
                IconResourceKey = "add_octopart",
                SupportedDesignEntityTypes = CyPhyComponentAuthoringInterpreter.SupportedDesignEntityType.Component
           )
        ]
        public void OctoPartDataImport_Delegate(object sender, EventArgs e)
        {
            bool allComponents = Prompt.ShowDialog("Run OctoPart Importer on all components?\n(No for only current component)\n");
            if (allComponents)
            {
                var folders = this.CurrentProj.RootFolder.ChildObjects;
                for (var i = 1; i < folders.Count; i++)
                {
                    if ( String.Compare(folders[i].Name, this.CurrentObj.ParentFolder.Name) == 0 )
                    {
                        var components = CyPhyClasses.Components.Cast(folders[i]);
                        foreach (var comp in components.Children.ComponentCollection)
                        {
                            this.Logger = new CyPhyGUIs.GMELogger(CurrentProj, this.GetType().Name);
                            GetOctoPartData(comp);
                        }
                    }
                }
            }
            else
            {
                GetOctoPartData((CyPhy.Component) this.GetCurrentDesignElement());
            }

            // Close the calling dialog box if the module ran successfully
            if (Close_Dlg)
            {
                if (sender is Form)
                {
                    // the TLP is in the dialog box
                    Form parentDB = (Form)sender;
                    parentDB.Close();
                }
            }
        }

        private void LogMessage(String message, CyPhyGUIs.GMELogger.MessageType_enum type)
        {
            if (this.Logger == null)
                this.Logger = new CyPhyGUIs.GMELogger(CurrentProj, this.GetType().Name);

            switch (type)
            {
                case CyPhyGUIs.SmartLogger.MessageType_enum.Error:
                    this.Logger.WriteError(message);
                    break;
                case CyPhyGUIs.SmartLogger.MessageType_enum.Warning:
                    this.Logger.WriteWarning(message);
                    break;
                default:
                    this.Logger.WriteInfo(message);
                    break;
            }
        }

        // MOT-347 Method to dispose of the Logger when done with it.
        private void disposeLogger()
        {
            if (null != this.Logger)
            {
                this.Logger.Dispose();
                this.Logger = null;
            }
        }

        public bool GetOctoPartData(CyPhy.Component component)
        {
            try
            {
                var part = new MfgBom.Bom.Part();
                part.octopart_mpn = GetDeviceNameFromComponentEDAModel(component);
                if (string.IsNullOrWhiteSpace(part.octopart_mpn))
                {
                    return false;
                }

                var desiredData = new List<string>(){ "category_uids",
                                                      "imagesets",
                                                      "datasheets",
                                                      "specs"
                                                    };

                part.QueryOctopartData(exact_only: true, includes: desiredData, grab_first: false);

                if (!part.valid_mpn)
                {
                    LogMessage(String.Format("OctoPart MPN number {0} was not found in database", part.octopart_mpn),
                               CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                    return false;
                }
                if (part.too_broad_mpn)
                {
                    LogMessage(String.Format("OctoPart MPN number {0} returned more than one possible match.", part.octopart_mpn),
                               CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                    return false;
                }

                // Add property for OctoPart MPN
                BuildCyPhyProperty(component, "octopart_mpn", part.octopart_mpn,
                                   CyPhyClasses.Property.AttributesClass.DataType_enum.String, false);

                // Add property for all details returned by specs
                if (String.IsNullOrWhiteSpace(part.TechnicalSpecifications))
                {
                    LogMessage(String.Format("No technical details provided for OctoPart MPN number {0}.", part.octopart_mpn),
                               CyPhyGUIs.SmartLogger.MessageType_enum.Warning);
                }
                else
                {
                    AddOctoPartTechSpecsToComponent(component, part);
                }

                // Add classification from category UIDs
                if (String.IsNullOrWhiteSpace(part.CategoryUID))
                {
                    LogMessage(String.Format("No Category UID/Classification for OctoPart MPN number {0}.", part.octopart_mpn),
                               CyPhyGUIs.SmartLogger.MessageType_enum.Warning);
                }
                else
                {
                    AddOctoPartClassificationToComponent(component, part);
                }

                // Add icon image (need to download)
                if (String.IsNullOrWhiteSpace(part.Icon))
                {
                    LogMessage(String.Format("No Icon was provided for OctoPart MPN number {0}.", part.octopart_mpn),
                               CyPhyGUIs.SmartLogger.MessageType_enum.Warning);
                }
                else
                {
                    AddOctoPartIconToComponent(component, part);
                }

                // Add datasheet documentation.
                if (String.IsNullOrWhiteSpace(part.Datasheet))
                {
                    LogMessage(String.Format("No datasheet was provided for OctoPart MPN number {0}.", part.octopart_mpn),
                               CyPhyGUIs.SmartLogger.MessageType_enum.Warning);
                }
                else
                {
                    AddOctoPartDatasheetToComponent(component, part);
                }

                LogMessage("OctoPart importer completed.", CyPhyGUIs.SmartLogger.MessageType_enum.Success);
                return true;
            }
            catch (OctoPart.OctopartQueryException ex)
            {
                LogMessage("Error: " + ex.Message, CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                return false;
            }
            finally
            {
                clean_up(true);
            }

        }

        // clean up loose ends on leaving this module
        void clean_up(bool close_dlg)
        {
            Close_Dlg = close_dlg;
            this.Logger.Dispose();
        }


        /// <summary>
        /// Gets name of the device for an Eagle model in a CyPhy component. Ensures only 1 EDAModel exists.
        /// This name will be the OctoPart MPN that is queried later.
        /// </summary>
        public string GetDeviceNameFromComponentEDAModel(CyPhy.Component component)
        {
            string edaModelDevice = "";
            IEnumerable<CyPhy.EDAModel> edaModels = component.Children.EDAModelCollection;
            int edaModelCount = edaModels.Count();

            if (edaModelCount == 0)
            {
                edaModelDevice = QueryCyPhyOctoPartMpnProperty(component);
                if (String.IsNullOrWhiteSpace(edaModelDevice))
                {
                    LogMessage("No EDAModel present in component.", CyPhyGUIs.SmartLogger.MessageType_enum.Warning);
                    disposeLogger();
                }
            }
            else if (edaModelCount > 1)
            {
                LogMessage("More than one EDAModel is present in component.", CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                disposeLogger();
            }
            else
            {
                edaModelDevice = edaModels.FirstOrDefault().Attributes.DeviceSet;
                if (string.IsNullOrWhiteSpace(edaModelDevice))
                {
                    LogMessage("DeviceSet in EDAModel is not set.", CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                    disposeLogger();
                }
            }

            return edaModelDevice;
        }


        /// <summary>
        /// Queries OctoPart for the category heirarchy with the category UID from OctoPart's website.
        /// The parent and all grandparent categories are stored and strung together to for the classification
        /// </summary>
        public void AddOctoPartClassificationToComponent(CyPhy.Component comp, MfgBom.Bom.Part part)
        {
            part.QueryCategory(part.CategoryUID);

            if (String.IsNullOrWhiteSpace(comp.Attributes.Classifications))
            {
                comp.Attributes.Classifications = part.Classification;
            }
            else
            {
                LogMessage(String.Format("A classification is already specified for component {0}. ", comp.Name) +
                           String.Format("Classification returned by OctoPart is {0}; will not overwrite.", part.Classification),
                           CyPhyGUIs.SmartLogger.MessageType_enum.Warning);
            }
        }


        /// <summary>
        /// Creates a CyPhy property for each technical detail kept on file for an OctoPart component
        /// </summary>
        public void AddOctoPartTechSpecsToComponent(CyPhy.Component comp, MfgBom.Bom.Part part)
        {
            dynamic dynJson = Newtonsoft.Json.JsonConvert.DeserializeObject(part.TechnicalSpecifications);
            if (dynJson != null)
            {
                foreach (var spec in dynJson)
                {
                    CyPhyClasses.Property.AttributesClass.DataType_enum type =
                        DeterminePropertyDataType((string)spec.Value.metadata.datatype, (string)spec.Value.metadata.key);

                    if (spec.Value.value.Count > 0)
                    {
                        BuildCyPhyProperty(comp, (string)spec.Value.metadata.key, (string)spec.Value.value[0], type, false);
                    }
                    // TODO: Add symbols if applicable ?
                    // spec.metadata.unit  // <-- returns null or {name, symbol}
                }

                #region layout

                // find the largest current X value so our new elements are added to the right of existing design elements
                // choose the greater value of PARAMETER_START_X and greatest_current_x, to handle case where models
                // have not yet been added (give user more space before the property list).
                greatest_current_x = 0;
                foreach (var child in GetCurrentDesignElement().AllChildren)
                {
                    foreach (MgaPart item in (child.Impl as MgaFCO).Parts)
                    {
                        int read_x, read_y;
                        string read_str;
                        item.GetGmeAttrs(out read_str, out read_x, out read_y);
                        greatest_current_x = (read_x > greatest_current_x) ? read_x : greatest_current_x;
                    }
                }

                int PARAMETER_START = (PARAMETER_START_X > greatest_current_x) ? PARAMETER_START_X : greatest_current_x;

                int num_parsed_properties = 0;
                foreach (var property in comp.Children.PropertyCollection)
                {
                    foreach (MgaPart item in (property.Impl as MgaFCO).Parts)
                    {
                        item.SetGmeAttrs(null, PARAMETER_START, PARAMETER_START_Y + (num_parsed_properties * PARAMETER_ADJUST_Y));
                    }
                    num_parsed_properties++;
                }

                #endregion

            }
        }


        /// <summary>
        /// Get OctoPart MPN CyPhy property value
        /// </summary>
        public string QueryCyPhyOctoPartMpnProperty(CyPhy.Component comp)
        {
            IEnumerable<CyPhy.Property> propertyCollection = comp.Children.PropertyCollection;
            CyPhy.Property property = propertyCollection.FirstOrDefault(p => p.Name.ToLower() == "octopart_mpn");
            return property == null ? "" : property.Attributes.Value;
        }


        public void BuildCyPhyProperty(CyPhy.Component comp,
                                       string name,
                                       string value,
                                       CyPhyClasses.Property.AttributesClass.DataType_enum type,
                                       bool is_prominent)
        {
            if (CheckPropertyDoesNotExist(comp, name))
            {
                CyPhy.Property property = ISIS.GME.Dsml.CyPhyML.Classes.Property.Create(comp);
                property.Name = name;
                property.Attributes.Value = value;
                property.Attributes.DataType = type;
                property.Attributes.IsProminent = is_prominent;
            }
        }


        /// <summary>
        /// Checks that a property of this name does not already exist to prevent overwriting user data.
        /// </summary>
        public bool CheckPropertyDoesNotExist(CyPhy.Component comp, string propertyName)
        {
            IEnumerable<CyPhy.Property> propertyCollection = comp.Children.PropertyCollection;
            return !propertyCollection.Any(p => p.Name == propertyName);
        }


        /// <summary>
        /// Check data type of a technical spec returned by OctoPart and convert to CyPhy DatType_enum
        /// </summary>
        public CyPhyClasses.Property.AttributesClass.DataType_enum DeterminePropertyDataType(string type, string name)
        {
            if (String.Compare(type, "integer") == 0)
            {
                return CyPhyClasses.Property.AttributesClass.DataType_enum.Integer;
            }
            else if (String.Compare(type, "boolean") == 0)
            {
                return CyPhyClasses.Property.AttributesClass.DataType_enum.Boolean;
            }
            else if (String.Compare(type, "string") == 0)
            {
                return CyPhyClasses.Property.AttributesClass.DataType_enum.String;
            }
            else // (String.Compare(type, "decimal") == 0)
            {
                return CyPhyClasses.Property.AttributesClass.DataType_enum.Float;
            }
        }


        /// <summary>
        /// OctoPart query returns link to image of component. Download this image to the Windows temporary
        /// directory and invoke the Icon CAT module to store as part of the CyPhy component.
        /// </summary>
        public void AddOctoPartIconToComponent(CyPhy.Component comp, MfgBom.Bom.Part part)
        {
            if (!comp.Children.ResourceCollection.Any(r => r.Name == "Icon.png"))
            {
                //Download icon to temp directory
                WebClient webClient = new WebClient();
                webClient.Headers.Add("user-agent", "meta-tools/" + VersionInfo.CyPhyML);
                String iconPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", "") + ".png");
                webClient.DownloadFile(part.Icon, iconPath);

                if (File.Exists(iconPath))
                {
                    // Invoke icon module
                    CustomIconAdd iconModule = new CustomIconAdd();
                    iconModule.SetCurrentDesignElement(comp);
                    iconModule.CurrentObj = CurrentObj;
                    iconModule.AddCustomIcon(iconPath);

                    // Relocate resource in component
                    CyPhy.Resource resource = comp.Children.ResourceCollection.FirstOrDefault(r => r.Name == "Icon.png");
                    if (resource != null)
                    {
                        foreach (MgaPart item in (resource.Impl as MgaFCO).Parts)
                        {
                            item.SetGmeAttrs(null, RESOURCE_START_X, RESOURCE_START_Y + (resources_created * RESOURCE_ADJUST_Y));
                        }
                        resources_created++;

                        String iconPath_RelativeToProjRoot = Path.Combine(comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.REL_TO_PROJ_ROOT),
                                                                          "icon.png");
                        comp.Preferences.Icon = iconPath_RelativeToProjRoot;
                    }
                    else
                    {
                        LogMessage(String.Format("Error creating icon resource for component {0}.", comp.Name),
                                   CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                    }

                    // Delete icon from temp directory for cleanup
                    File.Delete(iconPath);
                }
                else
                {
                    LogMessage(String.Format("Error downloading icon from OctoPart for MPN {0}", part.octopart_mpn),
                               CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                }
            }
            else
            {
                LogMessage(String.Format("An icon is already specified for component {0}. Will not overwrite with OctoPart data.", comp.Name),
                           CyPhyGUIs.SmartLogger.MessageType_enum.Warning);
            }
        }


        /// <summary>
        /// OctoPart query returns link to datasheet of component. Download this file to the Windows temporary
        /// directory and invoke the Documentation CAT module to store as part of the CyPhy component.
        /// </summary>
        public void AddOctoPartDatasheetToComponent(CyPhy.Component comp, MfgBom.Bom.Part part)
        {
            if (!comp.Children.ResourceCollection.Any(r => r.Name == "Datasheet.pdf"))
            {
                //Download icon to temp directory
                WebClient webClient = new WebClient();
                webClient.Headers.Add("user-agent", "meta-tools/" + VersionInfo.CyPhyML);
                String datasheetPath = Path.Combine(Path.GetTempPath(), "Datasheet.pdf");
                webClient.DownloadFile(part.Datasheet, datasheetPath);

                if (File.Exists(datasheetPath))
                {
                    // Invoke icon module
                    AddDocumentation docModule = new AddDocumentation();
                    docModule.SetCurrentDesignElement(comp);
                    docModule.CurrentObj = CurrentObj;
                    docModule.AddDocument(datasheetPath);

                    // Relocate resource in component
                    CyPhy.Resource resource = comp.Children.ResourceCollection.FirstOrDefault(r => r.Name.Contains("Datasheet"));

                    if (resource != null)
                    {
                        foreach (MgaPart item in (resource.Impl as MgaFCO).Parts)
                        {
                            item.SetGmeAttrs(null, RESOURCE_START_X, RESOURCE_START_Y + (resources_created * RESOURCE_ADJUST_Y));
                        }
                        resources_created++;
                    }
                    else
                    {
                        LogMessage(String.Format("Error creating datasheet resource for component {0}.", comp.Name),
                                   CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                    }

                    // Delete icon from temp directory for cleanup
                    File.Delete(datasheetPath);
                }
                else
                {
                    LogMessage(String.Format("Error downloading icon from OctoPart for MPN {0}", part.octopart_mpn),
                               CyPhyGUIs.SmartLogger.MessageType_enum.Error);
                }
            }
            else
            {
                LogMessage(String.Format("A datasheet is already specified for component {0}. ", comp.Name) +
                           String.Format("Datasheet returned by OctoPart query is at: {0}. ", part.Datasheet) +
                           "Will not overwrite with OctoPart data.",
                           CyPhyGUIs.SmartLogger.MessageType_enum.Warning);
            }
        }
    }

    public static class Prompt
    {
        public static bool ShowDialog(string text)
        {
            bool result = false;
            Form prompt = new Form();
            prompt.Width = 300;
            prompt.Height = 175;
            Label textLabel = new Label() { Left = 25, Top = 20, Width = 300, Height = 55, Text = text };
            Button confirmation = new Button() { Text = "No", Left = 150, Width = 100, Top = 100 };
            confirmation.Click += (sender, e) => { prompt.Close(); result = false; };
            Button rejection = new Button() { Text = "Yes", Left = 25, Width = 100, Top = 100 };
            rejection.Click += (sender, e) => { prompt.Close(); result = true; };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(rejection);
            prompt.Controls.Add(textLabel);
            prompt.ShowDialog();
            return result;
        }
    }
}
