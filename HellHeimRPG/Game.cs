using System;
using System.Collections.Generic;
using System.Text;
using HellHeimRPG.Components;
using HellHeimRPG.Filters;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using BulletSharp;

namespace HellHeimRPG {
    public class Clock {
        public float Time { get; private set; } = 0.0f;
        public float Delta { get; private set; } = 0.0016f;
        public int Ticks { get; private set; } = 0;

        public float Fps => 1 / (Math.Abs(Delta) < 0.0001f ? 0.0001f : Delta);

        public void Update(float delta)
        {
            Delta = delta;
            Ticks += 1;
            Time += delta;
        }
    }

    class Game {
        private static (int, int) _windowSize = (0, 0); 
        public static (int, int) WindowSize { get => _windowSize;  }

        public void Resize((int, int) size) => Game._windowSize = size;

        private static Camera3D _camera = new Camera3D();
        public static Camera3D Camera { get => _camera; }

        static List<Level> _levels = new List<Level>(); 
        public static Level CurrentLevel { get => _levels[0]; }

        public static Clock Clock { get; } = new Clock();

        public static Physics Physics { get; protected set; }

        public void Load() { 
            // Init physics
            Physics = new Physics();

            Functional.Test.greetFromFS("Dustin"); 

            _levels.Add(new Level());

            // Register all the filters
            Ecs.It.Register<ModelRenderer>();

            var model = new Model(Loader.LoadMesh("Resources/Models/monkey.obj"));
            var cubem = new Model(Loader.LoadMesh("Resources/Models/cube.obj"));
            var terr = new Model(Loader.LoadMesh("Resources/Models/terrain.obj"));

            var texture = new Texture();
            texture.Load("Resources/Textures/grass.jpg");
            //cubem.Material.Texture = texture;
            //model.Material.Texture = texture;

            terr.Material.Texture = texture;
            cubem.Material.Texture = texture;

            var monkey = Ecs.It.Create();
            monkey.Tag = "monkey";
            monkey.Add(model);
            monkey.Add(new Transform() { 
                Translation = new Vector3(0, 0, -4),
                Scale = new Vector3(1.0f, 1.0f, 1.0f),
            });

            var terrain = Ecs.It.Create();
            terrain.Tag = "terrain";
            terrain.Add(new Transform() { Translation = new Vector3(-1, -0.2f, -3) });
            terrain.Add(terr); 
            terrain.Add(Physics.AddStaticMeshShape(terr.Mesh, terrain.Get<Transform>().Matrix));

            var cube = Ecs.It.Create();
            cube.Tag = "cube";
            cube.Add(new Transform() { Translation = new Vector3(0.0f, 0.0f, -2.0f),
                                  Scale = new Vector3(0.2f, 0.2f, 0.2f) });
            cube.Add(cubem);
            cube.Add(
                Physics.CreateRidgedBody(10, Matrix4.Identity, 
                    Physics.AddBoxShape(new BulletSharp.Math.Vector3(1, 1, 1)))
            );

            cube.Get<RigidBody>().Translate(new BulletSharp.Math.Vector3(0, 20, 0));
        }

        public void Tick(double delta) { 
            Clock.Update((float)delta);

            Physics.Update();

            Input.It.Update();

            _camera.Update();
            Ecs.It.Update(); 
        }

        public void Render() { 
            Ecs.It.Render();
        } 
    }
}
