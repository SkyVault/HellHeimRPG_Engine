using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG
{
    class Terrain {
        // position + normal
        public int VertexSizeBytes { get; private set; } = 12 + 4 + 4;
        public int Size { get; private set; }  = 800;
        public int VertexCount { get; private set; }  = 128;

        float[] _vertices;
        float[] _normals;
        float[] _uvs;

        uint[] _indices;

        int _vao = -1;
        int _vbo = -1;
        int _nbo = -1;
        int _tbo = -1;
        int _ibo = -1;

        public Terrain() {
            int count = VertexCount * VertexCount;
            _vertices = new float[count * 3];
            _normals = new float[count * 3];
            _uvs = new float[count * 2];

            _indices = new uint[6 * (VertexCount - 1) * (VertexCount - 1)];

            int vertexPointer = 0;

            for (int i = 0; i < VertexCount; i++) {
                for (int j = 0; j < VertexCount; j++) {
                    _vertices[vertexPointer * 3 + 0] = (float)j / ((float)VertexCount - 1) * Size; 
                    _vertices[vertexPointer * 3 + 2] = 0; 
                    _vertices[vertexPointer * 3 + 1] = (float)i / ((float)VertexCount - 1) * Size;

                    _normals[vertexPointer * 3 + 0] = 0;
                    _normals[vertexPointer * 3 + 1] = 1;
                    _normals[vertexPointer * 3 + 2] = 0;

                    _uvs[vertexPointer * 2] = (float)j / ((float)VertexCount - 1);
                    _uvs[vertexPointer * 2 + 1] = (float)i / ((float)VertexCount - 1);
                    vertexPointer++; 
                }
            }

            int pointer = 0;

            for (int gz = 0; gz < VertexCount - 1; gz++) {
                for (int gx = 0; gx < VertexCount - 1; gx++) {
                    int topLeft = (gz * VertexCount) + gx;
                    int topRight = topLeft + 1;
                    int bottomLeft = ((gz + 1) * VertexCount) + gx;
                    int bottomRight = bottomLeft + 1;
                    _indices[pointer++] = (uint)topLeft;
                    _indices[pointer++] = (uint)bottomLeft;
                    _indices[pointer++] = (uint)topRight;
                    _indices[pointer++] = (uint)topRight;
                    _indices[pointer++] = (uint)bottomLeft;
                    _indices[pointer++] = (uint)bottomRight;
                }
            }

            _vao = GL.GenVertexArray();

            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _vertices.Length, _vertices, BufferUsageHint.DynamicDraw); 
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _nbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _nbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _normals.Length, _normals, BufferUsageHint.DynamicDraw); 
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _tbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _tbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _uvs.Length, _uvs, BufferUsageHint.DynamicDraw); 
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0); 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * _indices.Length, _indices, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.BindVertexArray(0);
        }

        public void Render() {
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);

            GL.DrawElements(BeginMode.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0); 
            //GL.DrawElements(BeginMode.LineLoop, _indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}