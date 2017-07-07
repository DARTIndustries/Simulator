using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Simulator.HelixOnly
{
    public class MotorContoller : IEnumerable<Motor>
    {
        private readonly Dictionary<string, Motor> _motors;

        public MotorContoller()
        {
            _motors = new Dictionary<string, Motor>();
        }

        public Motor this[int index] => _motors.Values.ElementAt(index);

        public Motor this[string key] => _motors[key];

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Motor> GetEnumerator()
        {
            return _motors.Values.GetEnumerator();
        }


        public void Register(string key, Motor m)
        {
            _motors.Add(key, m);
        }

        public List<string> Keys => _motors.Keys.ToList();

        public Vector3D CalculateNetThrustVector()
        {
            return default(Vector3D);
        }
    }

    public class Motor
    {
        public Motor(Vector3D thrust)
        {
            Direction = thrust;
        }

        public Vector3D ThrustVector()
        {
            var normal =  (Thrust / 127.0) * Direction;
            normal.Negate();
            return normal;
        }

        public Point3D LabelLocation { get; set; }
        public Point3D ThrustLocation { get; set; }
        public Vector3D Direction { get; private set; }
        public sbyte Thrust { get; set; }
    }
}
