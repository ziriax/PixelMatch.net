using System.Numerics;
using System.Runtime.CompilerServices;

namespace StronglyTyped.PixelMatch
{
	public static class PixelConversion
	{
		private const float ByteScale = 1f / 255;
		private const float ColorScale = ByteScale * ByteScale;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe Vector4 NormalizedPBgra32(uint raw)
		{
			var bgra = (byte*)&raw;
			float a = bgra[3] * ColorScale;
			var v = new Vector4(bgra[2], bgra[1], bgra[0], 255f);
			return v * a;
		}
	}
}