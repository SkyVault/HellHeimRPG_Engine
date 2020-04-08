using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Assimp;

namespace HellHeimRPG {
    class Loader {
        public static Mesh LoadMesh(string path) {
            if (!File.Exists(path)) {
                Console.WriteLine($"Resource: {path}, does not exist.");
                return new Mesh(new float[] { }, new float[] { }, new float[] { });
            }

            var rvertices = new List<float>();
            var rnormals = new List<float>();
            var ruvs = new List<float>();

            AssimpContext context = new AssimpContext();

            Scene scene = context.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality);

            if (scene.MeshCount == 0) {
                Console.WriteLine($"File {path}, doesn't contain any meshes");
                return new Mesh(new float[] { }, new float[] { }, new float[] { });
            }

            Assimp.Mesh mesh = scene.Meshes[0];

            foreach (var face in mesh.Faces) {
                foreach (int i in face.Indices) {
                    rvertices.Add(mesh.Vertices[i].X);
                    rvertices.Add(mesh.Vertices[i].Y);
                    rvertices.Add(mesh.Vertices[i].Z);

                    rnormals.Add(mesh.Normals[i].X);
                    rnormals.Add(mesh.Normals[i].Y);
                    rnormals.Add(mesh.Normals[i].Z);

                    if (mesh.TextureCoordinateChannelCount > 0) {
                        ruvs.Add(mesh.TextureCoordinateChannels[0][i].X);
                        ruvs.Add(mesh.TextureCoordinateChannels[0][i].Y);
                    }
                }
            }

            return new Mesh(rvertices.ToArray(),
                            rnormals.ToArray(),
                            ruvs.ToArray());
        }
    }
}
