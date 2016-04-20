using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace CyPhy2SystemC.SystemC
{
    public class Component : ModelBase<CyPhy.ComponentType>
    {
        public Component(CyPhy.ComponentType impl)
            : base(impl)
        {
            Parameters = new SortedSet<Parameter>();
            Ports = new SortedSet<Port>();
            Sources = new SortedSet<SourceFile>();
            HasSystemCModel = false;
        }
        public SortedSet<Parameter> Parameters { get; set; }
        public SortedSet<Port> Ports { get; set; }
        public ComponentAssembly Parent { get; set; }
        public SortedSet<SourceFile> Sources { get; set; }
        public bool HasSystemCModel { get; set; }
        public string ComponentDirectory { get; set; }

        public virtual void accept(Visitor visitor)
        {
            visitor.visit(this);
            foreach (var port_obj in Ports)
            {
                port_obj.accept(visitor);
            }
        }
    }
}
