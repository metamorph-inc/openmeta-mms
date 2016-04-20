using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;


namespace CSXCAD
{
    public abstract class Solid
    {
        public Compound Parent = null;
        public List<Transform> Transformations = new List<Transform>();

        public Solid()
        {
        }

        public Solid(Compound par, Vector3D pos, double rot)
        {
            Parent = par;
            Transformations.Add(new TRotateZ(rot));
            Transformations.Add(new TTranslate(pos));
        }

        public Transform LocalTransformation
        {
            get
            {
                Transform lt = new TGeneral();
                foreach (Transform t in Transformations)
                {
                    lt *= t;
                }
                return lt;
            }
        }

        public List<Transform> AllTransformations
        {
            get
            {
                List<Transform> tl = new List<Transform>(Transformations);
                if (Parent != null)
                {
                    tl.AddRange(Parent.AllTransformations);
                }
                return tl;
            }
        }

        public Transform AbsoluteTransformation
        {
            get
            {
                if (AllTransformations.Count == 0)
                {
                    return null;
                }

                Transform at = new TGeneral();
                foreach (Transform t in AllTransformations)
                {
                    at *= t;
                }
                return at;
            }
        }

        public abstract XElement ToXElement();

        public override string ToString()
        {
            string s = base.GetType().ToString();
            if (Parent != null)
            {
                s += " < \"" + Parent.Name + "\"";
            }
            foreach (Transform t in AllTransformations)
            {
                s += t.ToString();
            }
            return s;
        }
    }


    public class Compound : Solid
    {
        public string Name { get; protected set; }
        protected List<Primitive> m_primitives;
        protected List<Compound> m_compounds;

        public Compound(string name)
            : this(null, name, new Vector3D(), 0)
        {
        }

        public Compound(Compound parent, string name, Vector3D pos, double rot)
            : base(parent, pos, rot)
        {
            Name = name;
            m_primitives = new List<Primitive>();
            m_compounds = new List<Compound>();
        }

        public virtual Box BoundingBox
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(Compound c)
        {
            c.Parent = this;
            m_compounds.Add(c);
        }

        public void Add(Primitive p)
        {
            p.Parent = this;
            m_primitives.Add(p);
        }

        public List<Primitive> Children
        {
            get
            {
                return m_primitives;
            }
        }

        public List<Primitive> AllChildren
        {
            get
            {
                List<Primitive> pl = new List<Primitive>();
                pl.AddRange(Children);
                foreach (Compound c in m_compounds)
                {
                    pl.AddRange(c.AllChildren);
                }
                return pl;
            }
        }

        public override XElement ToXElement()
        {
            Dictionary<Material, List<Primitive>> materials = new Dictionary<Material, List<Primitive>>();

            // Fill up primitive lists in materialDictionary
            foreach (Primitive p in AllChildren)
            {
                List<Primitive> materialUsers;
                if (materials.TryGetValue(p.Material, out materialUsers))
                {
                    materialUsers.Add(p);
                }
                else
                {
                    foreach (KeyValuePair<Material, List<Primitive>> m in materials)
                    {
                        if (m.Key.Name.Equals(p.Material.Name))
                        {
                            Console.WriteLine("Warning: material with name '" + p.Material.Name + "' already exists");
                        }
                    }
                    materialUsers = new List<Primitive>();
                    materialUsers.Add(p);
                    materials.Add(p.Material, materialUsers);
                }
            }

            XElement xp = new XElement("Properties");
            foreach (KeyValuePair<Material, List<Primitive>> m in materials)
            {
                XElement xm = m.Key.ToXElement();

                XElement xpr = new XElement("Primitives");
                foreach (Primitive p in m.Value)
                {
                    xpr.Add(p.ToXElement());
                }
                xm.Add(xpr);

                xp.Add(xm);
            }

            return xp;
        }

        public override string ToString()
        {
            return this.GetType().ToString() + "::" + Name + ": " + base.ToString();
        }
    }


    public class XmlCompound : Compound
    {
        public XmlCompound(Compound parent, string name, Vector3D pos, double rot)
            : base(parent, name, pos, rot)
        {
        }

        public void Parse(XElement xdoc)
        {
            XElement xprop = xdoc.Element("ContinuousStructure").Element("Properties");

            foreach (XElement xm in xprop.Elements())
            {
                // Parse material
                Material m = Material.FromXElement(xm);

                // Parse primitives
                foreach (XElement xprim in xm.Element("Primitives").Elements())
                {
                    Primitive prim = Primitive.FromXElement(xprim, m);
                    prim.Parent = this;

                    // Parse transformations
                    XElement xts = xprim.Element("Transformation");
                    if (xts != null)
                    {
                        foreach (XElement xt in xts.Elements())
                        {
                            Transform nt = Transform.FromXElement(xt);
                            prim.Transformations.Add(nt);
                        }
                    }
                  
                    m_primitives.Add(prim);
                }
            }
        }
    }

