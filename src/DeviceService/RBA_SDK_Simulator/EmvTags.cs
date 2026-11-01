using System;
using System.Collections.Generic;
using System.Text;

namespace RBA_SDK_Simulator;

/// <summary>Builds the EMV tags a card returns in 33.02 / 33.03 / 33.05 messages.</summary>
public static class EmvTags
{
	public static KeyValuePair<int, byte[]> Tag(int tag, byte[] data)
	{
		return new KeyValuePair<int, byte[]>(tag, data);
	}

	public static List<KeyValuePair<int, byte[]>> ForCard(SimulatedCard card, bool mobileWallet)
	{
		List<KeyValuePair<int, byte[]>> list = new List<KeyValuePair<int, byte[]>>();
		if (!string.IsNullOrEmpty(card.Aid))
		{
			list.Add(Tag(0x4F, FromHex(card.Aid)));
		}
		if (!string.IsNullOrEmpty(card.ApplicationLabel))
		{
			list.Add(Tag(0x50, Encoding.ASCII.GetBytes(card.ApplicationLabel)));
		}
		list.Add(Tag(0x57, FromHex(card.Track2EquivalentHex)));
		list.Add(Tag(0x5A, FromHex(card.Pan.Length % 2 == 0 ? card.Pan : card.Pan + "F")));
		if (!string.IsNullOrEmpty(card.CardholderName))
		{
			list.Add(Tag(0x5F20, Encoding.ASCII.GetBytes(card.CardholderName)));
		}
		// 9F6E: form factor (Visa) / device type (Mastercard). DeviceService uses it to detect mobile wallets.
		if (card.Aid != null && card.Aid.StartsWith("A000000003", StringComparison.OrdinalIgnoreCase))
		{
			list.Add(Tag(0x9F6E, FromHex(mobileWallet ? "26200000" : "20000000")));
		}
		else if (card.Aid != null && card.Aid.StartsWith("A000000004", StringComparison.OrdinalIgnoreCase))
		{
			list.Add(Tag(0x9F6E, FromHex(mobileWallet ? "0000000030310000" : "0000000032300000")));
		}
		return list;
	}

	public static byte[] FromHex(string hex)
	{
		byte[] bytes = new byte[hex.Length / 2];
		for (int i = 0; i < bytes.Length; i++)
		{
			bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
		}
		return bytes;
	}
}
