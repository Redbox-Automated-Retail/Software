using System;

namespace Redbox.HAL.Core;

public class DriverDescriptor
{
	public Version DriverVersion { get; private set; }

	public string Manufacturer { get; private set; }

	public string Provider { get; private set; }

	public DriverDescriptor(Version version, string mfrName, string provider)
	{
		DriverVersion = version;
		Manufacturer = mfrName;
		Provider = provider;
	}
}
