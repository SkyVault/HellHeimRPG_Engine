using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using System.Text;
using BulletSharp;
using BulletSharp.Math;
using HellHeimRPG.Components;
using Vector3 = BulletSharp.Math.Vector3;

namespace HellHeimRPG {
    class Physics { 
        public Vector3 Gravity { get; set; } = new Vector3(0, -10, 0);
        public DiscreteDynamicsWorld World { get; protected set; }

        private CollisionDispatcher collisionDispatcher;
        private DbvtBroadphase broadphase; 
        private List<CollisionShape> collisionShapes = new List<CollisionShape>();
        private CollisionConfiguration collisionConfiguration;

        public Physics()
        {
            collisionConfiguration = new DefaultCollisionConfiguration();
            collisionDispatcher = new CollisionDispatcher(collisionConfiguration);

            broadphase = new DbvtBroadphase();

            World = new DiscreteDynamicsWorld(collisionDispatcher, broadphase, null, collisionConfiguration); 
            World.Gravity = Gravity;

            // Test ground
            var ground = CreateRidgedBody(0, Matrix4.Identity, AddBoxShape(new Vector3(100, 1, 100)));
        }

        public BulletSharp.Math.Vector4 Convert(OpenTK.Vector4 v) {
            return new BulletSharp.Math.Vector4 {X = v.X, Y = v.Y, Z = v.Z, W = v.W};
        }

        public OpenTK.Vector4 Convert(BulletSharp.Math.Vector4 v) {
            return new OpenTK.Vector4{X = v.X, Y = v.Y, Z = v.Z, W = v.W};
        }

        public OpenTK.Matrix4 Convert(BulletSharp.Math.Matrix m) {
            return new OpenTK.Matrix4()
            {
                Column0 = Convert(m.Column1),
                Column1 = Convert(m.Column2),
                Column2 = Convert(m.Column3),
                Column3 = Convert(m.Column4)
            };
        }

        public BulletSharp.Math.Matrix Convert(OpenTK.Matrix4 m) {
            return new BulletSharp.Math.Matrix
            {
                Column1 = Convert(m.Column0),
                Column2 = Convert(m.Column1),
                Column3 = Convert(m.Column2),
                Column4 = Convert(m.Column3)
            };
        }

        public RigidBody CreateRidgedBody(float mass, Matrix4 startTransformTk, CollisionShape shape)
        {
            var startTransform = Convert(startTransformTk);

            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;

            if (isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(startTransform); 

            var rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            var body = new RigidBody(rbInfo);

            World.AddRigidBody(body);

            return body;
        }

        public CollisionShape AddBoxShape(Vector3 bounds)
        {
            var shape = new BoxShape(bounds.X, bounds.Y, bounds.Z);
            collisionShapes.Add(shape);
            return shape;
        }

        public void Update()
        {
            World.StepSimulation(Game.Clock.Delta); 
        }
    }
}
