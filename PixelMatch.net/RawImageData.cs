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
		public readonly int Width;
		public readonly int Height;

		internal RawImageData(int width, int height, T* pixels, int pixelStride)
		{
			_handle = default;

			Pixels = pixels;
			Stride = pixelStride;

			Width = width;
			Height = height;
		}

		internal RawImageData(int width, int height, GCHandle handle, int pixelStride)
		{
			_handle = handle;

			Pixels = (T*)handle.AddrOfPinnedObject();
			Stride = pixelStride;
			Width = width;
			Height = height;
		}

		~RawImageData()
		{
			// Always dispose, even in finalizer (because GCHandle is a struct, we MUST Free it)
			Dispose();
		}

		public RawImageData(T[,] pixels) : this(
			pixels.GetLength(0), pixels.GetLength(1),
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

	public static unsafe class RawImageData
	{
		public static RawImageData<T> Create<T>(int width, int height, T* pixels, int pixelStride) where T : unmanaged
		{
			return new RawImageData<T>(width, height, pixels, pixelStride);
		}

		public static RawImageData<T> Create<T>(int width, int height, GCHandle handle, int pixelStride)
			where T : unmanaged
		{
			return new RawImageData<T>(width, height, handle, pixelStride);
		}

		public static RawImageData<T> Create<T>(int width, int height, byte* pixels, int byteStride)
			where T : unmanaged
		{
			if (byteStride % sizeof(T) != 0)
				throw new ArgumentException($"{nameof(byteStride)} must be a multiple of {sizeof(T)}");

			return Create(width, height, (T*)pixels, byteStride / sizeof(T));
		}

		public static RawImageData<T> Create<T>(int width, int height, IntPtr pixels, int byteStride)
			where T : unmanaged
		{
			return Create<T>(width, height, (byte*)pixels.ToPointer(), byteStride);
		}
	}
}