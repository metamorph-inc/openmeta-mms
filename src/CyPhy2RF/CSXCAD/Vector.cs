using System;

namespace CSXCAD
{
    public class Vector2D
    {
        public double x, y;

        public Vector2D()
            : this(0.0, 0.0)
        {
        }

        public Vector2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double Length
        {
            get
            {
                return Math.Sqrt(x*x + y*y);
            }
        }

        public double[] Coordinates
        {
            get
            {
                return new double[] { x, y };
            }
        }

        public static Vector2D operator*(Vector2D v, double c)
        {
            return new Vector2D(c * v.x, c * v.y);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Vector2D v = obj as Vector2D;
            if ((System.Object)v == null)
            {
                return false;
            }

            return this.x == v.x && this.y == v.y;
        }

        public bool Equals(Vector2D v)
        {
            if ((object)v == null)
            {
                return false;
            }

            return this.x == v.x && this.y == v.y;
        }

        public override int GetHashCode()
        {
            return (int)x ^ (int)y;
        }

        public static Vector2D Rotate(Vector2D v, double a)
        {
            double x = Math.Cos(a) * v.x - Math.Sin(a) * v.y;
            double y = Math.Sin(a) * v.x + Math.Cos(a) * v.y;

            return new Vector2D(x, y);
        }
    }

    public class Vector3D : Vector2D
    {
        public double z;

        public Vector3D()
            : this(0.0, 0.0, 0.0)
        {
        }

        public Vector3D(double x, double y, double z)
            : base(x, y)
        {
            this.z = z;
        }

        public new double Length
        {
            get
            {
                return Math.Sqrt(x*x + y*y + z*z);
            }
        }

        public new double[] Coordinates
        {
            get
            {
                return new double[] { x, y, z };
            }
            set
            {
                if (value.Length != 3)
                {
                    throw new ArrayTypeMismatchException();
                }

                x = value[0];
                y = value[1];
                z = value[2];
            }
        }

        public double this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Vector3D v = obj as Vector3D;
            if ((System.Object)v == null)
            {
                return false;
            }

            return base.Equals(obj) && this.z == v.z;
        }

        public bool Equals(Vector3D v)
        {
            if ((object)v == null)
            {
                return false;
            }

            return base.Equals((Vector2D)v) && this.z == v.z;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (int)z;
        }

        public static Vector3D operator *(double c, Vector3D v)
        {
            return new Vector3D(c * v.x, c * v.y, c * v.z);
        }

        public static Vector3D operator +(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3D operator -(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3D operator -(Vector3D v)
        {
            return new Vector3D(-v.x, -v.y, -v.z);
        }

        public static Vector3D RotateX(Vector3D v, double a)
        {
            double y = Math.Cos(a) * v.y - Math.Sin(a) * v.z;
            double z = Math.Sin(a) * v.y + Math.Cos(a) * v.z;

            return new Vector3D(v.x, y, z);
        }

        public static Vector3D RotateY(Vector3D v, double a)
        {
            double x =  Math.Cos(a) * v.x + Math.Sin(a) * v.z;
            double z = -Math.Sin(a) * v.x + Math.Cos(a) * v.z;

            return new Vector3D(x, v.y, z);
        }

        public static Vector3D RotateZ(Vector3D v, double a)
        {
            double x = Math.Cos(a) * v.x - Math.Sin(a) * v.y;
            double y = Math.Sin(a) * v.x + Math.Cos(a) * v.y;

            return new Vector3D(x, y, v.z);
        }

        public static Vector3D RotateXYZ(Vector3D p, Vector3D e, double a)
        {
            throw new NotImplementedException();
        }

        public Vector3D UnitVector()
        {
            return new Vector3D(x/Length, y/Length, z/Length);
        }

        public override string ToString()
        {
            return string.Format("{0:F2},{1:F2},{2:F2}", x, y, z);
        }
    }
}