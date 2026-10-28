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
		byte[] array = new byte[numberOfBytes];
		m_provider.GetBytes(array);
		return BaseConverter.Convert(10, 36, Math.Abs(BitConverter.ToInt32(array, 0)).ToString());
	}
}
