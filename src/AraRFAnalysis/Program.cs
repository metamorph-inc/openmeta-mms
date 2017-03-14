using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using CommandLine;
using CommandLine.Text;
using JobManager;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using CSXCAD;
using System.Xml.Linq;

namespace AraRFAnalysis
{
    public class Options
    {
        public string inputFile { get; set; }

        [Option('x', "x-position", Required = true,
            HelpText = "X position measured from the PCB corner closest to the EPM(s) in mm.")]
        public float x { get; set; }

        [Option('y', "y-position", Required = true,
            HelpText = "Y position measured from the PCB corner closest to the EPM(s) in mm.")]
        public float y { get; set; }

        [Option('r', "rotation", Required = true,
            HelpText = "Rotation around the Z-axis in degrees.")]
        public float rot { get; set; }

        [Option('s', "module-size", Required = true,
            HelpText = "Module size (e.g. 1x2, 2x2).")]
        public string moduleSize { get; set; }

        [Option('a', "all-slots", DefaultValue = false,
            HelpText = "Simulate all possible Endo slot locations.")]
        public bool allSlots { get; set; }

        [Option('i', "slot-index",
            HelpText = "Endo slot index of the module. Valid slot indeces are [1|4|6|7] for 1x2 and [3|5] for 2x2 modules.")]
        public int? slotIndex { get; set; }

        [Option('n', "no-endo", DefaultValue = false,
            HelpText = "Simulate the module without the Endo.")]
        public bool excludeEndo { get; set; }

        [Option('f', "frequency", DefaultValue = 2450,
            HelpText = "Center frequency of the excitation signal in MHz.")]
        public double frequency { get; set; }

        [Option('b', "bandwidth", DefaultValue = 500,
            HelpText = "Bandwidth of the excitation signal in MHz.")]
        public double bandwidth { get; set; }

        [Option('d', "directory", DefaultValue = @".\",
            HelpText = "Directory to hold intermediate simulation files and outputs.")]
        public string outputDirectory { get; set; }

        [Option("dut-resolution", DefaultValue = 0.5,
            HelpText = "Grid resolution around the DUT in mm.")]
        public double dutResolution { get; set; }

        [Option("input-impedance", DefaultValue = 50,
            HelpText = "Input impedance in Ohm.")]
        public int impedance { get; set; }

        [Option("sar", DefaultValue = false,
            HelpText = "Run SAR simulation.")]
        public bool sarMode { get; set; }

        [ValueList(typeof(List<string>))]
        public List<string> modelFileName { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption(HelpText = "Display this help message.")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                //Heading = HeadingInfo.Default,
                //Copyright = new CopyrightInfo(false, "MetaMorph Software", 2014),
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true
            };

            //help.AddPreOptionsLine(" ");
            help.AddPreOptionsLine("Usage: " + System.AppDomain.CurrentDomain.FriendlyName + " OPTIONS FILE");
            help.AddPreOptionsLine(" ");
            help.AddPreOptionsLine("Generates and executes an Ara RF simulation with the specified OPTIONS and the antenna model defined in FILE.");

            help.AddOptions(this);

            return help;
        }

