using HellHeimRPG.Components;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using BulletSharp;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG.Filters
{
    class Renderer : Filter
    {
        Shader _shader;
        Shader _terrainShader;
        Shader _selectionShader;

        private int modelLoc = -1;
        private int hasTextureLoc = -1;
        private int specularStrengthLoc = -1;
        private int diffuseLoc = -1;

        private FrameBuffer selectionBuffer;
        private Cubemap cubemap = new Cubemap();

        public static string ScreenFBO { get; set; } = "main";

        public Renderer()
        {
            _shader = new Shader(
                File.ReadAllText("Resources/Shaders/model_shader_vs.glsl"),
                File.ReadAllText("Resources/Shaders/model_shader_fs.glsl")
            );

            _terrainShader = new Shader(
                File.ReadAllText("Resources/Shaders/terrain_shader_vs.glsl"),
                File.ReadAllText("Resources/Shaders/terrain_shader_fs.glsl")
            );

            _selectionShader = new Shader(
                File.ReadAllText("Resources/Shaders/selection_shader_vs.glsl"),
                File.ReadAllText("Resources/Shaders/selection_shader_fs.glsl")
            );

            modelLoc = _shader.GetLoc("model");
            hasTextureLoc = _shader.GetLoc("hasTexture");
            specularStrengthLoc = _shader.GetLoc("specularStrength");
            diffuseLoc = _shader.GetLoc("diffuse");

            selectionBuffer = new FrameBuffer(Game.Resolution.W, Game.Resolution.H);

            Art.It.Add(selectionBuffer, "selection");

            cubemap.Load(
                "Resources/Textures/skybox/right.png",
                "Resources/Textures/skybox/left.png",
                "Resources/Textures/skybox/top.png",
                "Resources/Textures/skybox/bottom.png",
                "Resources/Textures/skybox/front.png",
                "Resources/Textures/skybox/back.png");
        }

        public override void OnCleanup(Ent ent)
        {
        }

        public override void OnLoad(Ent ent)
        {
        }

        public override void Update()
        {
        }

        internal void RenderMain()
        {
            _terrainShader.Bind(() =>
            {
                var proj = Art.It.Projection;

                _shader.SetUniform(_shader.GetLoc("projection"), proj);
                _shader.SetUniform(_shader.GetLoc("view"), Game.Camera.ViewMatrix);

                foreach (var ent in Ecs.It.Each(typeof(Transform), typeof(Terrain)))
                {
                    var body = ent.Get<Transform>();
                    var terrain = ent.Get<Terrain>();

                    _shader.SetUniform(_shader.GetLoc("model"), body.Matrix);

                    terrain.Render();
                }
            });

            void render(Ent ent, Model model, Matrix4 matrix)
            {
                _shader.SetUniform(modelLoc, matrix);

                if (model.Material.HasTexture)
                {
                    _shader.SetUniform(hasTextureLoc, 1f);
                    GL.BindTexture(TextureTarget.Texture2D, model.Material.Texture.Id);
                }
                else
                {
                    _shader.SetUniform(hasTextureLoc, 0f);
                }

                var color = model.Material.Diffuse;

                _shader.SetUniform(specularStrengthLoc, model.Material.Specular);
                _shader.SetUniform(diffuseLoc, color.R, color.G, color.B);

                model.Bind(() =>
                {
                    GL.DrawArrays(PrimitiveType.Triangles, 0, model.Mesh.Vertices.Length / 3);

                    if (ent.Has<Selectable>())
                    {
                        if (ent.Get<Selectable>().State)
                        {
                            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                            GL.LineWidth(1);

                            var r = (((int)((Math.Sin(Game.Clock.Time * 8) / 3) * (color.R) * 255) + 255 / 2) % 255) / 255f;
                            var g = (((int)((Math.Sin(Game.Clock.Time * 8.1) / 3) * (color.G) * 255) + 255 / 2) % 255) / 255f;
                            var b = (((int)((Math.Sin(Game.Clock.Time * 8.2) / 3) * (color.B) * 255) + 255 / 2) % 255) / 255f;
                            _shader.SetUniform(diffuseLoc, r, g, b);
                            GL.DrawArrays(PrimitiveType.Triangles, 0, model.Mesh.Vertices.Length / 3);
                            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        }
                    }
                });

                if (model.Material.HasTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
            }

            _shader.Bind(() =>
            {
                _shader.SetUniform(_shader.GetLoc("projection"), Art.It.Projection);
                _shader.SetUniform(_shader.GetLoc("lightPos"), 0.5f, 0, 1.0f);
                _shader.SetUniform(_shader.GetLoc("view"), Game.Camera.ViewMatrix);

                foreach (var ent in Ecs.It.Each(typeof(RigidBody), typeof(Model)))
                {
                    var model = ent.Get<Model>();
                    var matrix = Game.Physics.Convert(ent.Get<RigidBody>().WorldTransform);
                    render(ent, model, matrix);
                }

                foreach (var ent in Ecs.It.Each(typeof(Transform), typeof(Model)))
                {
                    var body = ent.Get<Transform>();
                    var model = ent.Get<Model>();
                    render(ent, model, body.Matrix);
                }
            });
        }

        internal void RenderSelection()
        {
            _selectionShader.Bind(() =>
            {
                _selectionShader.SetUniform(_selectionShader.GetLoc("projection"), Art.It.Projection);
                _selectionShader.SetUniform(_selectionShader.GetLoc("view"), Game.Camera.ViewMatrix);

                foreach (var ent in Ecs.It.Each(typeof(RigidBody), typeof(Model), typeof(Selectable)))
                {
                    var selectable = ent.Get<Selectable>();
                    var model = ent.Get<Model>();
                    var matrix = Game.Physics.Convert(ent.Get<RigidBody>().WorldTransform);

                    _selectionShader.SetUniform(_selectionShader.GetLoc("Key"), selectable.Key);
                    _selectionShader.SetUniform(_selectionShader.GetLoc("model"), matrix);

                    model.Bind(() =>
                    {
                        GL.DrawArrays(PrimitiveType.Triangles, 0, model.Mesh.Vertices.Length / 3);
                    });
                }

                foreach (var ent in Ecs.It.Each(typeof(Transform), typeof(Model), typeof(Selectable)))
                {
                    var selectable = ent.Get<Selectable>();
                    var model = ent.Get<Model>();

                    _selectionShader.SetUniform(_selectionShader.GetLoc("Key"), selectable.Key);
                    _selectionShader.SetUniform(_selectionShader.GetLoc("model"), ent.Get<Transform>().Matrix);

                    model.Bind(() =>
                    {
                        GL.DrawArrays(PrimitiveType.Triangles, 0, model.Mesh.Vertices.Length / 3);
                    });
                }
            });
        }

        internal override void Render()
        {
            var fbo = Art.It.GetFbo("main");

            fbo.Bind(() =>
            {
                GL.Enable(EnableCap.DepthTest);
                GL.ClearColor(0, 0, 0, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Viewport(0, 0, Game.Resolution.W, Game.Resolution.H);

                Art.It.RenderSkyBox(cubemap, Art.It.Projection, Game.Camera.ViewMatrixStatic);
                RenderMain();
            });

            selectionBuffer.Bind(() =>
            {
                GL.Enable(EnableCap.DepthTest);
                GL.ClearColor(0, 0, 0, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Viewport(0, 0, Game.Resolution.W, Game.Resolution.H);

                RenderSelection();
            });

            Art.It.RenderToScreen(Art.It.GetFbo(ScreenFBO));
        }
    }
}
