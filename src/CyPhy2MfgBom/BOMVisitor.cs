using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyGUIs;
using CyPhyCOMInterfaces;
using GME.MGA;

namespace CyPhy2MfgBom
{
    internal class BOMVisitor
    {
        public MfgBom.Bom.MfgBom bom { get; private set; }
        public GMELogger Logger { get; set; }
        public IMgaTraceability Traceability { get; set; }
        private String TopPath { get; set; }

        public BOMVisitor()
        {
            bom = new MfgBom.Bom.MfgBom();
            TopPath = null;
        }

        private String TrimPath(String path)
        {
            Logger.WriteDebug("TrimPath({0})", path);

            if (TopPath == null)
            {
                throw new ArgumentNullException("TopPath", "TopPath hasn't been established.");
            }

            if (false == path.StartsWith(TopPath))
            {
                String msg = String.Format("Path determination failure! TopPath of {0} is not contained within object path of {1}",
                                           TopPath,
                                           path);
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }

            // Trim the length of the top path, plus one for the trailing '/'
            return path.Substring(TopPath.Length + 1);
        }

        public void visit(CyPhy.TestBench testbench)
        {
            Logger.WriteDebug("visit(CyPhy.TestBench): {0}", testbench.Path);

            foreach (var tlsut in testbench.Children.TopLevelSystemUnderTestCollection)
            {
                if (tlsut is CyPhy.ComponentAssembly)
                {
                    visit(tlsut as CyPhy.ComponentAssembly);
                }
                else
                {
                    throw new NotSupportedException(String.Format("Top-Level System Under Test of kind {0} is not supported", tlsut.Impl.MetaBase.Name));
                }
            }

            foreach (var ca in testbench.Children.ComponentAssemblyCollection)
            {
                visit(ca);
            }
        }

        public void visit(CyPhy.ComponentAssembly assembly)
        {
            Logger.WriteDebug("visit(CyPhy.ComponentAssembly): {0}", assembly.Path);
            if (TopPath == null)
            {
                Logger.WriteDebug("Setting top path: {0}", assembly.Path);
                TopPath = assembly.Path;
                this.bom.designName = assembly.Name;    // MOT-256
            }

            foreach (var asm in assembly.Children.ComponentAssemblyCollection)
            {
                visit(asm);
            }

            foreach (var comp in assembly.Children.ComponentCollection)
            {
                visit(comp);
            }
        }

        public void visit(CyPhy.Component component)
        {
            Logger.WriteDebug("visit(CyPhy.Component): {0}", component.Path);
            
            var part = new MfgBom.Bom.Part();

            // Check for a Property called "octopart_mpn"
            var octopart_mpn = component.Children.PropertyCollection.FirstOrDefault(p => p.Name == "octopart_mpn" &&
                                                                                         !String.IsNullOrWhiteSpace(p.Attributes.Value));
            if (octopart_mpn != null)
            {
                part.octopart_mpn = octopart_mpn.Attributes.Value;
            }
            
            var instance = new MfgBom.Bom.ComponentInstance()
                                {
                                    gme_object_id = component.ID,
                                    path = TrimPath(component.Path)
                                };
            part.AddInstance(instance);
            
            bom.AddPart(part);
        }
    }
}
