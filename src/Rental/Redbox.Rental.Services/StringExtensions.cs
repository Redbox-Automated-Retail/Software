using System.Linq;
using System.Text.RegularExpressions;

namespace Redbox.Rental.Services;

public static class StringExtensions
{
	public static string GetLast(this string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}
		return value.Substring(value.Length - 1, 1);
	}

	public static string GetFirst(this string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}
		return value.Substring(0, 1);
	}

	public static string GetMiddle(this string value)
	{
		if (value.Length <= 2)
		{
			return string.Empty;
		}
		return value.Substring(1, value.Length - 2);
	}

	public static bool StartsAlphaNumeric(this string value)
	{
		return value.GetFirst().IsAlphaNumeric();
	}

	public static bool StartsAlpha(this string value)
	{
		return value.GetFirst().IsAlpha();
	}

	public static bool StartsAndEndsAlphaNumeric(this string value)
	{
		if (value.GetFirst().IsAlphaNumeric())
		{
			return value.GetLast().IsAlphaNumeric();
		}
		return false;
	}

	public static bool IsAlpha(this string strToCheck)
	{
		return !new Regex("[^a-zA-Z]").IsMatch(strToCheck);
	}

	public static bool IsAlphaNumeric(this string strToCheck)
	{
		return !new Regex("[^a-zA-Z0-9]").IsMatch(strToCheck);
	}

	public static bool IsAphaNumericOr(this string value, string extra)
	{
		return new string(value.Where((char c) => !extra.Contains(c)).ToArray()).IsAlphaNumeric();
	}
}
