using System;
using System.Text;

namespace Redbox.HAL.Component.Model.Extensions;

public static class ByteArrayExtensions
{
	public static void Dump(this byte[] array)
	{
		LogHelper.Instance.Log(" -- Byte array dump -- ");
		if (array.Length == 0)
		{
			LogHelper.Instance.Log(" Array is empty");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int num2 = 0;
		while (num < array.Length)
		{
			if (16 == num2)
			{
				num2 = 0;
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat("{0:x2} ", array[num]);
			num++;
			num2++;
		}
		LogHelper.Instance.Log(stringBuilder.ToString());
	}

	public static string AsString(this byte[] array)
	{
		StringBuilder hex = new StringBuilder(array.Length * 2);
		Array.ForEach(array, delegate(byte b)
		{
			hex.AppendFormat("{0:x2} ", b);
		});
		return hex.ToString();
	}
}
