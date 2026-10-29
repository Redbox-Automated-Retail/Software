namespace KioskAdmin;

public static class Extensions
{
	internal static string StripQuote(this string value)
	{
		return value.Replace("\"", "&quot;");
	}
}
