namespace Redbox.Core;

public static class GlobalStringExtensions
{
	public static string ExtractCodeFromBrackets(this string value, string prefix, string postfix)
	{
		int depth = 0;
		int codeStart = -1;
		int codeEnd = -1;
		int position = 0;
		do
		{
			bool flag = true;
			string candidatePrefix = string.Empty;
			if (position + prefix.Length < value.Length)
			{
				candidatePrefix = value.Substring(position, prefix.Length);
			}
			string candidatePostfix = string.Empty;
			if (position + candidatePostfix.Length < value.Length)
			{
				candidatePostfix = value.Substring(position, postfix.Length);
			}
			if (candidatePrefix == prefix)
			{
				if (codeStart == -1)
				{
					codeStart = position + prefix.Length;
				}
				depth++;
			}
			else if (candidatePostfix == postfix)
			{
				depth--;
				if (depth == 0)
				{
					codeEnd = position - postfix.Length;
					break;
				}
			}
			position++;
		}
		while (position < value.Length);
		if (codeStart == -1 || codeEnd == -1)
		{
			return null;
		}
		return value.Substring(codeStart, codeEnd - codeStart + postfix.Length);
	}
}
