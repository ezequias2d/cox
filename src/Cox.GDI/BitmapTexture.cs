using System;
using System.Drawing;
using System.Numerics;

namespace Cox.GDI
{
    public class BitmapTexture : ITexture
    {
        private readonly Bitmap _bitmap;

        public BitmapTexture(Bitmap bitmap)
        {
            _bitmap = bitmap;
            Width = (uint)bitmap.Width;
            Height = (uint)bitmap.Height;
        }

        public BitmapTexture(ITexture texture)
        {
            ITexture opTexture = texture;

            _bitmap = new Bitmap((int)texture.Width, (int)texture.Height);

            for (int i = 0; i < texture.Width; i++)
            {
                for (int j = 0; j < texture.Height; j++)
                    _bitmap.SetPixel(i, j, ToColor(opTexture[(uint)i, (uint)j]));
            }
        }

        public PixelColor this[uint x, uint y]
        {
            get
            {
                lock (_bitmap)
                {
                    return ToPixelColor(_bitmap.GetPixel((int) x, (int) y));
                }
            }
        }

        public uint Width { get; }

        public uint Height { get; }

        public ReadOnlySpan<byte> AsRaw => throw new NotImplementedException();

        public ITexture AsOptimizedReading => new BitmapTextureOtimizedReading(this);

        public uint FourCC => throw new NotImplementedException();

        public Bitmap GetBitmap() => _bitmap;

        private static PixelColor ToPixelColor(Color color)
        {
            return new PixelColor(new Vector4(color.R, color.G, color.B, color.A) * (1f / 255f));
        }
        private static Color ToColor(PixelColor pixelColor)
        {
            Vector4 color = Vector4.Clamp(pixelColor.Color, Vector4.Zero, Vector4.One);
            return Color.FromArgb((int)Math.Round(color.W * 255f), (int)Math.Round(color.X * 255f), (int)Math.Round(color.Y * 255f), (int)Math.Round(color.Z * 255f));
        }

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);
    }
}
