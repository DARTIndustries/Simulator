using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace Simulator
{
    public class Model3DWrapper
    {
        private readonly DispatcherObject _owner;
        private bool _camUpdates = true;
        public MeshElement3D Model { get; }

        public Model3DWrapper(MeshElement3D model, DispatcherObject owner)
        {
            _owner = owner;
            Model = model;
        }

        public Vector3D Velocity { get; set; }

        public PrincipalAxis AxisVelocity { get; set; }

        public PrincipalAxis Axis { get; set; }

        public Point3D TransformPosition => new Point3D(Model.Transform.Value.OffsetX, Model.Transform.Value.OffsetY, Model.Transform.Value.OffsetZ);

        public event Action<Point3D> UpdateCamera;

        public void Rotate(PrincipalAxis offset)
        {
            var matrix = Model.Transform.Value;

            var axis = new Vector3D(1, 0, 0);
            var angle = offset.Roll;

            matrix.Rotate(new Quaternion(axis, angle));

            axis = new Vector3D(0, 1, 0);
            angle = offset.Pitch;

            matrix.Rotate(new Quaternion(axis, angle));

            axis = new Vector3D(0, 1, 1);
            angle = offset.Yaw;

            matrix.Rotate(new Quaternion(axis, angle));

            Axis = new PrincipalAxis(Axis.Pitch + offset.Pitch, Axis.Roll + offset.Roll, Axis.Yaw + offset.Yaw);

            Model.Transform = new MatrixTransform3D(matrix);

            if (_camUpdates)
                UpdateCamera?.Invoke(TransformPosition);
        }

        public void Move(Vector3D offset)
        {
            var matrix = Model.Transform.Value;
            matrix.Translate(offset);
            Model.Transform = new MatrixTransform3D(matrix);

            if (_camUpdates)
                UpdateCamera?.Invoke(TransformPosition);
        }

        public void Tick()
        {
            _owner.Dispatcher.BeginInvoke(new Action(() =>
            {
                _camUpdates = false;
                Move(Velocity);
                Rotate(AxisVelocity);
                _camUpdates = true;
                UpdateCamera?.Invoke(TransformPosition);
            }));
        }
    }
}
