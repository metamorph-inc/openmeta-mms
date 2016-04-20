using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using System.IO;
using CyPhyGUIs;
using META;

namespace CyPhy2CADPCB
{
    class CyPhyParser
    {
        public CyPhyParser(GMELogger Logger)
        {
            logger = Logger;
        }
        
        private GMELogger logger;

        public AbstractClasses.Design ParseCyPhyDesign(CyPhy.ComponentAssembly componentAssembly)
        {
            AbstractClasses.Design design = new AbstractClasses.Design()
            {
                Name = componentAssembly.Name,
                ID = componentAssembly.Attributes.ID.ToString()
            };

            design.TopContainer = ParseCyPhyComponentAssembly(componentAssembly);

            return design;
        }

        private AbstractClasses.Container ParseCyPhyComponentAssembly(CyPhy.ComponentAssembly componentAssembly)
        {
            AbstractClasses.Container container = new AbstractClasses.Container()
            {
                name = componentAssembly.Name
            };

            foreach (var subCA in componentAssembly.Children.ComponentAssemblyCollection)
            {
                container.containers.Add(ParseCyPhyComponentAssembly(subCA));
            }
            foreach (var comp in componentAssembly.Children.ComponentCollection)
            {
                container.components.Add(ParseCyPhyComponent(comp));
            }

            return container;
        }

        private AbstractClasses.Component ParseCyPhyComponent(CyPhy.Component component)
        {
            string classification = component.Attributes.Classifications.ToString();
            if (   String.Compare(classification, "ara_template") == 0
                || String.Compare(classification, "template.ara_module_template") == 0)
            {
                // Stock module present for component replacement.
                AbstractClasses.AraTemplateComponent template = new AbstractClasses.AraTemplateComponent()
                {
                    name = component.Attributes.InstanceGUID,
                    classification = classification
                };
                ParseAraTemplateComponent(component, template);
                return template;
            }

            AbstractClasses.Component rtn = new AbstractClasses.Component()
            {
                // MOT-656 Switch to using component GUID as map key. Each component guaranteed to have a GUID, no need to check.
                name = component.Attributes.InstanceGUID,
                classification = classification
            };

            // Check for only one EDAModel
            IEnumerable<CyPhy.EDAModel> edas = component.Children.EDAModelCollection;
            if (edas.Count() > 1)
            {
                logger.WriteError("Multiple EDAModels found for component {0}. Component should " +
                                  "only have one EDAModel.", component.Name);
                return null;
            }

            IEnumerable<CyPhy.CAD2EDATransform> xforms = component.Children.CAD2EDATransformCollection;
            if (xforms.Count() == 0)
            {
                if (edas.Count() == 0)
                {
                    logger.WriteInfo("Skipping component {0}, no EDAModel or CAD2EDATransform objects found.", component.Name);
                }
                else
                {
                    logger.WriteWarning("EDAModel found for component {0} with no CAD2EDATransform, will generate " +
                                        "placeholder in visualizer based on EDAModel dimensions.", component.Name);
                    CyPhy.EDAModel edaModel = edas.First();
                }
            }
            else
            {
                // At this point you know all transforms point to the same EDAModel. The language also only
                //    allows for one transform connection per CAD model, so you know there are no duplicate
                //    transforms.
                foreach (var xform in xforms)
                {
                    CyPhy.CADModel cadModel = xform.SrcEnds.CADModel;
                    AddCadModelToComponent(xform, cadModel, component, rtn);
                }
            }

            return rtn;                
        }

