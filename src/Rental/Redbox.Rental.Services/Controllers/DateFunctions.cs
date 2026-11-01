using System;
using System.Globalization;

namespace Redbox.Rental.Services.Controllers;

public class DateFunctions
{
	public static bool ParseDateString(string dateString, out DateTime date)
	{
		string[] formats = new string[3] { "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMddHHmmss" };
		bool result = DateTime.TryParseExact(dateString, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var parsedDate);
		date = parsedDate;
		return result;
	}
}
