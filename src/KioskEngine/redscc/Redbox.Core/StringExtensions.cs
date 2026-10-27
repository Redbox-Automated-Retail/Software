using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Redbox.Core;

public static class StringExtensions
{
	internal static class ExtractIndexConstants
	{
		internal const string IndexerGroup = "indexer";

		internal const string TextIndexerRegex = "\\[\"(?<indexer>.*?)\"\\]";

		internal const string NumericIndexerRegex = "\\[(?<indexer>[0-9]*?)\\]";
	}

	internal static class NameValuePairConstants
	{
		internal const string NameGroup = "name";

		internal const string ValueGroup = "val";

		internal const string NameValuePairRegex = "(?n:((?<name>[\\w-_]*)=(?<val>[\\w-_\\\\\\/\\(\\)\\,'\\.\\s]*)))";
	}

	internal const BindingFlags GetObjectFromPathBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty;

	internal static readonly char[] PropertyPathSeparators = new char[1] { '.' };

	public static T ToEnum<T>(this string value)
	{
		return Enum<T>.Parse(value, default(T));
	}

	public static byte[] ReadToBuffer(this string fileName)
	{
		return (fileName == null) ? null : File.ReadAllBytes(fileName);
	}

	public static IDictionary<string, string> GetNameValuePairs(this string source)
	{
		IDictionary<string, string> dictionary = new Dictionary<string, string>();
		MatchCollection matchCollection = Regex.Matches(source, "(?n:((?<name>[\\w-_]*)=(?<val>[\\w-_\\\\\\/\\(\\)\\,'\\.\\s]*)))");
		foreach (Match item in matchCollection)
		{
			Group obj = item.Groups["name"];
			Group obj2 = item.Groups["val"];
			if (obj.Captures.Count == obj2.Captures.Count)
			{
				for (int i = 0; i < obj.Captures.Count; i++)
				{
					dictionary[obj.Captures[i].Value] = obj2.Captures[i].Value;
				}
			}
		}
		return dictionary;
	}

	public static string RemoveString(this string value, string token)
	{
		int num = value.IndexOf(token);
		return (num > -1) ? value.Remove(num, token.Length) : value;
	}

	public static string InnerTrim(this string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		foreach (char c in value)
		{
			if (!char.IsControl(c))
			{
				if (flag)
				{
					stringBuilder.Append(' ');
					flag = false;
				}
				stringBuilder.Append(c);
			}
			else
			{
				flag = true;
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetOnlyDigits(this string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in value)
		{
			if (char.IsDigit(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static byte[] HexToBytes(this string source)
	{
		List<byte> list = new List<byte>();
		int num = 0;
		int num2 = 0;
		while (num < source.Length / 2)
		{
			list.Add(byte.Parse(source.Substring(num2, 2), NumberStyles.HexNumber));
			num++;
			num2 += 2;
		}
		return list.ToArray();
	}

	public static byte[] Base85ToBytes(this string source)
	{
		return ASCII85.Decode(source);
	}

	public static byte[] Base64ToBytes(this string source)
	{
		return Convert.FromBase64String(source);
	}

	public static string EncryptToBase64(this string source)
	{
		return Encoding.UTF8.GetBytes(source).Encrypt().ToBase64();
	}

	public static string DecryptFromBase64(this string source)
	{
		return Encoding.UTF8.GetString(source.Base64ToBytes().Decrypt());
	}

	public static object GetValueForPath(this string path, object rootObject)
	{
		return GetObjectFromPath(path, rootObject, null);
	}

	public static void SetValueForPath(this string path, object rootObject, object value)
	{
		object objectFromPath = GetObjectFromPath(path, rootObject, 1);
		if (objectFromPath != null)
		{
			string[] array = path.Split(PropertyPathSeparators);
			PropertyInfo property = objectFromPath.GetType().GetProperty(array[array.Length - 1], BindingFlags.Instance | BindingFlags.Public);
			property?.SetValue(objectFromPath, ConversionHelper.ChangeType(value, property.PropertyType), null);
		}
	}

	public static Type ToType(this string typeName)
	{
		return string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);
	}

	public static object ExtractIndex(string part)
	{
		if (part == null)
		{
			return null;
		}
		Match match = Regex.Match(part, "\\[\"(?<indexer>.*?)\"\\]", RegexOptions.Singleline);
		Match match2 = Regex.Match(part, "\\[(?<indexer>[0-9]*?)\\]", RegexOptions.Singleline);
		object result = null;
		if (match.Success)
		{
			result = match.Groups["indexer"].Captures[0].Value;
		}
		else if (match2.Success)
		{
			string value = match2.Groups["indexer"].Captures[0].Value;
			if (int.TryParse(value, out var result2))
			{
				result = result2;
			}
		}
		return result;
	}

	public static Dictionary<string, object> ToDictionary(this string json)
	{
		return json.ToObject<Dictionary<string, object>>();
	}

	public static T ToObject<T>(this string json)
	{
		return JavaScriptConverterRegistry.Instance.GetSerializer().Deserialize<T>(json);
	}

	public static object ToObject(this string json, Type type)
	{
		return JavaScriptConverterRegistry.Instance.GetSerializer().Deserialize(type, json);
	}

	public static string ToLastFour(this string cardNumber)
	{
		if (cardNumber == null || cardNumber.Length <= 4)
		{
			return cardNumber;
		}
		int num = cardNumber.Length - 4;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append("X");
		}
		stringBuilder.Append(cardNumber.Substring(num));
		return stringBuilder.ToString();
	}

	public static string Escaped(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return null;
		}
		input = input.Replace("\\", "\\\\");
		input = input.Replace("'", "\\'");
		input = input.Replace("\"", "\\\"");
		input = input.Replace("]", string.Empty);
		input = input.Replace("[", string.Empty);
		input = input.Replace("<", string.Empty);
		input = input.Replace(">", string.Empty);
		input = input.Replace("|", string.Empty);
		return input;
	}

	public static string EscapedJson(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return null;
		}
		input = input.Replace("\\", "\\\\");
		input = input.Replace("'", "\\'");
		return input;
	}

	public static void ForEach(this string value, Action<char> action)
	{
		foreach (char obj in value)
		{
			action(obj);
		}
	}

	private static object GetObjectFromPath(string path, object rootObject, int? depthAdjust)
	{
		if (path == null)
		{
			return null;
		}
		string[] array = path.Split(PropertyPathSeparators);
		if (array.Length == 0 || rootObject == null)
		{
			return null;
		}
		int num = array.Length;
		if (depthAdjust.HasValue)
		{
			num -= depthAdjust.Value;
		}
		object obj = rootObject;
		for (int i = 0; i < num; i++)
		{
			object[] args = null;
			string text = array[i];
			object obj2 = ExtractIndex(text);
			if (obj2 != null)
			{
				text = text.Substring(0, text.IndexOf("["));
				args = new object[1] { obj2 };
			}
			try
			{
				object obj3 = obj.GetType().InvokeMember(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty, null, obj, args);
				obj = obj3;
				if (obj == null)
				{
					break;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}
		return obj;
	}
}
