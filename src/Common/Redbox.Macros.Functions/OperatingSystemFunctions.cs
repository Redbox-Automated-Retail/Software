using System;

namespace Redbox.Macros.Functions;

[FunctionSet("operating-system", "Operating System")]
public class OperatingSystemFunctions : FunctionSetBase
{
	public OperatingSystemFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-platform")]
	public static PlatformID GetPlatform(OperatingSystem operatingSystem)
	{
		return operatingSystem.Platform;
	}

	[Function("get-version")]
	public static Version GetVersion(OperatingSystem operatingSystem)
	{
		return operatingSystem.Version;
	}

	[Function("to-string")]
	public static string ToString(OperatingSystem operatingSystem)
	{
		return operatingSystem.ToString();
	}
}
