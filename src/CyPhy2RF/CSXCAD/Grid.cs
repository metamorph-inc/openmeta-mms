using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CSXCAD
{
    public class RectilinearGrid
    {
        protected const uint CoordSystem = 0;
        public const double DeltaUnit = 0.001; // mm
        public List<double> XLines = new List<double>();
        public List<double> YLines = new List<double>();
        public List<double> ZLines = new List<double>();
        private double m_maxResolution = 0.0;

        public RectilinearGrid()
        {
        }

        public List<double>[] Mesh
        {
            get
            {
                return new List<double>[3] { XLines, YLines, ZLines };
            }
        }

        public void AddAirbox(double padding)
        {
            foreach (var lines in Mesh)
            {
                lines.Sort();
                lines.Insert(0, lines.First() - padding);
                lines.Add(lines.Last() + padding);
            }
        }

        public void AddPML(uint p)
        {
            if (m_maxResolution <= 0.0)
            {
                Console.WriteLine("Error: No maximum grid resolution set (try smoothing the grid first)");
                return;
            }

            foreach (var lines in Mesh)
            {
                lines.Sort();
                for (uint i = 0; i < p; i++)
                {
                    // Based on boundary cell distances
                    lines.Insert(0, lines.First() - (lines.ElementAt(1) - lines.First()));
                    lines.Add(lines.Last() + (lines.Last() - lines.ElementAt(lines.Count - 2)));
                    /*
                    // Based on maxResoultion
                    lines.Insert(0, lines.First() - maxResolution);
                    lines.Add(lines.Last() + maxResolution);
                    */
                }
            }
        }

        /// <summary>
        /// Adds the coordinates of a vector to the lines of the grid.
        /// </summary>
        /// <param name="v">The vector containing the coordinates to be added to the grid lines.</param>
        public void Add(Vector3D v)
        {
            XLines.Add(v.x);
            YLines.Add(v.y);
            ZLines.Add(v.z);
            XLines = XLines.Distinct().ToList();
            YLines = YLines.Distinct().ToList();
            ZLines = ZLines.Distinct().ToList();
        }

        /// <summary>
        /// Shifts the existing grid points with the amount defined by the vector coordinates.
        /// </summary>
        /// <param name="v">The coordinates defining the amount of shifting.</param>
        public void Move(Vector3D v)
        {
            XLines = (from line in XLines select line + v.x).ToList();
            YLines = (from line in YLines select line + v.y).ToList();
            ZLines = (from line in ZLines select line + v.z).ToList();
        }

        public void Sort()
        {
            XLines.Sort();
            YLines.Sort();
            ZLines.Sort();
            XLines = XLines.Distinct().ToList();
            YLines = YLines.Distinct().ToList();
            ZLines = ZLines.Distinct().ToList();
        }

        public void SmoothMesh(double maxRes, double ratio = 1.5)
        {
            m_maxResolution = maxRes;
            foreach (var lines in Mesh)
            {
                List<double> smoothLines = SmoothLines(lines, maxRes, ratio);
                lines.Clear();
                lines.AddRange(smoothLines);
            }
            Sort();
        }

        public static List<double> SmoothLines(List<double> mesh, double maxRes, double ratio)
        {
            double maxRatio = 1.25 * maxRes;
            double[] lines;

            mesh.Sort();
            lines = mesh.Distinct().ToArray();

            List<double> newLines = new List<double>();

            for (uint i = 0; i < lines.Length - 1; i++)
            {
                if (lines[i + 1] - lines[i] > 1.001 * maxRes)
                {
                    double leftRes = (i == 0) ? maxRes : lines[i] - lines[i - 1];
                    double rightRes = (i + 1 == lines.Length - 1) ? maxRes : lines[i + 2] - lines[i + 1];

                    newLines.AddRange(SmoothRange(lines[i], lines[i + 1], leftRes, rightRes, maxRes, ratio));
                }
            }

            List<double> smoothLines = lines.ToList();
            smoothLines.AddRange(newLines);
            smoothLines.Sort();

            return smoothLines;
        }

        private static List<double> SmoothRange(double leftValue, double rightValue,
            double leftRes, double rightRes, double maxRes, double ratio)
        {
            // Left side
            double taper = leftRes * ratio;
            List<double> leftTaper = new List<double> { leftValue };

            while (taper < maxRes)
            {
                leftTaper.Add(leftTaper.Last() + taper);
                taper *= ratio;
            }

            // Right side
            taper = rightRes * ratio;
            List<double> rightTaper = new List<double> { rightValue };

            while (taper < maxRes)
            {
                rightTaper.Add(rightTaper.Last() - taper);
                taper *= ratio;
            }
            rightTaper.Sort();

            // Taper intersection
            while ((Math.Abs(rightTaper.First() - leftTaper.Last()) < maxRes) || (rightTaper.First() < leftTaper.Last()))
            {
                double leftTaperDiff = 0;
                if (leftTaper.Count > 1)
                {
                    leftTaperDiff = leftTaper.ElementAt(leftTaper.Count - 1) - leftTaper.ElementAt(leftTaper.Count - 2);
                }

                double rightTaperDiff = 0;
                if (rightTaper.Count > 1)
                {
                    rightTaperDiff = rightTaper.ElementAt(1) - rightTaper.ElementAt(0);
                }

                if (leftTaperDiff > rightTaperDiff)
                {
                    leftTaper.RemoveAt(leftTaper.Count-1);
                }
                else
                {
                    rightTaper.RemoveAt(0);
                }

                if (leftTaper.Count == 0 || rightTaper.Count == 0)
                {
                    break;
                }
            }


            List<double> smoothLines = new List<double>();
            if (leftTaper.Count == 0 || rightTaper.Count == 0)
            {
                smoothLines.AddRange(leftTaper);
                smoothLines.AddRange(rightTaper);
            }
            else
            {
                double gap = rightTaper.First() - leftTaper.Last();
                double n = Math.Ceiling(gap / maxRes) + 1;

                smoothLines.AddRange(leftTaper);
                for (int i = 1; i < n-1; i++)
                {
                    smoothLines.Add(leftTaper.Last() + i * gap / (n-1));
                }
                smoothLines.AddRange(rightTaper);
            }

            smoothLines.RemoveAt(0);
            smoothLines.RemoveAt(smoothLines.Count-1); 

            return smoothLines;
        }

        public virtual XElement ToXElement()
        {
            return new XElement("RectilinearGrid",
                new XAttribute("DeltaUnit", DeltaUnit),
                new XAttribute("CoordSystem", CoordSystem),
                new XElement("XLines", string.Join(",", XLines.Select(i => String.Format("{0:g}", i)))),
                new XElement("YLines", string.Join(",", YLines.Select(i => String.Format("{0:g}", i)))),
                new XElement("ZLines", string.Join(",", ZLines.Select(i => String.Format("{0:g}", i)))));
        }
    }

    public class SimpleGrid_6x3 : RectilinearGrid
    {
        public SimpleGrid_6x3(double resolution = 1.0)
        {
            for (double x = 0.0; x <= 3 * 23 - 3; x += resolution)
            {
                XLines.Add(x);
            }

            for (double y = 0.0; y <= 6 * 23 + 3; y += resolution)
            {
                YLines.Add(y);
            }

            for (double z = 0.0; z <= 5; z += resolution)
            {
                ZLines.Add(-z);
                ZLines.Add(+z);
            }
            ZLines = ZLines.Distinct().ToList();
            ZLines.Sort();
        }
    }

    public class BoundingGrid_6x3 : RectilinearGrid
    {
        public BoundingGrid_6x3()
        {
            XLines.Add(0.0);
            XLines.Add(3 * 23 - 3);
            YLines.Add(0.0);
            YLines.Add(6 * 23 + 3);
            ZLines.Add(-4.5);
            ZLines.Add(+4.5);
        }
    }
}
