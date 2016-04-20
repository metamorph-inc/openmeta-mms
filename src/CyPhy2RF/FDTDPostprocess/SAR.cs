using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;


namespace Postprocess
{
    public class SAR
    {
        public double Frequency { get; private set; }
        public double[][] Mesh { get; private set; }
        public double[, ,] Field { get; private set; }
        public string FileName { get; private set; }

        public enum ENormDir
        {
            X, Y, Z
        }

        private SAR()
        {
            FileName = null;
            Mesh = null;
            Field = null;
            FileName = null;
        }

        public SAR(string fileName)
        {
            FileName = fileName;
            Frequency = HDF5.ReadAttribute(fileName, @"/FieldData/FD", "frequency");
            Mesh = HDF5.ReadMesh(fileName);
            Field = HDF5.ReadFieldData3D(fileName);
        }

        public SAR GetFieldRange(double[][] ranges)
        {
            SAR dump = new SAR();

            dump.FileName = FileName;
            dump.Frequency = Frequency;

            // Generate indeces
            int[][] idxRanges = new int[3][];
            for (int n = 0; n < 3; n++)
            {
                if (ranges[n].Length == 0) // All
                {
                    idxRanges[n] = new int[Mesh[n].Length];
                    for (int i = 0; i < Mesh[n].Length; i++)
                    {
                        idxRanges[n][i] = i;
                    }
                }
                else if (ranges[n].Length == 1) // Single
                {
                    idxRanges[n] = new int[1];

                    if (ranges[n][0] <= Mesh[n].First())
                    {
                        idxRanges[n][0] = 0;
                    }
                    else if (ranges[n][0] >= Mesh[n].Last())
                    {
                        idxRanges[n][0] = Mesh[n].Length - 1;
                    }
                    else
                    {
                        int i = 0;
                        while (i < Mesh[n].Length && Mesh[n][i] < ranges[n][0])
                        {
                            i++;
                        }
                        idxRanges[n][0] = Math.Abs(Mesh[n][i] - ranges[n][0]) < Math.Abs(Mesh[n][i - 1] - ranges[n][0]) ? i : i - 1;
                    }
                }
                else // Some
                {
                    throw new NotImplementedException();
                }
            }

            dump.Mesh = new double[3][];
            for (int n = 0; n < 3; n++)
            {
                dump.Mesh[n] = new double[idxRanges[n].Length];
                for (int i = 0; i < idxRanges[n].Length; i++)
                {
                    dump.Mesh[n][i] = Mesh[n][idxRanges[n][i]];
                }
            }

            // Create field
            dump.Field = new double[idxRanges[0].Length, idxRanges[1].Length, idxRanges[2].Length];
            for (int i = 0; i < idxRanges[0].Length; i++)
            {
                for (int j = 0; j < idxRanges[1].Length; j++)
                {
                    for (int k = 0; k < idxRanges[2].Length; k++)
                    {
                        dump.Field[i, j, k] =
                            Field[idxRanges[0][0] + i, idxRanges[1][0] + j, idxRanges[2][0] + k];
                    }
                }
            }

            return dump;
        }

