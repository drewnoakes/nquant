using System;
using System.Collections.Generic;

namespace nQuant
{
    public class ColorData
    {
        public ColorData(int dataGranularity, int bitmapWidth, int bitmapHeight)
        {
            dataGranularity++;
            Moments = new ColorMoment[dataGranularity, dataGranularity, dataGranularity, dataGranularity];

            pixelsCount = bitmapWidth*bitmapHeight;
            pixels = new Pixel[pixelsCount];
            quantizedPixels = new Pixel[pixelsCount];
        }

        public ColorMoment[, , ,] Moments { get; private set; }

        public Pixel[] QuantizedPixels { get { return quantizedPixels; } }
        public Pixel[] Pixels { get { return pixels; } }

        public int PixelsCount { get { return pixels.Length; } }
        public void AddPixel(Pixel pixel, Pixel quantizedPixel)
        {
            pixels[pixelFillingCounter] = pixel;
            quantizedPixels[pixelFillingCounter++] = quantizedPixel;
        }

        private Pixel[] pixels;
        private Pixel[] quantizedPixels;
        private int pixelsCount;
        private int pixelFillingCounter;
    }
}