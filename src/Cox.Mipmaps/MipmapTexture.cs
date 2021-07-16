using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cox.Mipmaps
{
    public sealed unsafe class MipmapTexture : ITexture, IDisposable
    {
        private readonly PixelColor* pixelColor;
        private readonly int size;
        private bool disposed;
        public MipmapTexture(ITexture texture, int mipmapLevel, int threads = 0)
        {
            disposed = false;
            Width = Math.Max(texture.Width >> mipmapLevel, 1);
            Height = Math.Max(texture.Height >> mipmapLevel, 1);
            size = (int)(sizeof(PixelColor) * Width * Height);
            pixelColor = (PixelColor*)Marshal.AllocHGlobal(size).ToPointer();

            int unit = 1 << mipmapLevel;
            Helper.RunParallelTextureProcessing(texture, new Size(unit, unit), threads, 
                (tile, location) => 
                {
                    for(uint i = 0; i < tile.Width; i += (uint)unit)
                        for (uint j = 0; j < tile.Height; j += (uint)unit)
                            pixelColor[((location.X + i) >> mipmapLevel) + ((location.Y + j) >> mipmapLevel) * Width] = SampleMean(
                                tile,
                                new Rectangle(
                                    (int)i,
                                    (int)j, 
                                    (int)Math.Min(unit, tile.Width - i), 
                                    (int)Math.Min(unit, tile.Height - j)));
                });
        }

        public PixelColor this[uint x, uint y] => pixelColor[x + y * Width];

        public uint Width { get; }

        public uint Height { get; }

        public ReadOnlySpan<byte> AsRaw => new Span<byte>(pixelColor, size);

        public ITexture AsOptimizedReading => this;

        public uint FourCC => throw new NotImplementedException();

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Marshal.FreeHGlobal((IntPtr)pixelColor);
            }
        }

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);

        private static PixelColor SampleMean(ITexture texture, Rectangle rectangle)
        {
            PixelColor color = new PixelColor(Vector4.Zero);
            float div = 1f / (rectangle.Width * rectangle.Height);
            
            for (uint i = (uint)rectangle.X; i < rectangle.Right; i++)
                for (uint j = (uint)rectangle.Y; j < rectangle.Bottom; j++)
                    color += texture[i, j] * div;

            return color;
        }
    }
}
