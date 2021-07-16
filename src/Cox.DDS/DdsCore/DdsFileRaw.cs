using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DDS
{
    internal struct DdsFileRaw
    {
        /// <summary>
        /// A magic number containing the four character code value 'DDS ' (0x20534444).
        /// </summary>
        public uint Magic;

        /// <summary>
        /// A description of the data in the file.
        /// </summary>
        public DdsHeader Header;
    }
}
