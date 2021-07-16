using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cox.DXT.BC
{
    public struct BC1
    {
        public ushort Color0;
        public ushort Color1;
        public uint Palette;

        public PixelColor this[byte x, byte y]
        {
            get 
            {
                return DXTHelper.CalculateColor(
                    Color0, 
                    Color1, 
                    DXTHelper.UnpackColorPallet(Palette, x + (y * 4), 2));
            } 
        }
    }
}
