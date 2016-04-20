using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;

namespace CyPhy2SystemC.SystemC
{
    public class SourceFile : IComparable<SourceFile>
    {
        public enum SourceType { Header, Implemnation, Arduino, Unknown};

        public SourceFile(string path)
        {
            Path = path;
        }

        public virtual string Path { get; set; }

        public virtual SourceType Type
        {
            get
            {
                string ext = System.IO.Path.GetExtension(Path);
                if ( ext == ".h" || ext == ".hpp" )
                {
                    return SourceType.Header;
                }
                if (ext == ".cc" || ext == ".cpp")
                {
                    return SourceType.Implemnation;
                }
                if (ext == ".ino")
                {
                    return SourceType.Arduino;
                }
                return SourceType.Unknown;
            }
        }
        
        public virtual int CompareTo(SourceFile other)
        {
            return this.Path.CompareTo(other.Path);
        }
    }
}
