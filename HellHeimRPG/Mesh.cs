namespace HellHeimRPG
{
    class Mesh {
        float[] _vertices; 
        float[] _normals;
        float[] _uvs;

        public float[] Vertices { get => _vertices; }
        public float[] Normals { get => _normals; }
        public float[] Uvs { get => _uvs; }

        public Mesh(float[] vertices, float[] normals, float[] uvs) {
            this._vertices = vertices;
            this._normals = normals;
            this._uvs = uvs;
        }
    }
}