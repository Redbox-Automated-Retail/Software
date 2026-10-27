using System;
using System.Collections.Generic;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Controller.Framework.Services;

internal sealed class PowerCycleDeviceService : IPowerCycleDeviceService
{
	private readonly Dictionary<PowerCycleDevices, IPowerCycleDevice> Devices = new Dictionary<PowerCycleDevices, IPowerCycleDevice>();

	public IPowerCycleDevice Get(PowerCycleDevices device)
	{
		if (!Devices.ContainsKey(device))
		{
			return null;
		}
		return Devices[device];
	}

	internal PowerCycleDeviceService(IRuntimeService rts)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		string text = string.Empty;
		try
		{
			text = rts.RuntimePath(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
			Type typeFromHandle = typeof(IPowerCycleDevice);
			Type[] types = Assembly.LoadFrom(text).GetTypes();
			foreach (Type type in types)
			{
				if (!typeFromHandle.IsAssignableFrom(type) || type.IsInterface)
				{
					continue;
				}
				PowerCycleDeviceAttribute[] array = (PowerCycleDeviceAttribute[])type.GetCustomAttributes(typeof(PowerCycleDeviceAttribute), inherit: false);
				if (array.Length != 0)
				{
					IPowerCycleDevice value = (IPowerCycleDevice)Activator.CreateInstance(type, nonPublic: true);
					if (!Devices.ContainsKey(array[0].Device))
					{
						Devices[array[0].Device] = value;
					}
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[PowerCycleDeviceService]Unable to load assembly '{text}' to scan for power cycle devices.", e);
		}
		executionTimer.Stop();
		LogHelper.Instance.Log("[PowerCycleDeviceService] Time to scan for {0} power cycle devices: {1}", Devices.Keys.Count, executionTimer.Elapsed);
	}
}
