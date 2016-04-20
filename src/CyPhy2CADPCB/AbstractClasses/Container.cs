using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2CADPCB.AbstractClasses
{
    class Container
    {
        public Container()
        {
            components = new List<Component>();
            containers = new List<Container>();
        }

        public List<Component> components { get; set; }
        public List<Container> containers { get; set; }
        public String name { get; set; }

        public IEnumerable<Component> AllComponents
        {
            get
            {
                var rtn = new List<Component>();

                rtn.AddRange(containers.SelectMany(c => c.AllComponents));
                rtn.AddRange(components);
                
                return rtn;
            }
        }
    }
}
