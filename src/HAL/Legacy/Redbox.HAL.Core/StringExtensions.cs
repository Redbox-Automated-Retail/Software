using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Redbox.HAL.Core;

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

	public static string RemoveString(string value, string token)
	{
		int num = value.IndexOf(token);
		if (num <= -1)
		{
			return value;
		}
		return value.Remove(num, token.Length);
	}

	public static byte[] HexToBytes(string source)
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

	public static byte[] Base64ToBytes(string source)
	{
		return Convert.FromBase64String(source);
	}

	public static string EncryptToBase64(string source)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(source);
		return ByteHelper.ToBase64(ByteHelper.Encrypt(bytes));
	}

	public static string DecryptFromBase64(string source)
	{
		byte[] inputArray = Base64ToBytes(source);
		return Encoding.UTF8.GetString(ByteHelper.Decrypt(inputArray));
	}

	public static object GetValueForPath(string path, object rootObject)
	{
		return GetObjectFromPath(path, rootObject, null);
	}

	public static void SetValueForPath(string path, object rootObject, object value)
	{
		object objectFromPath = GetObjectFromPath(path, rootObject, 1);
		if (objectFromPath != null)
		{
			string[] array = path.Split(PropertyPathSeparators);
			PropertyInfo property = objectFromPath.GetType().GetProperty(array[array.Length - 1], BindingFlags.Instance | BindingFlags.Public);
			property?.SetValue(objectFromPath, ConversionHelper.ChangeType(value, property.PropertyType), null);
		}
	}

	public static Type ToType(string typeName)
	{
		return Type.GetType(typeName);
	}

	public static object ExtractIndex(string part)
	{
		return ExtractIndexInternal(part);
	}

	public static string ExtractCodeFromBrackets(string value, string prefix, string postfix)
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

	internal static object ExtractIndexInternal(string part)
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
