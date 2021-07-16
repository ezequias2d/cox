using System;
using System.Collections.Generic;
using System.Text;

namespace Cox
{
    public interface ITextureEncoder<T> where T : ITexture
    {
        T Encoder(ITexture texture);
    }
}
