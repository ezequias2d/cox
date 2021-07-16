using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DXT.BC
{
    public unsafe struct BC1OptimizedReading
    {
        private const byte PaletteSize = 16;
        private readonly PixelColor Color0;
        private readonly PixelColor Color1;
        private readonly PixelColor Color2;
        private readonly PixelColor Color3;
        private fixed byte Palette[PaletteSize];

        public BC1OptimizedReading(BC1 bc)
        {
            Color0 = Color1 = Color2 = Color3 = default;
            fixed(PixelColor* pColor = &Color0)
                DXTHelper.GetColorPalette(bc.Color0, bc.Color1, new Span<PixelColor>(pColor, 4));

            fixed(void* palettePtr = Palette)
                DXTHelper.UnpackColorPallet(bc.Palette, new Span<byte>(palettePtr, PaletteSize), 2);
        }

        public PixelColor this[byte x, byte y] 
        {
            get
            {
                fixed(PixelColor* colors = &Color0)
                {
                    return colors[Palette[x + (y * 4)]];
                }
            }
        }
    }
}
