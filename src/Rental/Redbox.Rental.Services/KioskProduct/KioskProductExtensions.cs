using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskClientService.Products;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskProduct;

public static class KioskProductExtensions
{
	public static List<ITitleProduct> InCartTitleProductFilters(this List<ITitleProduct> source, TitleFamily titleFamily, TitleType titleType, List<long> include, List<long> exclude)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		if (titleType == TitleType.Bluray)
		{
			list.AddRange(from item in source.FilterToBlueray()
				select (item));
		}
		else
		{
			switch (titleFamily)
			{
			case TitleFamily.All:
				list.AddRange(source);
				break;
			case TitleFamily.Movies:
				list.AddRange(source.Where((ITitleProduct item) => item.IsMovie));
				break;
			case TitleFamily.Games:
				list.AddRange(source.Where((ITitleProduct item) => item.IsGame));
				break;
			}
			if (titleType != TitleType.All)
			{
				list = list.Where((ITitleProduct item) => item.ApplyTitleTypeFilter(titleType)).ToList();
			}
		}
		if ((include != null && include.Count > 0) || (exclude != null && exclude.Count > 0))
		{
			list = list.FilterInCarts(include, exclude);
		}
		return list;
	}

	public static List<ITitleProduct> FilterInCarts(this List<ITitleProduct> list, List<long> include, List<long> exclude)
	{
		if (include != null || exclude != null)
		{
			list = list.FilterToTitles();
			if (include != null && include.Count() > 0)
			{
				list = list.Where((ITitleProduct item) => item.HasTitle(include)).ToList();
			}
			else if (exclude != null && exclude.Count() > 0)
			{
				list = list.Where((ITitleProduct item) => !item.HasTitle(exclude)).ToList();
			}
		}
		return list;
	}

	public static List<ITitleProduct> FilterToBlueray(this List<ITitleProduct> kioskProducts)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		kioskProducts.ForEach(delegate(ITitleProduct item)
		{
			if (item is TitleRollupProduct)
			{
				list.AddRange(((TitleRollupProduct)item).KioskProducts.FilterToBlueray());
			}
			if (item is MDVProduct && ((MDVProduct)item).TitleType == TitleType.Bluray)
			{
				list.Add(item);
			}
			if (item is TitleProduct && ((TitleProduct)item).TitleType == TitleType.Bluray)
			{
				list.Add(item);
			}
		});
		return list;
	}

	public static List<ITitleProduct> FilterToTitles(this List<ITitleProduct> kioskProducts)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		kioskProducts.ForEach(delegate(ITitleProduct item)
		{
			if (item is TitleRollupProduct)
			{
				list.AddRange(((TitleRollupProduct)item).KioskProducts.FilterToTitles());
			}
			if (item is MDVProduct || item is TitleProduct)
			{
				list.Add(item);
			}
		});
		return list;
	}

	public static bool ApplyTitleTypeFilter(this ITitleProduct titleProduct, TitleType titleType)
	{
		return titleProduct.TitleType == titleType;
	}

	public static bool ApplyGenreFilter(this ITitleProduct titleProduct, long genreFilter)
	{
		return titleProduct.Genres.Contains(genreFilter);
	}

	public static List<ITitleProduct> OrderList(this List<ITitleProduct> source, BrowseSort browseSort, BrowseViewFilter browseFilter, List<Title> sortedMovieTitles)
	{
		LogHelper.Instance.Log($"KioskProduct.OrderList - Product Count: {source.Count}, BrowseSort: {browseSort.ToString()}, BrowseViewFilter: {browseFilter.ToString()}");
		string outOfStockTitleDetailsABTestVersion = Redbox.Rental.Services.Configuration.Configuration.Instance.OutOfStockTitleDetailsABTestVersion;
		switch (browseSort)
		{
		case BrowseSort.MerchandiseDate:
			return (from item in source
				orderby (!(outOfStockTitleDetailsABTestVersion == "B") || item.TitleFamily != TitleFamily.Movies) && !InStock(item, browseFilter), item.SortMerchandise, item.SortBoxOfficeGross descending, item.SortName descending
				select item).ToList();
		case BrowseSort.Alpha:
			return source.OrderBy((ITitleProduct item) => item.SortAlpha).ToList();
		case BrowseSort.InitialNightPrice:
			return (from item in source
				orderby item.IsEmptyCase, SortPrice(item, browseFilter), item.SortAlpha
				select item).ToList();
		case BrowseSort.MerchandiseDateDescending:
			return (from item in source
				orderby item.ShowOutOfStock, item.IsEmptyCase, item.SortMerchandise descending, item.SortBoxOfficeGross descending, item.SortName
				select item).ToList();
		default:
		{
			ProductGroupSortOrder sortOrder = ServiceLocator.Instance.GetService<IProductGroupService>()?.GetSortOrder();
			IAnalyticsService service = ServiceLocator.Instance.GetService<IAnalyticsService>();
			try
			{
				if (sortOrder != null && sortOrder.SortOrderType > 0 && sortOrder.SortOrders != null && sortOrder.SortOrders.Count > 0)
				{
					source.ForEach(delegate(ITitleProduct item)
					{
						item.SortOrderPosition = 0;
						item.SortOrderType = 0;
					});
					service?.AddSessionProperty("SortOrderType", sortOrder.SortOrderType);
					var list = (from p in source
						join so in sortOrder.SortOrders on p.TitleRollupProductGroupId equals so.ProductGroupId into xso
						from subso in xso.DefaultIfEmpty()
						select new
						{
							p = p,
							Position = subso?.Position
						}).ToList();
					list.ForEach(item =>
					{
						item.p.SortOrderPosition = item.Position.GetValueOrDefault();
						item.p.SortOrderType = sortOrder.SortOrderType;
					});
					List<long> outOfStockTitleRollupProductGroupIdsToShow = (from item in list
						where item.p.ShowOutOfStock
						orderby item.Position ?? int.MaxValue, item.p.SortMerchandise descending, item.p.SortBoxOfficeGross descending, item.p.SortName descending
						select item.p.TitleRollupProductGroupId).Take(Redbox.Rental.Services.Configuration.Configuration.Instance.Current.Browse.NumberOfOutOfStockProductGroupsToShow).ToList();
					return (from item in list
						orderby item.p.ShowOutOfStock && !outOfStockTitleRollupProductGroupIdsToShow.Contains(item.p.TitleRollupProductGroupId), item.p.IsEmptyCase, item.Position ?? int.MaxValue, item.p.SortMerchandise descending, item.p.SortBoxOfficeGross descending, item.p.SortName descending
						select item.p).ToList();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("KioskProduct.OrderList - an exception occurred, skipping SortOrderTest", e);
			}
			source.ForEach(delegate(ITitleProduct item)
			{
				item.SortOrderPosition = 0;
				item.SortOrderType = 0;
			});
			try
			{
				if (sortedMovieTitles != null && sortedMovieTitles.Count > 0)
				{
					(from p in source
						join smt in sortedMovieTitles on p.ProductId equals smt.ProductId
						select new { p, smt }).ForEach(x =>
					{
						x.p.SortOrderPosition = x.smt.SortOrderPosition;
					});
				}
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("KioskProduct.OrderList - an exception occurred, skipping SortOrderPosition", e2);
			}
			service?.AddSessionProperty("SortOrderType", 0);
			return (from item in source
				orderby item.ShowOutOfStock, item.SortDate descending, item.SortBoxOfficeGross descending, item.SortName descending
				select item).ToList();
		}
		}
	}

	private static bool InStock(ITitleProduct titleProduct, BrowseViewFilter browseFilter)
	{
		if (titleProduct == null)
		{
			return false;
		}
		return browseFilter switch
		{
			BrowseViewFilter.All => titleProduct.IsRentableAndInStock, 
			BrowseViewFilter.ForSale => titleProduct.IsSellableAndInStock, 
			_ => false, 
		};
	}

	private static decimal SortPrice(ITitleProduct titleProduct, BrowseViewFilter browseFilter)
	{
		if (titleProduct == null)
		{
			return decimal.MaxValue;
		}
		List<ITitleProduct> source = ((titleProduct is ITitleRollupProduct && ((ITitleRollupProduct)titleProduct)?.KioskProducts != null) ? ((ITitleRollupProduct)titleProduct).KioskProducts : new List<ITitleProduct> { titleProduct });
		if (browseFilter == BrowseViewFilter.ForSale)
		{
			return source.Min((ITitleProduct tp) => (!(tp.PurchasePrice != 0m)) ? decimal.MaxValue : tp.PurchasePrice);
		}
		return source.Min((ITitleProduct tp) => (tp.RentalPriceRecords?.FirstOrDefault())?.InitialNight ?? decimal.MaxValue);
	}

	public static TitleFamily ToFamily(this TitleType titleType)
	{
		if (TitleProductTypes.MovieFamilyTitleTypes.Any((TitleType item) => item == titleType))
		{
			return TitleFamily.Movies;
		}
		if (TitleProductTypes.GameFamilyTitleTypes.Any((TitleType item) => item == titleType))
		{
			return TitleFamily.Games;
		}
		return TitleFamily.Movies;
	}

	public static long? ConvertToGenre(this string selectedGenre)
	{
		if (string.IsNullOrEmpty(selectedGenre) || selectedGenre.Equals("All", StringComparison.CurrentCultureIgnoreCase))
		{
			return null;
		}
		if (!long.TryParse(selectedGenre, out var result))
		{
			return null;
		}
		return result;
	}

	public static BrowseSort ConvertToBrowseSort(this BrowseSelectedSort selectedSort, BrowseViewFilter browseViewFilter)
	{
		if (selectedSort.Equals(BrowseSelectedSort.Alpha))
		{
			return BrowseSort.Alpha;
		}
		return browseViewFilter switch
		{
			BrowseViewFilter.ComingSoon => BrowseSort.MerchandiseDate, 
			BrowseViewFilter.Deals => BrowseSort.InitialNightPrice, 
			_ => BrowseSort.SortDate, 
		};
	}

	public static BrowseViewFilter ConvertToBrowseViewFilter(this string filter)
	{
		if (filter.Equals("All", StringComparison.CurrentCultureIgnoreCase))
		{
			return BrowseViewFilter.All;
		}
		if (filter.Equals("Newest", StringComparison.CurrentCultureIgnoreCase))
		{
			return BrowseViewFilter.All;
		}
		if (filter.Equals("for-sale", StringComparison.CurrentCultureIgnoreCase) || filter.Equals(BrowseViewFilter.ForSale.ToString(), StringComparison.CurrentCultureIgnoreCase))
		{
			return BrowseViewFilter.ForSale;
		}
		if (filter.Equals("coming-soon", StringComparison.CurrentCultureIgnoreCase) || filter.Equals(BrowseViewFilter.ComingSoon.ToString(), StringComparison.CurrentCultureIgnoreCase))
		{
			return BrowseViewFilter.ComingSoon;
		}
		return BrowseViewFilter.All;
	}

	public static TitleFamily ConvertToTitleFamily(this string family)
	{
		if (family.Equals("Movies", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleFamily.Movies;
		}
		if (family.Equals("Games", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleFamily.Games;
		}
		family.Equals("All", StringComparison.CurrentCultureIgnoreCase);
		return TitleFamily.All;
	}

	public static TitleType ConvertToTitleType(this string type)
	{
		if (type.Equals("All", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.All;
		}
		if (type.Equals("DVD", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.DVD;
		}
		if (type.Equals("BLURAY", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.Bluray;
		}
		if (type.Equals("BLU-RAY", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.Bluray;
		}
		if (type.Equals("Digital Code", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.DigitalCode;
		}
		if (type.Equals("DigitalCode", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.DigitalCode;
		}
		if (type.Equals("4K UHD", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType._4KUHD;
		}
		if (type.Equals("4KUHD", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType._4KUHD;
		}
		if (type.Equals("_4KUHD", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType._4KUHD;
		}
		if (type.Equals("Xbox 360", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.Xbox360;
		}
		if (type.Equals("Xbox360", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.Xbox360;
		}
		if (type.Equals("XBOX ONE", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.XboxOne;
		}
		if (type.Equals("XBOXONE", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.XboxOne;
		}
		if (type.Equals("PSP", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.PSP;
		}
		if (type.Equals("PS2", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.PS2;
		}
		if (type.Equals("PS3", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.PS3;
		}
		if (type.Equals("PS4", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.PS4;
		}
		if (type.Equals("DS", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.DS;
		}
		if (type.Equals("WII", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.Wii;
		}
		if (type.Equals("WII U", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.WiiU;
		}
		if (type.Equals("WIIU", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.WiiU;
		}
		if (type.Equals("NINTENDO SWITCH", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.NintendoSwitch;
		}
		if (type.Equals("NINTENDOSWITCH", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.NintendoSwitch;
		}
		if (type.Equals("PC", StringComparison.CurrentCultureIgnoreCase))
		{
			return TitleType.PC;
		}
		return TitleType.All;
	}

	public static List<IKioskProduct> ToKioskProducts(this List<ITitleProduct> products)
	{
		List<IKioskProduct> list = new List<IKioskProduct>();
		products.ForEach(delegate(ITitleProduct item)
		{
			list.Add(item);
		});
		return list;
	}

	public static List<ITitleProduct> ToTitleProducts(this List<IKioskProduct> products)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		products.ForEach(delegate(IKioskProduct item)
		{
			if (item is ITitleProduct)
			{
				list.Add((ITitleProduct)item);
			}
		});
		return list;
	}

	public static List<Dictionary<string, object>> ToDictionary(this List<IKioskProduct> products)
	{
		List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
		products.ForEach(delegate(IKioskProduct p)
		{
			if (p is TitleProduct)
			{
				result.Add((p as TitleProduct).ToDictionary());
			}
			else if (p is MDVProduct)
			{
				result.Add((p as MDVProduct).ToDictionary());
			}
			else if (p is TitleRollupProduct)
			{
				result.Add((p as TitleRollupProduct).ToDictionary());
			}
		});
		return result;
	}

	public static List<Dictionary<string, object>> ToDictionary(this List<IKioskProduct> products, TitleFamily titleFamily, TitleType titleType, List<long> include, List<long> exclude)
	{
		List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
		products.ForEach(delegate(IKioskProduct p)
		{
			if (p is TitleProduct)
			{
				result.Add((p as TitleProduct).ToDictionary());
			}
			else if (p is MDVProduct)
			{
				result.Add((p as MDVProduct).ToDictionary());
			}
			else if (p is TitleRollupProduct)
			{
				result.Add((p as TitleRollupProduct).ToDictionary());
			}
		});
		return result;
	}

	public static List<Dictionary<string, object>> ToDictionary(this List<ITitleProduct> products)
	{
		List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
		products.ForEach(delegate(ITitleProduct p)
		{
			if (p is TitleProduct)
			{
				result.Add((p as TitleProduct).ToDictionary());
			}
			else if (p is MDVProduct)
			{
				result.Add((p as MDVProduct).ToDictionary());
			}
			else if (p is TitleRollupProduct)
			{
				result.Add((p as TitleRollupProduct).ToDictionary());
			}
		});
		return result;
	}

	public static Dictionary<string, object> ToDictionary(this IKioskProduct product)
	{
		if (product is TitleRollupProduct)
		{
			return (product as TitleRollupProduct).ToDictionary();
		}
		if (product is TitleProduct)
		{
			return (product as TitleProduct).ToDictionary();
		}
		if (product is MDVProduct)
		{
			return (product as MDVProduct).ToDictionary();
		}
		return new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase)
		{
			["kiosk_product_type"] = (int)product.KioskProductType,
			["product_name"] = product.ProductName,
			["product_description"] = product.ProductDescription
		};
	}

	public static Dictionary<string, object> ToDictionary(this IKioskProduct product, TitleFamily titleFamily, TitleType titleType, List<long> include, List<long> exclude)
	{
		if (product is TitleRollupProduct)
		{
			return (product as TitleRollupProduct).ToDictionary();
		}
		if (product is TitleProduct)
		{
			return (product as TitleProduct).ToDictionary();
		}
		if (product is MDVProduct)
		{
			return (product as MDVProduct).ToDictionary();
		}
		return new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase)
		{
			["kiosk_product_type"] = (int)product.KioskProductType,
			["product_name"] = product.ProductName,
			["product_description"] = product.ProductDescription
		};
	}

	public static Dictionary<string, object> ToDictionary(this ITitleProduct product)
	{
		if (product is TitleRollupProduct)
		{
			return (product as TitleRollupProduct).ToDictionary();
		}
		if (product is TitleProduct)
		{
			return (product as TitleProduct).ToDictionary();
		}
		if (product is MDVProduct)
		{
			return (product as MDVProduct).ToDictionary();
		}
		return new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase)
		{
			["kiosk_product_type"] = (int)product.KioskProductType,
			["product_name"] = product.ProductName,
			["product_description"] = product.ProductDescription
		};
	}

	public static Dictionary<string, object> ToDictionary(this TitleProduct product)
	{
		return new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase)
		{
			["kiosk_product_type"] = (int)product.KioskProductType,
			["product_name"] = product.ProductName,
			["product_description"] = product.ProductDescription,
			["product_id"] = product.ProductId,
			["starring"] = product.Starring,
			["running_time"] = product.RunningTime,
			["product_type_id"] = product.ProductTypeId,
			["rating_id"] = product.RatingId,
			["long_name"] = product.LongName,
			["sort_name"] = product.SortName,
			["image_file"] = product.ImageFile,
			["description"] = product.Description,
			["genres"] = product.Genres.ToGenreDictionary(),
			["release_date"] = product.ReleaseDate,
			["box_office_gross"] = product.BoxOfficeGross,
			["directors"] = product.Directors,
			["coming_soon_days"] = product.ComingSoonDays,
			["sort_date"] = product.SortDate,
			["national_street_date"] = product.NationalStreetDate,
			["closed_captioned"] = product.ClosedCaptioned,
			["merchandise_date"] = product.MerchandiseDate,
			["do_not_rent_date"] = product.DoNotRentDate,
			["is_sellable"] = product.IsSellable,
			["purchase_price"] = product.PurchasePrice,
			["sell_thru"] = product.SellThru,
			["sell_thru_new"] = product.SellThruNew,
			["sellthru_date"] = product.SellThruDate,
			["sellthru_price"] = product.SellThruPrice,
			["disc_number"] = product.DiscNumber,
			["sub_platform_id"] = product.SubPlatformId,
			["number_of_players_text"] = product.NumberOfPlayersText,
			["studio"] = product.Studio,
			["do_not_rent"] = product.DoNotRent,
			["is_available"] = product.IsAvailable,
			["coming_soon"] = product.ComingSoon,
			["in_stock"] = product.InStock,
			["show_out_of_stock"] = product.ShowOutOfStock,
			["is_game"] = product.IsGame,
			["is_movie"] = product.IsMovie,
			["title_rollup_product_group_id"] = product.TitleRollupProductGroupId,
			["redbox_plus_eligible_date"] = product.RedboxPlusEligibleDate,
			["is_redbox_plus_eligible"] = product.IsRedboxPlusEligible,
			["discount_restriction"] = product.DiscountRestriction.ToString()
		};
	}

	public static Dictionary<long, object> ToGenreDictionary(this List<long> genres)
	{
		Dictionary<long, object> result = new Dictionary<long, object>();
		genres.ForEach(delegate(long g)
		{
			result[g] = true;
		});
		return result;
	}

	public static Dictionary<string, object> ToDictionary(this MDVProduct product)
	{
		return new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase)
		{
			["kiosk_product_type"] = (int)product.KioskProductType,
			["product_name"] = product.ProductName,
			["product_description"] = product.ProductDescription,
			["product_id"] = product.ProductId,
			["starring"] = product.Starring,
			["running_time"] = product.RunningTime,
			["product_type_id"] = product.ProductTypeId,
			["rating_id"] = product.RatingId,
			["long_name"] = product.LongName,
			["sort_name"] = product.SortName,
			["image_file"] = product.ImageFile,
			["description"] = product.Description,
			["genres"] = product.Genres.ToGenreDictionary(),
			["release_date"] = product.ReleaseDate,
			["box_office_gross"] = product.BoxOfficeGross,
			["directors"] = product.Directors,
			["coming_soon_days"] = product.ComingSoonDays,
			["sort_date"] = product.SortDate,
			["national_street_date"] = product.NationalStreetDate,
			["closed_captioned"] = product.ClosedCaptioned,
			["merchandise_date"] = product.MerchandiseDate,
			["do_not_rent_date"] = product.DoNotRentDate,
			["is_sellable"] = product.IsSellable,
			["sell_thru"] = product.SellThru,
			["sell_thru_new"] = product.SellThruNew,
			["sellthru_date"] = product.SellThruDate,
			["sellthru_price"] = product.SellThruPrice,
			["disc_number"] = product.DiscNumber,
			["sub_platform_id"] = product.SubPlatformId,
			["number_of_players_text"] = product.NumberOfPlayersText,
			["studio"] = product.Studio,
			["do_not_rent"] = product.DoNotRent,
			["is_available"] = product.IsAvailable,
			["coming_soon"] = product.ComingSoon,
			["in_stock"] = product.InStock,
			["show_out_of_stock"] = product.ShowOutOfStock,
			["kiosk_products"] = product.TitleProducts.ToKioskProducts().ToDictionary(),
			["is_game"] = product.IsGame,
			["is_movie"] = product.IsMovie,
			["title_rollup_product_group_id"] = product.TitleRollupProductGroupId,
			["redbox_plus_eligible_date"] = product.RedboxPlusEligibleDate,
			["is_redbox_plus_eligible"] = product.IsRedboxPlusEligible,
			["discount_restriction"] = product.DiscountRestriction.ToString()
		};
	}

	public static Dictionary<string, object> ToDictionary(this TitleRollupProduct product)
	{
		return new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase)
		{
			["kiosk_product_type"] = (int)product.KioskProductType,
			["product_name"] = product.ProductName,
			["product_description"] = product.ProductDescription,
			["product_id"] = product.ProductId,
			["starring"] = product.Starring,
			["running_time"] = product.RunningTime,
			["product_type_id"] = product.ProductTypeId,
			["rating_id"] = product.RatingId,
			["long_name"] = product.LongName,
			["sort_name"] = product.SortName,
			["image_file"] = product.ImageFile,
			["description"] = product.Description,
			["genres"] = product.Genres.ToGenreDictionary(),
			["release_date"] = product.ReleaseDate,
			["box_office_gross"] = product.BoxOfficeGross,
			["directors"] = product.Directors,
			["coming_soon_days"] = product.ComingSoonDays,
			["sort_date"] = product.SortDate,
			["national_street_date"] = product.NationalStreetDate,
			["closed_captioned"] = product.ClosedCaptioned,
			["merchandise_date"] = product.MerchandiseDate,
			["do_not_rent_date"] = product.DoNotRentDate,
			["is_sellable"] = product.IsSellable,
			["sell_thru"] = product.SellThru,
			["sell_thru_new"] = product.SellThruNew,
			["sellthru_date"] = product.SellThruDate,
			["sellthru_price"] = product.SellThruPrice,
			["disc_number"] = product.DiscNumber,
			["sub_platform_id"] = product.SubPlatformId,
			["number_of_players_text"] = product.NumberOfPlayersText,
			["studio"] = product.Studio,
			["do_not_rent"] = product.DoNotRent,
			["is_game"] = product.IsGame,
			["is_movie"] = product.IsMovie,
			["is_available"] = product.IsAvailable,
			["coming_soon"] = product.ComingSoon,
			["in_stock"] = product.InStock,
			["show_out_of_stock"] = product.ShowOutOfStock,
			["kiosk_products"] = product.KioskProducts.ToDictionary(),
			["redbox_plus_eligible_date"] = product.RedboxPlusEligibleDate,
			["is_redbox_plus_eligible"] = product.IsRedboxPlusEligible,
			["discount_restriction"] = product.DiscountRestriction.ToString()
		};
	}

	public static bool HasTitle(this ITitleProduct product, List<long> titles)
	{
		if (product is TitleRollupProduct)
		{
			return (product as TitleRollupProduct).HasTitle(titles);
		}
		if (product is TitleProduct)
		{
			return (product as TitleProduct).HasTitle(titles);
		}
		if (product is MDVProduct)
		{
			return (product as MDVProduct).HasTitle(titles);
		}
		return false;
	}

	public static bool HasTitle(this TitleProduct product, List<long> titles)
	{
		return titles.Contains(product.ProductId);
	}

	public static bool HasTitle(this MDVProduct product, List<long> titles)
	{
		return product.TitleProducts.Any((ITitleProduct tp) => titles.Contains(tp.ProductId));
	}

	public static bool HasTitle(this TitleRollupProduct product, List<long> titles)
	{
		return product.KioskProducts.Any((ITitleProduct kp) => kp.HasTitle(titles));
	}
}
