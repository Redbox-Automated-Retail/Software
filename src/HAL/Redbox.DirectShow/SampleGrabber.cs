using System;
using System.Drawing;
using System.Drawing.Imaging;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

public class SampleGrabber : ISampleGrabberCB
{
	private readonly ProcessImageCallback ProcessImage;

	internal bool LogSnapReceived { get; set; }

	internal Size Size { get; set; }

	internal bool Grab { get; set; }

	public int SampleCB(double sampleTime, IntPtr sample)
	{
		return 0;
	}

	public unsafe int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
	{
		if (!Grab)
		{
			return 0;
		}
		if (LogSnapReceived)
		{
			LogHelper.Instance.Log("Snap requested.");
		}
		Bitmap bitmap = new Bitmap(Size.Width, Size.Height, PixelFormat.Format24bppRgb);
		BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, Size.Width, Size.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
		int stride = bitmapData.Stride;
		int stride2 = bitmapData.Stride;
		byte* ptr = (byte*)bitmapData.Scan0.ToPointer() + stride2 * (Size.Height - 1);
		byte* ptr2 = (byte*)buffer.ToPointer();
		for (int i = 0; i < Size.Height; i++)
		{
			Win32.memcpy(ptr, ptr2, stride);
			ptr -= stride2;
			ptr2 += stride;
		}
		bitmap.UnlockBits(bitmapData);
		ProcessImage(bitmap);
		bitmap.Dispose();
		return 0;
	}

	internal SampleGrabber(ProcessImageCallback callback)
	{
		if (callback == null)
		{
			throw new ArgumentException("Callback cannot be null.");
		}
		ProcessImage = callback;
	}
}
