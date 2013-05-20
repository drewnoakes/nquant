using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace nQuant
{
    public class nQuant
    {
        private static int alphaTransparency = 10;
        private static int alphaFader = 70;
        private static string targetPath = string.Empty;

        public static void Main(string[] args)
        {
            Console.WriteLine("nQuant Version {0} .net color Quantizer. An adaptation of Xiaolin Wu's fast optimal color quantizer.", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Copyright (C) 2011 Matt Wrock.");

            if(args.Length < 1)
            {
                PrintUsage();
                Environment.Exit(1);
            }
            var sourcePath = args[0];
            ProcessArgs(args);
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine("The source file you specified does not exist.");
                Environment.Exit(1);
            }
            if (string.IsNullOrEmpty(targetPath))
            {
                var lastDot = sourcePath.LastIndexOf('.');
                if (lastDot == -1)
                    lastDot = sourcePath.Length;
                targetPath = sourcePath.Substring(0, lastDot) + "-quant.png";
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            var quantizer = new WuQuantizer();
            using(var bitmap = new Bitmap(sourcePath))
            {
                try
                {
                    using(var quantized = quantizer.QuantizeImage(bitmap, alphaTransparency, alphaFader))
                    {
                        quantized.Save(targetPath, ImageFormat.Png);
                    }
                }
                catch (QuantizationException q)
                {
                    Console.WriteLine(q.Message);
                }
            }
            Console.WriteLine(@"Completed in {0:s\.fff} secs with peak memory usage of {1}.", stopwatch.Elapsed, Process.GetCurrentProcess().PeakWorkingSet64.ToString("#,#"));
        }

        static private void ProcessArgs(string[] args)
        {
            for (var index = 1; index < args.Length; ++index)
            {
                var currentArg = args[index].ToUpper();
                if (currentArg.Length > 1
                  && (currentArg.StartsWith("-", StringComparison.Ordinal)
                  || currentArg.StartsWith("–", StringComparison.Ordinal)
                  || currentArg.StartsWith("/", StringComparison.Ordinal)))
                {
                    currentArg = currentArg.Substring(1);
                    switch (currentArg)
                    {
                        case "T":
                            if (index >= args.Length - 1 || !Int32.TryParse(args[index + 1], out alphaTransparency))
                            {
                               PrintUsage();
                                Environment.Exit(1);
                            }
                            break;

                        case "F":
                            if (index >= args.Length - 1 || !Int32.TryParse(args[index + 1], out alphaFader))
                            {
                                PrintUsage();
                                Environment.Exit(1);
                            }
                            break;

                        case "O":
                            if (index >= args.Length - 1)
                            {
                                PrintUsage();
                                Environment.Exit(1);
                            }
                            else
                                targetPath = args[index + 1];
                            break;

                        default:
                            PrintUsage();
                            Environment.Exit(1);
                            break;
                    }
                }
            }
        }

        static private void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("usage: nQuant <input image path> [options]");
            Console.WriteLine();
            Console.WriteLine("Valid options:");
            Console.WriteLine("  /t : Alpha Thresholds - All colors with an Alpha value equal to or less than this integer will be considered fully transparent. The default is 10.");
            Console.WriteLine("  /f : Alpha Fader - Alpha values will be normalized to the nearest multiple of this value. The default is 70.");
            Console.WriteLine("  /o : Output image file path. The default is <source image path directory>\\<source image file name without extension>-quant.png");
        }

    }
}
