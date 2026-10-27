using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class IdTechRev2 : AbstractDeviceDescriptor
{
	internal IdTechRev2(IUsbDeviceService ds, IDeviceSetupClassFactory factory)
		: base(ds, DeviceClass.HIDClass)
	{
		base.Vendor = "0ACD";
		base.Product = "0520";
		base.Friendlyname = "ID Tech Dual";
	}
}
