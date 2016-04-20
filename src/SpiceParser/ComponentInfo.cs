using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiceLib
{
    public class ComponentInfo
    {
        /// <summary>
        /// The name of the subcircuit.
        /// </summary>
        public string name;
        /// <summary>
        /// The subcircuit's (parameter name, default value) pairs.
        /// </summary>
        public Dictionary<string, string> parameters;
        /// <summary>
        /// Ordered list of the pin names used by the subcircuit.
        /// </summary>
        /// <remarks>The first list item is the name of pin 1, the second the name of pin 2, and so on.</remarks>
        public List<string> pins;
        /// <summary>
        /// A SPICE element type (="X" for subcircuit, "R" for resistor, etc.)
        /// </summary>
        public char elementType;

        /// <summary>
        /// Packages SPICE info about a component for GME.
        /// </summary>
        public ComponentInfo()
        {
            this.name = "";
            this.pins = new List<string>();
            this.parameters = new Dictionary<string, string>();
            this.elementType = 'X';
        }

        /// <summary>
        /// Converts structure to a multi-line printable string.
        /// </summary>
        override public string ToString()
        {
            string rVal = "No component info found.";
            if (this.name.Length > 0)
            {
                string pinListString = "None found.";
                if (this.pins.Count > 0)
                {
                    pinListString = String.Join(", ", this.pins.Select(x => x.ToString()).ToArray());
                }
                string paramListString = "None found.";
                if (this.parameters.Count > 0)
                {
                    paramListString = String.Join(", ", this.parameters.Select(x => "(" + x.Key + "=" + x.Value + ")").ToArray());
                }
                rVal= String.Format( "component name '{3}:{0}':\n     Pins: {1}\n     Params: {2}",
                    this.name, pinListString, paramListString, this.elementType);
            }
            return rVal;
        }

        /// <summary>
        /// Equals(object)
        /// overrides Equals comparing ComponentInfo with an object
        /// </summary>
        /// <param name="obj"> The object to compare.</param>
        /// <returns>true if the values are equal.</returns>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to ComponentInfo return false.
            ComponentInfo p = obj as ComponentInfo;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return Equals( p );
        }

        /// <summary>
        /// Equals( ComponentInfo )
        /// overrides Equals with comparing ComponentInfo with another ComponentInfo
        /// </summary>
        /// <param name="ci"> The ComponentInfo to compare.</param>
        /// <returns>true if the values are equal.</returns>
        public bool Equals(ComponentInfo ci)
        {
            // If parameter is null return false.
            if (ci == null)
            {
                //Console.WriteLine("Null param found.");
                return false;
            }

            if (elementType != ci.elementType)
            {
                //Console.WriteLine("elementType != ci.elementType.");
                return false;
            }

            if (!name.Equals(ci.name))
            {
                //Console.WriteLine("name.Equals(ci.name).");
                return false;
            }

            /***********************************
            if (!(pins.Count == ci.pins.Count))
            {
                //Console.WriteLine("!(pins.Count == ci.pins.Count).");
                return false;
            }

            foreach (string pinname in pins)
            {
                if (!ci.pins.Contains(pinname))
                {
                    //Console.WriteLine("!ci.pins.Contains(pinname).");
                    return false;
                }
            }
             ******************************/


            if (!pins.SequenceEqual(ci.pins))
            {
                return false;
            }


            if (!(parameters.Count == ci.parameters.Count))
            {
                //Console.WriteLine("!(parameters.Count == ci.parameters.Count).");
                return false;
            }
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                if (!ci.parameters.ContainsKey( entry.Key ) )
                {
                    //Console.WriteLine("!ci.parameters.ContainsKey( entry.Key ).");
                    return false;
                }
                else if (!(ci.parameters[entry.Key] == entry.Value))
                {
                    //Console.WriteLine("!(ci.parameters[entry.Key] == entry.Value).");
                    return false;
                }
            }
            //Console.WriteLine("OK.");
            // Return true if the fields match:
            return true;
        }

        /// <summary>
        /// GetHashCode
        /// overrides GetHashCode() for ComponentInfo
        /// </summary>
        /// <returns> int hashcode</returns>
        public override int GetHashCode()
        {
            int x1 = name.GetHashCode();
            int x2 = elementType.GetHashCode();
            int x3 = pins.GetHashCode();
            int x4 = parameters.GetHashCode();
            return x1 ^ x2 ^ x3 ^ x4;
        }
    }
}
