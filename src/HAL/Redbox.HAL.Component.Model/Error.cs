using System;
using System.Text;

namespace Redbox.HAL.Component.Model;

[Serializable]
public sealed class Error
{
	private readonly string? m_code;

	private readonly bool m_isWarning;

	private readonly string? m_details;

	private readonly string m_description;

	public string? Code => m_code;

	public string? Details => m_details;

	public bool IsWarning => m_isWarning;

	public string Description => m_description;

	public static Error Parse(string error)
	{
		string? code = ExtractCodeFromBrackets(error, "[", "]");
		int startIndex = error.IndexOf("]");
		string[] parts = error.Substring(startIndex).Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		string? details = null;
		if (parts.Length > 1)
		{
			details = parts[1];
		}
		string description = parts[0].Substring(parts[0].IndexOf(":") + 1);
		return new Error(code, description, details, parts[0].StartsWith("WARNING"));
	}

	public static Error NewError(string code, string description, Exception e)
	{
		StringBuilder details = new StringBuilder();
		details.Append(e.ToString());
		if (e.InnerException != null)
		{
			details.AppendFormat("\r\n\r\nInner Exception:\r\n---------------------------\r\n{0}\r\n", e.InnerException);
		}
		details.AppendFormat("\r\n\r\nStack Trace:\r\n----------------------------\r\n{0}\r\n", e.StackTrace);
		return new Error(code, description, details.ToString(), isWarning: false);
	}

	public static Error NewError(string code, string description, string? details)
	{
		return new Error(code, description, details, isWarning: false);
	}

	public static Error NewWarning(string code, string description, string? details)
	{
		return new Error(code, description, details, isWarning: true);
	}

	public override string ToString()
	{
		return string.Format("[{0}] {1}: {2}", Code, IsWarning ? "WARNING" : "ERROR", Description);
	}

	private Error(string? code, string description, string? details, bool isWarning)
	{
		m_code = code;
		m_details = details;
		m_isWarning = isWarning;
		m_description = description;
	}

	private static string? ExtractCodeFromBrackets(string value, string prefix, string postfix)
	{
		int depth = 0;
		int codeStart = -1;
		int codeEnd = -1;
		int position = 0;
		do
		{
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
