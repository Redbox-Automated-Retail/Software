using System.Security.Cryptography;
using System.Text;

namespace Redbox.Core;

public class IdHelper
{
	private const string HumanReadableChars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

	private static readonly RNGCryptoServiceProvider m_provider = new RNGCryptoServiceProvider();

	public static string GeneratePseudoUniqueId()
	{
		return GeneratePseudoUniqueId(8, "23456789ABCDEFGHJKLMNPQRSTUVWXYZ");
	}

	public static string GeneratePseudoUniqueId(int codeLength)
	{
		return GeneratePseudoUniqueId(codeLength, "23456789ABCDEFGHJKLMNPQRSTUVWXYZ");
	}

	public static string GeneratePseudoUniqueId(string validChars)
	{
		return GeneratePseudoUniqueId(8, validChars);
	}

	public static string GeneratePseudoUniqueId(int codeLength, string validChars)
	{
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = new byte[codeLength];
		m_provider.GetBytes(array);
		for (int i = 0; i < codeLength; i++)
		{
			int startIndex = array[i] % validChars.Length;
			stringBuilder.Append(validChars.Substring(startIndex, 1));
		}
		return stringBuilder.ToString();
	}
}
