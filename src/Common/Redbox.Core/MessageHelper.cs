using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Redbox.Core;

public static class MessageHelper
{
	public static readonly IntPtr HWND_BROADCAST = new IntPtr(65535);

	public static readonly IntPtr HWND_TOP = new IntPtr(0);

	public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

	public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

	public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

	public static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

	public const uint WM_FONTCHANGE = 29u;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
}
