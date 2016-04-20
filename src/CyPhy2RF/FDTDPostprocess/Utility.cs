using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Postprocess
{
    public class Utility
    {
        public static double[] LinearSpace(double start, double end, uint n)
        {
            if (end < start)
            {
                return null;
            }

            double[] space = new double[n];
            for (int i = 0; i < n; i++)
            {
                space[i] = start + i * (end - start) / (n - 1);
            }
            space[n - 1] = end;

            return space;
        }
    }
}
