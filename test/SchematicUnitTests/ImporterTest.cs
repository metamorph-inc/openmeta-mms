using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using META;
using Xunit;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GME.MGA;
using GME.CSharp;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using System.Reflection;

namespace SchematicUnitTests
{
    public class SchematicImporterTest
    {
        #region paths

        private static string path_CMUcam
            = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                "..", "..",
                                "..", "..",
                            "models",
                            "Eagle",
                            "CMUcam4-v10.sch");

        private static string path_PulseOxyFinalBoard
            = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                "..", "..",
                                "..", "..",
                            "models",
                            "PulseOxy",
                            "FinalBoard",
                            "schema.sch");

        private static string path_IOIO
            = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                "..", "..",
                                "..", "..",
                            "models",
                            "Eagle",
                            "IOIO",
                            "IOIO-OTG-v20.sch");

        #endregion

        [Fact]
        public void DeviceList_CMUcam()
        {
            var result = CyPhyComponentAuthoring.Modules
                                                .EDAModelImport
                                                .GetDevicesInEagleModel(path_CMUcam);

            Assert.True(result.Count == 228,
                        String.Format("Not all devices were loaded from test file: {0} vs 228",
                                      result.Count));
        }

        [Fact]
        public void ConvertAllDevices_CMUCam()
        {
            ConvertAllDevices(path_CMUcam);
        }

        [Fact]
        public void BuildCyPhy_CMUCam()
        {
            ConvertAllSchematicsToCyPhy(path_CMUcam);
        }

        [Fact]
        public void ExtractLibraries_CMUCam()
        {
            ExtractStandaloneLibraries(path_CMUcam);
        }


        [Fact]
        public void DeviceList_PulseOxyFinalBoard()
        {
            var result = CyPhyComponentAuthoring.Modules
                                                .EDAModelImport
                                                .GetDevicesInEagleModel(path_PulseOxyFinalBoard);

            Assert.True(result.Any(), "No devices loaded from test file.");
        }

        [Fact]
        public void ConvertAllDevices_PulseOxyFinalBoard()
        {
            ConvertAllDevices(path_PulseOxyFinalBoard);
        }

        [Fact]
        public void BuildCyPhy_PulseOxyFinalBoard()
        {
            ConvertAllSchematicsToCyPhy(path_PulseOxyFinalBoard);
        }

        [Fact]
        public void ExtractLibraries_PulseOxyFinalBoard()
        {
            ExtractStandaloneLibraries(path_PulseOxyFinalBoard);
        }


        [Fact]
        public void DeviceList_IOIO()
        {
            var result = CyPhyComponentAuthoring.Modules
                                                .EDAModelImport
                                                .GetDevicesInEagleModel(path_IOIO);

            Assert.True(result.Any(), "No devices loaded from test file.");
        }

        [Fact]
        public void ConvertAllDevices_IOIO()
        {
            ConvertAllDevices(path_IOIO);
        }

        [Fact]
        public void BuildCyPhy_IOIO()
        {
            ConvertAllSchematicsToCyPhy(path_IOIO);
        }

        [Fact]
        public void ExtractLibraries_IOIO()
        {
            ExtractStandaloneLibraries(path_IOIO);
        }


        private static List<Tuple<String, avm.schematic.eda.EDAModel>> ConvertAllDevices(string path)
        {
            var result = CyPhyComponentAuthoring.Modules
                                                .EDAModelImport
                                                .GetDevicesInEagleModel(path);

            ConcurrentBag<Tuple<String, Exception>> cb_exceptions = new ConcurrentBag<Tuple<String, Exception>>();
            var cb_schematics = new ConcurrentBag<Tuple<string, avm.schematic.eda.EDAModel>>();

            Parallel.ForEach(result, device =>
            {
                var parts = device.Split('\\');
                var lib = parts[0];
                var devSet = parts[1];
                var dev = parts[2];

                try
                {
                    var deviceXML = CyPhyComponentAuthoring.Modules
                                                           .EDAModelImport
                                                           .GetEagleDevice(path, lib, devSet, dev);

                    var avm_schematic = CyPhyComponentAuthoring.Modules
                                                               .EDAModelImport
                                                               .ConvertEagleDeviceToAvmEdaModel(deviceXML);

                    cb_schematics.Add(
                        new Tuple<String, avm.schematic.eda.EDAModel>(
                            device,
                            avm_schematic
                            ));
                }
                catch (Exception e)
                {
                    cb_exceptions.Add(new Tuple<String, Exception>(device, e));
                }
            });

            if (cb_exceptions.Any())
            {
                String msg = String.Format("Exceptions encountered when converting {0} device(s):"
                                           + Environment.NewLine + Environment.NewLine,
                                           cb_exceptions.Count);
                foreach (var tuple in cb_exceptions)
                {
                    msg += String.Format("{0}: {1}" + Environment.NewLine + Environment.NewLine,
                                         tuple.Item1,
                                         tuple.Item2);
                }
                Assert.True(false, msg);
            }

            return cb_schematics.ToList();
        }

        private static void ConvertAllSchematicsToCyPhy(string path)
        {
            var schematics = ConvertAllDevices(path);

            // Create MGA project on the spot.
            var proj = new MgaProject();
            String connectionString = String.Format("MGA={0}", Path.GetTempFileName());
            proj.Create(connectionString, "CyPhyML");
            proj.EnableAutoAddOns(true);

            var mgaGateway = new MgaGateway(proj);
            proj.CreateTerritoryWithoutSink(out mgaGateway.territory);

            var module = new CyPhyComponentAuthoring.Modules.EDAModelImport();

            Dictionary<String, String> d_failures = new Dictionary<string, string>();
            mgaGateway.PerformInTransaction(delegate
            {
                var rf = CyPhyClasses.RootFolder.GetRootFolder(proj);
                var cf = CyPhyClasses.Components.Create(rf);

                foreach (var t in schematics)
                {
                    var identifier = t.Item1;
                    var schematic = t.Item2;

                    CyPhy.Component component = CyPhyClasses.Component.Create(cf);
                    component.Name = identifier;

                    module.SetCurrentComp(component);
                    module.CurrentObj = component.Impl as MgaFCO;

                    try
                    {
                        var cyphySchematicModel = module.BuildCyPhyEDAModel(schematic, component);

                        Assert.Equal(component.Children.SchematicModelCollection.Count(), 1);
                    }
                    catch (Exception e)
                    {
                        d_failures[identifier] = e.ToString();
                    }
                }
            },
            transactiontype_enum.TRANSACTION_NON_NESTED,
            abort: true);
            
            proj.Save();
            proj.Close();

            if (d_failures.Any())
            {
                String msg = String.Format("Failures in converting {0} component(s):" + Environment.NewLine,
                                           d_failures.Count);
                foreach (var kvp in d_failures)
                {
                    msg += String.Format("{0}: {1}" + Environment.NewLine + Environment.NewLine,
                                         kvp.Key,
                                         kvp.Value);
                }

                Assert.True(false, msg);
            }
        }

        private static void ExtractStandaloneLibraries(string path)
        {
            var result = CyPhyComponentAuthoring.Modules
                                                .EDAModelImport
                                                .GetDevicesInEagleModel(path);
                        
            ConcurrentBag<Tuple<String, Exception>> cb_exceptions = new ConcurrentBag<Tuple<String, Exception>>();
            ConcurrentBag<Tuple<String, avm.schematic.SchematicModel>> cb_schematics = new ConcurrentBag<Tuple<string, avm.schematic.SchematicModel>>();

            Parallel.ForEach(result, device =>
            {
                var parts = device.Split('\\');
                var lib = parts[0];
                var devSet = parts[1];
                var dev = parts[2];

                try
                {
                    var deviceXML = CyPhyComponentAuthoring.Modules
                                                           .EDAModelImport
                                                           .GetEagleDevice(path, lib, devSet, dev);

                    var standaloneXML = CyPhyComponentAuthoring.Modules
                                                               .EDAModelImport
                                                               .ExtractStandaloneLibrary(deviceXML);
                }
                catch (Exception e)
                {
                    cb_exceptions.Add(new Tuple<String, Exception>(device, e));
                }
            });
            
            if (cb_exceptions.Any())
            {
                String msg = String.Format("Exceptions encountered when extracting libraries for {0} device(s):"
                                           + Environment.NewLine + Environment.NewLine,
                                           cb_exceptions.Count);
                foreach (var tuple in cb_exceptions)
                {
                    msg += String.Format("{0}: {1}" + Environment.NewLine + Environment.NewLine,
                                         tuple.Item1,
                                         tuple.Item2);
                }
                Assert.True(false, msg);
            }
        }
    }
}
