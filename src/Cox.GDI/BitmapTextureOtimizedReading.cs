using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Cox.GDI
{
    internal unsafe sealed class BitmapTextureOtimizedReading : ITexture, IDisposable
    {
        private readonly byte* _data;
        private readonly PixelColor* _palette;
        private readonly uint _paletteSize;
        private readonly int _dataSize;
        private readonly uint _pixelSize;
        private readonly PixelFormat _pixelFormat;
        private bool _alloced;
        private bool _paletteAlloced;
        public uint Width { get; }

        public uint Height { get; }

        public ReadOnlySpan<byte> AsRaw => new ReadOnlySpan<byte>(_data, _dataSize);

        public ITexture AsOptimizedReading => this;

        public uint FourCC => throw new NotImplementedException();

        public PixelColor this[uint x, uint y] => ReadColor(x, y);

        public BitmapTextureOtimizedReading(BitmapTexture texture)
        {
            Bitmap bitmap = texture.GetBitmap();
            _pixelFormat = bitmap.PixelFormat;
            _pixelSize = GetPixelSize(bitmap.PixelFormat);
            _alloced = false;

            Width = texture.Width;
            Height = texture.Height;

            BitmapData bmpData = default;
            try
            {
                bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                ImageLockMode.ReadOnly,
                                                bitmap.PixelFormat);

                _dataSize = bmpData.Stride * bmpData.Height;
                _data = (byte*)Marshal.AllocHGlobal(_dataSize).ToPointer();
                _alloced = true;
                IntPtr ptr = bmpData.Scan0;
                Buffer.MemoryCopy(bmpData.Scan0.ToPointer(), _data, _dataSize, _dataSize);
            }catch(Exception e)
            {
                Dispose();
                throw e;
            }
            finally
            {
                if (bitmap != null)
                    bitmap.UnlockBits(bmpData);   
            }


            if(bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                try
                {
                    _palette = (PixelColor*)Marshal.AllocHGlobal(bitmap.Palette.Entries.Length * sizeof(PixelColor)).ToPointer();
                    _paletteAlloced = true;
                    uint i = 0;
                    foreach(var color in bitmap.Palette.Entries)
                        _palette[i++] = new PixelColor(new Vector4(color.R, color.G, color.B, color.A) * (1f / byte.MaxValue));
                }
                catch (Exception e)
                {
                    Dispose();
                    throw e;
                }
            }
        }

        ~BitmapTextureOtimizedReading()
        {
            Dispose();
        }

        private PixelColor ReadColor(uint x, uint y)
        {
            uint position = (x + y * Width) * _pixelSize;
            //
            // B = 0
            // G = 1
            // R = 2
            // A = 3
            //
            switch (_pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return new PixelColor(new Vector4(_data[position + 2], _data[position + 1], _data[position], _data[position + 3]) * (1f / byte.MaxValue));
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    return new PixelColor(new Vector4(_data[position + 2], _data[position + 1], _data[position], byte.MaxValue) * (1f / byte.MaxValue));
                case PixelFormat.Format8bppIndexed:
                    return _palette[_data[position]];
                case PixelFormat.Format16bppGrayScale:
                    {
                        float color = *(ushort*)(_data + position) * (1f / ushort.MaxValue);
                        return new PixelColor(new Vector4(color, color, color, 1f));
                    }
                case PixelFormat.Format16bppArgb1555:
                    {
                        ushort value = *(ushort*)(_data + position);
                        return new PixelColor(
                            new Vector4(
                                new Vector3(value & 31, (value >> 5) & 31, (value >> 10) & 31) * (1f / 31f), (value >> 15) & 1));
                    }
                case PixelFormat.Format16bppRgb555:
                    {
                        ushort value = *(ushort*)(_data + position);
                        return new PixelColor(
                            new Vector4(
                                new Vector3(value & 31, (value >> 5) & 31, (value >> 10) & 31) * (1f / 31f), 1f));
                    }
                case PixelFormat.Format16bppRgb565:
                    {
                        ushort value = *(ushort*)(_data + position);
                        return new PixelColor(
                            new Vector4((value & 31) * (1f / 31f), ((value >> 5) & 63) * (1f / 63f), ((value >> 11) & 31) * (1f / 31f), 1f));
                    }
                case PixelFormat.Format48bppRgb:
                    return new PixelColor(new Vector4(
                        *(ushort*)(_data + position + 2), 
                        *(ushort*)(_data + position + 2), 
                        *(ushort*)(_data + position), 
                        ushort.MaxValue) * (1f / ushort.MaxValue));

                case PixelFormat.Format64bppPArgb:
                case PixelFormat.Format64bppArgb:
                    return new PixelColor(new Vector4(
                        *(ushort*)(_data + position + 4),
                        *(ushort*)(_data + position + 2),
                        *(ushort*)(_data + position),
                        *(ushort*)(_data + position + 6)) * (1f / ushort.MaxValue));
                default:
                    throw new Exception($"Unsuported bitmap pixel format: {_pixelFormat}");
            }
        }

        private static uint GetPixelSize(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppPArgb:
                    return 4;
                case PixelFormat.Format8bppIndexed:   
                    return 1;
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 2;
                case PixelFormat.Format48bppRgb:
                    return 6;
                case PixelFormat.Format64bppPArgb:
                case PixelFormat.Format64bppArgb:
                    return 8;
                default:
                    throw new Exception($"Unsuported bitmap pixel format: {pixelFormat}");
            }
        }

        public ITexture Slice(Rectangle rectangle) => new SimpleSlicedTexture(this, rectangle);

        public void Dispose()
        {
            if (_alloced)
            {
                Marshal.FreeHGlobal((IntPtr)_data);
                _alloced = false;
            }

            if (_paletteAlloced)
            {
                Marshal.FreeHGlobal((IntPtr)_palette);
                _paletteAlloced = false;
            }
        }
    }
}
