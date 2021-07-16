using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Cox.YCC
{
    public sealed class RctToRgbTexture : ITexture
    {
        private readonly ITexture _texture;

        public RctToRgbTexture(ITexture texture)
        {
            _texture = texture;
        }

        public PixelColor this[uint x, uint y] 
        { 
            get 
            {
                Vector4 color4 = _texture[x, y].Color;
                color4.X = color4.X * 2f - 1f;
                color4.Z = color4.Z * 2f - 1f;
                float g = color4.Y - (color4.X + color4.Z) * (1f / 4f);

                return new PixelColor(new Vector3(
                    color4.X + g,
                    g,
                    color4.Z + g),
                    color4.W);
            } 
        }

        public uint Width => _texture.Width;

        public uint Height => _texture.Height;

        public ReadOnlySpan<byte> AsRaw => throw new NotImplementedException();

        public uint FourCC => throw new NotImplementedException();

        public ITexture AsOptimizedReading => new RctToRgbTexture(_texture.AsOptimizedReading);

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);
    }
}
