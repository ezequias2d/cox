using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DXT.BC
{
    public unsafe struct BC4
    {
        public byte Color0;
        public byte Color1;

        public fixed byte Palette[6];

        public float this[byte x, byte y]
        {
            get
            {
                byte index;
                fixed (void* pPalette = Palette)
                    index = DXTHelper.UnpackColorPallet(new Span<byte>(pPalette, 6), x + (y * 4), 3);

                Span<float> paletteColors = stackalloc float[8];
                DXTHelper.GetColorPalette(Color0, Color1, paletteColors);

                return paletteColors[index];
            }
        }
    }
}
