namespace Redbox.HAL.Component.Model;

public interface IQueryUsbDeviceResult
{
	IDeviceDescriptor Descriptor { get; }

	bool Found { get; }

	DeviceStatus Status { get; }

	bool IsDisabled { get; }

	bool IsNotStarted { get; }

	bool Running { get; }
}