    public abstract class Primitive : Solid
    {
        protected uint m_priority;
        public Material Material { get; private set; }

        public Primitive(Compound parent, Material material, uint prio)
            : this(parent, material, prio, new Vector3D(0.0, 0.0, 0.0), 0) {
        }

        public Primitive(Compound parent, Material material, uint p, Vector3D t, double r)
            : base(parent, t, r)
        {
            Material = material;
            m_priority = p;
        }

        public abstract void Move(Vector3D v);

        public static Primitive FromXElement(XElement xprim, Material material = null)
        {
            Primitive prim;

            switch (xprim.Name.ToString())
            {
                case "Box":
                    prim = new Box(null, material,
                        Convert.ToUInt32(xprim.Attribute("Priority").Value),
                        new Vector3D(
                            Convert.ToDouble(xprim.Element("P1").Attribute("X").Value),
                            Convert.ToDouble(xprim.Element("P1").Attribute("Y").Value),
                            Convert.ToDouble(xprim.Element("P1").Attribute("Z").Value)),
                        new Vector3D(
                            Convert.ToDouble(xprim.Element("P2").Attribute("X").Value),
                            Convert.ToDouble(xprim.Element("P2").Attribute("Y").Value),
                            Convert.ToDouble(xprim.Element("P2").Attribute("Z").Value)));
                    break;

                case "LinPoly":
                    prim = new LinearPolygon(null, material,
                        Convert.ToUInt32(xprim.Attribute("Priority").Value),
                        Convert.ToUInt32(xprim.Attribute("NormDir").Value),
                        Convert.ToDouble(xprim.Attribute("Elevation").Value),
                        (from xvertex in xprim.Elements()
                         where xvertex.Name == "Vertex"
                         select new Vector2D(
                             Convert.ToDouble(xvertex.Attribute("X1").Value),
                             Convert.ToDouble(xvertex.Attribute("X2").Value))).ToList(),
                        Convert.ToDouble(xprim.Attribute("Length").Value));
                    break;

                case "Cylinder":
                    prim = new Cylinder(null, material,
                        Convert.ToUInt32(xprim.Attribute("Priority").Value),
                        new Vector3D(
                            Convert.ToDouble(xprim.Element("P1").Attribute("X").Value),
                            Convert.ToDouble(xprim.Element("P1").Attribute("Y").Value),
                            Convert.ToDouble(xprim.Element("P1").Attribute("Z").Value)),
                        new Vector3D(
                            Convert.ToDouble(xprim.Element("P2").Attribute("X").Value),
                            Convert.ToDouble(xprim.Element("P2").Attribute("Y").Value),
                            Convert.ToDouble(xprim.Element("P2").Attribute("Z").Value)),
                        Convert.ToDouble(xprim.Attribute("Radius").Value));
                    break;

                default:
                    Console.Error.WriteLine("Error: Unsupported primitive type " + xprim.Name.ToString());
                    return null;
            }

            return prim;
        }

        public override string ToString()
        {
            return base.GetType().ToString();
        }
    }

    public class Box : Primitive
    {
        public Vector3D P1, P2;

        public Box(Compound parent, Material m, uint p, Vector3D v0, Vector3D v1)
            : this(parent, m, p, v0, v1, new Vector3D(), 0)
        {
        }

        public Box(Compound parent, Material m, uint p, Vector3D v0, Vector3D v1, Vector3D t, double r)
            : base(parent, m, p, t, r)
        {
            P1 = v0;
            P2 = v1;
        }

        public override void Move(Vector3D v)
        {
            P1.x += v.x;
            P1.y += v.y;
            P1.z += v.z;

            P2.x += v.x;
            P2.y += v.y;
            P2.z += v.z;
        }

        public override XElement ToXElement()
        {
            XElement xe = new XElement("Box", new XAttribute("Priority", m_priority),
                        new XElement("P1", "",
                            new XAttribute("X", String.Format("{0:g}", P1.x)),
                            new XAttribute("Y", String.Format("{0:g}", P1.y)),
                            new XAttribute("Z", String.Format("{0:g}", P1.z))),
                        new XElement("P2", "",
                            new XAttribute("X", String.Format("{0:g}", P2.x)),
                            new XAttribute("Y", String.Format("{0:g}", P2.y)),
                            new XAttribute("Z", String.Format("{0:g}", P2.z))));

            xe.Add(AbsoluteTransformation.ToXElement());
            return xe;
        }

        public override string ToString()
        {
            return base.GetType().ToString() + ": " + P1 + ", " + P2; // +
                //" [" + AbsolutePosition + " " + string.Format("{0,6:F2}]", AbsoluteRotation);
        }
    }

    public class Sphere : Primitive
    {
        private Vector3D m_center;
        public double Radius;

        public Sphere(Compound parent, Material m, uint p, Vector3D center, double r)
            : base(parent, m, p)
        {
            m_center = center;
            Radius = r;
        }

