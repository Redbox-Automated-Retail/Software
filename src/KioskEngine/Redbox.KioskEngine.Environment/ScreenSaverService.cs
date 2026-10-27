using System;
using System.Runtime.InteropServices;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class ScreenSaverService : IScreenSaverService
{
	private const int SPI_GETSCREENSAVERACTIVE = 16;

	private const int SPI_SETSCREENSAVERACTIVE = 17;

	private const int SPI_GETSCREENSAVERTIMEOUT = 14;

	private const int SPI_SETSCREENSAVERTIMEOUT = 15;

	private const int SPI_GETSCREENSAVERRUNNING = 114;

	private const int SPIF_SENDWININICHANGE = 2;

	public static ScreenSaverService Instance => Singleton<ScreenSaverService>.Instance;

	public void EnableScreenSaver()
	{
		int lpvParam = 0;
		SystemParametersInfo(17, 1, ref lpvParam, 2);
	}

	public bool IsScreenSaverRunning()
	{
		bool lpvParam = false;
		SystemParametersInfo(114, 0, ref lpvParam, 0);
		return lpvParam;
	}

	public void DisableScreenSaver()
	{
		int lpvParam = 0;
		SystemParametersInfo(17, 0, ref lpvParam, 2);
	}

	public bool IsScreenSaverActive()
	{
		bool lpvParam = false;
		SystemParametersInfo(16, 0, ref lpvParam, 0);
		return lpvParam;
	}

	public int GetScreenSaverTimeout()
	{
		int lpvParam = 0;
		SystemParametersInfo(14, 0, ref lpvParam, 0);
		return lpvParam;
	}

	public void SetScreenSaverTimeout(int timeout)
	{
		int lpvParam = 0;
		SystemParametersInfo(15, timeout, ref lpvParam, 2);
	}

	private ScreenSaverService()
	{
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern bool SystemParametersInfo(int uAction, int uParam, ref int lpvParam, int flags);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern bool SystemParametersInfo(int uAction, int uParam, ref bool lpvParam, int flags);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetForegroundWindow();
}
