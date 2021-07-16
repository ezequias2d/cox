using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cox.DDS
{
    public sealed class DdsFile
    {
        public DdsFile(Stream stream)
        {
            DdsFileRaw raw = default;
            
            unsafe
            {
                byte[] buffer = new byte[sizeof(DdsFileRaw)];
                stream.ReadAsync(buffer, 0, buffer.Length);
                fixed (void* pBuffer = buffer)
                    raw = *(DdsFileRaw*)pBuffer;
            }


        }
    }
}
