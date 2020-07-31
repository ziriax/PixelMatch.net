using System;
using System.Numerics;

namespace StronglyTyped.PixelMatch
{
	public interface IAbstractImage<TRawColor> : IDisposable
		where TRawColor : unmanaged
	{
		(int width, int height) Size { get; }

		/// <summary>
		/// Returns a raw color value
		/// </summary>
		TRawColor this[int x, int y] { get; }

		/// <summary>
		/// Converts a raw to normalized, alpha-premultiplied color, with components between 0 and 1
		/// </summary>
		Vector4 Normalized(TRawColor raw);
	}
}