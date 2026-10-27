using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class DeviceSetupClassFactory : IDeviceSetupClassFactory
{
	private class NoSetupClass : IDeviceSetupClass
	{
		public DeviceClass Class => DeviceClass.None;

		public Guid Guid => Guid.Empty;
	}

	private class PortsSetupClass : IDeviceSetupClass
	{
		private readonly Guid Ports = new Guid("4D36E978-E325-11CE-BFC1-08002BE10318");

		public DeviceClass Class => DeviceClass.Ports;

		public Guid Guid => Ports;
	}

	private class UsbSetupClass : IDeviceSetupClass
	{
		private readonly Guid USB = new Guid("36FC9E60-C465-11CF-8056-444553540000");

		public DeviceClass Class => DeviceClass.USB;

		public Guid Guid => USB;
	}

	private class ImageSetupClass : IDeviceSetupClass
	{
		private readonly Guid Image = new Guid("6BDD1FC6-810F-11D0-BEC7-08002BE2092F");

		public DeviceClass Class => DeviceClass.Image;

		public Guid Guid => Image;
	}

	private class MonitorSetupClass : IDeviceSetupClass
	{
		private readonly Guid Monitor = new Guid("4d36e96e-e325-11ce-bfc1-08002be10318");

		public DeviceClass Class => DeviceClass.Monitor;

		public Guid Guid => Monitor;
	}

	private class HidSetupClass : IDeviceSetupClass
	{
		private readonly Guid HID = new Guid("745a17a0-74d3-11d0-b6fe-00a0c90f57da");

		public DeviceClass Class => DeviceClass.HIDClass;

		public Guid Guid => HID;
	}

	private class MouseSetupClass : IDeviceSetupClass
	{
		private readonly Guid Mouse = new Guid("4D36E96F-E325-11CE-BFC1-08002BE10318");

		public DeviceClass Class => DeviceClass.Mouse;

		public Guid Guid => Mouse;
	}

	private class BatterySetupClass : IDeviceSetupClass
	{
		private readonly Guid Batt = new Guid("72631e54-78a4-11d0-bcf7-00aa00b7b32a");

		public DeviceClass Class => DeviceClass.Battery;

		public Guid Guid => Batt;
	}

	private readonly Dictionary<DeviceClass, IDeviceSetupClass> ClassMap = new Dictionary<DeviceClass, IDeviceSetupClass>();

	public IDeviceSetupClass Get(DeviceClass clazz)
	{
		if (!ClassMap.ContainsKey(clazz))
		{
			return null;
		}
		return ClassMap[clazz];
	}

	public DeviceSetupClassFactory()
	{
		ClassMap[DeviceClass.Ports] = new PortsSetupClass();
		ClassMap[DeviceClass.USB] = new UsbSetupClass();
		ClassMap[DeviceClass.Image] = new ImageSetupClass();
		ClassMap[DeviceClass.Monitor] = new MonitorSetupClass();
		ClassMap[DeviceClass.HIDClass] = new HidSetupClass();
		ClassMap[DeviceClass.Mouse] = new MouseSetupClass();
		ClassMap[DeviceClass.Battery] = new BatterySetupClass();
		ClassMap[DeviceClass.None] = new NoSetupClass();
	}
}
