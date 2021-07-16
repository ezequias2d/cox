using System;
using System.Numerics;

namespace Cox.DXT.PalletPickers
{
    public sealed class ClosestPalletPicker : IPalettePicker
    {
        private ClosestPalletPicker() { }

        public static ClosestPalletPicker Instance { get; } = new ClosestPalletPicker();

        public byte PickColor(in ReadOnlySpan<PixelColor> palletColors, in PixelColor color)
        {
            float colorDistance = float.MaxValue;
            byte current = 0;

            for (byte i = 0; i < palletColors.Length; i++)
            {
                float newColorDistance = Vector4.DistanceSquared(color.Color, palletColors[i].Color);
                if (newColorDistance <= colorDistance)
                {
                    current = i;
                    colorDistance = newColorDistance;
                }
            }

            return current;
        }

        public byte PickColor(in ReadOnlySpan<float> palletColors, in float color)
        {
            float colorDistance = float.MaxValue;
            byte current = 0;

            for (byte i = 0; i < palletColors.Length; i++)
            {
                float newColorDistance = Math.Abs(color - palletColors[i]);
                if (newColorDistance <= colorDistance)
                {
                    current = i;
                    colorDistance = newColorDistance;
                }
            }

            return current;
        }

        public void PickColors(in ReadOnlySpan<PixelColor> palletColors, in ReadOnlySpan<PixelColor> colorsSrc, in Span<byte> colorsDst)
        {
            for (int i = 0; i < colorsSrc.Length; i++)
                colorsDst[i] = PickColor(palletColors, colorsSrc[i]);
        }

        public void PickColors(in ReadOnlySpan<float> palletColors, in ReadOnlySpan<float> colorsSrc, in Span<byte> colorsDst)
        {
            for (int i = 0; i < colorsSrc.Length; i++)
                colorsDst[i] = PickColor(palletColors, colorsSrc[i]);
        }
    }
}
