using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core;

public static class StringBuilderExtensions
{
	public static void ToLines(this StringBuilder builder, List<string> lines)
	{
		string text = builder.ToString();
		int lineStart = 0;
		for (int index = text.IndexOf(Environment.NewLine); index != -1; index = text.IndexOf(Environment.NewLine, lineStart))
		{
			lines.Add(text.Substring(lineStart, index - lineStart));
			lineStart = index + Environment.NewLine.Length;
		}
	}
}
