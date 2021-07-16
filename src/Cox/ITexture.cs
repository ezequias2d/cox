using System;
using System.Drawing;

namespace Cox
{
    public interface ITexture
    {
        uint Width { get; }
        uint Height { get; }

        PixelColor this[uint x, uint y] { get; }

        ReadOnlySpan<byte> AsRaw { get; }

        uint FourCC { get; }

        ITexture AsOptimizedReading { get; }

        ITexture Slice(Rectangle rectangle);
    }
}
