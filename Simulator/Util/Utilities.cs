using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Simulator.HelixOnly;

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

    public static class ModelExtensions
    {
        public static Point3D FindCenter(Model3D model)
        {
            var bounds = model.Bounds;

            double centerX = bounds.X + bounds.SizeX / 2;
            double centerY = bounds.Y + bounds.SizeY / 2;
            double centerZ = bounds.Z + bounds.SizeZ / 2;

            return new Point3D(centerX, centerY, centerZ);
        }

        public static void TransformAll(this Model3DGroup group, Func<Matrix3D, Matrix3D> generator)
        {
            group.Children.TransformAll(generator);
        }

        public static void TransformAll(this IEnumerable<Model3D> group, Func<Matrix3D, Matrix3D> generator)
        {
            foreach (var groupChild in group)
            {
                groupChild.Transform = new MatrixTransform3D(generator(groupChild.Transform.Value));
            }
        }

        public static void Move(this Model3D model, Vector3D offset)
        {
            var matrix = model.Transform.Value;
            matrix.Translate(offset);
            model.Transform = new MatrixTransform3D(matrix);
        }

        public static void Scale(this Model3D model, double amount)
        {
            var matrix = model.Transform.Value;
            matrix.Scale(new Vector3D(amount, amount, amount));
            model.Transform = new MatrixTransform3D(matrix);
        }

        public static void Rotate(this Model3D model, PrincipalAxis offset, Point3D about)
        {
            var matrix = model.Transform.Value;

            var axis = new Vector3D(1, 0, 0);
            var angle = offset.Roll;

            matrix.RotateAt(new Quaternion(axis, angle), about);

            axis = new Vector3D(0, 1, 0);
            angle = offset.Pitch;

            matrix.RotateAt(new Quaternion(axis, angle), about);

            axis = new Vector3D(0, 0, 1);
            angle = offset.Yaw;

            matrix.RotateAt(new Quaternion(axis, angle), about);

            model.Transform = new MatrixTransform3D(matrix);
        }

        public static double Distance(Point3D p1, Point3D p2)
        {
            double deltaX = p1.X - p2.X;
            double deltaY = p1.Y - p2.Y;
            double deltaZ = p1.Z - p2.Z;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

    }
}
