using Cox.DXT.BC;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Cox.DXT
{
    public static class DXTHelper
    {
        public static BC1 EncodeBC1(ReadOnlySpan<PixelColor> colors, IColorPicker colorPicker, IPalettePicker palletPicker)
        {
            Span<PixelColor> paletteColors = stackalloc PixelColor[4];
            (paletteColors[0], paletteColors[1]) = colorPicker.SelectColors(colors, palletPicker);

            ushort color0Raw = PackColor565(paletteColors[0]);
            ushort color1Raw = PackColor565(paletteColors[1]);

            bool hasAlpha = HasAlpha(colors);

            // if color0Raw <= color1Raw, then has alfa.
            if ((!hasAlpha && color0Raw <= color1Raw) || (hasAlpha && color0Raw > color1Raw))
            {
                Helper.Swap(ref color0Raw, ref color1Raw);
                Helper.Swap(ref paletteColors[0], ref paletteColors[1]);
            }

            hasAlpha = color0Raw <= color1Raw;

            GetColorPalette(color0Raw, color1Raw, paletteColors);

            {
                Span<byte> pallete = stackalloc byte[colors.Length];
                palletPicker.PickColors(paletteColors, colors, pallete);

                Span<byte> packed = stackalloc byte[sizeof(uint)];
                PackColorPallet(pallete, packed, 2);
                unsafe
                {
                    fixed (void* packedPtr = packed)
                    {
                        return new BC1
                        {
                            Color0 = color0Raw,
                            Color1 = color1Raw,
                            Palette = *(uint*)packedPtr
                        };
                    }
                }
            }

        }

        public static BC4 EncodeBC4(ReadOnlySpan<float> colors, IColorPicker colorPicker, IPalettePicker palettePicker)
        {
            Span<float> paletteColors = stackalloc float[8];

            (paletteColors[0], paletteColors[1]) = colorPicker.SelectColors(colors, palettePicker, true);

            byte color0Raw = PackChannel(paletteColors[0]);
            byte color1Raw = PackChannel(paletteColors[1]);

            if (color0Raw <= color1Raw)
                Helper.Swap(ref color0Raw, ref color1Raw);

            GetColorPalette(color0Raw, color1Raw, paletteColors);

            // compare mode0 with mode1
            Span<float> filtedColors = stackalloc float[colors.Length];
            FilterExtremes(colors, ref filtedColors);
            if(filtedColors.Length != colors.Length)
            {
                float mode0Error = SampleMeanError(palettePicker, colors, paletteColors, out _);

                Span<float> filtedPaletteColors = stackalloc float[8];
                (filtedPaletteColors[0], filtedPaletteColors[1]) = colorPicker.SelectColors(filtedColors, palettePicker, false);

                byte filtedColor0Raw = PackChannel(filtedPaletteColors[0]);
                byte filtedColor1Raw = PackChannel(filtedPaletteColors[1]);

                if (filtedColor0Raw > filtedColor1Raw)
                    Helper.Swap(ref filtedColor0Raw, ref filtedColor0Raw);

                GetColorPalette(filtedColor0Raw, filtedColor1Raw, filtedPaletteColors);

                float mode1Error = SampleMeanError(palettePicker, colors, filtedPaletteColors, out _);

                if (mode1Error < mode0Error)
                {
                    Helper.Copy<float, float>(filtedPaletteColors, paletteColors);
                    color0Raw = filtedColor0Raw;
                    color1Raw = filtedColor1Raw;
                }
            }

            {
                Span<byte> palette = stackalloc byte[colors.Length];
                palettePicker.PickColors(paletteColors, colors, palette);

                Span<byte> packed = stackalloc byte[6];
                PackColorPallet(palette, packed, 3);
                unsafe
                {
                    var bc = new BC4
                    {
                        Color0 = color0Raw,
                        Color1 = color1Raw
                    };
                    Helper.Copy<byte, byte>(packed, new Span<byte>(bc.Palette, 6));
                    return bc;
                }
            }
        }

        public static bool HasAlpha(ReadOnlySpan<PixelColor> colors)
        {
            foreach (PixelColor color in colors)
            {
                if (color.A < 1f)
                    return true;
            }
            return false;
        }

        public static ushort PackColor565(this PixelColor color)
        {
            return (ushort)(((ushort)Math.Round(color.R * 31f) << 11) + (((ushort)Math.Round(color.G * 63f) & 0x3F) << 5) + ((ushort)Math.Round(color.B * 31f) & 0x1F));
        }

        public static byte PackChannel(this float value)
        {
            return (byte)Math.Max(Math.Min(Math.Round(value * byte.MaxValue), byte.MaxValue), byte.MinValue);
        }

        public static PixelColor UnpackColor565(this ushort color)
        {
            float r = (color >> 11) * (1f / 31f);
            float g = ((color >> 5) & 0x3F) * (1f / 63f);
            float b = (color & 0x1F) * (1f / 31f);

            return new PixelColor(Vector4.Clamp(new Vector4(r, g, b, 1f), Vector4.Zero, Vector4.One));
        }

        public static float UnpackChannel(this byte value)
        {
            return value * (1f / byte.MaxValue);
        }

        public unsafe static void PackColorPallet(in ReadOnlySpan<byte> paletteSrc, in Span<byte> paletteDst, in byte bits = 2)
        {
            byte mask = (byte)(byte.MaxValue >> (8 - bits));

            int pos = 0;
            int avaliable = 8;
            for(int i = 0; i < paletteSrc.Length; i++)
            {
                avaliable -= bits;
                if(avaliable < 0)
                {
                    paletteDst[pos++] |= (byte)((paletteSrc[i] & mask) >> (- avaliable));
                    avaliable += 8;
                }

                paletteDst[pos] |= (byte)((paletteSrc[i] & mask) << avaliable);
            }
        }

        public static void UnpackColorPallet(uint paletteSrc, Span<byte> paletteDst, in byte bits = 2)
        {
            unsafe
            {
                uint* ptr = &paletteSrc;
                UnpackColorPallet(new Span<byte>(ptr, sizeof(uint)), paletteDst, bits);
            }
        }

        public static void UnpackColorPallet(Span<byte> paletteSrc, Span<byte> paletteDst, in byte bits = 2)
        {
            byte mask = (byte)(byte.MaxValue >> (8 - bits));

            int pos = 0;
            int avaliable = 8;
            for (int i = 0; i < paletteDst.Length; i++)
            {
                avaliable -= bits;
                if (avaliable < 0)
                {
                    paletteDst[i] |= (byte)((paletteSrc[pos++] << (-avaliable)) & mask);
                    avaliable += 8;
                }
                paletteDst[i] |= (byte)((paletteSrc[pos] >> avaliable) & mask);
            }
            //byte[] values = BitConverter.GetBytes(value);
            //byte[] realPallet = new byte[16];
            //for (int i = 0; i < realPallet.Length; i++)
            //    realPallet[i] = (byte)((values[i >> 2] >> ((i & 3) << 1)) & 0x3);

            //return realPallet;
        }

        public static byte UnpackColorPallet(in uint palette, in int index, in byte bits = 2)
        {
            unsafe
            {
                byte* ptr = stackalloc byte[sizeof(uint)];
                *(uint*)ptr = palette;
                return UnpackColorPallet(new Span<byte>(ptr, sizeof(uint)), index);
            }
        }

        public static byte UnpackColorPallet(Span<byte> palette, int index, in byte bits = 2)
        {
            byte mask = (byte)(byte.MaxValue >> (8 - bits));
            byte value = 0;

            int avaliable = 8 - bits * index % 8 - bits;
            int pos = index * bits / 8;

            if (avaliable < 0)
            {
                value |= (byte)((palette[pos++] << (-avaliable)) & mask);
                avaliable += 8;
            }

            value |= (byte)((palette[pos] >> avaliable) & mask);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float GetProjectionsOverLine(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ab = b - a;
            return Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
        }

        public static Vector3 GetClosestPointOnInterval(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ab = b - a;
            return a + (Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab)) * ab;
        }

        public static float GetProjectionsOverLine(Vector4 a, Vector4 b, Vector4 point)
        {
            Vector4 ab = b - a;
            return Vector4.Dot(point - a, ab) / Vector4.Dot(ab, ab);
        }

        public static ReadOnlySpan<float> GetProjections3(in Vector3 a, in Vector3 b, ReadOnlySpan<PixelColor> colors)
        {
            float[] projections = new float[colors.Length];

            Vector3 ab = b - a;
            for (int i = 0; i < projections.Length; i++)
                projections[i] = (float)Vector3.Dot(colors[i].Color3 - a, ab) / Vector3.Dot(ab, ab);

            return projections;
        }

        public static ReadOnlySpan<float> GetProjections3(in float a, in float b, ReadOnlySpan<float> colors)
        {
            float[] projections = new float[colors.Length];

            float ab = b - a;
            for (int i = 0; i < projections.Length; i++)
                projections[i] = (colors[i] * ab) + a;

            return projections;
        }

        public static ReadOnlySpan<float> GetProjections3Intergiate(in Vector3 a, in Vector3 b, ReadOnlySpan<PixelColor> colors)
        {
            float[] projections = new float[colors.Length];

            float n = colors[0].A < 1f ? 2f : 3f;
            float invN = 1f / n;
            Vector3 ab = b - a;
            for (int i = 0; i < projections.Length; i++)
                projections[i] = (float)Math.Round((float)Vector3.Dot(colors[i].Color3 - a, ab) / Vector3.Dot(ab, ab) * n) / invN;

            return projections;
        }

        public static ReadOnlySpan<float> GetProjections3Intergiate(in float a, in float b, ReadOnlySpan<float> colors)
        {
            float[] projections = new float[colors.Length];

            float ab = b - a;
            for (int i = 0; i < projections.Length; i++)
                projections[i] = (float)Math.Round(((colors[i] * ab) + a) * 7f) * (1f / 7f);

            return projections;
        }

        public static void GetColorPalette(in ushort rawColor0, in ushort rawColor1, Span<PixelColor> paletteColors)
        {
            paletteColors[0] = UnpackColor565(rawColor0);
            paletteColors[1] = UnpackColor565(rawColor1);

            if (rawColor0 <= rawColor1)
            {
                paletteColors[2] = paletteColors[0] * (1f / 2f) + paletteColors[1] * (1f / 2f);
                paletteColors[3] = new PixelColor(Vector4.Zero);
            }
            else
            {
                paletteColors[2] = paletteColors[0] * (2f / 3f) + paletteColors[1] * (1f / 3f);
                paletteColors[3] = paletteColors[0] * (1f / 3f) + paletteColors[1] * (2f / 3f);
            }
        }

        public static ReadOnlySpan<PixelColor> GetColorPalette(in ushort rawColor0, in ushort rawColor1)
        {
            PixelColor[] palette = new PixelColor[4];
            GetColorPalette(rawColor0, rawColor1, palette);
            return palette;
        }

        //public static (PixelColor color2, PixelColor color3) CalculateColors(in ushort rawColor0, in ushort rawColor1)
        //{
        //    PixelColor color2, color3;

        //    PixelColor color0 = UnpackColor565(rawColor0);
        //    PixelColor color1 = UnpackColor565(rawColor1);

        //    if (rawColor0 <= rawColor1)
        //    {
        //        color2 = color0 * (1f / 2f) + color1 * (1f / 2f);
        //        color3 = new PixelColor(Vector4.Zero);
        //    }
        //    else
        //    {
        //        color2 = color0 * (2f / 3f) + color1 * (1f / 3f);
        //        color3 = color0 * (1f / 3f) + color1 * (2f / 3f);
        //    }

        //    return (color2, color3);
        //}

        public static void GetColorPalette(in byte rawColor0, in byte rawColor1, Span<float> paletteColors)
        {
            paletteColors[0] = UnpackChannel(rawColor0);
            paletteColors[1] = UnpackChannel(rawColor1);

            if (rawColor0 > rawColor1)
            {
                paletteColors[2] = paletteColors[0] * (6f / 7f) + paletteColors[1] * (1f / 7f);
                paletteColors[3] = paletteColors[0] * (5f / 7f) + paletteColors[1] * (2f / 7f);
                paletteColors[4] = paletteColors[0] * (4f / 7f) + paletteColors[1] * (3f / 7f);
                paletteColors[5] = paletteColors[0] * (3f / 7f) + paletteColors[1] * (4f / 7f);
                paletteColors[6] = paletteColors[0] * (2f / 7f) + paletteColors[1] * (5f / 7f);
                paletteColors[7] = paletteColors[0] * (1f / 7f) + paletteColors[1] * (6f / 7f);
            }
            else
            {
                paletteColors[2] = paletteColors[0] * (4f / 5f) + paletteColors[1] * (1f / 5f);
                paletteColors[3] = paletteColors[0] * (3f / 5f) + paletteColors[1] * (2f / 5f);
                paletteColors[4] = paletteColors[0] * (2f / 5f) + paletteColors[1] * (3f / 5f);
                paletteColors[5] = paletteColors[0] * (1f / 5f) + paletteColors[1] * (4f / 5f);
                paletteColors[6] = 0f;
                paletteColors[7] = 1f;
            }
        }

        public static ReadOnlySpan<float> GetColorPalette(in byte rawColor0, in byte rawColor1)
        {
            float[] paletteColors = new float[8];
            GetColorPalette(rawColor0, rawColor1, paletteColors);
            return paletteColors;
        }
        //public static (float color2, float color3, float color4, float color5, float color6, float color7) CalculateColors(in byte rawColor0, in byte rawColor1)
        //{
        //    (float color2, float color3, float color4, float color5, float color6, float color7) colors;

        //    float color0 = UnpackChannel(rawColor0);
        //    float color1 = UnpackChannel(rawColor1);

        //    if (rawColor0 > rawColor1)
        //    {
        //        colors.color2 = color0 * (6f / 7f) + color1 * (1f / 7f);
        //        colors.color3 = color0 * (5f / 7f) + color1 * (2f / 7f);
        //        colors.color4 = color0 * (4f / 7f) + color1 * (3f / 7f);
        //        colors.color5 = color0 * (3f / 7f) + color1 * (4f / 7f);
        //        colors.color6 = color0 * (2f / 7f) + color1 * (5f / 7f);
        //        colors.color7 = color0 * (1f / 7f) + color1 * (6f / 7f);
        //    }
        //    else
        //    {
        //        colors.color2 = color0 * (4f / 5f) + color1 * (1f / 5f);
        //        colors.color3 = color0 * (3f / 5f) + color1 * (2f / 5f);
        //        colors.color4 = color0 * (2f / 5f) + color1 * (3f / 5f);
        //        colors.color5 = color0 * (1f / 5f) + color1 * (4f / 5f);
        //        colors.color6 = 0f;
        //        colors.color7 = 1f;
        //    }

        //    return colors;
        //}

        public static PixelColor CalculateColor(in ushort rawColor0, in ushort rawColor1, byte color)
        {
            PixelColor color0, color1;
            switch (color)
            {
                case 0:
                    return UnpackColor565(rawColor0);
                case 1:
                    return UnpackColor565(rawColor1);
                case 2:
                    color0 = UnpackColor565(rawColor0);
                    color1 = UnpackColor565(rawColor1);

                    return (rawColor0 <= rawColor1) ?
                        color0 * (1f / 2f) + color1 * (1f / 2f) :
                        color0 * (2f / 3f) + color1 * (1f / 3f);
                case 3:
                    color0 = UnpackColor565(rawColor0);
                    color1 = UnpackColor565(rawColor1);
                    return (rawColor0 <= rawColor1) ? 
                        new PixelColor(Vector4.Zero) :
                        color0 * (1f / 3f) + color1 * (2f / 3f);
            }

            return new PixelColor(Vector4.One * float.NaN);
        }

        public static float SampleMeanError(IPalettePicker palletPicker, ReadOnlySpan<PixelColor> colors, ReadOnlySpan<PixelColor> paletteColors, out Vector3 errorMean)
        {
            Span<byte> palette = stackalloc byte[colors.Length];
            palletPicker.PickColors(paletteColors, colors, palette);

            float error = 0f;
            errorMean = Vector3.Zero;

            float invLength = 1f / colors.Length;
            for (int i = 0; i < colors.Length; i++)
            {
                Vector3 aux = colors[i].Color3 - paletteColors[palette[i]].Color3;
                Vector3 auxAbs = Vector3.Abs(aux);
                error += (float)Math.Pow(auxAbs.X + auxAbs.Y + auxAbs.Z, 2);
                errorMean += aux * invLength;
            }

            return error;
        }

        public static float SampleMeanError(IPalettePicker palletPicker, ReadOnlySpan<float> colors, ReadOnlySpan<float> paletteColors, out float errorMean)
        {
            Span<byte> palette = stackalloc byte[colors.Length];
            palletPicker.PickColors(paletteColors, colors, palette);

            float error = 0f;
            errorMean = 0f;

            float invLength = 1f / colors.Length;
            for (int i = 0; i < colors.Length; i++)
            {
                float aux = colors[i] - paletteColors[palette[i]];
                error += (float)Math.Pow(aux, 2);
                errorMean += aux * invLength;
            }

            return error;
        }

        private static void FilterExtremes(ReadOnlySpan<float> values, ref Span<float> result)
        {
            const float threshold = 1f / 255f;
            int i = 0;
            foreach(var value in values)
            {
                if (!(Math.Abs(value - 1) <= threshold || value <= threshold))
                    result[i++] = value;
            }
            if(i == 0)
            {
                i = 1;
                result[0] = values[0];
            }
            result = result.Slice(0, i);
        }
    }
}
