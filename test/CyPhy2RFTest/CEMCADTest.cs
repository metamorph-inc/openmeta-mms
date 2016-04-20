using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Xunit;
using CSXCAD;
using System.Globalization;

namespace CyPhy2RFTest
{
    /*
    public class CEMCADFixture : IDisposable
    {
        public void Dispose()
        {
        }
    }
    */

    public class CSXCADTest // : IUseFixture<CEMCADFixture>
    {
        const double eps = 1e-15;

        private bool AlmostEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) <= eps;
        }

        [Fact]
        public void Vector()
        {
            Vector3D v0 = new Vector3D();
            Vector3D v1 = new Vector3D(0.0, 1.0, 2.0);
            Vector3D v2 = new Vector3D(0.0, 1.0, 2.0);
            Vector3D v3 = new Vector3D(1.0, 1.0, 1.0);

            // Basic equivalence
            Assert.Equal(v1, v2);
            Assert.NotEqual(v1, v3);

            // Addition
            Assert.Equal(v1, v1 + v0);
            Assert.Equal(v1, v1 - v0);

            // Constant multiplication
            Assert.Equal(2 * v3, new Vector3D(2.0, 2.0, 2.0));
            Assert.Equal(2 * v1, v1 + v1);

            // Length
            Vector3D a = new Vector3D(1.0, 1.0, 0.0);
            Assert.Equal(Math.Sqrt(2.0), a.Length);
            Assert.Equal(Math.Sqrt(3.0), v3.Length);

            // Rotation
            Vector3D vo = new Vector3D(1.0, 0.0, 0.0);
            Vector3D ve = new Vector3D(0.0, 1.0, 0.0);
            Assert.True((ve - Vector3D.RotateZ(vo, Math.PI / 2)).x < eps);
            Assert.True((ve - Vector3D.RotateZ(vo, Math.PI / 2)).y < eps);
            Assert.True((ve - Vector3D.RotateZ(vo, Math.PI / 2)).z < eps);
        }

        /// <summary>
        /// Test the basic 3D vector transformations.
        /// </summary>
        [Fact]
        public void Transform()
        {
            // TScale
            Vector3D vi = new Vector3D(1, 1, 1);
            TScale s = new TScale(1, 1, 2);
            Vector3D vo = s * vi;
            
            Assert.True(AlmostEqual(vi.x * s.x, vo.x));
            Assert.True(AlmostEqual(vi.y * s.y, vo.y));
            Assert.True(AlmostEqual(vi.z * s.z, vo.z));

            // TTranslate
            TTranslate t1 = new TTranslate(1, 0, 1);
            vo = t1 * vi;
            Assert.True(AlmostEqual(vi.x + t1.x, vo.x));

            TTranslate t2 = new TTranslate(vi);
            vo = t2 * vi;
            Assert.True(AlmostEqual(2 * vi.x, vo.x));
            Assert.True(AlmostEqual(2 * vi.y, vo.y));
            Assert.True(AlmostEqual(2 * vi.z, vo.z));

            // Rotate origin
            vi = new Vector3D(1, 0, 0);
            TRotateOrigin ro = new TRotateOrigin(0, 0, 1, Math.PI);
            vo = ro * vi;
            Assert.True(AlmostEqual(-vi.x, vo.x));
            ro = new TRotateOrigin(0, 1, 0, Math.PI);
            vo = ro * vi;
            Assert.True(AlmostEqual(-vi.x, vo.x));
            ro = new TRotateOrigin(1, 0, 0, Math.PI/2);
            vo = ro * vi;
            Assert.True(AlmostEqual(vi.x, vo.x));
            ro = new TRotateOrigin(1, 0, 1, Math.PI);
            vo = ro * vi;
            Assert.True(AlmostEqual(vi.x, vo.z));

            // Rotate X
            vi = new Vector3D(1, 1, 0);
            TRotateX rx = new TRotateX(Math.PI / 2);
            vo = rx * vi;
            Assert.True(AlmostEqual(vi.x, vo.x));
            Assert.True(AlmostEqual(vi.y, vo.z));

            // Rotate Y
            vi = new Vector3D(1, 0, 0);
            TRotateY ry = new TRotateY(Math.PI);
            vo = ry * vi;
            Assert.True(AlmostEqual(vi.x, -vo.x));

            // Rotate Z
            vi = new Vector3D(1, 1, 1);
            TRotateZ rz = new TRotateZ(Math.PI);
            vo = rz * vi;
            Assert.True(AlmostEqual(vi.x, -vo.x));
            Assert.True(AlmostEqual(vi.y, -vo.y));
        }

        /// <summary>
        /// The Polygon constructor should process both double[2,] and double[,2] arrays properly.
        /// </summary>
        [Fact]
        public void Polygon_SwappedArrayDimensionsInConstructor_ReturnsSameObject()
        {
            double[,] v1 = new double[2, 3]
            {
                {0.0, 1.0, 2.0},
                {3.0, 4.0, 5.0}
            };

            double[,] v2 = new double[v1.GetLength(1), v1.GetLength(0)];
            for (int i = 0; i < v2.GetLength(0); i++)
            {
                for (int j = 0; j < v2.GetLength(1); j++)
                {
                    v2[i, j] = v1[j, i];
                }
            }

            Material m = new Dielectric("dummy");
            Polygon p1 = new Polygon(null, m, 0, 2, 0, v1);
            Polygon p2 = new Polygon(null, m, 0, 2, 0, v2);

            Assert.Equal(p1.Points, p2.Points);
        }


        /// <summary>
        /// The re-imported Box primitive should match the original one.
        /// </summary>
        [Fact]
        public void Box_ExportToXmlAndParse_MatchesOriginal()
        {
            var v1 = new Vector3D(0.0, 0.0, 0.0);
            var v2 = new Vector3D(5.0, 0.0, 0.0);
            Box refBox = new Box(null, null, 0, v1, v2);

            XElement xmlBox = refBox.ToXElement();
            XmlCompound xb = new XmlCompound(null, "test-compound", new Vector3D(), 0.0);
            Box dutBox = (Box)Primitive.FromXElement(xmlBox, null);

            Assert.Equal(v1, dutBox.P1);
            Assert.Equal(v2, dutBox.P2);
        }

        /// <summary>
        /// The re-imported Cylinder primitive should match the original one.
        /// </summary>
        [Fact]
        public void Cylinder_ExportToXmlAndParse_MatchesOriginal()
        {
            var v1 = new Vector3D(0.0, 0.0, 0.0);
            var v2 = new Vector3D(5.0, 0.0, 0.0);
            double radius = 2.0;
            Cylinder refCylinder = new Cylinder(null, null, 0, v1, v2, radius);

            XElement xmlCylinder = refCylinder.ToXElement();
            Cylinder dutCylinder = (Cylinder)Primitive.FromXElement(xmlCylinder, null);

            Assert.Equal(v1, dutCylinder.P1);
            Assert.Equal(v2, dutCylinder.P2);
            Assert.Equal(radius, dutCylinder.Radius);
        }

        /// <summary>
        /// The orientation of the non-tranformed LumpedPort and the normal
        /// direction of its excitation vector should match.
        /// </summary>
        //[Fact(Skip="Disable output while developing other test cases")]
        public void LumpedPort_WithoutTransformation_NormalDirectionCorrect()
        {
            Compound excitation = new Compound("Excitation");
            LumpedPort lumpedPort = new LumpedPort(0, 1, 50, new Vector3D(), new Vector3D(1, 1, 1), ENormDir.X);
            excitation.Add(lumpedPort);
            Assert.True(false, "Test not implemented yet");
        }

        /// <summary>
        /// The orientation of the 90 degree rotated LumpedPort and the normal
        /// direction of its excitation vector should match.
        /// </summary>
        //[Fact(Skip="Disable output while developing other test cases")]
        public void LumpedPort_90DegTransformation_NormalDirectionCorrect()
        {
            Assert.True(false, "Test not implemented yet");
        }

        /// <summary>
        /// The orientation of the 180 degree rotated LumpedPort and the normal
        /// direction of its excitation vector should match.
        /// </summary>
        //[Fact(Skip="Disable output while developing other test cases")]
        public void LumpedPort_180DegTransformation_NormalDirectionCorrect()
        {
            Assert.True(false, "Test not implemented yet");
        }

        /// <summary>
        /// Compare the smoothing algorithm to that of the openEMS
        /// Octave/Matlab library.
        /// </summary>
        [Fact]
        public void Grid()
        {
            double maxRes = 1.0;
            double maxRatio = 1.5;

            List<double> mesh;
            List<double> smoothMesh;
            List<string> referenceMesh;

            // #1
            mesh = new List<double> { 0, 3 };
            smoothMesh = RectilinearGrid.SmoothLines(mesh, maxRes, maxRatio);
            referenceMesh = new List<string> { "0.00", "1.00", "2.00", "3.00" };

            //Console.Write("Original mesh: ");
            //Console.WriteLine(String.Join(", ", mesh.Select(i => String.Format("{0:f2}", i)).ToArray()));

            Assert.True(CompareGrid(referenceMesh, smoothMesh));

            Console.WriteLine();

            // #2
            mesh = new List<double> { 0, 0.01, 3 };
            smoothMesh = RectilinearGrid.SmoothLines(mesh, maxRes, maxRatio);
            referenceMesh = new List<string> {
                "0.00", "0.01", "0.03", "0.05", "0.08", "0.13", "0.21", "0.32", 
                "0.49", "0.75", "1.13", "1.71", "2.35", "3.00" };

            //Console.Write("Original mesh: ");
            //Console.WriteLine(String.Join(", ", mesh.Select(i => String.Format("{0:f2}", i)).ToArray()));

            Assert.True(CompareGrid(referenceMesh, smoothMesh));

            /*
            Console.WriteLine();

            mesh = new List<double> { 0, 0.01, 2.99, 3 };
            smoothMesh = RectilinearGrid.SmoothLines(mesh, maxRes, maxRatio);
            referenceMesh = new List<string> {
                "0.00", "0.01", "0.03", "0.05", "0.08", "0.13", "0.21", "0.32", 
                "0.49", "0.75", "0.94", "1.12", "1.31", "1.87", "2.25", "2.51", 
                "2.68", "2.79", "2.87", "2.92", "2.95", "2.98", "2.99", "3.00" };

            Console.WriteLine("maxRes = {0} ratio = {1}", maxRes, maxRatio);
            Console.WriteLine("Original mesh:");
            Console.WriteLine(String.Join(", ", mesh.Select(i => String.Format("{0:f2}", i)).ToArray()));

            Assert.True(CompareGrid(referenceMesh, smoothMesh));
            */
        }

        private bool CompareGrid(List<string> expected, List<double> actual, uint precision = 2)
        {
            //Console.Write("Smoothed mesh: ");
            //Console.WriteLine(String.Join(", ", actual.Select(i => String.Format("{0:f2}", i)).ToArray()));

            //Console.Write("Expected mesh: ");
            //Console.WriteLine(String.Join(", ", expected.Select(i => String.Format("{0:f2}", i)).ToArray()));

            if (actual.Count != expected.Count)
            {
                Console.WriteLine("Error: Expected {0} elements but got {1} instead", expected.Count, actual.Count);
                return false;
            }

            for (int i = 0; i < actual.Count; i++)
            {
                if (!String.Equals(String.Format("{0:f2}", actual.ElementAt(i)), expected.ElementAt(i)))
                {
                    Console.WriteLine("Error: Expected {0} at {1} but got {2} instead",
                        expected.ElementAt(i), i, actual.ElementAt(i));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Reconstruct the openEMS provided Dipole SAR model and compare some
        /// basic properties of the two.
        /// </summary>
        [Fact]
        public void DipoleSar_BuildModel_MatchesReference()
        {
            string fileName = "Dipole_SAR.xml";
            XElement refDoc = XDocument.Load(fileName).Element("openEMS");
            XElement dutDoc = BuildDipoleSarXml().Element("openEMS");

            // FDTD parameters
            XElement refFdtd = refDoc.Element("FDTD");
            XElement dutFdtd = dutDoc.Element("FDTD");

            Assert.Equal(refFdtd.Attribute("f_max").Value, dutFdtd.Attribute("f_max").Value);
            Assert.Equal(
                Double.Parse(refFdtd.Attribute("endCriteria").Value, CultureInfo.InvariantCulture),
                Double.Parse(dutFdtd.Attribute("endCriteria").Value, CultureInfo.InvariantCulture));
            Assert.Equal(
                Double.Parse(refFdtd.Attribute("NumberOfTimesteps").Value, CultureInfo.InvariantCulture),
                Double.Parse(dutFdtd.Attribute("NumberOfTimesteps").Value, CultureInfo.InvariantCulture));
            Assert.Equal(
                (from ra in refFdtd.Element("BoundaryCond").Attributes() select ra.Value).ToList(),
                (from da in dutFdtd.Element("BoundaryCond").Attributes() select da.Value).ToList());

            // Material properties
            XElement refProps = refDoc.Element("ContinuousStructure").Element("Properties");
            XElement dutProps = dutDoc.Element("ContinuousStructure").Element("Properties");

            Assert.Equal(
                (from ra in refProps.Elements() orderby ra.Attribute("Name").Value.ToString() select ra.Attribute("Name").Value.ToString()).ToList(),
                (from da in dutProps.Elements() orderby da.Attribute("Name").Value.ToString() select da.Attribute("Name").Value.ToString()).ToList());

            // Grid (number of elements, mins, maxs)
            XElement refGrid = refDoc.Element("ContinuousStructure").Element("RectilinearGrid");
            XElement dutGrid = dutDoc.Element("ContinuousStructure").Element("RectilinearGrid");

            Assert.Equal(
                Double.Parse(refGrid.Attribute("DeltaUnit").Value, CultureInfo.InvariantCulture),
                Double.Parse(dutGrid.Attribute("DeltaUnit").Value, CultureInfo.InvariantCulture));
            Assert.Equal(
                Double.Parse(refGrid.Attribute("CoordSystem").Value, CultureInfo.InvariantCulture),
                Double.Parse(dutGrid.Attribute("CoordSystem").Value, CultureInfo.InvariantCulture));

            foreach (var gridName in new string[] { "XLines", "YLines", "ZLines" })
            {
                double[] refLines = (from l in refGrid.Element(gridName).Value.ToString().Split(',') select Double.Parse(l, CultureInfo.InvariantCulture)).ToArray();
                double[] dutLines = (from l in dutGrid.Element(gridName).Value.ToString().Split(',') select Double.Parse(l, CultureInfo.InvariantCulture)).ToArray();

                for (int i = 0; i < refLines.Length; i++)
                {
                    Assert.Equal(refLines[i], dutLines[i], 9);
                }
            }
        }

        private XDocument BuildDipoleSarXml()
        {
            double unit = 1e-3;

            double f0 = 1e9;
            double c0 = 299792458.0;
            double lambda0 = c0 / f0;

            double fStop = 1.5e9;
            double lambdaMin = c0 / fStop;

            // Simulation engine
            Simulation fdtd = new Simulation();
            fdtd.Excitation = new GaussExcitation(0, fStop); // possible typo in Dipole_SAR.xml

            // Simulation space
            Compound s = new Compound("space");

            // Dipole antenna
            double dipoleLength = 0.46 * lambda0 / unit;

            s.Add(new Box(null, new Metal("Dipole"), 1,
                new Vector3D(0, 0, -dipoleLength / 2), new Vector3D(0, 0, dipoleLength / 2)));

            // Phantom
            Compound headPhantom = new Compound("head-phantom");

            Dielectric skinMaterial = new Dielectric("skin", 50, kappa: 0.65, density: 1100);
            skinMaterial.FillColor = new Material.Color(245, 215, 205, 250);
            skinMaterial.EdgeColor = new Material.Color(255, 235, 217, 250);
            Sphere skin = new Sphere(null, skinMaterial, 11, new Vector3D(), 1);
            skin.Transformations.Add(new TScale(80, 100, 100));
            headPhantom.Add(skin);

            Dielectric boneMaterial = new Dielectric("headbone", 13, kappa: 0.1, density: 2000);
            boneMaterial.FillColor = new Material.Color(227, 227, 227, 250);
            boneMaterial.EdgeColor = new Material.Color(202, 202, 202, 250);
            Sphere bone = new Sphere(null, boneMaterial, 12, new Vector3D(), 1);
            bone.Transformations.Add(new TScale(75, 95, 95));
            headPhantom.Add(bone);

            Dielectric brainMaterial = new Dielectric("brain", 60, kappa: 0.7, density: 1040);
            brainMaterial.FillColor = new Material.Color(255, 85, 127, 250);
            brainMaterial.EdgeColor = new Material.Color(71, 222, 179, 250);
            Sphere brain = new Sphere(null, brainMaterial, 13, new Vector3D(), 1);
            brain.Transformations.Add(new TScale(65, 85, 85));
            headPhantom.Add(brain);

            headPhantom.Transformations.Add(new TTranslate(100, 0, 0));

            s.Add(headPhantom);

            // Excitation
            double meshResAir = lambdaMin / 20 / unit;
            double meshResPhantom = 2.5;

            LumpedPort lp = new LumpedPort(100, 1, 50.0,
                new Vector3D(-0.1, -0.1, -meshResPhantom / 2),
                new Vector3D(+0.1, +0.1, +meshResPhantom / 2), ENormDir.Z, true);
            s.Add(lp);

            // Grid
            RectilinearGrid g = new RectilinearGrid();

            g.XLines.Add(0);
            g.YLines.Add(0);
            foreach (double z in new double[] { -1.0 / 3, 2.0 / 3 })
            {
                g.ZLines.Add(-dipoleLength / 2 - meshResPhantom * z);
                g.ZLines.Add(+dipoleLength / 2 + meshResPhantom * z);
            }

            foreach (Sphere sp in new Sphere[] { skin, bone, brain })
            {
                g.XLines.Add(sp.AbsoluteTransformation.Matrix[0, 3] + sp.AbsoluteTransformation.Matrix[0, 0]);
                g.XLines.Add(sp.AbsoluteTransformation.Matrix[0, 3] - sp.AbsoluteTransformation.Matrix[0, 0]);
                g.YLines.Add(sp.AbsoluteTransformation.Matrix[1, 3] + sp.AbsoluteTransformation.Matrix[1, 1]);
                g.YLines.Add(sp.AbsoluteTransformation.Matrix[1, 3] - sp.AbsoluteTransformation.Matrix[1, 1]);
                g.ZLines.Add(sp.AbsoluteTransformation.Matrix[2, 3] + sp.AbsoluteTransformation.Matrix[2, 2]);
                g.ZLines.Add(sp.AbsoluteTransformation.Matrix[2, 3] - sp.AbsoluteTransformation.Matrix[2, 2]);
            }

            g.ZLines.Add(-meshResPhantom / 2); // port
            g.ZLines.Add(+meshResPhantom / 2);

            // Mesh over dipole and phantom
            g.SmoothMesh(meshResPhantom);

            g.XLines.Add(-200);
            g.XLines.Add(250 + 100);
            g.YLines.Add(-250);
            g.YLines.Add(+250);
            g.ZLines.Add(-250);
            g.ZLines.Add(+250);

            g.SmoothMesh(meshResAir, 1.2);

            s.Add(new SARBox("SAR", f0, new Vector3D(-10, -100, -100), new Vector3D(180, 100, 100)));
            s.Add(new NF2FFBox("nf2ff",
                new Vector3D(g.XLines.First(), g.YLines.First(), g.ZLines.First()),
                new Vector3D(g.XLines.Last(), g.YLines.Last(), g.ZLines.Last()),
                lambdaMin / 15 / unit));

            g.AddPML(10);

            g.XLines.Sort();
            g.YLines.Sort();
            g.ZLines.Sort();

            // Export
            return new XDocument(
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
        }
    }
}
