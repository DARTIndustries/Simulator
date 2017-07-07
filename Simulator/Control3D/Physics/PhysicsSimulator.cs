using System;
using System.Windows.Media.Media3D;

namespace Simulator.Control3D.Physics
{
    public class PhysicsSimulator : IPhysicsSimulator
    {
        private readonly IPhysicsBody _body;

        public PhysicsSimulator(IPhysicsBody body)
        {
            _body = body;
            Drag = 0.98;
            Rotation = false;
            Movement = false;
        }

        public double Drag { get; set; }

        public bool Movement { get; set; }

        public bool Rotation { get; set; }

        public void ApplyForce(Point3D emitter, Vector3D force)
        {
            const double transScaling = 10.0;
            const double rotScaling = 100.0;

            if (Movement)
            {
                Acceleration += transScaling * force / _body.Mass;
            }

            if (Rotation)
            {
                var d = emitter - _body.CenterOfMass;

                var t = Vector3D.CrossProduct(d, force);


                RotationalAcceleration += new PrincipalAxis(
                    rotScaling * t.Y / MomentOfInertia(Axis.Y),
                    rotScaling * t.X / MomentOfInertia(Axis.X),
                    rotScaling * t.Z / MomentOfInertia(Axis.Z));
            }
        }

        public Vector3D Acceleration { get; private set; }

        public Vector3D Velocity { get; private set; }

        public PrincipalAxis RotationalAcceleration { get; private set; }

        public PrincipalAxis RotationalVelocity { get; private set; }

        public void Tick(TimeSpan delta)
        {
            if (Movement)
            {
                // Add current acceleration to velocity
                Velocity += Acceleration;

                // Apply the drag
                Velocity *= Drag;

                // Move the current velocity
                _body.Move(Velocity);
            }

            if (Rotation)
            {
                // Add the current RotationalAcceleration to Rotational Velocity
                RotationalVelocity += RotationalAcceleration;

                // Apply drag
                RotationalVelocity *= Drag;

                // Rotate around the CoM
                _body.Rotate(RotationalVelocity, _body.CenterOfMass);
            }

            //Clear the acceleration, but not velocity
            Acceleration = default(Vector3D);
            RotationalAcceleration = default(PrincipalAxis);
        }

        private double MomentOfInertia(Axis a)
        {
            switch (a)
            {
                case Axis.X:
                    return (_body.Mass / 12.0) * (Math.Pow(_body.Size.Y, 2) + Math.Pow(_body.Size.Z, 2));
                case Axis.Y:
                    return (_body.Mass / 12.0) * (Math.Pow(_body.Size.X, 2) + Math.Pow(_body.Size.Z, 2));
                case Axis.Z:
                    return (_body.Mass / 12.0) * (Math.Pow(_body.Size.Y, 2) + Math.Pow(_body.Size.X, 2));
                default:
                    throw new ArgumentOutOfRangeException(nameof(a), a, null);
            }
        }
    }
}