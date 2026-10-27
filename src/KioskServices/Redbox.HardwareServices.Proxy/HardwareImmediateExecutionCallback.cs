using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

public class HardwareImmediateExecutionCallback : ICallbackEntry
{
	public HardwareImmediateExecutionHandler EventHandler { get; set; }

	public string Command { get; set; }

	public bool Success { get; set; }

	public Stack<string> Stack { get; set; }

	public List<string> Errors { get; set; }

	public string Name => "HardwareImmediateExecutionCallback";

	public void Invoke()
	{
		try
		{
			LogHelper.Instance.Log("Invoke HardwareImmediateExecutionEventCallBack.EventHandler with: {0}, {1}", Command, Success);
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			if (service != null)
			{
				EventHandler(Command, Success, service.ToLuaTable(Stack), service.ToLuaTable(Errors));
			}
		}
		catch (Exception e)
		{
			IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
			string resourceName = $"HardwareImmediateExecutionCallback for command: {Command}";
			if (!service2.RaiseDebuggerOrError(e, resourceName))
			{
				LogHelper.Instance.Log("An unhandled exception was raised in  HardwareImmediateExecutionCallback handler.", e);
			}
		}
	}
}
