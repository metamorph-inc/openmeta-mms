﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GME.CSharp;
using ISIS.GME.Common.Classes;
using ISIS.GME.Dsml.CyPhyML.Interfaces;
using String = System.String;

namespace CyPhy2Simulink.Simulink
{
    public class SimulinkModel
    {
        public List<SimulinkBlock> Blocks { get; private set; }

        public IDictionary<string, string> SimulationParams { get; private set; }

        public List<string> UserLibraries { get; private set; }

        public SimulinkModel()
        {
            Blocks = new List<SimulinkBlock>();
            SimulationParams = new Dictionary<string, string>();
            UserLibraries = new List<string>();
        }

        public SimulinkModel(TestBench testBench) : this()
        {
            //Iterate through TB to find blocks
            testBench.TraverseDFS(children => children, (child, i) =>
            {
                if (child is ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkModel)
                {
                    var block = SimulinkBlock.FromDomainModel((ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkModel)child);
                    if (block != null)
                    {
                        SimulinkGenerator.GMEConsole.Info.Write("Added block {0} ({1})", block.Name, block.BlockType);
                        Blocks.Add(block);
                    }
                }
            });

            foreach (var param in testBench.Children.ParameterCollection)
            {
                if (!param.AllDstConnections.Any() && !param.AllSrcConnections.Any())
                {
                    if (param.Name == "CopyFile")
                    {
                        //Ignore
                    } else if (param.Name == "UserLibrary"  && param.Attributes.Value != "")
                    {
                        var baseName = Path.GetFileNameWithoutExtension(param.Attributes.Value);

                        UserLibraries.Add(baseName);
                    }
                    else
                    {
                        if (param.Attributes.Value != "")
                        {
                            SimulationParams[param.Name] = param.Attributes.Value;
                        }
                    }
                }
            }
        }

        public void GenerateSimulinkModelCode(TextWriter writer)
        {
            writer.WriteLine("% Generated by CyPhy2Simulink on {0}", DateTime.Now);
            writer.WriteLine();
            writer.WriteLine("disp('Generating Simulink model; don''t close this window');");
            writer.WriteLine();

            writer.WriteLine("load_system('simulink');");
            foreach (var lib in UserLibraries)
            {
                writer.WriteLine("load_system('{0}');", lib);
            }
            writer.WriteLine("sys = CreateOrOverwriteModel('NewModel');");

            writer.WriteLine("load_system(sys);");
            writer.WriteLine();
            writer.WriteLine("try");

            foreach (var block in Blocks)
            {
                block.GenerateSimulinkBlockCode(writer);
            }

            foreach (var block in Blocks)
            {
                block.GenerateSimulinkConnectionCode(writer);
            }

            writer.WriteLine("catch me");
            writer.WriteLine("save_system();");
            writer.WriteLine("close_system();");
            writer.WriteLine("rethrow(me);");
            writer.WriteLine("end");
            writer.WriteLine("save_system();");
            writer.WriteLine("close_system();");
        }

        public void GenerateSimulinkExecutionCode(TextWriter writer)
        {
            writer.WriteLine("% Generated by CyPhy2Simulink on {0}", DateTime.Now);
            writer.WriteLine();
            writer.WriteLine("disp('Running Simulink simulation; don''t close this window');");
            writer.WriteLine();
            writer.WriteLine("load_system('NewModel');");
            writer.WriteLine("try");
            writer.Write("sim(gcs");

            foreach (var param in SimulationParams)
            {
                writer.Write(", '{0}', '{1}'", param.Key, param.Value);
            }

            writer.WriteLine(");");
            writer.WriteLine("catch me");
            writer.WriteLine("save_system();");
            writer.WriteLine("close_system();");
            writer.WriteLine("rethrow(me);");
            writer.WriteLine("end");

            writer.WriteLine("save_system();");
            writer.WriteLine("close_system();");
        }
    }

    public class SimulinkBlock
    {
        private static IDictionary<ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkModel, string> _blockNameCache = new Dictionary<ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkModel, string>();
        private static ISet<string> _usedBlockNames = new HashSet<string>();
        private static Random _random = new Random();

