using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG {
    class Terrain {
        // position + normal
        public int VERTEX_SIZE_BYTES { get; private set; } = 12 + 4 + 4;
        public int SIZE { get; private set; }  = 800;
        public int VERTEX_COUNT { get; private set; }  = 128;

        float[] vertices;
        float[] normals;
        float[] uvs;

        uint[] indices;

        int vao = -1;
        int vbo = -1;
        int nbo = -1;
        int tbo = -1;
        int ibo = -1;

        public Terrain() {
            int count = VERTEX_COUNT * VERTEX_COUNT;
            vertices = new float[count * 3];
            normals = new float[count * 3];
            uvs = new float[count * 2];

            indices = new uint[6 * (VERTEX_COUNT - 1) * (VERTEX_COUNT - 1)];

            int vertex_pointer = 0;

            for (int i = 0; i < VERTEX_COUNT; i++) {
                for (int j = 0; j < VERTEX_COUNT; j++) {
                    vertices[vertex_pointer * 3 + 0] = (float)j / ((float)VERTEX_COUNT - 1) * SIZE; 
                    vertices[vertex_pointer * 3 + 1] = 0; 
                    vertices[vertex_pointer * 3 + 2] = (float)i / ((float)VERTEX_COUNT - 1) * SIZE;

                    normals[vertex_pointer * 3 + 0] = 0;
                    normals[vertex_pointer * 3 + 1] = 1;
                    normals[vertex_pointer * 3 + 2] = 0;

                    uvs[vertex_pointer * 2] = (float)j / ((float)VERTEX_COUNT - 1);
                    uvs[vertex_pointer * 2 + 1] = (float)i / ((float)VERTEX_COUNT - 1);
                    vertex_pointer++; 
                }
            }

            int pointer = 0;

            for (int gz = 0; gz < VERTEX_COUNT - 1; gz++) {
                for (int gx = 0; gx < VERTEX_COUNT - 1; gx++) {
                    int topLeft = (gz * VERTEX_COUNT) + gx;
                    int topRight = topLeft + 1;
                    int bottomLeft = ((gz + 1) * VERTEX_COUNT) + gx;
                    int bottomRight = bottomLeft + 1;
                    indices[pointer++] = (uint)topLeft;
                    indices[pointer++] = (uint)bottomLeft;
                    indices[pointer++] = (uint)topRight;
                    indices[pointer++] = (uint)topRight;
                    indices[pointer++] = (uint)bottomLeft;
                    indices[pointer++] = (uint)bottomRight;
                }
            }

            vao = GL.GenVertexArray();

            GL.BindVertexArray(vao);

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.DynamicDraw); 
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            nbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, nbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * normals.Length, normals, BufferUsageHint.DynamicDraw); 
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            tbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, tbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * uvs.Length, uvs, BufferUsageHint.DynamicDraw); 
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0); 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.BindVertexArray(0);
        }

        public void Render() {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

            GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0); 
            //GL.DrawElements(BeginMode.LineLoop, indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }

    class Shader {
        int program_id = 0;
        int vs_id = 0;
        int fs_id = 0; 

        public int Id { get => program_id; }

        void compile(int id, ShaderType type) {
            GL.CompileShader(id); 
            string log = GL.GetShaderInfoLog(id);

            if (log != System.String.Empty) {
                Console.WriteLine($"{type}::ERROR:: {log}");
            }
        }

        public Shader(string vs_code, string fs_code) {
            vs_id = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs_id, vs_code);
            compile(vs_id, ShaderType.VertexShader);

            fs_id = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs_id, fs_code); 
            compile(fs_id, ShaderType.FragmentShader);

            program_id = GL.CreateProgram();
            GL.AttachShader(program_id, vs_id);
            GL.AttachShader(program_id, fs_id);
            GL.LinkProgram(program_id);

            //TODO(Dustin): Handle program linking errors

        }

        ~Shader() {
            if (program_id == 0) return;

            GL.DetachShader(program_id, fs_id);
            GL.DetachShader(program_id, vs_id);
            GL.DeleteShader(vs_id);
            GL.DeleteShader(fs_id);
        }

        public int GetLoc(string name) {
            int result = GL.GetUniformLocation(program_id, name);

            if (result < 0) {
                Console.WriteLine($"Failed to find uniform: {name}");
            }

            return result;
        }
        
        public void SetUniform(int uid, float x, float y, float z) {
            GL.Uniform3(uid, x, y, z);
        }

        public void SetUniform(int uid, float f) {
            GL.Uniform1(uid, f);
        }

        public void SetUniform(int uid, float[] f) {
            switch(f.Length) {
                case 0: { break; }// TODO(Dustin): ERROR
                case 1: { GL.Uniform1(uid, f[0]); break; }
                case 2: { GL.Uniform2(uid, f[0], f[1]); break; }
                case 3: { GL.Uniform3(uid, f[0], f[1], f[2]); break; }
                default: { GL.Uniform4(uid, f[0], f[1], f[2], f[3]); break; }
            }
        }
 
        public void SetUniform(int uid, Matrix4 matrix) {
            GL.UniformMatrix4(uid, false, ref matrix);
        }

        public void Use() => GL.UseProgram(program_id);
        public void Use(int id) => GL.UseProgram(id);
        public void UnUse() => GL.UseProgram(0);

        public void Bind(Action func) {
            GL.UseProgram(program_id); 
            func(); 
            GL.UseProgram(0);
        }
    }

    class Mesh {
        float[] vertices; 
        float[] normals;
        float[] uvs;

        public float[] Vertices { get => vertices; }
        public float[] Normals { get => normals; }
        public float[] Uvs { get => uvs; }

        public Mesh(float[] vertices, float[] normals, float[] uvs) {
            this.vertices = vertices;
            this.normals = normals;
            this.uvs = uvs;
        }
    }
    
    class Texture {
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int Mips { get; set; } = 0;
        public int Id { get; private set; } = -1;

        ~Texture() {
            if (Id > 0) GL.DeleteTexture(Id);
        }

        public void Load(string path) {
            Bitmap bitmap = new Bitmap(path);

            Id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                data.Width,
                data.Height,
                0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data); 
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Bind(Action action) {
            GL.BindTexture(TextureTarget.Texture2D, Id);
            action.Invoke();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    class Material {
        public Texture Texture { get; set; } = null;
        public Color4 Diffuse { get; set; } = Color4.White;
        public float Specular { get; set; } = 0.5f;

        public bool HasTexture { get => Texture != null; }
    }

    class Model {
        Mesh mesh = null;
        Material material = new Material();

        public Mesh Mesh { get => mesh; set => Reset(value); }
        public Material Material { get => material; set => material = value; }

        int vao = 0;
        int vbo = 0;
        int nbo = 0;
        int tbo = 0;

        public Model() { }

        public Model(Mesh mesh) {
            this.mesh = mesh;
            Load();
        }

        public void Load() { 
            vao = GL.GenVertexArray();

            Art.It.BindVao(vao, () => { 
                vbo = GL.GenBuffer();
                nbo = GL.GenBuffer();
                tbo = GL.GenBuffer();

                Art.It.BindBuffer(vbo, () => { 
                    GL.BufferData(BufferTarget.ArrayBuffer,
                                  mesh.Vertices.Length * sizeof(float),
                                  mesh.Vertices,
                                  BufferUsageHint.StaticDraw); 

                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); 
                });

                Art.It.BindBuffer(nbo, () => {
                    GL.BufferData(BufferTarget.ArrayBuffer,
                                  mesh.Normals.Length * sizeof(float),
                                  mesh.Normals,
                                  BufferUsageHint.StaticDraw);
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                });

                Art.It.BindBuffer(tbo, () => {
                    GL.BufferData(BufferTarget.ArrayBuffer,
                                  mesh.Uvs.Length * sizeof(float),
                                  mesh.Uvs,
                                  BufferUsageHint.StaticDraw);
                    GL.EnableVertexAttribArray(2);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                });
            }); 
        }

        public void Reset(Mesh mesh) { 
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            this.mesh = mesh;
            Load();
        }

        ~Model() {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
        }

        public void Bind(Action fn)
            => Art.It.BindVao(vao, fn);
    }

    class Art {
        private Art() { }

        private static Art instance = null;

        public static Art It { get {
                if (instance == null) { instance = new Art(); }
                return instance;
        }}

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
