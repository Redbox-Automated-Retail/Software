using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Redbox.Installer.Host;

internal static class NativeMethods
{
	[Serializable]
	public struct RECT(int left_, int top_, int right_, int bottom_)
	{
		public int Left = left_;

		public int Top = top_;

		public int Right = right_;

		public int Bottom = bottom_;

		public int Height => Bottom - Top + 1;

		public int Width => Right - Left + 1;

		public Size Size => new Size(Width, Height);

		public Point Location => new Point(Left, Top);

		public Rectangle ToRectangle()
		{
			return Rectangle.FromLTRB(Left, Top, Right, Bottom);
		}

		public static RECT FromRectangle(Rectangle rectangle)
		{
			return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
		}

		public override int GetHashCode()
		{
			return Left ^ ((Top << 13) | (Top >> 19)) ^ ((Width << 26) | (Width >> 6)) ^ ((Height << 7) | (Height >> 25));
		}

		public static implicit operator Rectangle(RECT rect)
		{
			return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static implicit operator RECT(Rectangle rect)
		{
			return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}
	}

	public enum HatchStyle32
	{
		HS_HORIZONTAL,
		HS_VERTICAL,
		HS_FDIAGONAL,
		HS_BDIAGONAL,
		HS_CROSS,
		HS_DIAGCROSS
	}

	public enum BrushStyle32
	{
		BS_SOLID = 0,
		BS_HOLLOW = 1,
		BS_NULL = BS_HOLLOW,
		BS_HATCHED = 2,
		BS_PATTERN = 3,
		BS_INDEXED = 4,
		BS_DIBPATTERN = 5,
		BS_DIBPATTERNPT = 6,
		BS_PATTERN8X8 = 7,
		BS_DIBPATTERN8X8 = 8,
		BS_MONOPATTERN = 9
	}

	public struct LOGBRUSH
	{
		public BrushStyle32 lbStyle;

		public uint lbColor;

		public HatchStyle32 lbHatch;
	}

	public struct COMBOBOXINFO
	{
		public int cbSize;

		public RECT rcItem;

		public RECT rcButton;

		public ComboBoxButtonState buttonState;

		public IntPtr hwndCombo;

		public IntPtr hwndEdit;

		public IntPtr hwndList;
	}

	public enum ComboBoxButtonState
	{
		STATE_SYSTEM_NONE = 0,
		STATE_SYSTEM_INVISIBLE = 32768,
		STATE_SYSTEM_PRESSED = 8
	}

	public enum Bool
	{
		False,
		True
	}

	public struct Point(int x, int y)
	{
		public int x = x;

		public int y = y;
	}

	public struct Size(int cx, int cy)
	{
		public int cx = cx;

