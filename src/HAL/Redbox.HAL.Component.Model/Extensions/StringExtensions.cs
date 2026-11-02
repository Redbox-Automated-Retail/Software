using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Redbox.HAL.Component.Model.Extensions;

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

	public static byte[] HexToBytes(string source)
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

	public static byte[] Base64ToBytes(string source)
	{
		return Convert.FromBase64String(source);
	}

	public static object? GetValueForPath(string path, object rootObject)
	{
		return GetObjectFromPath(path, rootObject, null);
	}

	public static void SetValueForPath(string path, object rootObject, object? value)
	{
		object? parent = GetObjectFromPath(path, rootObject, 1);
		if (parent != null)
		{
			string[] parts = path.Split(PropertyPathSeparators);
			PropertyInfo? property = parent.GetType().GetProperty(parts[parts.Length - 1], BindingFlags.Instance | BindingFlags.Public);
			property?.SetValue(parent, ConversionHelper.ChangeType(value, property.PropertyType), null);
		}
	}

	public static Type? ToType(string typeName)
	{
		return Type.GetType(typeName);
	}

	public static object? ExtractIndex(string? part)
	{
		if (part == null)
		{
			return null;
		}
		Match stringIndexer = Regex.Match(part, "\\[\"(?<indexer>.*?)\"\\]", RegexOptions.Singleline);
		Match numericIndexer = Regex.Match(part, "\\[(?<indexer>[0-9]*?)\\]", RegexOptions.Singleline);
		object? index = null;
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

	public static string? ExtractCodeFromBrackets(string value, string prefix, string postfix)
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

	private static object? GetObjectFromPath(string? path, object? rootObject, int? depthAdjust)
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
		object? current = rootObject;
		for (int i = 0; i < depth; i++)
		{
			object[]? args = null;
			string member = parts[i];
			object? index = ExtractIndex(member);
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
