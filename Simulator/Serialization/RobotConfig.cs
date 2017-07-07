using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Simulator.Serialization
{
    public class RobotConfig
    {
        public string Name { get; set; }

        public string ModelFile { get; set; }

        public List<MotorConfiguration> Motors { get; set; }

        public Point3D CenterOfMass { get; set; }

        public int Mass { get; set; }
    }
}