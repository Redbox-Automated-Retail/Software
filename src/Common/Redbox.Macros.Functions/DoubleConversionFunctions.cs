using System.Globalization;

namespace Redbox.Macros.Functions;

[FunctionSet("double", "Conversion")]
public class DoubleConversionFunctions : FunctionSetBase
{
	public DoubleConversionFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("parse")]
	public static double Parse(string s)
	{
		return double.Parse(s, CultureInfo.InvariantCulture);
	}

	[Function("to-string")]
	public static string ToString(double value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}
}
