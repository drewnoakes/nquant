using System.Drawing;

namespace nQuant
{
    struct PaletteColorHistory
    {
        public ulong Alpha;
        public ulong Red;
        public ulong Green;
        public ulong Blue;
        public ulong Sum;

        public Color ToNormalizedColor()
        {
            return (Sum != 0) ? Color.FromArgb((int)(Alpha /= Sum), (int)(Red /= Sum), (int)(Green /= Sum), (int)(Blue /= Sum)) : Color.Empty;
        }

        public void AddPixel(Pixel pixel)
        {
            Alpha += pixel.Alpha;
            Red += pixel.Red;
            Green += pixel.Green;
            Blue += pixel.Blue;
            Sum++;
        }
    }
}
