using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using Newtonsoft.Json;
using MfgBom.CostEstimation;
using System.Reflection;

namespace CyPhy2MfgBomTest
{
    public class EstimateAndSelect
    {
        private static String TEST_DIRECTORY = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                "..", "..",
                                "..", "..",
                                "test",
                                "CyPhy2MfgBomTest");

        [Fact]
        public void FPGA_Quantity1()
        {
            int design_quantity = 1;
            float expected_cost = 30.4F;
            SelectFPGASupplier(design_quantity, expected_cost);
        }
                
        [Fact]
        public void FPGA_Quantity25()
        {
            int design_quantity = 25;
            float expected_cost = 26.45F;
            SelectFPGASupplier(design_quantity, expected_cost);
        }

        [Fact]
        public void FPGA_Quantity100()
        {
            int design_quantity = 100;
            float expected_cost = 24.3875F;
            SelectFPGASupplier(design_quantity, expected_cost);
        }

        [Fact]
        public void FPGA_Quantity0()
        {
            int design_quantity = 0;
            float expected_cost = 30.4F;
            SelectFPGASupplier(design_quantity, expected_cost);
        }

        [Fact]
        public void NoSuppliers()
        {
            var part = new MfgBom.Bom.Part();
            part.AddInstance(new MfgBom.Bom.ComponentInstance());
            part.SelectSupplier(1);

            Assert.Null(part.SelectedSupplierName);
            Assert.Null(part.SelectedSupplierPartCostPerUnit);
            Assert.Null(part.SelectedSupplierSku);
        }

        [Fact]
        public void NoUSDOffers()
        {
            var pathMockSellerMapStructure = Path.Combine(TEST_DIRECTORY,
                                                          "MockDataStructures",
                                                          "NoUSDOffers.SellerMapStructure.json");
            var jsonMockSellerMapStructure = File.ReadAllText(pathMockSellerMapStructure);

            var part = new MfgBom.Bom.Part()
            {
                SellerMapStructure = JsonConvert.DeserializeObject<MfgBom.Bom.Part.SellerMapStruct>(jsonMockSellerMapStructure)
            };
            part.AddInstance(new MfgBom.Bom.ComponentInstance());

            part.SelectSupplier(1);

            Assert.Null(part.SelectedSupplierName);
            Assert.Null(part.SelectedSupplierPartCostPerUnit);
            Assert.Null(part.SelectedSupplierSku);
        }
        
        private static void SelectFPGASupplier(int design_quantity, float expected_cost)
        {
            var pathMockSellerMapStructure = Path.Combine(TEST_DIRECTORY,
                                                          "MockDataStructures",
                                                          "LFE3-17EA-6MG328C.SellerMapStructure.json");
            var jsonMockSellerMapStructure = File.ReadAllText(pathMockSellerMapStructure);

            var part = new MfgBom.Bom.Part()
            {
                SellerMapStructure = JsonConvert.DeserializeObject<MfgBom.Bom.Part.SellerMapStruct>(jsonMockSellerMapStructure)
            };
            part.AddInstance(new MfgBom.Bom.ComponentInstance());

            part.SelectSupplier(design_quantity);

            Assert.False(String.IsNullOrWhiteSpace(part.SelectedSupplierName));
            Assert.Equal("Digi-Key", part.SelectedSupplierName);
            Assert.Equal(expected_cost, part.SelectedSupplierPartCostPerUnit);
        }
    }
}
