using System;
using System.Collections.Generic;
using System.Drawing;

namespace nQuant
{
    public class WuQuantizer : WuQuantizerBase, IWuQuantizer
    {
        protected override QuantizedPalette GetQuantizedPalette(int colorCount, ColorData data, IEnumerable<Box> cubes, byte alphaThreshold)
        {
            int imageSize = data.PixelsCount;
            List<Lookup> lookups = BuildLookups(cubes, data);

            Array.Clear(data.QuantizedPixels, 0, data.QuantizedPixels.Length);
            var quantizedPixels = data.QuantizedPixels;

            var alphas = new int[colorCount + 1];
            var reds = new int[colorCount + 1];
            var greens = new int[colorCount + 1];
            var blues = new int[colorCount + 1];
            var sums = new int[colorCount + 1];
            var palette = new QuantizedPalette(imageSize, colorCount + 1);

            Pixel[] pixels = data.Pixels;
            int pixelsCount = data.PixelsCount;
            int lookupsCount = lookups.Count;

            var cachedMatches = new Dictionary<int, int>();

            for (int pixelIndex = 0; pixelIndex < pixelsCount; pixelIndex++)
            {
                Pixel pixel = pixels[pixelIndex];
                palette.PixelIndex[pixelIndex] = -1;
                if (pixel.Alpha <= alphaThreshold)
                    continue;

                int bestMatch;
                int argb = pixel.Argb;

                if (!cachedMatches.TryGetValue(argb, out bestMatch))
                {
                    int match = quantizedPixels[pixelIndex].Argb;
                    bestMatch = match;
                    int bestDistance = int.MaxValue;

                    for (int lookupIndex = 0; lookupIndex < lookupsCount; lookupIndex++)
                    {
                        Lookup lookup = lookups[lookupIndex];
                        var deltaAlpha = pixel.Alpha - lookup.Alpha;
                        var deltaRed = pixel.Red - lookup.Red;
                        var deltaGreen = pixel.Green - lookup.Green;
                        var deltaBlue = pixel.Blue - lookup.Blue;

                        int distance = deltaAlpha*deltaAlpha + deltaRed*deltaRed + deltaGreen*deltaGreen + deltaBlue*deltaBlue;

                        if (distance >= bestDistance)
                            continue;

                        bestDistance = distance;
                        bestMatch = lookupIndex;
                    }

                    cachedMatches[argb] = bestMatch;
                }

                alphas[bestMatch] += pixel.Alpha;
                reds[bestMatch] += pixel.Red;
                greens[bestMatch] += pixel.Green;
                blues[bestMatch] += pixel.Blue;
                sums[bestMatch]++;

                palette.PixelIndex[pixelIndex] = bestMatch;
            }

            for (var paletteIndex = 0; paletteIndex < colorCount; paletteIndex++)
            {
                if (sums[paletteIndex] > 0)
                {
                    alphas[paletteIndex] /= sums[paletteIndex];
                    reds[paletteIndex] /= sums[paletteIndex];
                    greens[paletteIndex] /= sums[paletteIndex];
                    blues[paletteIndex] /= sums[paletteIndex];
                }

                var color = Color.FromArgb(alphas[paletteIndex], reds[paletteIndex], greens[paletteIndex], blues[paletteIndex]);
                palette.Colors[paletteIndex] = color;
            }

            palette.Colors[colorCount] = Color.FromArgb(0, 0, 0, 0);

            return palette;
        }
    }
}
