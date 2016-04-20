using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace MfgBom.CostEstimation
{
    public static class Estimation
    {
        public static CostEstimationResult ProcessRequest(CostEstimationRequest request)
        {
            CostEstimationResult result = new CostEstimationResult()
            {
                request = request,
                // Get copy of BOM by serializing and deserializing
                result_bom = MfgBom.Bom.MfgBom.Deserialize(request.bom.Serialize())                
            };

            
            // Update the Octopart data for each one
            Random rnd = new Random();
            foreach (var part in result.result_bom.Parts)
            {
                int numTries = 10;
                int i = 0;
                while (i++ < numTries)
                {
                    try
                    {
                        part.QueryOctopartData();

                        // Break the while loop
                        break;
                    }
                    catch (MfgBom.OctoPart.OctopartQueryRateException)
                    {
                        // Cool our heels a little
                        int interval = rnd.Next(3000, 5000);
                        System.Threading.Thread.Sleep(interval);
                    }
                    catch (MfgBom.OctoPart.OctopartQueryServerException ex)
                    {
                        throw ex;
                    }
                    if (i >= numTries)
                    {
                        throw new OctoPart.OctopartQueryException("Exceeded 10 tries");
                    }
                }
                
                System.Threading.Thread.Sleep(500);
            }


            // Choose a supplier for each
            foreach (var p in result.result_bom
                                    .Parts
                                    .Where(p => String.IsNullOrWhiteSpace(p.octopart_mpn) == false))
            {
                p.SelectSupplier(request.design_quantity);
            }
            
            // Try to calculate the cost
            result.per_design_parts_cost = result.result_bom
                                                 .Parts
                                                 .Sum(p => (p.SelectedSupplierPartCostPerUnit.HasValue)
                                                           ? p.SelectedSupplierPartCostPerUnit.Value * p.quantity
                                                           : 0);


            return result;
        }

        public static void SelectSupplier(this MfgBom.Bom.Part part, int design_quantity)
        {
            // Initialize SelectedSupplier fields
            part.SelectedSupplierPartCostPerUnit = null;
            part.SelectedSupplierName = null;
            part.SelectedSupplierSku = null;

            if (part.SellerMapStructure == null ||
                part.SellerMapStructure.sellerMap == null)
            {                
                using (StringWriter sw = new StringWriter())
                using (JsonTextWriter jtw = new JsonTextWriter(sw))
                {
                    Newtonsoft.Json.JsonSerializer.Create(
                            new Newtonsoft.Json.JsonSerializerSettings()
                            {
                                Formatting = Newtonsoft.Json.Formatting.Indented,
                                NullValueHandling = NullValueHandling.Ignore
                            })
                            .Serialize(jtw, part);

                    Console.Error.WriteLine("No sellers identified for part: " + Environment.NewLine + "{0}",
                                            sw.ToString());
                    return;
                }
            }

            int quantityNeeded = design_quantity * part.quantity;

            // Flatten the supplier structures for easier sorting
            List<FlatOffer> offers = new List<FlatOffer>();
            foreach (var sellerToSku in part.SellerMapStructure.sellerMap)
            {
                foreach (var skuToCurrency in sellerToSku.Value.skuMap)
                {
                    foreach (var currencyToPricePoint in skuToCurrency.Value.currencyMap)
                    {
                        foreach (var pricePoint in currencyToPricePoint.Value)
                        {
                            offers.Add(new FlatOffer()
                            {
                                supplier = sellerToSku.Key,
                                sku = skuToCurrency.Key,
                                currency = currencyToPricePoint.Key,
                                quantity = pricePoint.quantity,
                                price = pricePoint.price
                            });
                        }
                    }
                }
            }
            
            var offersUSD = offers.Where(o => o.currency == "USD");
            if (false == offersUSD.Any())
            {
                Console.WriteLine("Could not find any offers in USD for part with MPN {0}", part.octopart_mpn);
                return;
            }

            var offersByPrice = offersUSD.OrderBy(o => o.price);

            // Best offer is the cheapest one where we exceed its threshold.
            FlatOffer bestOffer = offersByPrice.FirstOrDefault(o => o.quantity <= quantityNeeded);
            if (bestOffer == null)
            {
                // Take lowest-quantity break.
                // If there's a tie in quantities, it will be the cheapest, since we're sorted by price.
                bestOffer = offersByPrice.OrderBy(o => o.quantity).First();
            }

            part.SelectedSupplierPartCostPerUnit = bestOffer.price;
            part.SelectedSupplierName = bestOffer.supplier;
            part.SelectedSupplierSku = bestOffer.sku;
        }

        private class FlatOffer
        {
            public String supplier;
            public String sku;
            public int quantity;
            public float price;
            public String currency;
        }
    }
}
