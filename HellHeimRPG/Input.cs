using OpenTK.Input;
using System.Collections.Generic;

namespace HellHeimRPG {

    internal class Input {

        private Input() {
        }

        private static Input _instance = null;

        public static Input It {
            get {
                if (_instance == null) { _instance = new Input(); }
                return _instance;
            }
        }

        private Dictionary<Key, (bool Now, bool Last)> _keyStates = new Dictionary<Key, (bool Last, bool Now)>();

        public (int, int) MousePosition {
            get {
                MouseState state = Mouse.GetCursorState();
                return (state.X, state.Y);
            }
        }

        public bool IsKeyPressed(Key key) {
            KeyboardState state = Keyboard.GetState();
            var now = state.IsKeyDown(key);

            if (_keyStates.ContainsKey(key)) {
                _keyStates[key] = (now, _keyStates[key].Last);
            } else {
                _keyStates[key] = (now, false);
            }

            return _keyStates[key].Now && !_keyStates[key].Last;
        }

        public void Update() {
            Key[] keys = new Key[_keyStates.Keys.Count];
            _keyStates.Keys.CopyTo(keys, 0);

            foreach (var key in keys) {
                var state = _keyStates[key];
                _keyStates[key] = (false, state.Now);
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

        public bool ToggleEditor => IsKeyPressed(Key.Tilde);
    }
}