using System;
using System.Security.Cryptography;

namespace Redbox.HAL.Core;

public class IdHelper
{
	private static readonly RNGCryptoServiceProvider m_provider = new RNGCryptoServiceProvider();

	public static string GeneratePseudoUniqueId()
	{
		return GeneratePseudoUniqueId(8);
	}

	public static string GeneratePseudoUniqueId(int numberOfBytes)
	{
		byte[] buffer = new byte[numberOfBytes];
		m_provider.GetBytes(buffer);
		return BaseConverter.Convert(10, 36, Math.Abs(BitConverter.ToInt32(buffer, 0)).ToString());
	}
}
