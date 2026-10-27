using System.Collections.Generic;
using System.Linq;

namespace Redbox.KioskEngine.ComponentModel;

public static class ListExtensions
{
	public static bool IsEqualTo<T>(this List<T> list1, List<T> list2)
	{
		if (list1 != null && list2 != null)
		{
			if (list1.Count == list2.Count)
			{
				IEnumerable<T> source = list1.Except(list2);
				IEnumerable<T> source2 = list2.Except(list1);
				if (!source.Any())
				{
					return !source2.Any();
				}
				return false;
			}
			return false;
		}
		if (list1 == null && list2 == null)
		{
			return true;
		}
		return false;
	}
}
