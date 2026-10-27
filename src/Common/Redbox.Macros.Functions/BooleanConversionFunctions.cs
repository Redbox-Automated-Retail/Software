using System.Globalization;

namespace Redbox.Macros.Functions;

[FunctionSet("bool", "Conversion")]
public class BooleanConversionFunctions : FunctionSetBase
{
	public BooleanConversionFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("parse")]
	public static bool Parse(string s)
	{
		return bool.Parse(s);
	}

	[Function("to-string")]
	public static string ToString(bool value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}
}
