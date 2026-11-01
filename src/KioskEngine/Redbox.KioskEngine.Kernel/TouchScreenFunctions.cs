using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class TouchScreenFunctions
{
	[KernelFunction(Name = "kernel.toggletouchscreen")]
	internal static void ToggleTouchScreen()
	{
		IEnvironmentNotificationService environmentNotificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			environmentNotificationService.ToggleTouchScreen();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was throw in ToggleTouchScreen.", e);
		}
	}

	[KernelFunction(Name = "kernel.touchscreenstatus")]
	internal static string TouchScreenStatus()
	{
		IEnvironmentNotificationService environmentNotificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			return environmentNotificationService.TouchScreenStatus().ToString();
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
		IEnvironmentNotificationService environmentNotificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			return environmentNotificationService.TouchScreenDriver();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was throw in TouchScreenDriver.", e);
		}
		return string.Empty;
	}
}
