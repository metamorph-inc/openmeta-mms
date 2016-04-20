using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MfgBom.Converters
{
    public static class CostEstResult2UserBomTable
    {
        public static UserBomTable Convert(CostEstimation.CostEstimationResult result)
        {
            var rtn = new UserBomTable();

            rtn.BomTitle = result.result_bom.designName;      // Add BOM info for MOT-256.
            rtn.QueryDateTime = DateTime.Now.ToString("f");
            rtn.HowManyBoards = (uint) result.request.design_quantity;

            rtn.Rows = result.result_bom
                             .Parts
                             .AsParallel()                             
                             .Select(p => Convert(p))
                             .ToList();

            return rtn;
        }

        private static UserBomTableRow Convert(MfgBom.Bom.Part part)
        {
            return new UserBomTableRow() {
                Description = part.Description,
                Manufacturer = part.Manufacturer,
                ManufacturerPartNumber = part.ManufacturerPartNumber,
                Notes = part.Notes,
                Package = part.Package,
                Quantity = part.quantity,
                ReferenceDesignators = String.Join(", " + Environment.NewLine, 
                                                   part.instances_in_design
                                                       .Select(ci => ci.path)),
                Supplier1 = part.SelectedSupplierName,
                Supplier1PartNumber = part.SelectedSupplierSku,
                Supplier1UnitPrice = part.SelectedSupplierPartCostPerUnit
            };
        }
    }
}
