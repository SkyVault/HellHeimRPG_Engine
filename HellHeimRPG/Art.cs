using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Buffer = OpenTK.Graphics.OpenGL4.Buffer;

namespace HellHeimRPG
{
    class FrameBuffer
    {
        public int ColorBuffer { get; set; } = 0;
        public int Fbo { get; set; } = 0;
        public int Rbo { get; set; } = 0;

        ~FrameBuffer()
        {
            if (Fbo > 0) GL.DeleteFramebuffer(Fbo);
            if (ColorBuffer > 0) GL.DeleteTexture(ColorBuffer);
        }

        public FrameBuffer(int width, int height)
        {
            Fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

            ColorBuffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            Rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, Rbo);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind(Action action)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
            action();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }

    class Art
    {
        private int _vao = 0;
        private int _vbo = 0;
        private Shader shader;

        private int _cube_vao = 0;
        private int _cube_vbo = 0;
        private Shader skybox_shader;

        public Matrix4 Projection =>
            Matrix4.CreatePerspectiveFieldOfView(
                        MathHelper.DegreesToRadians(45.0f),
                        (float)Game.WindowSize.Item1 / (float)Game.WindowSize.Item2,
                        0.01f,
                        1000f);


        private Art()
        {
            float[] vertices = new[]
            { // positions   // texCoords
             -1.0f,  1.0f,  0.0f, 1.0f,
             -1.0f, -1.0f,  0.0f, 0.0f,
              1.0f, -1.0f,  1.0f, 0.0f,

             -1.0f,  1.0f,  0.0f, 1.0f,
              1.0f, -1.0f,  1.0f, 0.0f,
              1.0f,  1.0f,  1.0f, 1.0f
            };

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), IntPtr.Zero);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            shader = new Shader(
@"
#version 450 core
layout (location = 0) in vec2 iVertex;
layout (location = 1) in vec2 iUvs; 

out vec2 Uvs;

void main() {
    Uvs = iUvs; 
    gl_Position = vec4(iVertex.x, iVertex.y, 0.0, 1.0);
} 
",
@" 
#version 450 core
in vec2 Uvs;

out vec4 Result;

uniform sampler2D screenTexture;

void main() {
    vec3 texel = texture(screenTexture, Uvs).rgb; 
    Result = vec4(texel, 1.0); 
}
"
                );

            float[] cubeVertex = {
                // positions          
                -1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f
            };

            _cube_vao = GL.GenVertexArray();
            GL.BindVertexArray(_cube_vao);

            _cube_vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _cube_vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * cubeVertex.Length, cubeVertex, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            skybox_shader = new Shader(
                File.ReadAllText("Resources/Shaders/skybox_shader_vs.glsl"),
                File.ReadAllText("Resources/Shaders/skybox_shader_fs.glsl"));
        }

        private static Art _instance = null;

        public static Art It
        {
            get
            {
                if (_instance == null) { _instance = new Art(); }
                return _instance;
            }
        }

        private Dictionary<string, FrameBuffer> frameBuffers = new Dictionary<string, FrameBuffer>();

        public FrameBuffer GetFbo(string id) => frameBuffers[id];

        public void RenderSkyBox(Cubemap cubemap, Matrix4 projection, Matrix4 view)
        {
            GL.DepthMask(false);

            skybox_shader.Bind(() =>
            {
                skybox_shader.SetUniform(skybox_shader.GetLoc("projection"), projection);
                skybox_shader.SetUniform(skybox_shader.GetLoc("view"), view);
                BindVao(_cube_vao, () =>
                {
                    cubemap.Bind(() =>
                    {
                        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
                    });
                });
            });

            GL.DepthMask(true);
        }

        public IEnumerable<(string, FrameBuffer)> Fbos()
        {
            foreach (var (key, value) in frameBuffers)
            {
                yield return (key, value);
            }
        }

        public void Add(FrameBuffer buffer, string id) => frameBuffers[id] = buffer;

        public FrameBuffer CreateFbo(int width, int height, string id)
        {
            var fbo = new FrameBuffer(width, height);
            frameBuffers[id] = fbo;
            return fbo;
        }

        public void BindVao(int vao, Action func)
        {
            GL.BindVertexArray(vao);
            func();
            GL.BindVertexArray(0);
        }

        public void BindBuffer(int vbo, Action func)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            func();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void RenderToScreen(FrameBuffer buffer)
        {
            GL.Viewport(0, 0, Game.WindowSize.Item1, Game.WindowSize.Item2);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, buffer.ColorBuffer);
            shader.Bind(() =>
            {
                GL.BindVertexArray(_vao);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                GL.BindVertexArray(0);
            });
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Model GenTriangle(float size = 1)
        {
            return new Model(
                new Mesh(
                    new float[]{
                      -size, -size, 0.0f,
                       size, -size, 0.0f,
                       0.0f,  size, 0.0f,
                    },
                    new float[] { },
                    new float[] { }
                )
            );
        }
    }
}
