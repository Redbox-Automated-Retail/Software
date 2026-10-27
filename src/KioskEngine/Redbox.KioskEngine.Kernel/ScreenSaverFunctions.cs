using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class ScreenSaverFunctions
{
	[KernelFunction(Name = "kernel.screensaverisactive")]
	internal static bool IsScreenSaverActive()
	{
		return ServiceLocator.Instance.GetService<IScreenSaverService>().IsScreenSaverActive();
	}

	[KernelFunction(Name = "kernel.screensaverisrunning")]
	internal static bool IsScreenSaverRunning()
	{
		return ServiceLocator.Instance.GetService<IScreenSaverService>().IsScreenSaverRunning();
	}

	[KernelFunction(Name = "kernel.screensavergettimeout")]
	internal static int GetScreenSaverTimeout()
	{
		return ServiceLocator.Instance.GetService<IScreenSaverService>().GetScreenSaverTimeout();
	}

	[KernelFunction(Name = "kernel.screensaversettimeout")]
	internal static void SetScreenSaverTimeout(int timeout)
	{
		ServiceLocator.Instance.GetService<IScreenSaverService>().SetScreenSaverTimeout(timeout);
	}

	[KernelFunction(Name = "kernel.screensaverdisable")]
	internal static void DisableScreenSaver()
	{
		ServiceLocator.Instance.GetService<IScreenSaverService>().DisableScreenSaver();
	}

	[KernelFunction(Name = "kernel.screensaverenable")]
	internal static void EnableScreenSaver()
	{
		ServiceLocator.Instance.GetService<IScreenSaverService>().EnableScreenSaver();
	}
}
