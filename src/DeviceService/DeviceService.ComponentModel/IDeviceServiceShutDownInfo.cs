using System;

namespace DeviceService.ComponentModel;

public interface IDeviceServiceShutDownInfo
{
	DateTime ShutDownTime { get; set; }

	ShutDownReason ShutDownReason { get; set; }
}
