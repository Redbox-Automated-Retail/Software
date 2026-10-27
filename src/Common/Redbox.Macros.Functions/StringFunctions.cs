using System.Globalization;

namespace Redbox.Macros.Functions;

[FunctionSet("string", "String")]
public class StringFunctions : FunctionSetBase
{
	public StringFunctions(PropertyDictionary propDict)
		: base(propDict)
	{
	}

	[Function("get-length")]
	public static int GetLength(string s)
	{
		return s.Length;
	}

	[Function("substring")]
	public static string Substring(string str, int startIndex, int length)
	{
		return str.Substring(startIndex, length);
	}

	[Function("starts-with")]
	public static bool StartsWith(string s1, string s2)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(s1, s2);
	}

	[Function("ends-with")]
	public static bool EndsWith(string s1, string s2)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(s1, s2);
	}

	[Function("to-lower")]
	public static string ToLower(string s)
	{
		return s.ToLower(CultureInfo.InvariantCulture);
	}

	[Function("to-upper")]
	public static string ToUpper(string s)
	{
		return s.ToUpper(CultureInfo.InvariantCulture);
	}

	[Function("replace")]
	public static string Replace(string str, string oldValue, string newValue)
	{
		return str.Replace(oldValue, newValue);
	}

	[Function("contains")]
	public static bool Contains(string source, string value)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, value, CompareOptions.None) >= 0;
	}

	[Function("index-of")]
	public static int IndexOf(string source, string value)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, value, CompareOptions.None);
	}

	[Function("last-index-of")]
	public static int LastIndexOf(string source, string value)
	{
		return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(source, value, CompareOptions.None);
	}

	[Function("pad-left")]
	public static string PadLeft(string s, int totalWidth, string paddingChar)
	{
		return s.PadLeft(totalWidth, paddingChar[0]);
	}

	[Function("pad-right")]
	public static string PadRight(string s, int totalWidth, string paddingChar)
	{
		return s.PadRight(totalWidth, paddingChar[0]);
	}

	[Function("trim")]
	public static string Trim(string s)
	{
		return s.Trim();
	}

	[Function("trim-start")]
	public static string TrimStart(string s)
	{
		return s.TrimStart();
	}

	[Function("trim-end")]
	public static string TrimEnd(string s)
	{
		return s.TrimEnd();
	}

	[Function("remove-at")]
	public static string RemoveStringAt(string s, int i)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		if (i != -1)
		{
			return s.Substring(0, i);
		}
		return s;
	}
}
