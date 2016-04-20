using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CSXCAD
{
    public abstract class Transform
    {
        public enum EType
        {
            UNDEFINED,
            Scale,
            Translate,
            Rotate_Origin,
            Rotate_X,
            Rotate_Y,
            Rotate_Z,
            Matrix
        }

        public EType Type { get; private set; }

        public abstract double[] Parameters { get; }

        public abstract double[,] Matrix { get; }

        public double X
        {
            get { return Matrix[0, 3]; }
        }

        public double Y
        {
            get { return Matrix[1, 3]; }
        }

        public double Z
        {
            get { return Matrix[2, 3]; }
        }

        public static Transform FromXElement(XElement xt)
        {
            double[] args = (from string p in xt.Attribute("Argument").Value.Split(',')
                             select Double.Parse(p, CultureInfo.InvariantCulture)).ToArray();
            
            Transform.EType tt;
            if (Enum.TryParse(xt.Name.ToString(), true, out tt))
            {
                switch (tt)
                {
                    case EType.Scale:
                        if (args.Length != 3) throw new ArgumentException();
                        return new TScale(args[0], args[1], args[2]);

                    case EType.Translate:
                        if (args.Length != 3) throw new ArgumentException();
                        return new TTranslate(args[0], args[1], args[2]);

                    case EType.Rotate_Origin:
                        if (args.Length != 4) throw new ArgumentException();
                        return new TRotateOrigin(args[0], args[1], args[2], args[3]);

                    case EType.Rotate_X:
                        if (args.Length != 1) throw new ArgumentException();
                        return new TRotateX(args[0]);

                    case EType.Rotate_Y:
                        if (args.Length != 1) throw new ArgumentException();
                        return new TRotateY(args[0]);

                    case EType.Rotate_Z:
                        if (args.Length != 1) throw new ArgumentException();
                        return new TRotateZ(args[0]);

                    case EType.Matrix:
                        if (args.Length != 16) throw new ArgumentException();
                        return new TGeneral(args);

                    default:
                        break;
                }
            }

            return null;
        }

        public static Transform operator *(Transform t1, Transform t2)
        {
            double[,] m1 = t1.Matrix;
            double[,] m2 = t2.Matrix;
            int numRows = m1.GetLength(0);
            int numCols = m2.GetLength(0);

            double[] mo = new double[numRows*numCols];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    mo[i * numCols + j] = 0;
                    for (int k = 0; k < numCols; k++)
                    {
                        mo[i * numCols + j] += m2[i, k] * m1[k, j];
                    }
                }
            }
            return new TGeneral(mo);
        }

        public static Vector3D operator *(Transform t, Vector3D v)
        {
            double[] vi = new double[4] { v.x, v.y, v.z, 1 };
            double[] vo = new double[4] { 0, 0, 0, 0 };

            double[,] m = t.Matrix;
            int numRows = m.GetLength(0);
            int numCols = m.GetLength(1);

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    vo[i] += m[i, j] * vi[j];
                }
            }
            return new Vector3D(vo[0], vo[1], vo[2]);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Transform t = obj as Transform;
            if ((System.Object)t == null)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (this.Matrix[i, j] != t.Matrix[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool Equals(Transform t)
        {
            if ((object)t == null)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (this.Matrix[i, j] != t.Matrix[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode(); // FIXME
        } 

        public XElement ToXElement()
        {
            // Skip unity matrices
            if (this.Equals(new TGeneral()))
            {
                return null;
            }

            XElement xe = new XElement("Transformation");

            xe.Add(new XElement("Matrix", "", new XAttribute("Argument",
                String.Join(",", Parameters.Select(i => String.Format("{0:0.0###}", i))))));

            return xe;
        }

        public override string ToString()
        {
            string s = base.ToString() + ": " + Type.ToString();
            s += " (" + String.Join(",", (from double p in Parameters select String.Format("{0:f1}", p))) + ")";
            return s;
        }
    }

    public class TScale : Transform
    {
        public double x, y, z;

        public TScale(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override double[,] Matrix
        {
            get
            {
                return new double[4, 4]
                {
                    {x, 0, 0, 0},
                    {0, y, 0, 0},
                    {0, 0, z, 0},
                    {0, 0, 0, 1}
                };
            }
        }

        public override double[] Parameters
        {
            get { return new double[] { x, y, z }; }
        }
    }

    public class TTranslate : Transform
    {
        public double x, y, z;

        public TTranslate(Vector3D v)
            : this(v.x, v.y, v.z)
        {
        }

        public TTranslate(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override double[,] Matrix
        {
            get
            {
                return new double[4, 4]
                {
                    {1, 0, 0, x},
                    {0, 1, 0, y},
                    {0, 0, 1, z},
                    {0, 0, 0, 1}
                };
            }
        }

        public override double[] Parameters
        {
            get { return new double[] { x, y, z }; }
        }
    }

    public class TRotateOrigin : Transform
    {
        public double x, y, z, a;

        public TRotateOrigin(Vector3D v, double a)
            : this(v.x, v.y, v.z, a)
        {
        }

        public TRotateOrigin(double x, double y, double z, double a)
        {
            // Normalize the parameter vector
            Vector3D e = new Vector3D(x, y, z);
            e = e.UnitVector();

            this.x = e.x;
            this.y = e.y;
            this.z = e.z;
            this.a = a;
        }

        public override double[,] Matrix
        {
            get
            {
                return new double[4, 4]
                {
                    {x*x+(1-x*x)*Math.Cos(a), x*y*(1-Math.Cos(a))-z*Math.Sin(a), x*z*(1-Math.Cos(a))+y*Math.Sin(a), 0},
                    {y*x*(1-Math.Cos(a))+z*Math.Sin(a), y*y+(1-y*y)*Math.Cos(a), y*z*(1-Math.Cos(a))-x*Math.Sin(a), 0},
                    {z*x*(1-Math.Cos(a))-y*Math.Sin(a), z*y*(1-Math.Cos(a))+x*Math.Sin(a), z*z+(1-z*z)*Math.Cos(a), 0},
                    {0, 0, 0, 1}
                };
            }
        }

        public override double[] Parameters
        {
            get { return new double[] { x, y, z, a }; }
        }
    }

    public class TRotateX : Transform
    {
        double a;

        public TRotateX(double a)
        {
            this.a = a;
        }

        public override double[,] Matrix
        {
            get
            {
                return new double[4, 4]
                {
                    {1, 0, 0, 0},
                    {0, Math.Cos(a), -Math.Sin(a), 0},
                    {0, Math.Sin(a),  Math.Cos(a), 0},
                    {0, 0, 0, 1}
                };
            }
        }

        public override double[] Parameters
        {
            get { return new double[] { a }; }
        }
    }

    public class TRotateY : Transform
    {
        double a;

        public TRotateY(double a)
        {
            this.a = a;
        }

        public override double[,] Matrix
        {
            get
            {
                return new double[4, 4]
                {
                    { Math.Cos(a), 0, Math.Sin(a), 0},
                    {0, 1, 0, 0},
                    {-Math.Sin(a), 0, Math.Cos(a), 0},
                    {0, 0, 0, 1}
                };
            }
        }

        public override double[] Parameters
        {
            get { return new double[] { a }; }
        }
    }

    public class TRotateZ : Transform
    {
        double a;

        public TRotateZ(double a)
        {
            this.a = a;
        }

        public override double[,] Matrix
        {
            get
            {
                return new double[4, 4]
                {
                    {Math.Cos(a), -Math.Sin(a), 0, 0},
                    {Math.Sin(a),  Math.Cos(a), 0, 0},
                    {0, 0, 1, 0},
                    {0, 0, 0, 1}
                };
            }
        }

        public override double[] Parameters
        {
            get { return new double[] { a }; }
        }
    }

    public class TGeneral : Transform
    {
        private double[,] matrix;

        public TGeneral()
        {
            // Unity matrix
            matrix = new double[4, 4]
            {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };
        }

        public TGeneral(double[] matrixElements)
        {
            if (matrixElements.Length != 16)
            {
                throw new ArgumentException();
            }

            matrix = new double[4, 4];
            for (int i = 0; i < matrixElements.Length; i++)
            {
                matrix[i / 4, i % 4] = matrixElements[i];
            }
        }

        public override double[,] Matrix
        {
            get
            {
                return matrix;
            }
        }

        public override double[] Parameters
        {
            get
            {
                double[] r = new double[16];
                for (int i = 0; i < r.Length; i++)
                {
                    r[i] = matrix[i / 4, i % 4];
                }
                return r;
            }
        }
    }

    public enum ENormDir
    {
        UNDEFINED = -1,
        X = 0,
        Y = 1,
        Z = 2
    }
}
