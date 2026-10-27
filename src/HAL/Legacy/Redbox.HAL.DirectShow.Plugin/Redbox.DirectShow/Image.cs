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
			int num = palette.Entries.Length;
			for (int i = 0; i < num; i++)
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
				int num = fileStream.Read(buffer, 0, 10000);
				if (num == 0)
				{
					break;
				}
				memoryStream.Write(buffer, 0, num);
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
		Bitmap bitmap = null;
		int num = 0;
		int width = bimap.Width;
		int height = bimap.Height;
		switch (bimap.PixelFormat)
		{
		case PixelFormat.Format16bppGrayScale:
			bitmap = CreateGrayscaleImage(width, height);
			num = 1;
			break;
		case PixelFormat.Format48bppRgb:
			bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			num = 3;
			break;
		case PixelFormat.Format64bppArgb:
			bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			num = 4;
			break;
		case PixelFormat.Format64bppPArgb:
			bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
			num = 4;
			break;
		default:
			throw new UnsupportedImageFormatException("Invalid pixel format of the source image.");
		}
		BitmapData bitmapData = bimap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bimap.PixelFormat);
		BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
		byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
		byte* ptr2 = (byte*)bitmapData2.Scan0.ToPointer();
		int stride = bitmapData.Stride;
		int stride2 = bitmapData2.Stride;
		for (int i = 0; i < height; i++)
		{
			ushort* ptr3 = (ushort*)(ptr + (nint)i * (nint)stride);
			byte* ptr4 = ptr2 + (nint)i * (nint)stride2;
			int num2 = 0;
			int num3 = width * num;
			while (num2 < num3)
			{
				*ptr4 = (byte)(*ptr3 >> 8);
				num2++;
				ptr3++;
				ptr4++;
			}
		}
		bimap.UnlockBits(bitmapData);
		bitmap.UnlockBits(bitmapData2);
		return bitmap;
	}

	public unsafe static Bitmap Convert8bppTo16bpp(Bitmap bimap)
	{
		Bitmap bitmap = null;
		int num = 0;
		int width = bimap.Width;
		int height = bimap.Height;
		switch (bimap.PixelFormat)
		{
		case PixelFormat.Format8bppIndexed:
			bitmap = new Bitmap(width, height, PixelFormat.Format16bppGrayScale);
			num = 1;
			break;
		case PixelFormat.Format24bppRgb:
			bitmap = new Bitmap(width, height, PixelFormat.Format48bppRgb);
			num = 3;
			break;
		case PixelFormat.Format32bppArgb:
			bitmap = new Bitmap(width, height, PixelFormat.Format64bppArgb);
			num = 4;
			break;
		case PixelFormat.Format32bppPArgb:
			bitmap = new Bitmap(width, height, PixelFormat.Format64bppPArgb);
			num = 4;
			break;
		default:
			throw new UnsupportedImageFormatException("Invalid pixel format of the source image.");
		}
		BitmapData bitmapData = bimap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bimap.PixelFormat);
		BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
		byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
		byte* ptr2 = (byte*)bitmapData2.Scan0.ToPointer();
		int stride = bitmapData.Stride;
		int stride2 = bitmapData2.Stride;
		for (int i = 0; i < height; i++)
		{
			byte* ptr3 = ptr + (nint)i * (nint)stride;
			ushort* ptr4 = (ushort*)(ptr2 + (nint)i * (nint)stride2);
			int num2 = 0;
			int num3 = width * num;
			while (num2 < num3)
			{
				*ptr4 = (ushort)(*ptr3 << 8);
				num2++;
				ptr3++;
				ptr4++;
			}
		}
		bimap.UnlockBits(bitmapData);
		bitmap.UnlockBits(bitmapData2);
		return bitmap;
	}
}
