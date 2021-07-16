using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Cox
{
    public static class Helper
    {
        private static bool Contains<T>(this ReadOnlySpan<T> span, in T value)
        {
            foreach (var element in span)
                if (element.Equals(value))
                    return true;
            return false;
        }

        public static float Clamp(float value, float min, float max) =>
                    Math.Min(Math.Max(value, min), max);

        public static bool Approximately(float a, float b, float border = float.Epsilon)
        {
            if (a == 0 || b == 0)
                return Math.Abs(a - b) <= border;
            return Math.Abs(a - b) / Math.Abs(a) <= border &&
                Math.Abs(a - b) / Math.Abs(b) <= border;
        }

        public static void Copy<TSrc, TDst>(ReadOnlySpan<TSrc> source, Span<TDst> destination) where TSrc : unmanaged where TDst : unmanaged
        {
            unsafe
            {
                fixed (void* pSource = source, pDestination = destination)
                    Buffer.MemoryCopy(pSource, pDestination, destination.Length * sizeof(TDst), source.Length * sizeof(TSrc));
            }
        }
        public static void Swap<T>(ref T t1, ref T t2)
        {
            T aux = t1;
            t1 = t2;
            t2 = aux;
        }
        public delegate void ParallelTextureProcessing(ITexture tile, Point location);

        public static void RunParallelTextureProcessing(ITexture texture, Size tileSize, int threads, ParallelTextureProcessing processing)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0);

            if (threads <= 0)
                threads = Environment.ProcessorCount;

            uint lines = (uint)Math.Floor(Math.Sqrt(threads));
            uint columns = (uint)Math.Floor(threads / (float)lines);

            uint unitW = (uint)Math.Min(Math.Floor(texture.Width / (float)(columns * tileSize.Width)), texture.Width);
            uint unitH = (uint)Math.Min(Math.Floor(texture.Height / (float)(lines * tileSize.Height)), texture.Height);

            if (unitW == 0)
                unitW = texture.Width;
            else
                unitW *= (uint)tileSize.Width;

            if (unitH == 0)
                unitH = texture.Height / (uint)tileSize.Height;
            else
                unitH *= (uint)tileSize.Height;

            columns = (uint)Math.Floor(texture.Width / (float)unitW);
            lines = (uint)Math.Floor(texture.Height / (float)unitH);
            for (uint i = 0; i < columns; i++)
                for (uint j = 0; j < lines; j++)
                {
                    uint locationX = i * unitW;
                    uint locationY = j * unitH;
                    Rectangle rectangle = new Rectangle(
                        (int)locationX,
                        (int)locationY,
                        (int)Math.Min(unitW, texture.Width - locationX),
                        (int)Math.Min(unitH, texture.Height - locationY));

                    ITexture tile = texture.Slice(rectangle);

                    ThreadPool.QueueUserWorkItem((obj) =>
                    {
                        processing.Invoke(tile, new Point((int)locationX, (int)locationY));
                        semaphore.Release(1);
                    });
                }
            for (uint i = 0; i < columns; i++)
                for (uint j = 0; j < lines; j++)
                    semaphore.Wait();
        }
    }
}
