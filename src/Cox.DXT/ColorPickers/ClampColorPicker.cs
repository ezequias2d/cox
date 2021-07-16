using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cox.DXT.ColorPickers
{
    public class ClampColorPicker : IColorPicker
    {
        private ClampColorPicker()
        {

        }

        private static ClampColorPicker _instance;

        public static ClampColorPicker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ClampColorPicker();
                return _instance;
            }
        }

        public (PixelColor, PixelColor) SelectColors(ReadOnlySpan<PixelColor> colors, IPalettePicker palletPicker)
        {
            Vector4 min = Vector4.One * float.MaxValue;
            Vector4 max = Vector4.One * float.MinValue;

            foreach(var color in colors)
            {
                min = Vector4.Min(color.Color, min);
                max = Vector4.Max(color.Color, max);
            }

            return (new PixelColor(min), new PixelColor(max));
        }

        public (float Color1, float Color2) SelectColors(ReadOnlySpan<float> colors, IPalettePicker palletPicker, bool mode0)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (var color in colors)
            {
                min = Math.Min(color, min);
                max = Math.Max(color, max);
            }

            return (min, max);
        }
    }
}
