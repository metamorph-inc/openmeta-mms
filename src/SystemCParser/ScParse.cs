//--------------------------------------------------------------------
//
//  ScParse.cs
//
//  This file defines the ScParse class, to help the SystemC CAT
//  create ports in a SystemC model.
//
//  See also: MOT-419 "CAT module for SystemC"
//
//  Henry Forson, 11/11/2014
//
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemCParser;

namespace SystemCParser
{
    public struct pinData_s
    {
        public string name;
        public string direction;
        public string type;
        public int dimension;

        public pinData_s(string name, string direction, string type, int dimension)
        {
            this.name = name;
            this.direction = direction;
            this.type = type;
            this.dimension = dimension;
        }

    };


    public class ScParse
    {

        enum stateMachine_e
        {
            findModule,
            findPin
        };

        /////////////////////////////////////////////////////
        //
        //             Public data types
        //
        /////////////////////////////////////////////////////

        public string scModuleName;
        public List<pinData_s> pinList;

        /////////////////////////////////////////////////////
        //
        //             Public methods
        //
        /////////////////////////////////////////////////////

        /// <summary>
        /// Parses the scInput string to get the module's name and a pin-data list.
        /// </summary>
        /// <seealso cref="https://www.doulos.com/knowhow/systemc/utilities/naming_ports_and_signals/"/>
        /// <param name="scInput">Text from a pre-processed SystemC-component header file.</param>
        public ScParse(string scInput)
        {
            scModuleName = "";
            pinList = new List<pinData_s>();

            stateMachine_e sm = stateMachine_e.findModule;

            // perl: /^\s*SC_MODULE\s*\(\s*(\w+)\s*\)\s*\{.*?\}\s*;/ms ) 
            string sc_module_1 = @"^\s*SC_MODULE\s*\(\s*(?<module_name>\w+)\s*\)";

            // perl: /^\s*struct\s*(\w+)\s*:[^{]*sc_module[^{]*\{.*?\}\s*;/
            string sc_module_2 = @"^\s*struct\s*(?<module_name>\w+)\s*:[^{]*sc_module[^\w]";

            // perl: /^\s*class\s*(\w+)\s*:[^{]*public\s+sc_module[^{]*\{.*?\}\s*;/
            string sc_module_3 = @"^\s*class\s*(?<module_name>\w+)\s*:[^{]*public\s+sc_module[^\w]";

            // List of supported SystemC port tags:
            string[] pinTags = { "sc_in", "sc_out", "sc_inout" };

            // perl: /^\s*($sc_port_types)\s*(<.+>)?\s+([^;]+);/
            string formatString = @"^\s*(?<port_direction>{0})\s*<(?<data_type>\w+)(<(?<dimension>\d+)>)?>\s+(?<pin_name>\w+).*;";
            string pinPattern = string.Format( formatString, string.Join( "|", pinTags ) );

            string[] lines = scInput.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (stateMachine_e.findModule == sm)
                {
                    Match match = Regex.Match(line, sc_module_1, RegexOptions.None);
                    if (match.Success)
                    {
                        scModuleName = match.Groups["module_name"].Value;
                        sm = stateMachine_e.findPin;
                    }
                    else
                    {
                        match = Regex.Match(line, sc_module_2, RegexOptions.None);
                        if (match.Success)
                        {
                            scModuleName = match.Groups["module_name"].Value;
                            sm = stateMachine_e.findPin;
                        }
                        else
                        {
                            match = Regex.Match(line, sc_module_3, RegexOptions.None);
                            if (match.Success)
                            {
                                scModuleName = match.Groups["module_name"].Value;
                                sm = stateMachine_e.findPin;
                            }
                        }
                    }
                }
                else if (stateMachine_e.findPin == sm)
                {
                    // Find pins
                    Match match = Regex.Match(line, pinPattern, RegexOptions.None);
                    if (match.Success)
                    {
                        pinData_s pin;
                        pin.dimension = 1;
                        pin.name = match.Groups["pin_name"].Value;
                        pin.direction = match.Groups["port_direction"].Value;
                        pin.type = match.Groups["data_type"].Value;
                        if (match.Groups["dimension"].Success)
                        {
                            pin.dimension = Convert.ToInt32(match.Groups["dimension"].Value);
                        }
                        pinList.Add(pin);
                    }

                }
            }

            // Sort the pin list by their names.
            pinList.Sort((s1, s2) => s1.name.CompareTo(s2.name));

        }
    }
}
