using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyPhyGUIs;

namespace CyPhy2Schematic.Schematic
{
    public abstract class Visitor
    {
        public virtual void visit(TestBench obj)
        {
        }
        public virtual void visit(ComponentAssembly obj)
        {
        }
        public virtual void visit(Component obj)
        {
        }
        public virtual void visit(Port obj)
        {
        }
        public virtual void visit(Connection obj)
        {
        }
        public virtual void upVisit(TestBench obj)
        {
        }
        public virtual void upVisit(ComponentAssembly obj)
        {
        }
        public virtual void upVisit(Component obj)
        {
        }
        public virtual void upVisit(Port obj)
        {
        }
        public virtual void upVisit(Connection obj)
        {
        }

        public GMELogger Logger { get; set; }
    }
}
