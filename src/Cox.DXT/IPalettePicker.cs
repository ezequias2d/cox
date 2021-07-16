using System;
using System.Collections.Generic;
using System.Text;

namespace Cox.DXT
{
    public interface IPalettePicker
    {

        /// <summary>
        /// Select one of <paramref name="palletColors"/> to represents the <paramref name="color"/> and return the index.
        /// </summary>
        /// <param name="palletColors">The color pallet.</param>
        /// <param name="color">The color to select in <paramref name="palletColors"/></param>
        /// <returns>Index of color selected.</returns>
        byte PickColor(in ReadOnlySpan<PixelColor> palletColors, in PixelColor color);


        /// <summary>
        /// Select one of <paramref name="paletteColors"/> to represents the <paramref name="color"/> and return the index.
        /// </summary>
        /// <param name="paletteColors">The color pallet.</param>
        /// <param name="color">Colors to select in <paramref name="paletteColors"/></param>
        /// <returns>Array of index of color selected.</returns>
        void PickColors(in ReadOnlySpan<PixelColor> paletteColors, in ReadOnlySpan<PixelColor> colorsSrc, in Span<byte> colorsDst);

        /// <summary>
        /// Select one of <paramref name="palletColors"/> to represents the <paramref name="color"/> and return the index.
        /// </summary>
        /// <param name="palletColors">The color pallet.</param>
        /// <param name="color">The color to select in <paramref name="palletColors"/></param>
        /// <returns>Index of color selected.</returns>
        byte PickColor(in ReadOnlySpan<float> palletColors, in float color);


        /// <summary>
        /// Select one of <paramref name="palletColors"/> to represents the <paramref name="color"/> and return the index.
        /// </summary>
        /// <param name="palletColors">The color pallet.</param>
        /// <param name="color">Colors to select in <paramref name="palletColors"/></param>
        /// <returns>Array of index of color selected.</returns>
        void PickColors(in ReadOnlySpan<float> palletColors, in ReadOnlySpan<float> colorsSrc, in Span<byte> colorsDst);
    }
}
