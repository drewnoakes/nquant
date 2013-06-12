using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace nQuant
{
    public class WuQuantizer : WuQuantizerBase, IWuQuantizer
    {
        private IEnumerable<byte> indexedPixels(ImageBuffer image, List<Pixel> lookups, int alphaThreshold, PaletteBuffer paletteBuffer)
        {
            int pixelsCount = image.Image.Width * image.Image.Height;

            var alphas = paletteBuffer.Alphas;
            var reds = paletteBuffer.Reds;
            var greens = paletteBuffer.Greens;
            var blues = paletteBuffer.Blues;
            var sums = paletteBuffer.Sums;

            PaletteLookup lookup = new PaletteLookup(lookups);
            foreach (var pixelLine in image.PixelLines)
            {
                for (int pixelIndex = 0; pixelIndex < pixelLine.Length; pixelIndex++)
                {
                    Pixel pixel = pixelLine[pixelIndex];
                    byte bestMatch = AlphaColor;
                    if (pixel.Alpha > alphaThreshold)
                    {
                        bestMatch = lookup.GetPaletteIndex(pixel);

                        alphas[bestMatch] += pixel.Alpha;
                        reds[bestMatch] += pixel.Red;
                        greens[bestMatch] += pixel.Green;
                        blues[bestMatch] += pixel.Blue;
                        sums[bestMatch]++;
                    }
                    yield return bestMatch;
                }
            }
        }

        internal override Image GetQuantizedImage(ImageBuffer image, int colorCount, List<Pixel> lookups, int alphaThreshold)
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