        public string Name { get; private set; }
        
        public string BlockType { get; private set; }

        public List<SimulinkPort> OutgoingPorts { get; private set; }

        public List<SimulinkParameter> Parameters { get; private set; }

        public SimulinkBlock(string name, string blockType)
        {
            Name = name;
            BlockType = blockType;
            OutgoingPorts = new List<SimulinkPort>();
            Parameters = new List<SimulinkParameter>();
        }

        public static SimulinkBlock FromDomainModel(ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkModel domainModel)
        {
            var result = new SimulinkBlock(GetBlockName(domainModel), domainModel.Attributes.BlockType);

            foreach (var param in domainModel.Children.SimulinkParameterCollection)
            {
                var simulinkParam = SimulinkParameter.FromDomainParameter(param);
                if (simulinkParam != null)
                {
                    result.Parameters.Add(simulinkParam);
                }
            }

            foreach (var port in domainModel.Children.SimulinkPortCollection)
            {
                var simulinkPort = SimulinkPort.FromDomainPort(port);
                if (simulinkPort != null)
                {
                    result.OutgoingPorts.Add(simulinkPort);
                }
            }

            return result;
        }

        public static string GetBlockName(ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkModel domainModel)
        {
            if (_blockNameCache.ContainsKey(domainModel))
            {
                return _blockNameCache[domainModel];
            }
            else
            {

                var originalCandidateName = domainModel.ParentContainer.Name;
                var candidateName = originalCandidateName;

                if (string.IsNullOrWhiteSpace(candidateName))
                {
                    throw new ArgumentException(string.Format("Parent of SimulinkModel '{0}' has no name", domainModel.Name));
                }

                while (_usedBlockNames.Contains(candidateName))
                {
                    candidateName = string.Format("{0}-{1:X}", originalCandidateName, _random.Next(0xFFFFFF));
                }

                _usedBlockNames.Add(candidateName);
                _blockNameCache[domainModel] = candidateName;

                return candidateName;
            }
        }

        public void GenerateSimulinkBlockCode(TextWriter writer)
        {
            writer.WriteLine("add_block('{0}', [gcs, '/{1}']);", BlockType, Name);

            foreach (var param in Parameters)
            {
                if (param.TestBenchParameterName == null)
                {
                    if (!string.IsNullOrWhiteSpace(param.Value))
                    {
                        writer.WriteLine("set_param([gcs, '/{0}'], '{1}', '{2}');", Name, param.Name, param.Value);
                    }
                }
                else
                {
                    writer.WriteLine("set_param([gcs, '/{0}'], '{1}', '${{{2}}}');", Name, param.Name, param.TestBenchParameterName);
                }
            }
        }

        public void GenerateSimulinkConnectionCode(TextWriter writer)
        {
            foreach (var port in OutgoingPorts)
            {
                foreach (var inputPortName in port.ConnectedInputPorts)
                {
                    writer.WriteLine("add_line(gcs, '{0}/{1}', '{2}');", Name, port.Name, inputPortName);
                }
            }
        }

        public static void ResetBlockNameCache()
        {
            _blockNameCache.Clear();
            _usedBlockNames.Clear();
        }
    }

    public class SimulinkPort
    {
        public string Name { get; private set; }

        public List<string> ConnectedInputPorts { get; private set; }

        public SimulinkPort(string name)
        {
            Name = name;
            ConnectedInputPorts = new List<string>();
        }

        public static SimulinkPort FromDomainPort(ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort port)
        {
            if (port.Attributes.SimulinkPortDirection != ISIS.GME.Dsml.CyPhyML.Classes.SimulinkPort.AttributesClass.SimulinkPortDirection_enum.@out)
            {
                return null;
            }
            else
            {
                var result = new SimulinkPort(GetPortId(port));
                result.AddConnectedInputPorts(port, new HashSet<ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort>());

                return result;
            }
        }

