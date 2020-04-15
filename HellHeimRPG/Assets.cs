using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HellHeimRPG
{
    public class Assets
    {
        private static Assets instance = null;

        public static Assets It { get { return instance ??= new Assets(); } }

        private Assets()
        {
            Console.WriteLine("here");
        }
 
        public Dictionary<string, Texture> Textures { get; } = new Dictionary<string, Texture>();
        public Dictionary<string, Mesh> Meshes { get; } = new Dictionary<string, Mesh>();

        public void Add<T>(T o, string id)
        {
            if (typeof(T) == typeof(Texture)) Textures.Add(id, (Texture) (object) o); 
            if (typeof(T) == typeof(Mesh)) Meshes.Add(id, (Mesh)(object)o);
        }

        public T Get<T>(string id)
        {
            if (typeof(T) == typeof(Texture)) return (T) (object) Textures[id]; 
            if (typeof(T) == typeof(Mesh)) return (T) (object) Meshes[id]; 
            return default(T);
        }
    }
}
