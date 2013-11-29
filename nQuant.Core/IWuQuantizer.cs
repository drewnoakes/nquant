using System.Drawing;

namespace nQuant
{
    public interface IWuQuantizer
    {
        Image QuantizeImage(Bitmap image, byte alphaThreshold, byte alphaFader);
    }
}