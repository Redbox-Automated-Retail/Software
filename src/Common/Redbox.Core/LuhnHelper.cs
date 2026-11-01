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
		for (int j = stringBuilder.Length + 1; j <= 16; j++)
		{
			stringBuilder.Insert(0, "0");
		}
		int sum = 0;
		string digits = stringBuilder.ToString();
		for (int k = 1; k <= 16; k++)
		{
			int weight = 1 + k % 2;
			int number = int.Parse(digits.Substring(k - 1, 1)) * weight;
			if (number > 9)
			{
				number -= 9;
			}
			sum += number;
		}
		return sum % 10 == 0;
	}
}