		public int cy = cy;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct ARGB
	{
		public byte Blue;

		public byte Green;

		public byte Red;

		public byte Alpha;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BLENDFUNCTION
	{
		public byte BlendOp;

		public byte BlendFlags;

		public byte SourceConstantAlpha;

		public byte AlphaFormat;
	}

	private struct SHFILEINFO
	{
		public int hIcon;

		public int iIcon;

		public int dwAttributes;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string szDisplayName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	}

	public const int WM_PAINT = 15;

	public const int WM_NC_PAINT = 133;

	public const int WM_SETFONT = 48;

	public const int WM_SETCURSOR = 32;

	public const int WM_SETREDRAW = 11;

	public const int WM_ERASEBKGND = 20;

	public const int WM_PRINTCLIENT = 792;

	public const int WM_CTLCOLOREDIT = 307;

	public const int WM_NC_HITTEST = 132;

	public const int WM_NC_LBUTTONDOWN = 161;

	public const int CB_INITSTORAGE = 353;

	public const int CB_SHOWDROPDOWN = 335;

	public const int WS_BORDER = 8388608;

	public const int WS_EX_LAYERED = 524288;

	public const int WS_EX_CLIENTEDGE = 512;

	public const int GWL_STYLE = -16;

	public const int GWL_EXSTYLE = -20;

	public const uint SWP_NOSIZE = 1u;

	public const uint SWP_NOMOVE = 2u;

	public const uint SWP_NOZORDER = 4u;

	public const uint SWP_NOREDRAW = 8u;

	public const uint SWP_NOACTIVATE = 16u;

	public const uint SWP_FRAMECHANGED = 32u;

	public const uint SWP_SHOWWINDOW = 64u;

	public const uint SWP_HIDEWINDOW = 128u;

	public const uint SWP_NOCOPYBITS = 256u;

	public const uint SWP_NOOWNERZORDER = 512u;

	public const uint SWP_NOSENDCHANGING = 1024u;

	public const int ULW_COLORKEY = 1;

	public const int ULW_ALPHA = 2;

	public const int ULW_OPAQUE = 4;

	public const byte AC_SRC_OVER = 0;

	public const byte AC_SRC_ALPHA = 1;

	public const int HTCLIENT = 2;

	public const int SHGFI_ICON = 256;

	public const int SHGFI_SMALLICON = 1;

	public const int SHGFI_LARGEICON = 0;

	public const int SHGFI_OPENICON = 2;

	public const int SHGFI_LINKOVERLAY = 32768;

	public const int SHGFI_SELECTED = 65536;

	public const int SHGFI_TYPENAME = 1024;

	public const int SHGFI_USEFILEATTRIBUTES = 16;

	public static string GetFileType(string path)
	{
		SHFILEINFO psfi = new SHFILEINFO
		{
			szTypeName = new string(' ', 80)
		};
		int uFlags = 1040;
		if (SHGetFileInfo(path, 0, ref psfi, Marshal.SizeOf(psfi), uFlags) == 0)
		{
			return string.Empty;
		}
		return psfi.szTypeName.Trim();
	}

	public static Icon GetAssociatedIcon(string strFilePath, bool bLargeIcon, bool extensionOnly)
	{
		SHFILEINFO psfi = default(SHFILEINFO);
		int num = 0x100 | ((!bLargeIcon) ? 1 : 0);
		if (extensionOnly)
		{
			num |= 0x10;
		}
		Icon result = null;
		if (SHGetFileInfo(strFilePath, 128, ref psfi, Marshal.SizeOf(psfi), num) != 0)
		{
			IntPtr intPtr = new IntPtr(psfi.hIcon);
			if (!intPtr.Equals(0))
			{
				result = Icon.FromHandle(intPtr);
			}
			DeleteObject(intPtr);
		}
		return result;
	}

	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, [MarshalAs(UnmanagedType.LPTStr)] string lpszClass, [MarshalAs(UnmanagedType.LPTStr)] string lpszWindow);

	[DllImport("user32")]
	public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

	[DllImport("user32")]
	public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateBrushIndirect([In] ref LOGBRUSH lplb);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern uint SetBkColor(IDeviceContext hdc, uint crColor);

	[DllImport("user32.dll")]
	public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

	[DllImport("user32.dll")]
	public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	public static extern bool GetComboBoxInfo(IntPtr hWnd, ref COMBOBOXINFO pcbi);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern int GetWindowLong(IntPtr hWnd, int index);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);

	[DllImport("user32.dll", ExactSpelling = true)]
	public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	public static extern IntPtr GetDC(IntPtr hWnd);

	[DllImport("user32.dll", ExactSpelling = true)]
	public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
	public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

	[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
	public static extern Bool DeleteDC(IntPtr hdc);

	[DllImport("gdi32.dll", ExactSpelling = true)]
	public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

	[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
	public static extern Bool DeleteObject(IntPtr hObject);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	public static extern Bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

	[DllImport("shell32.dll")]
	private static extern int SHGetFileInfo(string pszPath, int dwAttributes, ref SHFILEINFO psfi, int cbSizeFileInfo, int uFlags);

	public static int RGB(Color color)
	{
		return color.R | (color.G << 8) | (color.B << 16);
	}
}
