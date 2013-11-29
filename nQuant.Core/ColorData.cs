using System.Collections.Generic;

namespace nQuant
{
    public class ColorData
    {
        public ColorData(int dataGranularity, int bitmapWidth, int bitmapHeight)
        {
            dataGranularity++;
            Weights = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            MomentsAlpha = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            MomentsRed = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            MomentsGreen = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            MomentsBlue = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            Moments = new float[dataGranularity, dataGranularity, dataGranularity, dataGranularity];

            int pixelCount = bitmapWidth*bitmapHeight;
            pixels = new Pixel[pixelCount];
            quantizedPixels = new int[pixelCount];
        }

        public long[, , ,] Weights { get; private set; }
        public long[, , ,] MomentsAlpha { get; private set; }
        public long[, , ,] MomentsRed { get; private set; }
        public long[, , ,] MomentsGreen { get; private set; }
        public long[, , ,] MomentsBlue { get; private set; }
        public float[, , ,] Moments { get; private set; }

        public IList<int> QuantizedPixels { get { return quantizedPixels; } }
        public IList<Pixel> Pixels { get { return pixels; } }

        public int PixelsCount { get { return pixels.Length; } }
        public void AddPixel(Pixel pixel, int quantizedPixel)
        {
            pixels[pixelFillingCounter] = pixel;
            quantizedPixels[pixelFillingCounter++] = quantizedPixel;
        }

        private readonly Pixel[] pixels;
        private readonly int[] quantizedPixels;
        private int pixelFillingCounter;
    }
}