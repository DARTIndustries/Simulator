using System;
using System.Windows.Media.Media3D;

namespace Simulator.Control3D.Physics
{
    public interface IPhysicsSimulator
    {
        void ApplyForce(Point3D emitter, Vector3D force);

        void Tick(TimeSpan delta);
    }
}