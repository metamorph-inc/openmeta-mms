using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace CyPhy2SystemC.SystemC
{
    public class TestBench : ModelBase<CyPhy.TestBench>
    {
        public TestBench(CyPhy.TestBench impl)
            : base(impl)
        {
            ComponentAssemblies = new SortedSet<ComponentAssembly>();
            TestComponents = new SortedSet<Component>();
            TestParameters = new Dictionary<string, CyPhy.Parameter>(); // MOT-516
        }

        public SortedSet<ComponentAssembly> ComponentAssemblies { get; set; }
        public SortedSet<Component> TestComponents { get; set; }
        public Dictionary<string, CyPhy.Parameter> TestParameters { get; set; } // MOT-516

        public void accept(Visitor visitor)
        {
            visitor.visit(this);
            foreach (var componentassembly_obj in ComponentAssemblies)
            {
                componentassembly_obj.accept(visitor);
            }
            foreach (var testcomponent_obj in TestComponents)
            {
                testcomponent_obj.accept(visitor);
            }
        }
    }
}
