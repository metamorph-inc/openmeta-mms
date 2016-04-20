using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CyPhy2SystemC
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    [ProgId("ISIS.META.CyPhy2SystemC_Settings")]
    [Guid("7FD21679-8092-4D8F-BA74-3D3FFCD6966E")]
    public class CyPhy2SystemC_Settings : CyPhyGUIs.IInterpreterConfiguration
    {
        public const string ConfigFilename = "CyPhy2SystemC_Config.xml";
        
        public List<string> IncludeDirectoryPath { get; set; }
        public List<string> NonCheckedIncludeDirPaths { get; set; }
        public bool Verbose { get; set; }

        public CyPhy2SystemC_Settings()
        {
            this.NonCheckedIncludeDirPaths = new List<string>();
            this.IncludeDirectoryPath = new List<string>();
            this.Verbose = false;
        }
    }
}
