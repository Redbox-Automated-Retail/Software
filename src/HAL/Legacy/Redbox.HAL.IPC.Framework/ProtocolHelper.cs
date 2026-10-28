using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public static class ProtocolHelper
{
	public const string NotApplicable = "N/A";

	public static byte? ParseByte(string data)
	{
		if (data == "N/A")
		{
			return null;
		}
		if (byte.TryParse(data, out var result))
		{
			return result;
		}
		return null;
	}

	public static long? ParseLong(string data)
	{
		if (data == "N/A")
		{
			return null;
		}
		if (long.TryParse(data, out var result))
		{
			return result;
		}
		return null;
	}

	public static DateTime? ParseDate(string data)
	{
		if (data == "N/A")
		{
			return null;
		}
		if (DateTime.TryParse(data, out var result))
		{
			return DateTime.SpecifyKind(result, DateTimeKind.Utc);
		}
		return null;
	}

	public static TimeSpan? ParseTimeSpan(string data)
	{
		if (data == "N/A")
		{
			return null;
		}
		if (TimeSpan.TryParse(data, out var result))
		{
			return result;
		}
		return null;
	}

	public static ReadOnlyCollection<string> ParseProperties(string data)
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		Stack<char> stack = new Stack<char>();
		foreach (char c in data)
		{
			if (c == '|' && num == 0)
			{
				list.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
				continue;
			}
			if (c == '<')
			{
				stack.Push('>');
				num++;
				if (num == 1)
				{
					continue;
				}
			}
			if (c == '[' && (stack.Count == 0 || stack.Peek() != '>'))
			{
				stack.Push(']');
				num++;
				if (num == 1)
				{
					continue;
				}
			}
			if (stack.Count > 0 && c == stack.Peek())
			{
				num--;
				if (num == 0)
				{
					continue;
				}
			}
			stringBuilder.Append(c);
		}
		list.Add(stringBuilder.ToString());
		return list.AsReadOnly();
	}

	public static void FormatErrors(ErrorList errors, List<string> messages)
	{
		foreach (Error error in errors)
		{
			messages.Add($"|*{error}|{error.Details}*|");
		}
	}

	public static void FormatErrors(ErrorList errors, CommandContext context)
	{
		FormatErrors(errors, context.Messages);
	}

	public static void FormatEventMessages(IEnumerable<string> messages, CommandContext context)
	{
		foreach (string message in messages)
		{
			context.Messages.Add(message);
		}
	}

	public static void FormatFile(string path, CommandContext context)
	{
		byte[] inputArray = File.ReadAllBytes(path);
		context.Messages.Add(ByteHelper.ToBase64(inputArray));
	}

	public static void FormatCompressedFile(string path, CommandContext context)
	{
		CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
		byte[] inputArray = File.ReadAllBytes(path);
		byte[] buffer = algorithm.Compress(Encoding.ASCII.GetBytes(ByteHelper.ToBase64(inputArray)));
		FormatGzipCompressedMessage(buffer, context);
	}

	public static void FormatCompressedString(string value, CommandContext context)
	{
		CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA);
		byte[] buffer = algorithm.Compress(Encoding.ASCII.GetBytes(value));
		FormatLzmaCompressedMessage(buffer, context);
	}

	public static void FormatCompressedBytes(byte[] buffer, CommandContext context)
	{
		CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA);
		byte[] buffer2 = algorithm.Compress(buffer);
		FormatLzmaCompressedMessage(buffer2, context);
	}

	public static byte[] DecompressBase64String(string value)
	{
		CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA);
		return algorithm.Decompress(StringExtensions.Base64ToBytes(value));
	}

	private static void FormatLzmaCompressedMessage(byte[] buffer, CommandContext context)
	{
		context.Messages.Add($"LZMA|{ByteHelper.ToBase64(buffer)}");
	}

	private static void FormatGzipCompressedMessage(byte[] buffer, CommandContext context)
	{
		context.Messages.Add($"GZIP|{ByteHelper.ToBase64(buffer)}");
	}
}
