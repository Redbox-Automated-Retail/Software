namespace Redbox.HAL.Component.Model;

public interface IDeviceDescriptor
{
	string Vendor { get; }

	string Product { get; }

	string Friendlyname { get; }

	IDeviceSetupClass SetupClass { get; }

	bool ResetDriver();

	bool MatchDriver();

	bool Locate();

	DeviceStatus GetStatus();
}
