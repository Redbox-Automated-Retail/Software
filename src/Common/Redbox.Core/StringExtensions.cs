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
		IDictionary<string, string> pairs = new Dictionary<string, string>();
		foreach (Match match in Regex.Matches(source, "(?n:((?<name>[\\w-_]*)=(?<val>[\\w-_\\\\\\/\\(\\)\\,'\\.\\s]*)))"))
		{
			Group names = match.Groups["name"];
			Group values = match.Groups["val"];
			if (names.Captures.Count == values.Captures.Count)
			{
				for (int i = 0; i < names.Captures.Count; i++)
				{
					pairs[names.Captures[i].Value] = values.Captures[i].Value;
				}
			}
		}
		return pairs;
	}

	public static string RemoveString(this string value, string token)
	{
		int index = value.IndexOf(token);
		if (index <= -1)
		{
			return value;
		}
		return value.Remove(index, token.Length);
	}

	public static string InnerTrim(this string value)
	{
		StringBuilder trimmed = new StringBuilder();
		bool pendingSpace = false;
		foreach (char c in value)
		{
			if (!char.IsControl(c))
			{
				if (pendingSpace)
				{
					trimmed.Append(' ');
					pendingSpace = false;
				}
				trimmed.Append(c);
			}
			else
			{
				pendingSpace = true;
			}
		}
		return trimmed.ToString();
	}

	public static string GetOnlyDigits(this string value)
	{
		StringBuilder digits = new StringBuilder();
		foreach (char c in value)
		{
			if (char.IsDigit(c))
			{
				digits.Append(c);
			}
		}
		return digits.ToString();
	}

	public static byte[] HexToBytes(this string source)
	{
		List<byte> bytes = new List<byte>();
		int byteIndex = 0;
		int charIndex = 0;
		while (byteIndex < source.Length / 2)
		{
			bytes.Add(byte.Parse(source.Substring(charIndex, 2), NumberStyles.HexNumber));
			byteIndex++;
			charIndex += 2;
		}
		return bytes.ToArray();
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
		object parent = GetObjectFromPath(path, rootObject, 1);
		if (parent != null)
		{
			string[] parts = path.Split(PropertyPathSeparators);
			PropertyInfo property = parent.GetType().GetProperty(parts[parts.Length - 1], BindingFlags.Instance | BindingFlags.Public);
			if (property != null)
			{
				property.SetValue(parent, ConversionHelper.ChangeType(value, property.PropertyType), null);
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
		Match stringIndexer = Regex.Match(part, "\\[\"(?<indexer>.*?)\"\\]", RegexOptions.Singleline);
		Match numericIndexer = Regex.Match(part, "\\[(?<indexer>[0-9]*?)\\]", RegexOptions.Singleline);
		object index = null;
		int number;
		if (stringIndexer.Success)
		{
			index = stringIndexer.Groups["indexer"].Captures[0].Value;
		}
		else if (numericIndexer.Success && int.TryParse(numericIndexer.Groups["indexer"].Captures[0].Value, out number))
		{
			index = number;
		}
		return index;
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
		int maskedLength = cardNumber.Length - 4;
		string lastFour = cardNumber.Substring(maskedLength);
		if (!pad)
		{
			return lastFour;
		}
		StringBuilder masked = new StringBuilder();
		for (int i = 0; i < maskedLength; i++)
		{
			masked.Append("X");
		}
		masked.Append(lastFour);
		return masked.ToString();
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
		foreach (char c in value)
		{
			action(c);
		}
	}

	public static int? ToInt32(this string numberToConvert)
	{
		if (!int.TryParse(numberToConvert, out var number))
		{
			return null;
		}
		return number;
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
		string[] parts = path.Split(PropertyPathSeparators);
		if (parts.Length == 0 || rootObject == null)
		{
			return null;
		}
		int depth = parts.Length;
		if (depthAdjust.HasValue)
		{
			depth -= depthAdjust.Value;
		}
		object current = rootObject;
		for (int i = 0; i < depth; i++)
		{
			object[] args = null;
			string member = parts[i];
			object index = ExtractIndex(member);
			if (index != null)
			{
				member = member.Substring(0, member.IndexOf("["));
				args = new object[1] { index };
			}
			try
			{
				current = current.GetType().InvokeMember(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty, null, current, args);
				if (current == null)
				{
					break;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}
		return current;
	}
}
