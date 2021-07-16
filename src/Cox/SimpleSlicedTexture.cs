using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cox
{
    public sealed class SimpleSlicedTexture : ITexture
    {
        private readonly ITexture _texture;
        private readonly Rectangle _rectangle;

        public SimpleSlicedTexture(ITexture texture, Rectangle rectangle)
        {
            if (!(rectangle.X >= 0 && rectangle.X < texture.Width &&
                rectangle.Y >= 0 && rectangle.Y < texture.Height &&
                rectangle.Width >= 1 && rectangle.X + rectangle.Width - 1 < texture.Width &&
                rectangle.Height >= 1 && rectangle.Y + rectangle.Height - 1 < texture.Height))
                throw new ArgumentOutOfRangeException(nameof(rectangle));

            if(texture is SimpleSlicedTexture slicedTexture)
            {
                _texture = slicedTexture._texture;
                _rectangle = new Rectangle(
                    rectangle.Location.X + slicedTexture._rectangle.Location.X,
                    rectangle.Location.Y + slicedTexture._rectangle.Location.Y,
                    rectangle.Size.Width,
                    rectangle.Size.Height);
            }
            else
            {
                _texture = texture;
                _rectangle = rectangle;
            }
        }

        public PixelColor this[uint x, uint y]
        {
            get
            {
                if (!(x >= 0 && x < _rectangle.Width))
                    throw new ArgumentOutOfRangeException(nameof(x));
                    
                if (!(y >= 0 && y < _rectangle.Height))
                    throw new ArgumentOutOfRangeException(nameof(y));

                return _texture[x + (uint)_rectangle.X, y + (uint)_rectangle.Y];
            }
        }

        public uint Width => (uint)_rectangle.Width;

        public uint Height => (uint)_rectangle.Height;

        public ReadOnlySpan<byte> AsRaw => throw new NotImplementedException();

        public ITexture AsOptimizedReading => new SimpleSlicedTexture(_texture.AsOptimizedReading, _rectangle);

        public uint FourCC => throw new NotImplementedException();

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);
    }
}
