using System;
using System.Text;

namespace Redbox.HAL.Component.Model;

[Serializable]
public sealed class Error
{
	private readonly string m_code;

	private readonly bool m_isWarning;

	private readonly string m_details;

	private readonly string m_description;

	public string Code => m_code;

	public string Details => m_details;

	public bool IsWarning => m_isWarning;

	public string Description => m_description;

	public static Error Parse(string error)
	{
		string code = ExtractCodeFromBrackets(error, "[", "]");
		int startIndex = error.IndexOf("]");
		string[] array = error.Substring(startIndex).Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		string details = null;
		if (array.Length > 1)
		{
			details = array[1];
		}
		string description = array[0].Substring(array[0].IndexOf(":") + 1);
		return new Error(code, description, details, array[0].StartsWith("WARNING"));
	}

	public static Error NewError(string code, string description, Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(e.ToString());
		if (e.InnerException != null)
		{
			stringBuilder.AppendFormat("\r\n\r\nInner Exception:\r\n---------------------------\r\n{0}\r\n", e.InnerException);
		}
		stringBuilder.AppendFormat("\r\n\r\nStack Trace:\r\n----------------------------\r\n{0}\r\n", e.StackTrace);
		return new Error(code, description, stringBuilder.ToString(), isWarning: false);
	}

	public static Error NewError(string code, string description, string details)
	{
		return new Error(code, description, details, isWarning: false);
	}

	public static Error NewWarning(string code, string description, string details)
	{
		return new Error(code, description, details, isWarning: true);
	}

	public override string ToString()
	{
		return string.Format("[{0}] {1}: {2}", Code, IsWarning ? "WARNING" : "ERROR", Description);
	}

	private Error(string code, string description, string details, bool isWarning)
	{
		m_code = code;
		m_details = details;
		m_isWarning = isWarning;
		m_description = description;
	}

	private static string ExtractCodeFromBrackets(string value, string prefix, string postfix)
	{
		int num = 0;
		int num2 = -1;
		int num3 = -1;
		int num4 = 0;
		do
		{
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
