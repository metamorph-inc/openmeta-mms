using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using META;

using Tonka = ISIS.GME.Dsml.CyPhyML.Interfaces;
using TonkaClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CyPhyGUIs;
using CyPhyComponentFidelitySelector;
using GME.MGA;

namespace CyPhy2Schematic.Schematic
{
    public class CodeGenerator
    {
        public GMELogger Logger { get; set; }
        public bool verbose { get; set; }
        public string BasePath { get; set; } // Path of the root testbench object in GME object tree
        public static Dictionary<string, int> partNames;
        public Dictionary<Eagle.part, Component> partComponentMap;
        public Dictionary<Component, Eagle.part> componentPartMap;
        public Dictionary<Port, Eagle.net> polyNetMap;   // nets corresponding to power or ground planes
        public string[] polyComponentClasses = new string[] {
                "pcb_board",
                "template.pcb_template"
            };

        public Layout.LayoutParser signalIntegrityLayout { get; set; } // layout parser for signal integrity mode
        public Dictionary<ComponentAssembly, Layout.LayoutParser> preRouted { get; set; } // layout parsers for prerouted assemblies/subcircuits

        public enum Mode
        {
            EDA,
            SPICE,
            SPICE_SI
        };
        public Mode mode { get; private set; }

        public class GenerateLayoutCodeResult
        {
            // Results of GenerateLayoutCode(), for merge of MOT-782 and WEB-260 changes.
            public bool bonesFound;                 // MOT-782: Intended to prevent autorouting if true.
            public LayoutJson.Layout boardLayout;   // WEB-260
        }

        public class Result
        {
            // results of generate code
            public string runCommandArgs;       // arguments for the runCommand to be sent to job manager
            public bool bonesFound = false;     // MOT-782: Intended to prevent autorouting if true.
        }

        private CyPhyGUIs.IInterpreterMainParameters mainParameters { get; set; }
        private MgaTraceability Traceability;
        private Dictionary<string, CyPhy2SchematicInterpreter.IDs> mgaIdToDomainIDs;
        private ISet<IMgaObject> selectedSpiceModels;

        public CodeGenerator(IInterpreterMainParameters parameters, Mode mode, MgaTraceability traceability, Dictionary<string, CyPhy2SchematicInterpreter.IDs> mgaIdToDomainIDs, ISet<IMgaObject> selectedSpiceModels)
        {
            this.mainParameters = parameters;
            this.Traceability = traceability;
            this.mgaIdToDomainIDs = mgaIdToDomainIDs;
            this.verbose = ((CyPhy2Schematic.CyPhy2Schematic_Settings)parameters.config).Verbose;
            partNames = new Dictionary<string, int>();
            partComponentMap = new Dictionary<Eagle.part, Component>();
            componentPartMap = new Dictionary<Component, Eagle.part>();
            polyNetMap = new Dictionary<Port, Eagle.net>();
            preRouted = new Dictionary<ComponentAssembly, Layout.LayoutParser>();

            this.mode = mode;
            this.selectedSpiceModels = selectedSpiceModels;
        }

        private void CommonTraversal(TestBench TestBench_obj)
        {
            // 1. A first traversal maps CyPhy objects to a corresponding but significantly lighter weight object network that only includes a
            //     small set of concepts/classes : TestBench, ComponentAssembly, Component, Parameter, Port, Connection
            // 2. Second and third traversal passes compute the layout of the graph in schematic
            // 3. Forth traversal wires the object network
            //      the object network is hierarchical, but the wiring is direct and skips hierarchy. The dependency on CyPhy is largely localized to the
            //      traversal/visitor code (CyPhyVisitors.cs)

            TestBench_obj.accept(new CyPhyBuildVisitor(this.mainParameters.ProjectDirectory, this.mode, Traceability, mgaIdToDomainIDs, selectedSpiceModels)
            {
                Logger = Logger
            });

            if (mode == Mode.EDA)
            {
                TestBench_obj.accept(new CyPhyLayoutVisitor() { Logger = Logger });
                TestBench_obj.accept(new CyPhyLayout2Visitor() { Logger = Logger });
            }

            TestBench_obj.accept(new CyPhyConnectVisitor(this.mode) { Logger = Logger });
        }

