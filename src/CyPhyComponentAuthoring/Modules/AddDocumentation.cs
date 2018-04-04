using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using META;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using System.Windows.Forms;

namespace CyPhyComponentAuthoring.Modules
{
    [CyPhyComponentAuthoringInterpreter.IsCATModule(ContainsCATmethod = true)]
    public class AddDocumentation : CATModule
    {
        public CyPhyGUIs.GMELogger Logger { get; set; }
        private bool Close_Dlg;

        [CyPhyComponentAuthoringInterpreter.CATName(
            NameVal = "Add Documentation",
            DescriptionVal = "An document is imported from a file into a resource object in this Component.",
            RoleVal = CyPhyComponentAuthoringInterpreter.Role.Construct
            )
        ]
        public void callAddDocument(object sender, EventArgs e)
        {
            AddDocument();

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

        [CyPhyComponentAuthoringInterpreter.CATDnD(Extension = ".md")]
        [CyPhyComponentAuthoringInterpreter.CATDnD(Extension = ".mdown")]
        [CyPhyComponentAuthoringInterpreter.CATDnD(Extension = ".pdf")]
        public void AddDocument(string DocFileSourcePath = null)
        {
            this.Logger = new CyPhyGUIs.GMELogger(CurrentProj, this.GetType().Name);
            
            #region Selection dialog
            //  - Display a dialog box to let the user choose the Custom Icon file
            bool doc_file_chosen = false;
            bool test_mode = false;
            if (string.IsNullOrWhiteSpace(DocFileSourcePath))
            {
                doc_file_chosen = run_selection_dialog(out DocFileSourcePath);
            }
            else
            {
                test_mode = true;
                if (File.Exists(DocFileSourcePath))
                {
                    doc_file_chosen = true;
                }
                else
                {
                    this.Logger.WriteError("Invalid file path passed in: " + DocFileSourcePath);
                }
            }

            if (!doc_file_chosen)
            {
                this.Logger.WriteError("No document file chosen. Exiting.");
                clean_up(false);
                return;
            }
            #endregion
            
            #region Copy files to backend folder
            string path_DstDocFile = "";
            String name_OrgDocFile = Path.GetFileName(DocFileSourcePath);

            try
            {
                // Find the path of the current component
                String path_Comp = GetCurrentComp().GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE);
                String path_CompDocDir = Path.Combine(path_Comp, "doc");
                if (Directory.Exists(path_CompDocDir) == false)
                {
                    Directory.CreateDirectory(path_CompDocDir);
                }

                path_DstDocFile = System.IO.Path.Combine(path_CompDocDir, name_OrgDocFile);
                
                int count = 1;
                while (File.Exists(path_DstDocFile))
                {
                    String DstFileName = String.Format("{0}_({1}){2}", 
                                                       Path.GetFileNameWithoutExtension(name_OrgDocFile), 
                                                       count++, 
                                                       Path.GetExtension(name_OrgDocFile));

                    path_DstDocFile = System.IO.Path.Combine(path_CompDocDir, DstFileName);
                }

                System.IO.File.Copy(DocFileSourcePath, path_DstDocFile, false);
            }
            catch (Exception err_copy_file)
            {
                this.Logger.WriteError("Error copying file" + err_copy_file.Message);
                clean_up(false);
                return;
            }
            #endregion

            //- A Resource object should be created in the CyPhy Component which points to the file. 
            #region Create Resource
            CyPhy.Resource ResourceObj = CyPhyClasses.Resource.Create(GetCurrentComp());
            ResourceObj.Attributes.ID = Guid.NewGuid().ToString("B");
            ResourceObj.Attributes.Path = "doc\\" + Path.GetFileName(path_DstDocFile);
            ResourceObj.Name = Path.GetFileName(path_DstDocFile);
            #endregion

            clean_up(true);
        }
        
        // clean up loose ends on leaving this module
        void clean_up(bool close_dlg)
        {
            Close_Dlg = close_dlg;
            this.Logger.Dispose();
        }

        // Display a file dialog box to select the file to import
        // parameter will contain the filename, returns true or false based on if a file is returned or not
        bool run_selection_dialog(out string FileName)
        {
            // Open file dialog box
            DialogResult dr;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.Multiselect = false;
                ofd.Filter = "All files (*.*)|*.*";
                dr = ofd.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    FileName = ofd.FileName;
                    return true;
                }
            }
            FileName = "";
            this.Logger.WriteError("No file was selected.");
            return false;
        }
    }
}
