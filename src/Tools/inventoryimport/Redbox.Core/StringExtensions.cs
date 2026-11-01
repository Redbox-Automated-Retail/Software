using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

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
		int index = value.IndexOf(token);
		return (index > -1) ? value.Remove(index, token.Length) : value;
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
			property?.SetValue(parent, ConversionHelper.ChangeType(value, property.PropertyType), null);
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
		Match stringIndexer = Regex.Match(part, "\\[\"(?<indexer>.*?)\"\\]", RegexOptions.Singleline);
		Match numericIndexer = Regex.Match(part, "\\[(?<indexer>[0-9]*?)\\]", RegexOptions.Singleline);
		object index = null;
		if (stringIndexer.Success)
		{
			index = stringIndexer.Groups["indexer"].Captures[0].Value;
		}
		else if (numericIndexer.Success)
		{
			string value = numericIndexer.Groups["indexer"].Captures[0].Value;
			if (int.TryParse(value, out var number))
			{
				index = number;
			}
		}
		return index;
	}

	public static Dictionary<string, object> ToDictionary(this string json)
	{
		return json.ToObject<Dictionary<string, object>>();
	}

	public static T ToObject<T>(this string json)
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = int.MaxValue;
		JavaScriptSerializer javaScriptSerializer2 = javaScriptSerializer;
		javaScriptSerializer2.RegisterConverters(new JavaScriptConverter[2]
		{
			new DateTimeConverter(),
			new TimeSpanConverter()
		});
		return javaScriptSerializer2.Deserialize<T>(json);
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
		foreach (char c in value)
		{
			action(c);
		}
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
				object obj3 = current.GetType().InvokeMember(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty, null, current, args);
				current = obj3;
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
