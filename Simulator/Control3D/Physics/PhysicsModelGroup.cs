using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Simulator.Util;

namespace Simulator.Control3D.Physics
{
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

        public Size3D Size { get; set; }

        public void ApplyMatrix(Matrix3D matrix)
        {
            foreach (var model in this)
            {
                model.Transform = new MatrixTransform3D(matrix);
            }
        }

        public Matrix3D GetTransform()
        {
            return this[0].Transform.Value;
        }
    }
}