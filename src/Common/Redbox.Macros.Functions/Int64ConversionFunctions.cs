using System.Globalization;

namespace Redbox.Macros.Functions;

[FunctionSet("long", "Conversion")]
public class Int64ConversionFunctions : FunctionSetBase
{
	public Int64ConversionFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("parse")]
	public static long Parse(string s)
	{
		return long.Parse(s, CultureInfo.InvariantCulture);
	}

	[Function("to-string")]
	public static string ToString(long value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}
}
