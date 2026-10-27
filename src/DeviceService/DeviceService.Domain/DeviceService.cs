using System;
using System.Collections.Generic;
using System.Reflection;

namespace DeviceService.Domain;

public class DeviceService
{
	public static List<Version> CompatibleClientVersions = new List<Version>
	{
		new Version(1, 5),
		new Version(1, 8),
		new Version(1, 10)
	};

	public static Version AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version;

	public static bool IsClientVersionCompatible(Version deviceServiceClientVersion)
	{
		bool result = false;
		if (deviceServiceClientVersion != null)
		{
			foreach (Version compatibleClientVersion in CompatibleClientVersions)
			{
				if (compatibleClientVersion.Major == deviceServiceClientVersion.Major && compatibleClientVersion.Minor == deviceServiceClientVersion.Minor)
				{
					result = true;
				}
			}
		}
		return result;
	}
}
