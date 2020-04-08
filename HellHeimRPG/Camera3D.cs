using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellHeimRPG {
    class Camera3D {
        public Quaternion Orientation = Quaternion.Identity;
        public Vector3 Translation = Vector3.Zero;

        public Vector3 Front = new Vector3(0, 0, -1);
        public Vector3 Up { get => new Vector3(0, 1, 0); }

        public float Sensitivity { get; set; } = 0.002f;

        (int, int) lastMousePos = Input.It.MousePosition;

        float pitch = 0;
        float yaw = -MathHelper.PiOver2;

        bool locked = true;

        public float Pitch {
            get => MathHelper.RadiansToDegrees(pitch);
            set {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors(); 
            }
        }

        void UpdateVectors() {
            Front.X = (float)Math.Cos(pitch) * (float)Math.Cos(yaw);
            Front.Y = (float)Math.Sin(pitch);
            Front.Z = (float)Math.Cos(pitch) * (float)Math.Sin(yaw);

            Front = Vector3.Normalize(Front); 
        }

        public void Update() {
            KeyboardState state = Keyboard.GetState();
            if (Input.It.IsKeyPressed(Key.Tab)) {
                Console.WriteLine("HELLO");
                locked = !locked;
            }

            if (!locked) return;

            var mouse = Input.It.MousePosition;

            float delta_x = mouse.Item1 - lastMousePos.Item1;
            float delta_y = mouse.Item2 - lastMousePos.Item2;

            yaw += delta_x * Sensitivity;
            pitch -= delta_y * Sensitivity;

            Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);

            UpdateVectors(); 

            Mouse.SetPosition(Game.WindowSize.Item1 / 2, Game.WindowSize.Item2 / 2);
            lastMousePos = Input.It.MousePosition;

            float speed = 0.0016f * 10.0f;

            if (Input.It.Sprint)
                speed *= 2.5f;

            if (Input.It.MoveForward) {
                Translation += Front * speed;
            }

            if (Input.It.MoveBackward) { 
                Translation -= Front * speed;
            }

            if (Input.It.MoveLeft) {
                Translation -= Vector3.Normalize(Vector3.Cross(Front, Up)) * speed;
            }

            if (Input.It.MoveRight) {
                Translation += Vector3.Normalize(Vector3.Cross(Front, Up)) * speed;
            } 
        }

        public Matrix4 ViewMatrix {
            get {
                return Matrix4.LookAt(
                    Translation,
                    Translation + Front,
                    new Vector3(0, 1, 0)
                );
            }
        }
    }
}
