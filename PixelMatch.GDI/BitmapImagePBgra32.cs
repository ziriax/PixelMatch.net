using System.Drawing;
using System.Drawing.Imaging;

namespace StronglyTyped.PixelMatch
{
	public sealed class BitmapImagePBgra32 : InterleavedImagePBgra32
	{
		private Bitmap _unlockingImage;
		private readonly BitmapData _bitmapData;

		public BitmapImagePBgra32(Bitmap img, Rectangle? region = null)
			: this(img.LockBits(region ?? new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb), img)
		{

		}

		public BitmapImagePBgra32(BitmapData bitmapData, Bitmap unlockingImage = null)
			: base(new RawImageData<uint>((bitmapData.Width, bitmapData.Height), bitmapData.Scan0, bitmapData.Stride))
		{
			_bitmapData = bitmapData;
			_unlockingImage = unlockingImage;
		}

		public override void Dispose()
		{
			_unlockingImage?.UnlockBits(_bitmapData);
			_unlockingImage = null;

			base.Dispose();
		}
	}
}
