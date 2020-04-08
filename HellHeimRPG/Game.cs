using System;
using System.Collections.Generic;
using System.Text;
using HellHeimRPG.Components;
using HellHeimRPG.Filters;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG {
    class Game {
        private static (int, int) window_size = (0, 0); 
        public static (int, int) WindowSize { get => window_size;  }

        public void Resize((int, int) size) => Game.window_size = size;

        private static Camera3D camera = new Camera3D();
        public static Camera3D Camera { get => camera; }

        static List<Level> levels = new List<Level>(); 
        public static Level CurrentLevel { get => levels[0]; }

        public void Load() {
            Functional.Test.greetFromFS("Dustin"); 

            levels.Add(new Level());

            // Register all the filters
            Ecs.It.Register<ModelRenderer>();

            var model = new Model(Loader.LoadMesh("Resources/Models/monkey.obj"));
            var cubem = new Model(Loader.LoadMesh("Resources/Models/cube.obj"));

            var texture = new Texture();
            texture.Load("Resources/Textures/Lol_wut.png");
            cubem.Material.Texture = texture;
            model.Material.Texture = texture;

            var monkey = Ecs.It.Create();
            monkey.Tag = "monkey";
            monkey.Add(model);
            monkey.Add(new Body() { 
                Translation = new Vector3(0, 0, -4),
                Scale = new Vector3(1.0f, 1.0f, 1.0f),
            });

            var terrain = Ecs.It.Create();
            terrain.Tag = "terrain";
            terrain.Add(new Body() { Translation = new Vector3(-1, -0.2f, -3) });
            terrain.Add(new Terrain());

            var cube = Ecs.It.Create();
            cube.Tag = "cube";
            cube.Add(new Body() { Translation = new Vector3(0.0f, 0.0f, -2.0f),
                                  Scale = new Vector3(0.2f, 0.2f, 0.2f) });
            cube.Add(cubem);
        }

        float timer = 0;
        public void Tick(double delta) {
            timer += (float)delta;

            //foreach(var e in Ecs.It.Each(typeof(Body))) { 
            //    e.Get<Body>().Orientation = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), timer);
            //}
            Input.It.Update();

            camera.Update();
            Ecs.It.Update(); 
        }

        public void Render() { 
            Ecs.It.Render();
        } 
    }
}
