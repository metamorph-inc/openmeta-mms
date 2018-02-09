using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MfgBom.OctoPart;
using MfgBom.CostEstimation;

namespace BomCostAnalysis
{
    class Program
    {
        private static void Usage()
        {
            String.Format("USAGE: {0} manifest_path");
            Console.Out.WriteLine(Process.GetCurrentProcess().ProcessName);
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Usage();
                return -1;
            }

            // Parse arguments
            MfgBom.CostEstimation.CostEstimationRequest request;
            AVM.DDP.MetaTBManifest manifest;
            String pathResultsFolder;
            var result = ParseArgs(args, out request, out manifest, out pathResultsFolder);
            if (result != 0)
            {
                return result;
            }

            CostEstimationResult estimationResult;
            try
            {
                // Process costing request
                estimationResult = MfgBom.CostEstimation.Estimation.ProcessRequest(request);
            }
            catch (OctopartQueryException e)
            {
                Console.Error.WriteLine(e.Message);
                return 2;
            }

            // Dump result to JSON
            var pathResult = Path.Combine(pathResultsFolder,
                                          "CostEstimationResults.json");
            using (StreamWriter outfile = new StreamWriter(pathResult))
            {
                outfile.Write(estimationResult.Serialize());
            }
            var artifactResult = new AVM.DDP.MetaTBManifest.Artifact()
            {
                Location = "CostEstimationResults.json",
                Tag = "BomCostAnalysis::CostEstimationResults"
            };
            manifest.Artifacts.Add(artifactResult);


            // Populate manifest metrics
            var metricCost = manifest.Metrics.FirstOrDefault(m => m.Name == "part_cost_per_design");
            if (metricCost == null)
            {
                metricCost = new AVM.DDP.MetaTBManifest.Metric()
                {
                    Name = "part_cost_per_design",
                    DisplayedName = "Part Cost Per Design",
                    Unit = "USD",
                    GMEID = null
                };
                if (manifest.Metrics == null)
                {
                    manifest.Metrics = new List<AVM.DDP.MetaTBManifest.Metric>();
                }
                manifest.Metrics.Add(metricCost);
            }
            metricCost.Value = estimationResult.per_design_parts_cost.ToString();


            // Save UserBomTable
            var ubt = MfgBom.Converters.CostEstResult2UserBomTable.Convert(estimationResult);
            var pathUbt = Path.Combine(pathResultsFolder,
                                       "BomTable.csv");

            using (StreamWriter outfile = new StreamWriter(pathUbt))
            {
                outfile.Write(ubt.ToCsv());
            }

            // Also save it as HTML, for MOT-335
            var pathBomHtml = Path.Combine(pathResultsFolder,
                           "BomTable.html");
            using (StreamWriter outfile = new StreamWriter(pathBomHtml))
            {
                outfile.Write(ubt.ToHtml());    // MOT-335
            }

            var artifactUbt = new AVM.DDP.MetaTBManifest.Artifact()
            {
                Location = "BomTable.csv",
                Tag = "BomCostAnalysis::BomTable"
            };
            manifest.Artifacts.Add(artifactUbt);


            // Save manifest changes
            manifest.Serialize();


            return 0;
        }

        private static int ParseArgs(string[] args, out MfgBom.CostEstimation.CostEstimationRequest Request, out AVM.DDP.MetaTBManifest Manifest, out String pathResultsFolder)
        {
            Request = null;
            Manifest = null;
            pathResultsFolder = "";

            var pathManifest = args[0];
            if (false == File.Exists(pathManifest))
            {
                Console.Error.WriteLine("No file found at location: {0}",
                                        pathManifest);
                return -2;
            }

            pathResultsFolder = Path.GetDirectoryName(pathManifest);
            Manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(pathResultsFolder);
            if (null == Manifest)
            {
                Console.Error.WriteLine("Manifest is empty or didn't load correctly: {0}",
                                        pathManifest);
                return -3;
            }

            var artifactRequest = Manifest.Artifacts.FirstOrDefault(a => a.Tag == "CyPhy2MfgBom::CostEstimationRequest");
            if (null == artifactRequest)
            {
                Console.Error.WriteLine("Didn't find any artifacts in the manifest with the tag \"CyPhy2MfgBom::CostEstimationRequest\": {0}",
                                        pathManifest);
                return -4;
            }

            var pathRequest = Path.Combine(pathResultsFolder,
                                           artifactRequest.Location);
            if (false == File.Exists(pathRequest))
            {
                Console.Error.WriteLine("Couldn't find the file tagged with \"CyPhy2MfgBom::CostEstimationRequest\" at given path: {0}",
                                        pathRequest);
                return -5;
            }

            String jsonRequest;

            try
            {
                jsonRequest = File.ReadAllText(pathRequest);
                Request = MfgBom.CostEstimation.CostEstimationRequest.Deserialize(jsonRequest);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception loading the cost estimation request at \"{0}\": {1}",
                                        pathRequest,
                                        ex);
                return -6;
            }
            return 0;
        }
    }
}
