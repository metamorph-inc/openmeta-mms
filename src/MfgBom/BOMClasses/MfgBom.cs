using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace MfgBom.Bom
{
    public class MfgBom
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MfgBom()
        {
            Parts = new List<Part>();
        }

        public string designName;  // For MOT-256

        public List<Part> Parts { get; private set; }

        public void AddPart(Part part)
        {
            /* See if we already have a part with this MPN.
             * If so, we'll add this instance data to the existing part.
             * If not, we'll add this as a new part.
             */

            if (part.octopart_mpn != null && 
                Parts.Any(p => p.octopart_mpn == part.octopart_mpn))
            {
                // Consolidate with the existing part

                var existingPart = Parts.First(p => p.octopart_mpn == part.octopart_mpn);
                foreach (var instance in part.instances_in_design)
                {
                    existingPart.AddInstance(instance);
                }
            }
            else
            {
                // Add as a new part
                Parts.Add(part);
            }
        }

        /// <summary>
        /// Get the current object as a JSON string.
        /// </summary>
        /// <returns>JSON-formatted string</returns>
        public String Serialize()
        {
            JsonSerializer serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            String rtn = null;
            using (StringWriter sw = new StringWriter())
            using (JsonTextWriter jtw = new JsonTextWriter(sw))
            {
                serializer.Serialize(jtw, this);
                rtn = sw.ToString();
            }

            return rtn;
        }

        public static MfgBom Deserialize(String json)
        {
            return JsonConvert.DeserializeObject<MfgBom>(json);
        }

        /// <summary>
        /// Create and return a randomly-generated BOM data structure.
        /// </summary>
        /// <returns></returns>
        public static MfgBom CreateFakeBOM()
        {
            var bom = new MfgBom()
            {
                Parts = new List<Part>()
            };

            Random random = new Random();
            for (int i = 0; i <= 3; i++)
            {
                var part = new Part()
                {
                    octopart_mpn = Path.GetRandomFileName()
                };
                bom.Parts.Add(part);

                for (int j = 0; j <= 3; j++)
                {
                    var instance = new ComponentInstance()
                    {
                        gme_object_id = random.Next(100000).ToString(),
                        path = Path.GetRandomFileName()
                    };
                    part.AddInstance(instance);
                }
            }

            return bom;
        }
    }
}
