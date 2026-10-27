using System;

namespace Redbox.HAL.Component.Model;

public interface IScannerDeviceService
{
	DateTime? IRHardwareInstall { get; }

	CameraGeneration CurrentCameraGeneration { get; }

	IScannerDevice GetConfiguredDevice();

	void Shutdown();
}
