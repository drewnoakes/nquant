using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace nQuant.Tests
{
    public class BasicTests
    {
        [Test]
        public void OptimizeTest()
        {
            WuQuantizer quantizer = new WuQuantizer ();
            using (Bitmap bitmap = (Bitmap)Bitmap.FromFile (@"../../../samples/topo.png"))
            {
                Stopwatch sw = new Stopwatch ();
                sw.Start ();

                const int Runs = 1;
                for (int i = 0; i < Runs; i++)
                {
                    int alphaTransparency = 0;
                    int alphaFader = 0;
                    using (Image quantized = quantizer.QuantizeImage (bitmap, alphaTransparency, alphaFader))
                        quantized.Save ("output_nQuant.png", ImageFormat.Png);
                }

                Debug.WriteLine ("nQuant: {0} ms/image", sw.ElapsedMilliseconds / Runs);
            }            
        }         
    }
}