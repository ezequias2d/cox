using Cox.DXT;
using Cox.DXT.ColorPickers;
using Cox.DXT.PalletPickers;
using Cox.Mipmaps;
using System;
using System.IO;
using System.Text;

namespace Cox.File
{
    public class CoxFile
    {
        private static readonly uint Magic = BitConverter.ToUInt32(new byte[] { 0x43, 0x4f, 0x58, 0x0 }, 0);
        private readonly ITexture[] _surfaces;
        public CoxFile(Stream stream)
        {
            uint magic = stream.ReadMagic();
            if (magic != Magic)
                throw new ArgumentException("Stream does not contain a cox file data.");

            stream.ReadNewlineEoFHeader();

            var header = stream.ReadStructure<CoxFileHeader>();

            Width = header.Width;
            Height = header.Height;
            Depth = header.Depth;
            Mipmaps = header.MipmapCount;

            _surfaces = new ITexture[Depth * (Mipmaps + 1)];
            for (uint d = 0; d < Depth; d++)
                for(int m = 0; m <= Mipmaps; m++)
                {
                    this[d, m] = stream.LoadTexture(Width, Height, m);
                }
        }

        public CoxFile(uint width, uint height, uint depth, uint mipmaps, bool generateMipmaps, params ITexture[] textures)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Mipmaps = mipmaps;

            _surfaces = new ITexture[Depth * (Mipmaps + 1)];
            for (uint d = 0; d < Depth; d++)
                for (int m = 0; m <= Mipmaps; m++)
                {
                    if (generateMipmaps)
                    {
                        ITexture texture = textures[d];
                        if(m > 0)
                        {
                            MipmapTexture mipmap = new MipmapTexture(texture, m);

                            if(texture.FourCC == DXT1Texture.DefaultFourCC)
                                texture = new DXT1Texture(mipmap, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance);
                            else if(texture.FourCC == ATI1Texture.DefaultFourCC)
                                texture = new ATI1Texture(mipmap, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance);
                            else
                                throw new ArgumentException($"Not suppported mipmap for this FourCC: {Encoding.ASCII.GetString(BitConverter.GetBytes(texture.FourCC))}");
                        }
                        this[d, m] = texture;
                    }
                    else
                        this[d, m] = textures[m + d * (Mipmaps + 1)];
                }
        }

        /// <summary>
        /// The width of texture, minimum value is 1.
        /// </summary>
        public uint Width { get; }
        /// <summary>
        /// The height of texture, minimum value is 1.
        /// </summary>
        public uint Height { get; }
        /// <summary>
        /// The depth of texture, minimum value is 1.
        /// </summary>
        public uint Depth { get; }
        /// <summary>
        /// The number of mipmaps of texture, minimum value is 0.
        /// </summary>
        public uint Mipmaps { get; }

        public ITexture this[uint depth = 0, int mipmap = 0]
        {
            get => _surfaces[mipmap + depth * (Mipmaps + 1)];
            set => _surfaces[mipmap + depth * (Mipmaps + 1)] = value;
        } 

        public void Save(Stream stream)
        {
            stream.WriteMagic(Magic);
            stream.WriteNewlineEoFHeader();
            stream.WriteStructure(new CoxFileHeader
            {
                Width = Width,
                Height = Height,
                Depth = Depth,
                MipmapCount = Mipmaps
            });

            for (uint d = 0; d < Depth; d++)
                for (int m = 0; m <= Mipmaps; m++)
                    stream.SaveTexture(this[d, m]);
        }
            
    }
}
