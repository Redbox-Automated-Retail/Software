using System;

namespace Redbox.Macros.Functions;

[FunctionSet("version", "Version")]
public class VersionFunctions : FunctionSetBase
{
	public VersionFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-major")]
	public static int GetMajor(Version version)
	{
		return version.Major;
	}

	[Function("get-minor")]
	public static int GetMinor(Version version)
	{
		return version.Minor;
	}

	[Function("get-build")]
	public static int GetBuild(Version version)
	{
		return version.Build;
	}

	[Function("get-revision")]
	public static int GetRevision(Version version)
	{
		return version.Revision;
	}
}
