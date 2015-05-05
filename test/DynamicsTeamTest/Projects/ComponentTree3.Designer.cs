 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using GME.MGA;

namespace DynamicsTeamTest.Projects
{
    public class ComponentTree3Fixture : XmeImportFixture
    {
        protected override string xmeFilename
        {
            get { return Path.Combine("ComponentTree3", "ComponentTree3.xme"); }
        }
    }

    public partial class ComponentTree3 : IUseFixture<ComponentTree3Fixture>
    {
        internal string mgaFile { get { return this.fixture.mgaFile; } }
        private ComponentTree3Fixture fixture { get; set; }

        public void SetFixture(ComponentTree3Fixture data)
        {
            this.fixture = data;
        }

        //[Fact]
        //[Trait("Model", "ComponentTree3")]
        //[Trait("ProjectImport/Open", "ComponentTree3")]
        //public void ProjectXmeImport()
        //{
        //    Assert.True(File.Exists(mgaFile), "Failed to generate the mga.");
        //}

        [Fact]
        [Trait("Model", "ComponentTree3")]
        [Trait("ProjectImport/Open", "ComponentTree3")]
        public void ProjectMgaOpen()
        {
            var mgaReference = "MGA=" + mgaFile;

            MgaProject project = new MgaProject();
            project.OpenEx(mgaReference, "CyPhyML", null);
            project.Close(true);
            Assert.True(File.Exists(mgaReference.Substring("MGA=".Length)));
        }

        [Fact]
        [Trait("Model", "ComponentTree3")]
        [Trait("CheckerShouldFail", "ComponentTree3")]
        public void Fail_Testing_RICircuit_CA()
        {
            string outputDir = "Testing_RICircuit_CA";
            string testBenchPath = "/@Testing|kind=Testing|relpos=0/@RICircuit_CA|kind=TestBench|relpos=0";

            Assert.True(File.Exists(mgaFile), "Failed to generate the mga.");
            bool result = CyPhy2ModelicaRunner.Run(outputDir, mgaFile, testBenchPath);

            Assert.False(result, "CyPhy2Modelica_v2 should have failed, but did not.");
        }

        [Fact]
        [Trait("Model", "ComponentTree3")]
        [Trait("CheckerShouldFail", "ComponentTree3")]
        public void Fail_Testing_RICircuit_CARI2()
        {
            string outputDir = "Testing_RICircuit_CARI2";
            string testBenchPath = "/@Testing|kind=Testing|relpos=0/@RICircuit_CARI2|kind=TestBench|relpos=0";

            Assert.True(File.Exists(mgaFile), "Failed to generate the mga.");
            bool result = CyPhy2ModelicaRunner.Run(outputDir, mgaFile, testBenchPath);

            Assert.False(result, "CyPhy2Modelica_v2 should have failed, but did not.");
        }


    }
}

