using System;
using System.Collections.Generic;
using System.Linq;
using Wintellect.PowerCollections;

namespace Redbox.REDS.Framework;

internal static class ResourceLookup
{
	internal sealed class ScoredResource
	{
		public int Score { get; set; }

		public IResource Resource { get; set; }
	}

	public static IResource GetResource(MultiDictionary<string, IResource> resources, string name, IResourceBundleFilter filter)
	{
		ICollection<IResource> collection = resources[name];
		if (collection == null)
		{
			return null;
		}
		if (collection.Count == 1)
		{
			IResource resource = new List<IResource>(collection)[0];
			if (IsResourceEffective(resource))
			{
				return resource;
			}
			return null;
		}
		List<ScoredResource> list = new List<ScoredResource>();
		IResource each;
		foreach (IResource item in collection)
		{
			each = item;
			if (!IsResourceEffective(each))
			{
				continue;
			}
			int num = 0;
			if (filter != null)
			{
				List<IPropertyType> list2 = (from f in each.Type.GetFilterProperties()
					where each[f.Name] != null
					select f).ToList();
				if (list2.Count == 0)
				{
					num++;
				}
				else
				{
					int num2 = list2.Count((IPropertyType eachFilter) => each.PropertyEquals(eachFilter.Name, filter[eachFilter.Name]));
					if (num2 == list2.Count)
					{
						num += list2.Sum((IPropertyType eachFilter) => eachFilter.FilterScore);
					}
				}
			}
			list.Add(new ScoredResource
			{
				Score = num,
				Resource = each
			});
		}
		list.Sort((ScoredResource x, ScoredResource y) => y.Score.CompareTo(x.Score));
		if (list.Count <= 0)
		{
			return null;
		}
		return list[0].Resource;
	}

	public static IResource GetResource2(MultiDictionary<string, IResource> resources, string name, IResourceBundleFilter filter)
	{
		ICollection<IResource> collection = resources[name];
		if (collection == null)
		{
			return null;
		}
		if (collection.Count == 1)
		{
			IResource resource = new List<IResource>(collection)[0];
			if (IsResourceEffective(resource))
			{
				return resource;
			}
			return null;
		}
		List<ScoredResource> list = new List<ScoredResource>();
		IResource each;
		foreach (IResource item in collection)
		{
			each = item;
			if (!IsResourceEffective(each))
			{
				continue;
			}
			int num = 0;
			if (filter != null)
			{
				List<IPropertyType> list2 = (from f in each.Type.GetFilterProperties()
					where each[f.Name] != null
					select f).ToList();
				if (list2.Count == 0)
				{
					num++;
				}
				else
				{
					int num2 = list2.Count((IPropertyType eachFilter) => each.PropertyEquals2(eachFilter.Name, (eachFilter.Name.Substring(0, 2) == "!=") ? filter[eachFilter.Name.Substring(2)] : filter[eachFilter.Name]));
					if (num2 == list2.Count)
					{
						num += list2.Sum((IPropertyType eachFilter) => eachFilter.FilterScore);
					}
				}
			}
			list.Add(new ScoredResource
			{
				Score = num,
				Resource = each
			});
		}
		list.Sort((ScoredResource x, ScoredResource y) => y.Score.CompareTo(x.Score));
		if (list.Count <= 0)
		{
			return null;
		}
		return list[0].Resource;
	}

	private static bool IsResourceEffective(IResource resource)
	{
		DateTime now = DateTime.Now;
		DateTime? dateTime = null;
		if (resource["effective_date"] != null)
		{
			dateTime = (DateTime?)resource["effective_date"];
		}
		DateTime? dateTime2 = null;
		if (resource["expiry_date"] != null)
		{
			dateTime2 = (DateTime?)resource["expiry_date"];
		}
		if (dateTime.HasValue)
		{
			DateTime value = now;
			DateTime? dateTime3 = dateTime;
			if (value < dateTime3)
			{
				return false;
			}
		}
		if (dateTime2.HasValue)
		{
			DateTime value2 = now;
			DateTime? dateTime4 = dateTime2;
			if (value2 > dateTime4)
			{
				return false;
			}
		}
		if (resource["daypart_dayofweek_only"] != null && !resource.PropertyEquals2("daypart_dayofweek_only", now.DayOfWeek.ToString()))
		{
			return false;
		}
		DayOfWeek? dayOfWeek = null;
		if (resource["daypart_effective_weekday"] != null)
		{
			dayOfWeek = (DayOfWeek?)resource["daypart_effective_weekday"];
		}
		DayOfWeek? dayOfWeek2 = null;
		if (resource["daypart_expiry_weekday"] != null)
		{
			dayOfWeek2 = (DayOfWeek?)resource["daypart_expiry_weekday"];
		}
		if (dayOfWeek.HasValue && now.DayOfWeek < dayOfWeek)
		{
			return false;
		}
		if (dayOfWeek2.HasValue && now.DayOfWeek > dayOfWeek2)
		{
			return false;
		}
		TimeSpan? timeSpan = null;
		if (resource["daypart_effective_time"] != null)
		{
			timeSpan = (TimeSpan?)resource["daypart_effective_time"];
		}
		TimeSpan? timeSpan2 = null;
		if (resource["daypart_expiry_time"] != null)
		{
			timeSpan2 = (TimeSpan?)resource["daypart_expiry_time"];
		}
		if (timeSpan.HasValue)
		{
			TimeSpan timeOfDay = now.TimeOfDay;
			TimeSpan? timeSpan3 = timeSpan;
			if (timeOfDay < timeSpan3)
			{
				return false;
			}
		}
		if (timeSpan2.HasValue)
		{
			TimeSpan timeOfDay2 = now.TimeOfDay;
			TimeSpan? timeSpan4 = timeSpan2;
			if (timeOfDay2 > timeSpan4)
			{
				return false;
			}
		}
		return true;
	}
}
