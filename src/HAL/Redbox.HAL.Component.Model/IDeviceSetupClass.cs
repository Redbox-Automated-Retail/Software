using System;

namespace Redbox.HAL.Component.Model;

public interface IDeviceSetupClass
{
	DeviceClass Class { get; }

	Guid Guid { get; }
}
