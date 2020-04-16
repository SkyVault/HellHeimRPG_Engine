using System.Collections;
using System.Collections.Generic;
using OpenTK;

namespace HellHeimRPG
{
    public class Mesh {
        float[] _vertices; 
        float[] _normals;
        float[] _uvs;
        uint[] _indices;

        public float[] Vertices { get => _vertices; }
        public float[] Normals { get => _normals; }
        public float[] Uvs { get => _uvs; }

        public uint[] Indices { get => _indices; set => _indices = value; }

        public Mesh(float[] vertices, float[] normals, float[] uvs) {
            this._vertices = vertices;
            this._normals = normals;
            this._uvs = uvs;
        }

        public IEnumerable<(Vector3, Vector3, Vector3)> Triangles()
        {
            for (int i = 0; i < Vertices.Length; i += 9) {
                Vector3 a = new Vector3(Vertices[i + 0], Vertices[i + 1], Vertices[i + 2]);
                Vector3 b = new Vector3(Vertices[i + 3], Vertices[i + 4], Vertices[i + 5]);
                Vector3 c = new Vector3(Vertices[i + 6], Vertices[i + 7], Vertices[i + 8]);
                yield return (a, b, c);
            }
        }
    }
}