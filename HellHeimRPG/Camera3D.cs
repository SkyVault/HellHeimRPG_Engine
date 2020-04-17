using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellHeimRPG {
    class Camera3D {
        public Quaternion Orientation = Quaternion.Identity;
        public Vector3 Translation = new Vector3(0, 0, 10);

        public Vector3 Front = new Vector3(0, 0, -1);
        public Vector3 Up { get => new Vector3(0, 1, 0); }

        public float Sensitivity { get; set; } = 0.002f;

        (int, int) _lastMousePos = Input.It.MousePosition;

        float _pitch = 0;
        float _yaw = -MathHelper.PiOver2;

        bool _locked = true;

        public float Pitch {
            get => MathHelper.RadiansToDegrees(_pitch);
            set {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors(); 
            }
        }

        void UpdateVectors() {
            Front.X = (float)Math.Cos(_pitch) * (float)Math.Cos(_yaw);
            Front.Y = (float)Math.Sin(_pitch);
            Front.Z = (float)Math.Cos(_pitch) * (float)Math.Sin(_yaw);

            Front = Vector3.Normalize(Front); 
        }

        public void Update() {
            KeyboardState state = Keyboard.GetState();
            if (Input.It.IsKeyPressed(Key.Tab)) {
                Console.WriteLine("HELLO");
                _locked = !_locked;
            }

            if (!_locked) return;

            var mouse = Input.It.MousePosition;

            float deltaX = mouse.Item1 - _lastMousePos.Item1;
            float deltaY = mouse.Item2 - _lastMousePos.Item2;

            _yaw += deltaX * Sensitivity;
            _pitch -= deltaY * Sensitivity;

            Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);

            UpdateVectors(); 

            Mouse.SetPosition(Game.WindowSize.Item1 / 2, Game.WindowSize.Item2 / 2);
            _lastMousePos = Input.It.MousePosition;

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

        public Matrix4 ViewMatrixStatic {
            get {
                return Matrix4.LookAt(
                    Vector3.Zero, 
                    Front,
                    new Vector3(0, 1, 0)
                );
            }
        }
    }
}
