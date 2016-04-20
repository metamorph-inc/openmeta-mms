using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyPhy2CADPCB.AbstractClasses
{
    class CADModel
    {
        public String path { get; set; }
        public XYZTuple<Double, Double, Double> translationVector { get; set; }
        public XYZTuple<Double, Double, Double> rotationVector { get; set; }
        public XYZTuple<Double, Double, Double> scalingVector { get; set; }
    }
}
