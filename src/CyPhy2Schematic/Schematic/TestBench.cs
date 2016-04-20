using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace CyPhy2Schematic.Schematic
{
    public class TestBench : ModelBase<Tonka.TestBench>
    {
        public TestBench(Tonka.TestBench impl)
            : base(impl)
        {
            ComponentAssemblies = new SortedSet<ComponentAssembly>();
            TestComponents = new SortedSet<Component>();
            Parameters = new SortedSet<Parameter>();
            SolverParameters = new Dictionary<string, string>();
        }

        public SortedSet<ComponentAssembly> ComponentAssemblies { get; set; }
        public SortedSet<Component> TestComponents { get; set; }
        public SortedSet<Parameter> Parameters { get; set; }
        public Dictionary<string, string> SolverParameters { get; set; }
        public int CanvasXMax { get; set; }
        public int CanvasYMax { get; set; }

        public void accept(Visitor visitor)
        {
            visitor.visit(this);
            foreach (var testcomponent_obj in TestComponents)
            {
                testcomponent_obj.accept(visitor);
            }
            foreach (var componentassembly_obj in ComponentAssemblies)
            {
                componentassembly_obj.accept(visitor);
            }
        }

    }

}
