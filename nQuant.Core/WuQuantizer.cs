using System;
using System.Collections.Generic;
using System.Drawing;

namespace nQuant
{
    public class WuQuantizer : WuQuantizerBase, IWuQuantizer
    {
        protected override QuantizedPalette GetQuantizedPalette(int colorCount, ColorData data, IEnumerable<Box> cubes, int alphaThreshold)
        {
            var imageSize = data.Pixels.Count;
            LookupData lookups = BuildLookups(cubes, data);

            for(var index = 0; index < imageSize; ++index)
            {
                var indexParts = BitConverter.GetBytes(data.QuantizedPixels[index]);
                data.QuantizedPixels[index] = lookups.Tags[indexParts[Alpha], indexParts[Red], indexParts[Green], indexParts[Blue]];
            }

            var alphas = new int[colorCount + 1];
            var reds = new int[colorCount + 1];
            var greens = new int[colorCount + 1];
            var blues = new int[colorCount + 1];
            var sums = new int[colorCount + 1];
            var palette = new QuantizedPalette(imageSize);

            int pixelsCount = data.Pixels.Count;
            int lookupsCount = lookups.Lookups.Count;

            for (int pixelIndex = 0; pixelIndex < pixelsCount; pixelIndex++)
            {
                Pixel pixel = data.Pixels[pixelIndex];
                palette.PixelIndex[pixelIndex] = -1;
                if (pixel.Alpha <= alphaThreshold)
                    continue;

                int match = data.QuantizedPixels[pixelIndex];
                int bestMatch = match;
                int bestDistance = int.MaxValue;

                for (int lookupIndex = 0; lookupIndex < lookupsCount; lookupIndex++)
                {
                    Lookup lookup = lookups.Lookups[lookupIndex];
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
                palette.Colors.Add(color);
            }

            palette.Colors.Add(Color.FromArgb(0, 0, 0, 0));

            return palette;
        }
    }
}
