using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public static class DeviceServiceClientHelperFactory
{
	public static IDeviceServiceClientHelper Create()
	{
		if (DeviceServiceClientHelperSimulator.Instance.UseSimulatedCardReader)
		{
			return DeviceServiceClientHelperSimulator.Instance;
		}
		return DeviceServiceClientHelper.Instance;
	}
}
