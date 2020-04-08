using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG
{
    class Texture {
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int Mips { get; set; } = 0;
        public int Id { get; private set; } = -1;

        ~Texture() {
            if (Id > 0) GL.DeleteTexture(Id);
        }

        public void Load(string path) {
            Bitmap bitmap = new Bitmap(path);

            Id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                data.Width,
                data.Height,
                0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data); 
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Bind(Action action) {
            GL.BindTexture(TextureTarget.Texture2D, Id);
            action.Invoke();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}