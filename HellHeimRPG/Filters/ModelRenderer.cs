using HellHeimRPG.Components;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using BulletSharp;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG.Filters {
    class ModelRenderer : Filter {
        Shader _shader;
        Shader _terrainShader;

        private int modelLoc = -1;
        private int hasTextureLoc = -1;
        private int specularStrengthLoc = -1;
        private int diffuseLoc = -1;

        public ModelRenderer() {
            _shader = new Shader(
                File.ReadAllText("Resources/Shaders/model_shader_vs.glsl"),
                File.ReadAllText("Resources/Shaders/model_shader_fs.glsl")
            );

            _terrainShader = new Shader(
                File.ReadAllText("Resources/Shaders/terrain_shader_vs.glsl"),
                File.ReadAllText("Resources/Shaders/terrain_shader_fs.glsl")
            );

            modelLoc = _shader.GetLoc("model");
            hasTextureLoc = _shader.GetLoc("hasTexture"); 
            specularStrengthLoc = _shader.GetLoc("specularStrength");
            diffuseLoc = _shader.GetLoc("diffuse");
        }

        public override void OnCleanup(Ent ent) {
        }

        public override void OnLoad(Ent ent) {
        }

        public override void Update() {
        }

        internal override void Render() {
            _terrainShader.Bind(() =>
            {
                var proj = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(45.0f),
                    (float)Game.WindowSize.Item1/(float)Game.WindowSize.Item2, 
                    0.01f,
                    1000f
                );

                _shader.SetUniform(_shader.GetLoc("projection"), proj);
                _shader.SetUniform(_shader.GetLoc("view"), Game.Camera.ViewMatrix);

                foreach(var ent in Ecs.It.Each(typeof(Transform), typeof(Terrain))) {
                    var body = ent.Get<Transform>();
                    var terrain = ent.Get<Terrain>();

                    _shader.SetUniform(_shader.GetLoc("model"), body.Matrix);

                    terrain.Render();
                } 
            });

            _shader.Bind(() => { 
                var proj = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(45.0f),
                    (float)Game.Resolution.W/(float)Game.Resolution.H, 
                    0.01f,
                    1000f
                );

                _shader.SetUniform(_shader.GetLoc("projection"), proj);
                _shader.SetUniform(_shader.GetLoc("lightPos"), 0.5f, 0, 1.0f);
                _shader.SetUniform(_shader.GetLoc("view"), Game.Camera.ViewMatrix);

                foreach(var ent in Ecs.It.Each(typeof(Transform), typeof(Model))) {
                    var body = ent.Get<Transform>();
                    var model = ent.Get<Model>();

                    var matrix = body.Matrix;

                    if (ent.Has<RigidBody>()) {
                        matrix = Game.Physics.Convert(ent.Get<RigidBody>().WorldTransform);
                    }

                    _shader.SetUniform(modelLoc, matrix); 

                    if (model.Material.HasTexture) {
                        _shader.SetUniform(hasTextureLoc, 1f); 
                        GL.BindTexture(TextureTarget.Texture2D, model.Material.Texture.Id);
                    } else
                    { 
                        _shader.SetUniform(hasTextureLoc, 0f); 
                    }

                    var color = model.Material.Diffuse; 

                    _shader.SetUniform(specularStrengthLoc, model.Material.Specular);
                    _shader.SetUniform(diffuseLoc, color.R, color.G, color.B);

                    model.Bind(() => {
                        GL.DrawArrays(PrimitiveType.Triangles, 0, model.Mesh.Vertices.Length / 3);

                        if (ent.Has<Selectable>()) {
                            if (ent.Get<Selectable>().State) { 
                                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                                GL.LineWidth(1);

                                var r = (((int) ((Math.Sin(Game.Clock.Time * 8) / 3) * (color.R) * 255) + 255/2) % 255) / 255f;
                                var g = (((int) ((Math.Sin(Game.Clock.Time * 8.1) / 3) * (color.G) * 255) + 255/2) % 255) / 255f;
                                var b = (((int) ((Math.Sin(Game.Clock.Time * 8.2) / 3) * (color.B) * 255) + 255/2) % 255) / 255f;
                                _shader.SetUniform(diffuseLoc, r, g, b);
                                GL.DrawArrays(PrimitiveType.Triangles, 0, model.Mesh.Vertices.Length / 3);
                                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                            }
                        }
                    }); 

                    if (model.Material.HasTexture) {
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }
                } 
            }); 
        }
    }
}
