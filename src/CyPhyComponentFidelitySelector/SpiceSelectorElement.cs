using GME.MGA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;

namespace CyPhyComponentFidelitySelector
{
    public class FidelitySelectionRules
    {
        public class SelectionRule
        {
            public string xpath = "";
            public bool lowest;
        }
        public List<SelectionRule> rules = new List<SelectionRule>();

        public static FidelitySelectionRules DeserializeSpiceFidelitySelection(IMgaFCO currentobj)
        {
            FidelitySelectionRules xpaths = null;
            var settings = currentobj.RegistryValue["SpiceFidelitySettings"];
            if (settings == null)
            {
                return null;
            }
            try
            {
                xpaths = JsonConvert.DeserializeObject<FidelitySelectionRules>(settings);
            }
            catch (JsonException)
            {
            }

            return xpaths;
        }

        public static void SerializeInRegistry(MgaFCO currentobj, FidelitySelectionRules rules)
        {
            string savedJson = JsonConvert.SerializeObject(rules, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings() { });
            currentobj.RegistryValue["SpiceFidelitySettings"] = savedJson;
        }

        public static XElement CreateForAssembly(CyPhy.DesignElement system, Dictionary<XElement, CyPhy.SPICEModel> map=null)
        {
            Action<XElement, IEnumerable<CyPhy.SPICEModel>> addSpiceModels = (parentElement, models) =>
            {
                foreach (var model in models.OrderBy(sm => sm.Name))
                {
                    var spiceElement = new XElement("SpiceModel");
                    parentElement.Add(spiceElement);
                    spiceElement.SetAttributeValue("Name", model.Name);
                    // spiceElement.SetAttributeValue("Fidelity"
                    if (map != null)
                    {
                        map.Add(spiceElement, model);
                    }
                }
            };

            Queue<Tuple<XElement, CyPhy.DesignEntity>> containers = new Queue<Tuple<XElement, CyPhy.DesignEntity>>();
            containers.Enqueue(new Tuple<XElement, CyPhy.DesignEntity>(null, system));
            XElement root = null;

            while (containers.Count > 0)
            {
                var tuple = containers.Dequeue();
                var parent = tuple.Item1;
                var container = tuple.Item2;
                XElement element = new XElement(container.Impl.MetaBase.Name);
                element.SetAttributeValue("Name", container.Name);
                if (parent == null)
                {
                    root = element;
                }
                else
                {
                    parent.Add(element);
                }
                IEnumerable<CyPhy.Component> childComponents = new CyPhy.Component[] { };
                IEnumerable<CyPhy.ComponentRef> childComponentRefs = new CyPhy.ComponentRef[] { };
                if (container is CyPhy.DesignContainer)
                {
                    foreach (var child in (container as CyPhy.DesignContainer).Children.DesignContainerCollection)
                    {
                        containers.Enqueue(new Tuple<XElement, CyPhy.DesignEntity>(element, child));
                    }
                    childComponents = (container as CyPhy.DesignContainer).Children.ComponentCollection;
                    childComponentRefs = (container as CyPhy.DesignContainer).Children.ComponentRefCollection;
                    // TODO addSpiceModels(element, (container as CyPhy.DesignContainer).Children.SPICEModelCollection);
                }
                else if (container is CyPhy.ComponentAssembly)
                {
                    foreach (var child in (container as CyPhy.ComponentAssembly).Children.ComponentAssemblyCollection)
                    {
                        containers.Enqueue(new Tuple<XElement, CyPhy.DesignEntity>(element, child));
                    }
                    childComponents = (container as CyPhy.ComponentAssembly).Children.ComponentCollection;
                    childComponentRefs = (container as CyPhy.ComponentAssembly).Children.ComponentRefCollection;
                    element.SetAttributeValue("Classifications", (container as CyPhy.ComponentAssembly).Attributes.Classifications.Replace("\n", ";"));
                    addSpiceModels(element, (container as CyPhy.ComponentAssembly).Children.SPICEModelCollection);
                }
                foreach (var childComponent in childComponents)
                {
                    var childElement = new XElement("Component");
                    childElement.SetAttributeValue("Name", childComponent.Name);
                    childElement.SetAttributeValue("Classifications", childComponent.Attributes.Classifications.Replace("\n", ";"));
                    element.Add(childElement);
                    addSpiceModels(childElement, childComponent.Children.SPICEModelCollection);
                }
                foreach (var childComponentRef in childComponentRefs)
                {
                    if (childComponentRef.AllReferred.Impl.MetaBase.Name == "ComponentAssembly")
                    {
                        containers.Enqueue(new Tuple<XElement, CyPhy.DesignEntity>(element, childComponentRef.Referred.ComponentAssembly));
                        continue;
                    }
                    var childElement = new XElement("Component");
                    childElement.SetAttributeValue("Name", childComponentRef.Name);
                    childElement.SetAttributeValue("Classifications", childComponentRef.Referred.Component.Attributes.Classifications.Replace("\n", ";"));
                    element.Add(childElement);
                    addSpiceModels(childElement, childComponentRef.Referred.Component.Children.SPICEModelCollection);
                }
                // children.Sort((a, b) => a.name.CompareTo(b.name));
            }

            return root;
        }

        public static HashSet<XElement> SelectElements(XElement e, FidelitySelectionRules rules)
        {
            HashSet<XElement> results = new HashSet<XElement>();
            HashSet<XElement> parentsOfResults = new HashSet<XElement>();
            foreach (var xpath in rules.rules.Concat(new FidelitySelectionRules.SelectionRule[] { new FidelitySelectionRules.SelectionRule() { xpath = "//Component/SpiceModel" } }).Where(xp => string.IsNullOrWhiteSpace(xp.xpath) == false))
            {
                foreach (var element in e.XPathSelectElements(xpath.xpath).OrderBy(el => (xpath.lowest ? -1 : 1) * el.Ancestors().Count()))
                {
                    if (element is XElement && ((XElement)element).Name == "SpiceModel")
                    {
                        // FIXME perf is probably not good here
                        if (element.Ancestors().Any(parentsOfResults.Contains) == false && element.Parent.DescendantNodes().Any(parentsOfResults.Contains) == false)
                        {
                            results.Add(element);
                            parentsOfResults.Add(element.Parent);
                        }
                    }
                }
            }

            return results;
        }
    }
}
