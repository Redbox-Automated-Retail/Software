using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Redbox.Core;

namespace Redbox.Rental.Services;

public class DesktopHelper
{
	private const int SPIF_UPDATEINIFILE = 1;

	private const int SPIF_SENDWININICHANGE = 2;

	private const int SPI_SETDESKWALLPAPER = 20;

	private const int COLOR_DESKTOP = 1;

	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SystemParametersInfo", SetLastError = true)]
	private static extern bool SystemParametersInfoSet(int uAction, int uParam, object lpvParam, int fuWinIni);

	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SystemParametersInfo", SetLastError = true)]
	private static extern int SystemParametersInfoSetWallpaper(int uAction, int uParam, string lpvParam, int fuWinIni);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetSysColors(int cElements, int[] lpaElements, int[] lpaRgbValues);

	private static void RemoveWallpaper()
	{
		LogHelper.Instance.Log("Remove Wallpaper");
		SystemParametersInfoSetWallpaper(20, 0, "", 3);
	}

	public static void SetWallpaper(string path, WallpaperStyle style)
	{
		LogHelper.Instance.Log("Set Wallpaper: {0} {1}", path, style);
		if (path == string.Empty)
		{
			RemoveWallpaper();
		}
		else if (File.Exists(path) && (path.Trim().ToLowerInvariant().EndsWith(".bmp") || path.Trim().ToLowerInvariant().EndsWith(".jpg") || path.Trim().ToLowerInvariant().EndsWith(".jpeg")))
		{
			SystemParametersInfoSetWallpaper(20, 0, path, 3);
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: true);
			switch (style)
			{
			case WallpaperStyle.Stretch:
				registryKey.SetValue("WallpaperStyle", "2");
				registryKey.SetValue("TileWallpaper", "0");
				break;
			case WallpaperStyle.Center:
				registryKey.SetValue("WallpaperStyle", "1");
				registryKey.SetValue("TileWallpaper", "0");
				break;
			case WallpaperStyle.Tile:
				registryKey.SetValue("WallpaperStyle", "1");
				registryKey.SetValue("TileWallpaper", "1");
				break;
			}
			registryKey.Close();
		}
	}

	public static bool IsBackgroundSet(string path)
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: true);
		string text = registryKey.GetValue("Wallpaper") as string;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		bool result = text.Equals(path, StringComparison.CurrentCultureIgnoreCase);
		registryKey.Close();
		return result;
	}
}
