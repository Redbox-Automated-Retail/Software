using System.Globalization;

namespace Redbox.Macros.Functions;

[FunctionSet("int", "Conversion")]
public class Int32ConversionFunctions : FunctionSetBase
{
	public Int32ConversionFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("parse")]
	public static int Parse(string s)
	{
		return int.Parse(s, CultureInfo.InvariantCulture);
	}

	[Function("to-string")]
	public static string ToString(int value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}
}
