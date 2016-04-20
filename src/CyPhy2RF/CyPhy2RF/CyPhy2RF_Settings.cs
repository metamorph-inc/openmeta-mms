using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CyPhy2RF
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    [ProgId("ISIS.META.CyPhy2RF_Settings")]
    [Guid("C51EAB54-7164-4F3C-8A7C-8D2900A9A7EE")]
    public class CyPhy2RF_Settings : CyPhyGUIs.IInterpreterConfiguration
    {
        public const string ConfigFilename = "CyPhy2RF_Config.xml";
        
        public bool Verbose { get; set; }

        [CyPhyGUIs.WorkflowConfigItem]
        public string doDirectivity { get; set; }

        [CyPhyGUIs.WorkflowConfigItem]
        public string doSAR { get; set; }

        public CyPhy2RF_Settings()
        {
            this.Verbose = false;
            this.doDirectivity = null;
            this.doSAR = null;
        }
    }
}
