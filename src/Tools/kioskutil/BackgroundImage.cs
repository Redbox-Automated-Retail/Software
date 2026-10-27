using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Redbox.UpdateManager.KioskUtil;

public static class BackgroundImage
{
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

	public static void SetWallpaper(string wallPaperLocation)
	{
		SetWallpaper(wallPaperLocation, 2, 0);
	}

	public static string GetWallpaper()
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: false);
		string result = registryKey.GetValue("WallPaper").ToString();
		registryKey.Close();
		return result;
	}

	private static void SetWallpaper(string WallpaperLocation, int WallpaperStyle, int TileWallpaper)
	{
		SystemParametersInfo(20, 0, WallpaperLocation, 3);
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: true);
		registryKey.SetValue("WallpaperStyle", WallpaperStyle);
		registryKey.SetValue("TileWallpaper", TileWallpaper);
		registryKey.Close();
	}
}
