namespace Redbox.HAL.Component.Model;

public interface ITouchscreenDescriptor : IDeviceDescriptor
{
	bool SoftReset();

	bool HardReset();

	string ReadFirmware();
}
