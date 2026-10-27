using System;
using System.Security.Cryptography;
using System.Text;

namespace KioskClientService.API.App;

public static class StringExtensions
{
	public static string ToSHA512Hash(this string input)
	{
		using SHA512 sHA = SHA512.Create();
		byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(input));
		return BitConverter.ToString(array).Replace("-", "").ToLower();
	}

	public static string MaskLogValue(this string source, int visibleChars)
	{
		if (!string.IsNullOrEmpty(source))
		{
			return new string('X', source.Length - visibleChars) + source.Substring(source.Length - visibleChars);
		}
		return "null";
	}

	public static string EncryptToBase64(this string source)
	{
		return Encoding.UTF8.GetBytes(source).Encrypt().ToBase64();
	}

	public static string DecryptFromBase64(this string source)
	{
		return Encoding.UTF8.GetString(source.Base64ToBytes().Decrypt());
	}

	public static byte[] Base64ToBytes(this string source)
	{
		return Convert.FromBase64String(source);
	}

	public static string EllipseAt(this string source, int length)
	{
		return source.Substring(0, length) + "...";
	}
}
