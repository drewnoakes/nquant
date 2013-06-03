using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace nQuant
{
    public class WuQuantizer : WuQuantizerBase, IWuQuantizer
    {
        private IEnumerable<byte> indexedPixels(ImageBuffer image, Lookup[] lookups, int alphaThreshold, PaletteBuffer paletteBuffer)
        {
            int pixelsCount = image.Image.Width * image.Image.Height;

            var alphas = paletteBuffer.Alphas;
            var reds = paletteBuffer.Reds;
            var greens = paletteBuffer.Greens;
            var blues = paletteBuffer.Blues;
            var sums = paletteBuffer.Sums;

            Dictionary<int, byte> cachedMaches = new Dictionary<int, byte>();
            foreach (Pixel pixel in image.Pixels)
            {
                byte bestMatch = AlphaColor;
                if (pixel.Alpha > alphaThreshold)
                {
                    int argb = pixel.Argb;

                    if (!cachedMaches.TryGetValue(argb, out bestMatch))
                    {
                        int bestDistance = int.MaxValue;

                        for (int lookupIndex = 0; lookupIndex < lookups.Length; lookupIndex++)
                        {
                            Lookup lookup = lookups[lookupIndex];
                            var deltaAlpha = pixel.Alpha - lookup.Alpha;
                            var deltaRed = pixel.Red - lookup.Red;
                            var deltaGreen = pixel.Green - lookup.Green;
                            var deltaBlue = pixel.Blue - lookup.Blue;

                            int distance = deltaAlpha * deltaAlpha + deltaRed * deltaRed + deltaGreen * deltaGreen + deltaBlue * deltaBlue;

                            if (distance >= bestDistance)
                                continue;

                            bestDistance = distance;
                            bestMatch = (byte)lookupIndex;
                        }

                        cachedMaches[argb] = bestMatch;
                    }

                    alphas[bestMatch] += pixel.Alpha;
                    reds[bestMatch] += pixel.Red;
                    greens[bestMatch] += pixel.Green;
                    blues[bestMatch] += pixel.Blue;
                    sums[bestMatch]++;
                }
                yield return bestMatch;
            }
        }

        internal override Image GetQuantizedImage(ImageBuffer image, int colorCount, Lookup[] lookups, int alphaThreshold)
        {
            var result = new Bitmap(image.Image.Width, image.Image.Height, PixelFormat.Format8bppIndexed);
            var resultBuffer = new ImageBuffer(result);
            PaletteBuffer paletteBuffer = new PaletteBuffer(colorCount);
            resultBuffer.UpdatePixelIndexes(indexedPixels(image,lookups, alphaThreshold, paletteBuffer));
            result.Palette = paletteBuffer.BuildPalette(result.Palette);
            return result;
        }
    }
}
