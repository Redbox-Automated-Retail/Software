namespace Redbox.KioskEngine.ComponentModel;

public static class NumberExtensionMethods
{
	public static string ToCurrencyString(this decimal amount, bool useCentSign = false)
	{
		if (useCentSign && amount < 1m)
		{
			return (int)(amount * 100m) + "¢";
		}
		return $"{amount:C2}";
	}
}
