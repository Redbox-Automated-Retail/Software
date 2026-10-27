using System;

namespace Redbox.Macros.Functions;

[FunctionSet("timespan", "Conversion")]
public class TimeSpanConversionFunctions : FunctionSetBase
{
	public TimeSpanConversionFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("parse")]
	public static TimeSpan Parse(string s)
	{
		return TimeSpan.Parse(s);
	}

	[Function("to-string")]
	public static string ToString(TimeSpan value)
	{
		return value.ToString();
	}
}
