using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG {
    public class Cubemap : IResource
    {
        public int Id { get; private set; } = -1;

        public void Load(string path) { throw new NotImplementedException(); }

        public void Load(params string[] faces)
        {
            // We expect the path to be a directory where the cubemap lives
            // At least for now... until I can implement a single image cubemap

            Id = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, Id);

            int width;
            int height;
            int channels;

            for (int i = 0; i < faces.Length; i++)
            {
                Assert.IsTrue(File.Exists(faces[i]));
                Bitmap bitmap = new Bitmap(faces[i]);

                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(
                    TextureTarget.TextureCubeMapPositiveX + i,
                    0,
                    PixelInternalFormat.Rgba,
                    data.Width,
                    data.Height,
                    0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data); 
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0); 
        }

        public void Bind(Action action)
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, Id);
            action();
            GL.BindTexture(TextureTarget.TextureCubeMap, 0); 
        }
    }
}
