namespace StronglyTyped.PixelMatch
{
	/// <summary>
	/// Pixel matcher for 32-bit colors.
	/// </summary>
	public sealed class PixelMatcher32 : AbstractPixelMatcher<uint>
	{
		protected override bool AreEqual(uint color1, uint color2) => color1 == color2;
	}
}