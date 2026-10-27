using System;
using System.Diagnostics;
using Redbox.Core;

namespace Redbox.KioskEngine.Environment;

internal static class Windows10TouchScreen
{
	private const string EXE = "wmic.exe";

	private const string ARGS_MASK = "path Win32_PnPEntity where \"Name='HID-Compliant touch screen'\" call {0}";

	private const string ENABLE = "Enable";

	private const string DISABLE = "Disable";

	public static void Enable()
	{
		FireProcess("Enable");
	}

	public static void Disable()
	{
		FireProcess("Disable");
	}

	private static void FireProcess(string wmiMethod)
	{
		Process process = null;
		try
		{
			LogHelper.Instance.Log("Windows10TouchScreen - Attempting to " + wmiMethod + " HID-compliant touch screen");
			string arguments = $"path Win32_PnPEntity where \"Name='HID-Compliant touch screen'\" call {wmiMethod}";
			process = new Process
			{
				StartInfo = new ProcessStartInfo("wmic.exe", arguments)
				{
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					UseShellExecute = false,
					RedirectStandardOutput = true
				}
			};
			process.Start();
			process.WaitForExit();
			LogHelper.Instance.Log($"Windows10TouchScreen - operation result: {process.ExitCode}, output: {process.StandardOutput.ReadToEnd()}");
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Exception trying to toggle HID touch screen device!", e);
		}
		finally
		{
			process.Close();
		}
	}
}
