using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace PETBrowser
{
    public class AnalysisTools
    {
        private const string PetAnalysisToolsKeyName = @"SOFTWARE\META\PETBrowser\PETTools";

        public IList<AnalysisTool> PetAnalysisToolList { get; set; }

        public AnalysisTools()
        {
            PetAnalysisToolList = new List<AnalysisTool>();
            LoadAnalysisToolsFromRegistry();
        }

        private void LoadAnalysisToolsFromRegistry()
        {
            using (var petToolsKey = Registry.LocalMachine.OpenSubKey(PetAnalysisToolsKeyName))
            {
                if (petToolsKey != null) //Returns null if key doesn't exist
                {
                    foreach (var toolKeyName in petToolsKey.GetSubKeyNames())
                    {
                        using (var toolKey = petToolsKey.OpenSubKey(toolKeyName))
                        {
                            if (toolKey != null)
                            {
                                try
                                {
                                    var tool = new AnalysisTool(toolKey);
                                    PetAnalysisToolList.Add(tool);
                                }
                                catch (InvalidAnalysisToolException e)
                                {
                                    Trace.TraceWarning(e.Message);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //TODO: Create this key (with defaults) if it doesn't already exist?
                }
            }

            using (var petToolsKey = Registry.CurrentUser.OpenSubKey(PetAnalysisToolsKeyName))
            {
                if (petToolsKey != null) //Returns null if key doesn't exist
                {
                    foreach (var toolKeyName in petToolsKey.GetSubKeyNames())
                    {
                        using (var toolKey = petToolsKey.OpenSubKey(toolKeyName))
                        {
                            if (toolKey != null)
                            {
                                try
                                {
                                    var tool = new AnalysisTool(toolKey);
                                    PetAnalysisToolList.Add(tool);
                                }
                                catch (InvalidAnalysisToolException e)
                                {
                                    Trace.TraceWarning(e.Message);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //TODO: Create this key (with defaults) if it doesn't already exist?
                }
            }
        }
    }

    public class AnalysisTool
    {
        public string InternalName { get; private set; }
        public string DisplayName { get; private set; }
        public string ActionName { get; private set; }
        public string ExecutableFilePath { get; private set; }
        public string ProcessArguments { get; private set; }
        public string WorkingDirectory { get; private set; }
        public bool ShowConsoleWindow { get; private set; }

        public AnalysisTool(RegistryKey toolKey)
        {
            var keyNameComponents = toolKey.Name.Split('\\');
            InternalName = keyNameComponents[keyNameComponents.Length - 1];

            if (toolKey.GetValueKind("") == RegistryValueKind.String)
            {
                DisplayName = (string) toolKey.GetValue("");
            }
            else
            {
                throw new InvalidAnalysisToolException(string.Format("Tool {0} is missing the required DisplayName key", toolKey.Name));
            }

            if (toolKey.GetValueKind("ActionName") == RegistryValueKind.String)
            {
                ActionName = (string)toolKey.GetValue("ActionName");
            }
            else
            {
                throw new InvalidAnalysisToolException(string.Format("Tool {0} is missing the required ActionName key", toolKey.Name));
            }

            if (toolKey.GetValueKind("ExecutableFilePath") == RegistryValueKind.String)
            {
                ExecutableFilePath = (string)toolKey.GetValue("ExecutableFilePath");
            }
            else
            {
                throw new InvalidAnalysisToolException(string.Format("Tool {0} is missing the required ExecutableFilePath key", toolKey.Name));
            }

            if (toolKey.GetValueKind("ProcessArguments") == RegistryValueKind.String)
            {
                ProcessArguments = (string)toolKey.GetValue("ProcessArguments");
            }
            else
            {
                ProcessArguments = "";
            }

            if (toolKey.GetValueKind("WorkingDirectory") == RegistryValueKind.String)
            {
                WorkingDirectory = (string)toolKey.GetValue("WorkingDirectory");
            }
            else
            {
                WorkingDirectory = ".";
            }

            if (toolKey.GetValueKind("ShowConsoleWindow") == RegistryValueKind.DWord)
            {
                var value = (int)toolKey.GetValue("ShowConsoleWindow");
                if (value == 0)
                {
                    ShowConsoleWindow = false;
                }
                else
                {
                    ShowConsoleWindow = true;
                }
            }
            else
            {
                ShowConsoleWindow = false;
            }
        }
    }

    public class InvalidAnalysisToolException : Exception
    {
        public InvalidAnalysisToolException(string message) : base(message) { }
    }
}