        private void AddCadModelToComponent(CyPhy.CAD2EDATransform xform,
                                            CyPhy.CADModel cadModel,
                                            CyPhy.Component component,
                                            AbstractClasses.Component rtn)
        {
            string cadPath;
            bool retVal = cadModel.TryGetResourcePath(out cadPath, ComponentLibraryManager.PathConvention.REL_TO_PROJ_ROOT);
            if (retVal == false)
            {
                logger.WriteError("Unable to get CADModel's associated resource file path for component {0}", component.Name);
            }

            if (cadModel.Attributes.FileFormat == CyPhyClasses.CADModel.AttributesClass.FileFormat_enum.AP_203 ||
                cadModel.Attributes.FileFormat == CyPhyClasses.CADModel.AttributesClass.FileFormat_enum.AP_214)          
            {
                rtn.cadModels.Add(new AbstractClasses.STEPModel()
                {
                    path = cadPath,
                    rotationVector = new XYZTuple<Double, Double, Double>(xform.Attributes.RotationX,
                                                                          xform.Attributes.RotationY,
                                                                          xform.Attributes.RotationZ),
                    translationVector = new XYZTuple<Double, Double, Double>(xform.Attributes.TranslationX,
                                                                             xform.Attributes.TranslationY,
                                                                             xform.Attributes.TranslationZ),
                    scalingVector = new XYZTuple<Double, Double, Double>(xform.Attributes.ScaleX,
                                                                         xform.Attributes.ScaleY,
                                                                         xform.Attributes.ScaleZ),
                });
            }
            else if (cadModel.Attributes.FileFormat == CyPhyClasses.CADModel.AttributesClass.FileFormat_enum.STL)
            {
                rtn.cadModels.Add(new AbstractClasses.STLModel()
                {
                    path = cadPath,
                    rotationVector = new XYZTuple<Double, Double, Double>(xform.Attributes.RotationX,
                                                                          xform.Attributes.RotationY,
                                                                          xform.Attributes.RotationZ),
                    translationVector = new XYZTuple<Double, Double, Double>(xform.Attributes.TranslationX,
                                                                             xform.Attributes.TranslationY,
                                                                             xform.Attributes.TranslationZ),
                    scalingVector = new XYZTuple<Double, Double, Double>(xform.Attributes.ScaleX,
                                                                         xform.Attributes.ScaleY,
                                                                         xform.Attributes.ScaleZ),
                });

            }
            else
            {
                logger.WriteError("Visualizer currently only supports STP & STL files. Component {0} has an " +
                                  "EDAModel connected to a non-STEP/STL formatted CADModel.", component.Name);
            }
        }

        private void ParseAraTemplateComponent(CyPhy.Component component, AbstractClasses.AraTemplateComponent rtn)
        {
            // 1) Check only one CADModel (must be STEP)
            // 2) Grab CADModel resource (path to stock STEP file)
            // 3) Grab parameters in CADModel

            // 1
            IEnumerable<CyPhy.CADModel> cadModels = component.Children.CADModelCollection;
            if (cadModels.Count() == 0 || cadModels.Count() > 1)
            {
                logger.WriteError("Ara template component {0} must have one and only one CADModel.", component.Name);
            }
            CyPhy.CADModel cadModel = cadModels.First();
            if (cadModel.Attributes.FileFormat != CyPhyClasses.CADModel.AttributesClass.FileFormat_enum.AP_203 &&
                cadModel.Attributes.FileFormat != CyPhyClasses.CADModel.AttributesClass.FileFormat_enum.AP_214)
            {
                logger.WriteError("Ara template component {0} points to a non-STEP formatted component. Template " +
                                  "components must reference a STEP file only.", component.Name);
            }

            // 2
            string cadPath;
            bool retVal = cadModel.TryGetResourcePath(out cadPath, ComponentLibraryManager.PathConvention.REL_TO_PROJ_ROOT);
            if (retVal == false)
            {
                logger.WriteError("Unable to get CADModel's associated resource file path for component {0}", component.Name);
            }
            rtn.cadModels.Add(new AbstractClasses.STEPModel()
            {
                path = Path.Combine("..", "..", cadPath)
            });

            // 3
            rtn.parameters = new List<string>();
            foreach (var param in cadModel.Children.CADParameterCollection)
            {
                rtn.parameters.Add(param.Name);
            }
        }
    }
}
