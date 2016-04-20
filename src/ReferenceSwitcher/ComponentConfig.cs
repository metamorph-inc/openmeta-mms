using System;
using System.Runtime.InteropServices;
using GME.Util;
using GME.MGA;

namespace GME.CSharp
{
    
    abstract class ComponentConfig
    {
        // Set paradigm name. Provide * if you want to register it for all paradigms.
		public const string paradigmName = "*";
		
		// Set the human readable name of the interpreter. You can use white space characters.
        public const string componentName = "ReferenceSwitcher";
        
		// Specify an icon path
		public const string iconName = "ReferenceSwitcher.ico";
        
		// If null, updated with the assembly path + the iconName dynamically on registration
        public static string iconPath = null; 
        
		// Uncomment the flag if your component is paradigm independent.
		public static componenttype_enum componentType = componenttype_enum.COMPONENTTYPE_INTERPRETER;
				
        public const regaccessmode_enum registrationMode = regaccessmode_enum.REGACCESS_SYSTEM;
        public const string progID = "MGA.Interpreter.ReferenceSwitcher";
        public const string guid = "0CB32575-EB7C-4363-A9E6-4F6F0EC6EC84";
    }
}
