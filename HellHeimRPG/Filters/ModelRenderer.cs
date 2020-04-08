using HellHeimRPG.Components;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG.Filters {
    class ModelRenderer : Filter {
        Shader shader;
        Shader terrain_shader;

        public ModelRenderer() {
            shader = new Shader(
                File.ReadAllText("Resources/Shaders/model_shader_vs.glsl"),
                File.ReadAllText("Resources/Shaders/model_shader_fs.glsl")
            );

            terrain_shader = new Shader(
                File.ReadAllText("Resources/Shaders/terrain_shader_vs.glsl"),
                File.ReadAllText("Resources/Shaders/terrain_shader_fs.glsl")
            );
        }

        public override void OnCleanup(Ent ent) {
        }

        public override void OnLoad(Ent ent) {
        }

        public override void Update() {
        }

        internal override void Render() {
            terrain_shader.Bind(() => { 
                var proj = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(45.0f),
                    (float)Game.WindowSize.Item1/(float)Game.WindowSize.Item2, 
                    0.01f,
                    1000f
                );

                shader.SetUniform(shader.GetLoc("projection"), proj);
                shader.SetUniform(shader.GetLoc("lightPos"), 0.5f, 0, 1.0f);
                shader.SetUniform(shader.GetLoc("view"), Game.Camera.ViewMatrix);

                foreach(var ent in Ecs.It.Each(typeof(Body), typeof(Terrain))) {
                    var body = ent.Get<Body>();
                    var terrain = ent.Get<Terrain>();

                    shader.SetUniform(shader.GetLoc("model"), body.Transform);

                    terrain.Render();
                } 
            });

            shader.Bind(() => { 
                var proj = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(45.0f),
                    (float)Game.WindowSize.Item1/(float)Game.WindowSize.Item2, 
                    0.01f,
                    1000f
                );

                shader.SetUniform(shader.GetLoc("projection"), proj);
                shader.SetUniform(shader.GetLoc("lightPos"), 0.5f, 0, 1.0f);
                shader.SetUniform(shader.GetLoc("view"), Game.Camera.ViewMatrix);

                foreach(var ent in Ecs.It.Each(typeof(Body), typeof(Model))) {
                    var body = ent.Get<Body>();
                    var model = ent.Get<Model>();

                    shader.SetUniform(shader.GetLoc("model"), body.Transform); 

                    if (model.Material.HasTexture) {
                        GL.BindTexture(TextureTarget.Texture2D, model.Material.Texture.Id);
                    }

                    var color = model.Material.Diffuse; 

                    shader.SetUniform(shader.GetLoc("specularStrength"), model.Material.Specular);
                    shader.SetUniform(shader.GetLoc("diffuse"), color.R, color.G, color.B);

                    model.Bind(() => {
                        GL.DrawArrays(PrimitiveType.Triangles, 0, model.Mesh.Vertices.Length / 3);
                    }); 

                    if (model.Material.HasTexture) {
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }
                } 
            }); 
        }
    }
}
