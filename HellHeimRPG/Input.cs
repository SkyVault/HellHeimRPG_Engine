using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace HellHeimRPG {
    class Input {
        private Input() { }
        private static Input instance = null;

        public static Input It { get {
            if (instance == null){ instance = new Input(); }
            return instance;
        } }

        Dictionary<Key, (bool Now, bool Last)> keyStates = new Dictionary<Key, (bool Last, bool Now)>();

        public (int, int) MousePosition {
            get {
                MouseState state = Mouse.GetCursorState();
                return (state.X, state.Y);
            }
        }

        public bool IsKeyPressed(Key key) {
            KeyboardState state = Keyboard.GetState();
            var now = state.IsKeyDown(key);

            if (keyStates.ContainsKey(key)) {
                keyStates[key] = (now, keyStates[key].Last);
            } else {
                keyStates[key] = (now, false);
            }

            return keyStates[key].Now && !keyStates[key].Last;
        }

        public void Update() {
            Key[] keys = new Key[keyStates.Keys.Count];
            keyStates.Keys.CopyTo(keys, 0);

            foreach (var key in keys) {
                var state = keyStates[key];
                keyStates[key] = (false, state.Now);
            }

        }

        public bool Sprint {
            get {
                KeyboardState state = Keyboard.GetState();
                return (state.IsKeyDown(Key.LShift));
            }
        }

        public bool MoveLeft {
            get {
                KeyboardState state = Keyboard.GetState(); 
                return (state.IsKeyDown(Key.A));
            }
        }

        public bool MoveForward {
            get {
                KeyboardState state = Keyboard.GetState(); 
                return (state.IsKeyDown(Key.W));
            }
        }

        public bool MoveBackward {
            get {
                KeyboardState state = Keyboard.GetState(); 
                return (state.IsKeyDown(Key.S));
            }
        }

        public bool MoveRight {
            get {
                KeyboardState state = Keyboard.GetState(); 
                return (state.IsKeyDown(Key.D));
            }
        }
    }
}
