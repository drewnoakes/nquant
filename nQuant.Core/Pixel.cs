namespace nQuant
{
    internal struct Pixel
    {
        public Pixel(byte alpha, byte red, byte green, byte blue) : this()
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public byte Alpha;
        public byte Red;
        public byte Green;
        public byte Blue;
    }
}