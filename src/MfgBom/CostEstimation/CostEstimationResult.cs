using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace MfgBom.CostEstimation
{
    public class CostEstimationResult
    {
        /// <summary>
        /// The original request data structure.
        /// </summary>
        public CostEstimationRequest request;

        /// <summary>
        /// A modified copy of the original BOM, with
        /// request results populated in the Part objects.
        /// </summary>
        public Bom.MfgBom result_bom;

        /// <summary>
        /// The parts cost per unit built.
        /// </summary>
        public float per_design_parts_cost;
        
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

        public static CostEstimationResult Deserialize(String json)
        {
            return JsonConvert.DeserializeObject<CostEstimationResult>(json);
        }
    }
}
