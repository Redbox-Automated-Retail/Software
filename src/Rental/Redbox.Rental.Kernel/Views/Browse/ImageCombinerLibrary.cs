using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Redbox.Rental.Kernel.Views.Browse;

public static class ImageCombinerLibrary
{
	public static Bitmap? BuildDynamicBrowseImage(Image[] imageList)
	{
		if (imageList == null || imageList.Length != 3)
		{
			return null;
		}
		int coverWidth = 262;
		int coverHeight = 370;
		_ = Convert.ToDouble(coverWidth) / (double)coverHeight;
		double verticalOffsetRatio = 0.0424;
		double borderRatio = 0.01;
		int horizontalOffset = (int)Math.Round(0.086 * (double)coverWidth);
		int verticalOffset = (int)Math.Round(verticalOffsetRatio * (double)coverHeight);
		int borderWidth = (int)Math.Round(borderRatio * (double)coverWidth);
		int totalBorderWidth = borderWidth * (2 + imageList.Length - 1);
		int width = coverWidth + totalBorderWidth + horizontalOffset * 3;
		int height = coverHeight + totalBorderWidth + verticalOffset * 2;
		Bitmap bitmap = new Bitmap(width, height);
		Graphics graphics = Graphics.FromImage(bitmap);
		Bitmap bitmap2 = DarkenImage(SetBorder(ScaleImage(imageList[2], coverWidth, coverHeight), borderWidth), 0.6f);
		graphics.DrawImage(bitmap2, new Point
		{
			X = bitmap.Width - bitmap2.Width,
			Y = 0
		});
		Bitmap bitmap3 = DarkenImage(SetBorder(ScaleImage(imageList[1], coverWidth, coverHeight), borderWidth), 0.3f);
		graphics.DrawImage(bitmap3, new Point
		{
			X = bitmap.Width - horizontalOffset - bitmap3.Width,
			Y = (bitmap.Height - bitmap3.Height) / 2
		});
		Image image = ScaleImage(imageList[0], coverWidth, coverHeight);
		graphics.DrawImage(SetBorder(image, borderWidth), new Point
		{
			X = 0,
			Y = bitmap.Height - image.Height - 2 * borderWidth
		});
		return bitmap;
	}

	public static Image ScaleImage(Image image, int width, int height)
	{
		if (image == null)
		{
			throw new ArgumentNullException("Image is null.");
		}
		if (width <= 0 || height <= 0)
		{
			throw new ArgumentOutOfRangeException("Width or Height less than or equal to 0.");
		}
		if (image.Width == width && image.Height == height)
		{
			return image;
		}
		return new Bitmap(image, new Size
		{
			Width = width,
			Height = height
		});
	}

	public static Bitmap SetBorder(Image image, int borderWidth)
	{
		if (image == null)
		{
			throw new ArgumentNullException("Image is null.");
		}
		if (borderWidth <= 0)
		{
			throw new ArgumentOutOfRangeException("borderWidth less than or equal to 0.");
		}
		Bitmap bitmap = new Bitmap(image.Width + 2 * borderWidth, image.Height + 2 * borderWidth);
		using Graphics graphics = Graphics.FromImage(bitmap);
		Pen pen = new Pen(new SolidBrush(Color.FromArgb(60, 56, 57)), borderWidth);
		pen.Alignment = PenAlignment.Inset;
		graphics.DrawImage(image, new Rectangle(borderWidth, borderWidth, image.Width + borderWidth, image.Height + borderWidth));
		graphics.DrawRectangle(pen, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
		return bitmap;
	}

	public static Bitmap DarkenImage(Image image, float opacity)
	{
		if (image == null)
		{
			throw new ArgumentNullException("Image is null.");
		}
		if (opacity < 0f || opacity > 1f)
		{
			throw new ArgumentOutOfRangeException("Opacity needs to be between 0 and 1 inclusive.");
		}
		Bitmap bitmap = new Bitmap(image);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
		graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)Math.Ceiling(opacity * 255f), 0, 0, 0)), new Rectangle(0, 0, image.Width, image.Height));
		return bitmap;
	}

	public static Bitmap SetImageOpacity(Image image, float opacity)
	{
		if (image == null)
		{
			throw new ArgumentNullException("Image is null.");
		}
		if (opacity <= 0f)
		{
			throw new ArgumentOutOfRangeException("Opacity needs to be between 0 and 1 inclusive.");
		}
		Bitmap bitmap = new Bitmap(image.Width, image.Height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		ColorMatrix colorMatrix = new ColorMatrix();
		colorMatrix.Matrix33 = opacity;
		ImageAttributes imageAttributes = new ImageAttributes();
		imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		graphics.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
		return bitmap;
	}
}
