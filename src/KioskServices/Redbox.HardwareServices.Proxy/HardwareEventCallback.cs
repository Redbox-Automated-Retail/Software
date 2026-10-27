using System;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

public class HardwareEventCallback : ICallbackEntry
{
	public HardwareEventHandler EventHandler { get; set; }

	public string JobId { get; set; }

	public string Message { get; set; }

	public string Name => "HardwareEventCallback - " + (JobId ?? string.Empty);

	public string TimeStamp { get; set; }

	public void Invoke()
	{
		if (EventHandler == null)
		{
			return;
		}
		try
		{
			LogHelper.Instance.Log("Invoke HardwareEventCallback.EventHandler with: {0}, {1}, {2}", JobId, TimeStamp, Message);
			EventHandler(JobId, TimeStamp, Message);
		}
		catch (Exception e)
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			string resourceName = $"HardwareEventCallback for job ID {JobId}";
			if (!service.RaiseDebuggerOrError(e, resourceName))
			{
				LogHelper.Instance.Log("An unhandled exception was raised in HardwareEventCallback handler.", e);
			}
		}
	}
}
