using System.Collections.Generic;
using System.Drawing;

namespace nQuant
{
    internal class QuantizedPalette
    {
        public QuantizedPalette(int size)
        {
            Colors = new List<Color>();
            PixelIndex = new int[size];
        }
        public IList<Color> Colors { get; private set; }
        public int[] PixelIndex { get; private set; }
    }
}