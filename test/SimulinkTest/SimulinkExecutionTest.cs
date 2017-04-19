using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GME.MGA;
using SchematicUnitTests;
using Xunit;

namespace SimulinkTest
{
    public class SimulinkExecutionTest : InterpreterTestBaseClass, IUseFixture<InterpreterTestFixture>
    {
        #region Fixture
        InterpreterTestFixture fixture;
        public void SetFixture(InterpreterTestFixture data)
        {
            fixture = data;
        }

        public override MgaProject project
        {
            get
            {
                return fixture.proj;
            }
        }

        public override String TestPath
        {
            get
            {
                return fixture.path_Test;
            }
        }
        #endregion

        [SkipWithoutMatlabFact]
        public void PassTest()
        {
            Assert.True(true);
        }
    }
}
