using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simulator.Util
{
    class DebugMode
    {
        public static Visibility DebugVisibility
        {
#if DEBUG
            get { return Visibility.Visible; }
#else
            get { return Visibility.Hidden; }
#endif
        }
    }
}
