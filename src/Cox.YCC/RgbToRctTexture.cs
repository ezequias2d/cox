using System;
using System.Drawing;
using System.Numerics;

namespace Cox.YCC
{
    public sealed class RgbToRctTexture : ITexture
    {
        private readonly ITexture _texture;

        public RgbToRctTexture(ITexture texture)
        {
            _texture = texture;
        }

        public PixelColor this[uint x, uint y]
        {
            get
            {
                Vector4 color4 = _texture[x, y].Color;
                Vector3 color3 = new Vector3(
                    //Cr
                    (color4.X - color4.Y + 1f) * 0.5f,
                    //Y
                    (color4.X + color4.Y * 2f + color4.Z) * (1f / 4f),
                    //Cb
                    (color4.Z - color4.Y + 1f) * 0.5f); ;
                return new PixelColor(color3, color4.W);
            }
        }

        public uint Width => _texture.Width;

        public uint Height => _texture.Height;

        public ReadOnlySpan<byte> AsRaw => throw new NotImplementedException();

        public uint FourCC => throw new NotImplementedException();

        public ITexture AsOptimizedReading => new RgbToRctTexture(_texture.AsOptimizedReading);

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);
    }
}
