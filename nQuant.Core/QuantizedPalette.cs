using System.Drawing;

namespace nQuant
{
    public class QuantizedPalette
    {
        public QuantizedPalette(int size, int colorCount)
        {
            Colors = new Color[colorCount];
            PixelIndex = new int[size];
        }

        public Color[] Colors { get; private set; }
        public int[] PixelIndex { get; private set; }
    }
}