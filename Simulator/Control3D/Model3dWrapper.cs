using System;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Simulator.HelixOnly
{
    public class Model3DWrapper
    {
        private readonly DispatcherObject _owner;
        private bool _camUpdates = true;
        public Model3D Model { get; }

        public Model3DWrapper(Model3D model, Point3D centerOfMass, DispatcherObject owner)
        {
            _owner = owner;
            Model = model;
        }

        public Vector3D Velocity { get; set; }

        public PrincipalAxis AxisVelocity { get; set; }

        public PrincipalAxis Axis { get; set; }

        public Point3D TransformPosition => 
            new Point3D(
                Model.Transform.Value.OffsetX, 
                Model.Transform.Value.OffsetY, 
                Model.Transform.Value.OffsetZ);
        
        private Point3D _lastPosition = default(Point3D);
        public event Action<Size3D, Point3D, Vector3D> PositionUpdated;


    }
}
