namespace Simulator.HelixOnly
{
    public struct PrincipalAxis
    {
        private double _pitch;
        private double _roll;
        private double _yaw;

        public PrincipalAxis(double pitch, double roll, double yaw)
        {
            _pitch = pitch % 360.0;
            _roll = roll % 360.0;
            _yaw = yaw % 360.0;
        }

        public double Pitch
        {
            get { return _pitch; }
            set { _pitch = value % 360.0; }
        }

        public double Roll
        {
            get { return _roll; }
            set { _roll = value % 360.0; }
        }

        public double Yaw
        {
            get { return _yaw; }
            set { _yaw = value % 360.0; }
        }

        public static PrincipalAxis operator +(PrincipalAxis left, PrincipalAxis right)
        {
            return new PrincipalAxis(left._pitch + right._pitch,
                left._roll + right._roll,
                left._yaw + right._yaw);
        }

        public static PrincipalAxis operator -(PrincipalAxis left, PrincipalAxis right)
        {
            return new PrincipalAxis(left._pitch - right._pitch,
                left._roll - right._roll,
                left._yaw - right._yaw);
        }

        public static PrincipalAxis operator *(PrincipalAxis axis, double scalar)
        {
            return new PrincipalAxis(axis._pitch * scalar,
                axis._roll * scalar,
                axis._yaw * scalar);
        }

        public static PrincipalAxis operator *(double scalar, PrincipalAxis axis)
        {
            return new PrincipalAxis(axis._pitch * scalar,
                axis._roll * scalar,
                axis._yaw * scalar);
        }

        public static PrincipalAxis operator /(double scalar, PrincipalAxis axis)
        {
            return axis * (1.0 / scalar);
        }

        public static PrincipalAxis operator /(PrincipalAxis axis, double scalar)
        {
            return axis * (1.0 / scalar);
        }

        public override string ToString()
        {
            return $"Pitch: {Pitch}, Roll: {Roll}, Yaw: {Yaw}";
        }
    }
}