        public FieldSlice GetFieldSlice(ENormDir direction, double location)
        {
            double[,] field;
            double[][] mesh;
            SAR dumpSlice;

            switch (direction)
            {
                case ENormDir.X:
                    dumpSlice = GetFieldRange(new double[3][] { new double[] { location }, new double[] { }, new double[] { } });
                    field = new double[Mesh[1].Length, Mesh[2].Length];
                    for (int y = 0; y < Mesh[1].Length; y++)
                    {
                        for (int z = 0; z < Mesh[2].Length; z++)
                        {
                            field[y, z] = dumpSlice.Field[0, y, z];
                        }
                    }
                    mesh = new double[2][] { Mesh[1], Mesh[2] };
                    break;

                case ENormDir.Y:
                    dumpSlice = GetFieldRange(new double[3][] { new double[] { }, new double[] { location }, new double[] { } });
                    field = new double[Mesh[0].Length, Mesh[2].Length];
                    for (int x = 0; x < Mesh[0].Length; x++)
                    {
                        for (int z = 0; z < Mesh[2].Length; z++)
                        {
                            field[x, z] = dumpSlice.Field[x, 0, z];
                        }
                    }
                    mesh = new double[2][] { Mesh[0], Mesh[2] };
                    break;

                case ENormDir.Z:
                    dumpSlice = GetFieldRange(new double[3][] { new double[] { }, new double[] { }, new double[] { location } });
                    field = new double[Mesh[0].Length, Mesh[1].Length];
                    for (int x = 0; x < Mesh[0].Length; x++)
                    {
                        for (int y = 0; y < Mesh[1].Length; y++)
                        {
                            field[x, y] = dumpSlice.Field[x, y, 0];
                        }
                    }
                    mesh = new double[2][] { Mesh[0], Mesh[1] };
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new FieldSlice(mesh, field);
        }

        public double[][] GetFieldMesh(ENormDir direction)
        {
            double[][] mesh = new double[2][];

            switch (direction)
            {
                case ENormDir.X:
                    mesh[0] = this.Mesh[1];
                    mesh[1] = this.Mesh[2];
                    break;

                case ENormDir.Y:
                    mesh[0] = this.Mesh[0];
                    mesh[1] = this.Mesh[2];
                    break;

                case ENormDir.Z:
                    mesh[0] = this.Mesh[0];
                    mesh[1] = this.Mesh[1];
                    break;

                default:
                    break;
            }

            return mesh;
        }

        /// <summary>
        /// Exports a slice of the SAR field at the plane defined by the normal direction and location parameters.
        /// </summary>
        /// <param name="fileName">Name of the output .png file.</param>
        /// <param name="direction">Normal direction of the SAR slice plane.</param>
        /// <param name="location">Location of the slice along the normal direction.</param>
        public void ToPNG(string fileName, ENormDir direction, double location)
        {
            int imageSize = 512; // TBD
            double clipdB = -40.0; // TBD

            FieldSlice fieldSlice = GetFieldSlice(direction, location);

            int pixelSizeX, pixelSizeY;
            double fieldSizeX = fieldSlice.Mesh[0].Last() - fieldSlice.Mesh[0].First();
            double fieldSizeY = fieldSlice.Mesh[1].Last() - fieldSlice.Mesh[1].First();
            double stepSize;

            if (fieldSizeX > fieldSizeY)
            {
                pixelSizeX = imageSize;
                pixelSizeY = (int)(imageSize * fieldSizeY / fieldSizeX);
                stepSize = fieldSizeX / (imageSize - 1);
            }
            else
            {
                pixelSizeY = imageSize;
                pixelSizeX = (int)(imageSize * fieldSizeX / fieldSizeY);
                stepSize = fieldSizeY / (imageSize - 1);
            }
                 

            Bitmap bmp = new Bitmap(pixelSizeX, pixelSizeY, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            double fieldStartX = fieldSlice.Mesh[0][0];
            double fieldStartY = fieldSlice.Mesh[1][0];
            double fieldMax = fieldSlice.Max();

            for (int x = 0; x < pixelSizeX; x++)
            {
                for (int y = 0; y < pixelSizeY; y++)
                {
                    double valSAR = fieldSlice.GetValueAt(
                        fieldSlice.Mesh[0][0] + x * stepSize,
                        fieldSlice.Mesh[1][0] + y * stepSize);

                    double valSARdB = 10 * Math.Log10(valSAR);
                    valSARdB -= 10 * Math.Log10(fieldMax);

                    double hueSARdB = valSARdB / clipdB * 240.0;
                    hueSARdB = hueSARdB > 240.0 ? 240.0 : hueSARdB;
                    bmp.SetPixel(x, y, ColorFromHSV(hueSARdB, 1.0, 0.9));
                }
            }

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        /// <summary>
        /// Based on http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
        /// </summary>
        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60.0)) % 6;
            double f = hue / 60.0 - Math.Floor(hue / 60.0);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0:
                    return Color.FromArgb(v, t, p);
                case 1:
                    return Color.FromArgb(q, v, p);
                case 2:
                    return Color.FromArgb(p, v, t);
                case 3:
                    return Color.FromArgb(p, q, v);
                case 4:
                    return Color.FromArgb(t, p, v);
                default:
                    return Color.FromArgb(v, p, q);
            }
        }

        public void ToVTK(string vtkPrefix, double weight = 1.0, string fieldName = "SAR_local")
        {
            string fileName = vtkPrefix + "_" + Frequency + ".vtk";
            StreamWriter vtkFile = new StreamWriter(fileName);

            vtkFile.WriteLine("# vtk DataFile Version 2.0");
            vtkFile.WriteLine("Rectilinear grid generated by the CyPhy tool for openEMS");
            vtkFile.WriteLine("ASCII");
            vtkFile.WriteLine("DATASET RECTILINEAR_GRID");
            vtkFile.WriteLine("DIMENSIONS {0} {1} {2}", Mesh[0].Length, Mesh[1].Length, Mesh[2].Length);

            vtkFile.WriteLine("X_COORDINATES {0} double", Mesh[0].Length);
            vtkFile.WriteLine(String.Join(" ", Mesh[0].Select(i => String.Format("{0:e}", i))));
            vtkFile.WriteLine("Y_COORDINATES {0} double", Mesh[1].Length);
            vtkFile.WriteLine(String.Join(" ", Mesh[1].Select(i => String.Format("{0:e}", i))));
            vtkFile.WriteLine("Z_COORDINATES {0} double", Mesh[2].Length);
            vtkFile.WriteLine(String.Join(" ", Mesh[2].Select(i => String.Format("{0:e}", i))));
            vtkFile.WriteLine();

            vtkFile.WriteLine("POINT_DATA {0}", Mesh[0].Length * Mesh[1].Length * Mesh[2].Length);

            // Assume scalar field
            vtkFile.WriteLine("SCALARS " + fieldName + " double 1");
            vtkFile.WriteLine("LOOKUP_TABLE default");

            for (int k = 0; k < Field.GetLength(2); k++)
            {
                for (int j = 0; j < Field.GetLength(1); j++)
                {
                    for (int i = 0; i < Field.GetLength(0); i++)
                    {
                        vtkFile.WriteLine("{0,16:e8}", (double)Field[i, j, k] * 3.6740e+026);
                    }
                }
            }
            vtkFile.Close();
        }

