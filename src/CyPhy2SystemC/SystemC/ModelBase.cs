using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2SystemC.SystemC
{
    public abstract class ModelBase<T> : IComparable<ModelBase<T>> where T : ISIS.GME.Common.Interfaces.FCO
    {

        private string _name = null;

        public virtual string Name
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
                this._name = value.Replace(' ', '_');
            }
        }
        public T Impl { get; set; }

        public ModelBase(T impl)
        {
            this.Impl = impl;
            this.Name = impl.Name;
        }

        public int CompareTo(ModelBase<T> other)
        {
            return this.Name.CompareTo(other.Name);
        }
    }
}
