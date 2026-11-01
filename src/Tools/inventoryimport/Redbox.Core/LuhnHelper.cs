using System.Text;

namespace Redbox.Core;

public static class LuhnHelper
{
	public static bool IsCreditCardValid(string cardNumber)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < cardNumber.Length; i++)
		{
			if ("0123456789".IndexOf(cardNumber.Substring(i, 1)) >= 0)
			{
				stringBuilder.Append(cardNumber.Substring(i, 1));
			}
		}
		if (stringBuilder.Length < 13 || stringBuilder.Length > 16)
		{
			return false;
		}
		for (int i = stringBuilder.Length + 1; i <= 16; i++)
		{
			stringBuilder.Insert(0, "0");
		}
		int sum = 0;
		string digits = stringBuilder.ToString();
		for (int i = 1; i <= 16; i++)
		{
			int weight = 1 + i % 2;
			int number = int.Parse(digits.Substring(i - 1, 1));
			int product = number * weight;
			if (product > 9)
			{
				product -= 9;
			}
			sum += product;
		}
		return sum % 10 == 0;
	}
}
