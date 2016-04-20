using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GME.MGA;
using GME.MGA.Meta;

namespace ComponentCreator
{
    class ComponentCreator
    {
        MgaProject project = new MgaProject();

        [STAThread]
        static void Main(string[] args)
        {
            string startingDir = args.Length > 0 ? args[0] : ".";

            if (File.Exists(startingDir))
            {
                new ComponentCreator().Import(new string[] { startingDir });
            }
            else
            {
                new ComponentCreator().Import(Directory.EnumerateFiles(startingDir, "*.lbr", SearchOption.AllDirectories));
            }


        }

        public void Import(IEnumerable<string> lbrFiles)
        {

            project.Create("MGA=" + Path.GetFullPath(Path.Combine(".", "ComponentCreator", "Components")) + ".mga", "CyPhyML");

            IMgaComponent signalBlocksAddon = (IMgaComponent)Activator.CreateInstance(Type.GetTypeFromProgID("MGA.Addon.CyPhySignalBlocksAddOn"));
            signalBlocksAddon.Initialize(project);
            project.Notify(globalevent_enum.GLOBALEVENT_OPEN_PROJECT_FINISHED);
            System.Windows.Forms.Application.DoEvents(); // let CyPhySignalBlocksAddOn create libs

            project.BeginTransactionInNewTerr(transactiontype_enum.TRANSACTION_NON_NESTED);
            try
            {
                MgaMetaFolder componentsMeta = (MgaMetaFolder)project.RootMeta.RootFolder.DefinedFolderByName["Components", false];
                var components = project.RootFolder.CreateFolder(componentsMeta);
                components.Name = componentsMeta.Name;
            }
            finally
            {
                project.CommitTransaction();
            }

            foreach (string eagleFilePath in lbrFiles)
            {
                //Console.WriteLine(eagleFilePath);
                //Console.WriteLine(string.Join(" ", CyPhyComponentAuthoring.Modules.EDAModelImport.GetDevicesInEagleModel(eagleFilePath).ToArray()));
                foreach (string deviceName in CyPhyComponentAuthoring.Modules.EDAModelImport.GetDevicesInEagleModel(eagleFilePath))
                {
                    try
                    {
                        CreateNewComponentMga(eagleFilePath, deviceName);
                        Console.WriteLine("Imported {0} {1}", eagleFilePath, deviceName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Import failed: " + e.ToString());
                    }
                }
            }
            project.Save("", true);
            project.Close(true);

        }

        private void CreateNewComponentMga(string eagleFilePath, string deviceName)
        {
            string sanitizedDevicename = deviceName;
            while (sanitizedDevicename[0] == '\\')
            {
                sanitizedDevicename = sanitizedDevicename.Substring(1);
            }
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                sanitizedDevicename = sanitizedDevicename.Replace(invalid, '_');
            }
            // Directory.CreateDirectory(Path.Combine(startingDir, sanitizedDevicename));
            //project.EnableAutoAddOns(true); // FIXME just need CyPhySignalBlocksAddOn

            project.BeginTransactionInNewTerr(transactiontype_enum.TRANSACTION_NON_NESTED);
            try
            {
                project.RootFolder.Name = sanitizedDevicename;
                var components = (MgaFolder)project.RootFolder.ObjectByPath["/@Components|kind=Components"];
                MgaMetaModel componentMeta = (MgaMetaModel)project.RootMeta.RootFolder.DefinedFCOByName["Component", false];
                var component = components.CreateRootObject((MgaMetaFCO)componentMeta);
                component.Name = sanitizedDevicename;

                // Console.WriteLine("Libs " + string.Join("\n", project.RootFolder.ChildFolders.OfType<MgaFolder>().Select(x => x.LibraryName + " " + x.Name).ToArray()));
                var cyPhyComponent = ISIS.GME.Dsml.CyPhyML.Classes.Component.Cast(component);

                var import = new CyPhyComponentAuthoring.Modules.EDAModelImport();

                import.SetCurrentComp(cyPhyComponent);
                import.CurrentObj = component;

                import.ImportSelectedEagleDevice(deviceName, eagleFilePath, cyPhyComponent);

            }
            finally
            {
                project.CommitTransaction();
            }
        }
    }
}
