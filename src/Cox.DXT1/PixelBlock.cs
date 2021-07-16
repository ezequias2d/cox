using Cox.DXT;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DXT1
{
    internal unsafe struct PixelBlock
    {
        private const byte PalletSize = 16;
        private fixed float Colors[4 * 4];
        private fixed byte Palette[PalletSize];

        public PixelBlock(RawPixelBlock rawPixelBlock)
        {
            PixelColor color0 = DXTHelper.UnpackColor565(rawPixelBlock.Color0);
            PixelColor color1 = DXTHelper.UnpackColor565(rawPixelBlock.Color1);

            SetColor(color0, 0);
            SetColor(color1, 1);
            (PixelColor color2, PixelColor color3) = DXTHelper.CalculateColors(rawPixelBlock.Color0, rawPixelBlock.Color1);

            SetColor(color2, 2);
            SetColor(color3, 3);

            Span<byte> packedPallet = stackalloc byte[sizeof(uint)];
            fixed (void* packedPtr = packedPallet)
                *(uint*)packedPtr = rawPixelBlock.Palette;

            fixed (void* palette = Palette)
                DXTHelper.UnpackColorPallet(packedPallet, new Span<byte>(palette, PalletSize), 2);
        }

        private void SetColor(in PixelColor color, byte index)
        {
            fixed (void* ptr = Colors)
            {
                *(PixelColor*)((byte*)ptr + (index * sizeof(PixelColor))) = color;
            }
        }

        private PixelColor GetColor(byte index)
        {
            fixed(void* ptr = Colors)
            {
                return *(PixelColor*)((byte*)ptr + (index * sizeof(PixelColor)));
            }
        }

        public PixelColor this[byte x, byte y]
        {
            get => GetColor(Palette[x + (y * 4)]);
        }
    }
}
