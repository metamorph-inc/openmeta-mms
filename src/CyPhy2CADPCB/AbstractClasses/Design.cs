using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;

namespace CyPhy2CADPCB.AbstractClasses
{
    class Design
    {

        public String Name { get; set; }
        public String ID { get; set; }
        public Container TopContainer { get; set; }
        public List<CyPhy.Parameter> tb_parameters { get; set; }

        public IEnumerable<Component> AllComponents
        {
            get
            {
                if (TopContainer != null)
                {
                    return TopContainer.AllComponents;
                }
                else
                {
                    return new List<Component>();
                }
            }
        }
    }
}
