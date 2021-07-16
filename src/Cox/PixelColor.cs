using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cox
{
    public struct PixelColor
    {
        private Vector4 _color;
        public Vector4 Color { get => _color; set => _color = value; }
        public float R { get => Color.X; set => _color.X = value; }
        public float G { get => Color.Y; set => _color.Y = value; }
        public float B { get => Color.Z; set => _color.Z = value; }
        public float A { get => Color.W; set => _color.W = value; }

        public float BW { get => (R + G + B) * (1f / 3f); }

        public Vector3 Color3 => new Vector3(R, G, B);

        public PixelColor(Vector4 color)
        {
            _color = color;
        }

        public PixelColor(Vector3 color, float a = 1f)
        {
            _color = new Vector4(color, a);
        }

        public PixelColor(float bw, float a = 1f)
        {
            _color = new Vector4(new Vector3(bw), a);
        }

        public static PixelColor From(byte r, byte g, byte b, byte a) =>
            new PixelColor(new Vector4(r, g, b, a) * (1f / 255f));

        public static PixelColor operator +(PixelColor color1) =>
            color1;

        public static PixelColor operator -(PixelColor color1) =>
            new PixelColor(-color1.Color);

        public static PixelColor operator +(PixelColor color1, PixelColor color2) =>
            new PixelColor(color1.Color + color2.Color);

        public static PixelColor operator -(PixelColor color1, PixelColor color2) =>
            new PixelColor(color1.Color - color2.Color);

        public static PixelColor operator *(PixelColor color1, PixelColor color2) =>
            new PixelColor(color1.Color * color2.Color);

        public static PixelColor operator /(PixelColor color1, PixelColor color2) =>
            new PixelColor(color1.Color / color2.Color);

        public static PixelColor operator *(PixelColor color1, float factor) =>
            new PixelColor(color1.Color * factor);
        public static PixelColor operator /(PixelColor color1, float factor) =>
            new PixelColor(color1.Color / factor);

        public static PixelColor Min(ReadOnlySpan<PixelColor> colors)
        {
            if (colors.Length == 1)
                return colors[0];

            Vector4 color = Vector4.Min(colors[0].Color, colors[1].Color);

            for (int i = 2; i < colors.Length; i++)
                color = Vector4.Min(color, colors[i].Color);

            return new PixelColor(color);
        }

        public static PixelColor Max(ReadOnlySpan<PixelColor> colors)
        {
            if (colors.Length == 1)
                return colors[0];

            Vector4 color = Vector4.Max(colors[0].Color, colors[1].Color);

            for (int i = 2; i < colors.Length; i++)
                color = Vector4.Max(color, colors[i].Color);

            return new PixelColor(color);
        }

        public override bool Equals(object obj)
        {
            return obj is PixelColor color &&
                   Color.Equals(color.Color);
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode();
        }
    }
}
