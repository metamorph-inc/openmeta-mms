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

        [Fact]
        public void ExceedRateLimit()
        {
            var querier = new MfgBom.OctoPart.Querier(API_KEY);

            List<int> number = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                number.Add(i);
            }

            Assert.Throws<MfgBom.OctoPart.OctopartQueryRateException>(delegate
            {
                try
                {
                    Parallel.ForEach(number, i =>
                    {
                        List<String> includes = new List<string>() { "specs", "descriptions" };
                        querier.QueryMpn("SN74S74N", false, includes, true);
                    });
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }
            });
        }

        [Fact]
        public void ExceedLimitAndRecover()
        {
            ExceedRateLimit();

            var querier = new MfgBom.OctoPart.Querier(API_KEY);
            var part = new MfgBom.Bom.Part()
            {
                octopart_mpn = "SN74S74N"
            };
            Assert.True(part.QueryOctopartData(API_KEY));
        }

        [Fact]
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
