using Cox.DXT.BC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Cox.DXT
{
    public class ATI1Texture : ITexture
    {
        public static readonly uint DefaultFourCC = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("ATI1"), 0);
        private readonly BC4[] _blocks;
        private readonly uint _blockLine;

        public ATI1Texture(ReadOnlySpan<byte> raw, uint width, uint height)
        {
            Width = width;
            Height = height;

            _blockLine = (uint)Math.Ceiling(Width / 4f);
            _blocks = new BC4[_blockLine * (int)Math.Ceiling(Height / 4f)];
            Helper.Copy(raw, (Span<BC4>)(_blocks));
        }

        public ATI1Texture(ITexture texture, IColorPicker colorPicker, IPalettePicker palletPicker, int threads = 0)
        {
            Width = texture.Width;
            Height = texture.Height;

            _blockLine = (uint)Math.Ceiling(Width / 4f);
            texture = texture.AsOptimizedReading;
            _blocks = new BC4[_blockLine * (int)Math.Ceiling(Height / 4f)];

            Helper.RunParallelTextureProcessing(texture, new Size(4, 4), threads, 
                (tile, location)=> 
                {
                    Span<float> buffer = stackalloc float[16];

                    for (uint x = 0; x < tile.Width; x += 4)
                        for (uint y = 0; y < tile.Height; y += 4)
                        {
                            uint uMax = Math.Min(4, tile.Width - x);
                            uint vMax = Math.Min(4, tile.Height - y);

                            for (uint u = 0; u < uMax; u++)
                                for (uint v = 0; v < vMax; v++)
                                    buffer[(int)(u + v * uMax)] = tile[x + u, y + v].BW;

                            _blocks[((location.X + x) >> 2) + ((location.Y + y) >> 2) * _blockLine] =
                            DXTHelper.EncodeBC4(
                                buffer.Slice(0, (int)(uMax * vMax)),
                                colorPicker,
                                palletPicker);
                        }
                });
        }

        public PixelColor this[uint x, uint y]
        {
            get
            {
                return new PixelColor(_blocks[(x >> 2) + (y >> 2) * _blockLine][(byte)(x & 3), (byte)(y & 3)], 1f);
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
                    byte[] raw = new byte[_blocks.LongLength * sizeof(BC1)];

                    fixed (void* dst = raw, src = _blocks)
                        Buffer.MemoryCopy(src, dst, raw.Length, raw.Length);

                    return raw;
                }
            }
        }

        public ITexture AsOptimizedReading => this;

        public uint FourCC => DefaultFourCC;

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);

        private sealed class ATI1TextureOptimizedReading : ITexture
        {
            private readonly BC4OptimizedReading[] _blocks;
            private readonly uint _blockLine;

            internal ATI1TextureOptimizedReading(BC4[] bcs, uint width, uint height)
            {
                _blocks = new BC4OptimizedReading[bcs.Length];
                for (uint i = 0; i < bcs.LongLength; i++)
                    _blocks[i] = new BC4OptimizedReading(bcs[i]);

                Width = width;
                Height = height;
                _blockLine = (uint)Math.Ceiling(Width / 4f);

                
            }

            public PixelColor this[uint x, uint y] =>
                // equal to: _blocks[(x / 4) + (y / 4) * _blockLine][(byte)(x & 3), (byte)(y & 3)];
                new PixelColor(_blocks[(x >> 2) + (y >> 2) * _blockLine][(byte)(x & 3), (byte)(y & 3)], 1f);

            public uint Width { get; }

            public uint Height { get; }

            public ReadOnlySpan<byte> AsRaw => throw new NotImplementedException();

            public ITexture AsOptimizedReading => this;

            public uint FourCC => FourCC;

            public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);
        }
    }

}