        private Eagle.eagle GenerateSchematicCode(TestBench TestBench_obj)
        {
            // load schematic library
            Eagle.eagle eagle = null;
            try
            {
                eagle = Eagle.eagle.Deserialize(CyPhy2Schematic.Properties.Resources.schematicTemplate);
                Logger.WriteInfo("Parsed Eagle Library schema version: " + eagle.version);
            }
            catch (Exception e)
            {
                eagle = new Eagle.eagle();  // create an empty eagle object network
                Logger.WriteError("Error parsing XML: " + e.Message + "<br>Inner: " + e.InnerException + "<br>Stack: " + e.StackTrace);
            }
            // 2. The second traversal walks the lighter weight (largely CyPhy independent) object network and maps to the eagle XML object network
            //    the classes of this object network are automatically derived from the eagle XSD using the XSD2Code tool in the META repo
            //    an important step of this traversal is the routing which is implemented currently as a simple rats nest routing,
            //        the traversal and visitor code is localized in (SchematicTraversal.cs)
            TestBench_obj.accept(new EdaVisitor(this)
            {
                eagle_obj = eagle,
                Logger = Logger
            });

            // 2.5  Finally a serializer (XSD generated code), walks the object network and generates the XML file
            System.IO.Directory.CreateDirectory(this.mainParameters.OutputDirectory);
            String outFile = Path.Combine(this.mainParameters.OutputDirectory, "schema.sch");
            try
            {
                eagle.SaveToFile(outFile);
            }
            catch (Exception ex)
            {
                Logger.WriteError("Error Saving Schema File: {0}<br> Exception: {1}<br> Trace: {2}",
                    outFile, ex.Message, ex.StackTrace);
            }

            return eagle;
        }

        private GenerateLayoutCodeResult GenerateLayoutCode(Eagle.eagle eagle, Schematic.TestBench TestBench_obj)
        {
            // write layout file
            string layoutFile = Path.Combine(this.mainParameters.OutputDirectory, "layout-input.json");
            var myLayout = new Layout.LayoutGenerator(eagle.drawing.Item as Eagle.schematic, TestBench_obj, Logger, this.mainParameters.OutputDirectory, this,
                onlyConsiderExactConstraints: ((CyPhy2Schematic_Settings)mainParameters.config).onlyConsiderExactConstraints);
            myLayout.Generate(layoutFile);
            GenerateLayoutCodeResult result = new GenerateLayoutCodeResult();
            result.bonesFound = myLayout.bonesFound;  // MOT-782
            result.boardLayout = myLayout.boardLayout;
            return result;
        }

        private void GenerateSpiceCode(TestBench TestBench_obj)
        {
            var circuit = new Spice.Circuit() { name = TestBench_obj.Name };
            var siginfo = new Spice.SignalContainer() { name = TestBench_obj.Name, objectToNetId = new Dictionary<CyPhy2SchematicInterpreter.IDs,string>() };
            // now traverse the object network with Spice Visitor to build the spice and siginfo object network
            TestBench_obj.accept(new SpiceVisitor(Traceability, mgaIdToDomainIDs, this) { circuit_obj = circuit, siginfo_obj = siginfo, mode = this.mode });
            String spiceTemplateFile = Path.Combine(this.mainParameters.OutputDirectory, "schema.cir.template");
            circuit.Serialize(spiceTemplateFile);
            String siginfoFile = Path.Combine(this.mainParameters.OutputDirectory, "siginfo.json");
            siginfo.Serialize(siginfoFile);

        }

