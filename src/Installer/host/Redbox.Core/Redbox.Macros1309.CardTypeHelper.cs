using System.Text.RegularExpressions;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002ECardTypeHelper
{
	public static Redbox_002EMacros1309_002ECardType? GetCardType(string cardNumber)
	{
		if (NumberMatches(cardNumber, "^(34|37)", 15))
		{
			return Redbox_002EMacros1309_002ECardType.AmericanExpress;
		}
		if (NumberMatches(cardNumber, "^(51|52|53|54|55)", 16))
		{
			return Redbox_002EMacros1309_002ECardType.MasterCard;
		}
		if (NumberMatches(cardNumber, "^(4)", 13) || NumberMatches(cardNumber, "^(4)", 16))
		{
			return Redbox_002EMacros1309_002ECardType.Visa;
		}
		if (NumberMatches(cardNumber, "^(300|301|302|303|304|305|36|38)", 14))
		{
			return Redbox_002EMacros1309_002ECardType.MasterCard;
		}
		if (NumberMatches(cardNumber, "^(2014|2149)", 15) || NumberMatches(cardNumber, "^(6011|6500)", 16))
		{
			return Redbox_002EMacros1309_002ECardType.Discover;
		}
		if (NumberMatches(cardNumber, "^(3)", 16) || NumberMatches(cardNumber, "^(2131|1800)", 15))
		{
			return Redbox_002EMacros1309_002ECardType.JCB;
		}
		return null;
	}

	private static bool NumberMatches(string cardNumber, string regex, int length)
	{
		if (Regex.IsMatch(cardNumber, regex))
		{
			return cardNumber.Length == length;
		}
		return false;
	}
}
