using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace CyPhy2CADPCB
{
    public class XYZTuple<doubleX, doubleY, doubleZ>
    {
        public Double X { get; private set; }
        public Double Y { get; private set; }
        public Double Z { get; private set; }
        internal XYZTuple(Double x, Double y, Double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
    
    public class ComponentModel
    {
        public String cadpath { get; set; }
        public XYZTuple<Double, Double, Double> rotation { get; set; }
        public XYZTuple<Double, Double, Double> translation { get; set; }
        public XYZTuple<Double, Double, Double> scale { get; set; }
    }
    
    class CodeGenerator
    {

        public string ProduceCTString(AbstractClasses.Design design, string visualizerType)
        {
            string jsonResult = "";
            Dictionary<string, ComponentModel> componentDictionary = new Dictionary<string,ComponentModel>();

            bool edaFound = false;  // Need to check that at least one EDA model is found. A model of all placeholders is pointless..
            foreach ( AbstractClasses.Component component in design.AllComponents
                                                                   .Where(x => String.Compare(x.classification, "ara_template") != 0
                                                                            || String.Compare(x.classification, "template.ara_module_template") != 0))
            {
                if (componentDictionary.ContainsKey(component.name))
                {
                    continue;  // CAD component instance entries are identical.
                }

                ComponentModel compModel = new ComponentModel();

                // Not all components will have CAD models. Those without CAD models will have 
                //      placeholders at the component's EAGLE layout location. The Python script handles the placeholder.
                AbstractClasses.STEPModel stepModel = component.cadModels.OfType<AbstractClasses.STEPModel>().FirstOrDefault();
                AbstractClasses.STLModel stlModel = component.cadModels.OfType<AbstractClasses.STLModel>().FirstOrDefault();

                // If component has STEP model associated with EDAModel and visualizerType allows, give preference to STEP.
                var dostep = new List<string> { "step", "stp", "mix" };
                var dostl = new List<string> { "stl", "mix" };

                if (dostep.Any(s => visualizerType.Contains(s)) && stepModel != null)
                {
                    compModel.cadpath = stepModel.path;
                    compModel.translation = stepModel.translationVector;
                    compModel.rotation = stepModel.rotationVector;
                    compModel.scale = stepModel.scalingVector;
                    edaFound = true;
                }
                else if (dostl.Any(s => visualizerType.Contains(s)) && stlModel != null)
                {
                    compModel.cadpath = stlModel.path;
                    compModel.translation = stlModel.translationVector;
                    compModel.rotation = stlModel.rotationVector;
                    compModel.scale = stlModel.scalingVector;
                    edaFound = true;
                }
                componentDictionary.Add(component.name, compModel);    
            }

            if (!edaFound)
                // Has not yet been serialized, still ""
                return jsonResult;

            jsonResult = JsonConvert.SerializeObject(componentDictionary, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return jsonResult;
        }

        public string ProduceAraTemplateJson(AbstractClasses.Design design)
        {
            string jsonResult = "";
            Dictionary<string, string> templateCadMap = new Dictionary<string,string>();
            AbstractClasses.AraTemplateComponent template = design.AllComponents.OfType<AbstractClasses.AraTemplateComponent>().First();

            templateCadMap.Add("template_cadpath", template.cadModels.First().path);

            // Get list of parameter names at the test bench level
            List<string> tb_param_names = new List<string>();
            foreach (var tbp in design.tb_parameters)
            {
                tb_param_names.Add(tbp.Name);
            }

            // Loop through template parameter names to see if any match that of testbench level parameters
            foreach (string param in template.parameters)
            {
                if (tb_param_names.Contains(param))
                {
                    // Override with CAD file whose path is the value.
                    templateCadMap.Add(param, design.tb_parameters.Find(x => x.Name == param).Attributes.Value);
                }
                else if (String.Compare(param, "PCB") == 0)
                {
                    templateCadMap.Add(param, design.Name + ".FCStd");
                }
                else
                {
                    templateCadMap.Add(param, "stock");
                }
            }

            if (templateCadMap.Count() > 0)
            {
                jsonResult = JsonConvert.SerializeObject(templateCadMap, Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }

            return jsonResult;
        }
    }
}
