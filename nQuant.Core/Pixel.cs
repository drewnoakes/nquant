using System.Runtime.InteropServices;
namespace nQuant
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Pixel
    {
        public Pixel(byte alpha, byte red, byte green, byte blue)
        {
            Argb = 0;
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;

            System.Diagnostics.Debug.Assert(Argb == (alpha << 24 | red << 16 | green << 8 | blue));
        }

        public Pixel(int argb)
            : this()
        {
            Argb = argb;
            System.Diagnostics.Debug.Assert(Alpha == (argb >> 24));
            System.Diagnostics.Debug.Assert(Red == ((argb >> 16) & 255));
            System.Diagnostics.Debug.Assert(Green == ((argb >> 8) & 255));
            System.Diagnostics.Debug.Assert(Blue == (argb & 255));
        }

        [FieldOffsetAttribute(3)]
        public byte Alpha;
        [FieldOffsetAttribute(2)]
        public byte Red;
        [FieldOffsetAttribute(1)]
        public byte Green;
        [FieldOffsetAttribute(0)]
        public byte Blue;
        [FieldOffsetAttribute(0)]
        public int Argb;

        public long Distance()
        {
            return (Alpha * Alpha) + (Red * Red) + (Green * Green) + (Blue * Blue);
        }
    }
}