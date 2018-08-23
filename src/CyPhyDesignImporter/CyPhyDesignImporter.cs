using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CyPhyComponentImporter;
using GME.CSharp;
using GME.MGA;
using GME.MGA.Core;
using GME.MGA.Meta;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using CyPhyML = ISIS.GME.Dsml.CyPhyML.Interfaces;
using ISIS.GME.Common.Interfaces;
using META;

namespace CyPhyDesignImporter
{
    public class AVMDesignImporter : AVM2CyPhyML.AVM2CyPhyMLBuilder
    {
        string projroot;

        public AVMDesignImporter(GMEConsole console, IMgaProject project, object messageConsoleParameter = null)
            : base(CyPhyClasses.RootFolder.GetRootFolder((MgaProject)project), messageConsoleParameter)
        {
            projroot = Path.GetDirectoryName(project.ProjectConnStr.Substring("MGA=".Length));
            init(true);
        }

        public Dictionary<string, CyPhy.Component> avmidComponentMap
        {
            get
            {
                // TODO memoize
                CyPhy.RootFolder rootFolder = ISIS.GME.Common.Utils.CreateObject<CyPhyClasses.RootFolder>(project.RootFolder as MgaObject);
                return CyPhyComponentImporterInterpreter.getCyPhyMLComponentDictionary_ByAVMID(rootFolder);
            }
        }

        public Model[] ImportFiles(string[] fileNames, DesignImportMode mode = AVMDesignImporter.DesignImportMode.CREATE_DS)
        {
            List<Model> ret = new List<Model>();
            CyPhy.RootFolder rootFolder = ISIS.GME.Common.Utils.CreateObject<CyPhyClasses.RootFolder>(project.RootFolder as MgaObject);
            Dictionary<string, CyPhy.Component> avmidComponentMap = CyPhyComponentImporterInterpreter.getCyPhyMLComponentDictionary_ByAVMID(rootFolder);

            foreach (var inputFilePath in fileNames)
            {
                var container = ImportFile(inputFilePath, mode);
                ret.Add(container);
            }
            return ret.ToArray();
        }

        public Model ImportFile(string inputFilePath, DesignImportMode mode = DesignImportMode.CREATE_DS)
        {
            writeMessage(String.Format("Importing {0}", inputFilePath), MessageType.INFO);

            UnzipToTemp unzip = null;
            bool bZipArchive = Path.GetExtension(inputFilePath).Equals(".adp");
            if (bZipArchive)
            {
                unzip = new UnzipToTemp(null);
                List<string> entries = unzip.UnzipFile(inputFilePath);
                inputFilePath = entries.Where(entry => Path.GetDirectoryName(entry) == "" && entry.ToLower().EndsWith(".adm")).FirstOrDefault();
                if (inputFilePath != null)
                {
                    inputFilePath = Path.Combine(unzip.TempDirDestination, inputFilePath);
                }
            }

            Model rtn = null;

            avm.Design ad_import = null;
            using (unzip)
            {
                using (StreamReader streamReader = new StreamReader(inputFilePath))
                {
                    ad_import = CyPhyDesignImporterInterpreter.DeserializeAvmDesignXml(streamReader);
                }
                if (ad_import == null)
                {
                    throw new Exception("Could not load ADM file.");
                }

                rtn = ImportDesign(ad_import, mode);

                // Copy artifacts
                if (bZipArchive)
                {
                    // Delete ADM file from tmp folder
                    File.Delete(inputFilePath);

                    foreach (var keyAndEntity in id2DesignEntity)
                    {
                        string id = keyAndEntity.Key;
                        var ca = keyAndEntity.Value as CyPhy.ComponentAssembly;
                        string tempPath = Path.Combine(unzip.TempDirDestination, id);
                        if (Directory.Exists(tempPath))
                        {
                            var pathCA = ca.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE);
                            DirectoryCopy(tempPath, pathCA, true);
                        }
                    }
                }
            }

            return rtn;
        }

        // From http://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }


        public enum DesignImportMode
        {
            CREATE_CAS,
            CREATE_DS,
            CREATE_CA_IF_NO_DS_CONCEPTS,
        }

