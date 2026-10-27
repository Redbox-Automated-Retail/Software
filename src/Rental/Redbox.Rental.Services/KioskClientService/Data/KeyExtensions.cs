using System.Globalization;

namespace Redbox.Rental.Services.KioskClientService.Data;

internal static class KeyExtensions
{
	public static bool IsNotInFilteredList(this string key)
	{
		if (key.StartsWith("Deck", ignoreCase: true, CultureInfo.InvariantCulture))
		{
			return false;
		}
		if (string.Compare(key, "QLMDeckNumber", ignoreCase: true) == 0)
		{
			return false;
		}
		if (string.Compare(key, "MerchType", ignoreCase: true) == 0)
		{
			return false;
		}
		return true;
	}
}
