using System;
using System.Runtime.InteropServices;
using GME.Util;
using GME.MGA;

namespace GME.CSharp
{

    abstract class ComponentConfig
    {
        // Set paradigm name. Provide * if you want to register it for all paradigms.
		public const string paradigmName = "CyPhyML";

		// Set the human readable name of the interpreter. You can use white space characters.
        public const string componentName = "CyPhy2Simulink";

		// Specify an icon path
		public const string iconName = "CyPhy2Simulink.ico";

		public const string tooltip = "CyPhy2Simulink";

		// If null, updated with the assembly path + the iconName dynamically on registration
        public static string iconPath = null;

		// Uncomment the flag if your component is paradigm independent.
		public static componenttype_enum componentType = componenttype_enum.COMPONENTTYPE_INTERPRETER;

        public const regaccessmode_enum registrationMode = regaccessmode_enum.REGACCESS_SYSTEM;
        public const string progID = "MGA.Interpreter.CyPhy2Simulink";
        public const string guid = "AF0B9735-FF04-4589-8C56-DC997FB3FA97";
    }
}
