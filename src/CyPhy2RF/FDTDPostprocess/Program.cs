using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using System.Diagnostics;

using CSXCAD;

namespace Postprocess
{
    class Program
    {
        private static string manifestPath = ".";

        static int Main(string[] args)
        {
            string sarInputFile = null;
            string dirInputFile = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-sar":
                        sarInputFile = args[i + 1];
                        i++;
                        break;
                    case "-directivity":
                    case "-dir":
                        dirInputFile = args[i + 1];
                        i++;
                        break;
                    default:
                        Console.Error.WriteLine("Unrecognized command line argument '" + args[i] + "'");
                        return 1;
                }
            }

            try
            {
                if (dirInputFile != null)
                {
                    return ProcessNF2FF(dirInputFile);
                }

                if (sarInputFile != null)
                {
                    ProcessSAR(sarInputFile);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }

            return 0;
        }

        static void ProcessSAR(string inputFileName)
        {
            // Constants
            XElement xDoc = XElement.Load(inputFileName);
            double f0 = Convert.ToDouble(xDoc.Element("FDTD").Element("Excitation").Attribute("f0").Value);
            var leQuery = from xe in xDoc.Element("ContinuousStructure").Element("Properties").Elements("LumpedElement")
                          where xe.Attribute("Name").Value.Contains("resist")
                          select xe;
            double r = Convert.ToDouble(leQuery.First().Attribute("R").Value);

            // Port calculations
            double[] freqs = Utility.LinearSpace(f0 / 2, f0 * 3 / 2, 501);
            var lumpedPort = new LumpedPort(0, 1, r, new Vector3D(-10, -1, -1), new Vector3D(10, 1, 1), ENormDir.X, true);
            lumpedPort.ReadResults(freqs);
            double Pin_f0 = lumpedPort.GetPFdInAt(f0);
            Console.WriteLine();

            // SAR
            string sarFileName = @"SAR.h5";
            var sarDump = new Postprocess.SAR(sarFileName);
            double totalPower = HDF5.ReadAttribute(sarFileName, @"/FieldData/FD/f0", "power");
            Console.WriteLine("Field maximum: {0:e4}", sarDump.MaxValue);
            Console.WriteLine("Field maximum location: ({0})", String.Join(",", sarDump.MaxCoordinates.Select(x => String.Format("{0:f2}", x))));

            Console.WriteLine("Exporting SAR dump slices to PNG files...");
            string filenameSarX = "SAR-X.png";
            string filenameSarY = "SAR-Y.png";
            string filenameSarZ = "SAR-Z.png";
            sarDump.ToPNG(filenameSarX, Postprocess.SAR.ENormDir.X, sarDump.MaxCoordinates[0]);
            sarDump.ToPNG(filenameSarY, Postprocess.SAR.ENormDir.Y, sarDump.MaxCoordinates[1]);
            sarDump.ToPNG(filenameSarZ, Postprocess.SAR.ENormDir.Z, sarDump.MaxCoordinates[2]);
            Console.WriteLine("Exporting SAR to VTK file...");
            sarDump.ToVTK(inputFileName);

            // NF2FF
            Console.WriteLine("Calculating antenna parameters...");
            var nf2ff = new Postprocess.NF2FF(f0);
            try
            {
                nf2ff.ReadHDF5Result();

                Console.WriteLine("Maximum SAR:    {0:f3} W/kg (normalized to 1 W accepted power)", sarDump.MaxValue / Pin_f0);
                Console.WriteLine("Accepted power: {0:e4} W", Pin_f0);
                Console.WriteLine("Radiated power: {0:e4} W", nf2ff.RadiatedPower);
                Console.WriteLine("Absorbed power: {0:e4} W", totalPower);
                Console.WriteLine("Power budget:   {0:f3} %", 100 * (nf2ff.RadiatedPower + totalPower) / Pin_f0);

                Console.WriteLine("Populating manifest file...");
                var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(manifestPath);

                // Initialize Metrics list if necessary
                if (manifest.Metrics == null)
                {
                    manifest.Metrics = new List<AVM.DDP.MetaTBManifest.Metric>();
                }

                // Look for existing metric. Create a new one if not found.
                string metricName = "SAR_max";
                AVM.DDP.MetaTBManifest.Metric metric = manifest.Metrics.FirstOrDefault(m => m.Name.Equals(metricName));
                if (metric == null)
                {
                    metric = new AVM.DDP.MetaTBManifest.Metric()
                    {
                        Name = metricName
                    };
                    manifest.Metrics.Add(metric);
                }

                // Set metric attributes
                metric.DisplayedName = "SAR maximum";
                metric.Description = "Maximum Specific Absorption Ratio (SAR) averaged over volumes containing 1 gram of tissue.";
                metric.Unit = "W/kg";
                metric.Value = String.Format("{0:e4}", sarDump.MaxValue / Pin_f0);

                metric.VisualizationArtifacts = new List<AVM.DDP.MetaTBManifest.Artifact>();
                metric.VisualizationArtifacts.Add(new AVM.DDP.MetaTBManifest.Artifact() { Location = filenameSarX, Tag = "CyPhy2RF::SAR::X" });
                metric.VisualizationArtifacts.Add(new AVM.DDP.MetaTBManifest.Artifact() { Location = filenameSarY, Tag = "CyPhy2RF::SAR::Y" });
                metric.VisualizationArtifacts.Add(new AVM.DDP.MetaTBManifest.Artifact() { Location = filenameSarZ, Tag = "CyPhy2RF::SAR::Z" });

                manifest.Serialize(manifestPath);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error reading far-field results: {0}", e);
            }
        }

