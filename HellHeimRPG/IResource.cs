using System;
using System.Collections.Generic;
using System.Text;

namespace HellHeimRPG {
    public interface IResource
    {
        public void Load(string path);
        public void Load(params string[] path);
        public void Bind(Action action);
    }
}