        public Model ImportDesign(avm.Design ad_import, DesignImportMode mode = DesignImportMode.CREATE_DS)
        {
            TellCyPhyAddonDontAssignIds();

            // TODO: check ad_import.SchemaVersion
            CyPhy.DesignEntity cyphy_container;

            if (mode == DesignImportMode.CREATE_CA_IF_NO_DS_CONCEPTS)
            {
                bool containsNonCompound = false;
                Queue<avm.Container> containers = new Queue<avm.Container>();
                containers.Enqueue(ad_import.RootContainer);
                while (containers.Count > 0)
                {
                    avm.Container container = containers.Dequeue();
                    containsNonCompound |= container is avm.Optional || container is avm.Alternative;
                    foreach (var subcontainer in container.Container1)
                    {
                        containers.Enqueue(subcontainer);
                    }
                }
                if (containsNonCompound)
                {
                    cyphy_container = CreateDesignSpaceRoot(ad_import);
                }
                else
                {
                    cyphy_container = CreateComponentAssemblyRoot(ad_import);
                }
            }
            else if (mode == DesignImportMode.CREATE_CAS)
            {
                cyphy_container = CreateComponentAssemblyRoot(ad_import);
            }
            else if (mode == DesignImportMode.CREATE_DS)
            {
                cyphy_container = CreateDesignSpaceRoot(ad_import);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Unrecognized mode " + mode.ToString());
            }

            var ad_container = ad_import.RootContainer;

            ImportContainer(cyphy_container, ad_container);

            processValues();
            processPorts();

            Dictionary<avm.ConnectorCompositionTarget, avm.ConnectorCompositionTarget> connectorMap = new Dictionary<avm.ConnectorCompositionTarget, avm.ConnectorCompositionTarget>();
            foreach (var obj in this._avmCyPhyMLObjectMap)
            {
                if (obj.Key is avm.ConnectorCompositionTarget)
                {
                    avm.ConnectorCompositionTarget ad_compositionTarget1 = (avm.ConnectorCompositionTarget)obj.Key;
                    foreach (var ad_compositionTarget2ID in ad_compositionTarget1.ConnectorComposition.Where(id => string.IsNullOrEmpty(id) == false))
                    {
                        var ad_compositionTarget2 = _idConnectorMap[ad_compositionTarget2ID];
                        var cyphy_target = _avmCyPhyMLObjectMap[ad_compositionTarget2]; // TODO: handle lookup failure
                        if (string.Compare(ad_compositionTarget1.ID, ad_compositionTarget2.ID) < 0)
                        {
                            continue;
                        }
                        ConnectConnectorsAcrossHierarchy(obj.Value, cyphy_target);
                    }
                }
            }

            AddReferenceCoordinateSystemForAssemblyRoot(ad_import, cyphy_container);
            // Get ResourceDependencies
            if (cyphy_container.Kind == "ComponentAssembly")
            {
                var ca_cyphy_container = CyPhyClasses.ComponentAssembly.Cast(cyphy_container.Impl);
                foreach (var avmRes in ad_import.ResourceDependency)
                {
                    var cyphy_resource = CyPhyClasses.Resource.Create(ca_cyphy_container);
                    cyphy_resource.Name = avmRes.Name;
                    
                    if (!String.IsNullOrWhiteSpace(avmRes.Hash))
                    {
                        cyphy_resource.Attributes.Hash = avmRes.Hash;
                    }
                    if (!String.IsNullOrWhiteSpace(avmRes.ID))
                    {
                        cyphy_resource.Attributes.ID = avmRes.ID;
                    }
                    if (!String.IsNullOrWhiteSpace(avmRes.Notes))
                    {
                        cyphy_resource.Attributes.Notes = avmRes.Notes;
                    }
                    
                    cyphy_resource.Attributes.Path = avmRes.Path;

                    SetLayoutData(avmRes, cyphy_resource.Impl);
                }
            }

            DoLayout();

            return (Model)cyphy_container;
        }

        private IEnumerable<Model> getParents(Model fco)
        {
            yield return fco;
            Model parent = fco.ParentContainer as Model;
            while (parent != null)
            {
                yield return parent;
                parent = parent.ParentContainer as Model;
            }
        }

        private void ConnectConnectorsAcrossHierarchy(object cyphy_source, object cyphy_target)
        {
            Model source_parent;
            Model source_connector;
            if (cyphy_source is KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>)
            {
                source_parent = (Model)((KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>)cyphy_source).Key.ParentContainer;
                source_connector = (Model)((KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>)cyphy_source).Value;
            }
            else
            {
                source_connector = ((Model)cyphy_source);
                source_parent = (Model)source_connector.ParentContainer;
            }
            Model target_parent;
            Model target_connector;
            if (cyphy_target is KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>)
            {
                target_parent = (Model)((KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>)cyphy_target).Key.ParentContainer;
                target_connector = (Model)((KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>)cyphy_target).Value;
            }
            else
            {
                target_connector = ((Model)cyphy_target);
                target_parent = (Model)target_connector.ParentContainer;
            }

            if (target_parent.ID == source_parent.ID
                || (AVM2CyPhyML.AVM2CyPhyMLBuilder.GetFCOObjectReference(cyphy_target) == null && source_parent.ID == target_parent.ParentContainer.ID)
                || (AVM2CyPhyML.AVM2CyPhyMLBuilder.GetFCOObjectReference(cyphy_source) == null && target_parent.ID == source_parent.ParentContainer.ID)
                )
            {
                makeConnection(cyphy_source, cyphy_target, typeof(CyPhy.ConnectorComposition).Name);
                return;
            }
            // AVM2CyPhyML.AVM2CyPhyMLBuilder.GetFCOObject(
            List<Model> source_parents = getParents(source_parent).ToList();
            List<Model> target_parents = getParents(target_parent).ToList();
            if (source_parents.Last().ID != target_parents.Last().ID)
            {
                throw new ApplicationException(String.Format("'{0}' and '{1}' cannot be connected", source_parent.Path, target_parent.Path));
            }
            Model commonAncestor = source_parent;
            // remove common ancestors from source_ and target_parents
            while (true)
            {
                if (source_parents[source_parents.Count - 1].ID == target_parents[target_parents.Count - 1].ID)
                {
                    commonAncestor = source_parents[source_parents.Count - 1];
                    source_parents.RemoveAt(source_parents.Count - 1);
                    target_parents.RemoveAt(target_parents.Count - 1);
                }
                else
                {
                    break;
                }
                if (source_parents.Count == 0 || target_parents.Count == 0)
                {
                    break;
                }
            }
            var source_intermediary = ConnectCompositionThroughHierarchy(cyphy_source, source_connector, source_parents);
            var target_intermediary = ConnectCompositionThroughHierarchy(cyphy_target, target_connector, target_parents);

            MgaMetaRole connectorRole = ((MgaMetaModel)commonAncestor.Impl.MetaBase).RoleByName["Connector"];
            var lastConnector = CyPhyClasses.Connector.Cast(((MgaModel)commonAncestor.Impl).DeriveChildObject((MgaFCO)source_connector.Impl, connectorRole, true));

            makeConnection(lastConnector, source_intermediary, typeof(CyPhy.ConnectorComposition).Name);
            makeConnection(lastConnector, target_intermediary, typeof(CyPhy.ConnectorComposition).Name);
        }