        public void ParseArguments(string[] args)
        {
            var parser = new CommandLine.Parser(i => { i.MutuallyExclusive = true; i.IgnoreUnknownArguments = true; });

            if (!parser.ParseArguments(args, this))
            {
                throw new ArgumentException(new HelpText().RenderParsingErrorsText(this, 0));
            }

            // Input file
            if (modelFileName.Count == 0)
            {
                throw new ArgumentException("Input file has not been specified.");
            }
            inputFile = modelFileName.First();

            if (!File.Exists(inputFile))
            {
                throw new ArgumentException("Input file '" + Path.GetFullPath(inputFile) + "' not found.");
            }

            if (dutResolution < 0.1 || dutResolution > 10)
            {
                throw new ArgumentException("Grid resolution " + dutResolution + " mm around the DUT is out of the 0.1 to 10 mm valid range.");
            }

            // Module size
            if (!Endo.slotIndeces.ContainsKey(moduleSize))
            {
                throw new ArgumentException("-s/--module-size option violates format. Valid formats are '1x2' and '2x2'.");
            }

            // X and Y positions
            if (x < 0 || x > 40)
            {
                throw new ArgumentException("X position " + x + " mm is out of the 0 to 40 mm valid range.");
            }

            if (y < 0 || (moduleSize == "1x2" && y > 20))
            {
                throw new ArgumentException("Y position " + y + " mm is out of the 0 to 20 mm valid range.");
            }
            else if (moduleSize == "2x2" && y > 40)
            {
                throw new ArgumentException("Y position " + y + " mm is out of the 0 to 40 mm valid range.");
            }

            // No slot should be defined without the endo
            if (excludeEndo == true)
            {
                if (allSlots == true)
                {
                    throw new ArgumentException("Both no-endo and all-slots are not permitted at the same time.");
                }
                if (slotIndex != null)
                {
                    throw new ArgumentException("Both no-endo and slot-index are not permitted at the same time.");
                }
            }

            if (allSlots == true && slotIndex != null)
            {
                throw new ArgumentException("Both all-slots and slot-index are not permitted at the same time.");
            }

            // SAR
            if (sarMode == true && excludeEndo == true)
            {
                throw new ArgumentException("SAR mode with no-endo is not permitted.");
            }

            if (sarMode == true && slotIndex == null)
            {
                throw new ArgumentException("-i/--slot-index slot index has not been specified.");
            }

            // Check slot-index against module-size
            if (!allSlots)
            {
                int[] slots = Endo.slotIndeces[moduleSize];
                if (excludeEndo == true)
                {
                    slotIndex = slots[0];
                }
                else
                {
                    if (slotIndex == null)
                    {
                        throw new ArgumentException("Slot index has not been specified.");
                    }
                    if (!slots.Contains((int)slotIndex))
                    {
                        throw new ArgumentException("Slot #" + slotIndex + " does not fit a " + moduleSize + " module.");
                    }
                }
            }
        }

