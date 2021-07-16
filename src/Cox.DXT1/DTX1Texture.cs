using Cox.DXT;
using System;
using System.Drawing;
using System.Threading;

namespace Cox.DXT1
{
    public class DTX1Texture : ITexture
    {
        private readonly RawPixelBlock[,] _blocks;

        public DTX1Texture(ReadOnlySpan<byte> raw, uint width, uint height)
        {
            Width = width;
            Height = height;

            unsafe
            {
                _blocks = new RawPixelBlock[Width >> 2, Height >> 2];

                fixed (void* src = raw, dst = _blocks)
                    Buffer.MemoryCopy(src, dst, raw.Length, raw.Length);
            }
        }

        public DTX1Texture(ITexture texture, IColorPicker picker)
        {
            Width = texture.Width;
            Height = texture.Height;

            ITexture opTexture = texture.AsOptimizedReading;
            _blocks = new RawPixelBlock[(int)Math.Ceiling(Width / 4f), (int)Math.Ceiling(Height / 4f)];

            
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            for (uint i = 0; i < Width; i += 4)
                for (uint j = 0; j < Height; j += 4)
                {
                    uint posX = i;
                    uint posY = j;

                    ThreadPool.QueueUserWorkItem((obj) =>
                    {
                        uint uMax = Math.Min(4, Width - posX);
                        uint vMax = Math.Min(4, Height - posY);
                        Span<PixelColor> colors = stackalloc PixelColor[(int)(uMax * vMax)];

                        for (int u = 0; u < uMax; u++)
                            for (int v = 0; v < vMax; v++)
                                colors[u + v * (int)uMax] = opTexture[posX + (uint)u, posY + (uint)v];

                        _blocks[posX >> 2, posY >> 2] = RawPixelBlock.Encode(colors, picker, uMax, vMax);
                        semaphore.Release(1);
                    });
                }

            for (uint i = 0; i < Width; i += 4)
                for (uint j = 0; j < Height; j += 4)
                    semaphore.Wait();
        }

        public PixelColor this[uint x, uint y]
        {
            get
            {
                return new PixelBlock(_blocks[x >> 2, y >> 2])[(byte)(x & 3), (byte)(y & 3)];
            }
        }

        public uint Width { get; }

        public uint Height { get; }

        public ReadOnlySpan<byte> AsRaw
        {
            get
            {
                unsafe
                {
                    byte[] raw = new byte[_blocks.GetLongLength(0) * _blocks.GetLongLength(1) * sizeof(RawPixelBlock)];

                    fixed(void* dst = raw, src = _blocks)
                        Buffer.MemoryCopy(src, dst, raw.Length, raw.Length);

                    return raw;
                }
            }
        }

        public ITexture AsOptimizedReading => new DTX1OptimizedReadingTexture(_blocks, Width, Height);

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);
    }
}