        public override string ToString()
        {
            string s = "HDF5 file: " + FileName + Environment.NewLine;
            s += "Frequency: " + Frequency + Environment.NewLine;
            s += "Field: (" + Field.GetLength(0) + "," + Field.GetLength(1)
            + "," + Field.GetLength(2) + ")";
            return s;
        }

        public double MaxValue
        {
            get
            {
                double maxVal = Field[0, 0, 0];
                for (int i = 0; i < Field.GetLength(0); i++)
                {
                    for (int j = 0; j < Field.GetLength(1); j++)
                    {
                        for (int k = 0; k < Field.GetLength(2); k++)
                        {
                            maxVal = Field[i, j, k] > maxVal ? Field[i, j, k] : maxVal;
                        }
                    }
                }
                return maxVal;
            }
        }

        public double[] MaxCoordinates
        {
            get
            {
                double maxVal = Field[0, 0, 0];
                int[] maxIndeces = new int[3] { 0, 0, 0 };
                for (int i = 0; i < Field.GetLength(0); i++)
                {
                    for (int j = 0; j < Field.GetLength(1); j++)
                    {
                        for (int k = 0; k < Field.GetLength(2); k++)
                        {
                            if (Field[i, j, k] > maxVal)
                            {
                                maxVal = Field[i, j, k];
                                maxIndeces = new int[3] { i, j, k };
                            }
                        }
                    }
                }
                return new double[] { Mesh[0][maxIndeces[0]], Mesh[1][maxIndeces[1]], Mesh[2][maxIndeces[2]] };
            }
        }
    }

    public class FieldSlice
    {
        public double[][] Mesh;
        public double[,] Field;

        public FieldSlice(double[][] mesh, double[,] field)
        {
            if (mesh.GetLength(0) != 2)
            {
                throw new ArgumentException();
            }
            Mesh = mesh;
            Field = field;
        }

        public double Max()
        {
            double maxVal = Field[0, 0];

            for (int i = 0; i < Field.GetLength(0); i++)
            {
                for (int j = 0; j < Field.GetLength(1); j++)
                {
                    maxVal = Field[i, j] > maxVal ? Field[i, j] : maxVal;
                }
            }

            return maxVal;
        }

        public double GetFieldLengh(int d)
        {
            return Mesh[d].Last() - Mesh[d].First();
        }


        /// <summary>
        /// Return the field-slice value at (x,y) with interpolation if necessary.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns></returns>
        public double GetValueAt(double x, double y)
        {
            int[,] boundingIndeces = GetBoundingMeshIndeces(x, y);

            // Indeces
            int xi1 = boundingIndeces[0, 0];
            int xi2 = boundingIndeces[0, 1];
            int yi1 = boundingIndeces[1, 0];
            int yi2 = boundingIndeces[1, 1];

            // Coordinates
            double x1 = Mesh[0][xi1];
            double x2 = Mesh[0][xi2];
            double y1 = Mesh[1][yi1];
            double y2 = Mesh[1][yi2];

            // Interpolate along axis x
            double u1, u2;
            if (x1 == x2)
            {
                u1 = Field[xi1, yi1];
                u2 = Field[xi1, yi2];
            }
            else
            {
                u1 = (Field[xi1, yi1] * (x2 - x) + Field[xi2, yi1] * (x - x1)) / (x2 - x1);
                u2 = (Field[xi1, yi2] * (x2 - x) + Field[xi2, yi2] * (x - x1)) / (x2 - x1);
            }

            // Interpolate along axis y
            double v;
            if (y1 == y2)
            {
                v = u1;
            }
            else
            {
                v = (u1 * (y2 - y) + u2 * (y - y1)) / (y2 - y1);
            }

            return v;
        }

        public int[,] GetBoundingMeshIndeces(double x, double y)
        {
            if (x < Mesh[0].First() || x > Mesh[0].Last() ||
                y < Mesh[1].First() || y > Mesh[1].Last())
            {
                throw new ArgumentException();
            }

            int[,] mi = new int[2, 2];
            for (int i = 0; i < Mesh[0].Length; i++)
            {
                if (Mesh[0][i] == x)
                {
                    mi[0, 0] = i;
                    mi[0, 1] = i;
                    break;
                }

                if (Mesh[0][i] > x)
                {
                    mi[0, 0] = i - 1;
                    mi[0, 1] = i;
                    break;
                }
            }

            for (int i = 0; i < Mesh[1].Length; i++)
            {
                if (Mesh[1][i] == y)
                {
                    mi[1, 0] = i;
                    mi[1, 1] = i;
                    break;
                }

                if (Mesh[1][i] > y)
                {
                    mi[1, 0] = i - 1;
                    mi[1, 1] = i;
                    break;
                }
            }

            return mi;
        }
    }
}
