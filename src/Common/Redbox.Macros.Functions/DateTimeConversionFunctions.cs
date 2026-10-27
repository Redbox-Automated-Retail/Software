using System;
using System.Globalization;

namespace Redbox.Macros.Functions;

[FunctionSet("datetime", "Conversion")]
public class DateTimeConversionFunctions : FunctionSetBase
{
	public DateTimeConversionFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("parse")]
	public static DateTime Parse(string s)
	{
		return DateTime.Parse(s, CultureInfo.InvariantCulture);
	}

	[Function("to-string")]
	public static string ToString(DateTime value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}
}
