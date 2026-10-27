using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductPricing.Model;

public static class PricingExtensions
{
	private static List<T> GetActiveProductPricing<T>(this List<T> ppList, DateTime kioskDT) where T : IPricing
	{
		return ppList.Where(delegate(T p)
		{
			if (p.StartDate > kioskDT)
			{
				return false;
			}
			if (p.EndDate.HasValue && kioskDT > p.EndDate.Value)
			{
				return false;
			}
			if (p.DaysOfWeek.HasValue)
			{
				DaysOfWeek value = (DaysOfWeek)p.DaysOfWeek.Value;
				if (kioskDT.DayOfWeek == DayOfWeek.Sunday && !value.HasFlag(DaysOfWeek.Sunday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Monday && !value.HasFlag(DaysOfWeek.Monday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Tuesday && !value.HasFlag(DaysOfWeek.Tuesday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Wednesday && !value.HasFlag(DaysOfWeek.Wednesday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Thursday && !value.HasFlag(DaysOfWeek.Thursday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Friday && !value.HasFlag(DaysOfWeek.Friday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Saturday && !value.HasFlag(DaysOfWeek.Saturday))
				{
					return false;
				}
			}
			return (!p.StartTime.HasValue || !p.EndTime.HasValue || TimeBetween(kioskDT, p.StartTime.Value, p.EndTime.Value)) ? true : false;
		}).ToList();
	}

	private static bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
	{
		TimeSpan timeOfDay = datetime.TimeOfDay;
		if (start < end)
		{
			if (start <= timeOfDay)
			{
				return timeOfDay <= end;
			}
			return false;
		}
		if (end < timeOfDay)
		{
			return !(timeOfDay < start);
		}
		return true;
	}

	public static T GetActiveDefaultRentalPricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, PriceLevelType plt) where T : IRentalPricing
	{
		return ppList.GetActiveProductPricing(kioskDT).Where(delegate(T p)
		{
			T val = p;
			if (val.PriceLevelType != plt)
			{
				val = p;
				if (val.PriceLevelType == PriceLevelType.Title)
				{
					val = p;
					return Convert.ToInt32(val.PriceLevelTypeValue) == titleId;
				}
				return false;
			}
			return true;
		}).OrderByDescending(delegate(T p)
		{
			T val = p;
			return val.StartDate;
		})
			.ThenBy(delegate(T p)
			{
				T val = p;
				return val.LocationRank;
			})
			.ThenBy(delegate(T p)
			{
				T val = p;
				return val.InitialNight;
			})
			.FirstOrDefault();
	}

	public static T GetActiveRentalPricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, PriceLevelType plt) where T : IRentalPricing
	{
		return ppList.GetActiveProductPricing(kioskDT).Where(delegate(T p)
		{
			T val = p;
			if (val.PriceLevelType != plt)
			{
				val = p;
				if (val.PriceLevelType == PriceLevelType.Title)
				{
					val = p;
					if (Convert.ToInt32(val.PriceLevelTypeValue) == titleId)
					{
						goto IL_0046;
					}
				}
				return false;
			}
			goto IL_0046;
			IL_0046:
			val = p;
			return val.InitialDays == 1;
		}).OrderByDescending(delegate(T p)
		{
			T val = p;
			return val.StartDate;
		})
			.ThenBy(delegate(T p)
			{
				T val = p;
				return val.InitialNight;
			})
			.FirstOrDefault();
	}

	public static List<T> GetActiveMNPRentalPricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, PriceLevelType plt) where T : IRentalPricing
	{
		return ppList.GetActiveProductPricing(kioskDT).Where(delegate(T p)
		{
			T val = p;
			if (val.PriceLevelType != plt)
			{
				val = p;
				if (val.PriceLevelType == PriceLevelType.Title)
				{
					val = p;
					if (Convert.ToInt32(val.PriceLevelTypeValue) == titleId)
					{
						goto IL_0046;
					}
				}
				return false;
			}
			goto IL_0046;
			IL_0046:
			val = p;
			return val.InitialDays > 1;
		}).OrderByDescending(delegate(T p)
		{
			T val = p;
			return val.StartDate;
		})
			.ThenBy(delegate(T p)
			{
				T val = p;
				return val.InitialNight;
			})
			.ToList();
	}

	public static T GetActivePurchasePricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, bool useLowestPriceSortOrder) where T : IPurchasePricing
	{
		List<T> activeProductPricing = ppList.GetActiveProductPricing(kioskDT);
		if (useLowestPriceSortOrder)
		{
			return activeProductPricing.Where(delegate(T p)
			{
				T val = p;
				return val.ProductId == titleId;
			}).OrderByDescending(delegate(T p)
			{
				T val = p;
				return val.PricingId;
			}).ThenBy(delegate(T p)
			{
				T val = p;
				return val.PurchasePrice;
			})
				.FirstOrDefault();
		}
		return activeProductPricing.Where(delegate(T p)
		{
			T val = p;
			return val.ProductId == titleId;
		}).OrderByDescending(delegate(T p)
		{
			T val = p;
			return val.PricingId;
		}).ThenBy(delegate(T p)
		{
			T val = p;
			return val.LocationRank;
		})
			.ThenByDescending(delegate(T p)
			{
				T val = p;
				return val.StartDate;
			})
			.ThenBy(delegate(T p)
			{
				T val = p;
				return val.PurchasePrice;
			})
			.FirstOrDefault();
	}
}
