using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redbox.Core;

public static class CardTypeHelper
{
	private static bool m_isInitialized;

	private static RedboxGiftCard m_rbGiftCard = new RedboxGiftCard();

	public static void Initialize(List<BinRange> rbGiftCardBinRanges)
	{
		if (!m_isInitialized)
		{
			m_rbGiftCard.AddBinRanges(rbGiftCardBinRanges);
			m_isInitialized = true;
		}
	}

	public static void ResetGiftCardRange(List<BinRange> ranges)
	{
		m_rbGiftCard.ClearRanges();
		m_rbGiftCard.AddBinRanges(ranges);
	}

	public static CardType? GetCardType(string cardNumber)
	{
		return GetCardType(cardNumber, DateTime.UtcNow);
	}

	public static CardType? GetCardType(string cardNumber, DateTime now)
	{
		return GetCardType(cardNumber, now, checkGCRanges: true);
	}

	public static CardType? GetCardType(string cardNumber, bool checkGCRanges)
	{
		return GetCardType(cardNumber, DateTime.UtcNow, checkGCRanges);
	}

	public static CardType? GetCardType(string cardNumber, DateTime now, bool checkGCRanges)
	{
		if (checkGCRanges && m_rbGiftCard.IsRedboxGiftCard(cardNumber))
		{
			return CardType.RedboxGiftCard;
		}
		if (NumberMatches(cardNumber, "^(4)", 16))
		{
			return CardType.Visa;
		}
		if (NumberMatches(cardNumber, "^(34|37)", 15))
		{
			return CardType.AmericanExpress;
		}
		if (NumberMatches(cardNumber, "^(51|52|53|54|55)", 16))
		{
			return CardType.MasterCard;
		}
		if (NumberMatches(cardNumber, "^(222[1-9]|22[3-9]|23|24|25|26|27[0-1]|2720)", 16))
		{
			return CardType.MasterCard;
		}
		if (NumberMatches(cardNumber, "^(30[0-5]|3095|3[8-9]|6011[0|2-4]|60117[4|7-9]|60118[6-9]|60119|62212[6-9]|6221[3-9]|622[2-8]|6229[0-1]|62292[0-5]|62[4-6]|628[2-8]|64[4-9]|65)", 16) || NumberMatches(cardNumber, "^(36)", 14))
		{
			return CardType.Discover;
		}
		return null;
	}

	private static bool NumberMatches(string cardNumber, string regex, int minLength, int maxLength = 19)
	{
		if (Regex.IsMatch(cardNumber, regex))
		{
			if (cardNumber.Length >= minLength)
			{
				return cardNumber.Length <= maxLength;
			}
			return false;
		}
		return false;
	}
}
