using System;

namespace Cox.DDS
{
    internal unsafe struct DdsHeader
    {
        public const uint DefaultSize = 124;
        /// <summary>
        /// Size of structure. This member must be set to 124.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Flags to indicate which members contain valid data.
        /// </summary>
        public DdsFlags Flags;
        /// <summary>
        /// Surface height (in pixels).
        /// </summary>
        public uint Height;
        /// <summary>
        /// Surface width (in pixels).
        /// </summary>
        public uint Width;
        /// <summary>
        /// The pitch or number of bytes per scan line in an uncompressed texture; 
        /// the total number of bytes in the top level texture for a compressed texture.
        /// </summary>
        public uint PitchOrLinearSize;
        /// <summary>
        /// Depth of a volume texture (in pixels), otherwise unused.
        /// </summary>
        public uint Depth;
        /// <summary>
        /// Number of mipmap levels, otherwise unused.
        /// </summary>
        public uint MipmapCount;
        #region unused
        /// <summary>
        /// Unused.
        /// </summary>
        public fixed uint Reserved1[11];
        #endregion unused
        /// <summary>
        /// The pixel format (see DDS_PIXELFORMAT).
        /// </summary>
        public DdsPixelFormat PixelFormat;
        /// <summary>
        /// Specifies the complexity of the surfaces stored.
        /// </summary>
        public Caps Caps;
        /// <summary>
        /// Additional detail about the surfaces stored.
        /// </summary>
        public Caps2 Caps2;

        #region unused
        /// <summary>
        /// Unused.
        /// </summary>
        public uint Caps3;
        /// <summary>
        /// Unused.
        /// </summary>
        public uint Caps4;
        /// <summary>
        /// Unused.
        /// </summary>
        public uint Reserved2;
        #endregion

        public bool IsDxt10 => PixelFormat.Flags.HasFlag(PixelFormatFlags.FourCC) && 
            PixelFormat.FourCC == DdsPixelFormat.Dx10FourCc;

        public override string ToString() =>
            $"Size: {Size}, Flags: {Flags}, Height: {Height}, Width: {Width}, PitchOrLinearSize: {PitchOrLinearSize}, " +
            $"Depth: {Depth}, MipmapCount: {MipmapCount}, PixelFormat: {{{PixelFormat}}}, Caps: {Caps}, Caps2: {Caps2}, " +
            $"Caps3: {Caps3}, Caps4: {Caps4}";
    }
}
