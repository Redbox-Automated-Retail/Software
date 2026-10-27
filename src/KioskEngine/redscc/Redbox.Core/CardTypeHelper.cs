using System.Text.RegularExpressions;

namespace Redbox.Core;

public static class CardTypeHelper
{
	public static CardType? GetCardType(string cardNumber)
	{
		if (NumberMatches(cardNumber, "^(34|37)", 15))
		{
			return CardType.AmericanExpress;
		}
		if (NumberMatches(cardNumber, "^(51|52|53|54|55)", 16))
		{
			return CardType.MasterCard;
		}
		if (NumberMatches(cardNumber, "^(4)", 13) || NumberMatches(cardNumber, "^(4)", 16))
		{
			return CardType.Visa;
		}
		if (NumberMatches(cardNumber, "^(300|301|302|303|304|305|36|38)", 14))
		{
			return CardType.MasterCard;
		}
		if (NumberMatches(cardNumber, "^(2014|2149)", 15) || NumberMatches(cardNumber, "^(6011|6500)", 16))
		{
			return CardType.Discover;
		}
		if (NumberMatches(cardNumber, "^(3)", 16) || NumberMatches(cardNumber, "^(2131|1800)", 15))
		{
			return CardType.JCB;
		}
		return null;
	}

	private static bool NumberMatches(string cardNumber, string regex, int length)
	{
		return Regex.IsMatch(cardNumber, regex) && cardNumber.Length == length;
	}
}
