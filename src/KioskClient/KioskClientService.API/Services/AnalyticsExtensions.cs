using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace KioskClientService.API.Services;

public static class AnalyticsExtensions
{
	public static double? Median<T>(this IEnumerable<T> data, Func<T, int?> selector)
	{
		return data.Select(selector).Median();
	}

	private static double? Median(this IEnumerable<int?> data)
	{
		List<int?> source = data.Where((int? e) => e.HasValue).ToList();
		if (!source.Any())
		{
			return null;
		}
		ImmutableSortedSet<int?> source2 = source.ToImmutableSortedSet();
		int num = source2.Count() / 2;
		if (source2.Count() % 2 == 0)
		{
			return (double?)(source2.ElementAt(num) + source2.ElementAt(num - 1)) / 2.0;
		}
		return source2.ElementAt(num);
	}
}