        public void Print()
        {
            Console.WriteLine("Input file:     " + modelFileName.First());
            Console.WriteLine("Module size:    " + moduleSize);
            Console.WriteLine("Slot index:     " + slotIndex);
            Console.WriteLine("All-slots:      " + allSlots);
            Console.WriteLine("Endo:           " + !excludeEndo);
            Console.WriteLine("Frequency:      " + frequency + " MHz");
            Console.WriteLine("Bandwidth:      " + bandwidth + " MHz");
            Console.WriteLine("DUT resolution: " + dutResolution + " mm");
            Console.WriteLine();
        }
    }

    internal class Endo
    {
        public static Dictionary<string, int[]> slotIndeces = new Dictionary<string, int[]>()
            {
                //{"1x1", new int[] {0, 2}},
                {"1x2", new int[] {1, 4, 6, 7}},
                {"2x2", new int[] {3, 5}}
            };
        }

    public class Program
    {
        static int Main(string[] args)
        {
            var options = new Options();

            try
            {
                options.ParseArguments(args);
            }
            catch (ArgumentException e)
            {
                Console.Error.WriteLine(Environment.NewLine + "ERROR: " + e.Message);
                Console.Error.Write(options.GetUsage());
                return -1;
            }

            options.Print();
            Run(options);

            return 0;
        }

        public static void Run(Options options)
        {
            int[] slotIndeces = null;
            if (options.allSlots == true)
            {
                slotIndeces = Endo.slotIndeces[options.moduleSize];
            }
            else
            {
                slotIndeces = new int[] { (int)options.slotIndex };
            }

            foreach (int i in slotIndeces)
            {
                var t = new SimulationSetup(options);
                t.slotIndex = (uint)i;
                t.Initialize();

                string suffix;
                if (options.excludeEndo == true)
                {
                    suffix = options.moduleSize;
                }
                else
                {
                    suffix = i.ToString();
                }

                JobHandler jh = new JobHandler();
                if (options.sarMode)
                {
                    Console.WriteLine("Generating simulation files in '" + Path.Combine(options.outputDirectory, "sar-" + suffix) + "'");
                    t.GenerateSarSimulationInput(Path.Combine(options.outputDirectory, "sar-" + suffix));
                    jh.RunScript(Path.GetFullPath(Path.Combine("sar-" + suffix, "run_sar.cmd")));
                }
                else
                {
                    Console.WriteLine("Generating simulation files in '" + Path.Combine(options.outputDirectory, "dir-" + suffix) + "'");
                    t.GenerateFarFieldSimulationInput(Path.Combine(options.outputDirectory, "dir-" + suffix));
                    jh.RunScript(Path.GetFullPath(Path.Combine("dir-" + suffix, "run_farfield.cmd")));
                }
            }
        }
    }

    internal class JobHandler
    {
        public void RunScript(string filename)
        {

            var dispatch = new CyPhyMasterInterpreter.JobManagerDispatch();
            dispatch.StartJobManager(Path.GetDirectoryName(filename));

            Job j;
            JobServer manager;

            j = dispatch.CreateJob(out manager, Path.GetDirectoryName(filename));

            j.RunCommand = Path.GetFileName(filename);
            j.WorkingDirectory = Path.GetDirectoryName(filename);
            j.Title = "Ara RF Simulation";
            manager.AddJob(j);
        }
    }

    public class SimulationSetup
    {
        string inputFile;
        string outputDirectory;
        public uint slotIndex;
        double dutResolution = 0.5;
        double sarResolution = 5.0;
        string moduleSize;

        const double c0 = 299792458.0;
        double lambda;
        double lambdaMin;
        double frequency = 2400e6;
        double bandwidth = 500e6;

        readonly double unit = 1e-3;
        double x = 0.0;
        double y = 0.0;
        double rot = 0.0;
        double impedance = 50.0;

        readonly double thickness = 0.56;

        CSXCAD.XmlCompound antenna;
        CSXCAD.Ara.Endo endo;

        bool excludeEndo;

        public SimulationSetup(Options options)
        {
            inputFile = options.inputFile;
            outputDirectory = options.outputDirectory;
            excludeEndo = options.excludeEndo;
            slotIndex = (options.slotIndex == null) ? 0 : (uint)options.slotIndex; // FIXME
            x = options.x;
            y = options.y;
            rot = options.rot / 180 * Math.PI;
            frequency = options.frequency * 1e6;
            bandwidth = options.bandwidth * 1e6;
            lambda = c0 / frequency;
            lambdaMin = c0 / (frequency + bandwidth);
            moduleSize = options.moduleSize;
            impedance = options.impedance;
        }

        public void Initialize()
        {
            // Ara Endo
            endo = new CSXCAD.Ara.Endo();

            CSXCAD.Ara.Module araModule = null;
            switch (moduleSize)
            {
                case "1x2":
                    araModule = new CSXCAD.Ara.Module_1x2("dut-module");
                    break;
                case "2x2":
                    araModule = new CSXCAD.Ara.Module_2x2("dut-module");
                    break;
                default:
                    break;
            }
            endo.AddModule(slotIndex, araModule);

            antenna = new CSXCAD.XmlCompound(null, "dut-antenna", new Vector3D(x, y, thickness), rot);
            antenna.Parse(XElement.Load(inputFile));
            araModule.PCB.Add(antenna);

            // Excitation
            CSXCAD.Compound excitation = new Compound(null, "Excitation", new Vector3D(x, y, thickness), rot);
            excitation.Add(new LumpedPort(100, 
                                          1,
                                          impedance,
                                          new Vector3D(0, 0, -araModule.PCB.Thickness),
                                          new Vector3D(0, 0, 0),
                                          ENormDir.Z, 
                                          true));
            araModule.PCB.Add(excitation);
        }

        public void GenerateFarFieldSimulationInput(string outputDirectory)
        {
            Compound simulationSpace = new Compound("space");
            Compound solidSpace = new Compound("solid-space");
            simulationSpace.Add(solidSpace);

            Compound dut;
            if (excludeEndo == true)
            {
                dut = endo.GetModule(slotIndex);
            }
            else
            {
                dut = endo;
            }
            solidSpace.Add(dut); // modifies dut parent (!)

            double airBox = 40;
            double maxRes = Math.Round(lambdaMin / 20 / unit);
            double maxRatio = 1.5;

            RectilinearGrid grid = new RectilinearGrid();
            grid.Add(dut.BoundingBox.P1);
            grid.Add(dut.BoundingBox.P2);

            Vector3D antennaPosition = new Vector3D(
                antenna.AbsoluteTransformation.X,
                antenna.AbsoluteTransformation.Y,
                antenna.AbsoluteTransformation.Z);

            solidSpace.Transformations.Add(new TTranslate(-antennaPosition));

            grid.Move(-antennaPosition);
            grid.Add(new Vector3D(0, 0, 0));
            grid.ZLines.Add(-(antenna.Parent as CSXCAD.Ara.PCB).Thickness);
            grid.Sort();

            grid.SmoothMesh(dutResolution, maxRatio);
            grid.AddAirbox(airBox);
            grid.SmoothMesh(maxRes, maxRatio);

            simulationSpace.Add(new NF2FFBox("nf2ff",
                new Vector3D(grid.XLines.First(), grid.YLines.First(), grid.ZLines.First()),
                new Vector3D(grid.XLines.Last(), grid.YLines.Last(), grid.ZLines.Last()),
                lambdaMin / 15 / unit));

            grid.AddPML(8);

            Simulation fdtd = new Simulation();
            fdtd.Excitation = new GaussExcitation(frequency, bandwidth);

            // Export
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("CyPhy generated openEMS simulation file"),
                new XElement("openEMS",
                    fdtd.ToXElement(),
                    new XElement("ContinuousStructure",
                        new XAttribute("CoordSystem", 0),
                        simulationSpace.ToXElement(),
                        grid.ToXElement()
                    )
                )
            );

            if (dut is CSXCAD.Ara.Module)
            {
                dut.Parent = endo;
            }

            System.IO.Directory.CreateDirectory(outputDirectory);
            string openEmsInput = Path.Combine(outputDirectory, "openEMS_input.xml");
            doc.Save(openEmsInput);

            string nf2ffInput = Path.Combine(outputDirectory, "nf2ff_input.xml");
            var nf2ff = new Postprocess.NF2FF(frequency);
            nf2ff.ToXDocument().Save(nf2ffInput);

            File.WriteAllText(Path.Combine(outputDirectory, "run_farfield.cmd"), AraRFAnalysis.Properties.Resources.run_farfield);
        }

        public void GenerateSarSimulationInput(string outputDirectory)
        {
            Compound simulationSpace = new Compound("space");
            Compound solidSpace = new Compound("solid-space");
            simulationSpace.Add(solidSpace);
            solidSpace.Add(endo);

            var headPhantom = new CSXCAD.Ara.HeadPhantom();
            headPhantom.Transformations.Add(new TRotateX(Math.PI / 2));
            headPhantom.Transformations.Add(new TTranslate(32.0, 80.0, -headPhantom.Width / 2 - 7.0)); // TODO: Make endo width/height accessibles
            solidSpace.Add(headPhantom);

            double airBox = 40;
            double envResolution = Math.Round(lambdaMin / 20 / unit);
            double maxRatio = 1.5;

            RectilinearGrid grid = new BoundingGrid_6x3();

            Vector3D dutPosition = new Vector3D(
                antenna.AbsoluteTransformation.X,
                antenna.AbsoluteTransformation.Y,
                antenna.AbsoluteTransformation.Z);

            solidSpace.Transformations.Add(new TTranslate(-dutPosition));

            grid.Move(-dutPosition);
            grid.Add(new Vector3D(0, 0, 0));
            grid.ZLines.Add(-(antenna.Parent as CSXCAD.Ara.PCB).Thickness);
            grid.Sort();
            grid.SmoothMesh(dutResolution, maxRatio);

            grid.Add(headPhantom.BoundingBox.P1);
            grid.Add(headPhantom.BoundingBox.P2);
            grid.SmoothMesh(sarResolution, maxRatio);

            grid.AddAirbox(airBox);
            grid.SmoothMesh(envResolution, maxRatio);

            simulationSpace.Add(new SARBox("SAR", frequency,
                new Vector3D(headPhantom.XGridPoints.First(), headPhantom.YGridPoints.First(), headPhantom.ZGridPoints.First()),
                new Vector3D(headPhantom.XGridPoints.Last(), headPhantom.YGridPoints.Last(), headPhantom.ZGridPoints.Last())));
            simulationSpace.Add(new NF2FFBox("nf2ff",
                new Vector3D(grid.XLines.First(), grid.YLines.First(), grid.ZLines.First()),
                new Vector3D(grid.XLines.Last(), grid.YLines.Last(), grid.ZLines.Last()),
                lambdaMin / 15 / unit));

            grid.AddPML(8);

            Simulation fdtd = new Simulation();
            fdtd.Excitation = new GaussExcitation(frequency, bandwidth);

            // Export
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("CyPhy generated openEMS simulation file"),
                new XElement("openEMS",
                    fdtd.ToXElement(),
                    new XElement("ContinuousStructure",
                        new XAttribute("CoordSystem", 0),
                        simulationSpace.ToXElement(),
                        grid.ToXElement()
                    )
                )
            );

            System.IO.Directory.CreateDirectory(Path.Combine(outputDirectory));
            string openEmsInput = Path.Combine(outputDirectory, "openEMS_input.xml");
            doc.Save(openEmsInput);

            string nf2ffInput = Path.Combine(outputDirectory, "nf2ff_input.xml");
            var nf2ff = new Postprocess.NF2FF(frequency);
            nf2ff.ToXDocument().Save(nf2ffInput);

            File.WriteAllText(Path.Combine(outputDirectory, "run_sar.cmd"), AraRFAnalysis.Properties.Resources.run_sar);
        }
    }
}
