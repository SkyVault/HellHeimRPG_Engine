using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using Buffer = OpenTK.Graphics.OpenGL4.Buffer;

namespace HellHeimRPG {
    class FrameBuffer
    {
        public int ColorBuffer { get; set; } = 0;
        public int Fbo { get; set; } = 0;
        public int Rbo { get; set; } = 0;

        ~FrameBuffer() {
            if (Fbo > 0) GL.DeleteFramebuffer(Fbo);
            if (ColorBuffer > 0) GL.DeleteTexture(ColorBuffer);
        }

        public FrameBuffer(int width, int height) { 
            Fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

            ColorBuffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero); 

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorBuffer, 0);

            Rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, Rbo);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind(Action action) {
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

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float)*vertices.Length, vertices, BufferUsageHint.StaticDraw);
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
        }

        private static Art _instance = null;

        public static Art It { get {
                if (_instance == null) { _instance = new Art(); }
                return _instance;
        }}

        private Dictionary<string, FrameBuffer> frameBuffers = new Dictionary<string, FrameBuffer>();

        public FrameBuffer GetFbo(string id) => frameBuffers[id];

        public IEnumerable<(string, FrameBuffer)> Fbos() {
            foreach (var (key, value) in frameBuffers) {
                yield return (key, value);
            }
        }

        public FrameBuffer CreateFbo(int width, int height, string id)
        {
            var fbo = new FrameBuffer(width, height);
            frameBuffers[id] = fbo;
            return fbo;
        }

        public void BindVao(int vao, Action func) {
            GL.BindVertexArray(vao);
            func();
            GL.BindVertexArray(0);
        }

        public void BindBuffer(int vbo, Action func) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            func();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void RenderToScreen(FrameBuffer buffer) {
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

        public Model GenTriangle(float size = 1) {
            return new Model(
                new Mesh(
                    new float[]{ 
                      -size, -size, 0.0f,
                       size, -size, 0.0f,
                       0.0f,  size, 0.0f, 
                    },
                    new float[]{ },
                    new float[]{ }
                )
            );
        }
    }
}