        private void AddConnectedInputPorts(ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort port, ISet<ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort> visited)
        {
            visited.Add(port);

            if (port.Attributes.SimulinkPortDirection == ISIS.GME.Dsml.CyPhyML.Classes.SimulinkPort.AttributesClass.SimulinkPortDirection_enum.@in && port.ParentContainer is ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkModel)
            {
                var parentComponentName =
                    SimulinkBlock.GetBlockName((ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkModel) port.ParentContainer);
                var portId = GetPortId(port);
                ConnectedInputPorts.Add(string.Format("{0}/{1}", parentComponentName, portId));
                SimulinkGenerator.GMEConsole.Info.WriteLine("Connection: {0}/{1}", parentComponentName, portId);
            }

            foreach (var connection in port.SrcConnections.PortCompositionCollection)
            {
                var adjacent = connection.SrcEnd;
                if (adjacent is ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort && !visited.Contains(adjacent))
                {
                    AddConnectedInputPorts((ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort) adjacent, visited);
                }
            }

            foreach (var connection in port.DstConnections.PortCompositionCollection)
            {
                var adjacent = connection.DstEnd;
                if (adjacent is ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort && !visited.Contains(adjacent))
                {
                    AddConnectedInputPorts((ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort) adjacent, visited);
                }
            }
        }

        private static string GetPortId(ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkPort port)
        {
            var portId = port.Attributes.SimulinkPortID;
            if (string.IsNullOrWhiteSpace(portId))
            {
                throw new ArgumentException(String.Format("Port '{0}' in object '{1}' has no SimulinkPortID", port.Name, port.ParentContainer.Name));
            }
            return portId;
        }
    }

    public class SimulinkParameter
    {
        public string Name { get; private set; }
        
        public string Value { get; private set; }

        public string TestBenchParameterName { get; private set; }

        public SimulinkParameter(string name, string value, string testBenchParameterName)
        {
            Name = name;
            Value = value;
            TestBenchParameterName = testBenchParameterName;
        }

        public static SimulinkParameter FromDomainParameter(ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkParameter param)
        {
            var value = GetAdjacentParameterValue(param);
            var tbParamName = TryGetTestbenchParamName((FCO) param, new HashSet<FCO>());

            if (value != null)
            {
                return new SimulinkParameter(param.Name, value, tbParamName);
            }
            else
            {
                return null;
            }
        }

        private static string GetAdjacentParameterValue(ISIS.GME.Dsml.CyPhyML.Interfaces.SimulinkParameter param)
        {
            var connections = param.SrcConnections.SimulinkParameterPortMapCollection.ToList();

            if (connections.Count() != 1)
            {
                return null;
            }
            else
            {
                var source = connections.First().SrcEnd;

                if (source is Parameter)
                {
                    return ((Parameter) source).Attributes.Value;
                }
                else if (source is Property)
                {
                    return ((Property) source).Attributes.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        private static string TryGetTestbenchParamName(FCO fco, ISet<FCO> visited)
        {
            if (fco.ParentContainer.Kind == "TestBench") //Note: We're not guaranteed to be working with domain-specific objects here
            {
                SimulinkGenerator.GMEConsole.Info.WriteLine("Testbench parameter found: {0}", fco.Name);
                return fco.Name;
            }

            visited.Add(fco);

            foreach (var connection in fco.AllDstConnections)
            {
                var adjacent = (FCO) connection.GenericDstEnd; //Cast to abstract FCO class, rather than interface (hopefully this always works?)
                if ((adjacent.Kind == "Parameter" || adjacent.Kind == "Property") && !visited.Contains(adjacent)) //Note: GenericDstEnd doesn't give domain-specific objects
                {                                                                // (these are instances of FCO)
                    var result = TryGetTestbenchParamName(adjacent, visited);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            foreach (var connection in fco.AllSrcConnections)
            {
                var adjacent = (FCO)connection.GenericSrcEnd; //Cast to abstract FCO class, rather than interface (hopefully this always works?)
                if ((adjacent.Kind == "Parameter" || adjacent.Kind == "Property") && !visited.Contains(adjacent))
                {
                    var result = TryGetTestbenchParamName(adjacent, visited);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}