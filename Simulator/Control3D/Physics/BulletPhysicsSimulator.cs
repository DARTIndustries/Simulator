using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using BulletSharp;
using BulletSharp.Math;
using HelixToolkit.Wpf;
using IronPython.Modules;
using Simulator.Util;

namespace Simulator.Control3D.Physics
{
    public class BulletPhysicsSimulator : IDisposable
    {
        private DynamicsWorld _world;
        private CollisionConfiguration _collisionConfiguration;
        private Dispatcher _dispatcher;
        private BroadphaseInterface _broadphase;
        private List<CollisionShape> _collisionShapes { get; } = new List<CollisionShape>();
        private List<Model3D> _linkedBodies;
        private RigidBody _primary;

        public BulletPhysicsSimulator()
        {
            _collisionConfiguration = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collisionConfiguration);
            _broadphase = new DbvtBroadphase();
            _world = new DiscreteDynamicsWorld(_dispatcher, _broadphase, null, _collisionConfiguration);
            _world.Gravity = new Vector3(0, 0,-9.8f);

            _linkedBodies = new List<Model3D>();

            BoxShape platform = new BoxShape(25, 25, 0);
            _collisionShapes.Add(platform);
            LocalCreateRigidBody(0, Matrix.Identity, platform);
            platform.UserObject = "Platform";


            BoxShape ground = new BoxShape(500, 500, 0);
            _collisionShapes.Add(ground);
            var offset = Matrix3D.Identity;
            offset.Translate(new Vector3D(0, 0, -50));
            LocalCreateRigidBody(0, offset.ToBullet(), ground);



        }

        public void SetPrimaryBody(Model3D body, float mass, Point3D centerOfMass, Vector3D translate)
        {
            var vectorSize = new Vector3D(body.Bounds.Size.X / 2.0, body.Bounds.Size.Y / 2.0, body.Bounds.Size.Z / 2.0).ToBullet();
            BoxShape bodyShape = new BoxShape(vectorSize);

            _collisionShapes.Add(bodyShape);
            var localInertia = bodyShape.CalculateLocalInertia(mass);

            using (var info = new RigidBodyConstructionInfo(mass, null, bodyShape, localInertia))
            {
                RigidBody rbody = new RigidBody(info);
                rbody.UserObject = body;
                rbody.Translate(translate.ToBullet());

                if (_primary != null)
                    _world.RemoveRigidBody(_primary);

                _primary = rbody;
                _world.AddRigidBody(rbody);
            }
        }



        public void AddLinkedBody(Model3D secondary)
        {
            _linkedBodies.Add(secondary);
        }

        public void ApplyForce(Point3D emitter, Vector3D force)
        {
            const int forceScale = 5;

            force *= forceScale;

            if (force.X != 0 || force.Y != 0 || force.Z != 0)
            {
                _primary?.Activate();
                _primary?.ApplyForce(force.ToBullet(), _primary.CenterOfMassPosition - emitter.ToVector3D().ToBullet());
            }
        }

        public void Tick(TimeSpan delta)
        {
            _world?.StepSimulation((float)delta.TotalSeconds);

            foreach (RigidBody cobj in _world.CollisionObjectArray)
            {
                if (cobj.UserObject is Model3D)
                {
                    var model = (Model3D) cobj.UserObject;
                    Debug.Print(cobj.CenterOfMassPosition.ToString());
                    var worldMatrix = cobj.WorldTransform.ToMedia3D();
                    model.Transform = new MatrixTransform3D(worldMatrix);

                    foreach (var linkedBody in _linkedBodies)
                    {
                        linkedBody.Transform = new MatrixTransform3D(worldMatrix);
                    }
                }
            }
        }

        public RigidBody LocalCreateRigidBody(float mass, Matrix startTransform, CollisionShape shape)
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
        public bool Primary { get; set; }

        public Point3D CenterOfMass { get; set; }

        public float Mass { get; set; }

        public bool Collideable { get; set; }
    }
}
