using System;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StronglyTyped.PixelMatch
{
	[TestClass]
	public class DiffTests
	{
		private readonly PixelMatcher32 _defaultMatcher = new PixelMatcher32();
		private readonly PixelMatcher32 _preciseMatcher = new PixelMatcher32 { Threshold = 0.05f };
		private readonly PixelMatcher32 _exactMatcher = new PixelMatcher32 { Threshold = 0f };

		private int Compare(string imagePath1, string imagePath2, PixelMatcher32 matcher)
		{
			using var stream1 = File.OpenRead(Path.Combine("fixtures", imagePath1 + ".png"));
			using var stream2 = File.OpenRead(Path.Combine("fixtures", imagePath2 + ".png"));
			using var bitmap1 = new Bitmap(stream1, true);
			using var bitmap2 = new Bitmap(stream2, true);
			using var image1 = new BitmapImagePBgra32(bitmap1);
			using var image2 = new BitmapImagePBgra32(bitmap2);
			return matcher.Compare(image1, image2);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Compare_DifferentImageSizes_Throws() => Assert.AreEqual(0, Compare("1a", "5a", _exactMatcher));

		[TestMethod]
		public void Compare_1A_1A() => Assert.AreEqual(0, Compare("1a", "1a", _exactMatcher));

		[TestMethod]
		public void Compare_6A_6A() => Assert.AreEqual(0, Compare("6a", "6a", _exactMatcher));

		[TestMethod]
		public void Compare_1A_1B() => Assert.AreEqual(143, Compare("1a", "1b", _preciseMatcher));

		[TestMethod]
		public void Compare_2A_2B() => Assert.AreEqual(12437, Compare("2a", "2b", _preciseMatcher));

		[TestMethod]
		public void Compare_3A_3B() => Assert.AreEqual(212, Compare("3a", "3b", _preciseMatcher));

		[TestMethod]
		public void Compare_4A_4B() => Assert.AreEqual(36049, Compare("4a", "4b", _preciseMatcher));

		[TestMethod]
		public void Compare_5A_5B() => Assert.AreEqual(0, Compare("5a", "5b", _preciseMatcher));

		[TestMethod]
		public void Compare_6A_6B() => Assert.AreEqual(51, Compare("6a", "6b", _preciseMatcher));

		[TestMethod]
		public void Compare_7A_7B() => Assert.AreEqual(2448, Compare("7a", "7b", _defaultMatcher));
	}
}
