using System;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG
{
    class Model {
        Mesh _mesh;

        public Mesh Mesh { get => _mesh; set => Reset(value); }
        public Material Material { get; set; } = new Material();

        int _vao = 0;
        int _vbo = 0;
        int _nbo = 0;
        int _tbo = 0;
        int _ibo = 0;

        public Model() { }

        public Model(Mesh mesh) {
            this._mesh = mesh;
            Load();
        }

        public void Load() { 
            _vao = GL.GenVertexArray();

            Art.It.BindVao(_vao, () => { 
                _vbo = GL.GenBuffer();
                _nbo = GL.GenBuffer();
                _tbo = GL.GenBuffer();
                _ibo = GL.GenBuffer();

                Art.It.BindBuffer(_vbo, () => { 
                    GL.BufferData(BufferTarget.ArrayBuffer,
                        _mesh.Vertices.Length * sizeof(float),
                        _mesh.Vertices,
                        BufferUsageHint.StaticDraw); 

                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); 
                });

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    _mesh.Indices.Length * sizeof(uint),
                    _mesh.Indices,
                    BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                Art.It.BindBuffer(_nbo, () => {
                    GL.BufferData(BufferTarget.ArrayBuffer,
                        _mesh.Normals.Length * sizeof(float),
                        _mesh.Normals,
                        BufferUsageHint.StaticDraw);
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                });

                Art.It.BindBuffer(_tbo, () => {
                    GL.BufferData(BufferTarget.ArrayBuffer,
                        _mesh.Uvs.Length * sizeof(float),
                        _mesh.Uvs,
                        BufferUsageHint.StaticDraw);
                    GL.EnableVertexAttribArray(2);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                });
            }); 
        }

        public void Reset(Mesh mesh) { 
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            this._mesh = mesh;
            Load();
        }

        ~Model() {
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
        }

        public void Bind(Action fn)
        {
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            fn();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}