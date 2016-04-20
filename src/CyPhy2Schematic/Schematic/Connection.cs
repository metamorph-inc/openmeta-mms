using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2Schematic.Schematic
{
    public class Connection : IComparable<Connection>
    {
        public Connection(Port srcPort, Port dstPort)
        {
            SrcPort = srcPort;
            DstPort = dstPort;
        }
        public Port SrcPort { get; set; }
        public Port DstPort { get; set; }

        public int CompareTo(Connection obj)
        {
            string me = string.Format("{0}->{1}", SrcPort.Name, DstPort.Name);
            string other = string.Format("{0}->{1}", obj.SrcPort.Name, obj.DstPort.Name);
            return me.CompareTo(other);
        }
       
    }

}
