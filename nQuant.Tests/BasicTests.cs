using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using NUnit.Framework;

namespace nQuant.Tests
{
    public class BasicTests
    {
        [Test]
        public void OptimizeTest()
        {
            const int Runs = 1;

            using (Bitmap bitmap = (Bitmap)Bitmap.FromFile (@"../../../samples/topo.png"))
            {
                int alphaTransparency = 0;
                int alphaFader = 0;

                Stopwatch sw = new Stopwatch ();

                IWuQuantizer quantizer = new WuQuantizer ();
                sw.Start ();

                for (int i = 0; i < Runs; i++)
                    using (Image quantized = quantizer.QuantizeImage (bitmap, alphaTransparency, alphaFader))
                        quantized.Save ("output_nQuant.png", ImageFormat.Png);

                Debug.WriteLine ("nQuant: {0} ms/image", sw.ElapsedMilliseconds / Runs);

                quantizer = new WuQuantizerParallel();
                sw.Restart();

                for (int i = 0; i < Runs; i++)
                    using (Image quantized = quantizer.QuantizeImage (bitmap, alphaTransparency, alphaFader))
                        quantized.Save ("output_nQuant_parallel.png", ImageFormat.Png);

                Debug.WriteLine ("nQuant parallel: {0} ms/image", sw.ElapsedMilliseconds / Runs);

                ParallelOptions parallelOptions = new ParallelOptions();
                parallelOptions.MaxDegreeOfParallelism = 2;
                quantizer = new WuQuantizerParallel (parallelOptions);
                sw.Restart ();

                for (int i = 0; i < Runs; i++)
                    using (Image quantized = quantizer.QuantizeImage (bitmap, alphaTransparency, alphaFader))
                        quantized.Save ("output_nQuant_parallel_2.png", ImageFormat.Png);

                Debug.WriteLine ("nQuant parallel (2 parallels): {0} ms/image", sw.ElapsedMilliseconds / Runs);
            }            
        }         
    }
}