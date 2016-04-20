using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2CADPCB.AbstractClasses
{
    class Component
    {
        public Component()
        {
            cadModels = new List<CADModel>();
        }

        public List<CADModel> cadModels { get; set; }
        public String name { get; set; }
        public String classification { get; set; }
    }
}