        private void GenerateChipFitCommandFile()
        {
            var chipFitBat = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, "chipfit.bat"));
            chipFitBat.Write(CyPhy2Schematic.Properties.Resources.chipFit);
            chipFitBat.Close();
        }

        private void GenerateShowChipFitResultsCommandFile()
        {
            var showChipFitResultsBat = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, "showChipFitResults.bat"));
            showChipFitResultsBat.Write(CyPhy2Schematic.Properties.Resources.showChipFitResults);
            showChipFitResultsBat.Close();
        }

        private void GeneratePlacementCommandFile()
        {
            var placeBat = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, "placement.bat"));
            placeBat.Write(CyPhy2Schematic.Properties.Resources.placement);
            placeBat.Close();
        }

        private void GeneratePlaceOnlyCommandFile()
        {
            var placeBat = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, "placeonly.bat"));
            placeBat.Write(CyPhy2Schematic.Properties.Resources.placeonly);
            placeBat.Close();
        }

        private void GeneratePopulateTemplateScriptFile()
        {
            var writer = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, "PopulateSchemaTemplate.py"));
            writer.Write(CyPhy2Schematic.Properties.Resources.PopulateSchemaTemplate);
            writer.Close();
        }

        private void GenerateLayoutReimportFiles(LayoutJson.Layout layoutJson)
        {
            File.WriteAllText(Path.Combine(this.mainParameters.OutputDirectory, "layoutReimport.bat"), CyPhy2Schematic.Properties.Resources.layoutReimport);
            var metadata = new Dictionary<string, object>();
            metadata["currentFCO"] = this.mainParameters.CurrentFCO.AbsPath;
            metadata["mgaFile"] = Path.Combine(this.mainParameters.ProjectDirectory, Path.GetFileName(this.mainParameters.Project.ProjectConnStr.Substring("MGA=".Length)));
            metadata["layoutBox"] = String.Format("0,0,{0},{1},0;0,0,{0},{1},1", layoutJson.boardWidth, layoutJson.boardHeight);

            File.WriteAllText(Path.Combine(this.mainParameters.OutputDirectory, "layoutReimportMetadata.json"), JsonConvert.SerializeObject(metadata, Formatting.Indented));
        }

        private string GenerateCommandArgs(TestBench Testbench_obj)
        {
            string commandArgs = "";
            var icg = Testbench_obj.Parameters.FirstOrDefault(p => p.Name.Equals("interChipSpace"));       // in mm
            var eg = Testbench_obj.Parameters.FirstOrDefault(p => p.Name.Equals("boardEdgeSpace"));    // in mm
            var maxRetries = Testbench_obj.Parameters.FirstOrDefault(p => p.Name.Equals("maxRetries"));
            var maxThreads = Testbench_obj.Parameters.FirstOrDefault(p => p.Name.Equals("maxThreads")); // MOT-729

            if (Testbench_obj.Impl.Children.ComponentAssemblyCollection.Where(ca => string.IsNullOrEmpty(((GME.MGA.IMgaFCO)ca.Impl).RegistryValue["layoutFile"]) == false).Count() > 0)
            {
                // there is a layoutFile for the SUT. The layout.json takes up the whole board
                commandArgs += " -e 0 -i 0";
            }
            else
            {
                if (icg != null)
                {
                    commandArgs += " -i " + icg.Value;
                }
                if (eg != null)
                {
                    commandArgs += " -e " + eg.Value;
                }
            }
            if (maxRetries != null)
            {
                commandArgs += " -s " + maxRetries.Value;
            }
            if (maxThreads != null)
            {
                commandArgs += " -t " + maxThreads.Value;   // MOT-729
            }

            return commandArgs;
        }

        private void CopyBoardFilesSpecifiedInTestBench(TestBench Testbench_obj)
        {
            CopyResourceFromTestBench(Testbench_obj, "designRules");
            CopyResourceFromTestBench(Testbench_obj, "boardTemplate");
            CopyResourceFromTestBench(Testbench_obj, "autorouterConfig", "autoroute.ctl");
        }

        private void CopyBoardFilesSpecifiedInPcbComponent(TestBench tb_obj)
        {
            // Look to see if there is a PCB component in the top level assembly
            var compImpl = tb_obj.ComponentAssemblies
                                 .SelectMany(c => c.ComponentInstances)
                                 .Select(i => i.Impl)
                                 .FirstOrDefault(j => (j as Tonka.Component).Attributes
                                                                            .Classifications
                                                                            .Contains("pcb_board")
                                                   || (j as Tonka.Component).Attributes
                                                                            .Classifications
                                                                            .Contains("template.pcb_template"));
            var comp = (compImpl != null) ? compImpl as Tonka.Component : null;

            if (comp == null)
                return;

            CopyResourceFromComp(comp, "boardTemplate");
            CopyResourceFromComp(comp, "designRules");
            CopyResourceFromComp(comp, "autoRouterConfig", "autoroute.ctl");
        }

        private void CopyResourceFromComp(Tonka.Component comp, string resName, string destFileName = null)
        {
            var res = (comp != null) ? comp.Children.ResourceCollection.Where(r =>
                r.Name.ToUpper().Contains(resName.ToUpper())).FirstOrDefault() : null;
            if (res != null)
            {
                var rPath = Path.Combine(comp.GetDirectoryPath(ComponentLibraryManager.PathConvention.REL_TO_PROJ_ROOT),
                                         res.Attributes.Path);

                if (String.IsNullOrWhiteSpace(destFileName))
                {
                    destFileName = Path.GetFileName(rPath);
                }

                try
                {
                    CopyFile(rPath, destFileName);
                }
                catch (FileNotFoundException)
                {
                    Logger.WriteError("PCB Template <a href=\"mga:{0}\">{1}</a> specifies a {2} file, {3}, that cannot be found.",
                                      comp.ID,
                                      comp.Name,
                                      resName,
                                      rPath);
                    throw;
                }
                catch (DirectoryNotFoundException)
                {
                    // If the directory of the source file doesn't exist, throw this error.
                    if (!Directory.Exists(Path.Combine(this.mainParameters.ProjectDirectory,
                                                       Path.GetDirectoryName(rPath))))
                    {
                        Logger.WriteError("PCB Template <a href=\"mga:{0}\">{1}</a> specifies a {2} file, {3}, that cannot be found.",
                                      comp.ID,
                                      comp.Name,
                                      resName,
                                      rPath);
                        throw;
                    }
                    else
                    {
                        // The destination directory must not exist.
                        Logger.WriteError("The output directory, {0}, does not exist.",
                                          this.mainParameters.OutputDirectory);
                        throw;
                    }
                }
            }
        }

        private void CopyResourceFromTestBench(TestBench Testbench_obj, string param, string destFileName = null)
        {
            var par = Testbench_obj.Parameters.Where(p => p.Name.Equals(param)).FirstOrDefault();
            if (par == null)
                return;

            if (String.IsNullOrWhiteSpace(destFileName))
            {
                destFileName = Path.GetFileName(par.Value);
            }

            String pathSrcFile = Path.Combine(Testbench_obj.Impl.Impl.Project.GetRootDirectoryPath(),
                                              par.Value);

            try
            {
                CopyFile(par.Value, destFileName);
            }
            catch (FileNotFoundException)
            {
                Logger.WriteError("This test bench specifies a {0} file, {1}, that cannot be found.",
                                  param,
                                  par.Value);
                throw;
            }
            catch (DirectoryNotFoundException)
            {
                // If the directory of the source file doesn't exist, throw this error.
                if (!Directory.Exists(Path.Combine(this.mainParameters.ProjectDirectory,
                                                   Path.GetDirectoryName(par.Value))))
                {
                    Logger.WriteError("This test bench specifies a {0} file, {1}, that cannot be found.",
                                      param,
                                      par.Value);
                    throw;
                }
                else
                {
                    // The destination directory must not exist.
                    Logger.WriteError("The output directory, {0}, does not exist.",
                                      this.mainParameters.OutputDirectory);
                    throw;
                }
            }
        }

        /// <summary>
        /// Copy a file to the output directory from somewhere in the project.
        /// Could throw
        /// </summary>
        /// <param name="srcRelPath">The path of the file to copy, relative to the project root directory</param>
        /// <param name="dstName">The name that the file should receive in the output directory</param>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the source file cannot be found</exception>
        /// <exception cref="System.IO.DirectoryNotFound">Thrown if the the source or destination directory cannot be found</exception>
        private void CopyFile(string srcRelPath, string dstName)
        {
            var source = Path.Combine(this.mainParameters.ProjectDirectory, srcRelPath);
            var dest = Path.Combine(this.mainParameters.OutputDirectory, dstName);
            System.IO.File.Copy(source, dest, overwrite: true);
        }

        private void GenerateSpiceCommandFile(TestBench Testbench_obj)
        {
            var spiceBat = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, "runspice.bat"));
            spiceBat.Write(CyPhy2Schematic.Properties.Resources.runspice);

            // find a voltage source test component
            var voltageSrc = Testbench_obj.Impl.Children
                                               .TestComponentCollection
                                               .FirstOrDefault(c => c.Children.SPICEModelCollection
                                                                              .Select(x => x.Attributes.Class)
                                                                              .Contains("V"));
            if (voltageSrc != null)
            {
                // add a call to spice post process
                spiceBat.Write("if exist \"schema.raw\" (\n");
                spiceBat.Write("\t\"%META_PATH%\\bin\\python27\\scripts\\python.exe\" -E -m SpiceVisualizer.post_process -m {0} schema.raw\n", voltageSrc.Name);
                spiceBat.Write(")\n");
            }
            spiceBat.Close();
        }

        private void GenerateSpiceViewerLauncher()
        {
            var launchViewerBat = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, "LaunchSpiceViewer.bat"));
            launchViewerBat.Write(CyPhy2Schematic.Properties.Resources.LaunchSpiceViewer);
            launchViewerBat.Close();
        }

        private void GenerateReferenceDesignatorMappingTable(TestBench Testbench_obj)
        {
            using (var sw = new StreamWriter(Path.Combine(this.mainParameters.OutputDirectory, "reference_designator_mapping_table.html")))
            {

                // Get all nested assemblies using an interative approach.
                var assemblies = new List<ComponentAssembly>();
                assemblies.AddRange(Testbench_obj.ComponentAssemblies);
                for (int i = 0; i < assemblies.Count; i++ )
                {
                    var assembly = assemblies[i];
                    assemblies.AddRange(assembly.ComponentAssemblyInstances);
                }

                // Get all instances from everywhere.
                var componentInstances = assemblies.SelectMany(a => a.ComponentInstances).ToList();
                componentInstances.AddRange(Testbench_obj.TestComponents);

                // Build mapping table
                List<XrefItem> xrefTable = new List<XrefItem>();
                foreach (var ci in componentInstances)
                {
                    String path = ci.Impl.Path;
                    String refDes = ci.Name;
                    XrefItem row = new XrefItem() { ReferenceDesignator = refDes, GmePath = path };
                    xrefTable.Add(row);
                }

                // Convert it to HTML
                string html = Xref2Html.makeHtmlFile(
                    "",
                    xrefTable,
                    "");

                // Write mapping table to file
                sw.Write(html);
            }
        }

        public Result GenerateCode()
        {
            Result result = new Result();

            // map the root testbench obj
            var testbench = TonkaClasses.TestBench.Cast(this.mainParameters.CurrentFCO);
            if (testbench == null)
            {
                Logger.WriteError("Invalid context of invocation <{0}>, invoke the interpreter from a Testbench model",
                    this.mainParameters.CurrentFCO.Name);
                return result;
            }
            var TestBench_obj = new TestBench(testbench);
            BasePath = testbench.Path;

            CommonTraversal(TestBench_obj);

            GenerateReferenceDesignatorMappingTable(TestBench_obj);

            switch (mode)
            {
                case Mode.EDA:
                    var eagleSch = GenerateSchematicCode(TestBench_obj);
                    CopyBoardFilesSpecifiedInPcbComponent(TestBench_obj);
                    CopyBoardFilesSpecifiedInTestBench(TestBench_obj);     // copy DRU/board template file if the testbench has it specified
                    GenerateLayoutCodeResult glcResult = GenerateLayoutCode(eagleSch, TestBench_obj);    // MOT-782
                    result.bonesFound = glcResult.bonesFound;
                    var layout = glcResult.boardLayout;
                    GenerateChipFitCommandFile();
                    GenerateShowChipFitResultsCommandFile();
                    GeneratePlacementCommandFile();
                    GeneratePlaceOnlyCommandFile();
                    GenerateLayoutReimportFiles(layout);
                    result.runCommandArgs = GenerateCommandArgs(TestBench_obj);
                    break;
                case Mode.SPICE_SI:
                    // parse and map the nets to ports
                    signalIntegrityLayout = new Layout.LayoutParser("layout.json", Logger, this)
                    {
                        mode = this.mode
                    };
                    signalIntegrityLayout.BuildMaps();

                    // spice code generator uses the mapped traces
                    // to generate subcircuits for traces and inserts them appropriately
                    GenerateSpiceCode(TestBench_obj);
                    GenerateSpiceCommandFile(TestBench_obj);
                    break;
                case Mode.SPICE:
                    GeneratePopulateTemplateScriptFile();
                    GenerateSpiceCode(TestBench_obj);
                    GenerateSpiceCommandFile(TestBench_obj);
                    GenerateSpiceViewerLauncher();
                    break;
                default:
                    throw new NotSupportedException(String.Format("Mode {0} is not supported", mode.ToString()));
            }

            return result;
        }
    }
}
