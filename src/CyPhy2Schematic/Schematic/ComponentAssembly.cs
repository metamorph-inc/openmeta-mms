using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhyComponentFidelitySelector;
using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using GME.MGA;

namespace CyPhy2Schematic.Schematic
{
    public class ComponentAssembly : ModelBase<Tonka.ComponentAssembly>, DesignEntity
    {
        public ComponentAssembly(Tonka.ComponentAssembly impl)
            : base(impl)
        {
            ComponentAssemblyInstances = new SortedSet<ComponentAssembly>();
            ComponentInstances = new SortedSet<Component>();
            Parameters = new SortedSet<Parameter>();
            Ports = new SortedSet<Port>();
        }
        public SortedSet<ComponentAssembly> ComponentAssemblyInstances { get; set; }
        public SortedSet<Component> ComponentInstances { get; set; }
        public SortedSet<Parameter> Parameters { get; set; }
        public SortedSet<Port> Ports { get; set; }
        public ComponentAssembly Parent { get; set; }
        public ComponentAssembly SystemUnderTest { get; set; }
        public ISet<IMgaObject> selectedSpiceModels;

        public int CanvasXMax { get; set; }
        public int CanvasYMax { get; set; }

        public void accept(Visitor visitor)
        {
            visitor.visit(this);
            foreach (var componentassembly_obj in this.ComponentAssemblyInstances)
            {
                componentassembly_obj.accept(visitor);
            }
            foreach (var component_obj in this.ComponentInstances)
            {
                component_obj.accept(visitor);
            }
            foreach (var port_obj in this.Ports)
            {
                port_obj.accept(visitor);
            }
            visitor.upVisit(this);
        }

        ISIS.GME.Common.Interfaces.FCO DesignEntity.Impl
        {
            get
            {
                return this.Impl;
            }
        }

        public string SpiceLib
        {
            get;
            set;
        }

        public int CompareTo(DesignEntity other)
        {
            int name = this.Name.CompareTo(other.Name);
            if (name == 0)
            {
                return this.Impl.ID.CompareTo(other.Impl.ID);
            }
            return name;
        }

    }


}
