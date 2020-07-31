using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace StronglyTyped.PixelMatch
{
	class Program
	{
		static int Main(string[] args)
		{
			RootCommand rootCommand = new RootCommand("Compares two images using the pixelmatch algorithm (https://github.com/mapbox/pixelmatch).")
			{
				new Option<float>(new []{"--threshold", "-t"}, () => 0.1f)
				{
					IsRequired =  false,
					Description = "The threshold between 0 and 1. Default is 0.1"
				},

				new Option<bool>(new []{"--include-anti-aliased-pixels", "-iaa"})
				{
					IsRequired =  false,
					Description = "Include anti-aliased pixels as regular pixels? By default anti-aliased pixels are ignored",
				},

				new Option<bool>(new []{"--skip-color-correction", "-scc"} )
				{
					IsRequired =  false,
					Description = "Skip color correction when loading the images? By default color correction is applied"
				},

				new Argument<FileInfo>("imagePath1")
				{
					Description = "The path of the first image to compare",
					Arity = ArgumentArity.ExactlyOne
				},

				new Argument<FileInfo>("imagePath2")
				{
					Description = "The path of the second image to compare",
					Arity = ArgumentArity.ExactlyOne
				},
			};

			rootCommand.Handler = CommandHandler.Create((float threshold, bool skipColorCorrection, bool includeAntiAliasedPixels, FileInfo imagePath1, FileInfo imagePath2) =>
			{
				using var stream1 = imagePath1.OpenRead();
				using var stream2 = imagePath2.OpenRead();
				using var bitmap1 = new Bitmap(stream1, !skipColorCorrection);
				using var bitmap2 = new Bitmap(stream2, !skipColorCorrection);
				using var image1 = new BitmapImagePBgra32(bitmap1);
				using var image2 = new BitmapImagePBgra32(bitmap2);
				var matcher = new PixelMatcher32
				{
					Threshold = threshold,
					IgnoreAntiAliasedPixels = !includeAntiAliasedPixels
				};

				var area = bitmap1.Width * bitmap1.Height;
				var sw = new Stopwatch();
				sw.Start();
				var count = matcher.Compare(image1, image2);
				var percentage = count * 100D / area;
				var ms = sw.ElapsedMilliseconds;
				Console.WriteLine($"matched in: {ms}ms");
				Console.WriteLine($"different pixels: {count}");
				Console.WriteLine($"error: {percentage:0.00}%");
				return (int)Math.Ceiling(percentage);
			});

			return rootCommand.InvokeAsync(args).Result;
		}
	}
}
