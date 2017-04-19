using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;

namespace SimulinkTest
{
    public class SkipWithoutMatlabFact : FactAttribute
    {
        public override string Skip
        {
            get
            {
                if (!IsMatlabInstalled)
                {
                    return "Matlab is not installed";
                }
                else
                {
                    return null;
                }
            }

            set { }
        }

        private bool? _isMatlabInstalled = null;
        private bool IsMatlabInstalled
        {
            get
            {
                if (_isMatlabInstalled == null)
                {
                    // We consider MATLAB to be installed if its COM object is registered
                    var matlabType = Type.GetTypeFromProgID("Matlab.Application");

                    _isMatlabInstalled = !(matlabType == null);
                }

                return _isMatlabInstalled.Value;
            }
        }
    }
}