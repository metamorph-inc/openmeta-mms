using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GME.MGA;
using Xunit;
using System.IO;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using GME.CSharp;

namespace ConnectorUnrollTest
{
    public class UnrollTestFixture : IDisposable
    {
        public string mgaPath;
        public string xmePath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                            "..\\..\\..",
                                             "ConnectorUnrollTest",
                                             "Model",
                                             "ConnectorUnrollTest.xme");

        public UnrollTestFixture()
        {
            try
            {
                String mgaConnectionString;
                GME.MGA.MgaUtils.ImportXMEForTest(xmePath, out mgaConnectionString);
                mgaPath = mgaConnectionString.Substring("MGA=".Length);

                Assert.True(File.Exists(Path.GetFullPath(mgaPath)),
                            String.Format("{0} not found. Model import may have failed.", mgaPath));

                _proj = new MgaProject();
                bool ro_mode;
                proj.Open("MGA=" + Path.GetFullPath(mgaPath), out ro_mode);
                proj.EnableAutoAddOns(true);
            }
            catch (Exception e)
            {
                importException = e;
            }
        }

        public void Dispose()
        {
            if (_proj != null)
            {
                _proj.Save();
                _proj.Close();
                _proj = null;
            }
        }

        private MgaProject _proj;
        public MgaProject proj
        {
            get
            {
                if (importException != null)
                {
                    throw new Exception("Xme import failed", importException);
                }
                return _proj;
            }
        }

