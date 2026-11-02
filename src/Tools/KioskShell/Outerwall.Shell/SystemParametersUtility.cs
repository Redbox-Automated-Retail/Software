using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Outerwall.Shell.Interop;

namespace Outerwall.Shell;

internal static class SystemParametersUtility
{
	public static bool SetWallpaper(string wallpaper, WallpaperStyle style, TileWallpaper tile)
	{
		if (string.IsNullOrEmpty(wallpaper))
		{
			throw new ArgumentNullException("wallpaper");
		}
		if (!wallpaper.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException($"Only bitmap images can be set as a wallpaper. The specified image was \"{wallpaper}\".", "wallpaper");
		}
		if (SystemParametersInfo(SPI.SPI_SETDESKWALLPAPER, 0u, wallpaper, SPIF.SPIF_UPDATEINIFILE | SPIF.SPIF_SENDCHANGE))
		{
			RegistryKey? registryKey;
			try
			{
				registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: true);
			}
			catch (Exception innerException)
			{
				throw new ApplicationException("Unable to open desktop registry key.", innerException);
			}
			if (registryKey == null)
			{
				throw new ApplicationException("Unable to locate desktop registry key.");
			}
			try
			{
				using (registryKey)
				{
					registryKey.SetValue("WallpaperStyle", style);
					registryKey.SetValue("TileWallpaper", tile);
				}
			}
			catch (Exception innerException2)
			{
				throw new ApplicationException("Unable to set wallpaper.", innerException2);
			}
			UpdatePerUserSystemParameters();
			return true;
		}
		return false;
	}

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, string pvParam, SPIF fWinIni);

	[DllImport("user32.dll")]
	private static extern bool UpdatePerUserSystemParameters();
}
