using System;
using System.Security.Cryptography;

namespace UpdateClientService.API.App;

public static class ByteExtensions
{
	public static string GetSHA1Hash(this byte[] data)
	{
		using SHA1Managed sHA1Managed = new SHA1Managed();
		byte[] inArray = sHA1Managed.ComputeHash(data);
		return Convert.ToBase64String(inArray);
	}
}
