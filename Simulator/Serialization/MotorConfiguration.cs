using System.Windows.Media.Media3D;

namespace Simulator.Serialization
{
    public struct MotorConfiguration
    {
        public string Key { get; set; }

        public Vector3D Vector { get; set; }

        public Point3D Location { get; set; }
    }
}