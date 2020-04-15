using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK;

namespace HellHeimRPG {
    class Selectable
    {
        public bool State { get; set; } = false;
        public Vector3 Key = Vector3.One;

        public Selectable()
        {
            var rnd = new Random();
            Key.X = (float)rnd.NextDouble();
            Key.Y = (float)rnd.NextDouble();
            Key.Z = (float)rnd.NextDouble();
        }
    }
}
