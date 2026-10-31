using System;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Data.Linq;
#endif
using System.Linq;

namespace Redbox.Core;

public static class LinqExtensions
{
#if NETFRAMEWORK
	// LINQ to SQL exists only on .NET Framework.
	public static T ToResult<T>(this ISingleResult<T> result)
	{
		List<T> list = new List<T>(result);
		if (list.Count <= 0)
		{
			return default(T);
		}
		return list[0];
	}
#endif

	public static T ToResult<T>(this IQueryable<T> result)
	{
		List<T> list = new List<T>(result);
		if (list.Count <= 0)
		{
			return default(T);
		}
		return list[0];
	}

	public static T ToResult<T>(this IEnumerable<T> result)
	{
		List<T> list = new List<T>(result);
		if (list.Count <= 0)
		{
			return default(T);
		}
		return list[0];
	}

	public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
	{
		foreach (T item in items)
		{
			action(item);
		}
	}
}
