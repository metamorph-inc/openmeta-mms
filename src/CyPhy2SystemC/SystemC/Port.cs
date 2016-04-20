using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace CyPhy2SystemC.SystemC
{
    public class Port : ModelBase<CyPhy.SystemCPort>
    {
        public Port(CyPhy.SystemCPort impl)
            : base(impl)
        {
            SrcConnections = new List<Connection>();
            DstConnections = new List<Connection>();
            IsOutput = false;
        }

        public Component Parent { get; set; }
        public List<Connection> SrcConnections { get; set; }
        public List<Connection> DstConnections { get; set; }
        public string DataType { get; set; }
        public bool IsOutput { get; set; }

        public void accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}
