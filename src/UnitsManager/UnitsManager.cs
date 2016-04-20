using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
// using avm;
using CyPhyML = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace UnitsManager
{
    using TypePair = KeyValuePair<Type, Type>;

    public static class UnitsMap
    {
        public enum MessageType { OUT, ERROR, WARNING, INFO };

        private static bool isInitialized = false;

        private static List<CyPhyML.Units> _cyPhyMLUnitsFolders;
        private static CyPhyML.RootFolder _cyPhyMLRootFolder;


        private static Dictionary<String, CyPhyML.unit> _unitSymbolCyPhyMLUnitMap = new Dictionary<string, CyPhyML.unit>();


        public static void init(CyPhyML.RootFolder rootFolder)
        {
            if (!isInitialized)
            {
                // Save the rootFolder
                _cyPhyMLRootFolder = rootFolder;

                // Add units to the dictionary
                getCyPhyMLUnits();
                getCyPhyMLNamedUnits();

                isInitialized = true;
            }
        }

        public static CyPhyML.unit getCyPhyMLUnitFromString(string units, CyPhyML.RootFolder rootFolder)
        {
            CyPhyML.unit rVal = null;
            init( rootFolder );

            if (!String.IsNullOrWhiteSpace(units))
            {
                if (_unitSymbolCyPhyMLUnitMap.ContainsKey(units))
                {
                    rVal = _unitSymbolCyPhyMLUnitMap[units];
                }
                else
                {
                    // writeMessage(String.Format("WARNING: No unit lib match found for: {0}", units), MessageType.WARNING);
                }
            }

            return rVal;
        }

        private static void getCyPhyMLUnits(CyPhyML.RootFolder rootFolder)
        {
            foreach (CyPhyML.TypeSpecifications typeSpecifications in rootFolder.Children.TypeSpecificationsCollection)
            {
                foreach (CyPhyML.Units units in typeSpecifications.Children.UnitsCollection)
                {
                    _cyPhyMLUnitsFolders.Add(units);
                }
            }
        }

        private static void getCyPhyMLUnits()
        {
            _cyPhyMLUnitsFolders = new List<CyPhyML.Units>();

            // Collect all of the Root Folders in the project.
            // They will be sorted, with the QUDT lib in front, followed by all other libs, then the user's Root Folder.
            var cyPhyMLRootFolderList = new List<CyPhyML.RootFolder>();
            cyPhyMLRootFolderList.AddRange(_cyPhyMLRootFolder.LibraryCollection
                                                             .OrderByDescending(lc => lc.Name.Equals("UnitLibrary QUDT")));
            cyPhyMLRootFolderList.Add(_cyPhyMLRootFolder);

            // Now, for each Root Folder that we gathered, go through and find all units, and add them to our master index.
            if (cyPhyMLRootFolderList.Count > 0)
            {
                cyPhyMLRootFolderList.ForEach(lrf => getCyPhyMLUnits(lrf));
            }
        }


        private static void getCyPhyMLNamedUnits(bool resetUnitLibrary = false)
        {
            if (false == _cyPhyMLUnitsFolders.Any()) return;

            // If the caller has passed in this map already
            if (resetUnitLibrary) _unitSymbolCyPhyMLUnitMap.Clear();
            if (_unitSymbolCyPhyMLUnitMap.Count > 0) return;

            foreach (CyPhyML.unit cyPhyMLUnit in _cyPhyMLUnitsFolders.SelectMany(uf => uf.Children.unitCollection))
            {
                // Angle-type measures are an exception to this rule.
                /*
				if (cyPhyMLUnit.Attributes.Abbreviation != "rad" &&
					cyPhyMLUnit.Attributes.Abbreviation != "deg" &&
					isUnitless(cyPhyMLUnit))
				{
					continue;
				}*/

                if (!_unitSymbolCyPhyMLUnitMap.ContainsKey(cyPhyMLUnit.Attributes.Abbreviation))
                {
                    _unitSymbolCyPhyMLUnitMap.Add(cyPhyMLUnit.Attributes.Abbreviation, cyPhyMLUnit);
                }
                if (!_unitSymbolCyPhyMLUnitMap.ContainsKey(cyPhyMLUnit.Attributes.Symbol))
                {
                    _unitSymbolCyPhyMLUnitMap.Add(cyPhyMLUnit.Attributes.Symbol, cyPhyMLUnit);
                }
                if (!_unitSymbolCyPhyMLUnitMap.ContainsKey(cyPhyMLUnit.Attributes.FullName))
                {
                    _unitSymbolCyPhyMLUnitMap.Add(cyPhyMLUnit.Attributes.FullName, cyPhyMLUnit);
                }
            }
        }

    }
}