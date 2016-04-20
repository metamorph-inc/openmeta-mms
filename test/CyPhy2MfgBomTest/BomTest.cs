using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CyPhy2MfgBomTest
{
    public class BomTest
    {
        [Fact]
        public void FakeBomSerialize()
        {
            var bom = MfgBom.Bom.MfgBom.CreateFakeBOM();
            
            String json = bom.Serialize();

            Console.Out.WriteLine(json);
        }
    }
}
