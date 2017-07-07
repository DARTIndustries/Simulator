using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using SharpDX;
using Simulator.HelixOnly;
using Simulator.Util;

namespace Simulator.Control3D
{
    public interface IPhysicsBody
    {
        void Move(Vector3D delta);

        void Rotate(PrincipalAxis delta, Point3D about);

        int Mass { get; }

        double MomentOfInertia(Axis a);
        
        Point3D CenterOfMass { get; }
    }

    public class PhysicsModelGroup : List<Model3D>, IPhysicsBody
    {
        private readonly Rect3D _bounds;

        public int Mass { get; }

        public Point3D CenterOfMass { get; }

        public PhysicsModelGroup(int mass, Point3D centerOfMass, Rect3D bounds)
        {
            _bounds = bounds;
            Mass = mass;
            CenterOfMass = centerOfMass;
        }

        public void Move(Vector3D delta)
        {
            foreach (var model in this)
            {
                model.Move(delta);
            }
        }

        public void Rotate(PrincipalAxis delta, Point3D about)
        {
            foreach (var model in this)
            {
                model.Rotate(delta, about);
            }
        }

        public double MomentOfInertia(Axis a)
        {
            switch (a)
            {
                case Axis.X:
                    return (Mass / 12.0) * (Math.Pow(_bounds.SizeY, 2) + Math.Pow(_bounds.SizeZ, 2));
                case Axis.Y:
                    return (Mass / 12.0) * (Math.Pow(_bounds.SizeX, 2) + Math.Pow(_bounds.SizeZ, 2));
                case Axis.Z:
                    return (Mass / 12.0) * (Math.Pow(_bounds.SizeY, 2) + Math.Pow(_bounds.SizeX, 2));
                default:
                    throw new ArgumentOutOfRangeException(nameof(a), a, null);
            }
        }
    }

    public class PhysicsSimulator
    {
        private readonly IPhysicsBody _body;

        public PhysicsSimulator(IPhysicsBody body)
        {
            _body = body;
            Drag = 0.98;
            Rotation = true;
            Movement = true;
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
                    rotScaling * t.Y / _body.MomentOfInertia(Axis.Y),
                    rotScaling * t.X / _body.MomentOfInertia(Axis.X),
                    rotScaling * t.Z / _body.MomentOfInertia(Axis.Z));
            }
        }

        public Vector3D Acceleration { get; set; }

        public Vector3D Velocity { get; set; }

        public PrincipalAxis RotationalAcceleration { get; set; }

        public PrincipalAxis RotationalVelocity { get; set; }


        public void Tick()
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
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }
}
