using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.Globalization;

namespace BinPackTest
{
    public class TBManifestFixture
    {
        public TBManifestFixture()
        {
            Assert.True(File.Exists(TBManifest.pathExecutable),
                        String.Format("MaxRectsBinPack executable could not be found at {0}",
                                      TBManifest.pathExecutable));
        }
    }

    public class TBManifest : IUseFixture<TBManifestFixture>
    {
        #region Fixture
        TBManifestFixture fixture;
        public void SetFixture(TBManifestFixture data)
        {
            fixture = data;
        }
        #endregion

        String pathTest = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                "..", "..", "testfiles");
        public static String pathExecutable = Path.Combine(META.VersionInfo.MetaPath,
                                                           "bin",
                                                           "MaxRectsBinPack.exe");

        private void runCommand(String nameManifest)
        {
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = pathExecutable,
                    Arguments = "layout-input.json " + nameManifest,
                    WorkingDirectory = pathTest,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Give it 30 seconds to execute.
            Assert.True(process.WaitForExit(30*1000),
                        "MaxRectsBinPack timed out (over 30 seconds)");

            Assert.Equal(0, process.ExitCode);
        }

        [Fact]
        public void RunsWithoutManifestMod()
        {
            runCommand("");
        }

        [Fact]
        public void NoMetrics()
        {
            String pathOriginalManifest = Path.Combine(pathTest,
                                                       "org_tb_manifest.no_metrics.json");
            String pathManifestCopy = Path.Combine(pathTest,
                                                   "tb_manifest.no_metrics.json");
            File.Copy(pathOriginalManifest, pathManifestCopy, true);
            Assert.True(File.Exists(pathManifestCopy));

            runCommand("tb_manifest.no_metrics.json");

            String json = File.ReadAllText(pathManifestCopy);
            var manifest = JsonConvert.DeserializeObject<AVM.DDP.MetaTBManifest>(json);

            Assert.Equal(2, manifest.Metrics.Count);

            var fits = manifest.Metrics.First(m => m.Name.StartsWith("fits_"));
            Assert.NotNull(fits);
            Assert.Equal("true", fits.Value);

            var occupied = manifest.Metrics.First(m => m.Name.StartsWith("pct_occupied_"));
            Assert.NotNull(occupied);
            Assert.DoesNotThrow(() => float.Parse(occupied.Value, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void OldValues()
        {
            String pathOriginalManifest = Path.Combine(pathTest,
                                                       "org_tb_manifest.old_values.json");
            String pathManifestCopy = Path.Combine(pathTest,
                                                   "tb_manifest.old_values.json");
            File.Copy(pathOriginalManifest, pathManifestCopy, true);
            Assert.True(File.Exists(pathManifestCopy));

            runCommand("tb_manifest.old_values.json");

            String json = File.ReadAllText(pathManifestCopy);
            var manifest = JsonConvert.DeserializeObject<AVM.DDP.MetaTBManifest>(json);

            Assert.Equal(2, manifest.Metrics.Count);

            var fits = manifest.Metrics.First(m => m.Name.StartsWith("fits_"));
            Assert.NotNull(fits);
            Assert.Equal("true", fits.Value);

            var occupied = manifest.Metrics.First(m => m.Name.StartsWith("pct_occupied_"));
            Assert.NotNull(occupied);
            Assert.DoesNotThrow(() => float.Parse(occupied.Value, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ManifestDoesNotExist()
        {
            String pathManifest = Path.Combine(pathTest,
                                               "manifest_does_not_exists.json");

            runCommand("tb_manifest.old_values.json");
        }
    }
}
