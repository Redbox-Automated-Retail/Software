namespace Redbox.Core;

public static class GlobalStringExtensions
{
	public static string ExtractCodeFromBrackets(this string value, string prefix, string postfix)
	{
		int num = 0;
		int num2 = -1;
		int num3 = -1;
		int num4 = 0;
		do
		{
			bool flag = true;
			string text = string.Empty;
			if (num4 + prefix.Length < value.Length)
			{
				text = value.Substring(num4, prefix.Length);
			}
			string text2 = string.Empty;
			if (num4 + text2.Length < value.Length)
			{
				text2 = value.Substring(num4, postfix.Length);
			}
			if (text == prefix)
			{
				if (num2 == -1)
				{
					num2 = num4 + prefix.Length;
				}
				num++;
			}
			else if (text2 == postfix)
			{
				num--;
				if (num == 0)
				{
					num3 = num4 - postfix.Length;
					break;
				}
			}
			num4++;
		}
		while (num4 < value.Length);
		if (num2 == -1 || num3 == -1)
		{
			return null;
		}
		return value.Substring(num2, num3 - num2 + postfix.Length);
	}
}
