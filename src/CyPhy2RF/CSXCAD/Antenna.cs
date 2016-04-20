using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSXCAD
{
    namespace Antenna
    {
        /// <summary>
        /// 2.4 GHz Inverted F Antenna based on http://www.ti.com/lit/an/swru120b/swru120b.pdf
        /// </summary>
        public class InvertedF_2400MHz : Compound
        {
            const double H1 = 5.70;
            const double H2 = 0.74;
            const double H3 = 1.29;
            const double H4 = 2.21;
            const double H5 = 0.66;
            const double H6 = 1.21;
            const double H7 = 0.80;
            const double H8 = 1.80;
            const double H9 = 0.61;
            const double W1 = 1.21;
            const double W2 = 0.46;
            const double L1 = 25.58;
            const double L2 = 16.40;
            const double L3 = 2.18;
            const double L4 = 4.80;
            const double L5 = 1.00;
            const double L6 = 1.00;
            const double L7 = 3.20;
            const double L8 = 0.45;

            private LinearPolygon m_antenna;
            private double m_thickness;

            public Box BoundingBox
            {
                get
                {
                    double xMin = m_antenna.Points.Min(v => v.x);
                    double xMax = m_antenna.Points.Max(v => v.x);
                    double yMin = m_antenna.Points.Min(v => v.y);
                    double yMax = m_antenna.Points.Max(v => v.y);

                    return new Box(null, null,0, new Vector3D(xMin, yMin, 0), new Vector3D(xMax, yMax, m_thickness));
                }
            }

            public InvertedF_2400MHz(double thickness = 0.0, bool debug = false)
                : base("Antenna-2400MHz-Inverted-F")
            {
                Vector3D feedPoint = new Vector3D(L3 + L4 + L7 + L8 + W2 / 2, 0, 0);
                m_thickness = thickness;

                // Antenna
                Metal copper = new Metal("antenna-inverted-f");
                copper.FillColor = new Material.Color(235, 148, 7, 255);
                copper.EdgeColor = new Material.Color(235, 148, 7, 255);

                uint priority = 100;
                uint normDir = 2;

                double[,] points = new double[,] { 
                    { L3+L4+L7, L3+L4+L7, L3, L3, L3+L4+L5, L3+L4+L5, L3+L4, L3+L4, L3+L4+L7-0.1, L3+L4+L7+L8, L3+L4+L7+L8, L3+L4+L7+L8+W2, L3+L4+L7+L8+W2, L3+L4+L7, L3+L4+L7, L1-L2, L1-L2, L1, L1, 0, 0, L3+L4, L3+L4, L3+L4+L5+0.2, L3+L4+L5+0.2, L3+L4+L7-L6-0.2, L3+L4+L7-L6-0.2 },
                    { 0, H2+H3, H2+H3, H1+W1-H4, H1+W1-H4, H2+H3+H5+H6, H2+H3+H5+H6, H2+H3+H5, H2+H3+H5, H2+H3 + 0.2, 0, 0, H2+H3+Math.Sqrt(2)/2*W2 + W2/4, H2+H3+H5+Math.Sqrt(2)*W2 -0.08, H1-H8, H1-H8, H1, H1, H1+W1, H1+W1, H2, H2, 0, 0, H2, H2, 0 }
                };

                m_antenna = new LinearPolygon(null, copper, priority, normDir, 0.0, points, thickness);
                m_antenna.Move(-feedPoint); // move feedpoint to origin w/o transformations
                this.Add(m_antenna);

                if (debug)
                {
                    // Ground plane placeholders (priority = 0)
                    double groundPlaneWidth = 5.0;

                    Metal groundPlane = new Metal("ground-plane");
                    groundPlane.FillColor = new Material.Color(235, 148, 7, 255);
                    groundPlane.EdgeColor = new Material.Color(235, 148, 7, 255);

                    //Box groundPlaneLeft = new Box(null, groundPlane, 0, new Vector3D(-W2 / 2 - L8, -groundPlaneWidth, 0), new Vector3D(L3 + L4 + L7, 0, thickness));
                    Box groundPlaneLeft = new Box(null, groundPlane, 0, new Vector3D(0, -groundPlaneWidth, 0), new Vector3D(L3 + L4 + L7, 0, thickness));
                    Box groundPlaneRight = new Box(null, groundPlane, 0, new Vector3D(L3 + L4 + L7 + L8 + W2 + L8, -groundPlaneWidth, 0), new Vector3D(L1, 0, thickness));
                    groundPlaneLeft.Move(-feedPoint);
                    groundPlaneRight.Move(-feedPoint);
                    this.Add(groundPlaneLeft);
                    this.Add(groundPlaneRight);

                    /*
                    double pcbWidth = 0.56; // FIXME
                    Box groundPlaneBottom = new Box(null, groundPlane, 0, new Vector3D(0, -groundPlaneWidth, 0-pcbWidth), new Vector3D(L1, 0, thickness-pcbWidth));
                    groundPlaneBottom.Move(-feedPoint);
                    this.Add(groundPlaneBottom);

                    // Keep-off region
                    Dielectric keepOff = new Dielectric("keep-off");
                    keepOff.FillColor = new Material.Color(255, 0, 0, 32);
                    keepOff.EdgeColor = new Material.Color(255, 0, 0, 32);
                    Box keepOffArea = new Box(null, keepOff, 0, new Vector3D(-1, 0, 0), new Vector3D(L1 + 1, H1 + W1 + 1, thickness));
                    keepOffArea.Move(-feedPoint);
                    this.Add(keepOffArea);
                    */
                }
            }
        }

        /// <summary>
        /// 2.4 GHz Small-Size Antanna based on http://www.ti.com/lit/an/swra117d/swra117d.pdf
        /// </summary>
        public class Small_15x6mm_2400MHz : Compound
        {
            readonly double L1 = 3.94;
            readonly double L2 = 2.70;
            readonly double L3 = 5.00;
            readonly double L4 = 2.64;
            readonly double L5 = 2.00;
            readonly double L6 = 4.90;
            public readonly double W1 = 0.90;
            public readonly double W2 = 0.50;
            public readonly double D1 = 0.50;
            public readonly double D2 = 0.30;
            public readonly double D3 = 0.30;
            public readonly double D4 = 0.50;
            public readonly double D5 = 1.40;
            public readonly double D6 = 1.70;

            private double m_thickness;
            private LinearPolygon m_antenna;

            public List<Vector2D> antennaPoly
            {
                get
                {
                    return m_antenna.Points;
                }
            }

            public Small_15x6mm_2400MHz(double thickness = 0.01)
                : base("Patch-antenna-2400MHz-15x6mm")
            {
                Vector3D feedPoint = new Vector3D(W1 + D5 + W2 / 2, D4 / 2, 0.0);

                Metal copper = new Metal("copper");
                copper.FillColor = new Material.Color(235, 148, 7, 255);
                copper.EdgeColor = new Material.Color(235, 148, 7, 255);

                uint priority = 100;
                uint normDir = 2;
                m_thickness = thickness;

                double[,] points = new double[,]
                {
                    {0, 0},
                    {W1, 0},
                    {W1, L6},
                    {W1+D5, L6},
                    {W1+D5, 0},
                    {W1+D5+W2, 0},
                    {W1+D5+W2, L6},
                    {L3-W2, L6},
                    {L3-W2, L6-L4},
                    {L3+L5+W2, L6-L4},
                    {L3+L5+W2, L6},
                    {L3+L5+L2-W2, L6},
                    {L3+L5+L2-W2, L6-L4},
                    {L3+L5+L2+L5+W2, L6-L4},
                    {L3+L5+L2+L5+W2, L6},
                    {L3+L5+L2+L5+L2-W2, L6},
                    {L3+L5+L2+L5+L2-W2, L6-L1},
                    {L3+L5+L2+L5+L2, L6-L1},
                    {L3+L5+L2+L5+L2, L6+W2},
                    {L3+L5+L2+L5, L6+W2},
                    {L3+L5+L2+L5, L6-L4+W2},
                    {L3+L5+L2, L6-L4+W2},
                    {L3+L5+L2, L6+W2},
                    {L3+L5, L6+W2},
                    {L3+L5, L6-L4+W2},
                    {L3, L6-L4+W2},
                    {L3, L6+W2},
                    {0, L6+W2}
                };

                m_antenna = new LinearPolygon(null, copper, priority, normDir, 0.0, points, thickness);
                m_antenna.Move(-feedPoint);
                this.Add(m_antenna);
            }

            public Box BoundingBox
            {
                get
                {
                    double xMin = m_antenna.Points.Min(v => v.x);
                    double xMax = m_antenna.Points.Max(v => v.x);
                    double yMin = m_antenna.Points.Min(v => v.y);
                    double yMax = m_antenna.Points.Max(v => v.y);

                    return new Box(null, null, 0, new Vector3D(xMin, yMin, 0), new Vector3D(xMax, yMax, m_thickness));
                }
            }
        }
    }
}
