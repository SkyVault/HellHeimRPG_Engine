using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG {
    class Art {
        private Art() { }

        private static Art _instance = null;

        public static Art It { get {
                if (_instance == null) { _instance = new Art(); }
                return _instance;
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
