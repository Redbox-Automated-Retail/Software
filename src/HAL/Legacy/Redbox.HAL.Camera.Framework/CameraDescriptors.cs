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
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
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