        private Object ConnectCompositionThroughHierarchy(object cyphy_source, Model source_connector, List<Model> source_parents)
        {
            object srcIntermediary = cyphy_source;
            for (int i = 0; i < source_parents.Count; i++)
            {
                Model parent = source_parents[i];
                MgaMetaRole connectorRole = ((MgaMetaModel)parent.Impl.MetaBase).RoleByName["Connector"];
                var newIntermediary = CyPhyClasses.Connector.Cast(((MgaModel)parent.Impl).DeriveChildObject((MgaFCO)source_connector.Impl, connectorRole, true));
                makeConnection(srcIntermediary, newIntermediary, typeof(CyPhy.ConnectorComposition).Name);
                srcIntermediary = newIntermediary;
            }

            return srcIntermediary;
        }

        private void TellCyPhyAddonDontAssignIds()
        {
            var cyPhyAddon = project.AddOnComponents.Cast<IMgaComponentEx>().Where(x => x.ComponentName.ToLowerInvariant() == "CyPhyAddOn".ToLowerInvariant()).FirstOrDefault();
            if (cyPhyAddon != null)
            {
                cyPhyAddon.ComponentParameter["DontAssignGuidsOnNextTransaction".ToLowerInvariant()] = true;
            }
        }

        private void AddReferenceCoordinateSystemForAssemblyRoot(avm.Design ad_import, CyPhy.DesignEntity cyphy_container)
        {
            foreach (var root in ad_import.DomainFeature.OfType<avm.cad.AssemblyRoot>())
            {
                CyPhyML.ComponentRef componentRef;
                if (idToComponentInstanceMap.TryGetValue(root.AssemblyRootComponentInstance, out componentRef))
                {
                    MgaFCO rcs = CreateChild((ISIS.GME.Common.Interfaces.Model)componentRef.ParentContainer, typeof(CyPhyML.ReferenceCoordinateSystem));
                    rcs.Name = "AssemblyRoot";
                    CyPhyML.ReferenceCoordinateSystem componentRcs = componentRef.Referred.Component.Children.ReferenceCoordinateSystemCollection.FirstOrDefault();
                    if (componentRcs == null)
                    {
                        componentRcs = CyPhyClasses.ReferenceCoordinateSystem.Create(componentRef.Referred.Component);
                    }

                    ((MgaModel)componentRef.ParentContainer.Impl).CreateSimpleConnDisp(((MgaMetaModel)componentRef.ParentContainer.Impl.MetaBase).RoleByName[typeof(CyPhyML.RefCoordSystem2RefCoordSystem).Name],
                        rcs, (MgaFCO)componentRcs.Impl, null, (MgaFCO)componentRef.Impl);

                    while (rcs.ParentModel.ID != cyphy_container.ID)
                    {
                        var oldrcs = rcs;
                        rcs = CreateChild(rcs.ParentModel.ParentModel, typeof(CyPhyML.ReferenceCoordinateSystem));
                        rcs.Name = "AssemblyRoot";
                        ((MgaModel)rcs.ParentModel).CreateSimplerConnDisp(((MgaMetaModel)rcs.ParentModel.Meta).RoleByName[typeof(CyPhyML.RefCoordSystem2RefCoordSystem).Name],
                            rcs, oldrcs);
                    }
                }
            }
        }

        private CyPhy.DesignContainer CreateDesignSpaceRoot(avm.Design ad_import)
        {
            CyPhy.DesignSpace ds;
            CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder((MgaProject)project);
            ds = rf.Children.DesignSpaceCollection.Where(d => d.Name == "DesignSpaces").FirstOrDefault();
            if (ds == null)
            {
                ds = CyPhyClasses.DesignSpace.Create(rf);
                ds.Name = "DesignSpaces";
            }

            CyPhy.DesignContainer cyphy_container = CyPhyClasses.DesignContainer.Create(ds);
            // container.Name = ad_import.Name; RootContainer has a name too
            int designID;
            if (int.TryParse(ad_import.DesignID, out designID))
            {
                cyphy_container.Attributes.ID = designID;
            }
            cyphy_container.Attributes.ContainerType = CyPhyClasses.DesignContainer.AttributesClass.ContainerType_enum.Compound;
            return cyphy_container;
        }

        private CyPhy.ComponentAssembly CreateComponentAssemblyRoot(avm.Design ad_import)
        {
            CyPhy.ComponentAssemblies cyphy_cas;
            CyPhy.RootFolder rf = CyPhyClasses.RootFolder.GetRootFolder((MgaProject)project);
            cyphy_cas = rf.Children.ComponentAssembliesCollection.Where(d => d.Name == typeof(CyPhyClasses.ComponentAssemblies).Name).FirstOrDefault();
            if (cyphy_cas == null)
            {
                cyphy_cas = CyPhyClasses.ComponentAssemblies.Create(rf);
                cyphy_cas.Name = typeof(CyPhyClasses.ComponentAssemblies).Name;
            }
            CyPhy.ComponentAssembly cyphy_container = CyPhyClasses.ComponentAssembly.Create(cyphy_cas);
            // container.Name = ad_import.Name; RootContainer has a name too
            // TODO: check ad_import.SchemaVersion
            int designID;
            if (int.TryParse(ad_import.DesignID, out designID))
            {
                cyphy_container.Attributes.ID = designID;
            }
            if (string.IsNullOrEmpty(ad_import.RootContainer.Description) == false)
            {
                cyphy_container.Attributes.Description = ad_import.RootContainer.Description;
            }
            return cyphy_container;
        }

