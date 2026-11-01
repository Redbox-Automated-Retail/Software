using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RBA_SDK_Simulator;

/// <summary>
/// Builds the P87_RES_CARD_DATA string of an 87 (E2EE / OnGuard) card read, in the layout
/// IUC285Proxy.ParseOnGuardData reads. Encrypted fields are dummy values: they have the right shape
/// but cannot be decrypted by a payment processor.
/// </summary>
public static class OnGuardCardData
{
	private const string GiftCardBin = "601056";

	public static string Build(SimulatedCard card)
	{
		// Redbox gift cards are whitelisted on the terminal and come back as clear track 2 data.
		if (card.Pan.StartsWith(GiftCardBin, StringComparison.Ordinal))
		{
			return $"{card.Pan}={card.Expiry}{card.ServiceCode}0000000000";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(card.Pan.Substring(0, 6));
		stringBuilder.Append(card.Pan.Substring(card.Pan.Length - 4));
		stringBuilder.Append(card.Pan.Length.ToString("00", CultureInfo.InvariantCulture));
		stringBuilder.Append(LuhnValid(card.Pan) ? "1" : "0");
		stringBuilder.Append(card.Expiry);
		stringBuilder.Append(card.ServiceCode);
		stringBuilder.Append("0");
		string text = card.CardholderName ?? string.Empty;
		stringBuilder.Append(text.Length.ToString("00", CultureInfo.InvariantCulture));
		stringBuilder.Append(text);
		if (card.Encrypted)
		{
			stringBuilder.Append("1");
			stringBuilder.Append("4");
			stringBuilder.Append(DummyHex(card.Pan, 20).ToUpperInvariant());
			stringBuilder.Append("0001");
			AppendField(stringBuilder, DummyBase64(card.Pan + "IC", 32));
			AppendField(stringBuilder, DummyBase64(card.Pan + "AES", 16));
			AppendField(stringBuilder, DummyBase64(card.Pan + "LS", 32));
		}
		else
		{
			stringBuilder.Append("0");
			AppendField(stringBuilder, card.Track1);
			AppendField(stringBuilder, card.Track2);
		}
		stringBuilder.Append("0");
		return stringBuilder.ToString();
	}

	private static void AppendField(StringBuilder sb, string value)
	{
		sb.Append(value.Length.ToString("00", CultureInfo.InvariantCulture));
		sb.Append(value);
	}

	private static byte[] DummyBytes(string seed, int length)
	{
		byte[] bytes = new byte[length];
		int seedHash = seed.Aggregate(17, (int h, char c) => h * 31 + c);
		Random random = new Random(seedHash);
		random.NextBytes(bytes);
		return bytes;
	}

	private static string DummyHex(string seed, int length)
	{
		return string.Concat(DummyBytes(seed, length / 2).Select((byte b) => b.ToString("x2", CultureInfo.InvariantCulture)));
	}

	/// <summary>Base64 of a length that is not a multiple of 3, so it ends in '=' like the real encrypted blocks.</summary>
	private static string DummyBase64(string seed, int length)
	{
		return Convert.ToBase64String(DummyBytes(seed, length + ((length % 3 == 0) ? 1 : 0)));
	}

	private static bool LuhnValid(string pan)
	{
		int sum = 0;
		bool doubleDigit = false;
		for (int i = pan.Length - 1; i >= 0; i--)
		{
			if (!char.IsDigit(pan[i]))
			{
				return false;
			}
			int digit = pan[i] - 48;
			if (doubleDigit)
			{
				digit *= 2;
				if (digit > 9)
				{
					digit -= 9;
				}
			}
			sum += digit;
			doubleDigit = !doubleDigit;
		}
		return sum % 10 == 0;
	}
}
