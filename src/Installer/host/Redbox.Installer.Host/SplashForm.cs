using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Redbox.Installer.Host;

public class SplashForm : Form
{
	private Image m_image;

	private IContainer components;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public Image Image
	{
		get
		{
			return m_image;
		}
		set
		{
			m_image = value;
			if (Image != null)
			{
				base.Width = Image.Width;
				base.Height = Image.Height;
				base.Location = new Point((Screen.PrimaryScreen.Bounds.Width - Image.Width) / 2, (Screen.PrimaryScreen.Bounds.Height - Image.Height) / 2);
				SetBitmap((Bitmap)Image);
			}
		}
	}

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 524288;
			return createParams;
		}
	}

	public SplashForm()
	{
		InitializeComponent();
	}

	public void SetBitmap(Bitmap bitmap)
	{
		if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
		{
			throw new ApplicationException("The bitmap must be formatted at 32bpp ARGB.");
		}
		IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
		IntPtr intPtr = NativeMethods.CreateCompatibleDC(dC);
		IntPtr intPtr2 = IntPtr.Zero;
		IntPtr hObject = IntPtr.Zero;
		try
		{
			intPtr2 = bitmap.GetHbitmap(Color.FromArgb(0));
			hObject = NativeMethods.SelectObject(intPtr, intPtr2);
			NativeMethods.Size psize = new NativeMethods.Size(bitmap.Width, bitmap.Height);
			NativeMethods.Point pprSrc = new NativeMethods.Point(0, 0);
			NativeMethods.Point pptDst = new NativeMethods.Point(base.Left, base.Top);
			NativeMethods.BLENDFUNCTION pblend = new NativeMethods.BLENDFUNCTION
			{
				BlendOp = 0,
				BlendFlags = 0,
				SourceConstantAlpha = byte.MaxValue,
				AlphaFormat = 1
			};
			NativeMethods.UpdateLayeredWindow(base.Handle, dC, ref pptDst, ref psize, intPtr, ref pprSrc, 0, ref pblend, 2);
		}
		finally
		{
			NativeMethods.ReleaseDC(IntPtr.Zero, dC);
			if (intPtr2 != IntPtr.Zero)
			{
				NativeMethods.SelectObject(intPtr, hObject);
				NativeMethods.DeleteObject(intPtr2);
			}
			NativeMethods.DeleteDC(intPtr);
		}
	}

	private void OnMouseClick(object sender, MouseEventArgs e)
	{
		Close();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(292, 266);
		base.ControlBox = false;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "SplashForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "SplashForm";
		base.TopMost = true;
		base.MouseClick += new System.Windows.Forms.MouseEventHandler(OnMouseClick);
		base.ResumeLayout(false);
	}
}
