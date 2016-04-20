using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Newtonsoft.Json;


namespace CyPhy2Schematic.Spice
{
    public class SignalBase
    {
        public string name;
        public string gmeid;
    }

    public class Signal : SignalBase
    {
        public string net;
        public int    spicePort;
    }

    public class SignalContainer : SignalBase
    {
        public List<SignalBase> signals;
        public Dictionary<CyPhy2SchematicInterpreter.IDs, string> objectToNetId;

        public SignalContainer()
        {
            signals = new List<SignalBase>();
        }

        public void Serialize(string siginfoFile)
        {
            StreamWriter writer = new StreamWriter(siginfoFile);
            string sjson = JsonConvert.SerializeObject(this, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            writer.Write(sjson);
            writer.Close();
        }
    }
}