        static int ProcessNF2FF(string inputFileName)
        {
            // Constants
            XElement xDoc = XElement.Load(inputFileName);
            double f0 = Convert.ToDouble(xDoc.Element("FDTD").Element("Excitation").Attribute("f0").Value);
            var leQuery = from xe in xDoc.Element("ContinuousStructure").Element("Properties").Elements("LumpedElement")
                          where xe.Attribute("Name").Value.Contains("resist")
                          select xe;
            double r = Convert.ToDouble(leQuery.First().Attribute("R").Value);

            // Port calculations
            double[] freqs = Utility.LinearSpace(f0 / 2, f0 * 3 / 2, 501);
            var antennaPort = new LumpedPort(0, 1, r, new Vector3D(-10, -1, -1), new Vector3D(10, 1, 1), ENormDir.X, true);
            antennaPort.ReadResults(freqs);
            double Pin_f0 = antennaPort.GetPFdInAt(f0);

            // NF2FF
            var nf2ff = new Postprocess.NF2FF(f0);
            try
            {
                nf2ff.ReadHDF5Result();
                nf2ff.ToVTK(fileName: "directivity_pattern.vtk");

                Console.WriteLine("Radiated power:    {0,15:e4} W", nf2ff.RadiatedPower);
                Console.WriteLine("Directivity (max): {0,15:e4} dBi", 10.0*Math.Log10(nf2ff.Directivity));

                var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(manifestPath);

                // Initialize Metrics list if necessary
                if (manifest.Metrics == null)
                {
                    manifest.Metrics = new List<AVM.DDP.MetaTBManifest.Metric>();
                }

                // Look for existing metric. Create a new one if not found.
                string metricName = "Directivity";
                AVM.DDP.MetaTBManifest.Metric metric = manifest.Metrics.FirstOrDefault(m => m.Name.Equals(metricName));
                if (metric == null)
                {
                    metric = new AVM.DDP.MetaTBManifest.Metric()
                    {
                        Name = metricName
                    };
                    manifest.Metrics.Add(metric);
                }

                // Set metric attributes
                metric.DisplayedName = "Antenna directivity";
                metric.Description = "Antenna directivity.";
                metric.Unit = "dBi";
                metric.Value = String.Format("{0:e4}", 10.0 * Math.Log10(nf2ff.Directivity));

                manifest.Serialize(manifestPath);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error reading far-field results: {0}", e);
                return 5;
            }
            return 0;
        }
    }
}
