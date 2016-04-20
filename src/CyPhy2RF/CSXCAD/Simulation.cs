using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

using System.IO;
using System.Numerics;
using System.Globalization;

namespace CSXCAD
{
    public class Simulation
    {
        private uint numTimesteps;
        private double energyCriteria;
        public Excitation Excitation;
        private string[] boundaryCondition = new string[6]
        {
            "PML_8", "PML_8",
            "PML_8", "PML_8",
            "PML_8", "PML_8"
        };

        public Simulation()
            : this(1000000000, 1e-5)
        {
        }

        public Simulation(uint nts, double edc)
        {
            numTimesteps = nts;
            energyCriteria = edc;
        }

        public double MaximumFrequency
        {
            get
            {
                return (Excitation == null ? 0 : Excitation.MaximumFrequency);
            }
        }

        public XElement ToXElement()
        {
            XElement xe = new XElement("FDTD",
                new XAttribute("NumberOfTimesteps", numTimesteps),
                new XAttribute("MaxTime", 3 * Excitation.Length),
                new XAttribute("endCriteria", energyCriteria),
                new XAttribute("f_max", MaximumFrequency));

            xe.Add(Excitation.ToXElement());

            xe.Add(new XElement("BoundaryCond", "",
                new XAttribute("xmin", boundaryCondition[0]),
                new XAttribute("xmax", boundaryCondition[1]),
                new XAttribute("ymin", boundaryCondition[2]),
                new XAttribute("ymax", boundaryCondition[3]),
                new XAttribute("zmin", boundaryCondition[4]),
                new XAttribute("zmax", boundaryCondition[5])));

            return xe;
        }
    }

    public abstract class Excitation
    {
        public abstract double Length { get; }
        public abstract double MaximumFrequency { get; }
        public abstract XElement ToXElement();
    }

    public class GaussExcitation : Excitation
    {
        private double centerFrequency;
        private double cutoffFrequency;

        public GaussExcitation(double fcenter, double fcut)
        {
            centerFrequency = fcenter;
            cutoffFrequency = fcut;
        }

        public override double Length
        {
            get { return 20.0 / centerFrequency; }
        }

        public override double MaximumFrequency
        {
            get
            {
                return centerFrequency + cutoffFrequency;
            }
        }

        public override XElement ToXElement()
        {
            XElement xe = new XElement("Excitation", "",
                new XAttribute("Type", 0),
                new XAttribute("f0", centerFrequency),
                new XAttribute("fc", cutoffFrequency));

            return xe;
        }
    }

