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
		ImmutableSortedSet<int?> sorted = source.ToImmutableSortedSet();
		int middle = sorted.Count() / 2;
		if (sorted.Count() % 2 == 0)
		{
			return (double?)(sorted.ElementAt(middle) + sorted.ElementAt(middle - 1)) / 2.0;
		}
		return sorted.ElementAt(middle);
	}
}
