using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public class HardwareFunctions
{
	[KernelFunction(Name = "kernel.process_hardware_results")]
	internal static void ProcessHardwareResults(string jobId, string jobName)
	{
		try
		{
			HardwareHelper.ProcessResults(jobId, jobName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Unhandled Exception processing hardware results: ", e);
		}
	}

	[KernelFunction(Name = "kernel.refresh_hardware_state")]
	internal static void RefreshHardwareState()
	{
		IHardwareState service = ServiceLocator.Instance.GetService<IHardwareState>();
		if (service == null)
		{
			LogHelper.Instance.Log("Hardware State was not found.");
		}
		else if (service.IsInitialized)
		{
			service.Refresh();
		}
		else
		{
			service.Initialize();
		}
	}
}