        Dictionary<avm.schematic.eda.RelativeLayerEnum, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum> d_RelativeLayer =
            new Dictionary<avm.schematic.eda.RelativeLayerEnum, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum>() 
        {
            { avm.schematic.eda.RelativeLayerEnum.Opposite, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.Opposite },
            { avm.schematic.eda.RelativeLayerEnum.Same, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeLayer_enum.Same }
        };
        Dictionary<avm.schematic.eda.RelativeLayerEnum, CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum> d_RelativeRangeLayer =
            new Dictionary<avm.schematic.eda.RelativeLayerEnum, CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum>() 
        {
            { avm.schematic.eda.RelativeLayerEnum.Opposite, CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.Opposite },
            { avm.schematic.eda.RelativeLayerEnum.Same, CyPhyClasses.RelativeRangeConstraint.AttributesClass.RelativeLayer_enum.Same }
        };
        Dictionary<avm.schematic.eda.RangeConstraintTypeEnum, CyPhyClasses.RangeLayoutConstraint.AttributesClass.Type_enum> d_RangeType =
            new Dictionary<avm.schematic.eda.RangeConstraintTypeEnum, CyPhyClasses.RangeLayoutConstraint.AttributesClass.Type_enum>()
        {
            { avm.schematic.eda.RangeConstraintTypeEnum.Inclusion, CyPhyClasses.RangeLayoutConstraint.AttributesClass.Type_enum.Inclusion},
            { avm.schematic.eda.RangeConstraintTypeEnum.Exclusion, CyPhyClasses.RangeLayoutConstraint.AttributesClass.Type_enum.Exclusion}
        };
        Dictionary<avm.schematic.eda.RelativeRotationEnum, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum> d_RelativeRotation = 
            new Dictionary<avm.schematic.eda.RelativeRotationEnum, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum>()
        {
            { avm.schematic.eda.RelativeRotationEnum.r0, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._0 },
            { avm.schematic.eda.RelativeRotationEnum.r90, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._90 },
            { avm.schematic.eda.RelativeRotationEnum.r180, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._180 },
            { avm.schematic.eda.RelativeRotationEnum.r270, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum._270 },
            { avm.schematic.eda.RelativeRotationEnum.NoRestriction, CyPhyClasses.RelativeLayoutConstraint.AttributesClass.RelativeRotation_enum.No_Restriction }
        };
        
