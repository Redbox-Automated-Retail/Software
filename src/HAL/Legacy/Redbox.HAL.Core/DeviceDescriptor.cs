using System;

namespace Redbox.HAL.Core;

public class DeviceDescriptor
{
	private string m_deviceRep;

	public string Vendor { get; private set; }

	public string Product { get; private set; }

	public string Friendlyname { get; private set; }

	public DriverDescriptor DriverInfo { get; private set; }

	public Guid DeviceGuid { get; private set; }

	public override string ToString()
	{
		if (m_deviceRep != null)
		{
			return m_deviceRep;
		}
		return m_deviceRep = $"vid_{Vendor}&pid_{Product}";
	}

	public DeviceDescriptor(string v, string p, string friendly)
	{
		Vendor = v;
		Product = p;
		Friendlyname = friendly;
		DeviceGuid = Guid.Empty;
	}

	public DeviceDescriptor(string v, string p, string friendly, DriverDescriptor desc, Guid deviceGuid)
	{
		Vendor = v;
		Product = p;
		Friendlyname = friendly;
		DriverInfo = desc;
		DeviceGuid = deviceGuid;
	}
}
