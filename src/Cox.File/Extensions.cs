using Cox.DXT;
using Cox.DXT.ColorPickers;
using Cox.DXT.PalletPickers;
using Cox.Mipmaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cox.File
{
    internal static class Extensions
    {
        public static uint ReadMagic(this Stream stream)
        {
            byte[] data = new byte[sizeof(uint)];
            stream.Read(data, 0, sizeof(uint));
            unsafe
            {
                fixed (void* pData = data)
                    return *(uint*)pData;
            }
        }

        public static void WriteMagic(this Stream stream, uint magic)
        {
            byte[] data = new byte[sizeof(uint)];
            unsafe
            {
                fixed (void* pData = data)
                    *(uint*)pData = magic;
            }
            stream.Write(data, 0, sizeof(uint));
        }

        public static void WriteMagic(this Stream stream, string magic)
        {
            if (magic.Length > 4)
                throw new ArgumentOutOfRangeException(nameof(magic));

            unsafe
            {
                byte[] data = Encoding.ASCII.GetBytes(magic);
                fixed (void* pData = data)
                    WriteMagic(stream, *(uint*)pData);
            }
        }

        public static void WriteNewlineEoFHeader(this Stream stream)
        {
            // new line
            stream.WriteByte(0x0D);
            stream.WriteByte(0x0A);

            // eof
            stream.WriteByte(0x03);
        }

        public static void ReadNewlineEoFHeader(this Stream stream)
        {
            // new line
            stream.ReadByte();
            stream.ReadByte();

            // eof
            stream.ReadByte();
        }

        public static T ReadStructure<T>(this Stream stream) where T : unmanaged
        {
            unsafe
            {
                byte[] data = new byte[sizeof(T)];
                stream.Read(data, 0, sizeof(T));
                fixed (void* pData = data)
                    return *(T*)pData;
            }
        }

        public static void WriteStructure<T>(this Stream stream, in T structure) where T : unmanaged
        {
            unsafe
            {
                byte[] data = new byte[sizeof(T)];
                fixed (void* pData = data)
                    *(T*)pData = structure;
                stream.Write(data, 0, sizeof(T));
            }
        }

        public static ITexture LoadTexture(this Stream stream, uint width, uint height, int mipmap)
        {
            var surfaceHeader = stream.ReadStructure<CoxSurfaceHeader>();

            width >>= mipmap;
            height >>= mipmap;

            if(surfaceHeader.FourCC == DXT1Texture.DefaultFourCC)
            {
                byte[] data = new byte[(int)Math.Floor(width * (1f / 4f)) * (int)Math.Floor(height * (1f / 4f)) * 8];
                stream.Read(data, 0, data.Length);
                return new DXT1Texture(data, width, height);
            } 
            else if(surfaceHeader.FourCC == ATI1Texture.DefaultFourCC)
            {
                byte[] data = new byte[(int)Math.Floor(width * (1f / 4f)) * (int)Math.Floor(height * (1f / 4f)) * 8];
                stream.Read(data, 0, data.Length);
                return new ATI1Texture(data, width, height);
            }
            else
            {
                throw new ArgumentException("Not supported texture FourCC.");
            }
        }

        public static void SaveTexture(this Stream stream, ITexture texture)
        {
            stream.WriteStructure(new CoxSurfaceHeader
            {
                FourCC = texture.FourCC
            });

            stream.Write(texture.AsRaw.ToArray(), 0, texture.AsRaw.Length);
        }
    }
}
