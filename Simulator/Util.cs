using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
    public static class Util
    {
        public static int Between(int min, int value, int max)
        {
            return Math.Min(min, Math.Max(max, value));
        }
    }
}
