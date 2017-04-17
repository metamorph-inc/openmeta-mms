using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhyComponentAuthoring.Modules;
using CyPhyGUIs;
using GME.MGA;
using Xunit;

namespace ComponentAndArchitectureTeamTest
{
    public class SimulinkConnectorFixture : IDisposable
    {
        public SimulinkModelImport.SimulinkConnector Connector { get; set; }

        public SmartLogger Logger { get; set; }

        public SimulinkConnectorFixture()
        {
            Connector = null;
            Logger = new SmartLogger();

            Logger.AddWriter(new ConsoleTextWriter());

            try
            {
                Logger.WriteInfo("Attempting to launch MATLAB COM automation server");
                Connector = new SimulinkModelImport.SimulinkConnector(Logger);
            }
            catch (Exception e)
            {
                Logger.WriteWarning("MATLAB/Simulink not found");
            }
        }

        public void Dispose()
        {
            Connector.Dispose();
            Logger.Dispose();
        }
    }

    public class SimulinkComponentImportTest : IUseFixture<SimulinkConnectorFixture>
    {
        private SimulinkConnectorFixture _fixture = null;

        public void SetFixture(SimulinkConnectorFixture data)
        {
            if (_fixture == null)
            {
                _fixture = data;
            }
        }

        [Fact]
        public void TestListSystemObjects()
        {
            if (_fixture.Connector != null)
            {
                var systemObjects = _fixture.Connector.ListSystemObjects("simulink");

                Assert.NotEmpty(systemObjects);
                Assert.Contains("simulink/Commonly Used Blocks/Mux", systemObjects);
                Assert.Contains("simulink/Commonly Used Blocks/Relational Operator", systemObjects);
                Assert.Contains("simulink/Sources/Signal Generator", systemObjects);
            }
        }

        [Fact]
        public void TestGetPortsMux()
        {
            if (_fixture.Connector != null)
            {
                IDictionary<int, string> inPorts;
                IDictionary<int, string> outPorts;

                _fixture.Connector.ListSystemObjects("simulink");
                _fixture.Connector.ListPorts("simulink/Commonly Used Blocks/Mux", out inPorts, out outPorts);

                Assert.Equal(2, inPorts.Count);
                Assert.True(inPorts.ContainsKey(1));
                Assert.Equal("", inPorts[1]);
                Assert.True(inPorts.ContainsKey(2));
                Assert.Equal("", inPorts[2]);

                Assert.Equal(1, outPorts.Count);
                Assert.True(outPorts.ContainsKey(1));
                Assert.Equal("", outPorts[1]);
            }
        }
    }
}