        public override void Move(Vector3D v)
        {
            m_center += v;
        }

        public override XElement ToXElement()
        {
            XElement xe = new XElement("Sphere",
                        new XAttribute("Priority", m_priority),
                        new XAttribute("Radius", Radius),
                        new XElement("Center", "",
                            new XAttribute("X", String.Format("{0:0.0###}", m_center[0])),
                            new XAttribute("Y", String.Format("{0:0.0###}", m_center[1])),
                            new XAttribute("Z", String.Format("{0:0.0###}", m_center[2]))));

            xe.Add(AbsoluteTransformation.ToXElement());
            return xe;
        }
    }

    public class Cylinder : Primitive
    {
        public double Radius;
        public Vector3D P1, P2;

        public Cylinder(Compound parent, Material m, uint p, Vector3D v0, Vector3D v1, double r)
            : base(parent, m, p)
        {
            Radius = r;
            P1 = v0;
            P2 = v1;
        }

        public override void Move(Vector3D v)
        {
            P1 += v;
            P2 += v;
        }

        public override XElement ToXElement()
        {
            XElement xe = new XElement("Cylinder",
                        new XAttribute("Priority", m_priority),
                        new XAttribute("Radius", Radius),
                        new XElement("P1", "",
                            new XAttribute("X", String.Format("{0:g}", P1.x)),
                            new XAttribute("Y", String.Format("{0:g}", P1.y)),
                            new XAttribute("Z", String.Format("{0:g}", P1.z))),
                        new XElement("P2", "",
                            new XAttribute("X", String.Format("{0:g}", P2.x)),
                            new XAttribute("Y", String.Format("{0:g}", P2.y)),
                            new XAttribute("Z", String.Format("{0:g}", P2.z))));

            xe.Add(AbsoluteTransformation.ToXElement());
            return xe;
        }
    }

    public class Polygon : Primitive
    {
        protected List<Vector2D> m_points;
        protected uint m_normDir;
        protected double m_elevation;

        public List<Vector2D> Points
        {
            get { return m_points; }
            set
            {
                m_points.Clear();
                m_points.AddRange(value);
            }
        }

        public Polygon(Compound parent, Material m, uint p, uint normDir = 2, double elevation = 0.0, double[,] points = null)
            : base(parent, m, p)
        {
            m_points = new List<Vector2D>();
            if (points != null)
            {
                if (points.GetLength(0) == 2)
                {
                    for (int i = 0; i < points.GetLength(1); i++)
                    {
                        m_points.Add(new Vector2D(points[0, i], points[1, i]));
                    }
                }
                else if (points.GetLength(1) == 2)
                {
                    for (int i = 0; i < points.GetLength(0); i++)
                    {
                        m_points.Add(new Vector2D(points[i, 0], points[i, 1]));
                    }
                }
            }
            m_elevation = elevation;
            m_normDir = normDir;
        }

        public Polygon(Compound parent, Material m, uint p, uint normDir = 2, double elevation = 0.0, List<Vector2D> points = null)
            : base(parent, m, p)
        {
            m_points = new List<Vector2D>();
            if (points != null)
            {
                m_points.AddRange(points);
            }
            m_elevation = elevation;
            m_normDir = normDir;
        }

        public override void Move(Vector3D v)
        {
            foreach (Vector2D p in m_points)
            {
                p.x += v.x;
                p.y += v.y;
            }

            switch (m_normDir)
            {
                case 0:
                    m_elevation += v.x;
                    break;
                case 1:
                    m_elevation += v.y;
                    break;
                case 2:
                    m_elevation += v.z;
                    break;
                default:
                    break;
            }
        }

        public override XElement ToXElement()
        {
            throw new NotImplementedException();
        }
    }

    public class LinearPolygon : Polygon
    {
        private double m_extrudeLength;

        public LinearPolygon(Compound parent, Material m, uint p, uint normDir, double elevation, double[,] points, double extrudeLength)
            : base(parent, m, p, normDir, elevation, points)
        {
            m_extrudeLength = extrudeLength;
        }

        public LinearPolygon(Compound parent, Material m, uint p, uint normDir, double elevation, List<Vector2D> points, double length)
            : base(parent, m, p, normDir, elevation, points)
        {
            m_extrudeLength = length;
        }

        public override XElement ToXElement()
        {
            XElement xlp = new XElement("LinPoly",
                new XAttribute("Priority", m_priority),
                new XAttribute("Elevation", m_elevation),
                new XAttribute("Length", m_extrudeLength),
                new XAttribute("NormDir", m_normDir));

            foreach (Vector2D p in m_points)
            {
                xlp.Add(new XElement("Vertex", "",
                    new XAttribute("X1", p.x),
                    new XAttribute("X2", p.y)));
                }

            xlp.Add(AbsoluteTransformation.ToXElement());
            return xlp;
        }
    }
}
