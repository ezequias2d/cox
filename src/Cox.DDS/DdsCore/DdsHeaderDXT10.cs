using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DDS
{
    internal struct DdsHeaderDXT10
    {
        /// <summary>
        /// The surface pixel format.
        /// </summary>
        public DxgiFormat DxgiFormat;
        /// <summary>
        /// Identifies the type of resource.
        /// </summary>
        public D3d10ResourceDimension ResourceDimension;
        /// <summary>
        /// Identifies other, less common options for resources.
        /// </summary>
        public uint MiscFlag;
        /// <summary>
        /// The number of elements in the array.
        /// 
        /// For a 2D texture that is also a cube-map texture, this number represents 
        /// the number of cubes. This number is the same as the number in the NumCubes 
        /// member of D3D10_TEXCUBE_ARRAY_SRV1 or D3D11_TEXCUBE_ARRAY_SRV). In this case, 
        /// the DDS file contains arraySize*6 2D textures.
        /// 
        /// For a 3D texture, you must set this number to 1.
        /// </summary>
        public uint ArraySize;

        /// <summary>
        /// Contains additional metadata (formerly was reserved). The lower 3 bits indicate
        /// the alpha mode of the associated resource. The upper 29 bits are reserved and 
        /// are typically 0.
        /// </summary>
        public uint MiscFlags2;

    }
}
