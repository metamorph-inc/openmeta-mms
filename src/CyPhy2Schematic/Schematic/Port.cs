using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace CyPhy2Schematic.Schematic
{
    public class Port : ModelBase<Tonka.SchematicModelPort>
    {
        public Port(Tonka.SchematicModelPort impl)
            : base(impl)
        {
            SrcConnections = new List<Connection>();
            DstConnections = new List<Connection>();
            _connectedPorts = new Dictionary<string, ISIS.GME.Common.Interfaces.FCO>();
        }

        public DesignEntity Parent { get; set; }
        public Component ComponentParent {
            get
            {
                return (Component)this.Parent;
            }
        }
        public List<Connection> SrcConnections { get; set; }
        public List<Connection> DstConnections { get; set; }
        private Dictionary<string, ISIS.GME.Common.Interfaces.FCO> _connectedPorts;
        public Dictionary<string, ISIS.GME.Common.Interfaces.FCO> connectedPorts
        {
            get
            {
                return _connectedPorts;
            }
            set
            {
                if (_connectedPorts != null && _connectedPorts.Count > 0)
                {
                    throw new ApplicationException();
                }
                _connectedPorts = value;
            }
        }

        public void accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}
