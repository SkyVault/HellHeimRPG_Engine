using OpenTK.Graphics;

namespace HellHeimRPG
{
    class Material {
        public Texture Texture { get; set; } = null;
        public Color4 Diffuse { get; set; } = Color4.White;
        public float Specular { get; set; } = 0.5f;

        public bool HasTexture { get => Texture != null; }
    }
}