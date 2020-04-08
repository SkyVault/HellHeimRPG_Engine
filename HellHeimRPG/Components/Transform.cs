using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellHeimRPG.Components {
    class Transform {
        public Vector3 Translation { get; set; }  = Vector3.Zero;
        public Quaternion Orientation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        public Matrix4 Matrix {
            get {
                Matrix4 m = Matrix4.Identity;
                m = Matrix4.CreateTranslation(Translation) * m;
                m = Matrix4.CreateFromQuaternion(Orientation) * m;
                m = Matrix4.CreateScale(Scale) * m;

                return m;
            }
        }
    }
}
