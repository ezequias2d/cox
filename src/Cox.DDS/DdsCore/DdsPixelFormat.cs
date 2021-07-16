using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DDS
{
    internal struct DdsPixelFormat
    {
        public const uint Dxt1FourCc = 0x31545844;
        public const uint Dxt2FourCc = 0x32545844;
        public const uint Dxt3FourCc = 0x33545844;
        public const uint Dxt4FourCc = 0x34545844;
        public const uint Dxt5FourCc = 0x35545844;
        public const uint Dx10FourCc = 0x30315844;

        /// <summary>
        /// Structure size; set to 32 (bytes).
        /// </summary>
        public uint Size;
        /// <summary>
        /// Values which indicate what type of data is in the surface.
        /// </summary>
        public PixelFormatFlags Flags;
        /// <summary>
        /// Four-character codes for specifying compressed or custom formats.
        /// Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5. 
        /// 
        /// A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 
        /// extended header, and the dxgiFormat member of that structure 
        /// indicates the true format. When using a four-character code, 
        /// <see cref="Flags"/> must include <see cref="PixelFormatFlags.FourCC"/>.
        /// </summary>
        public uint FourCC;
        /// <summary>
        /// Number of bits in an RGB (possibly including alpha) format. 
        /// Valid when <see cref="Flags"/> includes <see cref="PixelFormatFlags.Rgb"/>, 
        /// <see cref="PixelFormatFlags.Luminance"/>, or <see cref="PixelFormatFlags.Yuv"/>.
        /// </summary>
        public uint RgbBitCount;
        /// <summary>
        /// Red (or lumiannce or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
        /// </summary>
        public uint RBitMask;
        /// <summary>
        /// Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
        /// </summary>
        public uint GBitMask;
        /// <summary>
        /// Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
        /// </summary>
        public uint BBitMask;
        /// <summary>
        /// Alpha mask for reading alpha data. <see cref="Flags"/> must include 
        /// <see cref="PixelFormatFlags.AlphaPixels"/> or <see cref="PixelFormatFlags.Alpha"/>. 
        /// For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
        /// </summary>
        public uint ABitMask;
    }
}
