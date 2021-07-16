using System;
using System.Collections.Generic;
using System.Numerics;

namespace Cox.DXT.ColorPickers
{
    public class LinearRegressionColorPicker : IColorPicker
    {
        private LinearRegressionColorPicker()
        {

        }

        private static LinearRegressionColorPicker _instance;

        public static LinearRegressionColorPicker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LinearRegressionColorPicker();
                return _instance;
            }
        }

        public (PixelColor, PixelColor) SelectColors(ReadOnlySpan<PixelColor> colors, IPalettePicker palletPicker)
        {
            return CalculateColors(colors, palletPicker);
        }

        public (float Color1, float Color2) SelectColors(ReadOnlySpan<float> colors, IPalettePicker palletPicker, bool mode0 = true)
        {
            //Span<PixelColor> colors2 = stackalloc PixelColor[colors.Length];
            //int i = 0;
            //foreach (var color in colors)
            //    colors2[i++] = new PixelColor(color, 1f);

            Span<float> colorPalette = stackalloc float[8];
            ReadOnlySpan<float> filtedColors = FilterColors(colors);
            //ReadOnlySpan<PixelColor> filtedColors = FilterColors(colors2);
            float minColor = 0f;
            float maxColor = 1f;

            float error = float.MaxValue;
            float sampleError = 0f;

            bool loop = true;
            bool intergiate = false;
            uint count = 0;

            do
            {
                (float A, float B) = CalculateAB3(filtedColors, minColor - sampleError * 0.5f, maxColor + sampleError * 0.5f, intergiate, out float min, out float max);

                //(Vector3 vA, Vector3 vB) = CalculateAB3(filtedColors, Vector3.One * (minColor - sampleError * 0.5f), Vector3.One * (maxColor + sampleError * 0.5f), intergiate, out float min, out float max);
                //(float A, float B) = ((vA.X + vA.Y + vA.Z) / 3f, (vB.X + vB.Y + vB.Z) / 3f);
                intergiate = true;
                SetIfNanOrInfinity(colors[0], ref A);
                SetIfNanOrInfinity(0, ref B);
                SetIfNanOrInfinity(0, ref min);
                SetIfNanOrInfinity(0, ref max);

                var rawColor0 = DXTHelper.PackChannel(Helper.Clamp(A + B * min, 0f, 1f));
                var rawColor1 = DXTHelper.PackChannel(Helper.Clamp(A + B * max, 0f, 1f));

                if ((rawColor0 <= rawColor1 && mode0) || (rawColor0 > rawColor1 && !mode0))
                    Helper.Swap(ref rawColor0, ref rawColor1);

                DXTHelper.GetColorPalette(rawColor0, rawColor1, colorPalette);

                float newError = DXTHelper.SampleMeanError(palletPicker, colors, colorPalette, out sampleError);

                if ((Helper.Approximately(newError, error) || newError < error) && (minColor != colorPalette[0] || maxColor != colorPalette[1]))
                {
                    error = newError;
                    minColor = colorPalette[0];
                    maxColor = colorPalette[1];
                    count++;
                }
                else
                    loop = false;
            } while (loop && count < 16);

            Console.Write($"{count}, ");
            return (minColor, maxColor);
        }

        private static ReadOnlySpan<PixelColor> FilterColors(ReadOnlySpan<PixelColor> colors)
        {
            List<PixelColor> listColors = new List<PixelColor>(colors.Length);

            foreach(var color in colors)
            {
                bool add = true;
                foreach (var aux in listColors)
                {
                    
                    if (Helper.Approximately(Vector3.Distance(color.Color3, aux.Color3), 0f, 0.00226411870270441476278097560981f))
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    listColors.Add(color);
            }

            return listColors.ToArray();
        }

        private static ReadOnlySpan<float> FilterColors(ReadOnlySpan<float> colors)
        {
            List<float> listColors = new List<float>(colors.Length);

            foreach (var color in colors)
            {
                bool add = true;
                foreach (var aux in listColors)
                {

                    if (Helper.Approximately(color, aux, 0.00226411870270441476278097560981f))
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    listColors.Add(color);
            }

            return listColors.ToArray();
        }

        /// <summary>
        /// Least squares algorithm.
        /// </summary>
        /// <param name="colors">Colors to regretion.</param>
        /// <returns>A and B</returns>
        private static (PixelColor, PixelColor) CalculateColors(ReadOnlySpan<PixelColor> colors, IPalettePicker palletPicker)
        {
            Span<PixelColor> colorPalette = stackalloc PixelColor[4];
            ReadOnlySpan<PixelColor> filtedColors = FilterColors(colors);
            Vector3 minColor = Vector3.Zero;
            Vector3 maxColor = Vector3.One;

            float error = float.MaxValue;
            Vector3 errorMean = Vector3.Zero;

            bool loop = true;
            bool intergiate = false;
            uint count = 0;
            do
            {
                (Vector3 A, Vector3 B) = CalculateAB3(filtedColors, minColor - errorMean * 0.5f, maxColor + errorMean * 0.5f, intergiate, out float min, out float max);
                intergiate = true;
                SetIfNanOrInfinity(colors[0].Color3, ref A);
                SetIfNanOrInfinity(Vector3.Zero, ref B);
                SetIfNanOrInfinity(0, ref min);
                SetIfNanOrInfinity(0, ref max);

                var rawColor0 = DXTHelper.PackColor565(new PixelColor(Vector3.Clamp(A + B * min, Vector3.Zero, Vector3.One), 1f));
                var rawColor1 = DXTHelper.PackColor565(new PixelColor(Vector3.Clamp(A + B * max, Vector3.Zero, Vector3.One), 1f));
                DXTHelper.GetColorPalette(rawColor0, rawColor1, colorPalette);

                float newError = DXTHelper.SampleMeanError(palletPicker, colors, colorPalette, out errorMean);

                if ((Helper.Approximately(newError, error) || newError < error) && (minColor != colorPalette[0].Color3 || maxColor != colorPalette[1].Color3))
                {
                    error = newError;
                    minColor = colorPalette[0].Color3;
                    maxColor = colorPalette[1].Color3;
                    count++;
                }
                else
                    loop = false;
            } while (loop && count < 16);

            return (new PixelColor(new Vector4(minColor, 1f)), new PixelColor(new Vector4(maxColor, 1f)));
        }

        //private static float SampleMeanError(IPalletPicker palletPicker, ReadOnlySpan<PixelColor> colors, ref Vector3 min, ref Vector3 max, out Vector3 errorVector)
        //{
        //    PixelColor[] blockColors = new PixelColor[4];
        //    ushort color0Raw = DXTHelper.PackColor565(new PixelColor(new Vector4(min, 1f)));
        //    ushort color1Raw = DXTHelper.PackColor565(new PixelColor(new Vector4(max, 1f)));

        //    blockColors[0] = DXTHelper.UnpackColor565(color0Raw);
        //    blockColors[1] = DXTHelper.UnpackColor565(color1Raw);
        //    (blockColors[2], blockColors[3]) = DXTHelper.CalculateColors(color0Raw, color1Raw);

        //    Span<byte> pallet = stackalloc byte[colors.Length];
        //    palletPicker.PickColors(blockColors, colors, pallet);

        //    float error = 0;
        //    errorVector = Vector3.Zero;
            
        //    for (int i = 0; i < colors.Length; i++)
        //    {
        //        Vector3 aux = colors[i].Color3 - blockColors[pallet[i]].Color3;
        //        //error += Vector3.Dot(aux * aux, Vector3.One) * (1f / 3f);
        //        error += (float)Math.Pow(aux.X + aux.Y + aux.Z, 2);
        //        errorVector += aux / colors.Length;
        //    }

        //    min = blockColors[0].Color3;
        //    max = blockColors[1].Color3;

        //    return error;
        //}

        //private static float SampleMeanError(IPalletPicker palletPicker, ReadOnlySpan<float> colors, ref float min, ref float max, out float errorMean)
        //{
        //    float[] blockColors = new float[8];
        //    byte color0Raw = DXTHelper.PackChannel(min);
        //    byte color1Raw = DXTHelper.PackChannel(max);

        //    blockColors[0] = DXTHelper.UnpackChannel(color0Raw);
        //    blockColors[1] = DXTHelper.UnpackChannel(color1Raw);
        //    (blockColors[2], blockColors[3], blockColors[4], blockColors[5], blockColors[6], blockColors[7]) = DXTHelper.CalculateColors(color0Raw, color1Raw);

        //    Span<byte> pallet = stackalloc byte[colors.Length];
        //    palletPicker.PickColors(blockColors, colors, pallet);

        //    float error = 0f;
        //    errorMean = 0f;

        //    for (int i = 0; i < colors.Length; i++)
        //    {
        //        float aux = Math.Abs(colors[i] - blockColors[pallet[i]]);
        //        error += (float)Math.Pow(aux, 2);
        //        errorMean += aux / colors.Length;
        //    }

        //    min = blockColors[0];
        //    max = blockColors[1];

        //    return error;
        //}

        private static void SetIfNanOrInfinity(Vector3 value, ref Vector3 ref1)
        {
            if (float.IsNaN(ref1.X) || float.IsInfinity(ref1.X))
                ref1.X = value.X;

            if (float.IsNaN(ref1.Y) || float.IsInfinity(ref1.Y))
                ref1.Y = value.Y;

            if (float.IsNaN(ref1.Z) || float.IsInfinity(ref1.Z))
                ref1.Z = value.Z;
        }

        private static void SetIfNanOrInfinity(float value, ref float ref1)
        {
            if (float.IsNaN(ref1) || float.IsInfinity(ref1))
                ref1 = value;
        }

        private static (Vector3 A, Vector3 B) CalculateAB3(ReadOnlySpan<PixelColor> colors, in Vector3 minColor, Vector3 maxColor, bool intergiateProjection, out float min, out float max)
        {
            Vector3 colorSampleMean = SampleMean(colors).Color3;
            
            ReadOnlySpan<float> projections;
            if (intergiateProjection)
                projections = DXTHelper.GetProjections3Intergiate(minColor, maxColor, colors);
            else
                projections = DXTHelper.GetProjections3(minColor, maxColor, colors);

            float projectionsSampleMean = SamplerMean(projections);

            Vector3 numerator = Vector3.Zero;
            float denominator = 0;

            for (int i = 0; i < colors.Length; i++)
            {
                float dProjection = projections[i] - projectionsSampleMean;
                numerator += dProjection * (colors[i].Color3 - colorSampleMean);
                denominator += dProjection * dProjection;
            }

            Vector3 result = numerator / denominator;

            min = Min(projections);
            max = Max(projections);

            return (colorSampleMean - result * projectionsSampleMean , result);
        }

        private static (float A, float B) CalculateAB3(ReadOnlySpan<float> colors, in float minColor, float maxColor, bool intergiateProjection, out float min, out float max)
        {
            float colorSampleMean = SampleMean(colors);

            ReadOnlySpan<float> projections;
            if (intergiateProjection)
                projections = DXTHelper.GetProjections3Intergiate(minColor, maxColor, colors);
            else
                projections = DXTHelper.GetProjections3(minColor, maxColor, colors);

            float projectionsSampleMean = SamplerMean(projections);

            float numerator = 0f;
            float denominator = 0f;

            for (int i = 0; i < colors.Length; i++)
            {
                float dProjection = projections[i] - projectionsSampleMean;
                numerator += dProjection * (colors[i] - colorSampleMean);
                denominator += dProjection * dProjection;
            }

            min = Min(projections);
            max = Max(projections);

            return (colorSampleMean - numerator * projectionsSampleMean / denominator, numerator / denominator);
        }

        private static float Max(ReadOnlySpan<float> span)
        {
            float value = float.MinValue;
            foreach (var a in span)
                value = Math.Max(value, a);
            return value;
        }

        private static float Min(ReadOnlySpan<float> span)
        {
            float value = float.MaxValue;
            foreach (var a in span)
                value = Math.Min(value, a);
            return value;
        }

        //private static ReadOnlySpan<float> GetProjections3(in Vector3 min, in Vector3 max, ReadOnlySpan<PixelColor> colors)
        //{
        //    float[] projections = new float[colors.Length];
        //    Vector3 a = min;
        //    Vector3 b = Vector3.Normalize(max - a);

        //    float abDot = Vector3.Dot(a, b);
        //    float inv4BbDot = 1f / (Vector3.Dot(b, b) * 3);

        //    for (int i = 0; i < projections.Length; i++)
        //        projections[i] = (Vector3.Dot(colors[i].Color.ToVector3(), b) + abDot) * inv4BbDot;

        //    return projections;
        //}

        private static PixelColor SampleMean(ReadOnlySpan<PixelColor> colors)
        {
            PixelColor total = new PixelColor(Vector4.Zero);

            float div = 1f / colors.Length;
            for (uint i = 0; i < colors.Length; i++)
                total += colors[(int)i];

            return total * div;
        }

        private static float SampleMean(ReadOnlySpan<float> colors)
        {
            float total = 0f;

            float div = 1f / colors.Length;
            for (uint i = 0; i < colors.Length; i++)
                total += colors[(int)i];

            return total * div;
        }

        private static Vector4 SampleMean(IList<PixelColor> colors)
        {
            Vector4 total = Vector4.Zero;

            float div = 1f / colors.Count;
            foreach(var color in colors)
                total += color.Color * div;

            return total;
        }

        private static float SamplerMean(ReadOnlySpan<float> projections)
        {
            double total = 0;

            double div = 1f / projections.Length;  
            for (uint i = 0; i < projections.Length; i++)
                total += projections[(int)i] * div;

            return (float)total;
        }
    }
}
