using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace nQuant
{
    public class WuQuantizerParallel : WuQuantizerBase, IWuQuantizer
    {
        public WuQuantizerParallel()
        {
        }

        public WuQuantizerParallel(ParallelOptions parallelOptions)
        {
            this.parallelOptions = parallelOptions;
        }

        protected override QuantizedPalette GetQuantizedPalette(int colorCount, ColorData data, IEnumerable<Box> cubes, int alphaThreshold)
        {
            int imageSize = data.PixelsCount;
            LookupData lookups = BuildLookups(cubes, data);

            for (var index = 0; index < imageSize; ++index)
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

            int lookupsCount = lookups.Lookups.Count;

            Parallel.ForEach(
                data.Pixels,
                (pixel, state, pixelIndex) =>
                    {
                        palette.PixelIndex[pixelIndex] = -1;
                        if (pixel.Alpha <= alphaThreshold)
                            return;

                        int match = data.QuantizedPixels[(int)pixelIndex];
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

                        palette.PixelIndex[pixelIndex] = bestMatch;

                        Interlocked.Add(ref alphas[bestMatch], pixel.Alpha);
                        Interlocked.Add(ref reds[bestMatch], pixel.Red);
                        Interlocked.Add(ref greens[bestMatch], pixel.Green);
                        Interlocked.Add(ref blues[bestMatch], pixel.Blue);
                        Interlocked.Increment(ref sums[bestMatch]);
                    });

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

        private ParallelOptions parallelOptions = new ParallelOptions();
    }
}