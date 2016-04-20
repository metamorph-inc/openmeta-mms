using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2Schematic.Schematic
{
    public interface DesignEntity : IComparable<DesignEntity>
    {
        string Name
        {
            get;
        }
        ISIS.GME.Common.Interfaces.FCO Impl
        {
            get;
        }
        ComponentAssembly Parent {
            get;
        }
        string SpiceLib
        {
            get;
            set;
        }
        ComponentAssembly SystemUnderTest { get; }
    }

    public abstract class ModelBase<T> : IComparable<ModelBase<T>> where T : ISIS.GME.Common.Interfaces.FCO
    {

        private string _name = null;

        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name))
                {
                    this._name = this.Impl.Name.Replace(' ', '_');
                }

                return this._name;
            }
            set 
            {
                this._name = value;
            }
        }
        public T Impl { get; set; }
        public float CanvasX { get; set; }
        public float CanvasY { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public float CanvasWidth { get; set; }
        public float CanvasHeight { get; set; }

        public ModelBase(T impl)
        {
            this.Impl = impl;
            this.Name = impl.Name;
            CanvasX = 0;
            CanvasY = 0;
            CanvasWidth = 0;
            CanvasHeight = 0;
        }

        public int CompareTo(ModelBase<T> other)
        {
            int name = this.Name.CompareTo(other.Name);
            if (name == 0)
            {
                return this.Impl.ID.CompareTo(other.Impl.ID);
            }
            return name;
        }

        public override int GetHashCode()
        {
            return this.Impl.ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ModelBase<T>)
            {
                return ((ModelBase<T>)obj).CompareTo(this) == 0;
            }
            return false;
        }
    }
}
