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
        public byte Alpha;
        [FieldOffset(1)]
        public byte Red;
        [FieldOffset(2)]
        public byte Green;
        [FieldOffset(3)]
        public byte Blue;
        [FieldOffset(0)]
        public int Argb;
    }
}