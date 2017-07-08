using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using BulletSharp.Math;
using Simulator.Control3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

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

        public static Vector3D ToMedia3D(this Vector3 v)
        {
            return new Vector3D(v.X, v.Y, v.Z);
        }

        public static Vector3 ToBullet(this Vector3D v)
        {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static Point3D ToMedia3D(this Point3D v)
        {
            return new Point3D(v.X, v.Y, v.Z);
        }

        public static Point3D ToBullet(this Point3D v)
        {
            return new Point3D((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static Matrix3D ToMedia3D(this Matrix v)
        {
            return new Matrix3D(
                v.M11, v.M12, v.M13, v.M14,
                v.M21, v.M22, v.M23, v.M24,
                v.M31, v.M32, v.M33, v.M34,
                v.M41, v.M42, v.M43, v.M44);
        }

        public static Matrix ToBullet(this Matrix3D v)
        {
            return new Matrix(
                (float)v.M11, (float)v.M12, (float)v.M13, (float)v.M14,
                (float)v.M21, (float)v.M22, (float)v.M23, (float)v.M24,
                (float)v.M31, (float)v.M32, (float)v.M33, (float)v.M34,
                (float)v.OffsetX, (float)v.OffsetY, (float)v.OffsetZ, (float)v.M44);
        }

    }
}
