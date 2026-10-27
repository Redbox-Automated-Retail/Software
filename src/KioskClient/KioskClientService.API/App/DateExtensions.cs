using System;
using System.Globalization;

namespace KioskClientService.API.App;

public static class DateExtensions
{
	public static int GetAbbreviatedMonthNumber(this string month)
	{
		return Array.FindIndex(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames, (string t) => t.Equals(month, StringComparison.CurrentCultureIgnoreCase)) + 1;
	}
}
