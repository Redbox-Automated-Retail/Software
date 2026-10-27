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
		if (fileName != null)
		{
			return File.ReadAllBytes(fileName);
		}
		return null;
	}

	public static IDictionary<string, string> GetNameValuePairs(this string source)
	{
		IDictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (Match item in Regex.Matches(source, "(?n:((?<name>[\\w-_]*)=(?<val>[\\w-_\\\\\\/\\(\\)\\,'\\.\\s]*)))"))
		{
			Group obj2 = item.Groups["name"];
			Group obj3 = item.Groups["val"];
			if (obj2.Captures.Count == obj3.Captures.Count)
			{
				for (int i = 0; i < obj2.Captures.Count; i++)
				{
					dictionary[obj2.Captures[i].Value] = obj3.Captures[i].Value;
				}
			}
		}
		return dictionary;
	}

	public static string RemoveString(this string value, string token)
	{
		int num = value.IndexOf(token);
		if (num <= -1)
		{
			return value;
		}
		return value.Remove(num, token.Length);
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
			if (property != null)
			{
				property.SetValue(objectFromPath, ConversionHelper.ChangeType(value, property.PropertyType), null);
			}
		}
	}

	public static Type ToType(this string typeName)
	{
		if (!string.IsNullOrEmpty(typeName))
		{
			return Type.GetType(typeName);
		}
		return null;
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
		int result2;
		if (match.Success)
		{
			result = match.Groups["indexer"].Captures[0].Value;
		}
		else if (match2.Success && int.TryParse(match2.Groups["indexer"].Captures[0].Value, out result2))
		{
			result = result2;
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
		return cardNumber.ToLastFour(pad: true);
	}

	public static string ToLastFour(this string cardNumber, bool pad)
	{
		if (cardNumber == null || cardNumber.Length <= 4)
		{
			return cardNumber;
		}
		int num = cardNumber.Length - 4;
		string text = cardNumber.Substring(num);
		if (!pad)
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append("X");
		}
		stringBuilder.Append(text);
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

	public static int? ToInt32(this string numberToConvert)
	{
		if (!int.TryParse(numberToConvert, out var result))
		{
			return null;
		}
		return result;
	}

	public static string SafePadLeft(this string theString, int length, char padCharacter)
	{
		return theString?.PadLeft(length, padCharacter);
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
				obj = obj.GetType().InvokeMember(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty, null, obj, args);
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
