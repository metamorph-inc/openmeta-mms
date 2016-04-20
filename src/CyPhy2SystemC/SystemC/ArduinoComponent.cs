using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using System.IO;

namespace CyPhy2SystemC.SystemC
{
    class ArduinoComponent : Component
    {
        public ArduinoComponent(CyPhy.ComponentType impl)
            : base(impl)
        {

        }

        public string FirmwarePath { get; set; }

        public override string Name
        {
            get {
                return this.Namespace;
            }
        }

        public string ArduinoModule
        {
            get
            {
                return base.Name;
            }
        }

        public string Namespace
        {
            get
            {
                string baseName = Path.GetFileNameWithoutExtension(FirmwarePath);
                if (String.IsNullOrWhiteSpace(baseName)) 
                {
                    return "Unknown";
                }
                else 
                {
                    return baseName;
                }
            }
        }

        public string SetupFunction
        {
            get
            {
                return Namespace + "::setup";
            }
        }

        public string LoopFunction
        {
            get
            {
                return Namespace + "::loop";
            }
        }
    }
}
