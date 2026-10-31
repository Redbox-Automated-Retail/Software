using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

public static class CameraDescriptors
{
	private static readonly DriverDescriptor[] DriverInfos;

	private static readonly DeviceDescriptor[] CameraHardwareIds;

	public static DeviceDescriptor FromKnownCamera(KnownCameras camera)
	{
		return CameraHardwareIds[(int)camera];
	}

	public static DriverDescriptor ForKnownCamera(KnownCameras camera)
	{
		return DriverInfos[(int)camera];
	}

	static CameraDescriptors()
	{
		DriverInfos = new DriverDescriptor[5]
		{
			null,
			null,
			null,
			new DriverDescriptor(new Version("5.7.19104.104"), "Sonix", "Sonix"),
			new DriverDescriptor(new Version("2.1.0.0"), "AVEO", "AVEO")
		};
		CameraHardwareIds = (DeviceDescriptor[])(object)new DeviceDescriptor[5]
		{
			default(DeviceDescriptor),
			new DeviceDescriptor("eb1a", "2800", "1st Generation"),
			new DeviceDescriptor("0c45", "6260", "2nd Gen"),
			new DeviceDescriptor("0c45", "627b", "3rd Gen"),
			new DeviceDescriptor("1871", "0f01", "4th Gen")
		};
	}
}
