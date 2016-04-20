using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Text.RegularExpressions;



namespace ArduinoPreproc
{
    class Program
    {
        static void Main(string[] args)
        {
            ArduinoSDK sdk;

            if (args == null || args.Length != 1)
            {
                Console.WriteLine("usage: ArduinoPreproc <ino-file>");
                Environment.Exit(-1);
            }

            sdk = new ArduinoSDK(@"D:\arduino-1.0.5");

            Regex importRegexp = new Regex(@"^\s*#include\s*[<""](\S+)["">]");

            Console.WriteLine(String.Format("Preprocessing {0}", args[0]));

            using (StreamReader r = new StreamReader(args[0]))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    Match m = importRegexp.Match(line);
                    if (m.Success)
                    {
                        string include = m.Groups[1].Value;
                        Console.WriteLine(String.Format("\tuses: {0}", include));
                    }
                }
            }

        }
    }

    class ArduinoLibrary
    {
        private DirectoryInfo path;
        private FileInfo[] sourceFiles;
        private FileInfo[] topHeaderFiles;

        public ArduinoLibrary(DirectoryInfo path)
        {
            this.path = path;
            topHeaderFiles = path.GetFiles("*.h");
            FileInfo[] cFiles = path.GetFiles("*.c", SearchOption.AllDirectories);
            FileInfo[] cppFiles = path.GetFiles("*.cpp", SearchOption.AllDirectories);
            sourceFiles = cFiles.Concat(cppFiles).ToArray();
        }

        public string Name
        {
            get
            {
                return path.Name;
            }
        }

        public string Path
        {
            get
            {
                return path.FullName;
            }
        }

        public IEnumerable<FileInfo> Headers
        {
            get
            {
                return topHeaderFiles;
            }
        }

        public IEnumerable<FileInfo> Sources
        {
            get
            {
                return sourceFiles;
            }
        }
    }

    class ArduinoSDK
    {
        private Dictionary<string, ArduinoLibrary> importToLibraryTable;
        private List<ArduinoLibrary> libraries;

        public ArduinoSDK(string rootSDKFolder)
        {
            importToLibraryTable = new Dictionary<string, ArduinoLibrary>();
            libraries = new List<ArduinoLibrary>();
            AddLibraries(Path.Combine(rootSDKFolder, "libraries"));
        }

        void AddLibraries(string folderPath)
        {
            DirectoryInfo folder = new DirectoryInfo(folderPath);
            foreach (var subFolder in folder.EnumerateDirectories())
            {
                ArduinoLibrary lib = new ArduinoLibrary(subFolder);
                libraries.Add(lib);
                Console.WriteLine("=====================================================");
                Console.WriteLine(String.Format("{0}: {1}", lib.Name, lib.Path));
                Console.WriteLine("-----------------------------------------------------");
                foreach (var header in lib.Headers)
                {
                    importToLibraryTable[header.Name] = lib;
                    Console.WriteLine(String.Format("\t{0}", header.Name));
                }
                Console.WriteLine("-----------------------------------------------------");
                foreach (var source in lib.Sources)
                {
                    Console.WriteLine(String.Format("\t{0}", source.Name));
                }
            }
        }
    }
}