        Dictionary<string, CyPhy.DesignEntity> id2DesignEntity = new Dictionary<string, CyPhyML.DesignEntity>();
        private void ImportContainer(CyPhy.DesignEntity cyphy_container, avm.Container ad_container)
        {
            // If an ID is provided, add to map.
            if (!String.IsNullOrWhiteSpace(ad_container.ID))
            {
                id2DesignEntity.Add(ad_container.ID, cyphy_container);
                if (cyphy_container is CyPhyML.ComponentAssembly)
                {
                    ((CyPhyML.ComponentAssembly)cyphy_container).Attributes.ManagedGUID = ad_container.ID;
                }
            }

            cyphy_container.Name = ad_container.Name;
            if (cyphy_container is CyPhy.ComponentAssembly) {
                var asm = cyphy_container as CyPhy.ComponentAssembly;
                asm.Attributes.Classifications = String.Join("\n", ad_container);
            }
            AVM2CyPhyML.CyPhyMLComponentBuilder.SetLayoutData(ad_container, cyphy_container.Impl);

            Dictionary<Type, CyPhyClasses.DesignContainer.AttributesClass.ContainerType_enum> typeToAttribute = new Dictionary<Type, CyPhyClasses.DesignContainer.AttributesClass.ContainerType_enum>()
            {
                {typeof(avm.DesignSpaceContainer), CyPhyClasses.DesignContainer.AttributesClass.ContainerType_enum.Compound},
                {typeof(avm.Alternative), CyPhyClasses.DesignContainer.AttributesClass.ContainerType_enum.Alternative},
                {typeof(avm.Optional), CyPhyClasses.DesignContainer.AttributesClass.ContainerType_enum.Optional},
                {typeof(avm.Compound), CyPhyClasses.DesignContainer.AttributesClass.ContainerType_enum.Compound},
            };
            if (cyphy_container is CyPhy.DesignContainer)
            {
                ((CyPhy.DesignContainer)cyphy_container).Attributes.ContainerType = typeToAttribute[ad_container.GetType()];
                if (ad_container is avm.Alternative)
                {
                    ((IMgaFCO)cyphy_container.Impl).SetRegistryValueDisp("icon", "alternative_ds.png");
                }
                if (ad_container is avm.Optional)
                {
                    ((IMgaFCO)cyphy_container.Impl).SetRegistryValueDisp("icon", "optional_ds");
                }
            }
            if (ad_container is avm.Alternative)
            {
                foreach (var ad_mux in ((avm.Alternative)ad_container).ValueFlowMux)
                {
                    processMux((CyPhy.DesignContainer)cyphy_container, ad_mux);
                }
            }

            foreach (avm.Port avmPort in ad_container.Port)
            {
                if (cyphy_container is CyPhy.DesignContainer)
                {
                    process((CyPhy.DesignContainer)cyphy_container, avmPort);
                }
                else
                {
                    process((CyPhy.ComponentAssembly)cyphy_container, avmPort);
                }
            }
            foreach (var ad_connector in ad_container.Connector)
            {
                var cyphy_connector = CyPhyClasses.Connector.Cast(CreateChild((ISIS.GME.Common.Interfaces.Model)cyphy_container, typeof(CyPhyClasses.Connector)));
                processConnector(ad_connector, cyphy_connector);
            }

            foreach (var ad_prop in ad_container.Property)
            {
                if (cyphy_container is CyPhy.DesignContainer)
                {
                    process((CyPhy.DesignContainer)cyphy_container, ad_prop);
                }
                else
                {
                    process((CyPhy.ComponentAssembly)cyphy_container, ad_prop);
                }
            }

            foreach (var ad_componentinstance in ad_container.ComponentInstance)
            {
                CyPhy.ComponentRef cyphy_componentref;
                if (cyphy_container is CyPhy.DesignContainer)
                {
                    cyphy_componentref = CyPhyClasses.ComponentRef.Create((CyPhy.DesignContainer)cyphy_container);
                }
                else
                {
                    cyphy_componentref = CyPhyClasses.ComponentRef.Create((CyPhy.ComponentAssembly)cyphy_container);
                }
                ImportComponentInstance(ad_componentinstance, cyphy_componentref);
            }

            foreach (var ad_childcontainer in ad_container.Container1)
            {
                CyPhy.DesignEntity cyphy_childcontainer;
                if (cyphy_container is CyPhy.DesignContainer)
                {
                    cyphy_childcontainer = CyPhyClasses.DesignContainer.Create((CyPhy.DesignContainer)cyphy_container);
                    // TODO: assign cyphy_childcontainer.Attributes.Description (need it in CyPhyML.xme first)
                }
                else
                {
                    cyphy_childcontainer = CyPhyClasses.ComponentAssembly.Create((CyPhy.ComponentAssembly)cyphy_container);
                    if (string.IsNullOrEmpty(ad_childcontainer.Description) == false)
                    {
                        ((CyPhyML.ComponentAssembly)cyphy_childcontainer).Attributes.Description = ad_childcontainer.Description;
                    }
                }
                ImportContainer(cyphy_childcontainer, ad_childcontainer);
            }

            foreach (var constraint in ad_container.ContainerFeature.OfType<avm.schematic.eda.ExactLayoutConstraint>())
            {
                CyPhyML.ExactLayoutConstraint cyphy_constraint = CyPhyClasses.ExactLayoutConstraint.Cast(CreateChild((ISIS.GME.Common.Interfaces.Model)cyphy_container, typeof(CyPhyClasses.ExactLayoutConstraint)));
                cyphy_constraint.Name = typeof(CyPhyML.ExactLayoutConstraint).Name;
                SetLayoutData(constraint, cyphy_constraint.Impl);

                if (constraint.XSpecified)
                {
                    cyphy_constraint.Attributes.X = constraint.X.ToString();
                }
                if (constraint.YSpecified)
                {
                    cyphy_constraint.Attributes.Y = constraint.Y.ToString();
                }
                if (constraint.LayerSpecified)
                {
                    cyphy_constraint.Attributes.Layer = d_LayerEnumMap[constraint.Layer];
                }
                if (constraint.RotationSpecified)
                {
                    cyphy_constraint.Attributes.Rotation = d_RotationEnumMap[constraint.Rotation];
                }
                if (false == String.IsNullOrWhiteSpace(constraint.Notes))
                {
                    cyphy_constraint.Attributes.Notes = constraint.Notes;
                }

                foreach (var idTarget in constraint.ConstraintTarget)
                {
                    CyPhyML.ComponentRef compInstance;
                    if (idToComponentInstanceMap.TryGetValue(idTarget, out compInstance))
                    {
                        CyPhyClasses.ApplyExactLayoutConstraint.Connect(cyphy_constraint, compInstance);
                    }
                }

                foreach (var idTarget in constraint.ContainerConstraintTarget)
                {
                    CyPhyML.DesignEntity deInstance;
                    if (id2DesignEntity.TryGetValue(idTarget, out deInstance))
                    {
                        CyPhyClasses.ApplyExactLayoutConstraint.Connect(cyphy_constraint, deInstance);
                    }
                }
            }

            foreach (var constraint in ad_container.ContainerFeature.OfType<avm.schematic.eda.RangeLayoutConstraint>())
            {
                CyPhyML.RangeLayoutConstraint cyphy_constraint = CyPhyClasses.RangeLayoutConstraint.Cast(CreateChild((ISIS.GME.Common.Interfaces.Model)cyphy_container, typeof(CyPhyClasses.RangeLayoutConstraint)));
                cyphy_constraint.Name = typeof(CyPhyML.RangeLayoutConstraint).Name;
                SetLayoutData(constraint, cyphy_constraint.Impl);

                cyphy_constraint.Attributes.LayerRange = d_LayerRangeEnumMap[constraint.LayerRange];
                if (constraint.XRangeMinSpecified && constraint.XRangeMaxSpecified)
                {
                    cyphy_constraint.Attributes.XRange = constraint.XRangeMin + ":" + constraint.XRangeMax;
                }                
                if (constraint.YRangeMinSpecified && constraint.YRangeMaxSpecified)
                {
                    cyphy_constraint.Attributes.YRange = constraint.YRangeMin + ":" + constraint.YRangeMax;
                }
                if (constraint.TypeSpecified)
                {
                    cyphy_constraint.Attributes.Type = d_RangeType[constraint.Type];
                }                
                if (false == String.IsNullOrWhiteSpace(constraint.Notes))
                {
                    cyphy_constraint.Attributes.Notes = constraint.Notes;
                }

                foreach (var compId in constraint.ConstraintTarget)
                {
                    CyPhyML.ComponentRef compInstance;
                    if (idToComponentInstanceMap.TryGetValue(compId, out compInstance))
                    {
                        CyPhyClasses.ApplyRangeLayoutConstraint.Connect(cyphy_constraint, compInstance);
                    }
                }                
                foreach (var idTarget in constraint.ContainerConstraintTarget)
                {
                    CyPhyML.DesignEntity deInstance;
                    if (id2DesignEntity.TryGetValue(idTarget, out deInstance))
                    {
                        CyPhyClasses.ApplyRangeLayoutConstraint.Connect(cyphy_constraint, deInstance);
                    }
                }
            }

            foreach (var constraint in ad_container.ContainerFeature.OfType<avm.schematic.eda.RelativeLayoutConstraint>())
            {
                CyPhyML.RelativeLayoutConstraint cyphy_constraint = CyPhyClasses.RelativeLayoutConstraint.Cast(CreateChild((ISIS.GME.Common.Interfaces.Model)cyphy_container, typeof(CyPhyClasses.RelativeLayoutConstraint)));
                cyphy_constraint.Name = typeof(CyPhyML.RelativeLayoutConstraint).Name;
                SetLayoutData(constraint, cyphy_constraint.Impl);

                if (constraint.XOffsetSpecified)
                {
                    cyphy_constraint.Attributes.XOffset = constraint.XOffset.ToString();
                }
                if (constraint.YOffsetSpecified)
                {
                    cyphy_constraint.Attributes.YOffset = constraint.YOffset.ToString();
                }
                if (constraint.RelativeLayerSpecified)
                {
                    cyphy_constraint.Attributes.RelativeLayer = d_RelativeLayer[constraint.RelativeLayer];
                }
                if (constraint.RelativeRotationSpecified)
                {
                    cyphy_constraint.Attributes.RelativeRotation = d_RelativeRotation[constraint.RelativeRotation];
                }

                if (false == String.IsNullOrWhiteSpace(constraint.Notes))
                {
                    cyphy_constraint.Attributes.Notes = constraint.Notes;
                }

                foreach (var compId in constraint.ConstraintTarget)
                {
                    CyPhyML.ComponentRef compInstance;
                    if (idToComponentInstanceMap.TryGetValue(compId, out compInstance))
                    {
                        CyPhyClasses.ApplyRelativeLayoutConstraint.Connect(cyphy_constraint, compInstance);
                    }
                }
                if (string.IsNullOrWhiteSpace(constraint.Origin) == false)
                {
                    CyPhyML.ComponentRef compInstance;
                    if (idToComponentInstanceMap.TryGetValue(constraint.Origin, out compInstance))
                    {
                        CyPhyClasses.RelativeLayoutConstraintOrigin.Connect(compInstance, cyphy_constraint);
                    }
                }
            }

            foreach (var constraint in ad_container.ContainerFeature.OfType<avm.schematic.eda.RelativeRangeLayoutConstraint>())
            {
                CyPhyML.RelativeRangeConstraint cyphy_constraint = 
                    CyPhyClasses.RelativeRangeConstraint.Cast(CreateChild((ISIS.GME.Common.Interfaces.Model)cyphy_container, 
                                                              typeof(CyPhyClasses.RelativeRangeConstraint)));
                cyphy_constraint.Name = typeof(CyPhyML.RelativeRangeConstraint).Name;
                SetLayoutData(constraint, cyphy_constraint.Impl);

                if (constraint.RelativeLayerSpecified)
                {
                    cyphy_constraint.Attributes.RelativeLayer = d_RelativeRangeLayer[constraint.RelativeLayer];
                }
                if (constraint.XRelativeRangeMinSpecified && constraint.XRelativeRangeMaxSpecified)
                {
                    cyphy_constraint.Attributes.XOffsetRange = String.Format("{0}:{1}", constraint.XRelativeRangeMin, constraint.XRelativeRangeMax);
                }
                if (constraint.YRelativeRangeMinSpecified && constraint.YRelativeRangeMaxSpecified)
                {
                    cyphy_constraint.Attributes.YOffsetRange = String.Format("{0}:{1}", constraint.YRelativeRangeMin, constraint.YRelativeRangeMax);
                }
                if (false == String.IsNullOrWhiteSpace(constraint.Notes))
                {
                    cyphy_constraint.Attributes.Notes = constraint.Notes;
                }

                foreach (var compId in constraint.ConstraintTarget)
                {
                    CyPhyML.ComponentRef compInstance;
                    if (idToComponentInstanceMap.TryGetValue(compId, out compInstance))
                    {
                        CyPhyClasses.ApplyRelativeRangeLayoutConstraint.Connect(cyphy_constraint, compInstance);
                    }
                }
                foreach (var idTarget in constraint.ContainerConstraintTarget)
                {
                    CyPhyML.DesignEntity deInstance;
                    if (id2DesignEntity.TryGetValue(idTarget, out deInstance))
                    {
                        CyPhyClasses.ApplyRelativeRangeLayoutConstraint.Connect(cyphy_constraint, deInstance);
                    }
                }

                if (string.IsNullOrWhiteSpace(constraint.Origin) == false)
                {
                    CyPhyML.ComponentRef compInstance;
                    if (idToComponentInstanceMap.TryGetValue(constraint.Origin, out compInstance))
                    {
                        CyPhyClasses.RelativeRangeLayoutConstraintOrigin.Connect(compInstance, cyphy_constraint);
                    }
                }
            }

            foreach (var constraint in ad_container.ContainerFeature.OfType<avm.schematic.eda.GlobalLayoutConstraintException>())
            {
                CyPhyML.GlobalLayoutConstraintException cyphy_constraint =
                    CyPhyClasses.GlobalLayoutConstraintException.Cast(CreateChild((ISIS.GME.Common.Interfaces.Model)cyphy_container,
                                                                      typeof(CyPhyClasses.GlobalLayoutConstraintException)));
                cyphy_constraint.Name = typeof(CyPhyML.GlobalLayoutConstraintException).Name;
                SetLayoutData(constraint, cyphy_constraint.Impl);

                switch (constraint.Constraint)
                {
                    case avm.schematic.eda.GlobalConstraintTypeEnum.BoardEdgeSpacing:
                        cyphy_constraint.Attributes.Constraint = CyPhyClasses.GlobalLayoutConstraintException.AttributesClass.Constraint_enum.Board_Edge_Spacing;
                        break;
                    default:
                        throw new NotSupportedException("GlobalConstraintException value of " + constraint.Constraint.ToString() + " isn't supported");
                }

                cyphy_constraint.Attributes.Notes = constraint.Notes;
                foreach (var compId in constraint.ConstraintTarget)
                {
                    CyPhyML.ComponentRef compInstance;
                    if (idToComponentInstanceMap.TryGetValue(compId, out compInstance))
                    {
                        CyPhyClasses.ApplyGlobalLayoutConstraintException.Connect(cyphy_constraint, compInstance);
                    }
                }
                foreach (var idTarget in constraint.ContainerConstraintTarget)
                {
                    CyPhyML.DesignEntity deInstance;
                    if (id2DesignEntity.TryGetValue(idTarget, out deInstance))
                    {
                        CyPhyClasses.ApplyGlobalLayoutConstraintException.Connect(cyphy_constraint, deInstance);
                    }
                }
            }

            // Get ResourceDependencies
            if (cyphy_container.Kind == "ComponentAssembly")
            {
                var ca_cyphy_container = CyPhyClasses.ComponentAssembly.Cast(cyphy_container.Impl);
                foreach (var avmRes in ad_container.ResourceDependency)
                {
                    // hack for layoutFile
                    if (avmRes.Name == "layoutFile")
                    {
                        ((IMgaFCO)cyphy_container.Impl).set_RegistryValue("layoutFile", avmRes.Path);
                        foreach (var circuitLayout in ad_container.DomainModel.OfType<avm.schematic.eda.CircuitLayout>().Where(x => x.UsesResource == avmRes.ID))
                        {
                            ((IMgaFCO)cyphy_container.Impl).set_RegistryValue("layoutBox", circuitLayout.BoundingBoxes ?? "");
                        }
                        continue;
                    }

                    var cyphy_resource = CyPhyClasses.Resource.Create(ca_cyphy_container);
                    cyphy_resource.Name = avmRes.Name;

                    if (!String.IsNullOrWhiteSpace(avmRes.Hash))
                    {
                        cyphy_resource.Attributes.Hash = avmRes.Hash;
                    }
                    if (!String.IsNullOrWhiteSpace(avmRes.ID))
                    {
                        cyphy_resource.Attributes.ID = avmRes.ID;
                    }
                    if (!String.IsNullOrWhiteSpace(avmRes.Notes))
                    {
                        cyphy_resource.Attributes.Notes = avmRes.Notes;
                    }

                    cyphy_resource.Attributes.Path = avmRes.Path;

                    SetLayoutData(avmRes, cyphy_resource.Impl);
                }
            }

            foreach (var simpleFormula in ad_container.Formula.OfType<avm.SimpleFormula>())
            {
                CyPhyML.SimpleFormula cyphy_simpleFormula = CyPhyClasses.SimpleFormula.Cast(CreateChild((ISIS.GME.Common.Interfaces.Model)cyphy_container, typeof(CyPhyClasses.SimpleFormula)));
                process(simpleFormula, cyphy_simpleFormula);
            }

            foreach (var complexFormula in ad_container.Formula.OfType<avm.ComplexFormula>())
            {
                var cyphy_customFormula = CyPhyClasses.CustomFormula.Cast(CreateChild((ISIS.GME.Common.Interfaces.Model)cyphy_container, typeof(CyPhyClasses.CustomFormula)));
                processComplexFormula(complexFormula, cyphy_customFormula);
            }
        }

