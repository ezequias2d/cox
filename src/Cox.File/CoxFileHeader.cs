using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.File
{
    internal struct CoxFileHeader
    {
        /// <summary>
        /// Width of textures.
        /// </summary>
        public uint Width;
        /// <summary>
        /// Height of textures.
        /// </summary>
        public uint Height;
        /// <summary>
        /// Depth of textures or number of layers.
        /// </summary>
        public uint Depth;
        /// <summary>
        /// Number of mipmaps per texture.
        /// </summary>
        public uint MipmapCount;
    }
}
