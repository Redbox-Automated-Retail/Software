using System;

namespace Redbox.Macros.Functions;

[FunctionSet("version", "Conversion")]
public class VersionConversionFunctions : FunctionSetBase
{
	public VersionConversionFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("parse")]
	public static Version Parse(string version)
	{
		return new Version(version);
	}

	[Function("to-string")]
	public static string ToString(Version value)
	{
		return value.ToString();
	}
}
