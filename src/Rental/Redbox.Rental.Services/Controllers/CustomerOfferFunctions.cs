using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public static class CustomerOfferFunctions
{
	public static string GetSingularTitleFamilyStringFromOffer(CustomerOffer customerOffer)
	{
		return GetSingularTitleFamilyString(GetTitleFamilyFromOffer(customerOffer));
	}

	public static string GetSingularTitleFamilyString(TitleFamily? family)
	{
		return family switch
		{
			TitleFamily.Movies => Resources.Movie, 
			TitleFamily.Games => Resources.game, 
			_ => Resources.disc, 
		};
	}

	public static string GetPluralTitleFamilyStringFromOffer(CustomerOffer customerOffer)
	{
		return GetPluralTitleFamilyString(GetTitleFamilyFromOffer(customerOffer));
	}

	public static string GetPluralTitleFamilyString(TitleFamily? family)
	{
		return family switch
		{
			TitleFamily.Movies => Resources.Movies, 
			TitleFamily.Games => Resources.Games, 
			_ => Resources.disc, 
		};
	}

	public static TitleFamily? GetTitleFamilyFromOffer(CustomerOffer customerOffer)
	{
		TitleFamily? result = customerOffer.TitleFamily;
		if (!result.HasValue)
		{
			if (customerOffer.FormatIds != null && customerOffer.FormatIds.Count > 0)
			{
				result = ((TitleType)customerOffer.FormatIds[0]).GetTitleFamily();
			}
			if (!result.HasValue && customerOffer.ApplicableProducts != null && customerOffer.ApplicableProducts.Count > 0)
			{
				IEnumerable<TitleFamily> source = customerOffer.ApplicableProducts.Select((ITitleProduct p) => p.TitleFamily).Distinct();
				if (source.Count() == 1)
				{
					result = source.First();
				}
			}
		}
		return result;
	}

	public static List<TitleType> GetTitleTypesFromFormatIds(List<int> formatIds)
	{
		List<TitleType> list = new List<TitleType>();
		if (formatIds != null)
		{
			foreach (int formatId in formatIds)
			{
				list.Add((TitleType)formatId);
			}
		}
		return list;
	}

	public static List<TitleType> GetIncludedTitleTypes(List<TitleType> excludedTypes)
	{
		List<TitleType> list = new List<TitleType>();
		foreach (TitleType value in Enum.GetValues(typeof(TitleType)))
		{
			if (value != TitleType.All && value != TitleType.DigitalCode && !excludedTypes.Contains(value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public static bool AreAllGameTypesIncluded(List<TitleType> formats, bool includeFormats)
	{
		bool flag = false;
		List<TitleType> gameTitleTypes = GetGameTitleTypes();
		flag = ((!includeFormats) ? gameTitleTypes.Any((TitleType f) => formats.Contains(f)) : gameTitleTypes.Any((TitleType f) => !formats.Contains(f)));
		return !flag;
	}

	private static List<TitleType> GetGameTitleTypes()
	{
		List<TitleType> list = new List<TitleType>();
		foreach (TitleType value in Enum.GetValues(typeof(TitleType)))
		{
			if (value.GetTitleFamily() == TitleFamily.Games)
			{
				list.Add(value);
			}
		}
		return list;
	}

	public static bool KioskHas4kDiscs()
	{
		ServiceLocator.Instance.GetService<IDataService>().GetProducts(BrowseViewFilter.All, TitleFamily.Movies, TitleType._4KUHD, BrowseSort.Alpha, null, null, null, null, isADAView: false, null, null, out var products);
		return products.Any((IKioskProduct t) => ((ITitleProduct)t).HasBeenInStock);
	}
}
