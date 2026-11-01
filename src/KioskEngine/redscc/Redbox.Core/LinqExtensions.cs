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
	// LINQ to SQL (System.Data.Linq) is .NET Framework only.
	public static T ToResult<T>(this ISingleResult<T> result)
	{
		List<T> list = new List<T>(result);
		return (list.Count > 0) ? list[0] : default(T);
	}
#endif

	public static T ToResult<T>(this IQueryable<T> result)
	{
		List<T> list = new List<T>(result);
		return (list.Count > 0) ? list[0] : default(T);
	}

	public static T ToResult<T>(this IEnumerable<T> result)
	{
		List<T> list = new List<T>(result);
		return (list.Count > 0) ? list[0] : default(T);
	}

	public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
	{
		foreach (T item in items)
		{
			action(item);
		}
	}
}
