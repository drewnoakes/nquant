using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xunit;
using Xunit.Extensions;

namespace nQuant.Facts
{
    public class BasicTests
    {
        [Theory,
        InlineData("topo.png"),
        InlineData("tile.png")]
        public void EndToEndTest(string pngFileName)
        {
            const string testFilePath = "output_nquant.png";
            if (File.Exists(testFilePath)) { File.Delete(testFilePath); }
            var sw = new Stopwatch();
            var originalFilePath = Path.Combine(@"../../../samples", pngFileName);
            using (var bitmap = (Bitmap)Image.FromFile(originalFilePath))
            {
                const int alphaTransparency = 0;
                const int alphaFader = 0;
                var quantizer = new WuQuantizer ();
                sw.Start ();

                using (var quantized = quantizer.QuantizeImage (bitmap, alphaTransparency, alphaFader))
                    quantized.Save (testFilePath, ImageFormat.Png);
            }

            Debug.WriteLine("nQuant: {0} ms/image", sw.ElapsedMilliseconds);
            Assert.True(File.Exists(testFilePath));
            var fileLength = new FileInfo(testFilePath).Length;
            Assert.True(fileLength > 0);
            Assert.True(fileLength < new FileInfo(originalFilePath).Length);
        }         
    }
}