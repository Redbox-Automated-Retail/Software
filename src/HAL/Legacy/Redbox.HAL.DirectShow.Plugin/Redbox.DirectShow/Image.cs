using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Redbox.DirectShow;

public static class Image
{
	public static bool IsGrayscale(Bitmap image)
	{
		bool result = false;
		if (image.PixelFormat == PixelFormat.Format8bppIndexed)
		{
			result = true;
			ColorPalette palette = image.Palette;
			for (int i = 0; i < 256; i++)
			{
				Color color = palette.Entries[i];
				if (color.R != i || color.G != i || color.B != i)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	public static Bitmap CreateGrayscaleImage(int width, int height)
	{
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
		SetGrayscalePalette(bitmap);
		return bitmap;
	}

	public static void SetGrayscalePalette(Bitmap image)
	{
		if (image.PixelFormat != PixelFormat.Format8bppIndexed)
		{
			throw new UnsupportedImageFormatException("Source image is not 8 bpp image.");
		}
		ColorPalette palette = image.Palette;
		for (int i = 0; i < 256; i++)
		{
			ref Color reference = ref palette.Entries[i];
			reference = Color.FromArgb(i, i, i);
		}
		image.Palette = palette;
	}

	public static Bitmap Clone(Bitmap source, PixelFormat format)
	{
		if (source.PixelFormat == format)
		{
			return Clone(source);
		}
		int width = source.Width;
		int height = source.Height;
		Bitmap bitmap = new Bitmap(width, height, format);
		Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(source, 0, 0, width, height);
		graphics.Dispose();
		return bitmap;
	}

	public static Bitmap Clone(Bitmap source)
	{
		BitmapData bitmapData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);
		Bitmap bitmap = Clone(bitmapData);
		source.UnlockBits(bitmapData);
		if (source.PixelFormat == PixelFormat.Format1bppIndexed || source.PixelFormat == PixelFormat.Format4bppIndexed || source.PixelFormat == PixelFormat.Format8bppIndexed || source.PixelFormat == PixelFormat.Indexed)
		{
			ColorPalette palette = source.Palette;
			ColorPalette palette2 = bitmap.Palette;
			int length = palette.Entries.Length;
			for (int i = 0; i < length; i++)
			{
				ref Color reference = ref palette2.Entries[i];
				reference = palette.Entries[i];
			}
			bitmap.Palette = palette2;
		}
		return bitmap;
	}

	public static Bitmap Clone(BitmapData sourceData)
	{
		int width = sourceData.Width;
		int height = sourceData.Height;
		Bitmap bitmap = new Bitmap(width, height, sourceData.PixelFormat);
		BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
		SystemTools.CopyUnmanagedMemory(bitmapData.Scan0, sourceData.Scan0, height * sourceData.Stride);
		bitmap.UnlockBits(bitmapData);
		return bitmap;
	}

	public static Bitmap FromFile(string fileName)
	{
		Bitmap bitmap = null;
		FileStream fileStream = null;
		try
		{
			fileStream = File.OpenRead(fileName);
			MemoryStream memoryStream = new MemoryStream();
			byte[] buffer = new byte[10000];
			while (true)
			{
				int bytesRead = fileStream.Read(buffer, 0, 10000);
				if (bytesRead == 0)
				{
					break;
				}
				memoryStream.Write(buffer, 0, bytesRead);
			}
			return (Bitmap)System.Drawing.Image.FromStream(memoryStream);
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream.Dispose();
			}
		}
	}

	public unsafe static Bitmap Convert16bppTo8bpp(Bitmap bimap)
	{
		Bitmap newImage = null;
		int layers = 0;
		int width = bimap.Width;
		int height = bimap.Height;
		switch (bimap.PixelFormat)
		{
		case PixelFormat.Format16bppGrayScale:
			newImage = CreateGrayscaleImage(width, height);
			layers = 1;
			break;
		case PixelFormat.Format48bppRgb:
			newImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			layers = 3;
			break;
		case PixelFormat.Format64bppArgb:
			newImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			layers = 4;
			break;
		case PixelFormat.Format64bppPArgb:
			newImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
			layers = 4;
			break;
		default:
			throw new UnsupportedImageFormatException("Invalid pixel format of the source image.");
		}
		BitmapData sourceData = bimap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bimap.PixelFormat);
		BitmapData newData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, newImage.PixelFormat);
		byte* basePtr = (byte*)sourceData.Scan0.ToPointer();
		byte* newBasePtr = (byte*)newData.Scan0.ToPointer();
		int srcStride = sourceData.Stride;
		int dstStride = newData.Stride;
		for (int i = 0; i < height; i++)
		{
			ushort* src = (ushort*)(basePtr + (nint)i * (nint)srcStride);
			byte* dst = newBasePtr + (nint)i * (nint)dstStride;
			int x = 0;
			int lineSize = width * layers;
			while (x < lineSize)
			{
				*dst = (byte)(*src >> 8);
				x++;
				src++;
				dst++;
			}
		}
		bimap.UnlockBits(sourceData);
		newImage.UnlockBits(newData);
		return newImage;
	}

	public unsafe static Bitmap Convert8bppTo16bpp(Bitmap bimap)
	{
		Bitmap newImage = null;
		int layers = 0;
		int width = bimap.Width;
		int height = bimap.Height;
		switch (bimap.PixelFormat)
		{
		case PixelFormat.Format8bppIndexed:
			newImage = new Bitmap(width, height, PixelFormat.Format16bppGrayScale);
			layers = 1;
			break;
		case PixelFormat.Format24bppRgb:
			newImage = new Bitmap(width, height, PixelFormat.Format48bppRgb);
			layers = 3;
			break;
		case PixelFormat.Format32bppArgb:
			newImage = new Bitmap(width, height, PixelFormat.Format64bppArgb);
			layers = 4;
			break;
		case PixelFormat.Format32bppPArgb:
			newImage = new Bitmap(width, height, PixelFormat.Format64bppPArgb);
			layers = 4;
			break;
		default:
			throw new UnsupportedImageFormatException("Invalid pixel format of the source image.");
		}
		BitmapData sourceData = bimap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bimap.PixelFormat);
		BitmapData newData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, newImage.PixelFormat);
		byte* basePtr = (byte*)sourceData.Scan0.ToPointer();
		byte* newBasePtr = (byte*)newData.Scan0.ToPointer();
		int srcStride = sourceData.Stride;
		int dstStride = newData.Stride;
		for (int i = 0; i < height; i++)
		{
			byte* src = basePtr + (nint)i * (nint)srcStride;
			ushort* dst = (ushort*)(newBasePtr + (nint)i * (nint)dstStride);
			int x = 0;
			int lineSize = width * layers;
			while (x < lineSize)
			{
				*dst = (ushort)(*src << 8);
				x++;
				src++;
				dst++;
			}
		}
		bimap.UnlockBits(sourceData);
		newImage.UnlockBits(newData);
		return newImage;
	}
}