        private void processMux(CyPhyML.DesignContainer designContainer, avm.ValueFlowMux ad_mux)
        {
            _avmValueNodeIDMap.Add(ad_mux.ID, new KeyValuePair<avm.ValueNode, object>(null, ad_mux));
        }

        Dictionary<string, CyPhy.ComponentRef> idToComponentInstanceMap = new Dictionary<string, CyPhy.ComponentRef>();
        private void ImportComponentInstance(avm.ComponentInstance ad_componentinstance, CyPhy.ComponentRef cyphy_componentref)
        {
            AVM2CyPhyML.CyPhyMLComponentBuilder.SetLayoutData(ad_componentinstance, cyphy_componentref.Impl);

            ISIS.GME.Dsml.CyPhyML.Interfaces.Component component;
            if (avmidComponentMap.TryGetValue(ad_componentinstance.ComponentID, out component) == false)
            {
                throw new ApplicationException(String.Format("Cannot find Component with ID {0}. Has it been imported?", ad_componentinstance.ComponentID));
            }
            cyphy_componentref.Referred.Component = component;
            cyphy_componentref.Name = ad_componentinstance.Name;
            //cyphy_componentref.Attributes.ID = ad_componentinstance.ID;
            cyphy_componentref.Attributes.InstanceGUID = ad_componentinstance.ID;
            idToComponentInstanceMap[ad_componentinstance.ID] = cyphy_componentref;

            foreach (var ad_propinstance in ad_componentinstance.PrimitivePropertyInstance)
            {
                Func<IMgaObject, string> getID = o =>
                {
                    var id = ((IMgaFCO)o).StrAttrByName["ID"];
                    if (id == "")
                    {
                        id = "id-" + Guid.Parse(((IMgaFCO)o).GetGuidDisp()).ToString("D");
                    }
                    return id;
                };

                var cyphy_component = this.avmidComponentMap[ad_componentinstance.ComponentID];
                CyPhy.ValueFlowTarget cyphy_componentPort;
                try
                {
                    cyphy_componentPort = cyphy_component.AllChildren.OfType<CyPhy.ValueFlowTarget>()
                        .Where(x => getID(x.Impl) == ad_propinstance.IDinComponentModel).SingleOrDefault();
                }
                catch (System.InvalidOperationException e)
                {
                    throw new ApplicationException(String.Format("Error: more than one PrimitivePropertyInstance with ID '{0}' in '{1}'. Run the ComponentExporter to fix.",
                        ad_propinstance.IDinComponentModel, ad_componentinstance.Name));
                }

                _avmCyPhyMLObjectMap.Add(ad_propinstance, new KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>(cyphy_componentref, cyphy_componentPort));
                    registerValueNode(ad_propinstance.Value, ad_propinstance);
            }

            foreach (var ad_connectorInstance in ad_componentinstance.ConnectorInstance)
            {
                _idConnectorMap.Add(ad_connectorInstance.ID, ad_connectorInstance); // FIXME could be dup

                var cyphy_component = this.avmidComponentMap[ad_componentinstance.ComponentID];
                CyPhy.Connector cyphy_componentConnector;
                try
                {
                    cyphy_componentConnector = cyphy_component.AllChildren.OfType<CyPhy.Connector>()
                    .Where(x => ((MgaFCO)x.Impl).StrAttrByName["ID"] == ad_connectorInstance.IDinComponentModel).SingleOrDefault();
                }
                catch (System.InvalidOperationException e)
                {
                    throw new ApplicationException(String.Format("Error: more than one ConnectorInstance with ID '{0}' in '{1}'. Run the ComponentExporter to fix.",
                        ad_connectorInstance.IDinComponentModel, ad_componentinstance.Name));
                }

                if (cyphy_componentConnector == null)
                {
                    throw new ApplicationException("adm error: component instance " + ad_componentinstance.ID + " has connector with IDinComponentModel "
                        + ad_connectorInstance.IDinComponentModel + " that has no matching Connector in the Component");
                }

                _avmCyPhyMLObjectMap.Add(ad_connectorInstance, new KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>(cyphy_componentref, cyphy_componentConnector));
            }

            foreach (var ad_port in ad_componentinstance.PortInstance)
            {
                registerPort(ad_port);

                var cyphy_component = this.avmidComponentMap[ad_componentinstance.ComponentID];
                var cyphy_componentConnector = cyphy_component.AllChildren.OfType<CyPhy.Port>()
                    .Where(x => ((MgaFCO)x.Impl).StrAttrByName["ID"] == ad_port.IDinComponentModel).FirstOrDefault();
                if (cyphy_componentConnector == null)
                {
                    throw new ApplicationException("adm error: component instance " + ad_componentinstance.ID + " has connector with IDinComponentModel "
                        + ad_port.IDinComponentModel + " that has no matching Connector in the Component");
                }
                _avmCyPhyMLObjectMap.Add(ad_port, new KeyValuePair<ISIS.GME.Common.Interfaces.Reference, ISIS.GME.Common.Interfaces.FCO>(cyphy_componentref, cyphy_componentConnector));
            }
        }

