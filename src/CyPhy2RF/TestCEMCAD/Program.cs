using System;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

using System.IO;
using System.Diagnostics;

using CSXCAD;
using CSXCAD.Ara;
using Postprocess;

namespace TestCEMCAD
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestAraPhone();
            //TestGrid();
            //Nf2ffToVtk();
            //ExportAntenna_InvertedF();
            ExportAntenna_Small_15x6mm();
        }

        static void TestAraPhone()
        {
            Endo e = new Endo(null, "endo", new Vector3D(0, 0, 0), 0);

            Module_1x2 m1 = new Module_1x2("m1");
            Module_2x2 m2 = new Module_2x2("m2");
            Module_1x2 m4 = new Module_1x2("m4");
            Module_2x2 m5 = new Module_2x2("m5");
            Module_1x2 m6 = new Module_1x2("m6");
            Module_1x2 m7 = new Module_1x2("m7");

            XmlCompound u1 = new XmlCompound(m1, "u1", new Vector3D(0, 0, 0), 0);
            u1.Parse(XElement.Load("Box.xml"));
            u1.Transformations.Add(new TTranslate(20, 3, 3));
            m5.Add(u1);

            //e.AddModule(1, m1);
            //e.AddModule(3, m2);
            //e.AddModule(4, m4);
            e.AddModule(5, m5);
            //e.AddModule(6, m6);
            //e.AddModule(7, m7);

            // Phantom
            Compound headPhantom = new Compound("head-phantom");

            Dielectric skinMaterial = new Dielectric("skin", 50, kappa: 0.65, density: 1100);
            skinMaterial.FillColor = new Material.Color(245, 215, 205, 54);
            skinMaterial.EdgeColor = new Material.Color(255, 235, 217, 250);
            Sphere skin = new Sphere(null, skinMaterial, 11, new Vector3D(), 1);
            skin.Transformations.Add(new TScale(80, 100, 80));
            headPhantom.Add(skin);

            Dielectric boneMaterial = new Dielectric("bone", 13, kappa: 0.1, density: 2000);
            boneMaterial.FillColor = new Material.Color(227, 227, 227, 54);
            boneMaterial.EdgeColor = new Material.Color(202, 202, 202, 250);
            Sphere bone = new Sphere(null, boneMaterial, 12, new Vector3D(), 1);
            bone.Transformations.Add(new TScale(75, 95, 75));
            headPhantom.Add(bone);

            Dielectric brainMaterial = new Dielectric("brain", 60, kappa: 0.7, density: 1040);
            brainMaterial.FillColor = new Material.Color(255, 85, 127, 54);
            brainMaterial.EdgeColor = new Material.Color(71, 222, 179, 250);
            Sphere brain = new Sphere(null, brainMaterial, 13, new Vector3D(), 1);
            brain.Transformations.Add(new TScale(65, 85, 65));
            headPhantom.Add(brain);

            headPhantom.Transformations.Add(new TTranslate(33, 70, 90));

            Compound s = new Compound("space");
            s.Add(e);
            s.Add(headPhantom);

            RectilinearGrid g = new SimpleGrid_6x3();
            g.ZLines.Add(170);
            double airBox = 50;
            double maxRes = 5;
            double ratio = 1.5;

            g.AddAirbox(airBox);
            g.SmoothMesh(maxRes, ratio);

            s.Add(new SARBox("SAR", 1200e6, new Vector3D(), new Vector3D(20, 20, 20)));
            s.Add(new NF2FFBox("nf2ff",
                new Vector3D(g.XLines.First(), g.YLines.First(), g.ZLines.First()),
                new Vector3D(g.XLines.Last(), g.YLines.Last(), g.ZLines.Last())));
            s.Add(new LumpedPort(100, 1, 50.0,
                new Vector3D(-0.1, -0.1, -1.25),
                new Vector3D(+0.1, +0.1, +1.25), ENormDir.Z, true));
                
            Simulation fdtd = new Simulation();
            fdtd.Excitation = new GaussExcitation(1e9, 1.5e9);

            g.AddPML(10);

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("Test XML file for CyPhy generated openEMS simulations"),
                new XElement("openEMS",
                    fdtd.ToXElement(),
                    new XElement("ContinuousStructure",
                        new XAttribute("CoordSystem", 0),
                        s.ToXElement(),
                        g.ToXElement()
                    )
                )
            );

            doc.Save("CSXTest.xml");
        }

        static void TestGrid()
        {
            double maxRes;
            double ratio;

            List<double> mesh;
            List<double> smoothMesh;
            List<string> referenceMesh;

            // #1
            maxRes = 1;
            ratio = 1.5;

            mesh = new List<double> { 0, 3 };
            smoothMesh = RectilinearGrid.SmoothLines(mesh, maxRes, ratio);
            referenceMesh = new List<string> { "0.00", "1.00", "2.00", "3.00" };

            Console.WriteLine("maxRes = {0} ratio = {1}", maxRes, ratio);
            Console.WriteLine("Original mesh:");
            Console.WriteLine(String.Join(", ", mesh.Select(i => String.Format("{0:f2}",i)).ToArray()));

            EvalGridTest(smoothMesh, referenceMesh);

            Console.WriteLine();

            // #2
            mesh = new List<double> { 0, 0.01, 3 };
            smoothMesh = RectilinearGrid.SmoothLines(mesh, maxRes, ratio);
            referenceMesh = new List<string> {
                "0.00", "0.01", "0.03", "0.05", "0.08", "0.13", "0.21", "0.32", 
                "0.49", "0.75", "1.13", "1.71", "2.35", "3.00" };

            Console.WriteLine("maxRes = {0} ratio = {1}", maxRes, ratio);
            Console.WriteLine("Original mesh:");
            Console.WriteLine(String.Join(", ", mesh.Select(i => String.Format("{0:f2}",i)).ToArray()));

            EvalGridTest(smoothMesh, referenceMesh);

            Console.WriteLine();

            maxRes = 1;
            ratio = 1.5;
            mesh = new List<double> { 0, 0.01, 2.99, 3 };
            smoothMesh = RectilinearGrid.SmoothLines(mesh, maxRes, ratio);
            referenceMesh = new List<string> {
                "0.00", "0.01", "0.03", "0.05", "0.08", "0.13", "0.21", "0.32", 
                "0.49", "0.75", "0.94", "1.12", "1.31", "1.87", "2.25", "2.51", 
                "2.68", "2.79", "2.87", "2.92", "2.95", "2.98", "2.99", "3.00" };

            Console.WriteLine("maxRes = {0} ratio = {1}", maxRes, ratio);
            Console.WriteLine("Original mesh:");
            Console.WriteLine(String.Join(", ", mesh.Select(i => String.Format("{0:f2}",i)).ToArray()));

            EvalGridTest(smoothMesh, referenceMesh);
        }

        static bool EvalGridTest(List<double> actual, List<string> reference)
        {
            Console.WriteLine("Smoothed mesh:");
            Console.WriteLine(String.Join(", ", actual.Select(i => String.Format("{0:f2}",i)).ToArray()));

            Console.WriteLine("Reference mesh:");
            Console.WriteLine(String.Join(", ", reference.Select(i => String.Format("{0:f2}",i)).ToArray()));

            if (actual.Count != reference.Count)
            {
                Console.WriteLine("Error: Expected {0} elements but got {1} instead", reference.Count, actual.Count);
                return false;
            }

            for (int i = 0; i < actual.Count; i++ )
            {
                if (!String.Equals(String.Format("{0:f2}", actual.ElementAt(i)), reference.ElementAt(i)))
                {
                    Console.WriteLine("Error: Expected {0} at {1} but got {2} instead",
                        reference.ElementAt(i), i, actual.ElementAt(i));
                    return false;
                }
            }

            return true;
        }

        static void ExportAntenna_InvertedF()
        {
            double thickness = 0.01;
            double airBox = 5.0;
            double innerResolution = 0.5;
            double outerResolution = 5.0;

            var antenna = new CSXCAD.Antenna.InvertedF_2400MHz(thickness);

            const double pcbThickness = 1.5;
            var lumpedPort = new LumpedPort(90, 1, 50, new Vector3D(0.0, 0.0, -pcbThickness), new Vector3D(0.0, 0.0, 0), ENormDir.Z, true);
            antenna.Add(lumpedPort);

            double margin = 2.0;
            double groundWidth = 5.0;
            var p1 = new Vector3D(antenna.BoundingBox.P1.x-margin, -groundWidth-margin, -pcbThickness);
            var p2 = new Vector3D(antenna.BoundingBox.P2.x+margin, antenna.BoundingBox.P2.y+margin, 0);

            var substrate = new Dielectric("pcb", 3.38, 1e-3 * 2 * Math.PI * 2.45e9 * 3.38 * Material.Eps0);
            substrate.EdgeColor = new Material.Color(10, 255, 10, 128);
            substrate.FillColor = new Material.Color(10, 255, 10, 128);
            var pcb = new CSXCAD.Box(null, substrate, 60, p1, p2);
            //antenna.Add(pcb);

            var topGround = new Metal("bottom-ground");
            topGround.EdgeColor = new Material.Color(235, 148, 7, 255);
            topGround.FillColor = topGround.EdgeColor;
            var topGroundPlane = new CSXCAD.Box(null, topGround, 100,
                new Vector3D(antenna.BoundingBox.P1.x, 0, -pcbThickness),
                new Vector3D(antenna.BoundingBox.P2.x, -groundWidth, -pcbThickness));
            antenna.Add(topGroundPlane);

            var bottomGround = new Metal("top-ground");
            bottomGround.EdgeColor = new Material.Color(235, 148, 7, 255);
            bottomGround.FillColor = bottomGround.EdgeColor;
            var topGroundPlaneLeft = new CSXCAD.Box(null, bottomGround, 100,
                new Vector3D(antenna.BoundingBox.P1.x, 0, 0),
                new Vector3D(-0.46/2-0.45, -groundWidth, 0));
            var topGroundPlaneRight = new CSXCAD.Box(null, bottomGround, 100,
                new Vector3D(0.46/2+0.45, 0, 0),
                new Vector3D(antenna.BoundingBox.P2.x, -groundWidth, 0));
            antenna.Add(topGroundPlaneLeft);
            antenna.Add(topGroundPlaneRight);

            Simulation fdtd = new Simulation();
            fdtd.Excitation = new GaussExcitation(2450e6, 500e6);

            RectilinearGrid grid = new RectilinearGrid(); ;
            grid.Add(new Vector3D(0,0,0));

            grid.SmoothMesh(innerResolution);
            grid.AddAirbox(airBox);
            grid.SmoothMesh(outerResolution);
            var nf2ff = new NF2FFBox("nf2ff",
                new Vector3D(grid.XLines.First(), grid.YLines.First(), grid.ZLines.First()),
                new Vector3D(grid.XLines.Last(), grid.YLines.Last(), grid.ZLines.Last()));
            antenna.Add(nf2ff);
            grid.AddPML(8);

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("Test XML file for CyPhy generated openEMS simulations"),
                new XElement("openEMS",
                    fdtd.ToXElement(),
                    new XElement("ContinuousStructure",
                        new XAttribute("CoordSystem", 0),
                        antenna.ToXElement(),
                        grid.ToXElement()
                    )
                )
            );

            doc.Save("InvertedF.xml");
        }

        static void ExportAntenna_Small_15x6mm()
        {
            double thickness = 0.01;
            double airBox = 5.0;
            double innerResolution = 0.5;
            double outerResolution = 5.0;

            var antenna = new CSXCAD.Antenna.Small_15x6mm_2400MHz(thickness);

            const double pcbThickness = 1.5;
            var lumpedPort = new LumpedPort(90, 1, 50, new Vector3D(0.0, 0.0, -pcbThickness), new Vector3D(0.0, 0.0, 0), ENormDir.Z, true);
            antenna.Add(lumpedPort);

            double margin = 2.0;
            double groundWidth = 5.0;
            var p1 = new Vector3D(antenna.BoundingBox.P1.x - margin, -groundWidth - margin, -pcbThickness);
            var p2 = new Vector3D(antenna.BoundingBox.P2.x + margin, antenna.BoundingBox.P2.y + margin, 0);

            double epsRel = 4.88;
            var substrate = new Dielectric("pcb", epsRel, 1e-3 * 2 * Math.PI * 2.45e9 * epsRel * Material.Eps0);
            substrate.EdgeColor = new Material.Color(10, 255, 10, 128);
            substrate.FillColor = new Material.Color(10, 255, 10, 128);
            var pcb = new CSXCAD.Box(null, substrate, 60, p1, p2);
            antenna.Add(pcb);

            var bottomGround = new Metal("bottom-ground");
            bottomGround.EdgeColor = new Material.Color(235, 148, 7, 255);
            bottomGround.FillColor = bottomGround.EdgeColor;
            var bottomGroundPlane = new CSXCAD.Box(null, bottomGround, 100,
                new Vector3D(antenna.BoundingBox.P1.x - antenna.D1, antenna.D4 / 2, -pcbThickness),
                new Vector3D(antenna.BoundingBox.P2.x + antenna.D3, -groundWidth, -pcbThickness-0.01));
            antenna.Add(bottomGroundPlane);

            var topGround = new Metal("top-ground");
            topGround.EdgeColor = new Material.Color(235, 148, 7, 255);
            topGround.FillColor = topGround.EdgeColor;
            var topGroundPlane = new CSXCAD.Box(null, topGround, 100,
                new Vector3D(antenna.BoundingBox.P1.x - antenna.D1, -antenna.D4 / 2, 0),
                new Vector3D(antenna.BoundingBox.P2.x + antenna.D3, -groundWidth, 0.01));
            antenna.Add(topGroundPlane);

            var viaMetal = new Metal("via");
            viaMetal.EdgeColor = new Material.Color(235, 148, 7, 255);
            viaMetal.FillColor = viaMetal.EdgeColor;
            var via = new Cylinder(null, viaMetal, 100,
                new Vector3D(-(antenna.W1 / 2 + antenna.D5 + antenna.W2 / 2), 0, -pcbThickness),
                new Vector3D(-(antenna.W1 / 2 + antenna.D5 + antenna.W2 / 2), 0, 0),
                0.25);
            antenna.Add(via);

            Simulation fdtd = new Simulation();
            fdtd.Excitation = new GaussExcitation(2450e6, 500e6);

            RectilinearGrid grid = new RectilinearGrid(); ;
            grid.Add(new Vector3D(0, 0, 0));
            grid.Add(pcb.P1);
            grid.Add(pcb.P2);
            /*
            foreach (var v in antenna.antennaPoly)
            {
                grid.Add(new Vector3D(v.x, v.y, 0));
            }
            */

            grid.SmoothMesh(innerResolution);
            grid.AddAirbox(airBox);
            grid.SmoothMesh(outerResolution);
            var nf2ff = new NF2FFBox("nf2ff",
                new Vector3D(grid.XLines.First(), grid.YLines.First(), grid.ZLines.First()),
                new Vector3D(grid.XLines.Last(), grid.YLines.Last(), grid.ZLines.Last()));
            antenna.Add(nf2ff);
            grid.AddPML(8);

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("Test XML file for CyPhy generated openEMS simulations"),
                new XElement("openEMS",
                    fdtd.ToXElement(),
                    new XElement("ContinuousStructure",
                        new XAttribute("CoordSystem", 0),
                        antenna.ToXElement(),
                        grid.ToXElement()
                    )
                )
            );

            doc.Save("Small_15x6mm.xml");
        }

        static void Nf2ffToVtk()
        {
            var nf2ff = new NF2FF(1e9, "ref_3D_pattern.h5");

            // Assume openEMS and NF2FF are run and 3D_Pattern.h5 is available
            nf2ff.ReadHDF5Result();
            nf2ff.ToVTK(fileName: "dut_3D_pattern.vtk");
        }
    }
}
