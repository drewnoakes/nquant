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
            int pixelsCount = data.PixelsCount;
            IList<Pixel> pixels = data.Pixels;

            LookupData lookups = BuildLookups(cubes, data);

            for (var index = 0; index < pixelsCount; ++index)
            {
                var indexParts = BitConverter.GetBytes(data.QuantizedPixels[index]);
                data.QuantizedPixels[index] = lookups.Tags[indexParts[Alpha], indexParts[Red], indexParts[Green], indexParts[Blue]];
            }

            var alphas = new int[colorCount + 1];
            var reds = new int[colorCount + 1];
            var greens = new int[colorCount + 1];
            var blues = new int[colorCount + 1];
            var sums = new int[colorCount + 1];
            var palette = new QuantizedPalette(pixelsCount);

            IList<Lookup> lookupsList = lookups.Lookups;
            int lookupsCount = lookupsList.Count;

            // split processing into batches 
            int parallels = parallelOptions.MaxDegreeOfParallelism > 0 ? parallelOptions.MaxDegreeOfParallelism : Environment.ProcessorCount;
            int pixelsPerFrame = pixelsCount / parallels;
            // make sure we don't run on too small frames
            pixelsPerFrame = Math.Max (10000, pixelsPerFrame);
            int totalFrames = (int)Math.Ceiling (((double)pixelsCount) / pixelsPerFrame);

            Parallel.For(
                0,
                totalFrames,
                parallelOptions,
                frame =>
                    {
                        Dictionary<int, int> cachedMaches = new Dictionary<int, int> ();

                        int[] alphasLocal = new int[colorCount + 1];
                        int[] redsLocal = new int[colorCount + 1];
                        int[] greensLocal = new int[colorCount + 1];
                        int[] bluesLocal = new int[colorCount + 1];
                        int[] sumsLocal = new int[colorCount + 1];

                        int startPixelIndex = frame*pixelsPerFrame;
                        int endPixelIndex = Math.Min (pixelsCount, (frame + 1) * pixelsPerFrame);

                        for (int pixelIndex = startPixelIndex; pixelIndex < endPixelIndex; pixelIndex++)
                        {
                            Pixel pixel = pixels[pixelIndex];

                            palette.PixelIndex[pixelIndex] = -1;
                            if (pixel.Alpha <= alphaThreshold)
                                return;

                            int bestMatch;
                            int argb = pixel.Argb;

                            if (!cachedMaches.TryGetValue(argb, out bestMatch))
                            {
                                int match = data.QuantizedPixels[pixelIndex];
                                bestMatch = match;
                                int bestDistance = int.MaxValue;

                                for (int lookupIndex = 0; lookupIndex < lookupsCount; lookupIndex++)
                                {
                                    Lookup lookup = lookupsList[lookupIndex];
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

                                cachedMaches[argb] = bestMatch;
                            }

                            alphasLocal[bestMatch] += pixel.Alpha;
                            redsLocal[bestMatch] += pixel.Red;
                            greensLocal[bestMatch] += pixel.Green;
                            bluesLocal[bestMatch] += pixel.Blue;
                            sumsLocal[bestMatch]++;

                            palette.PixelIndex[pixelIndex] = bestMatch;
                        }

                        lock (this)
                        {
                            for (int i = 0; i < colorCount + 1; i++)
                            {
                                alphas[i] += alphasLocal[i];
                                reds[i] += redsLocal[i];
                                greens[i] += greensLocal[i];
                                blues[i] += bluesLocal[i];
                                sums[i] += sumsLocal[i];
                                //Interlocked.Add(ref alphas[i], alphasLocal[i]);
                                //Interlocked.Add(ref reds[i], redsLocal[i]);
                                //Interlocked.Add(ref greens[i], greensLocal[i]);
                                //Interlocked.Add(ref blues[i], bluesLocal[i]);
                                //Interlocked.Add(ref sums[i], sumsLocal[i]);
                            }
                        }
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