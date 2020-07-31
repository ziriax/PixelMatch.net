using System.Numerics;
using System.Runtime.CompilerServices;

namespace StronglyTyped.PixelMatch
{
	public class InterleavedImagePBgra32 : IAbstractImage<uint>
	{
		private RawImageData<uint> _rawImageData;
		private readonly bool _disposeImageData;

		public InterleavedImagePBgra32(RawImageData<uint> rawImageData, bool disposeImageData = true)
		{
			_rawImageData = rawImageData;
			_disposeImageData = disposeImageData;
		}

		public virtual void Dispose()
		{
			if (_disposeImageData)
			{
				_rawImageData?.Dispose();
			}

			_rawImageData = null;
		}

		public int Width => _rawImageData.Width;
		public int Height => _rawImageData.Height;

		public uint this[int x, int y]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _rawImageData[x, y];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector4 Normalized(uint raw) => PixelConversion.NormalizedPBgra32(raw);
	}
}