using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Threading.Tasks;

namespace CyPhy2MfgBomTest
{
    public class OctopartQuery
    {
        private static String API_KEY = "22becbab";

        [Fact(Skip="Octopart API key needed")]
        public void QueryAndParse_ManyMPNs()
        {
            var listMpn = new List<String>()
            {
                "SN74S74N",
                "ERJ-2GE0R00X",
                "2-406549-1",
                "CRCW06031K00FKEA",
                "SMD100F-2",
                "TDGL002",
                "ATMEGA48-20AU"
            };

            var dictFailures = new Dictionary<String, Exception>();

            foreach (var mpn in listMpn)
            {
                var part = new MfgBom.Bom.Part()
                {
                    octopart_mpn = mpn
                };

                try
                {
                    part.QueryOctopartData();
                }
                catch (Exception ex)
                {
                    dictFailures.Add(mpn, ex);
                }
            }

            if (dictFailures.Any())
            {
                var msg = String.Format("Exception(s) encountered when processing {0} MPN(s):" + Environment.NewLine, 
                                        dictFailures.Count);

                foreach (var kvp in dictFailures)
                {
                    msg += kvp.Key + ": " + kvp.Value + Environment.NewLine + Environment.NewLine;
                }

                Assert.False(dictFailures.Any(), msg);
            }
        }
    }
}