    public class NF2FFBox : Compound
    {
        public NF2FFBox(string name, Vector3D p1, Vector3D p2, double or = Double.NaN)
            : base(name)
        {
            // E-field
            Dump.EDumpMode dm = Dump.EDumpMode.NODE_INTERPOLATION;
            Dump.EDumpType dumpType = Dump.EDumpType.E_FIELD_TIME_DOMAIN;
            Dump.EFileType ft = Dump.EFileType.HDF5_FILE;
            m_primitives.Add(new Box(this, new Dump("nf2ff_E_xn", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p1.y, p1.z), new Vector3D(p1.x, p2.y, p2.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_E_xp", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p2.x, p1.y, p1.z), new Vector3D(p2.x, p2.y, p2.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_E_yn", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p1.y, p1.z), new Vector3D(p2.x, p1.y, p2.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_E_yp", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p2.y, p1.z), new Vector3D(p2.x, p2.y, p2.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_E_zn", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p1.y, p1.z), new Vector3D(p2.x, p2.y, p1.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_E_zp", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p1.y, p2.z), new Vector3D(p2.x, p2.y, p2.z)));

            // H-field
            dumpType = Dump.EDumpType.H_FIELD_TIME_DOMAIN;
            m_primitives.Add(new Box(this, new Dump("nf2ff_H_xn", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p1.y, p1.z), new Vector3D(p1.x, p2.y, p2.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_H_xp", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p2.x, p1.y, p1.z), new Vector3D(p2.x, p2.y, p2.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_H_yn", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p1.y, p1.z), new Vector3D(p2.x, p1.y, p2.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_H_yp", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p2.y, p1.z), new Vector3D(p2.x, p2.y, p2.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_H_zn", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p1.y, p1.z), new Vector3D(p2.x, p2.y, p1.z)));
            m_primitives.Add(new Box(this, new Dump("nf2ff_H_zp", dm, dumpType, ft, optResolution: or), 0,
                new Vector3D(p1.x, p1.y, p2.z), new Vector3D(p2.x, p2.y, p2.z)));
        }
    }

    public class SARBox : Compound
    {
        public SARBox(string name, double f, Vector3D p1, Vector3D p2)
            : this(name, new List<double> { f }, p1, p2)
        {
        }

        public SARBox(string name, List<double> fs, Vector3D p1, Vector3D p2)
            : base(name + "-box")
        {
            Dump d = new Dump(name,
                Dump.EDumpMode.CELL_INTERPOLATION,
                Dump.EDumpType.ONE_GRAM_AVG_SAR_FREQ_DOMAIN,
                Dump.EFileType.HDF5_FILE, fs);
            m_primitives.Add(new Box(this, d, 0, p1, p2));
        }
    }

    public class LumpedPort : Compound
    {
        uint m_portNum;
        double m_rInternal; // lumped element

        // Fields filled up after openEMS simulation
        double[] m_time;
        double[] m_uTdValue;
        double[] m_iTdValue;
        public double[] Freqs { get; private set;}
        Complex[] m_uFdValue;
        Complex[] m_iFdValue;

        public LumpedPort(uint priority, uint portNum, double r, Vector3D p1, Vector3D p2,
            ENormDir dir, bool excite = false)
            : base("port-" + portNum)
        {
            m_portNum = portNum;
            m_rInternal = Double.NaN;

            m_time = null;
            m_uTdValue = null;
            m_iTdValue = null;
            Freqs = null;
            m_uFdValue = null;
            m_iFdValue = null;

            if (p1.Coordinates[(uint)dir] == p2.Coordinates[(uint)dir])
            {
                Console.WriteLine("Error: Excitation vector normal direction component must not be zero");
                return;
            }

            double dirSign = p1.Coordinates[(uint)dir] < p2.Coordinates[(uint)dir] ? +1.0 : -1.0;

            // Lumped element
            Material le;
            if (r > 0 && r != Double.NaN)
            {
                m_rInternal = r;
                le = new LumpedElement("port_resist_" + portNum, dir, r: r, c: 1);
            }
            else
            {
                m_rInternal = Double.NaN;
                le = new Metal("port_resist_" + portNum);
            }
            m_primitives.Add(new Box(this, le, priority, p1, p2));
            ZReference = m_rInternal;

            // Excitation
            Vector3D ev = new Vector3D();
            ev[(int)dir] = 1.0;
            ExcitationField ef = new ExcitationField("port_excite_" + portNum,
                ExcitationField.EType.E_FIELD_SOFT, -dirSign * ev);
            m_primitives.Add(new Box(this, ef, priority, p1, p2));

            // Probes
            Vector3D u1 = 0.5 * (p1 + p2);
            Vector3D u2 = 0.5 * (p1 + p2);
            u1[(int)dir] = p1[(int)dir];
            u2[(int)dir] = p2[(int)dir];
            Probe probe_u = new Probe("port_ut" + portNum, Probe.EType.VOLTAGE_PROBE, -dirSign);
            m_primitives.Add(new Box(this, probe_u, priority, u1, u2));

            Vector3D i1 = new Vector3D(p1.x, p1.y, p1.z);
            Vector3D i2 = new Vector3D(p2.x, p2.y, p2.z);
            i1[(int)dir] = 0.5 * (p1[(int)dir] + p2[(int)dir]);
            i2[(int)dir] = 0.5 * (p1[(int)dir] + p2[(int)dir]);
            Probe probe_i = new Probe("port_it" + portNum, Probe.EType.CURRENT_PROBE, dirSign, dir);
            m_primitives.Add(new Box(this, probe_i, priority, i1, i2));
        }

        public void ReadResults(double[] freqs, string pathName = "")
        {
            Freqs = freqs;

            // Voltage
            StreamReader uFile = new StreamReader(Path.Combine(pathName, "port_ut" + m_portNum));
            List<double> time = new List<double>();
            List<double> uTdValue = new List<double>();

            string line;
            while ((line = uFile.ReadLine()) != null)
            {
                if (!line.StartsWith("%"))
                {
                    string[] values = line.Split('\t');
                    time.Add(Double.Parse(values[0], CultureInfo.InvariantCulture));
                    uTdValue.Add(Double.Parse(values[1], CultureInfo.InvariantCulture));
                }
            }
            uFile.Close();
            m_time = time.ToArray();
            m_uTdValue = uTdValue.ToArray();

            double dTime = m_time.ElementAt(1) - m_time.ElementAt(0);
            m_uFdValue = new Complex[Freqs.Length];

            for (int i = 0; i < Freqs.Length; i++)
            {
                m_uFdValue[i] = 0.0;
                for (int j = 0; j < uTdValue.Count; j++)
                {
                    m_uFdValue[i] += uTdValue[j] * Complex.Exp(-Complex.ImaginaryOne * 2 * Math.PI * Freqs[i] * m_time[j]);
                }
                m_uFdValue[i] = 2 * dTime * m_uFdValue[i];
            }

            // Current
            StreamReader iFile = new StreamReader(Path.Combine(pathName, "port_it" + m_portNum));
            time = new List<double>();
            List<double> iTdValue = new List<double>();

            while ((line = iFile.ReadLine()) != null)
            {
                if (!line.StartsWith("%"))
                {
                    string[] values = line.Split('\t');
                    time.Add(Double.Parse(values[0], CultureInfo.InvariantCulture));
                    iTdValue.Add(Double.Parse(values[1], CultureInfo.InvariantCulture));
                }
            }
            iFile.Close();
            m_iTdValue = iTdValue.ToArray();

            if (m_time.Length != time.Count)
            {
                Console.WriteLine("Error: Voltage and current result time-domain vector lengths do not match");
                throw new FileLoadException();
            }


            dTime = time.ElementAt(1) - time.ElementAt(0);
            m_iFdValue = new Complex[Freqs.Length];

            for (int i = 0; i < Freqs.Length; i++)
            {
                m_iFdValue[i] = 0.0;
                for (int j = 0; j < iTdValue.Count; j++)
                {
                    m_iFdValue[i] += iTdValue[j] * Complex.Exp(-Complex.ImaginaryOne * 2 * Math.PI * Freqs[i] * time[j]);
                }
                m_iFdValue[i] = 2 * dTime * m_iFdValue[i];
            }
        }

        public Complex ZReference { get; private set; }

        public Complex[] ZFdIn
        {
            get
            {
                if (Freqs == null)
                {
                    return null;
                }

                Complex[] z = new Complex[Freqs.Length];
                for (int i = 0; i < Freqs.Length; i++)
                {
                    z[i] = m_uFdValue[i] / m_iFdValue[i];
                }
                return z;
            }
        }

        public Complex[] UFdIncident
        {
            get
            {
                if (Freqs == null)
                {
                    return null;
                }

                Complex[] u = new Complex[Freqs.Length];
                for (int i = 0; i < Freqs.Length; i++)
                {
                    u[i] = 0.5 * (m_uFdValue[i] + m_iFdValue[i] * ZReference);
                }
                return u;
            }
        }

        public Complex[] IFdIncident
        {
            get
            {
                if (Freqs == null)
                {
                    return null;
                }

                Complex[] i = new Complex[Freqs.Length];
                for (int j = 0; j < Freqs.Length; j++)
                {
                    i[j] = 0.5 * (m_iFdValue[j] + m_uFdValue[j] / ZReference);
                }
                return i;
            }
        }

        public Complex[] UFdReflected
        {
            get
            {
                if (Freqs == null)
                {
                    return null;
                }

                Complex[] u = new Complex[Freqs.Length];
                for (int i = 0; i < Freqs.Length; i++)
                {
                    u[i] = m_uFdValue[i] - UFdIncident[i];
                }
                return u;
            }
        }

        public Complex[] IFdReflected
        {
            get
            {
                if (Freqs == null)
                {
                    return null;
                }

                Complex[] i = new Complex[Freqs.Length];
                for (int j = 0; j < Freqs.Length; j++)
                {
                    i[j] = m_iFdValue[j] - IFdIncident[j];
                }
                return i;
            }
        }

        public Complex[] S11
        {
            get
            {
                if (Freqs == null)
                {
                    return null;
                }

                Complex[] uRefl = UFdReflected;
                Complex[] uInc = UFdIncident;
                Complex[] s = new Complex[Freqs.Length];

                for (int i = 0; i < Freqs.Length; i++)
                {
                    s[i] = uRefl[i] / uInc[i];
                }
                return s;
            }
        }

        public double[] PFdIn
        {
            get
            {
                if (Freqs == null)
                {
                    return null;
                }

                double[] s = new double[Freqs.Length];
                for (int i = 0; i < Freqs.Length; i++)
                {
                    s[i] = 0.5 * (m_uFdValue[i] * Complex.Conjugate(m_iFdValue[i])).Real;
                }
                return s;
            }
        }

        public double GetPFdInAt(double freq)
        {
            if (PFdIn == null || Freqs == null || freq < Freqs.First() || freq > Freqs.Last())
            {
                return Double.NaN;
            }

            int i;
            for (i = 0; i < Freqs.Length && Freqs[i] < freq; i++) ;

            if (freq == Freqs[i])
            {
                return PFdIn[i];
            }

            // Interpolate (linear)
            return PFdIn[i - 1] + (PFdIn[i] - PFdIn[i - 1]) / (Freqs[i] - Freqs[i - 1]) * (freq - Freqs[i - 1]);
        }
    }
}
