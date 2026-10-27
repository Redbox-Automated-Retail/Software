using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class LegacyDeviceDescriptor : IDeviceDescriptor
{
	private readonly IDriverDescriptor DriverDescriptor;

	public string Vendor { get; private set; }

	public string Product { get; private set; }

	public string Friendlyname { get; private set; }

	public IDeviceSetupClass SetupClass { get; private set; }

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool MatchDriver()
	{
		return ServiceLocator.Instance.GetService<IUsbDeviceService>().MatchDriver(this, DriverDescriptor);
	}

	public bool Locate()
	{
		return (GetStatus() & DeviceStatus.Found) != 0;
	}

	public DeviceStatus GetStatus()
	{
		return ServiceLocator.Instance.GetService<IUsbDeviceService>().FindDeviceStatus(this);
	}

	public bool ResetDriver()
	{
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		if (!service.ChangeByHWID(this, DeviceState.Disable))
		{
			return false;
		}
		PerfFunctions.Wait(1500);
		bool result = service.ChangeByHWID(this, DeviceState.Enable);
		PerfFunctions.SpinWait(1500);
		return result;
	}

	internal LegacyDeviceDescriptor(string vendor, string product, string friendly, IDriverDescriptor driver)
	{
		Vendor = vendor.ToLower();
		Product = product.ToLower();
		Friendlyname = friendly;
		DriverDescriptor = driver;
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service.Get(DeviceClass.Image);
	}
}
