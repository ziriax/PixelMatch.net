using System;
using System.Runtime.InteropServices;

namespace StronglyTyped.PixelMatch
{
	/// <summary>
	/// Wraps a memory region of pixels
	/// </summary>
	/// <remarks>
	/// When passing an array, this pins the array in memory, so make sure to dispose asap!
	/// </remarks>
	public sealed unsafe class RawImageData<T> : IDisposable where T : unmanaged
	{
		private GCHandle _handle;

		public readonly T* Pixels;
		public readonly int Stride;
		public readonly (int, int) Size;

		public RawImageData((int width, int height) size, T* pixels, int pixelStride)
		{
			_handle = default;

			Pixels = pixels;
			Stride = pixelStride;
			Size = size;
		}

		public RawImageData((int width, int height) size, byte* pixels, int byteStride) : this(size, (T*)pixels, byteStride >> 2)
		{
			if (byteStride % 4 != 0)
				throw new ArgumentException($"{nameof(byteStride)} must be a multiple of 4");
		}

		public RawImageData((int width, int height) size, IntPtr pixels, int byteStride) : this(size, (byte*)pixels.ToPointer(), byteStride)
		{
		}

		public RawImageData((int width, int height) size, GCHandle handle, int pixelStride)
		{
			_handle = handle;

			Pixels = (T*)handle.AddrOfPinnedObject();
			Stride = pixelStride;
			Size = size;
		}

		~RawImageData()
		{
			// Always dispose, even in finalizer (because GCHandle is a struct, we MUST Free it)
			Dispose();
		}

		public RawImageData(T[,] pixels) : this(
			(pixels.GetLength(0), pixels.GetLength(1)),
			GCHandle.Alloc(pixels, GCHandleType.Pinned),
			pixels.GetLength(0))
		{
		}

		public T this[int x, int y] => Pixels[x + y * Stride];

		public void Dispose()
		{
			GC.SuppressFinalize(this);

			if (_handle.IsAllocated)
			{
				_handle.Free();
				_handle = default;
			}
		}
	}
}