using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Redbox.Core;

namespace Redbox.ControlPanel.UI;

public static class DisplayHelper
{
	internal static class ChangeDisplaySettingsFlags
	{
		public const int CDS_NONE = 0;

		public const int CDS_UPDATEREGISTRY = 1;

		public const int CDS_TEST = 2;

		public const int CDS_FULLSCREEN = 4;

		public const int CDS_GLOBAL = 8;

		public const int CDS_SET_PRIMARY = 16;

		public const int CDS_VIDEOPARAMETERS = 32;

		public const int CDS_ENABLE_UNSAFE_MODES = 256;

		public const int CDS_DISABLE_UNSAFE_MODES = 512;

		public const int CDS_RESET = 1073741824;

		public const int CDS_RESET_EX = 536870912;

		public const int CDS_NORESET = 268435456;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct DisplayDEVMODE
	{
		private const int DM_INTERLACED = 2;

		[FieldOffset(36)]
		public short dmSize;

		[FieldOffset(104)]
		public int dmBitsPerPel;

		[FieldOffset(108)]
		public int dmPelsWidth;

		[FieldOffset(112)]
		public int dmPelsHeight;

		[FieldOffset(116)]
		public int dmDisplayFlags;

		[FieldOffset(120)]
		public int dmDisplayFrequency;

		private bool Interlaced => (dmDisplayFlags & 2) == 2;

		public static DisplayDEVMODE Create()
		{
			DisplayDEVMODE displayDEVMODE = default(DisplayDEVMODE);
			displayDEVMODE.dmSize = (short)Marshal.SizeOf(displayDEVMODE);
			return displayDEVMODE;
		}

		public override string ToString()
		{
			return $"{dmPelsWidth}x{dmPelsHeight}, Frequency: {dmDisplayFrequency}, Interlaced: {Interlaced}, BitsPerPixel: {dmBitsPerPel}";
		}

		public override bool Equals(object obj)
		{
			return GetHashCode() == obj.GetHashCode();
		}

		public override int GetHashCode()
		{
			return ((((17 * 23 + dmPelsWidth.GetHashCode()) * 23 + dmPelsHeight.GetHashCode()) * 23 + dmDisplayFrequency.GetHashCode()) * 23 + Interlaced.GetHashCode()) * 23 + dmBitsPerPel.GetHashCode();
		}

		public static bool operator ==(DisplayDEVMODE left, DisplayDEVMODE right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DisplayDEVMODE left, DisplayDEVMODE right)
		{
			return !(left == right);
		}
	}

	private const int ENUM_CURRENT_SETTINGS = -1;

	[DllImport("user32.dll", CharSet = CharSet.Ansi)]
	private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DisplayDEVMODE devMode);

	[DllImport("user32.dll", CharSet = CharSet.Ansi)]
	private static extern int ChangeDisplaySettings(ref DisplayDEVMODE devMode, int flags);

	public static List<DisplayDEVMODE> GetSupportedScreenResolutions()
	{
		List<DisplayDEVMODE> list = new List<DisplayDEVMODE>();
		DisplayDEVMODE devMode = DisplayDEVMODE.Create();
		int num = 0;
		while (EnumDisplaySettings(null, num++, ref devMode))
		{
			if (!list.Contains(devMode))
			{
				list.Add(devMode);
			}
		}
		return list;
	}

	public static DisplayDEVMODE? GetCurrentScreenResolution()
	{
		DisplayDEVMODE devMode = DisplayDEVMODE.Create();
		if (!EnumDisplaySettings(null, -1, ref devMode))
		{
			return null;
		}
		return devMode;
	}

	public static void SetScreenResolution(DisplayDEVMODE devMode)
	{
		try
		{
			ChangeDisplaySettings(ref devMode, 33);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Exception trying to set the screen resolution!", e);
		}
	}
}
