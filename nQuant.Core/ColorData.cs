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
        }

        public ColorMoment[, , ,] Moments { get; private set; }

        public Pixel[] Pixels { get { return pixels; } }

        public void AddPixel(Pixel pixel)
        {
            pixels[pixelFillingCounter++] = pixel;
        }

        private Pixel[] pixels;
        private int pixelsCount;
        private int pixelFillingCounter;
    }
}