        private MgaFCO CreateChild(ISIS.GME.Common.Interfaces.Model parent, Type type)
        {
            var role = ((MgaMetaModel)parent.Impl.MetaBase).RoleByName[type.Name];
            return (MgaFCO)((MgaModel)parent.Impl).CreateChildObject(role);
        }

        private MgaFCO CreateChild(MgaModel parent, Type type)
        {
            var role = ((MgaMetaModel)parent.MetaBase).RoleByName[type.Name];
            return (MgaFCO)((MgaModel)parent).CreateChildObject(role);
        }

        Dictionary<avm.schematic.eda.RotationEnum, string> d_RotationEnumMap = new Dictionary<avm.schematic.eda.RotationEnum,string>()
        {
            { avm.schematic.eda.RotationEnum.r0, "0"},
            { avm.schematic.eda.RotationEnum.r90, "1"},
            { avm.schematic.eda.RotationEnum.r180, "2"},
            { avm.schematic.eda.RotationEnum.r270, "3"}
        };

        Dictionary<avm.schematic.eda.LayerEnum, string> d_LayerEnumMap = new Dictionary<avm.schematic.eda.LayerEnum, string>()
        {
            { avm.schematic.eda.LayerEnum.Top, "0"},
            { avm.schematic.eda.LayerEnum.Bottom, "1"}
        };

        Dictionary<avm.schematic.eda.LayerRangeEnum, string> d_LayerRangeEnumMap = new Dictionary<avm.schematic.eda.LayerRangeEnum, string>()
        {
            { avm.schematic.eda.LayerRangeEnum.Top, "0"},
            { avm.schematic.eda.LayerRangeEnum.Bottom, "1"},
            { avm.schematic.eda.LayerRangeEnum.Either, "0:1"},
        };

    }
}
