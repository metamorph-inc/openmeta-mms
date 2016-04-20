using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2CADPCB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime.InteropServices;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    [ProgId("ISIS.META.CyPhy2CADPCB_Settings")]
    [Guid("54448B4E-F898-41CA-B706-E1E6B27AA7E8")]

    public class CyPhy2CADPCB_Settings : CyPhyGUIs.IInterpreterConfiguration
    {
        public const string ConfigFilename = "CyPhy2CADPCB_config.xml";

        public CyPhy2CADPCB_Settings()
        {
            this.Verbose = false;
            this.layoutFile = "layout.json";
            this.useSavedLayout = null;
            this.runLayout = null;
            this.launchVisualizer = null;
            this.visualizerType = null;
            this.layoutFilePath = null;

        }

        public bool Verbose { get; set; }
        public string layoutFile { get; set; }

        [CyPhyGUIs.WorkflowConfigItem]
        public string useSavedLayout { get; set; }

        [CyPhyGUIs.WorkflowConfigItem]
        public string visualizerType { get; set; }

        [CyPhyGUIs.WorkflowConfigItem]
        public string layoutFilePath { get; set; }

        [CyPhyGUIs.WorkflowConfigItem]
        public string runLayout { get; set; }

        [CyPhyGUIs.WorkflowConfigItem]
        public string launchVisualizer { get; set; }

    }
}

