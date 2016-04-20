using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HDF5DotNet;

namespace Postprocess
{
    public class HDF5
    {
        public static double ReadAttribute(string file, string dataSetOrGroup, string attribute)
        {
            double attr = Double.NaN;

            try
            {
                H5FileId fileId = H5F.open(file, H5F.OpenMode.ACC_RDONLY);
                H5ObjectInfo objectInfo = H5O.getInfoByName(fileId, dataSetOrGroup);
                H5GroupId groupId = null;
                H5DataSetId dataSetId = null;
                H5AttributeId attrId;

                if (objectInfo.objectType == H5ObjectType.GROUP)
                {
                    groupId = H5G.open(fileId, dataSetOrGroup);
                    attrId = H5A.open(groupId, attribute);
                }
                else
                {
                    dataSetId = H5D.open(fileId, dataSetOrGroup);
                    attrId = H5A.open(dataSetId, attribute);
                }
                H5DataTypeId attrTypeId = H5A.getType(attrId);

                double[] dAttrs = new double[] { };
                if (H5T.equal(attrTypeId, H5T.copy(H5T.H5Type.NATIVE_FLOAT)))
                {
                    float[] fAttrs = new float[H5S.getSimpleExtentNPoints(H5A.getSpace(attrId))];
                    H5A.read(attrId, attrTypeId, new H5Array<float>(fAttrs));
                    dAttrs = (from f in fAttrs select (double)f).ToArray();
                }
                else if (H5T.equal(attrTypeId, H5T.copy(H5T.H5Type.NATIVE_DOUBLE)))
                {
                    dAttrs = new double[H5S.getSimpleExtentNPoints(H5A.getSpace(attrId))];
                    H5A.read(attrId, attrTypeId, new H5Array<double>(dAttrs));
                }

                H5T.close(attrTypeId);
                H5A.close(attrId);
                if (groupId != null) H5G.close(groupId);
                if (dataSetId != null) H5D.close(dataSetId);
                H5F.close(fileId);

                return (double)dAttrs[0];
            }

            catch (HDFException e)
            {
                Console.WriteLine("Error: Unhandled HDF5 exception");
                Console.WriteLine(e.Message);
            }

            return attr;
        }

        public static double[][] ReadMesh(string fileName)
        {
            double[][] meshes = new double[3][];
            string[] meshNames = { "x", "y", "z" };

            H5FileId fileId = H5F.open(fileName, H5F.OpenMode.ACC_RDONLY);

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

                meshes[i] = mesh.Select(x => (double)x * 1000.0).ToArray(); // m -> mm

                H5D.close(dsId);
                H5T.close(dtId);
            }

            H5F.close(fileId);

            return meshes;
        }

        public static double[,] ReadFieldData2D(string file, string dataSet)
        {
            H5FileId fileId = H5F.open(file, H5F.OpenMode.ACC_RDONLY);
            H5DataSetId fDataSetId = H5D.open(fileId, dataSet);
            H5DataTypeId fDataTypeId = H5D.getType(fDataSetId);

            long[] dims = H5S.getSimpleExtentDims(H5D.getSpace(fDataSetId)).ToArray();
            double[,] data = new double[dims[0], dims[1]];
            H5D.read(fDataSetId, fDataTypeId, new H5Array<double>(data));

            double[,] fieldValues = new double[dims[1], dims[0]];
            for (int i = 0; i < dims[1]; i++)
            {
                for (int j = 0; j < dims[0]; j++)
                {
                    fieldValues[i, j] = (double)data[j, i];
                }
            }

            H5T.close(fDataTypeId);
            H5D.close(fDataSetId);
            H5F.close(fileId);

            return fieldValues;
        }

        public static double[, ,] ReadFieldData3D(string fileName)
        {
            H5FileId fileId = H5F.open(fileName, H5F.OpenMode.ACC_RDONLY);
            H5DataSetId fDataSetId = H5D.open(fileId, "/FieldData/FD/f0");
            H5DataTypeId fDataTypeId = H5D.getType(fDataSetId);

            if (!H5T.equal(fDataTypeId, H5T.copy(H5T.H5Type.NATIVE_FLOAT)))
            {
                Console.WriteLine("Error: Invalid dataset type, expected {0}", H5T.H5Type.NATIVE_FLOAT);
            }

            long[] dims = H5S.getSimpleExtentDims(H5D.getSpace(fDataSetId)).ToArray();
            if (dims.Length != 3)
            {
                Console.WriteLine("Error: Invalid field data dimensions");
            }

            float[, ,] data = new float[dims[0], dims[1], dims[2]];
            H5D.read(fDataSetId, fDataTypeId, new H5Array<float>(data));

            // Reorder
            double[, ,] fieldValues = new double[dims[2], dims[1], dims[0]];
            for (int i = 0; i < dims[0]; i++)
            {
                for (int j = 0; j < dims[1]; j++)
                {
                    for (int k = 0; k < dims[2]; k++)
                    {
                        fieldValues[k, j, i] = data[i, j, k];
                    }
                }
            }

            return fieldValues;
        }
    }
}
