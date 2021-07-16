using Cox.DXT;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cox.DXT1
{
    internal unsafe struct RawPixelBlock
    {
        public ushort Color0;
        public ushort Color1;
        public uint Palette;

        public static RawPixelBlock Encode(ReadOnlySpan<PixelColor> colors, IColorPicker picker, uint width, uint height)
        {
            PixelColor[] blockColors = new PixelColor[4];
            (blockColors[0], blockColors[1]) = picker.SelectColors(colors);

            ushort color0Raw = DXTHelper.PackColor565(blockColors[0]);
            ushort color1Raw = DXTHelper.PackColor565(blockColors[1]);

            bool hasAlpha = HasAlpha(colors);
            // if color0Raw <= color1Raw, then has alfa.
            if ((!hasAlpha && color0Raw <= color1Raw) || (hasAlpha && color0Raw > color1Raw))
            {
                Swap(ref color0Raw, ref color1Raw);
                Swap(ref blockColors[0], ref blockColors[1]);
            }

            hasAlpha = color0Raw <= color1Raw;

            (blockColors[2], blockColors[3]) = DXTHelper.CalculateColors(color0Raw, color1Raw);

            byte[] pallete = DXTHelper.GeneratePallet(colors, blockColors, color0Raw, color1Raw);

            return new RawPixelBlock
            {
                Color0 = color0Raw,
                Color1 = color1Raw,
                Palette = DXTHelper.PackColorPallet2(pallete, width, height)
            };
        }

        private static bool HasAlpha(ReadOnlySpan<PixelColor> colors)
        {
            foreach(PixelColor color in colors)
            {
                if (color.A < 1f)
                    return true;
            }
            return false;
        }        

        private static void Swap<T>(ref T t1, ref T t2)
        {
            T aux = t1;
            t1 = t2;
            t2 = aux;
        }
    }
}
