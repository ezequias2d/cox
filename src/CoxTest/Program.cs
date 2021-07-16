using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cox;
using Cox.DXT;
using Cox.DXT.ColorPickers;
using Cox.DXT.PalletPickers;
using Cox.File;
using Cox.GDI;
using Cox.Mipmaps;
using Cox.YCC;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;

namespace CoxTest
{
    public class Bench
    {
        private readonly ITexture texture;

        public Bench()
        {
            string file = "D:\\Drive\\Arquivos\\Wallpaper\\TexturesCom_Grass0157_1_seamless_S.jpg";

            Bitmap bitmap = new Bitmap(file);
            texture = new BitmapTexture(bitmap);
        }

        [Benchmark]
        public ITexture One() => new DXT1Texture(texture, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance, 1);

        [Benchmark]
        public ITexture Two() => new DXT1Texture(texture, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance, 2);

        [Benchmark]
        public ITexture Three() => new DXT1Texture(texture, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance, 3);

        [Benchmark]
        public ITexture Four() => new DXT1Texture(texture, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance, 4);

        [Benchmark]
        public ITexture Eigth() => new DXT1Texture(texture, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance, 8);

        [Benchmark(Baseline = true)]
        public ITexture All() => new DXT1Texture(texture, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance);

        [Benchmark]
        public ITexture Double() => new DXT1Texture(texture, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance, Environment.ProcessorCount * 2);

    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Transform(new Vector4(0f, 0f, 0f, 1f)));
            Console.WriteLine(Transform(new Vector4(1f, 1f, 1f, 1f)));

            Console.WriteLine(new Vector3(1f / 31f, 1f / 63f, 1f / 31f).Length());
            Random r = new Random();
            byte[] codes = new byte[16];
            for (int i = 0; i < codes.Length; i++)
                //codes[i] = (byte)r.Next(0, 4);
                codes[i] = (byte)(7 - (i % 8));

            Span<byte> packet = stackalloc byte[6];
            DXTHelper.PackColorPallet(codes, packet, 3);

            Span<byte> unpacket = stackalloc byte[16];
            DXTHelper.UnpackColorPallet(packet, unpacket, 3);

            for (int i = 0; i < codes.Length; i++)
            {
                Console.WriteLine($"{codes[i]} = {unpacket[i]} = {DXTHelper.UnpackColorPallet(packet, i, 3)}");
            }


            string file = "D:\\Drive\\Arquivos\\Wallpaper\\demiurge-overlord-z4231.jpg";

            Bitmap bitmap = new Bitmap(file);
            BitmapTexture original = new BitmapTexture(bitmap);

            ITexture bctexture;
            //bctexture = new DXT1Texture(new MipmapTexture(original, 1), LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance);
            bctexture = new MipmapTexture(original, 1);
            //MipmapTexture mipmap = new MipmapTexture(dtx1, 1);
            //dtx1 = new DXT1Texture(mipmap, LinearRegressionColorPicker.Instance, ClosestPalletPicker.Instance);

            BitmapTexture result = new BitmapTexture(bctexture);

            string resultUri = Path.ChangeExtension(Path.ChangeExtension(file, "") + "Result3.", ".png");
            result.GetBitmap().Save(resultUri, ImageFormat.Png);

            string coxUri = Path.ChangeExtension(Path.ChangeExtension(file, ""), ".cox");

            CoxFile coxFile = new CoxFile(bctexture.Width, bctexture.Height, 1, 7, true, bctexture);
            using (var coxStream = File.OpenWrite(coxUri))
            {
                coxFile.Save(coxStream);
            }
            //var summary = BenchmarkRunner.Run<Bench>();
        }

        private static Vector4 Transform(Vector4 value, float kr = 96f / 255f, float kb = 32f / 255f)
        {
            float kg = 1f - (kr + kb);
            float krD = -0.5f / (1f - kr);
            float kbD = -0.5f / (1f - kb);
            Matrix4x4 matrix = Matrix4x4.Transpose(new Matrix4x4(
                                    kr, kg, kb, 0f,
                                    kr * kbD, kg * kbD, 0.5f, 0f,
                                    0.5f, kg * krD, kb * krD, 0f,
                                    0f, 0f, 0f, 1f))
                * Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0.5f)) *
                new Matrix4x4(0, 1, 0, 0,
                              1, 0, 0, 0,
                              0, 0, 1, 0,
                              0, 0, 0, 1);

            return Vector4.Transform(value, matrix);
        }
    }
}
