using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core;

public static class EnumerableExtensions
{
	public static string Join<T>(this IEnumerable<T> list, string separator)
	{
		List<string> list2 = new List<string>();
		foreach (T item in list)
		{
			list2.Add((item == null) ? "(null)" : item.ToString());
		}
		return string.Join(separator, list2.ToArray());
	}

	public static string GetRangedList<T>(this IList<T> listItems, int startIndex, int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = Math.Min(listItems.Count, startIndex + count);
		for (int i = startIndex; i < num; i++)
		{
			if (i < listItems.Count)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(listItems[i]);
			}
		}
		return stringBuilder.ToString();
	}
}
