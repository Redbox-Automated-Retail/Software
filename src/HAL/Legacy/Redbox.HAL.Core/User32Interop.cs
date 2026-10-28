using System;
using System.Runtime.InteropServices;

namespace Redbox.HAL.Core;

public static class User32Interop
{
	public struct COPYDATASTRUCT
	{
		public IntPtr dwData;

		public int cbData;

		[MarshalAs(UnmanagedType.LPStr)]
		public string lpData;
	}

	public const int WM_USER = 1024;

	public const int WM_COPYDATA = 74;

	[DllImport("user32.dll")]
	public static extern int SendMessage(int window, int msg, int wparm, ref COPYDATASTRUCT lp);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
