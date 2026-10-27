using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Redbox.KioskEngine.Drawing;

public static class FancyText
{
	private const int BlurAmount = 6;

	public static Image ImageFromText(string strText, Font fnt, Color clrFore, Color clrBack)
	{
		using Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
		SizeF sizeF = graphics.MeasureString(strText, fnt);
		using Bitmap bitmap = new Bitmap((int)sizeF.Width, (int)sizeF.Height);
		using Graphics graphics2 = Graphics.FromImage(bitmap);
		using SolidBrush brush = new SolidBrush(Color.FromArgb(16, clrBack.R, clrBack.G, clrBack.B));
		using SolidBrush brush2 = new SolidBrush(clrFore);
		graphics2.SmoothingMode = SmoothingMode.HighQuality;
		graphics2.InterpolationMode = InterpolationMode.HighQualityBilinear;
		graphics2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		graphics2.DrawString(strText, fnt, brush, 0f, 0f);
		Bitmap bitmap2 = new Bitmap(bitmap.Width + 6, bitmap.Height + 6);
		using Graphics graphics3 = Graphics.FromImage(bitmap2);
		graphics3.SmoothingMode = SmoothingMode.HighQuality;
		graphics3.InterpolationMode = InterpolationMode.HighQualityBilinear;
		graphics3.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		for (int i = 0; i <= 6; i++)
		{
			for (int j = 0; j <= 6; j++)
			{
				graphics3.DrawImageUnscaled(bitmap, i, j);
			}
		}
		graphics3.DrawString(strText, fnt, brush2, 3f, 3f);
		return bitmap2;
	}
}
