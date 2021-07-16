using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cox.DXT1
{
    public class DTX1OptimizedReadingTexture : ITexture
    {
        private readonly PixelBlock[,] _pixelBlocks;

        internal DTX1OptimizedReadingTexture(RawPixelBlock[,] pixelBlocks, uint width, uint height)
        {
            _pixelBlocks = new PixelBlock[pixelBlocks.GetLongLength(0), pixelBlocks.GetLongLength(1)];
            for (uint i = 0; i < pixelBlocks.GetLongLength(0); i++)
                for(uint j = 0; j < pixelBlocks.GetLongLength(1); j++)
                    _pixelBlocks[i, j] = new PixelBlock(pixelBlocks[i, j]);

            Width = width;
            Height = height;
        }

        public PixelColor this[uint x, uint y] => _pixelBlocks[x >> 2, y >> 2][(byte)(x & 3), (byte)(y & 3)];

        public uint Width { get; }

        public uint Height { get; }

        public ReadOnlySpan<byte> AsRaw => throw new NotImplementedException();

        public ITexture AsOptimizedReading => this;

        public uint FourCC => throw new NotImplementedException();

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);
    }
}
