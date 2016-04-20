using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2SystemC.SystemC
{
    public abstract class Visitor
    {
        public virtual void visit(TestBench obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Visitor::visit TestBench: {0}", obj.Name);
        }
        public virtual void visit(ComponentAssembly obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Visitor::visit ComponentAssembly: {0}", obj.Name);
        }
        public virtual void visit(Component obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Visitor::visit Component: {0}", obj.Name);
        }
        public virtual void visit(Port obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Visitor::visit Port: {0}", obj.Name);
        }
        public virtual void visit(Connection obj)
        {
            if (CodeGenerator.verbose) CodeGenerator.GMEConsole.Info.WriteLine("Visitor::visit Connection: {0}", obj.Name);
        }
    }
}
