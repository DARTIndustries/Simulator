using System.Windows.Media.Media3D;
using Simulator.Control3D.Physics;

namespace Simulator.Control3D
{
    public interface IPhysicsBody
    {
        void Move(Vector3D delta);

        void Rotate(PrincipalAxis delta, Point3D about);

        void ApplyMatrix(Matrix3D matrix);

        int Mass { get; }

        Size3D Size { get; set; }
        
        Point3D CenterOfMass { get; }

        Matrix3D GetTransform();
    }
}