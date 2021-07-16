using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DDS
{
    [Flags]
    internal enum Caps2
    {
        /// <summary>
        /// Required for a cube map.
        /// </summary>
        Cubemap = 0x200,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        PositiveX = 0x400,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        NegativeX = 0x800,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        PositiveY = 0x1000,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        NegativeY = 0x2000,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        PositiveZ = 0x4000,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        NegativeZ = 0x8000,
        /// <summary>
        /// Required for a volume texture.
        /// </summary>
        Volume = 0x200000,

        CubemapPositiveX = Cubemap | PositiveX,
        CubemapNegativeX = Cubemap | NegativeX,
        CubemapPositiveY = Cubemap | PositiveY,
        CubemapNegativeY = Cubemap | NegativeY,
        CubemapPositiveZ = Cubemap | PositiveZ,
        CubemapNegativeZ = Cubemap | NegativeZ,

        CubemapAllFaces = CubemapPositiveX | CubemapNegativeX |
            CubemapPositiveY | CubemapNegativeY |
            CubemapPositiveZ | CubemapNegativeZ,

        FlagsVolume = Volume
    }
}
