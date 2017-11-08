using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CyPhy2Schematic.Spice
{
    class Circuit
    {
        public string name { get; set; }
        public List<Node> nodes { get; set; }       // list of devices in the circuit
        public Dictionary<string, string> subcircuits { get; set; }
        public string analysis { get; set; }

        public Circuit()
        {
            nodes = new List<Node>();
            subcircuits = new Dictionary<string, string>();
        }

        public void Serialize(string spiceFile)
        {
            StreamWriter writer = new StreamWriter(spiceFile);
            writer.WriteLine("CyPhy2Schematic Circuit {0}", name);
            writer.WriteLine("* Network Topology");
            foreach (var node in nodes)
            {
                node.Serialize(writer);
            }
            writer.WriteLine();
            writer.WriteLine("* Sub Circuits");
            foreach (var sub in subcircuits)
            {
                writer.Write(sub.Value);
                writer.WriteLine();
            }
            writer.WriteLine();
            if (analysis != null)
            {
                writer.WriteLine(analysis);
            }
            writer.WriteLine();
            writer.WriteLine(".end");
            writer.Close();


            /// TBD Testbench aspects of circuit???
        }
    }

    class Node
    {
        public string name { get; set; }                // device instance name
        public char type { get; set; }                  // circuit device type: R, C, X, ...
        public string classType { get; set; }           // subcircuit class type
        public SortedDictionary<int, string> nets { get; set; } // nets sorted by an order parameter
        public SortedDictionary<string, string> parameters { get; set; }    // device parameters

        public Node()
        {
            nets = new SortedDictionary<int, string>();
            parameters = new SortedDictionary<string, string>();
        }

        public void Serialize(StreamWriter writer)
        {
            writer.Write("{0}{1} ", type, name);
            foreach (var net in nets)                   // ports are index-ordered
                writer.Write("{0} ", net.Value);
            if (classType != null) 
                writer.Write("{0} ", classType);            // sub-circuit name or model name
            foreach (var param in parameters)           // params are name-ordered
            {
                // value parameters are special - don't require a name= prefix
                if (param.Key.Contains("value"))
                    writer.Write("{0} ", param.Value);
                else
                    writer.Write("{0}={1} ", param.Key, param.Value);
            }
            writer.WriteLine();
        }
    }

}
