using System;

namespace Redbox.JSONPrettyPrinter;

internal sealed class Mask
{
	public static string NullString(string actual)
	{
		if (actual != null)
		{
			return actual;
		}
		return string.Empty;
	}

	public static string NullString(string actual, string mask)
	{
		if (actual != null)
		{
			return actual;
		}
		return mask;
	}

	public static string EmptyString(string actual, string emptyValue)
	{
		if (NullString(actual).Length != 0)
		{
			return actual;
		}
		return emptyValue;
	}

	private Mask()
	{
		throw new NotSupportedException();
	}
}
