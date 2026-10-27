using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class IdTechRev1 : AbstractDeviceDescriptor
{
	internal IdTechRev1(IUsbDeviceService ds, IDeviceSetupClassFactory factory)
		: base(ds, DeviceClass.HIDClass)
	{
		base.Vendor = "0ACD";
		base.Product = "0200";
		base.Friendlyname = "ID Tech Single";
	}
}
