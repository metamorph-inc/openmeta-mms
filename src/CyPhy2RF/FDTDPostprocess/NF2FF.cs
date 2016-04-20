using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Numerics;
using System.IO;
using HDF5DotNet;

namespace Postprocess
{

    public class NF2FF
    {
        double m_frequency;
        double[] m_theta;
        double[] m_phi;
        string m_resultFile;
        public double RadiatedPower { get; private set; }
        public double Directivity { get; private set; }

        public Complex[,] ETheta { get; private set; }
        public Complex[,] EPhi { get; private set; }
        public double[,] ENorm { get; private set; }
        public double[,] PRadiated { get; private set; }

        public NF2FF(double f, string resultFile = "FF_3D_pattern.h5")
        {
            m_frequency = f;
            m_resultFile = resultFile;
            ETheta = null;
            EPhi = null;
            ENorm = null;
            PRadiated = null;
            SetPolarMesh3D();
        }

        public void SetPolarMesh2D()
        {
            m_theta = Utility.LinearSpace(-Math.PI, Math.PI, 360 / 2 + 1);
            m_phi = new double[] { 0.0, Math.PI / 2 };
        }

        public void SetPolarMesh3D()
        {
            m_theta = Utility.LinearSpace(0, Math.PI, 180 / 2 + 1);
            m_phi = Utility.LinearSpace(0, 2 * Math.PI, 360 / 2 + 1);
        }

        public void ReadHDF5Result()
        {
            ReadHDF5Mesh();
            ReadHDF5FieldData();

            m_frequency = HDF5.ReadAttribute(m_resultFile, "/nf2ff", "Frequency");
            RadiatedPower = HDF5.ReadAttribute(m_resultFile, "/nf2ff", "Prad");
            Directivity = HDF5.ReadAttribute(m_resultFile, "/nf2ff", "Dmax");
        }

        private void ReadHDF5Mesh()
        {
            double[][] meshes = new double[3][];
            string[] meshNames = { "phi", "r", "theta" };

            H5FileId fileId = H5F.open(m_resultFile, H5F.OpenMode.ACC_RDONLY);

            if (HDF5.ReadAttribute(m_resultFile, "/Mesh", "MeshType") != 2)
            {
                Console.WriteLine("Error: Invalid NF2FF mesh type in <{0}>", m_resultFile);
                return;
            }

            for (int i = 0; i < meshNames.Length; i++)
            {
                H5DataSetId dsId = H5D.open(fileId, "/Mesh/" + meshNames[i]);
                H5DataTypeId dtId = H5D.getType(dsId);

                if (!H5T.equal(dtId, H5T.copy(H5T.H5Type.NATIVE_FLOAT)))
                {
                    Console.WriteLine("Error: Invalid dataset type, expected {0}", H5T.H5Type.NATIVE_FLOAT);
                }

                float[] mesh = new float[H5D.getStorageSize(dsId) / H5T.getSize(dtId)];
                H5D.read(dsId, dtId, new H5Array<float>(mesh));

                meshes[i] = mesh.Select(x => (double)x).ToArray();

                H5D.close(dsId);
                H5T.close(dtId);
            }

            H5F.close(fileId);

            m_theta = meshes[2];
            m_phi = meshes[0];
        }

        public void ReadHDF5FieldData()
        {
            double[,] re = HDF5.ReadFieldData2D(m_resultFile, "/nf2ff/E_theta/FD/f0_real");
            double[,] im = HDF5.ReadFieldData2D(m_resultFile, "/nf2ff/E_theta/FD/f0_imag");

            ETheta = new Complex[re.GetLength(0), re.GetLength(1)];
            for (int i = 0; i < re.GetLength(0); i++)
            {
                for (int j = 0; j < re.GetLength(1); j++)
                {
                    ETheta[i, j] = new Complex(re[i, j], im[i, j]);
                }
            }

            re = HDF5.ReadFieldData2D(m_resultFile, "/nf2ff/E_phi/FD/f0_real");
            im = HDF5.ReadFieldData2D(m_resultFile, "/nf2ff/E_phi/FD/f0_imag");

            EPhi = new Complex[re.GetLength(0), re.GetLength(1)];
            for (int i = 0; i < re.GetLength(0); i++)
            {
                for (int j = 0; j < re.GetLength(1); j++)
                {
                    EPhi[i, j] = new Complex(re[i, j], im[i, j]);
                }
            }

            ENorm = new double[re.GetLength(0), re.GetLength(1)];
            for (int i = 0; i < re.GetLength(0); i++)
            {
                for (int j = 0; j < re.GetLength(1); j++)
                {
                    ENorm[i, j] = Math.Sqrt(Complex.Abs(ETheta[i, j]) * Complex.Abs(ETheta[i, j])
                        + Complex.Abs(EPhi[i,j] * Complex.Abs(EPhi[i,j])));
                }
            }

            PRadiated = HDF5.ReadFieldData2D(m_resultFile, "/nf2ff/P_rad/FD/f0");
        }

        public XDocument ToXDocument()
        {
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("CyPhy generated descriptor for near-field to far-field conversion"),
                new XElement("nf2ff",
                    new XAttribute("Outfile", m_resultFile),
                    new XAttribute("freq", m_frequency),
                    (from p in new string[] { "xn", "xp", "yn", "yp", "zn", "zp" }
                     select new XElement("Planes", "",
                        new XAttribute("E_Field", "nf2ff_E_" + p + ".h5"),
                        new XAttribute("H_Field", "nf2ff_H_" + p + ".h5"))),
                    new XElement("theta", String.Join(",", m_theta)),
                    new XElement("phi", String.Join(",", m_phi))));

            return doc;
        }

        public void ToVTK(double scale = 1.0, string fileName = "FF_3D_pattern.vtk")
        {
            StreamWriter vtkFile = new StreamWriter(fileName);

            vtkFile.WriteLine("# vtk DataFile Version 3.0");
            vtkFile.WriteLine("Structured grid generated by the CyPhy tool for openEMS");
            vtkFile.WriteLine("ASCII");
            vtkFile.WriteLine("DATASET STRUCTURED_GRID");
            vtkFile.WriteLine("DIMENSIONS {0} {1} {2}", 1, m_theta.Length, m_phi.Length);
            vtkFile.WriteLine("POINTS {0} double", m_theta.Length * m_phi.Length);

            double maxENorm = ENorm.Cast<double>().Max();
            for (int i = 0; i < m_phi.Length; i++)
            {
                for (int j = 0; j < m_theta.Length; j++)
                {
                    vtkFile.WriteLine("{0:e} {1:e} {2:e}",
                        scale / maxENorm * Directivity * ENorm[j, i] * Math.Sin(m_theta[j]) * Math.Cos(m_phi[i]),
                        scale / maxENorm * Directivity * ENorm[j, i] * Math.Sin(m_theta[j]) * Math.Sin(m_phi[i]),
                        scale / maxENorm * Directivity * ENorm[j, i] * Math.Cos(m_theta[j]));
                }
            }

            vtkFile.WriteLine();
            vtkFile.WriteLine();

            vtkFile.WriteLine("POINT_DATA {0}", m_phi.Length * m_theta.Length);
            vtkFile.WriteLine("SCALARS gain double 1");
            vtkFile.WriteLine("LOOKUP_TABLE default");

            for (int i = 0; i < m_phi.Length; i++)
            {
                for (int j = 0; j < m_theta.Length; j++)
                {
                    vtkFile.WriteLine("{0,16:e8}", ENorm[j, i] / maxENorm * Directivity);
                }
            }
            vtkFile.Close();
        }
    }
}
 