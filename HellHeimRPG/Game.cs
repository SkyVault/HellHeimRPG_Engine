using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using HellHeimRPG.Components;
using HellHeimRPG.Filters;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using BulletSharp;
using Harp;
using ImGuiNET;
using Vector3 = OpenTK.Vector3;

namespace HellHeimRPG
{
    public class Clock
    {
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

    class Game
    {
        private static (int, int) _windowSize = (0, 0);
        public static (int, int) WindowSize { get => _windowSize; }

        public void Resize((int, int) size) => Game._windowSize = size;

        private static Camera3D _camera = new Camera3D();
        public static Camera3D Camera { get => _camera; }

        static List<Level> _levels = new List<Level>();
        public static Level CurrentLevel { get => _levels[0]; }

        public static Clock Clock { get; } = new Clock();

        public static Physics Physics { get; protected set; }

        public static Harp.Harp Harp { get; protected set; } = new Harp.Harp();
        public static Harp.Env Env { get; protected set; } = new Harp.Env();

        private FrameBuffer framebuffer;

        public void Load()
        {
            // Init harp
            Game.Harp.LoadHarpLibInto(Env);
            Game.Harp.Eval(Env, "(io/writeln \"Hello from Harp!\"");

            // Init physics
            Physics = new Physics();

            Functional.Test.greetFromFS("Dustin");

            _levels.Add(new Level());

            // Register all the filters
            Ecs.It.Register<ModelRenderer>();

            Assets.It.Add(Loader.LoadMesh("Resources/Models/monkey.obj"), "monkey");
            Assets.It.Add(Loader.LoadMesh("Resources/Models/cube.obj"), "cube");
            Assets.It.Add(Loader.LoadMesh("Resources/Models/terrain.obj"), "terrain");

            var model = new Model(Assets.It.Get<Mesh>("monkey"));
            var cubem = new Model(Assets.It.Get<Mesh>("cube"));
            var terr  = new Model(Assets.It.Get<Mesh>("terrain"));

            var texture = new Texture();
            texture.Load("Resources/Textures/grass.jpg");

            Assets.It.Add(texture, "grass");

            terr.Material.Texture = texture;
            cubem.Material.Texture = texture;

            var monkey = Ecs.It.Create();
            monkey.Tag = "monkey";
            monkey.Add(model);
            monkey.Add(new Transform()
            {
                Translation = new Vector3(0, 0, -4),
                Scale = new Vector3(1.0f, 1.0f, 1.0f),
            });
            monkey.Add(new Selectable() { State = true });

            var terrain = Ecs.It.Create();
            terrain.Tag = "terrain";
            terrain.Add(new Transform() { Translation = new Vector3(-1, -0.2f, -3) });
            terrain.Add(terr);
            terrain.Add(Physics.AddStaticMeshShape(terr.Mesh, terrain.Get<Transform>().Matrix));
            terrain.Add(new Selectable());

            var cube = Ecs.It.Create();
            cube.Tag = "cube";
            cube.Add(new Transform()
            {
                Translation = new Vector3(0.0f, 0.0f, -2.0f),
                Scale = new Vector3(0.2f, 0.2f, 0.2f)
            });
            cube.Add(cubem);
            cube.Add(
                Physics.CreateRidgedBody(10, Matrix4.Identity,
                    Physics.AddBoxShape(new BulletSharp.Math.Vector3(1, 1, 1)))
            );
            cube.Add(new Selectable());

            cube.Get<RigidBody>().Translate(new BulletSharp.Math.Vector3(0, 20, 0));

            framebuffer = Art.It.CreateFbo(1280, 720, "main");
        }

        public void Tick(double delta)
        {
            Clock.Update((float)delta);

            Physics.Update();

            Input.It.Update();

            _camera.Update();
            Ecs.It.Update();
        }

        public void Render()
        {
            framebuffer.Bind(() =>
            {
                GL.Enable(EnableCap.DepthTest);
                GL.ClearColor(0, 0, 0, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                Ecs.It.Render(); 
            });

            Art.It.RenderToScreen(framebuffer);
        }
    }
}
