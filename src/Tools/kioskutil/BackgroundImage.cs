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

	public static string? GetWallpaper()
	{
		RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: false)!; // the desktop key and value exist; throws otherwise, as before
		string? result = key.GetValue("WallPaper")!.ToString();
		key.Close();
		return result;
	}

	private static void SetWallpaper(string WallpaperLocation, int WallpaperStyle, int TileWallpaper)
	{
		SystemParametersInfo(20, 0, WallpaperLocation, 3);
		RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: true)!; // throws when missing, as before
		key.SetValue("WallpaperStyle", WallpaperStyle);
		key.SetValue("TileWallpaper", TileWallpaper);
		key.Close();
	}
}
