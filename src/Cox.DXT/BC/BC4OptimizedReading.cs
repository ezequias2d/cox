using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DXT.BC
{
    public unsafe struct BC4OptimizedReading
    {
        private const byte PaletteSize = 16;

        private readonly float Color0;
        private readonly float Color1;
        private readonly float Color2;
        private readonly float Color3;
        private readonly float Color4;
        private readonly float Color5;
        private readonly float Color6;
        private readonly float Color7;

        public fixed byte Palette[16];

        public BC4OptimizedReading(BC4 bc)
        {
            Color0 = Color1 = Color2 = Color3 = Color4 = Color5 = Color6 = Color7 = default;
            fixed (float* pColor = &Color0)
                DXTHelper.GetColorPalette(bc.Color0, bc.Color1, new Span<float>(pColor, 8));

            fixed (void* palettePtr = Palette)
                DXTHelper.UnpackColorPallet(new Span<byte>(bc.Palette, 6), new Span<byte>(palettePtr, PaletteSize), 3);
        }

        public float this[byte x, byte y]
        {
            get
            {
                fixed (float* colors = &Color0)
                {
                    return colors[Palette[x + (y * 4)]];
                }
            }
        }
    }
}
