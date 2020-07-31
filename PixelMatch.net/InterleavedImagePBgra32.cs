using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelMatch.net
{
	public class InterleavedImagePBgra32 : IAbstractImage<uint>
	{
		private RawImageData<uint> _rawImageData;

		public InterleavedImagePBgra32(RawImageData<uint> rawImageData)
		{
			_rawImageData = rawImageData;
		}

		public bool IsDisposed => _rawImageData == null;

		public virtual void Dispose()
		{
			_rawImageData?.Dispose();
			_rawImageData = null;
		}

		public (int width, int height) Size => _rawImageData.Size;

		public uint this[int x, int y]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _rawImageData[x, y];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector4 Normalized(uint raw) => PixelConversion.NormalizedPBgra32(raw);
	}
}