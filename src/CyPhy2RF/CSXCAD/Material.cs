using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;

namespace CSXCAD
{
    public abstract class Material
    {
        public string Name { get; private set; } // unique identifier string
        public Color FillColor { set; get; }
        public Color EdgeColor { set; get; }

        public const double Eps0 = 8.8542e-012;

        public Material(string name)
        {
            Name = name;
        }

        public class Color
        {
            public uint R, G, B, a;
            public Color(uint r, uint g, uint b)
                : this(r, g, b, 255)
            {
            }
            public Color(uint r, uint g, uint b, uint a)
            {
                this.R = r;
                this.G = g;
                this.B = b;
                this.a = a;
            }

            public override bool Equals(System.Object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                Color c = obj as Color;
                if ((System.Object)c == null)
                {
                    return false;
                }

                return (R == c.R) && (G == c.G) && (B == c.B) && (a == c.a);
            }

            public bool Equals(Color c)
            {
                if ((object)c == null)
                {
                    return false;
                }

                return (R == c.R) && (G == c.G) && (B == c.B) && (a == c.a);
            }

            public override int GetHashCode()
            {
                return (int)R ^ (int)G ^ (int)B ^ (int)a;
            }
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Material m = obj as Material;
            if ((System.Object)m == null)
            {
                return false;
            }

            return Name.Equals(m.Name)
                && (FillColor == null ? true : FillColor.Equals(m.FillColor))
                && (EdgeColor == null ? true : EdgeColor.Equals(m.EdgeColor));
        }

        public bool Equals(Material m)
        {
            if ((object)m == null)
            {
                return false;
            }

            return Name.Equals(m.Name)
                && (FillColor == null ? true : FillColor.Equals(m.FillColor))
                && (EdgeColor == null ? true : EdgeColor.Equals(m.EdgeColor));
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode()
                ^ (FillColor == null ? 0 : FillColor.GetHashCode())
                ^ (EdgeColor == null ? 0 : EdgeColor.GetHashCode());
        }

        public virtual XElement ToXElement()
        {
            XElement xm = new XElement(ToString(), new XAttribute("Name", Name));

            if (FillColor != null)
            {
                xm.Add(new XElement("FillColor", "",
                    new XAttribute("R", FillColor.R),
                    new XAttribute("G", FillColor.G),
                    new XAttribute("B", FillColor.B),
                    new XAttribute("a", FillColor.a))
                    );
            };

            if (EdgeColor != null)
            {
                xm.Add(new XElement("EdgeColor", "",
                    new XAttribute("R", EdgeColor.R),
                    new XAttribute("G", EdgeColor.G),
                    new XAttribute("B", EdgeColor.B),
                    new XAttribute("a", EdgeColor.a))
                    );
            };

            return xm;
        }

        public static Material FromXElement(XElement xm)
        {
            Material material;
            switch (xm.Name.ToString())
            {
                case "Material":
                    material = new Dielectric(xm.Attribute("Name").Value);
                    break;
                case "Metal":
                    material = new Metal(xm.Attribute("Name").Value);
                    break;
                default:
                    Console.WriteLine("Error: Unsupported material type " + xm.Name.ToString());
                    return null;
            }

            XElement xfc = xm.Element("FillColor");
            if (xfc != null)
            {
                material.FillColor = new Material.Color(
                    Convert.ToUInt32(xfc.Attribute("R").Value),
                    Convert.ToUInt32(xfc.Attribute("G").Value),
                    Convert.ToUInt32(xfc.Attribute("B").Value),
                    Convert.ToUInt32(xfc.Attribute("a").Value));
            }

            XElement xec = xm.Element("EdgeColor");
            if (xec != null)
            {
                material.EdgeColor = new Material.Color(
                    Convert.ToUInt32(xfc.Attribute("R").Value),
                    Convert.ToUInt32(xfc.Attribute("G").Value),
                    Convert.ToUInt32(xfc.Attribute("B").Value),
                    Convert.ToUInt32(xfc.Attribute("a").Value));
            }

            return material;
        }
    }

    public class Metal : Material
    {
        public Metal(string name)
            : base(name)
        {
        }

        public override string ToString()
        {
            return "Metal";
        }
    }

    public class Dielectric : Material
    {
        private double m_epsRel;
        private double m_kappa; // S / m
        private double m_density; // kg / m^3

        public double EpsRel
        {
            get
            {
                return m_epsRel;
            }
            set
            {
                if (value > 0.0 && value < 20.0)
                {
                    m_epsRel = value;
                }
            }
        }

        public Dielectric(string name, double epsRel = 1.0, double kappa = Double.NaN, double density = Double.NaN)
            : base(name)
        {
            m_epsRel = epsRel;
            m_kappa = kappa; // S/m
            m_density = density; // kg/m^3
        }

        public override XElement ToXElement()
        {
            XElement xe = base.ToXElement();
            XElement xp = new XElement("Property", "");

            xp.Add(
                new XAttribute("Epsilon", m_epsRel)
            );

            if (!Double.IsNaN(m_kappa))
            {
                xp.Add(new XAttribute("Kappa", String.Format("{0:0.0###}", m_kappa)));
            }

            if (!Double.IsNaN(m_density))
            {
                xp.Add(new XAttribute("Density", String.Format("{0:0.0###}", m_density)));
            }

            xe.Add(xp);
            return xe;
        }

        public override string ToString()
        {
            return "Material";
        }
    }


