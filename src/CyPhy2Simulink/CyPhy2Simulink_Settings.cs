using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CyPhy2Simulink
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    [ProgId("ISIS.META.CyPhy2Simulink_Settings")]
    [Guid("FEDF3A05-DEAC-48DA-A7D6-C992BF957E5A")]
    public class CyPhy2Simulink_Settings : CyPhyGUIs.IInterpreterConfiguration
    {
        public const string ConfigFilename = "CyPhy2Simulink_Config.xml";

        public bool Verbose { get; set; }

        public CyPhy2Simulink_Settings()
        {
            this.Verbose = false;
        }
    }
}
