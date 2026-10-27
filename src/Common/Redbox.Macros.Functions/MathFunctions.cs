using System;

namespace Redbox.Macros.Functions;

[FunctionSet("math", "Math")]
public class MathFunctions : FunctionSetBase
{
	public MathFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("round")]
	public static double Round(double value)
	{
		return Math.Round(value);
	}

	[Function("floor")]
	public static double Floor(double value)
	{
		return Math.Floor(value);
	}

	[Function("ceiling")]
	public static double Ceiling(double value)
	{
		return Math.Ceiling(value);
	}

	[Function("abs")]
	public static double Abs(double value)
	{
		return Math.Abs(value);
	}
}
