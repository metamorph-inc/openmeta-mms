using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSXCAD
{
    namespace Ara
    {
        public class Endo : Compound
        {
            private uint m_priority;

            private uint m_cols;
            private uint m_rows;
            public Slot[] Slots { get; private set; }

            private double m_baseHeight; // mm
            private double m_ribWidth; // mm
            private double m_ribHeight; // mm
            private double m_modWidth; // mm

            public class Slot
            {
                public uint Index { get; set; }
                public uint[] Size { get; set; }
                public Module Module { get; set; }
                public Vector3D Position { get; set; }
                public double Rotation { get; set; }

                public Slot(uint idx, uint rows, uint cols, Vector3D pos, double rot)
                {
                    if (rows == 0 || rows > 2 || cols == 0 || cols > 2)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    Size = new uint[2] {rows, cols};
                    Index = idx;
                    Module = null;
                    Position = pos;
                    Rotation = rot;
                }
            }

            public Endo()
                : this(null, "endo", new Vector3D(0, 0, 0), 0)
            {
            }

            public Endo(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                // Endo medium (6x3)
                m_cols = 3;
                m_rows = 6;
                Slots = new Slot[8];

                double[,] slotPositions = new double[8, 3] {
                    { 20,  3, 1.5 }, { 23,  3, 1.5 }, { 20, 26, 1.5 }, { 23, 26, 1.5 },
                    { 20, 49, 1.5 }, { 23, 72, 1.5 }, { 20, 95, 1.5 }, { 23, 118, 1.5 }
                };
                double[] slotRotations = new double[8] { Math.PI / 2, 0, Math.PI / 2, 0, Math.PI / 2, 0, Math.PI / 2, 0 };
                uint[,] slotSizes = new uint[8, 2] {
                    { 1, 1 }, { 1, 2 }, { 1, 1 }, { 2, 2 },
                    { 1, 2 }, { 2, 2 }, { 1, 2 }, { 1, 2 }
                };

                for (uint ss = 0; ss < 8; ss++)
                {
                    Vector3D v = new Vector3D(slotPositions[ss, 0], slotPositions[ss, 1], slotPositions[ss, 2]);
                    double r = slotRotations[ss];
                    Slots[ss] = new Slot(ss, slotSizes[ss,0], slotSizes[ss,1], v, r);
                }

                m_priority = 50;
                m_baseHeight = 3;
                m_ribWidth = 3;
                m_ribHeight = 3;
                m_modWidth = 20;

                Metal m = new Metal("endo");
                m.FillColor = new Material.Color(85, 170, 255, 255);
                m.EdgeColor = new Material.Color(85, 170, 255, 255);

                // Base plane
                m_primitives.Add(new Box(this, m, m_priority,
                    new Vector3D(0, 0, -m_baseHeight / 2),
                    new Vector3D(m_cols * 20 + (m_cols - 1) * m_ribWidth, m_rows * 20 + (m_rows + 1) * m_ribWidth, m_baseHeight / 2)));

                // Module side
                foreach (int i in new int[] {0, 1, 2, 4, 6})
                {
                    m_primitives.Add(new Box(this, m, m_priority,
                        new Vector3D( 0, i*(m_modWidth + m_ribWidth), m_baseHeight / 2 ),
                        new Vector3D( m_modWidth, i*(m_modWidth+m_ribWidth) + m_ribWidth, m_baseHeight / 2 + m_ribHeight )));
                }

                foreach (int i in new int[] {0, 1, 3, 5, 6})
                {
                    m_primitives.Add(new Box(this, m, m_priority,
                        new Vector3D( m_modWidth + m_ribWidth, i*(m_modWidth + m_ribWidth), m_baseHeight / 2 ),
                        new Vector3D( m_cols * m_modWidth + (m_cols - 1) * m_ribWidth, i*(m_modWidth+m_ribWidth) + m_ribWidth, m_baseHeight / 2 + m_ribHeight )));
                }

                m_primitives.Add(new Box(this, m, m_priority,
                    new Vector3D( m_modWidth, 0, m_baseHeight / 2 ),
                    new Vector3D( m_modWidth + m_ribWidth, m_rows * m_modWidth + (m_rows+1) * m_ribWidth, m_baseHeight / 2 + m_ribHeight )));

                // Display side
                foreach (int i in new int[] { 0, 6 })
                {
                    m_primitives.Add(new Box(this, m, m_priority,
                        new Vector3D(0, i * (m_modWidth + m_ribWidth), -m_baseHeight / 2),
                        new Vector3D(m_cols * m_modWidth + (m_cols - 1) * m_ribWidth, i * (m_modWidth + m_ribWidth) + m_ribWidth, -(m_baseHeight / 2 + m_ribHeight))));
                }
            }

            public override Box BoundingBox
            {
                get
                {
                    Vector3D p1 = new Vector3D(0, 0, 0);
                    Vector3D p2 = new Vector3D(
                            m_ribWidth + m_cols * (m_ribWidth + m_modWidth),
                            m_ribHeight + m_rows * (m_ribHeight + m_modWidth),
                            0);
                    return new Box(null, null, 0, AbsoluteTransformation * p1, AbsoluteTransformation * p2);
                }
            }

            public Module GetModule(uint idx)
            {
                return Slots[idx].Module;
            }

            public void AddModule(uint idx, Module m)
            {
                if (Slots[idx].Size[0] != m.Size[0] || Slots[idx].Size[1] != m.Size[1])
                {
                    Console.WriteLine("Warning: Attempted to insert " + m.Size[0] + "x" + m.Size[1] + " module \"" +
                       m.Name + "\" into " + Slots[idx].Size[0] + "x" + Slots[idx].Size[1] + " slot #" + idx + " (request ignored)");
                    return;
                }

                m.Transformations.Clear();
                m.Transformations.Add(new TRotateZ(Slots[idx].Rotation));
                m.Transformations.Add(new TTranslate(Slots[idx].Position));
                Slots[idx].Module = m;
                this.Add(m);
            }
        }

        public abstract class Module : Compound
        {
            private uint[] m_size;
            public PCB PCB { get; protected set; }

            public Module(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                PCB = null;
            }

            public uint[] Size
            {
                get
                {
                    return m_size;
                }
                set
                {
                    if (value.Length == 2)
                        m_size = value;
                }
            }
        }

        public class Module_1x2 : Module
        {
            public Module_1x2(string name)
                : this(null, name, new Vector3D(0, 0, 0), 0)
            {
            }

            public Module_1x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                Size = new uint[2] { 1, 2 };
                m_compounds.Add(new Base_1x2(this, Name + "-base", new Vector3D(), 0));
                PCB = new PCB_1x2(this, Name + "-pcb", new Vector3D(1.25, 1.2, 0.88), 0);
                m_compounds.Add(PCB);
            }

            public override Box BoundingBox
            {
                get
                {
                    Vector3D v1 = new Vector3D(0, 0, 0);
                    Vector3D v2 = new Vector3D(43, 20, 4.0);
                    return new Box(null, null, 0, AbsoluteTransformation * v1, AbsoluteTransformation * v2);
                }
            }
        }

        public class Module_2x2 : Module
        {
            public Module_2x2(string name)
                : this(null, name, new Vector3D(0, 0, 0), 0)
            {
            }

            public Module_2x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                Size = new uint[2] { 2, 2 };
                m_compounds.Add(new Base_2x2(this, Name + "-base", new Vector3D(), 0));
                PCB = new PCB_2x2(this, Name + "-pcb", new Vector3D(1, 0.75, 0.88), 0);
                m_compounds.Add(PCB);
            }

            public override Box BoundingBox
            {
                get
                {
                    Vector3D v1 = new Vector3D(0, 0, 0);
                    Vector3D v2 = new Vector3D(43, 43, 4.0);
                    return new Box(null, null, 0, AbsoluteTransformation * v1, AbsoluteTransformation * v2);
                }
            }
        }

        public class Base : Compound
        {
            protected const double m_thickness = 0.5;

            protected const uint m_priority = 60;
            protected const uint m_cutoutPriority = 61;

            public Material Material { set; get; }
            private static Material m_defaultMaterial;

            public Material CutoutMaterial;
            private static Material m_defaultCutoutMaterial;

            public Base(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                Material = DefaultMaterial;
            }

            public static Material DefaultMaterial
            {
                get
                {
                    if (m_defaultMaterial == null)
                    {
                        m_defaultMaterial = new Dielectric("base");
                        (m_defaultMaterial as Dielectric).EpsRel = 3.0;
                    }
                    return m_defaultMaterial;
                }
            }

            public static Material DefaultCutoutMaterial
            {
                get
                {
                    if (m_defaultCutoutMaterial == null)
                    {
                        m_defaultCutoutMaterial = new Dielectric("base-cutout");
                    }
                    return m_defaultCutoutMaterial;
                }
            }
        }

        public class Base_1x2 : Base
        {
            private const double m_width = 0.7;
            private double m_heightLow = 1.75 - 0.5;
            private double m_heightHigh = 2.29 - 0.5;

            public Base_1x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                Material.FillColor = new Material.Color(220, 10, 10);
                Material.EdgeColor = new Material.Color(220, 10, 10);

                CutoutMaterial = DefaultCutoutMaterial;
                CutoutMaterial.FillColor = new Material.Color(2, 32, 63, 123);
                CutoutMaterial.EdgeColor = new Material.Color(2, 32, 63, 123);

                LinearPolygon lp = new LinearPolygon(this, this.Material, m_priority, 2, 0,
                    new double[,] { { 43.0, 20.0, 20.0, 0.0, 0.0, 43.0 }, { 1.78, 1.78, 0.0, 0.0, 20.0, 20.0 } },
                    m_thickness);
                m_primitives.Add(lp);

                lp = new LinearPolygon(this, this.Material, m_priority, 2, m_thickness,
                    new double[,] { {43.0, 20.0, 20.0, 43.0-m_width, 43.0-m_width, 2.65, 2.65, 43.0},
                    {1.78, 1.78, 1.78+m_width, 1.78+m_width, 19.96-m_width, 19.96-m_width, 20.0, 20.0}},
                    m_heightLow);
                m_primitives.Add(lp);

                lp = new LinearPolygon(this, this.Material, m_priority, 2, m_thickness,
                    new double[,] { { m_width, 0.0, 0.0, m_width }, { 16.2, 16.2, 20.0, 20.0 } },
                    m_heightHigh);
                m_primitives.Add(lp);

                lp = new LinearPolygon(this, this.Material, m_priority, 2, m_thickness,
                    new double[,] { { 20.0, 15.24, 15.24, 20.0 }, { 0.0, 0.0, 1.12, 1.12 } },
                    m_heightHigh);
                m_primitives.Add(lp);

                lp = new LinearPolygon(this, this.Material, m_priority, 2, m_thickness,
                    new double[,] { {6.65, 0.0, 0.0, m_width, m_width, 6.65},
                    {0.0, 0.0, 6.65, 6.65, m_width, m_width} }, m_heightHigh);
                m_primitives.Add(lp);

                // Cutout
                Box b = new Box(this, CutoutMaterial, m_cutoutPriority,
                    new Vector3D(5.76, 5.70, 0),
                    new Vector3D(18.0, 11.0, m_thickness));
                m_primitives.Add(b);
            }
        }

        public class Base_2x2 : Base
        {
            private const double m_width = 0.75;
            private const double m_widthWide = 1.0;
            private double m_heightLow = 1.75 - 0.5;
            private double m_heightHigh = 2.29 - 0.5;


            public Base_2x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                this.Material.FillColor = new Material.Color(220, 10, 10);
                this.Material.EdgeColor = new Material.Color(220, 10, 10);

                CutoutMaterial = DefaultCutoutMaterial;
                CutoutMaterial.FillColor = new Material.Color(2, 32, 63, 123);
                CutoutMaterial.EdgeColor = new Material.Color(2, 32, 63, 123);

                LinearPolygon lp = new LinearPolygon(this, this.Material, m_priority, 2, 0,
                    new double[,] { { 0, 43, 43, 1.8, 1.8, 0 }, { 0, 0, 41.2, 41.2, 18.3, 18.3 } },
                    m_thickness);
                m_primitives.Add(lp);

                lp = new LinearPolygon(this, this.Material, m_priority, 2, m_thickness, new double[,] {
                    {0, 43, 43, 1.8, 1.8, 1.8+m_width, 1.8+m_width, 43-m_width, 43-m_width, 0},
                    {0, 0, 41.2, 41.2, 19.2, 19.2, 41.2-m_width, 41.2-m_width, m_width, m_width}
                    }, m_heightLow);
                m_primitives.Add(lp);

                lp = new LinearPolygon(this, this.Material, m_priority, 2, m_thickness,
                    new double[,] { { 0, m_widthWide, m_widthWide, 0 }, { 0, 0, 3.45, 3.45 } },
                    m_heightHigh);
                m_primitives.Add(lp);

                lp = new LinearPolygon(this, this.Material, m_priority, 2, m_thickness,
                    new double[,] { { 0, m_widthWide, m_widthWide, 0 }, { 14.85, 14.85, 14.85+3.45, 14.85+3.45 } },
                    m_heightHigh);
                m_primitives.Add(lp);

                // Cutout
                Box b = new Box(this, CutoutMaterial, m_cutoutPriority,
                    new Vector3D(5.72, 3.9, 0),
                    new Vector3D(18, 9.21, m_thickness));
                m_primitives.Add(b);
            }
        }

        public abstract class PCB : Compound
        {
            public readonly double Thickness = 0.56;

            protected const uint m_priority = 60;
            protected const uint m_cutoutPriority = 61;

            public Material Material { set; get; }
            private static Material m_defaultMaterial;

            public Material CutoutMaterial;
            private static Material m_defaultCutoutMaterial;

            public PCB(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                Material = DefaultMaterial;
            }

            public static Material DefaultMaterial
            {
                get
                {
                    if (m_defaultMaterial == null)
                    {
                        double epsRel = 4.88;
                        m_defaultMaterial = new Dielectric("pcb", epsRel, kappa: 1e-3 * 2 * Math.PI * 2.45e9 * Material.Eps0 * epsRel);
                    }
                    return m_defaultMaterial;
                }
            }

            public static Material DefaultCutoutMaterial
            {
                get
                {
                    if (m_defaultCutoutMaterial == null)
                    {
                        m_defaultCutoutMaterial = new Dielectric("pcb-cutout");
                    }
                    return m_defaultCutoutMaterial;
                }
            }
        }

        public class PCB_1x2 : PCB
        {
            public PCB_1x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                Material = DefaultMaterial;
                Material.FillColor = new Material.Color(85, 170, 0, 123);
                Material.EdgeColor = new Material.Color(12, 62, 153);
                CutoutMaterial = DefaultCutoutMaterial;
                CutoutMaterial.FillColor = new Material.Color(0, 85, 0, 123);
                CutoutMaterial.EdgeColor = new Material.Color(144, 73, 241, 123);

                LinearPolygon pcb = new LinearPolygon(this, Material, m_priority, 2, 0, new double[,] {
                    { 40.5, 40.5, 18.5, 18.5, 14.2, 14.2, 5.2, 5.2, 0.0, 0.0, 4.0, 4.0, 0.0, 0.0 },
                    { 18.0, 1.4, 1.4, 0.0, 0.0, 4.0, 4.0, 0.0, 0.0, 5.26, 5.26, 14.05, 14.05, 18.0} }, Thickness);
                m_primitives.Add(pcb);

                // Cutout
                for (double xx = 0.0; xx <= 4 * 2.3; xx += 2.3)
                {
                    foreach (double yy in new double[] { 0, 2.3 })
                    {
                        Cylinder c = new Cylinder(this, CutoutMaterial, m_cutoutPriority,
                            new Vector3D ( xx + 5.14 + 0.84, yy + 5.21 + 0.84, 0.0 ),
                            new Vector3D ( xx + 5.14 + 0.84, yy + 5.21 + 0.84, Thickness ), 0.84);
                        m_primitives.Add(c);
                    }
                }

                // Shield
                //m_compounds.Add(new Shield_1x2(this, Name + "-shield", new Vector3D(0, 0, Thickness), 0));

                // EPMs
                m_compounds.Add(new EPM_1x2(this, "epm-1", new Vector3D(5.45 + 8.5 / 2, -1.2, -0.62), 0));
                m_compounds.Add(new EPM_1x2(this, "epm-2", new Vector3D(-1.2, 9.69, -0.62), -Math.PI / 2));
            }
        }

        public class PCB_2x2 : PCB
        {

            public PCB_2x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                Material = DefaultMaterial;
                Material.FillColor = new Material.Color(85, 170, 0, 123);
                Material.EdgeColor = new Material.Color(12, 62, 153);
                CutoutMaterial = DefaultCutoutMaterial;
                CutoutMaterial.FillColor = new Material.Color(0, 85, 0, 123);
                CutoutMaterial.EdgeColor = new Material.Color(144, 73, 241, 123);

                LinearPolygon pcb = new LinearPolygon(this, Material, m_priority, 2, 0, new double[,] {
                    { 0, 41.16, 41.16, 40.22, 40.22, 41.16, 41.16, 1.71, 1.71, 0, 0, 4.73, 4.73, 0 },
                    { 0, 0, 14.47, 14.47, 24.91, 24.91, 39.66, 39.66, 16.91, 16.91, 14.21, 14.21, 2.76, 2.76 }
                    }, Thickness);
                m_primitives.Add(pcb);

                // Cutout
                for (double xx = 0.0; xx <= 4 * 2.3; xx += 2.3)
                {
                    for (double yy = 0.0; yy <= 1 * 2.3; yy += 2.3)
                    {
                        Cylinder c = new Cylinder(this, CutoutMaterial, m_cutoutPriority,
                            new Vector3D(xx + 5.55 + 0.84, yy + 3.9 + 0.84, 0.0),
                            new Vector3D(xx + 5.55 + 0.84, yy + 3.9 + 0.84, Thickness), 0.84);
                        m_primitives.Add(c);
                    }
                }

                // Shield
                //m_compounds.Add(new Shield_2x2(this, Name + "-shield", new Vector3D(0, 0, Thickness), 0));

                // EPMs
                m_compounds.Add(new EPM_2x2(this, "epm-1", new Vector3D(-1, 2.8 + 11.33 / 2, -0.62), -Math.PI / 2));
            }
        }

        public class Shield : Compound
        {
            public Material Material { set; get; }
            private static Metal m_defaultMaterial;
            protected const uint m_priority = 65;
            protected const double m_height = 1.83;
            protected const double m_thickness = 0.18;

            public static Metal DefaultMaterial
            {
                get
                {
                    if (m_defaultMaterial == null)
                    {
                        m_defaultMaterial = new Metal("shield");
                    }
                    return m_defaultMaterial;
                }
            }

            public Shield(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                Material = DefaultMaterial;
                Material.FillColor = new Material.Color(235, 235, 235, 255);
                Material.EdgeColor = new Material.Color(155, 155, 155, 255);
            }

            protected List<Primitive> Pad(double w, Vector3D p, double r)
            {
                List<Primitive> pl = new List<Primitive>();

                pl.Add(new Box(this, Material, m_priority,
                    new Vector3D(-w / 2, -0.68, 0),
                    new Vector3D(w / 2, -0.5, m_height), p, r));

                pl.Add(new Box(this, Material, m_priority,
                    new Vector3D(-w / 2, -0.68, m_height-m_thickness),
                    new Vector3D(w / 2, 0, m_height), p, r));

                return pl;
            }

            protected double[] LinSpace(double start, double end, uint n)
            {
                if (n == 0)
                    return new double[] {};

                if (n == 1)
                    return new double[1] { start };

                double[] ls = new double[n];
                for (int i = 0; i < n; i++)
                {
                    ls[i] = start + (end - start) / (n-1) * i;
                }
                return ls;
            }
        }

        public class Shield_1x2 : Shield
        {
            public Shield_1x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                // Top
                LinearPolygon shield = new LinearPolygon(this, Material, m_priority, 2, m_height-m_thickness, new double[,] {
                    { 0.68, 0.68, 4.45, 4.45, 0.68, 0.68, 39.82, 39.82, 17.52, 17.52, 14.1, 14.1, 5.3, 5.3 },
                    { 0.68, 5.3, 5.3, 14.1, 14.1, 17.32, 17.32, 2.08, 2.08, 0.68, 0.68, 4.45, 4.45, 0.68} },
                    m_thickness);
                m_primitives.Add(shield);

                // West pads
                double width = 1.73;
                double x = 0.68;
                foreach (double yy in LinSpace(0.68 + width / 2, 5.3 - width / 2, 2))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(x, yy, 0), -Math.PI / 2));
                }

                width = 3; x = 0.68;
                double y = 14.1 + width / 2;
                m_primitives.AddRange(base.Pad(width, new Vector3D(x, y, 0), -Math.PI / 2));
               
                // North pads
                width = 3;
                y = 17.32;
                foreach (double xx in LinSpace(0.68 + width / 2, 39.82 - width / 2, 10))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(xx, y, 0), -Math.PI));
                }

                // East pads
                width = 3; x = 39.82;
                foreach (double yy in LinSpace(2.08 + width / 2, 17.32 - width / 2, 4))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(x, yy, 0), Math.PI / 2));
                }

                // South pads
                width = 3;  y = 2.08;
                foreach (double xx in LinSpace(24.5 + width / 2, 39.82 - width / 2, 4))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(xx, y, 0), 0));
                }

                width = 4.42; x = 19.08 + width / 2; y = 2.08;
                m_primitives.AddRange(base.Pad(width, new Vector3D(x, y, 0), 0));

                width = 1.25; y = 0.68;
                foreach (double xx in LinSpace(14.1 + width / 2, 17.52 - width / 2, 2))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(xx, y, 0), 0));
                }

                width = 1.73; y = 0.68;
                foreach (double xx in LinSpace(0.68 + width / 2, 5.3 - width / 2, 2))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(xx, y, 0), 0));
                }
            }
        }

        public class Shield_2x2 : Shield
        {
            public Shield_2x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                // Top
                LinearPolygon shield = new LinearPolygon(this, Material, m_priority, 2, m_height - m_thickness, new double[,] {
                    { 0.68, 40.33, 40.33, 39.38, 39.38, 40.33, 40.33, 2.68, 2.68, 0.68, 0.68, 4.82, 4.82, 0.68},
                    { 0.68, 0.68, 14.03, 14.03, 25.18, 25.18, 38.83, 38.83, 15.83, 15.83, 14.21, 14.21, 2.61, 2.61 } },
                    m_thickness);
                m_primitives.Add(shield);

                // West pads
                double width, x, y;
                width = 3;
                x = 2.68;
                foreach (double yy in LinSpace(38.15 + 0.68 - width / 2, 0.68 + 15.15 + 1 + width / 2, 6))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(x, yy, 0), -Math.PI / 2));
                }

                width = 1.71;
                x = 2.68;
                y = 0.68 + 16.62 + width / 2;
                m_primitives.AddRange(base.Pad(width, new Vector3D(x, y, 0), -Math.PI / 2));

                width = 1.63; y = 0.68 + 13.52 + width / 2; x = 0.68;
                m_primitives.AddRange(base.Pad(width, new Vector3D(x, y, 0), -Math.PI / 2));

                width = 1.93; y = 0.68 + width / 2; x = 0.68;
                m_primitives.AddRange(base.Pad(width, new Vector3D(x, y, 0), -Math.PI / 2));

                // North pads
                width = 3; y = 38.15 + 0.68;
                foreach (double xx in LinSpace(39.15 + 0.68 - width / 2, 2 + width / 2 + 0.68, 10))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(xx, y, 0), Math.PI));
                }

                // East pads
                width = 3;
                x = 0.68 + 39.65;
                foreach (double yy in LinSpace(0.68 + width / 2, 0.68 + 4 + width / 2, 2))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(x, yy, 0), Math.PI / 2));
                }
                
                width = 5.35;
                y = 8.68 + width / 2;
                m_primitives.AddRange(base.Pad(width, new Vector3D(x, y, 0), Math.PI / 2));

                width = 2.3;
                x = 0.68 + 38.7;
                foreach (double yy in LinSpace(0.68 + 13.35 + 1 + width / 2, 0.68 + 24.5 - 1 - width / 2, 3))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(x, yy, 0), Math.PI / 2));
                }

                width = 3.8;
                x = 0.68 + 39.65;
                foreach (double yy in LinSpace(0.68 + 24.5 + width / 2, 0.68 + 38.15 - width / 2, 3))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(x, yy, 0), Math.PI / 2));
                }

                // South pads
                width = 3;
                y = 0.68;
                foreach (double xx in LinSpace(0.68 + width / 2, 39.65 + 0.68 - width / 2, 10))
                {
                    m_primitives.AddRange(base.Pad(width, new Vector3D(xx, y, 0), 0));
                }
            }
        }

        public abstract class EPM : Compound
        {
            private const uint m_priority = 71;

            public EPM(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
            }

            protected class Pole : Compound
            {
                public Dielectric Material { set; get; }
                private static Dielectric m_defaultMaterial;
                public uint Priority { set; get; }
                private double m_width;

                public static Dielectric DefaultMaterial
                {
                    get
                    {
                        if (m_defaultMaterial == null)
                        {
                            m_defaultMaterial = new Dielectric("epm-pole", kappa: 250, density: 8110);
                        }
                        return m_defaultMaterial;
                    }
                }

                public Pole(Compound parent, string name, Vector3D pos, double rot, double width)
                    : base(parent, name, pos, rot)
                {
                    m_width = width;
                    Priority = EPM.m_priority;
                    Material = DefaultMaterial;
                    Material.FillColor = new Material.Color(95, 95, 95, 255);
                    Material.EdgeColor = new Material.Color(95, 95, 95, 255);

                    LinearPolygon pole = new LinearPolygon(this, this.Material, this.Priority, 0, 0, new double[,] {
                        { 0, 0, 5.11, 5.11, 3.59, 2.2 },
                        { 0, 2.38, 2.38, 0.91, 0.91, 0} }, m_width);
                    m_primitives.Add(pole);
                }
            }     

            protected class Alnico : Compound
            {
                public Dielectric Material { set; get; }
                private static Dielectric m_defaultMaterial;
                public uint Priority { set; get; }
                private const double m_width = 1.0;

                public static Dielectric DefaultMaterial
                {
                    get
                    {
                        if (m_defaultMaterial == null)
                        {
                            m_defaultMaterial = new Dielectric("epm-alnico", kappa: 189, density: 7300);
                        }
                        return m_defaultMaterial;
                    }
                }

                public Alnico(Compound parent, string name, Vector3D pos, double rot)
                    : base(parent, name, pos, rot)
                {
                    Priority = EPM.m_priority;
                    Material = DefaultMaterial;
                    Material.FillColor = new Material.Color(55, 55, 65, 255);
                    Material.EdgeColor = new Material.Color(55, 55, 65, 255);

                    LinearPolygon alnico = new LinearPolygon(this, Material, Priority, 0, 0, new double[,] {
                        { 2.6, 2.6, 5.11, 5.11 },
                        { 0.91, 2.38, 2.38, 0.91} }, m_width);
                    m_primitives.Add(alnico);
                }
            }

            protected class Neodymium : Compound
            {
                public Dielectric Material { set; get; }
                private static Dielectric m_defaultMaterial;
                public uint Priority { set; get; }
                private const double m_width = 1.0;

                public static Dielectric DefaultMaterial
                {
                    get
                    {
                        if (m_defaultMaterial == null)
                        {
                            m_defaultMaterial = new Dielectric("epm-neodymium", kappa: 55.5, density: 7500);
                        }
                        return m_defaultMaterial;
                    }
                }

                public Neodymium(Compound parent, string name, Vector3D pos, double rot)
                    : base(parent, name, pos, rot)
                {
                    Priority = EPM.m_priority;
                    Material = DefaultMaterial;
                    Material.FillColor = new Material.Color(55, 55, 65, 255);
                    Material.EdgeColor = new Material.Color(55, 55, 65, 255);

                    LinearPolygon neodymium = new LinearPolygon(this, this.Material, this.Priority, 0, 0, new double[,] {
                        { 0, 0, 2, 2 },
                        { 0, 2.38, 2.38, 0} }, m_width);
                    m_primitives.Add(neodymium);
                }
            }

            protected class Coil : Compound
            {
                public Metal Material { set; get; }
                private static Metal m_defaultMaterial;
                public uint Priority { set; get; }
                private const double m_width = 1.0;
                private const double m_d = 0.1;
                private const double m_thickness = 0.53;

                public static Metal DefaultMaterial
                {
                    get
                    {
                        if (m_defaultMaterial == null)
                        {
                            m_defaultMaterial = new Metal("epm-coil");
                        }
                        return m_defaultMaterial;
                    }
                }

                public Coil(Compound parent, string name, Vector3D pos, double rot)
                    : base(parent, name, pos, rot)
                {
                    Priority = EPM.m_priority - 1;
                    Material = DefaultMaterial;
                    Material.FillColor = new Material.Color(235, 148, 7, 255);
                    Material.EdgeColor = new Material.Color(235, 148, 7, 255);

                    LinearPolygon neodymium = new LinearPolygon(this, this.Material, this.Priority, 0, m_d, new double[,] {
                        { 2.6 - m_thickness, 2.6 - m_thickness, 5.11 + m_thickness, 5.11 + m_thickness },
                        { 0.91 - m_thickness, 2.38 + m_thickness, 2.38 + m_thickness, 0.91 - m_thickness} }, m_width-2*m_d);
                    m_primitives.Add(neodymium);
                }
            }
        }

        public class EPM_1x2 : EPM
        {
            public EPM_1x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                foreach (double xx in new double[] { 0, 7.58 })
                {
                    m_compounds.Add(new Pole(this, "pole", new Vector3D(xx-8.5/2, 0, 0), 0, 0.92));
                }

                foreach (double xx in new double[] { 1.92, 4.75 })
                {
                    m_compounds.Add(new Pole(this, "pole", new Vector3D(xx-8.5/2, 0, 0), 0, 1.83));
                }

                foreach (double xx in new double[] { 0.92, 3.75, 6.58 })
                {
                    m_compounds.Add(new Alnico(this, "alnico", new Vector3D(xx - 8.5 / 2, 0, 0), 0));
                    m_compounds.Add(new Neodymium(this, "neodymium", new Vector3D(xx - 8.5 / 2, 0, 0), 0));
                    m_compounds.Add(new Coil(this, "coil", new Vector3D(xx - 8.5 / 2, 0, 0), 0));
                }
            }
        }

        public class EPM_2x2 : EPM
        {
            public EPM_2x2(Compound parent, string name, Vector3D pos, double rot)
                : base(parent, name, pos, rot)
            {
                foreach (double xx in new double[] { 0, 10.41 })
                {
                    m_compounds.Add(new Pole(this, "pole", new Vector3D(xx - 11.33 / 2, 0, 0), 0, 0.92));
                }

                foreach (double xx in new double[] { 1.92, 4.74, 7.58 })
                {
                    m_compounds.Add(new Pole(this, "pole", new Vector3D(xx - 11.33 / 2, 0, 0), 0, 1.83));
                }

                foreach (double xx in new double[] { 0.92, 3.75, 6.58, 9.41 })
                {
                    m_compounds.Add(new Alnico(this, "alnico", new Vector3D(xx - 11.33 / 2, 0, 0), 0));
                    m_compounds.Add(new Neodymium(this, "neodymium", new Vector3D(xx - 11.33 / 2, 0, 0), 0));
                    m_compounds.Add(new Coil(this, "coil", new Vector3D(xx - 11.33 / 2, 0, 0), 0));
                }
            }
        }

        public class HeadPhantom : Compound
        {
            public double Width { get; private set; }
            public double Height { get; private set; }
            public double GridResolution = 1.0; // mm

            public HeadPhantom(string name = "head-phantom", double width = 160, double height = 200)
                : base(name)
            {
                Width = width;
                Height = height;

                Dielectric skinMaterial = new Dielectric("skin", 50, kappa: 0.65, density: 1100);
                skinMaterial.FillColor = new Material.Color(245, 215, 205, 15);
                skinMaterial.EdgeColor = new Material.Color(255, 235, 217, 15);
                Sphere skin = new Sphere(null, skinMaterial, 11, new Vector3D(), 1);
                skin.Transformations.Add(new TScale(Width / 2, Width / 2, Height / 2));
                this.Add(skin);

                Dielectric boneMaterial = new Dielectric("bone", 13, kappa: 0.1, density: 2000);
                boneMaterial.FillColor = new Material.Color(227, 227, 227, 15);
                boneMaterial.EdgeColor = new Material.Color(202, 202, 202, 15);
                Sphere bone = new Sphere(null, boneMaterial, 12, new Vector3D(), 1);
                bone.Transformations.Add(new TScale(0.95 * Width / 2, 0.95 * Width / 2, 0.95 * Height / 2));
                this.Add(bone);

                Dielectric brainMaterial = new Dielectric("brain", 60, kappa: 0.7, density: 1040);
                brainMaterial.FillColor = new Material.Color(255, 85, 127, 15);
                brainMaterial.EdgeColor = new Material.Color(71, 222, 179, 15);
                Sphere brain = new Sphere(null, brainMaterial, 13, new Vector3D(), 1);
                brain.Transformations.Add(new TScale(0.9 * Width / 2, 0.9 * Width / 2, 0.9 * Height / 2));
                this.Add(brain);
            }

            public Box BoundingBox
            {
                get
                {
                    Vector3D v1 = new Vector3D(-Width / 2, -Width / 2, -Height / 2);
                    Vector3D v2 = new Vector3D(+Width / 2, +Width / 2, +Height / 2);
                    return new Box(null, null, 0, AbsoluteTransformation * v1, AbsoluteTransformation * v2);
                }
            }

            public List<double> XGridPoints
            {
                get
                {
                    // Return bounding _cube_ for now
                    List<double> g = new List<double>();
                    /*
                    double min = AbsoluteTransformation.Matrix[0,3] - (Height > Width ? Height : Width) / 2;
                    double max = AbsoluteTransformation.Matrix[0,3] + (Height > Width ? Height : Width) / 2;
                    */
                    double min = AbsoluteTransformation.Matrix[0, 3] - Width / 2;
                    double max = AbsoluteTransformation.Matrix[0, 3] + Width / 2;

                    for (double p = min; p <= max; p += GridResolution)
                    {
                        g.Add(p);
                    }

                    return g;
                }
            }

            public List<double> YGridPoints
            {
                get
                {
                    List<double> g = new List<double>();
                    /*
                    double min = AbsoluteTransformation.Matrix[1,3] - (Height > Width ? Height : Width) / 2;
                    double max = AbsoluteTransformation.Matrix[1,3] + (Height > Width ? Height : Width) / 2;
                    */
                    double min = AbsoluteTransformation.Matrix[1, 3] - Height / 2;
                    double max = AbsoluteTransformation.Matrix[1, 3] + Height / 2;

                    for (double p = min; p <= max; p += GridResolution)
                    {
                        g.Add(p);
                    }

                    return g;
                }
            }

            public List<double> ZGridPoints
            {
                get
                {
                    List<double> g = new List<double>();
                    /*
                    double min = AbsoluteTransformation.Matrix[2,3] - (Height > Width ? Height : Width) / 2;
                    double max = AbsoluteTransformation.Matrix[2,3] + (Height > Width ? Height : Width) / 2;
                    */
                    double min = AbsoluteTransformation.Matrix[2, 3] - Width / 2;
                    double max = AbsoluteTransformation.Matrix[2, 3] + Width / 2;

                    for (double p = min; p <= max; p += GridResolution)
                    {
                        g.Add(p);
                    }

                    return g;
                }
            }
        }
    }
}