    public class Dump : Material
    {
        public enum EDumpMode
        {
            OMIT_INTERPOLATION = 0,
            NODE_INTERPOLATION = 1,
            CELL_INTERPOLATION = 2
        }

        public enum EDumpType
        {
            E_FIELD_TIME_DOMAIN = 0,
            H_FIELD_TIME_DOMAIN = 1,
            E_CURRENT_TIME_DOMAIN = 2,
            ROT_H_TIME_DOMAIN = 3,

            E_FIELD_FREQ_DOMAIN = 10,
            H_FIELD_FREQ_DOMAIN = 11,
            E_CURRENT_FREQ_DOMAIN = 12,
            ROT_H_FREQ_DOMAIN = 13,

            LOCAL_SAR_FREQ_DOMAIN = 20,
            ONE_GRAM_AVG_SAR_FREQ_DOMAIN = 21,
            TEN_GRAM_AVG_SAR_FREQ_DOMAIN = 22,
            RAW_DATA_FOR_SAR = 29
        }

        public enum EFileType
        {
            VTK_FILE = 0,
            HDF5_FILE = 1
        }

        private EDumpMode m_dumpMode;
        private EDumpType m_dumpType;
        private EFileType m_fileType;
        private List<double> m_frequencies;
        private double m_optResolution;

        public Dump(string name, EDumpMode dm, EDumpType dt, EFileType ft, double f, double optResolution = Double.NaN)
            : this(name, dm, dt, ft, new List<double> { f }, optResolution)
        {
        }

        public Dump(string name, EDumpMode dm, EDumpType dt, EFileType ft, List<double> f = null, double optResolution = Double.NaN)
            : base(name)
        {
            m_dumpMode = dm;
            m_dumpType = dt;
            m_fileType = ft;
            m_frequencies = (f == null) ? new List<double>() : f;
            m_optResolution = optResolution;
        }

        public override XElement ToXElement()
        {
            XElement xe = base.ToXElement();
            xe.Add(
                new XAttribute("DumpMode", (uint)m_dumpMode),
                new XAttribute("DumpType", (uint)m_dumpType),
                new XAttribute("FileType", (uint)m_fileType)
            );

            if (!Double.IsNaN(m_optResolution))
            {
                xe.Add(new XAttribute("OptResolution", m_optResolution));
            }

            if (m_frequencies != null)
            {
                xe.Add(new XElement("FD_Samples",
                    String.Join(",", m_frequencies)));
            }

            return xe;
        }

        public override string ToString()
        {
            return "DumpBox";
        }
    }

    public class Probe : Material
    {
        private double m_weight;
        private EType m_type;
        private ENormDir m_dir;

        public enum EType
        {
            VOLTAGE_PROBE = 0,
            CURRENT_PROBE = 1,
            E_FIELD_PROBE = 2,
            H_FIELD_PROBE = 3,

            WAVEGUIDE_VOLTAGE_PROBE = 10,
            WAVEGUIDE_CURRENT_PROBE = 11
        }

        public Probe(string name, EType type, double weight = 1.0, ENormDir dir = ENormDir.UNDEFINED)
            : base(name)
        {
            m_weight = weight;
            m_type = type;
            m_dir = dir;
        }

        public override XElement ToXElement()
        {
            XElement xe = base.ToXElement();
            xe.Add(
                new XAttribute("Type", (uint)m_type),
                new XAttribute("Weight", m_weight)
            );

            if (m_dir != ENormDir.UNDEFINED)
            {
                xe.Add(new XAttribute("NormDir", (uint)m_dir));
            }
            return xe;
        }

        public override string ToString()
        {
            return "ProbeBox";
        }
    }

    public class ExcitationField : Material
    {
        public enum EType
        {
            E_FIELD_SOFT = 0,
            E_FIELD_HARD = 1,
            H_FIELD_SOFT = 2,
            H_FIELD_HARD = 3,
            PLANE_WAVE = 10
        }

        private EType m_type;
        private Vector3D m_vector;

        public ExcitationField(string name, EType type, Vector3D ev)
            : base(name)
        {
            m_type = type;
            m_vector = ev;
        }

        public override XElement ToXElement()
        {
            XElement xe = base.ToXElement();

            xe.Add(
                new XAttribute("Type", (uint)m_type),
                new XAttribute("Excite", String.Join(",", m_vector.Coordinates))
            );

            return xe;
        }

        public override string ToString()
        {
            return "Excitation";
        }
    }

    public class LumpedElement : Material
    {
        private ENormDir m_dir;
        double m_r;
        double m_l;
        double m_c;

        public LumpedElement(string name, ENormDir dir,
            double r = Double.NaN, double l = Double.NaN, double c = Double.NaN)
            : base(name)
        {
            m_dir = dir;
            m_r = r;
            m_l = l;
            m_c = c;
        }

        public override XElement ToXElement()
        {
            XElement xe = base.ToXElement();

            xe.Add(new XAttribute("Direction", (uint)m_dir));

            if (!Double.IsNaN(m_r))
            {
                xe.Add(new XAttribute("R", m_r));
            }

            if (!Double.IsNaN(m_c))
            {
                xe.Add(new XAttribute("Caps", m_c));
            }

            if (!Double.IsNaN(m_l))
            {
                xe.Add(new XAttribute("L", m_l)); // FIXME: attribute name
            }

            return xe;
        }

        public override string ToString()
        {
            return "LumpedElement";
        }
    }
}

