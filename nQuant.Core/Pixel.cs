using System.Runtime.InteropServices;

namespace nQuant
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Pixel
    {
        public Pixel(byte alpha, byte red, byte green, byte blue) : this()
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        [FieldOffset(0)]
        public readonly byte Alpha;
        [FieldOffset(1)]
        public readonly byte Red;
        [FieldOffset(2)]
        public readonly byte Green;
        [FieldOffset(3)]
        public readonly byte Blue;
        [FieldOffset(0)]
        public readonly int Argb;
    }
}