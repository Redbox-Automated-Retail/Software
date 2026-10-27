using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core;

public static class StringBuilderExtensions
{
	public static void ToLines(this StringBuilder builder, List<string> lines)
	{
		string text = builder.ToString();
		int num = 0;
		for (int num2 = text.IndexOf(Environment.NewLine); num2 != -1; num2 = text.IndexOf(Environment.NewLine, num))
		{
			lines.Add(text.Substring(num, num2 - num));
			num = num2 + Environment.NewLine.Length;
		}
	}
}
