using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using BulletSharp;
using BulletSharp.Math;
using IronPython.Modules;
using Simulator.Util;

namespace Simulator.Control3D.Physics
{
    class BulletPhysicsSimulator : IPhysicsSimulator, IDisposable
    {
        private DynamicsWorld _world;
        private CollisionConfiguration _collisionConfiguration;
        private Dispatcher _dispatcher;
        private BroadphaseInterface _broadphase;
        private List<CollisionShape> _collisionShapes { get; } = new List<CollisionShape>();

        public BulletPhysicsSimulator()
        {
            _collisionConfiguration = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collisionConfiguration);
            _broadphase = new DbvtBroadphase();
            _world = new DiscreteDynamicsWorld(_dispatcher, _broadphase, null, _collisionConfiguration);
            _world.Gravity = new Vector3(0, 0,-9.8f);

            BoxShape groundShape = new BoxShape(50, 50, 0.01f);
            _collisionShapes.Add(groundShape);

            CollisionObject ground = LocalCreateRigidBody(0, Matrix.Identity, groundShape);
            ground.UserObject = "Ground";
        }

        public void AddBody(Model3D body, PhysicsInfo physInfo)
        {
            var vectorSize = new Vector3D(body.Bounds.Size.X / 2.0, body.Bounds.Size.Y / 2.0, body.Bounds.Size.Z / 2.0).ToBullet();
            BoxShape bodyShape = new BoxShape(vectorSize);

            _collisionShapes.Add(bodyShape);
            var localInertia = bodyShape.CalculateLocalInertia(physInfo.Mass);

            using (var info = new RigidBodyConstructionInfo(physInfo.Mass, null, bodyShape, localInertia))
            {
                RigidBody rbody = new RigidBody(info);
                rbody.UserObject = body;

                rbody.Translate(new Vector3(0, 0, 0.01f));

                _world.AddRigidBody(rbody);
            }
        }

        public void ApplyForce(Point3D emitter, Vector3D force)
        {
            
        }

        public void Tick(TimeSpan delta)
        {
            _world?.StepSimulation((float)delta.TotalSeconds);

            foreach (var cobj in _world.CollisionObjectArray)
            {
                if (cobj.UserObject is Model3D)
                {
                    var model = (Model3D) cobj.UserObject;

                    model.Transform = new MatrixTransform3D(cobj.WorldTransform.ToMedia3D());
                }
            }
        }

        public virtual RigidBody LocalCreateRigidBody(float mass, Matrix startTransform, CollisionShape shape)
        {
            //rigidbody is dynamic if and only if mass is non zero, otherwise static
            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            //using motionstate is recommended, it provides interpolation capabilities, and only synchronizes 'active' objects
            DefaultMotionState myMotionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);
            rbInfo.Dispose();

            _world.AddRigidBody(body);

            return body;
        }

        public void Dispose()
        {
            _world?.Dispose();
            _collisionConfiguration?.Dispose();
            _dispatcher?.Dispose();
            _broadphase?.Dispose();
        }
    }

    public struct PhysicsInfo
    {
        public Point3D CenterOfMass { get; set; }

        public float Mass { get; set; }
    }
}