        private Exception importException;
    }

    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del)
        {
            var mgaGateway = new MgaGateway(project);
            project.CreateTerritoryWithoutSink(out mgaGateway.territory);
            mgaGateway.PerformInTransaction(del);
        }
    }

    public class UnrollTest : IUseFixture<UnrollTestFixture>
    {
        #region Fixture
        UnrollTestFixture fixture;
        public void SetFixture(UnrollTestFixture data)
        {
            fixture = data;
        }
        #endregion

        private MgaProject proj
        {
            get
            {
                return fixture.proj;
            }
        }

        [Fact]
        public void ComponentAssembly_Basic()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@Basic|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly ca = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var caObj = proj.get_ObjectByPath(path);
                ca = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(caObj);
                
                var connector = ca.Children.ConnectorCollection.First();
                var connPorts = connector.Children.PortCollection;
                numConnPorts = connPorts.Where(p => IsSupported(p)).Count();

                Unroll(ca);
            });

            proj.PerformInTransaction(delegate
            {
                var caPorts = ca.Children.PortCollection;

                var violations = new List<String>();
                foreach (var port in caPorts)
                {
                    if (false == CyPhyElaborateCS.Unroller.SupportedPortTypes.Contains(port.Kind))
                    {
                        violations.Add(String.Format("Port {0} of kind {1} should not have been supported", port.Name, port.Kind));
                    }
                }
                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }

                Assert.Equal(numConnPorts,
                             caPorts.Where(p => IsSupported(p)).Count());
            });
        }

        [Fact]
        public void ComponentAssembly_Basic_SubAsm()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@Basic_SubAsm|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topCa = null;
            CyPhy.ComponentAssembly subCa = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var topCaObj = proj.get_ObjectByPath(path);
                topCa = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topCaObj);
                subCa = topCa.Children.ComponentAssemblyCollection.First();

                var connector = subCa.Children.ConnectorCollection.First();
                var connPorts = connector.Children.PortCollection;
                numConnPorts = connPorts.Where(p => IsSupported(p)).Count();

                Unroll(topCa);
            });

            proj.PerformInTransaction(delegate
            {
                var subCaPorts = subCa.Children.PortCollection;

                var violations = new List<String>();
                foreach (var port in subCaPorts)
                {
                    if (false == CyPhyElaborateCS.Unroller.SupportedPortTypes.Contains(port.Kind))
                    {
                        violations.Add(String.Format("Port {0} of kind {1} should not have been supported", port.Name, port.Kind));
                    }
                }
                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }

                Assert.Equal(numConnPorts,
                             subCaPorts.Where(p => IsSupported(p)).Count());
            });
        }

        [Fact]
        public void ComponentAssembly_SubAsm_Connected()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@SubAsm_Connected|kind=ComponentAssembly|relpos=0";
            String path_SubAsm1 = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@SubAsm_Connected|kind=ComponentAssembly|relpos=0/@SubAsm1|kind=ComponentAssembly|relpos=0";
            String path_SubAsm2 = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@SubAsm_Connected|kind=ComponentAssembly|relpos=0/@SubAsm2|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topCa = null;
            CyPhy.ComponentAssembly subAsm1 = null;
            CyPhy.ComponentAssembly subAsm2 = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var topCaObj = proj.get_ObjectByPath(path);
                topCa = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topCaObj);
                subAsm1 = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(proj.get_ObjectByPath(path_SubAsm1));
                subAsm2 = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(proj.get_ObjectByPath(path_SubAsm2));

                numConnPorts = subAsm1.Children.ConnectorCollection.First().Children.PortCollection.Where(p => IsSupported(p)).Count();
                int numConnPorts2 = subAsm2.Children.ConnectorCollection.First().Children.PortCollection.Where(p => IsSupported(p)).Count();
                Assert.Equal(numConnPorts, numConnPorts2);

                Unroll(topCa);
            });

            proj.PerformInTransaction(delegate
            {
                var ports_SubAsm1 = subAsm1.Children.PortCollection;
                Assert.Equal(numConnPorts, ports_SubAsm1.Count());

                var ports_SubAsm2 = subAsm2.Children.PortCollection;
                Assert.Equal(numConnPorts, ports_SubAsm2.Count());

                // All ports should be connected.
                List<String> violations = new List<String>();
                foreach (var port1 in ports_SubAsm1)
                {
                    var port2 = ports_SubAsm2.First(p => p.Name == port1.Name);

                    var conn1 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == port1.ID && pc.DstEnds.Port.ID == port2.ID);
                    var conn2 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == port2.ID && pc.DstEnds.Port.ID == port1.ID);

                    if (conn1 == null && conn2 == null)
                    {
                        violations.Add(String.Format("Connection missing between {0} ports", port1.Name));
                    }
                }
                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }
            });
        }

        [Fact]
        public void ComponentAssembly_CompInstAndRef()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@CompInstAndRef|kind=ComponentAssembly|relpos=0";
            CyPhy.ComponentAssembly topCa = null;
            proj.PerformInTransaction(delegate
            {
                var topCaObj = proj.get_ObjectByPath(path);
                topCa = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topCaObj);
                Unroll(topCa);
            });

            proj.PerformInTransaction(delegate
            {
                var comp1 = topCa.Children.ComponentCollection.First(c => c.Name == "Basic1");
                var comp2 = topCa.Children.ComponentCollection.First(c => c.Name == "Basic2");

                var ports_Comp1 = comp1.Children.PortCollection;
                var ports_Comp2 = comp2.Children.PortCollection;

                // All ports should be connected.
                List<String> violations = new List<String>();
                foreach (var port1 in ports_Comp1)
                {
                    var port2 = ports_Comp2.First(p => p.Name == port1.Name);

                    var conn1 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == port1.ID && pc.DstEnds.Port.ID == port2.ID);
                    var conn2 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == port2.ID && pc.DstEnds.Port.ID == port1.ID);

                    if (conn1 == null && conn2 == null)
                    {
                        violations.Add(String.Format("Connection missing between {0} ports", port1.Name));
                    }
                }
                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }
            });
        }
        
        [Fact]
        public void ComponentAssembly_SubAsm_Connections()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@SubAsm_Connections|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topCa = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var topCaObj = proj.get_ObjectByPath(path);
                topCa = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topCaObj);
                
                var connector = topCa.Children.ConnectorCollection.First();
                numConnPorts = connector.Children.PortCollection.Where(p => IsSupported(p)).Count();

                Unroll(topCa);
            });

            proj.PerformInTransaction(delegate
            {
                var topCaPorts = topCa.Children.PortCollection;
                Assert.Equal(numConnPorts, topCaPorts.Count());

                var subCa = topCa.Children.ComponentAssemblyCollection.First();
                var subCaPorts = subCa.Children.PortCollection;
                Assert.Equal(numConnPorts, subCaPorts.Count());
                
                List<String> violations = new List<String>();
                foreach (var topPort in topCaPorts)
                {
                    var subPort = subCaPorts.First(p => p.Name == topPort.Name);

                    var conn1 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == topPort.ID && pc.DstEnds.Port.ID == subPort.ID);
                    var conn2 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == subPort.ID && pc.DstEnds.Port.ID == topPort.ID);

                    if (conn1 == null && conn2 == null)
                    {
                        violations.Add(String.Format("No connection found for {0}", topPort.Name));
                    }
                }
                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }
            });
        }

        [Fact]
        public void ComponentAssembly_SubAsm_and_Component()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@SubAsm_and_Comp|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topCa = null;
            CyPhy.ComponentAssembly subCa = null;
            CyPhy.Component comp = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var topCaObj = proj.get_ObjectByPath(path);
                topCa = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topCaObj);

                subCa = topCa.Children.ComponentAssemblyCollection.First();
                comp = topCa.Children.ComponentCollection.First();

                numConnPorts = subCa.Children.ConnectorCollection.First().Children.PortCollection.Where(p => IsSupported(p)).Count();
                int numConnPorts2 = comp.Children.ConnectorCollection.First().Children.PortCollection.Where(p => IsSupported(p)).Count();
                Assert.Equal(numConnPorts, numConnPorts2);

                Unroll(topCa);
            });

            proj.PerformInTransaction(delegate
            {
                var subCaPorts = subCa.Children.PortCollection;
                Assert.Equal(numConnPorts, subCaPorts.Count());

                var compPorts = comp.Children.PortCollection;
                Assert.Equal(numConnPorts, compPorts.Count());
                
                // All ports should be connected.
                List<String> violations = new List<String>();
                foreach (var port1 in subCaPorts)
                {
                    var port2 = compPorts.First(p => p.Name == port1.Name);

                    var conn1 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == port1.ID && pc.DstEnds.Port.ID == port2.ID);
                    var conn2 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == port2.ID && pc.DstEnds.Port.ID == port1.ID);

                    if (conn1 == null && conn2 == null)
                    {
                        violations.Add(String.Format("Connection missing between {0} ports", port1.Name));
                    }
                }
                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }
            });
        }

        //[Fact]
        public void TestBench_WithComp()
        {
            String path = "/@TestBenches|kind=Testing|relpos=0/@WithComp|kind=TestBench|relpos=0";

            CyPhy.TestBench tb = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var tbObj = proj.get_ObjectByPath(path);
                tb = ISIS.GME.Dsml.CyPhyML.Classes.TestBench.Cast(tbObj);

                numConnPorts = tb.Children.TestComponentCollection.First()
                                 .Children.ConnectorCollection.First()
                                 .Children.PortCollection.Where(p => IsSupported(p)).Count();
                Assert.True(numConnPorts > 0);

                Unroll(tb);
            });

            proj.PerformInTransaction(delegate
            {
                var comp = tb.Children.ComponentCollection.First();
                var tc = tb.Children.TestComponentCollection.First();

                var compPorts = comp.Children.PortCollection;
                var tcPorts = tc.Children.PortCollection;

                var tbPortComp = tb.Children.PortCompositionCollection;

                Assert.Equal(numConnPorts, compPorts.Count());
                Assert.Equal(numConnPorts, tcPorts.Count());
                Assert.Equal(numConnPorts, tbPortComp.Count());

                foreach (var portComp in tbPortComp)
                {
                    Assert.True(portComp.SrcEnds.Port.Name == portComp.DstEnds.Port.Name);
                    Assert.True(portComp.SrcEnds.Port.Kind == portComp.DstEnds.Port.Kind);
                }
            });
        }

        [Fact]
        public void TestBench_WithAsm()
        {
            String path = "/@TestBenches|kind=Testing|relpos=0/@WithAsm|kind=TestBench|relpos=0";

            CyPhy.TestBench tb = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var tbObj = proj.get_ObjectByPath(path);
                tb = ISIS.GME.Dsml.CyPhyML.Classes.TestBench.Cast(tbObj);

                numConnPorts = tb.Children.TestComponentCollection.First()
                                 .Children.ConnectorCollection.First()
                                 .Children.PortCollection.Where(p => IsSupported(p)).Count();
                Assert.True(numConnPorts > 0);

                Unroll(tb);
            });

            proj.PerformInTransaction(delegate
            {
                var asm = tb.Children.ComponentAssemblyCollection.First();
                var subAsm = asm.Children.ComponentAssemblyCollection.First();
                var tc = tb.Children.TestComponentCollection.First();

                var asmPorts = asm.Children.PortCollection;
                var subAsmPorts = subAsm.Children.PortCollection;
                var tcPorts = tc.Children.PortCollection;
                
                var tbPortComp = tb.Children.PortCompositionCollection;
                var asmPortComp = asm.Children.PortCompositionCollection;

                Assert.Equal(numConnPorts, asmPorts.Count());
                Assert.Equal(numConnPorts, subAsmPorts.Count());
                Assert.Equal(numConnPorts, tcPorts.Count());
                Assert.Equal(numConnPorts, tbPortComp.Count());
                Assert.Equal(numConnPorts, asmPortComp.Count());

                foreach (var portComp in tbPortComp)
                {
                    Assert.True(portComp.SrcEnds.Port.Name == portComp.DstEnds.Port.Name);
                    Assert.True(portComp.SrcEnds.Port.Kind == portComp.DstEnds.Port.Kind);
                }
                foreach (var portComp in asmPortComp)
                {
                    Assert.True(portComp.SrcEnds.Port.Name == portComp.DstEnds.Port.Name);
                    Assert.True(portComp.SrcEnds.Port.Kind == portComp.DstEnds.Port.Kind);
                }
            });
        }

        [Fact]
        public void SubAsm_and_Comp_and_Conn()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@SubAsm_and_Comp_and_Conn|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topCa = null;
            CyPhy.ComponentAssembly subCa = null;
            CyPhy.Component comp = null;
            int? numConnPorts = null;

            proj.PerformInTransaction(delegate
            {
                var topCaObj = proj.get_ObjectByPath(path);
                topCa = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topCaObj);
                subCa = topCa.Children.ComponentAssemblyCollection.First();
                comp = topCa.Children.ComponentCollection.First();

                numConnPorts = topCa.Children.ConnectorCollection.First().Children.PortCollection.Where(p => IsSupported(p)).Count();

                Unroll(topCa);
            });

            proj.PerformInTransaction(delegate
            {
                var topCaPorts = topCa.Children.PortCollection;
                var subCaPorts = subCa.Children.PortCollection;
                var compPorts = comp.Children.PortCollection;

                Assert.Equal(numConnPorts, topCaPorts.Count());
                Assert.Equal(numConnPorts, subCaPorts.Count());
                Assert.Equal(numConnPorts, compPorts.Count());

                // All ports should be connected.
                List<String> violations = new List<String>();
                foreach (var port1 in subCaPorts.Union(topCaPorts))
                {
                    var port2 = compPorts.First(p => p.Name == port1.Name);

                    var conn1 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == port1.ID && pc.DstEnds.Port.ID == port2.ID);
                    var conn2 = topCa.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.Port.ID == port2.ID && pc.DstEnds.Port.ID == port1.ID);

                    if (conn1 == null && conn2 == null)
                    {
                        violations.Add(String.Format("Connection missing between {0} and {1}", port1.Path, port2.Path));
                    }
                }

                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }
            });
        }

        [Fact]
        public void Mismatch_1OfEachType()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@Mismatch_1OfEachType|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topAsm = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var topAsmObj = proj.get_ObjectByPath(path);
                topAsm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topAsmObj);

                var connector = topAsm.Children.ComponentAssemblyCollection.First()
                                      .Children.ConnectorCollection.First();
                numConnPorts = connector.Children.PortCollection.Where(p => IsSupported(p)).Count();

                Unroll(topAsm);
            });

            proj.PerformInTransaction(delegate
            {
                var subAsm1 = topAsm.Children.ComponentAssemblyCollection.First(ca => ca.Name == "SubAsm1");
                var subAsm2 = topAsm.Children.ComponentAssemblyCollection.First(ca => ca.Name == "SubAsm2");

                Assert.Equal(numConnPorts, subAsm1.Children.PortCollection.Count());
                Assert.Equal(numConnPorts, subAsm2.Children.PortCollection.Count());
                Assert.Equal(numConnPorts, topAsm.Children.PortCompositionCollection.Count());

                foreach (var pc in topAsm.Children.PortCompositionCollection)
                {
                    Assert.True(pc.SrcEnds.Port.Kind == pc.DstEnds.Port.Kind);
                }
            });
        }

        [Fact]
        public void Mismatch_TypesDontMatch()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@Mismatch_TypesDontMatch|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topAsm = null;
            proj.PerformInTransaction(delegate
            {
                var topAsmObj = proj.get_ObjectByPath(path);
                topAsm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topAsmObj);

                Unroll(topAsm);
            });

            proj.PerformInTransaction(delegate
            {
                var subAsm1 = topAsm.Children.ComponentAssemblyCollection.First(ca => ca.Name == "SubAsm1");
                var subAsm2 = topAsm.Children.ComponentAssemblyCollection.First(ca => ca.Name == "SubAsm2");

                Assert.Equal(1, subAsm1.Children.PortCollection.Count());
                Assert.Equal(1, subAsm2.Children.PortCollection.Count());
                Assert.Equal(0, topAsm.Children.PortCompositionCollection.Count());
            });
        }

        [Fact]
        public void Mismatch_NonUniqueTypes()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@Mismatch_NonUniqueTypes|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topAsm = null;
            proj.PerformInTransaction(delegate
            {
                var topAsmObj = proj.get_ObjectByPath(path);
                topAsm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topAsmObj);

                Unroll(topAsm);
            });

            proj.PerformInTransaction(delegate
            {
                var subAsm1 = topAsm.Children.ComponentAssemblyCollection.First(ca => ca.Name == "SubAsm1");
                var subAsm2 = topAsm.Children.ComponentAssemblyCollection.First(ca => ca.Name == "SubAsm2");

                Assert.Equal(1, subAsm1.Children.PortCollection.Count());
                Assert.Equal(2, subAsm2.Children.PortCollection.Count());
                Assert.Equal(0, topAsm.Children.PortCompositionCollection.Count());
            });
        }

        [Fact]
        public void Mismatch_RolesMatchButNotTypes()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@Mismatch_RolesMatchButNotTypes|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topAsm = null;
            proj.PerformInTransaction(delegate
            {
                var topAsmObj = proj.get_ObjectByPath(path);
                topAsm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topAsmObj);

                Unroll(topAsm);
            });

            proj.PerformInTransaction(delegate
            {
                var subAsm1 = topAsm.Children.ComponentAssemblyCollection.First(ca => ca.Name == "SubAsm1");
                var subAsm2 = topAsm.Children.ComponentAssemblyCollection.First(ca => ca.Name == "SubAsm2");

                Assert.Equal(1, subAsm1.Children.PortCollection.Count());
                Assert.Equal(1, subAsm2.Children.PortCollection.Count());
                Assert.Equal(0, topAsm.Children.PortCompositionCollection.Count());
            });
        }

        [Fact]
        public void Component_Basic()
        {
            String path = "/@Components|kind=Components|relpos=0/@Basic|kind=Component|relpos=0";

            CyPhy.Component comp = null;
            int? numConnPorts = null;
            IEnumerable<CyPhy.Port> connPorts = null;
            proj.PerformInTransaction(delegate
            {
                var compObj = proj.get_ObjectByPath(path);
                comp = ISIS.GME.Dsml.CyPhyML.Classes.Component.Cast(compObj);

                var connector = comp.Children.ConnectorCollection.First();
                connPorts = connector.Children.PortCollection.Where(p => IsSupported(p));
                numConnPorts = connPorts.Count();

                Unroll(comp, false);
            });

            proj.PerformInTransaction(delegate
            {
                var compPorts = comp.Children.PortCollection;
                var violations = new List<String>();
                foreach (var port in compPorts)
                {
                    if (false == CyPhyElaborateCS.Unroller.SupportedPortTypes.Contains(port.Kind))
                    {
                        violations.Add(String.Format("Port {0} of kind {1} should not have been supported", port.Name, port.Kind));
                    }
                }
                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }

                Assert.Equal(numConnPorts,
                             compPorts.Where(p => IsSupported(p)).Count());
                
                // Now check that all ports are equivalent
                var violations2 = new List<String>();
                foreach (var portOrg in connPorts)
                {
                    var portNew = compPorts.FirstOrDefault(p => p.Name.EndsWith(portOrg.Name));
                    if (portNew == null)
                    {
                        violations2.Add(String.Format("Could not find equivalent unrolled port for {0}", portOrg.Path));
                    }
                    else if (false == CompareObjects(portOrg.Impl as MgaFCO, portNew.Impl as MgaFCO))
                    {
                        violations2.Add(String.Format("Ports were not equivalent: {0} -- {1}", portOrg.Path, portNew.Path));
                    }
                }
                if (violations2.Any())
                {
                    String msg = "";
                    foreach (var violation in violations2)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }
            });
        }

        [Fact]
        public void Component_Modelica()
        {
            String path = "/@Components|kind=Components|relpos=0/@Modelica|kind=Component|relpos=0";

            CyPhy.Component comp = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var compObj = proj.get_ObjectByPath(path);
                comp = ISIS.GME.Dsml.CyPhyML.Classes.Component.Cast(compObj);

                var connector = comp.Children.ConnectorCollection.First();
                var connPorts = connector.Children.ModelicaConnectorCollection;
                numConnPorts = connPorts.Where(p => IsSupported(p)).Count();

                Unroll(comp);
            });

            proj.PerformInTransaction(delegate
            {
                Assert.False(comp.Children.ConnectorCollection.Any());

                var compPorts = comp.Children.ModelicaConnectorCollection;
                var modelicaModel = comp.Children.ModelicaModelCollection.First();
                var mmPorts = modelicaModel.Children.ModelicaConnectorCollection;

                Assert.Equal(numConnPorts, compPorts.Count());
                Assert.Equal(2, comp.Children.PortCompositionCollection.Count());

                List<String> violations = new List<String>();
                foreach (var compPort in compPorts)
                {
                    var mmPort = mmPorts.First(p => compPort.Name.EndsWith(p.Name));
                    var conn1 = comp.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.ModelicaConnector.ID == compPort.ID
                                                                                          && pc.DstEnds.ModelicaConnector.ID == mmPort.ID);
                    var conn2 = comp.Children.PortCompositionCollection.FirstOrDefault(pc => pc.SrcEnds.ModelicaConnector.ID == mmPort.ID
                                                                                          && pc.DstEnds.ModelicaConnector.ID == compPort.ID);

                    if (conn1 == null && conn2 == null)
                    {
                        violations.Add(String.Format("No connection found for port {0}", compPort.Name));
                    }
                }
                if (violations.Any())
                {
                    String msg = "";
                    foreach (var violation in violations)
                    {
                        msg += violation + Environment.NewLine;
                    }
                    Assert.True(false, msg);
                }
            });
        }
        
        [Fact]
        public void MixedComposition()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@MixedComposition|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly topAsm = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var topAsmObj = proj.get_ObjectByPath(path);
                topAsm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topAsmObj);

                var connector = topAsm.Children.ConnectorCollection.First();
                numConnPorts = connector.Children.PortCollection.Where(p => IsSupported(p)).Count();

                Unroll(topAsm);
            });

            proj.PerformInTransaction(delegate
            {
                var subAsm = topAsm.Children.ComponentAssemblyCollection.First();
                var comp = topAsm.Children.ComponentCollection.First();

                Assert.Equal(numConnPorts, topAsm.Children.PortCollection.Count());
                Assert.Equal(numConnPorts + 1, subAsm.Children.PortCollection.Where(p => IsSupported(p)).Count());

                Assert.Equal(1, topAsm.Children.SchematicModelPortCollection.Count());
                var pinTopAsm = topAsm.Children.SchematicModelPortCollection.First();
                var pinSubAsm = subAsm.Children.SchematicModelPortCollection.First(p => p.Name == "Pin");
                Assert.NotNull(pinSubAsm);
                var pinComposition = topAsm.Children.PortCompositionCollection.FirstOrDefault(pc => (pc.SrcEnds.Port.ID == pinTopAsm.ID && pc.DstEnds.Port.ID == pinSubAsm.ID)
                                                                                                 || (pc.SrcEnds.Port.ID == pinSubAsm.ID && pc.DstEnds.Port.ID == pinTopAsm.ID));
                Assert.NotNull(pinComposition);

                Assert.Equal(1, topAsm.Children.ModelicaConnectorCollection.Count());
                var mcTopAsm = topAsm.Children.ModelicaConnectorCollection.First();
                var mcSubComp = comp.Children.ModelicaConnectorCollection.First(mc => mc.Name == "MC1");
                Assert.NotNull(mcSubComp);
                Assert.NotNull(mcTopAsm);
 
                var mcComposition = topAsm.Children.PortCompositionCollection.FirstOrDefault(pc => (pc.SrcEnds.Port.ID == mcTopAsm.ID && pc.DstEnds.Port.ID == mcSubComp.ID)
                                                                                                || (pc.SrcEnds.Port.ID == mcSubComp.ID && pc.DstEnds.Port.ID == mcTopAsm.ID));
                Assert.NotNull(mcComposition);
            });
        }

        [Fact]
        public void ComponentInstanceTest()
        {
            String path = "/@CompInstanceTests|kind=Components|relpos=0/@Assembly|kind=ComponentAssemblies|relpos=0/@AsmWithInstance|kind=ComponentAssembly|relpos=0";

            CyPhy.ComponentAssembly asm = null;
            CyPhy.Component compDef = null;
            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var topAsmObj = proj.get_ObjectByPath(path);
                asm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(topAsmObj);

                compDef = asm.Children.ComponentRefCollection.First()
                                      .Referred.Component;

                var connector = compDef.Children.ConnectorCollection.First();
                numConnPorts = connector.Children.PortCollection.Where(p => IsSupported(p)).Count();

                Unroll(asm);
            });

            // What do we wanna test?
            // Well, we wanna check that the component definition has been unrolled.
            // We also wanna check that the Assembly contains two component instances.
            // Finally, we wanna make sure we have all the right connections within the assembly.
            proj.PerformInTransaction(delegate
            {
                Assert.Equal(numConnPorts, compDef.Children.PortCollection.Count(p => IsSupported(p)));

                var comp1 = asm.Children.ComponentCollection.First(c => c.Name == "CompRef1");
                var comp2 = asm.Children.ComponentCollection.First(c => c.Name == "CompRef2");

                Assert.True(comp1.IsInstance);
                Assert.True(comp2.IsInstance);

                Assert.Equal(numConnPorts, comp1.Children.PortCollection.Count(p => IsSupported(p)));
                Assert.Equal(numConnPorts, comp2.Children.PortCollection.Count(p => IsSupported(p)));

                var connectionsInAsm = asm.Children.PortCompositionCollection;
                Assert.Equal(numConnPorts, connectionsInAsm.Count());
                foreach (var connection in connectionsInAsm)
                {
                    var src = connection.GenericSrcEnd;
                    var dst = connection.GenericDstEnd;

                    Assert.NotEqual(src.ParentContainer, dst.ParentContainer);
                    Assert.Equal(src.Name, dst.Name);
                }
            });
        }

        [Fact]
        public void AsmHasConnInstance()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@AsmHasConnInstance|kind=ComponentAssembly|relpos=0";
            CyPhy.ComponentAssembly asm = null;

            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var asmObj = proj.get_ObjectByPath(path);
                asm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(asmObj);

                var conn = asm.Children.ConnectorCollection.First();
                numConnPorts = conn.AllChildren.Count();

                using (var unroller = new CyPhyElaborateCS.Unroller(asm.Impl.Project))
                {
                    unroller.UnrollComponentAssembly(asm.Impl as MgaModel);
                }
            });

            /* What do we want to test?
             * First, we want to make sure that the Unroll succeeded.
             * Then, we want to check that the Connector is gone,
             * and that three ports are in its place.
             */            
            proj.PerformInTransaction(delegate
            {
                Assert.Equal(0, asm.Children.ConnectorCollection.Count());
                Assert.Equal(numConnPorts, asm.Children.PortCollection.Count());

                var mc = asm.Children.ModelicaConnectorCollection.First();
                Assert.Equal("definition", mc.Attributes.Definition);
                Assert.Equal("definitionnotes", mc.Attributes.DefinitionNotes);
                Assert.Equal("instancenotes", mc.Attributes.InstanceNotes);
                Assert.Equal("class", mc.Attributes.Class);
                Assert.Equal("locator", mc.Attributes.Locator);
                var mcParam = mc.Children.ModelicaParameterCollection.First();
                Assert.Equal("defaultvalue", mcParam.Attributes.DefaultValue);
                Assert.Equal("value", mcParam.Attributes.Value);

                var pin = asm.Children.SchematicModelPortCollection.First();
                Assert.Equal("definition", pin.Attributes.Definition);
                Assert.Equal("definitionnotes", pin.Attributes.DefinitionNotes);
                Assert.Equal("instancenotes", pin.Attributes.InstanceNotes);
                Assert.Equal("edagate", pin.Attributes.EDAGate);
                Assert.Equal("edasymbolrotation", pin.Attributes.EDASymbolRotation);
                Assert.Equal("edasymbollocationx", pin.Attributes.EDASymbolLocationX);
                Assert.Equal("edasymbollocationy", pin.Attributes.EDASymbolLocationY);
                Assert.Equal(1, pin.Attributes.SPICEPortNumber);

                var scPort = asm.Children.SystemCPortCollection.First();
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.DataType_enum.sc_int, scPort.Attributes.DataType);
                Assert.Equal("definition", scPort.Attributes.Definition);
                Assert.Equal(2, scPort.Attributes.DataTypeDimension);
                Assert.Equal("definitionnotes", scPort.Attributes.DefinitionNotes);
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.Directionality_enum.@in, scPort.Attributes.Directionality);
                Assert.Equal("instancenotes", scPort.Attributes.InstanceNotes);
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.Function_enum.reset_async, scPort.Attributes.Function);
            });
        }

        [Fact]
        public void CompHasConnInstance()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@CompHasConnInstance|kind=ComponentAssembly|relpos=0";
            CyPhy.ComponentAssembly asm = null;

            int? numConnPorts = null;
            proj.PerformInTransaction(delegate
            {
                var asmObj = proj.get_ObjectByPath(path);
                asm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(asmObj);

                var comp = asm.Children.ComponentRefCollection.First();
                var conn = comp.Referred.Component.Children.ConnectorCollection.First();
                numConnPorts = conn.AllChildren.Count();

                Unroll(asm);
            });

            /* What do we want to test?
             * First, check that the component is an instance.
             * Then check that it has no connector.
             * Check that three ports are in its place.
             * Finally, check that all attributes from the constituent ports survived.
             */
            proj.PerformInTransaction(delegate
            {
                Assert.Equal(0, asm.Children.ComponentRefCollection.Count());
                Assert.Equal(1, asm.Children.ComponentCollection.Count());

                var comp = asm.Children.ComponentCollection.First();
                Assert.True(comp.IsInstance);
                Assert.Equal(0, comp.Children.ConnectorCollection.Count());
                Assert.Equal(numConnPorts, comp.Children.PortCollection.Count());

                var mc = comp.Children.ModelicaConnectorCollection.First();
                Assert.Equal("definition", mc.Attributes.Definition);
                Assert.Equal("definitionnotes", mc.Attributes.DefinitionNotes);
                Assert.Equal("instancenotes", mc.Attributes.InstanceNotes);
                Assert.Equal("class", mc.Attributes.Class);
                Assert.Equal("locator", mc.Attributes.Locator);
                var mcParam = mc.Children.ModelicaParameterCollection.First();
                Assert.Equal("defaultvalue", mcParam.Attributes.DefaultValue);
                Assert.Equal("value", mcParam.Attributes.Value);

                var pin = comp.Children.SchematicModelPortCollection.First();
                Assert.Equal("definition", pin.Attributes.Definition);
                Assert.Equal("definitionnotes", pin.Attributes.DefinitionNotes);
                Assert.Equal("instancenotes", pin.Attributes.InstanceNotes);
                Assert.Equal("edagate", pin.Attributes.EDAGate);
                Assert.Equal("edasymbolrotation", pin.Attributes.EDASymbolRotation);
                Assert.Equal("edasymbollocationx", pin.Attributes.EDASymbolLocationX);
                Assert.Equal("edasymbollocationy", pin.Attributes.EDASymbolLocationY);
                Assert.Equal(1, pin.Attributes.SPICEPortNumber);

                var scPort = comp.Children.SystemCPortCollection.First();
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.DataType_enum.sc_int, scPort.Attributes.DataType);
                Assert.Equal("definition", scPort.Attributes.Definition);
                Assert.Equal(2, scPort.Attributes.DataTypeDimension);
                Assert.Equal("definitionnotes", scPort.Attributes.DefinitionNotes);
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.Directionality_enum.@in, scPort.Attributes.Directionality);
                Assert.Equal("instancenotes", scPort.Attributes.InstanceNotes);
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.Function_enum.reset_async, scPort.Attributes.Function);
            });
        }

        [Fact]
        public void AsmUsesConnInstFromLib()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@AsmUsesConnInstFromLib|kind=ComponentAssembly|relpos=0";
            CyPhy.ComponentAssembly asm = null;

            proj.PerformInTransaction(delegate
            {
                var asmObj = proj.get_ObjectByPath(path);
                asm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(asmObj);

                Unroll(asm);
            });

            /* What do we want to test?
             * Then check that the connector is gone.
             * Check that three ports are in its place.
             * Finally, check that all attributes from the constituent ports survived.
             */
            proj.PerformInTransaction(delegate
            {
                Assert.Equal(3, asm.Children.DomainModelPortCollection.Count());
                Assert.Equal(0, asm.Children.ConnectorCollection.Count());
                                
                var mc = asm.Children.ModelicaConnectorCollection.First();
                Assert.Equal("definition", mc.Attributes.Definition);
                Assert.Equal("definitionnotes", mc.Attributes.DefinitionNotes);
                Assert.Equal("instancenotes", mc.Attributes.InstanceNotes);
                Assert.Equal("class", mc.Attributes.Class);
                Assert.Equal("locator", mc.Attributes.Locator);
                var mcParam = mc.Children.ModelicaParameterCollection.First();
                Assert.Equal("defaultvalue", mcParam.Attributes.DefaultValue);
                Assert.Equal("value", mcParam.Attributes.Value);

                var pin = asm.Children.SchematicModelPortCollection.First();
                Assert.Equal("definition", pin.Attributes.Definition);
                Assert.Equal("definitionnotes", pin.Attributes.DefinitionNotes);
                Assert.Equal("instancenotes", pin.Attributes.InstanceNotes);
                Assert.Equal("edagate", pin.Attributes.EDAGate);
                Assert.Equal("edasymbolrotation", pin.Attributes.EDASymbolRotation);
                Assert.Equal("edasymbollocationx", pin.Attributes.EDASymbolLocationX);
                Assert.Equal("edasymbollocationy", pin.Attributes.EDASymbolLocationY);
                Assert.Equal(1, pin.Attributes.SPICEPortNumber);

                var scPort = asm.Children.SystemCPortCollection.First();
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.DataType_enum.sc_int, scPort.Attributes.DataType);
                Assert.Equal("definition", scPort.Attributes.Definition);
                Assert.Equal(2, scPort.Attributes.DataTypeDimension);
                Assert.Equal("definitionnotes", scPort.Attributes.DefinitionNotes);
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.Directionality_enum.@in, scPort.Attributes.Directionality);
                Assert.Equal("instancenotes", scPort.Attributes.InstanceNotes);
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.Function_enum.reset_async, scPort.Attributes.Function);
            });
        }

        [Fact]
        public void CompHasConnInstFromLib()
        {
            String path = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@CompHasConnInstFromLib|kind=ComponentAssembly|relpos=0";
            CyPhy.ComponentAssembly asm = null;
            CyPhy.Component comp = null;

            proj.PerformInTransaction(delegate
            {
                var asmObj = proj.get_ObjectByPath(path);
                asm = ISIS.GME.Dsml.CyPhyML.Classes.ComponentAssembly.Cast(asmObj);

                Unroll(asm);

                comp = asm.Children.ComponentCollection.First();
            });

            /* What do we want to test?
             * Then check that the connector is gone.
             * Check that three ports are in its place.
             * Finally, check that all attributes from the constituent ports survived.
             */
            proj.PerformInTransaction(delegate
            {
                Assert.Equal(3, comp.Children.DomainModelPortCollection.Count());
                Assert.Equal(0, comp.Children.ConnectorCollection.Count());

                var mc = comp.Children.ModelicaConnectorCollection.First();
                Assert.Equal("definition", mc.Attributes.Definition);
                Assert.Equal("definitionnotes", mc.Attributes.DefinitionNotes);
                Assert.Equal("instancenotes", mc.Attributes.InstanceNotes);
                Assert.Equal("class", mc.Attributes.Class);
                Assert.Equal("locator", mc.Attributes.Locator);
                var mcParam = mc.Children.ModelicaParameterCollection.First();
                Assert.Equal("defaultvalue", mcParam.Attributes.DefaultValue);
                Assert.Equal("value", mcParam.Attributes.Value);

                var pin = comp.Children.SchematicModelPortCollection.First();
                Assert.Equal("definition", pin.Attributes.Definition);
                Assert.Equal("definitionnotes", pin.Attributes.DefinitionNotes);
                Assert.Equal("instancenotes", pin.Attributes.InstanceNotes);
                Assert.Equal("edagate", pin.Attributes.EDAGate);
                Assert.Equal("edasymbolrotation", pin.Attributes.EDASymbolRotation);
                Assert.Equal("edasymbollocationx", pin.Attributes.EDASymbolLocationX);
                Assert.Equal("edasymbollocationy", pin.Attributes.EDASymbolLocationY);
                Assert.Equal(1, pin.Attributes.SPICEPortNumber);

                var scPort = comp.Children.SystemCPortCollection.First();
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.DataType_enum.sc_int, scPort.Attributes.DataType);
                Assert.Equal("definition", scPort.Attributes.Definition);
                Assert.Equal(2, scPort.Attributes.DataTypeDimension);
                Assert.Equal("definitionnotes", scPort.Attributes.DefinitionNotes);
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.Directionality_enum.@in, scPort.Attributes.Directionality);
                Assert.Equal("instancenotes", scPort.Attributes.InstanceNotes);
                Assert.Equal(ISIS.GME.Dsml.CyPhyML.Classes.SystemCPort.AttributesClass.Function_enum.reset_async, scPort.Attributes.Function);
            });
        }

        private void Unroll(CyPhy.Component comp, bool cleanup = true)
        {
            using (var unroller = new CyPhyElaborateCS.Unroller(comp.Impl.Project))
            {
                unroller.UnrollComponent(comp.Impl as MgaModel, cleanup);
            }
        }

        private void Unroll(CyPhy.ComponentAssembly topCa)
        {
            Assert.True(CallElaborator(proj, topCa.Impl as MgaFCO, null, 0));
        }

        private void Unroll(CyPhy.TestBench tb)
        {
            var tlsut = tb.Children.TopLevelSystemUnderTestCollection.First();
            if (tlsut.GenericReferred.Kind == "Component")
            {
                using (var unroller = new CyPhyElaborateCS.Unroller(tb.Impl.Project))
                {
                    unroller.UnrollComponent(tb.Impl as MgaModel);
                }
            }
            else
            {
                Assert.True(CallElaborator(proj, tb.Impl as MgaFCO, null, 0));
            }
        }

        private bool IsSupported(CyPhy.Port port)
        {
            return (CyPhyElaborateCS.Unroller.SupportedPortTypes.Contains(port.Kind));
        }
        
        private bool CompareObjects(MgaFCO fco1, MgaFCO fco2)
        {
            if (fco1.MetaBase.Name != fco2.MetaBase.Name)
            {
                return false;
            }

            var attr_FCO1 = fco1.Attributes;
            var attr_FCO2 = fco2.Attributes;

            for (int i = 1; i <= attr_FCO1.Count; i++)
            {
                var attr1 = attr_FCO1[i];
                var attr2 = attr_FCO2[i];

                if (attr1.Meta.Name == "ID")
                {
                    continue;
                }

                var valAttr1 = attr1.Value.ToString();
                var valAttr2 = attr2.Value.ToString();

                if (valAttr1 != valAttr2)
                {
                    return false;
                }
            }

            if (fco1.ObjType == GME.MGA.Meta.objtype_enum.OBJTYPE_MODEL)
            {
                var model1 = fco1 as MgaModel;
                var model2 = fco2 as MgaModel;

                var children_Model1 = model1.ChildFCOs;
                var children_Model2 = model2.ChildFCOs;

                if (children_Model1.Count != children_Model2.Count)
                {
                    return false;
                }

                MgaFCO child1 = null;
                MgaFCO child2 = null;
                foreach (MgaFCO i in children_Model1)
                {
                    child1 = i;
                    foreach (MgaFCO j in children_Model2)
                    {
                        if (i.Name == j.Name)
                        {
                            child2 = j;
                        }
                    }

                    if (false == CompareObjects(child1, child2))
                        return false;
                }
            }

            return true;
        }

        private Dictionary<string, string> GetIDs(MgaModel rootModel)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            Queue<IMgaFCO> fcos = new Queue<IMgaFCO>();
            var filter = rootModel.Project.CreateFilter();

            Action<IMgaModel> enqueueModelAndDescendants = (model) =>
            {
                fcos.Enqueue(model);
                foreach (MgaFCO fco in ((MgaModel)model).GetDescendantFCOs(filter))
                {
                    fcos.Enqueue(fco);
                }
            };
            enqueueModelAndDescendants(rootModel);

            while (fcos.Count > 0)
            {
                IMgaFCO fco = fcos.Dequeue();
                if (ret.ContainsKey(fco.ID) == false)
                {
                    ret.Add(fco.ID, fco.AbsPath);
                    if (fco is IMgaReference)
                    {
                        var referred = ((IMgaReference)fco).Referred;
                        if (referred != null)
                        {
                            if (referred is MgaModel)
                            {
                                enqueueModelAndDescendants((MgaModel)referred);
                            }
                            else
                            {
                                fcos.Enqueue(referred);
                            }
                        }
                    }
                }
            }
            return ret;
        }


        private bool CallElaborator(
            MgaProject project,
            MgaFCO currentobj,
            MgaFCOs selectedobjs,
            int param,
            bool expand = true)
        {
            Dictionary<string, string> originalIDs;
            CyPhyElaborateCS.CyPhyElaborateCSInterpreter elaborator;
            bool result;
            try
            {
                //this.Logger.WriteDebug("Elaborating model...");
                elaborator = new CyPhyElaborateCS.CyPhyElaborateCSInterpreter();
                elaborator.Initialize(project);
                int verbosity = 128;
                originalIDs = GetIDs((MgaModel)currentobj);
                result = elaborator.RunInTransaction(project, currentobj, selectedobjs, verbosity);

            }
            catch (Exception)
            {
                return false;
            }

            var filter = project.CreateFilter();
            foreach (MgaFCO fco in ((MgaModel)currentobj).GetDescendantFCOs(filter))
            {
                if (fco is IMgaConnection)
                {
                    continue;
                }
                if (fco.ParentModel != null && fco.ParentModel.Meta.Name == "ModelicaConnector")
                {
                    continue; // FIXME: maybe this is a bug
                }
                string original = null;
                if (originalIDs.ContainsKey(fco.ID) == false)
                {
                    Assert.True(elaborator.Traceability.TryGetMappedObject(fco.ID, out original), "Unmapped fco " + fco.ID + " " + fco.AbsPath);
                    Assert.True(originalIDs.ContainsKey(original), "Mapped FCO in traceability " + fco.AbsPath + " is mapped to unknown ID " + original);
                }
            }

            return result;
        }
    }
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            int ret = Xunit.ConsoleClient.Program.Main(new string[] {
                System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length),
                //"/noshadow",
            });
            Console.In.ReadLine();
            return ret;
        }
    }

}
