using System;

namespace TaskSchedulerInterop;

public struct PropSheetHeader
{
	public uint dwSize;

	public uint dwFlags;

	public IntPtr hwndParent;

	public IntPtr hInstance;

	public IntPtr hIcon;

	public string pszCaption;

	public uint nPages;

	public uint nStartPage;

	public IntPtr phpage;

	public IntPtr pfnCallback;

	public IntPtr hbmWatermark;

	public IntPtr hplWatermark;

	public IntPtr hbmHeader;
}
