using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class QueryUsbDeviceResult : IQueryUsbDeviceResult
{
	public IDeviceDescriptor Descriptor { get; private set; }

	public bool Found => (Status & DeviceStatus.Found) != 0;

	public DeviceStatus Status { get; internal set; }

	public bool IsDisabled
	{
		get
		{
			ThrowIfNotFound();
			return (DeviceStatus.Disabled & Status) != 0;
		}
	}

	public bool IsNotStarted
	{
		get
		{
			ThrowIfNotFound();
			return (Status & DeviceStatus.NotStarted) != 0;
		}
	}

	public bool Running
	{
		get
		{
			if ((Status & DeviceStatus.Found) != DeviceStatus.None)
			{
				return (Status & DeviceStatus.Enabled) != 0;
			}
			return false;
		}
	}

	public override string ToString()
	{
		if (!Found)
		{
			return "UNKNOWN device";
		}
		return $"{Descriptor} ( {Descriptor.Friendlyname} ) status = {Status}";
	}

	internal QueryUsbDeviceResult(IDeviceDescriptor descriptor)
	{
		Status = DeviceStatus.None;
		Descriptor = descriptor;
	}

	private void ThrowIfNotFound()
	{
		if (!Found)
		{
			throw new InvalidOperationException("Device not found");
		}
	}
}
