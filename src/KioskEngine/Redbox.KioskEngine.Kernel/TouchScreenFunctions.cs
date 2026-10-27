using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class TouchScreenFunctions
{
	[KernelFunction(Name = "kernel.toggletouchscreen")]
	internal static void ToggleTouchScreen()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			service.ToggleTouchScreen();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was throw in ToggleTouchScreen.", e);
		}
	}

	[KernelFunction(Name = "kernel.touchscreenstatus")]
	internal static string TouchScreenStatus()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			return service.TouchScreenStatus().ToString();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was throw in TouchScreenStatus.", e);
		}
		return TouchScreenState.Unavailable.ToString();
	}

	[KernelFunction(Name = "kernel.touchscreendriver")]
	internal static string TouchScreenDriver()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			return service.TouchScreenDriver();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was throw in TouchScreenDriver.", e);
		}
		return string.Empty;
	}
}
