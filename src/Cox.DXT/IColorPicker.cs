using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DXT
{
    public interface IColorPicker
    {

        (PixelColor Color1, PixelColor Color2) SelectColors(ReadOnlySpan<PixelColor> colors, IPalettePicker palletPicker);

        (float Color1, float Color2) SelectColors(ReadOnlySpan<float> colors, IPalettePicker palletPicker, bool mode0);
    }
}
