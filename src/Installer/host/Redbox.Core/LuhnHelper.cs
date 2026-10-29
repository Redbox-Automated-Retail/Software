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
		int num = 0;
		string text = stringBuilder.ToString();
		for (int k = 1; k <= 16; k++)
		{
			int num2 = 1 + k % 2;
			int num3 = int.Parse(text.Substring(k - 1, 1));
			int num4 = num3 * num2;
			if (num4 > 9)
			{
				num4 -= 9;
			}
			num += num4;
		}
		return num % 10 == 0;
	}
}
