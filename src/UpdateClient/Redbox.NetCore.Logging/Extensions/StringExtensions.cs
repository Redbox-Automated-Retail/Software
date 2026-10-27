namespace Redbox.NetCore.Logging.Extensions;

public static class StringExtensions
{
	public static string MaskLogValue(this string source, int visibleChars)
	{
		if (source == null)
		{
			return null;
		}
		if (source.Equals(string.Empty))
		{
			return string.Empty;
		}
		if (source.Length <= visibleChars)
		{
			return new string('X', source.Length);
		}
		return new string('X', source.Length - visibleChars) + source.Substring(source.Length - visibleChars);
	}
}
