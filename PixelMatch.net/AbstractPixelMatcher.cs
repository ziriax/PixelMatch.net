// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace StronglyTyped.PixelMatch
{
	public class AbstractPixelMatcher<TRawColor>
		where TRawColor : unmanaged
	{
		/// <summary>
		/// Matching threshold (0 to 1); smaller is more sensitive (default=0.1)
		/// </summary>
		public float Threshold = 0.1f;

		/// <summary>
		/// Ignore differences in anti-aliased pixels? (default=true)
		/// </summary>
		public bool IgnoreAntiAliasedPixels = true;

		private static readonly Vector4 Rgb2Y = new Vector4(0.29889531f, 0.58662247f, 0.11448223f, 0);
		private static readonly Vector4 Rgb2I = new Vector4(0.59597799f, -0.27417610f, -0.32180189f, 0);
		private static readonly Vector4 Rgb2Q = new Vector4(0.21147017f, -0.52261711f, 0.31114694f, 0);
		private static readonly Vector4 Yid2D = new Vector4(0.5053f, 0.299f, 0.1957f, 0);

		/// <summary>
		/// calculate color luminance difference according to the paper "Measuring perceived color difference
		/// using YIQ NTSC transmission color space in mobile applications" by Y. Kotsarenko and F. Ramos
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float ColorDeltaY(Vector4 color1, Vector4 color2)
		{
			var y1 = Vector4.Dot(color1, Rgb2Y);
			var y2 = Vector4.Dot(color2, Rgb2Y);
			var yd = y1 - y2;
			return yd;
		}

		/// <summary>
		/// calculate color difference according to the paper "Measuring perceived color difference
		/// using YIQ NTSC transmission color space in mobile applications" by Y. Kotsarenko and F. Ramos
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float ColorDelta(Vector4 color1, Vector4 color2)
		{
			var y1 = Vector4.Dot(color1, Rgb2Y);
			var y2 = Vector4.Dot(color2, Rgb2Y);
			var yd = y1 - y2;

			var i = Vector4.Dot(color1, Rgb2I) - Vector4.Dot(color2, Rgb2I);
			var q = Vector4.Dot(color1, Rgb2Q) - Vector4.Dot(color2, Rgb2Q);
			var v = new Vector4(yd, i, q, 0);

			var delta = Vector4.Dot(v * v, Yid2D);

			// encode whether the pixel lightens or darkens in the sign
			return y1 > y2 ? -delta : delta;
		}

		/// <summary>
		/// check if a pixel has 3+ adjacent pixels of the same color. 
		/// </summary>
		private bool HasManySiblings(IAbstractImage<TRawColor> img, int x1, int y1, int width, int height)
		{
			var x0 = Math.Max(x1 - 1, 0);
			var y0 = Math.Max(y1 - 1, 0);
			var x2 = Math.Min(x1 + 1, width - 1);
			var y2 = Math.Min(y1 + 1, height - 1);
			var zeroes = x1 == x0 || x1 == x2 || y1 == y0 || y1 == y2 ? 1 : 0;

			var color = img[x1, y1];

			// go through 8 adjacent pixels
			for (var x = x0; x <= x2; x++)
			{
				for (var y = y0; y <= y2; y++)
				{
					if (x == x1 && y == y1)
						continue;

					if (AreEqual(color, img[x, y]))
					{
						zeroes++;
					}

					if (zeroes > 2)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// check if a pixel is likely a part of anti-aliasing;
		/// based on "Anti-aliased Pixel and Intensity Slope Detector" paper by V. Vysniauskas, 2009
		/// </summary>
		private bool IsAntiAliased(IAbstractImage<TRawColor> img1, Vector4 color1, int x1, int y1, int width, int height, IAbstractImage<TRawColor> img2)
		{
			var x0 = Math.Max(x1 - 1, 0);
			var y0 = Math.Max(y1 - 1, 0);
			var x2 = Math.Min(x1 + 1, width - 1);
			var y2 = Math.Min(y1 + 1, height - 1);
			var zeroes = x1 == x0 || x1 == x2 || y1 == y0 || y1 == y2 ? 1 : 0;
			float min = 0;
			float max = 0;
			int minX = 0;
			int minY = 0;
			int maxX = 0;
			int maxY = 0;

			// go through 8 adjacent pixels
			for (var x = x0; x <= x2; x++)
			{
				for (var y = y0; y <= y2; y++)
				{
					if (x == x1 && y == y1)
						continue;

					// brightness delta between the center pixel and adjacent one
					var color2 = img1.Normalized(img1[x, y]);
					var delta = ColorDeltaY(color1, color2);

					// count the number of equal, darker and brighter adjacent pixels
					if (delta == 0)
					{
						zeroes++;
						// if found more than 2 equal siblings, it's definitely not anti-aliasing
						if (zeroes > 2)
							return false;

						// remember the darkest pixel
					}
					else if (delta < min)
					{
						min = delta;
						minX = x;
						minY = y;
						// remember the brightest pixel
					}
					else if (delta > max)
					{
						max = delta;
						maxX = x;
						maxY = y;
					}
				}
			}

			// if there are no both darker and brighter pixels among siblings, it's not anti-aliasing
			if (min == 0 || max == 0)
				return false;

			// if either the darkest or the brightest pixel has 3+ equal siblings in both images
			// (definitely not anti-aliased), this pixel is anti-aliased
			return (HasManySiblings(img1, minX, minY, width, height) && HasManySiblings(img2, minX, minY, width, height)) ||
					 (HasManySiblings(img1, maxX, maxY, width, height) && HasManySiblings(img2, maxX, maxY, width, height));
		}

		/// <summary>
		/// Compare two abstract images
		/// </summary>
		/// <param name="img1">The first image</param>
		/// <param name="img2">The second image</param>
		/// <param name="onDifference">Optional callback to record pixel locations to their differences. Anti-aliased pixels that do not count as a difference also also reported with a zero difference!</param>
		/// <returns>
		/// A number of different pixels
		/// </returns>
		public int Compare(IAbstractImage<TRawColor> img1, IAbstractImage<TRawColor> img2, Action<int, int, float> onDifference = null)
		{
			if (img1.Size != img2.Size)
				throw new ArgumentOutOfRangeException($"Can't compare images with different sizes ({img1.Size} vs {img2.Size}");

			// maximum acceptable square distance between two colors;
			// 35215 is the maximum possible value for the YIQ difference metric using 8-bit colors
			var maxDelta = (35215f / 255f / 255f) * (Threshold * Threshold);
			var diff = 0;
			var aa = !IgnoreAntiAliasedPixels;
			var (width, height) = img1.Size;

			// compare each pixel of one image against the other one
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					// Fast path if pixels are binary equal
					var raw1 = img1[x, y];
					var raw2 = img2[x, y];
					if (AreEqual(raw1, raw2))
						continue;

					var norm1 = img1.Normalized(raw1);
					var norm2 = img2.Normalized(raw2);

					// squared YUV distance between colors at this pixel position, negative if the img2 pixel is darker
					var delta = ColorDelta(norm1, norm2);

					if (Math.Abs(delta) > maxDelta)
					{
						// the color difference is above the threshold
						// check it's a real rendering difference or just anti-aliasing
						if (aa || (!IsAntiAliased(img1, norm1, x, y, width, height, img2) && !IsAntiAliased(img2, norm2, x, y, width, height, img1)))
						{
							// found substantial difference not caused by anti-aliasing
							diff++;
						}
						else
						{
							// we ignore anti-aliased differences
							delta = 0;
						}

						onDifference?.Invoke(x, y, delta);
					}
				}
			}

			// return the number of different pixels
			return diff;
		}

		protected virtual unsafe bool AreEqual(TRawColor color1, TRawColor color2)
		{
			var ptr1 = (byte*)&color1;
			var ptr2 = (byte*)&color2;
			var xor = 0;
			for (int i = 0; i < sizeof(TRawColor); ++i)
			{
				xor |= ptr1[i] ^ ptr2[i];
			}
			return xor == 0;
		}
	}
}
