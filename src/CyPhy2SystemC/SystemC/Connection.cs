using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2SystemC.SystemC
{
    public class Connection : IComparable<Connection>
    {
        private static Dictionary<string, Connection> connections;

        static Connection()
        {
            connections = new Dictionary<string, Connection>();
        }

        public Connection(Port srcPort, Port dstPort)
        {
            SrcPort = srcPort;
            DstPort = dstPort;
            Name = srcPort.Name;
            DataType = srcPort.DataType;
        }
        public Port SrcPort { get; set; }
        public Port DstPort { get; set; }
        public string DataType { get; set; }

        private string _name = null;

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                // Find a unique name by appending a sequence to the basename
                string baseName = value.Replace(' ', '_');
                string name = baseName;
                int seq = 2;
                while (connections.ContainsKey(name))
                {
                    name = baseName + (seq++);
                }
                this._name = name;
                connections.Add(name, this);
            }
        }

        public int CompareTo(Connection obj)
        {
            string me = string.Format("{0}-{2}->{1}", SrcPort.Name, DstPort.Name, obj.DataType);
            string other = string.Format("{0}-{2}->{1}", obj.SrcPort.Name, obj.DstPort.Name, obj.DataType);
            return me.CompareTo(other);
        }
       
    }

}
