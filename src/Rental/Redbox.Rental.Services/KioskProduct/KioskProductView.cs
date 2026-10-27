using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Products;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskProduct;

public class KioskProductView
{
	public class BarcodeForProductResult : IBarcodeForProductResult
	{
		public long ProductId { get; set; }

		public string Barcode { get; set; }

		public bool BarcodeAssigned { get; set; }

		public LineItemStatus? Status { get; set; }

		public RentalShoppingCartItemAction Action { get; set; }
	}

	private class BarcodeAndTotalRents
	{
		public string Barcode { get; set; }

		public int RentalCount { get; set; }
	}

	private class ProductAndBarcodes
	{
		public long ProductId { get; set; }

		public List<BarcodeAndRentalCount> BarcodesAndRentalCounts { get; set; }

		public ProductAndBarcodes()
		{
			BarcodesAndRentalCounts = new List<BarcodeAndRentalCount>();
		}
	}

	private class BarcodeAndRentalCount
	{
		public string Barcode { get; set; }

		public int RentalCount { get; set; }
	}

	private Store _store;

	private IStoreManager _storeManager;

	private Dictionary<long, ITitleProduct> _titleProducts = new Dictionary<long, ITitleProduct>();

	private Dictionary<long, MDVProduct> _mdvProducts = new Dictionary<long, MDVProduct>();

	private Dictionary<long, MDVProduct> _mdvWorkaroundProducts = new Dictionary<long, MDVProduct>();

	private Dictionary<long, TitleRollupProduct> _titleRollupProducts = new Dictionary<long, TitleRollupProduct>();

	private List<ITitleProduct> _titlesWithNoRollup = new List<ITitleProduct>();

	private List<MDVProduct> _mdvTitlesWithNoRollup = new List<MDVProduct>();

	private List<MDVProduct> _mdvWorkaroundTitlesWithNoRollup = new List<MDVProduct>();

	private List<ITitleProduct> _gamesProducts = new List<ITitleProduct>();

	private List<ITitleProduct> _movieProducts = new List<ITitleProduct>();

	private List<ISubscriptionProduct> _subscriptionProducts = new List<ISubscriptionProduct>();

	private List<IRedboxPlusPromoCampaign> _redboxPlusPromoCampaigns = new List<IRedboxPlusPromoCampaign>();

	private DateTime _lastRedboxPlusPromoCampaignLoad = DateTime.MinValue;

	public IProfileDataInstance ProfileDataInstance { get; set; }

	public ICacheDataInstance CacheDataInstance { get; set; }

	public List<Title> MovieTitles { get; } = new List<Title>();

	public Dictionary<long, ITitleProduct> TitleProducts => _titleProducts;

	public Dictionary<long, MDVProduct> MDVProducts => _mdvProducts;

	public Dictionary<long, TitleRollupProduct> TitleRollupProducts => _titleRollupProducts;

	public KioskProductView(Store store, IProfileDataInstance profileDataInstance, ICacheDataInstance cacheDataInstance, IStoreManager storeManager)
	{
		_store = store;
		_storeManager = storeManager;
		ProfileDataInstance = profileDataInstance;
		CacheDataInstance = cacheDataInstance;
		LoadView();
	}

	public void LoadView()
	{
		LogHelper.Instance.Log("KioskProductView.LoadView start");
		LoadTitleProducts();
		LoadMDVProducts();
		LoadMDVWorkaroundProducts();
		LoadTitleRollupProducts();
		LoadTitlesWithNoRollupOrMDV();
		LoadMDVTitlesWithNoRollup();
		LoadMDVWorkaroundTitlesWithNoRollup();
		LoadMovies();
		LoadGames();
		LoadMovieTitles();
		LoadMoreLikeThis();
		LogHelper.Instance.Log("KioskProductView.LoadView complete");
	}

	public void LoadView(TitleCache titleCache)
	{
		TitleProduct titleProduct = AddTitleProduct(titleCache);
		if (titleProduct != null)
		{
			AddMDVProduct(titleProduct);
			AddTitleRollupProduct(titleProduct);
			LoadTitlesWithNoRollupOrMDV();
			LoadMDVTitlesWithNoRollup();
			LoadMDVWorkaroundTitlesWithNoRollup();
			LoadMovies();
			LoadGames();
		}
	}

	public IRedboxPlusPromoCampaign GetRedboxPlusPromoCampaign(int promoCampaignId)
	{
		if ((_redboxPlusPromoCampaigns == null || _redboxPlusPromoCampaigns.Count == 0) && DateTime.Now > _lastRedboxPlusPromoCampaignLoad.AddMinutes(1.0))
		{
			_lastRedboxPlusPromoCampaignLoad = DateTime.Now;
			LoadSubscriptionProducts();
		}
		return _redboxPlusPromoCampaigns?.FirstOrDefault((IRedboxPlusPromoCampaign x) => x.PromoCampaignId == promoCampaignId);
	}

	public List<ISubscriptionProduct> GetSubscriptionProducts()
	{
		LoadSubscriptionProducts();
		List<ISubscriptionProduct> list = new List<ISubscriptionProduct>();
		list.AddRange(_subscriptionProducts);
		return list;
	}

	public IKioskProduct GetProduct(long productId, bool excludeTitleRollup)
	{
		IKioskProduct kioskProduct = null;
		kioskProduct = _movieProducts.FirstOrDefault((ITitleProduct x) => x.ProductId == productId);
		if (kioskProduct == null)
		{
			kioskProduct = FilterToFormat(TitleType.Bluray, _movieProducts).FirstOrDefault((ITitleProduct x) => x.ProductId == productId);
			if (kioskProduct == null)
			{
				kioskProduct = FilterToFormat(TitleType.DigitalCode, _movieProducts).FirstOrDefault((ITitleProduct x) => x.ProductId == productId);
				if (kioskProduct == null)
				{
					kioskProduct = FilterToFormat(TitleType._4KUHD, _movieProducts).FirstOrDefault((ITitleProduct x) => x.ProductId == productId);
					if (kioskProduct == null)
					{
						kioskProduct = _gamesProducts.FirstOrDefault((ITitleProduct x) => x.ProductId == productId);
						if (kioskProduct == null)
						{
							_titleProducts.TryGetValue(productId, out var value);
							kioskProduct = value;
						}
					}
				}
			}
		}
		if (excludeTitleRollup)
		{
			if (kioskProduct is TitleRollupProduct titleRollupProduct)
			{
				kioskProduct = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.ProductId == productId);
			}
			if (kioskProduct is MDVProduct mDVProduct)
			{
				kioskProduct = mDVProduct.PrimaryTitle;
			}
		}
		return kioskProduct;
	}

	public List<IBarcodeForProductResult> GetBarcodesForProducts(List<ISearchBarcodeForProduct> searchProducts, List<string> excludeBarcodesList)
	{
		List<IBarcodeForProductResult> list = new List<IBarcodeForProductResult>();
		if (searchProducts != null && searchProducts.Count > 0)
		{
			LogHelper.Instance.Log("GetBarcodesForProducts Begin.  searching for products: " + searchProducts.Select((ISearchBarcodeForProduct x) => x.ProductId).ToList().Join(", "));
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			foreach (ISearchBarcodeForProduct searchProduct in searchProducts)
			{
				IBarcodeForProductResult barcodeForProductResult = new BarcodeForProductResult
				{
					ProductId = searchProduct.ProductId,
					Barcode = searchProduct.Barcode,
					Action = searchProduct.Action
				};
				list.Add(barcodeForProductResult);
				if (searchProduct.Barcode == null)
				{
					bool flag = false;
					Dictionary<string, List<BarcodeAndTotalRents>> dictionary = new Dictionary<string, List<BarcodeAndTotalRents>>();
					if (GetProduct(searchProduct.ProductId, excludeTitleRollup: true) is ITitleProduct titleProduct)
					{
						if (titleProduct.BarCodeProducts == null)
						{
							continue;
						}
						foreach (BarcodeProduct eachBarcodeProduct in titleProduct.BarCodeProducts.Values)
						{
							if (!list.Any((IBarcodeForProductResult x) => x.Barcode == eachBarcodeProduct.Barcode) && (excludeBarcodesList == null || !excludeBarcodesList.Contains(eachBarcodeProduct.Barcode)))
							{
								int rentalCount = (int)(service.GetInventory(eachBarcodeProduct.Barcode)?.TotalRentalCount ?? 0);
								BarcodeAndTotalRents item = new BarcodeAndTotalRents
								{
									Barcode = eachBarcodeProduct.Barcode,
									RentalCount = rentalCount
								};
								if (!dictionary.Keys.Contains(eachBarcodeProduct.Status))
								{
									dictionary[eachBarcodeProduct.Status] = new List<BarcodeAndTotalRents>();
								}
								dictionary[eachBarcodeProduct.Status].Add(item);
							}
							else
							{
								flag = true;
							}
						}
						foreach (string item2 in new List<string> { "Known", "Thinned", "Redeployment", "Rebalancing" })
						{
							if (TryAssignBarcodeFromList(dictionary, item2, barcodeForProductResult))
							{
								break;
							}
						}
						if (string.IsNullOrEmpty(barcodeForProductResult.Barcode))
						{
							if (flag)
							{
								LogHelper.Instance.Log($"> WARNING: Last Barcode Not Reservable for product id: {searchProduct.ProductId}");
								barcodeForProductResult.Status = LineItemStatus.ProductSelected;
							}
							else
							{
								LogHelper.Instance.Log($">  WARNING: No vendable barcodes found for product id: {searchProduct.ProductId}");
								barcodeForProductResult.Status = LineItemStatus.InventoryError;
							}
						}
					}
					else
					{
						LogHelper.Instance.Log(">  Warning: Unable to find title product for barcode: " + searchProduct.Barcode);
					}
				}
				else
				{
					LogHelper.Instance.Log($">  Barcode {barcodeForProductResult.Barcode} was ALREADY assigned to product : {barcodeForProductResult.ProductId} ");
				}
			}
			LogHelper.Instance.Log("GetBarcodesForProducts End.");
		}
		list.Sort((IBarcodeForProductResult a, IBarcodeForProductResult b) => (a.Barcode ?? "").CompareTo(b.Barcode ?? ""));
		return list;
	}

	private bool TryAssignBarcodeFromList(Dictionary<string, List<BarcodeAndTotalRents>> barcodeAndTotalRentsByStatus, string status, IBarcodeForProductResult barcodeForProductResult)
	{
		if (barcodeAndTotalRentsByStatus != null && barcodeAndTotalRentsByStatus.TryGetValue(status, out var barcodeAndTotalRents) && barcodeAndTotalRents.Count > 0)
		{
			if (barcodeForProductResult.Action == RentalShoppingCartItemAction.rental)
			{
				barcodeForProductResult.Barcode = barcodeAndTotalRents.First((BarcodeAndTotalRents x) => x.RentalCount == barcodeAndTotalRents.Min((BarcodeAndTotalRents y) => y.RentalCount)).Barcode;
			}
			else
			{
				barcodeForProductResult.Barcode = barcodeAndTotalRents.First((BarcodeAndTotalRents x) => x.RentalCount == barcodeAndTotalRents.Max((BarcodeAndTotalRents y) => y.RentalCount)).Barcode;
			}
			barcodeForProductResult.BarcodeAssigned = true;
			barcodeForProductResult.Status = LineItemStatus.ProductSelected;
			LogHelper.Instance.Log($">  Barcode {barcodeForProductResult.Barcode} selected for product: {barcodeForProductResult.ProductId} ");
		}
		return !string.IsNullOrEmpty(barcodeForProductResult?.Barcode);
	}

	private void LoadMovieTitles()
	{
		try
		{
			MovieTitles.Clear();
			List<Title> outer = (from pair in ProfileDataInstance.Titles.Where(delegate(KeyValuePair<long, Title> pair)
				{
					TitleType titleTypeFromProductTypeId = TitleProduct.GetTitleTypeFromProductTypeId(pair.Value.ProductTypeId);
					if (titleTypeFromProductTypeId.ToFamily() != TitleFamily.Movies)
					{
						return false;
					}
					return titleTypeFromProductTypeId != TitleType.DigitalCode;
				})
				select pair.Value).ToList();
			var inner = (from pair in ProfileDataInstance.ProductGroups
				where pair.Value.ProductGroupTypeId == 1
				select pair.Value).ToList().SelectMany((ProductGroup tr) => tr.ProductIds, (ProductGroup pg, long p) => new { pg, p });
			var list = (from t in outer
				join tr in inner on t.ProductId equals tr.p
				orderby t.SortDate ?? DateTime.Now descending, t.BoxOfficeGross descending, t.SortName.ToLower() descending
				select new { t, tr.pg }).ToList();
			list.ForEach(tpg =>
			{
				tpg.t.ProductGroup = tpg.pg;
			});
			MovieTitles.AddRange((from tpg in list
				select tpg.t into t
				where t.IsRentable
				select t).ToList());
			Dictionary<long, int> ProductGroupOrder = new Dictionary<long, int>();
			int position = 1;
			MovieTitles.ForEach(delegate(Title mt)
			{
				if (!ProductGroupOrder.ContainsKey(mt.ProductGroup.ProductGroupId))
				{
					ProductGroupOrder[mt.ProductGroup.ProductGroupId] = position++;
				}
				mt.ProductGroup.SortOrderPosition = ProductGroupOrder[mt.ProductGroup.ProductGroupId];
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("KioskProductView.LoadMovieTitles - an unhandled exception occurred", e);
		}
	}

	public List<IKioskProduct> GetProducts(BrowseViewFilter browseFilter, TitleFamily titleFamily, TitleType titleType, BrowseSort browseSort, long? genreFilter, long? ratingFilter, List<long> include, List<long> exclude, bool isADAView, decimal? priceRangeLow, decimal? priceRangeHigh, string studio = null, List<TitleType> excludeTitleTypes = null, bool excludeDiscountRestriction = false)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		list = FilterByTitleFamily(browseFilter, titleFamily, new List<TitleType> { titleType }, list);
		if (genreFilter.HasValue)
		{
			list = list.Where((ITitleProduct item) => ApplyGenreFilter(item, genreFilter.Value)).ToList();
		}
		if (ratingFilter.HasValue)
		{
			list = list.Where((ITitleProduct item) => ApplyRatingFilter(item, ratingFilter.Value)).ToList();
		}
		if (((include != null && include.Count > 0) || (exclude != null && exclude.Count > 0) || (excludeTitleTypes != null && excludeTitleTypes.Count > 0)) | excludeDiscountRestriction)
		{
			list = FilterInCarts(list, include, exclude, excludeTitleTypes, excludeDiscountRestriction);
		}
		if (isADAView)
		{
			list = FilterToTitles(list);
		}
		if (browseFilter == BrowseViewFilter.ForSale && (priceRangeHigh.HasValue || priceRangeLow.HasValue))
		{
			list = list.Where((ITitleProduct item) => FilterByPrice(item, priceRangeLow, priceRangeHigh)).ToList();
		}
		if (browseFilter == BrowseViewFilter.All && priceRangeLow.HasValue && priceRangeHigh.HasValue)
		{
			list = list.Where((ITitleProduct item) => FilterByRentalPrice(item, priceRangeLow, priceRangeHigh)).ToList();
		}
		if (browseFilter == BrowseViewFilter.All && studio != null)
		{
			list = list.Where((ITitleProduct item) => FilterByStudio(item, studio)).ToList();
		}
		list = ApplyMainFilter(list, browseFilter);
		return ((IEnumerable<ITitleProduct>)list.OrderList(browseSort, browseFilter, MovieTitles)).Select((Func<ITitleProduct, IKioskProduct>)((ITitleProduct item) => item)).ToList();
	}

	private List<ITitleProduct> FilterByTitleFamily(BrowseViewFilter browseFilter, TitleFamily titleFamily, List<TitleType> titleTypes, List<ITitleProduct> list, bool includeTitleTypes = true)
	{
		bool flag = titleTypes == null || titleTypes.Count == 0 || titleTypes.Contains(TitleType.All);
		if (titleFamily == TitleFamily.Movies && !flag)
		{
			list.AddRange(from item in FilterToFormat(titleTypes, _movieProducts, includeTitleTypes)
				where ApplyMainFilter(item, browseFilter)
				select (item));
		}
		else
		{
			if (titleFamily == TitleFamily.All || titleFamily == TitleFamily.Movies)
			{
				list.AddRange(from item in _movieProducts
					where ApplyMainFilter(item, browseFilter)
					select (item));
			}
			if (titleFamily == TitleFamily.All || titleFamily == TitleFamily.Games)
			{
				list.AddRange(from item in _gamesProducts
					where ApplyMainFilter(item, browseFilter)
					select (item));
			}
			if (!flag)
			{
				list = list.Where((ITitleProduct item) => ApplyTitleTypeFilter(item, titleTypes, includeTitleTypes)).ToList();
			}
		}
		return list;
	}

	public List<ITitleProduct> GetProductsForCustomerOffer(TitleFamily titleFamily, List<TitleType> titleTypes, bool includeTitleTypes, List<long> include, List<long> exclude, TimeSpan? includeOlderThan, bool isAdaView, bool isPurchase)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		if (include == null || include.Count == 0)
		{
			BrowseViewFilter browseFilter = (isPurchase ? BrowseViewFilter.ForSale : BrowseViewFilter.All);
			list = FilterByTitleFamily(browseFilter, titleFamily, titleTypes, list, includeTitleTypes);
		}
		else
		{
			list.AddRange(_movieProducts.Where((ITitleProduct p) => include.Contains(p.ProductId)));
			list.AddRange(_gamesProducts.Where((ITitleProduct p) => include.Contains(p.ProductId)));
		}
		if (exclude != null && exclude.Count > 0)
		{
			list = list.Where((ITitleProduct i) => !exclude.Contains(i.ProductId)).ToList();
		}
		list = list.Where((ITitleProduct i) => i.InStock).ToList();
		if (includeOlderThan.HasValue)
		{
			DateTime cutoffDate = DateTime.Now.Date.AddDays(-includeOlderThan.Value.Days);
			list = list.Where((ITitleProduct i) => i.ReleaseDate < cutoffDate).ToList();
		}
		if (!isPurchase)
		{
			list.AddRange(list.Where((ITitleProduct i) => i.HasDeal && i is TitleRollupProduct).SelectMany((ITitleProduct r) => ((TitleRollupProduct)r).KioskProducts.Where((ITitleProduct k) => !k.HasDeal)).ToList());
			list = list.Where((ITitleProduct i) => !i.HasDeal).ToList();
		}
		if (isAdaView)
		{
			list = FilterToTitles(list);
		}
		return list.OrderList(BrowseSort.SortDate, BrowseViewFilter.All, MovieTitles);
	}

	public List<IKioskProduct> GetCarouselProducts(int maxCarouselItems)
	{
		List<IKioskProduct> resultList = new List<IKioskProduct>();
		List<ITitleProduct> source = _movieProducts.OrderList(BrowseSort.SortDate, BrowseViewFilter.All, MovieTitles).ToList();
		source.Where((ITitleProduct x) => IsCarouselViewable(x, Genres.HitMovies) && !resultList.Contains(x)).Take(maxCarouselItems)?.ForEach(resultList.Add);
		source.Where((ITitleProduct x) => IsCarouselViewable(x, Genres.Top20Movies) && !resultList.Contains(x)).Take(maxCarouselItems - resultList.Count)?.ForEach(resultList.Add);
		source.Where((ITitleProduct x) => IsCarouselViewable(x, Genres.ActionAndAdventure) && !resultList.Contains(x)).Take(maxCarouselItems - resultList.Count)?.ForEach(resultList.Add);
		return resultList.Take(maxCarouselItems).ToList();
	}

	public List<ITitleProduct> GetOutOfStockMovies()
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		list.AddRange(_titleRollupProducts.Values.Where((TitleRollupProduct r) => r.KioskProducts.Any((ITitleProduct p) => !p.InStock)));
		list.AddRange(_titlesWithNoRollup.Where((ITitleProduct t) => t.IsMovie && !t.InStock));
		return list;
	}

	public int GetInStockGameCount()
	{
		if (_gamesProducts == null)
		{
			return 0;
		}
		return _gamesProducts.Count((ITitleProduct x) => IsRentable(x));
	}

	private bool IsCarouselViewable(ITitleProduct titleProduct, Genres? genre)
	{
		if (!IsRentable(titleProduct))
		{
			return false;
		}
		if (titleProduct.ProductId == 1139)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(titleProduct.ImageFile) && !titleProduct.SellThruNew)
		{
			if (genre.HasValue)
			{
				if (titleProduct.Genres != null)
				{
					return titleProduct.Genres.Contains((long)genre.Value);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool IsRentable(ITitleProduct titleProduct)
	{
		return titleProduct?.IsRentableAndInStock ?? false;
	}

	private bool IsSellable(IKioskProduct kioskProduct)
	{
		if (!(kioskProduct is ITitleProduct))
		{
			return false;
		}
		if (!(kioskProduct is ITitleProduct titleProduct))
		{
			return false;
		}
		return titleProduct.IsSellableAndInStock;
	}

	public IKioskProduct GetTitleRollupProduct(long productGroupId)
	{
		return _titleRollupProducts.Where(delegate(KeyValuePair<long, TitleRollupProduct> item)
		{
			KeyValuePair<long, TitleRollupProduct> keyValuePair = item;
			return keyValuePair.Value.ProductGroupId == productGroupId;
		}).Select(delegate(KeyValuePair<long, TitleRollupProduct> item)
		{
			KeyValuePair<long, TitleRollupProduct> keyValuePair = item;
			return keyValuePair.Value;
		}).FirstOrDefault();
	}

	public IKioskProduct GetProductByBarcode(string barcode)
	{
		IKioskProduct kioskProduct = null;
		kioskProduct = _movieProducts.FirstOrDefault((ITitleProduct x) => x.BarCodeProducts?.ContainsKey(barcode) ?? false);
		if (kioskProduct == null)
		{
			kioskProduct = FilterToFormat(TitleType.Bluray, _movieProducts).FirstOrDefault((ITitleProduct x) => x.BarCodeProducts?.ContainsKey(barcode) ?? false);
			if (kioskProduct == null)
			{
				kioskProduct = FilterToFormat(TitleType.DigitalCode, _movieProducts).FirstOrDefault((ITitleProduct x) => x.BarCodeProducts?.ContainsKey(barcode) ?? false);
				if (kioskProduct == null)
				{
					kioskProduct = FilterToFormat(TitleType._4KUHD, _movieProducts).FirstOrDefault((ITitleProduct x) => x.BarCodeProducts?.ContainsKey(barcode) ?? false);
					if (kioskProduct == null)
					{
						kioskProduct = _gamesProducts.FirstOrDefault((ITitleProduct x) => x.BarCodeProducts?.ContainsKey(barcode) ?? false);
					}
				}
			}
		}
		if (kioskProduct is TitleRollupProduct titleRollupProduct)
		{
			kioskProduct = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.BarCodeProducts?.ContainsKey(barcode) ?? false);
		}
		return kioskProduct;
	}

	private List<ITitleProduct> FilterInCarts(List<ITitleProduct> list, List<long> include, List<long> exclude, List<TitleType> excludeTitleTypes, bool excludeDiscountRestriction)
	{
		if (include != null || exclude != null || excludeTitleTypes != null)
		{
			list = FilterToTitles(list);
			if (include != null && include.Count() > 0)
			{
				list = list.Where((ITitleProduct item) => item.HasTitle(include)).ToList();
			}
			else if (exclude != null && exclude.Count() > 0)
			{
				list = list.Where((ITitleProduct item) => !item.HasTitle(exclude)).ToList();
			}
			if (excludeTitleTypes != null && excludeTitleTypes.Count() > 0)
			{
				list = list.Where((ITitleProduct item) => !excludeTitleTypes.Contains(item.TitleType)).ToList();
			}
			if (excludeDiscountRestriction)
			{
				list = list.Where((ITitleProduct item) => item.DiscountRestriction == DiscountRestriction.None).ToList();
			}
		}
		return list;
	}

	private List<ITitleProduct> FilterToTitles(List<ITitleProduct> kioskProducts)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		kioskProducts.ForEach(delegate(ITitleProduct item)
		{
			if (item is TitleRollupProduct)
			{
				list.AddRange(FilterToTitles(((TitleRollupProduct)item).KioskProducts));
			}
			if (item is MDVProduct || item is TitleProduct)
			{
				list.Add(item);
			}
		});
		return list;
	}

	private List<ITitleProduct> FilterToFormat(TitleType titleType, List<ITitleProduct> kioskProducts)
	{
		return FilterToFormat(new List<TitleType> { titleType }, kioskProducts);
	}

	private List<ITitleProduct> FilterToFormat(List<TitleType> titleTypes, List<ITitleProduct> kioskProducts, bool includeTitleTypes = true)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		kioskProducts.ForEach(delegate(ITitleProduct item)
		{
			if (item is TitleRollupProduct)
			{
				list.AddRange(FilterToFormat(titleTypes, ((TitleRollupProduct)item).KioskProducts, includeTitleTypes));
			}
			if (item is MDVProduct && ((includeTitleTypes && titleTypes.Contains(((MDVProduct)item).TitleType)) || (!includeTitleTypes && !titleTypes.Contains(((MDVProduct)item).TitleType))))
			{
				list.Add(item);
			}
			if (item is TitleProduct && ((includeTitleTypes && titleTypes.Contains(((TitleProduct)item).TitleType)) || (!includeTitleTypes && !titleTypes.Contains(((TitleProduct)item).TitleType))))
			{
				list.Add(item);
			}
		});
		return list;
	}

	private bool ApplyTitleTypeFilter(ITitleProduct titleProduct, List<TitleType> titleTypes, bool includeTitleTypes = true)
	{
		if (!includeTitleTypes || !titleTypes.Contains(titleProduct.TitleType))
		{
			if (!includeTitleTypes)
			{
				return !titleTypes.Contains(titleProduct.TitleType);
			}
			return false;
		}
		return true;
	}

	private bool ApplyGenreFilter(ITitleProduct titleProduct, long genreFilter)
	{
		return titleProduct.Genres.Contains(genreFilter);
	}

	private bool ApplyRatingFilter(ITitleProduct titleProduct, long ratingFilter)
	{
		return titleProduct.RatingId == ratingFilter;
	}

	private bool FilterByPrice(ITitleProduct item, decimal? priceRangeLow, decimal? priceRangeHigh)
	{
		bool flag = false;
		if (item is TitleRollupProduct titleRollupProduct)
		{
			foreach (ITitleProduct kioskProduct in titleRollupProduct.KioskProducts)
			{
				flag = flag || (kioskProduct.IsSellableAndInStock && kioskProduct.PurchasePrice >= (priceRangeLow ?? 0.01m) && kioskProduct.PurchasePrice <= (priceRangeHigh ?? decimal.MaxValue));
			}
		}
		else
		{
			flag = item.IsSellableAndInStock && item.PurchasePrice >= (priceRangeLow ?? 0.01m) && item.PurchasePrice <= (priceRangeHigh ?? decimal.MaxValue);
		}
		return flag;
	}

	private bool FilterByRentalPrice(ITitleProduct item, decimal? priceRangeLow, decimal? priceRangeHigh)
	{
		bool flag = false;
		if (item is TitleRollupProduct titleRollupProduct)
		{
			foreach (ITitleProduct kioskProduct in titleRollupProduct.KioskProducts)
			{
				decimal num = ((kioskProduct.RentalPriceRecords != null && kioskProduct.RentalPriceRecords.Count > 0) ? kioskProduct.RentalPriceRecords[0].InitialNight : 0m);
				flag = flag || (kioskProduct.IsRentableAndInStock && num >= (priceRangeLow ?? 0.01m) && num <= (priceRangeHigh ?? decimal.MaxValue));
			}
		}
		else
		{
			decimal num2 = ((item.RentalPriceRecords != null && item.RentalPriceRecords.Count > 0) ? item.RentalPriceRecords[0].InitialNight : 0m);
			flag = item.IsRentableAndInStock && num2 >= (priceRangeLow ?? 0.01m) && num2 <= (priceRangeHigh ?? decimal.MaxValue);
		}
		return flag;
	}

	private bool FilterByStudio(ITitleProduct titleProduct, string studio)
	{
		return titleProduct.Studio.ToLower() == studio.ToLower();
	}

	private List<ITitleProduct> ApplyMainFilter(List<ITitleProduct> list, BrowseViewFilter browseFilter)
	{
		return list.Where((ITitleProduct kioskProduct) => ApplyMainFilter(kioskProduct, browseFilter)).ToList();
	}

	public bool ApplyMainFilter(ITitleProduct titleProduct, BrowseViewFilter browseFilter)
	{
		if ((titleProduct.IsRentable || titleProduct.IsSellable || titleProduct.ComingSoon) && (titleProduct.InStock || titleProduct.ShowOutOfStock || titleProduct.ComingSoon))
		{
			return ApplyBrowseFilter(titleProduct, browseFilter);
		}
		return false;
	}

	private bool ApplyBrowseFilter(ITitleProduct titleProduct, BrowseViewFilter browseFilter)
	{
		if ((browseFilter != BrowseViewFilter.All || (!IsRentable(titleProduct) && !titleProduct.ShowOutOfStock)) && (browseFilter != BrowseViewFilter.ComingSoon || !titleProduct.ComingSoon) && (browseFilter != BrowseViewFilter.Deals || !titleProduct.HasDeal || (!IsRentable(titleProduct) && !titleProduct.ShowOutOfStock)))
		{
			if (browseFilter != BrowseViewFilter.ForSale || !IsSellable(titleProduct))
			{
				if (browseFilter == BrowseViewFilter.RedboxPlus && titleProduct.IsRedboxPlusEligible)
				{
					if (!IsRentable(titleProduct))
					{
						return titleProduct.ShowOutOfStock;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return true;
	}

	private bool ApplyGameRatingFilter(ITitleProduct titleProduct, Ratings rating)
	{
		return (titleProduct.RatingId ?? 999) <= (long)rating;
	}

	private void LoadTitleProducts()
	{
		_titleProducts.Clear();
		LogHelper.Instance.Log("Loading {0} Title Products from cache", CacheDataInstance.Titles.Count);
		CacheDataInstance.Titles.ToList().ForEach(delegate(KeyValuePair<long, TitleCache> pair)
		{
			AddTitleProduct(pair.Value);
		});
	}

	private TitleProduct AddTitleProduct(TitleCache titleCache)
	{
		if (!ProfileDataInstance.Titles.TryGetValue(titleCache.ProductId, out var value))
		{
			LogHelper.Instance.Log("KioskProductView.AddTitleProduct - Profile title {0} not found", titleCache.ProductId);
			return null;
		}
		return AddTitleProduct(value, titleCache, (TitleType)value.ProductTypeId, ProfileDataInstance);
	}

	private TitleProduct AddTitleProduct(Title title, TitleCache titleCache, TitleType titleType, IProfileDataInstance profile)
	{
		if (_titleProducts.ContainsKey(titleCache.ProductId))
		{
			LogHelper.Instance.Log("KioskProductView.AddTitleProduct - Title product {0} already exists", titleCache.ProductId);
			return null;
		}
		TitleProduct titleProduct = new TitleProduct(title, titleCache, (TitleType)title.ProductTypeId, ProfileDataInstance, _store, _storeManager);
		_titleProducts[titleProduct.ProductId] = titleProduct;
		return titleProduct;
	}

	private void AddMissingProductIds(List<long> Ids)
	{
		AddMissingProductIds(Ids, force: false);
	}

	private void AddMissingProductIds(List<long> Ids, bool force)
	{
		if (!force && !Ids.Any((long id) => _titleProducts.ContainsKey(id)))
		{
			return;
		}
		Ids.ForEach(delegate(long id)
		{
			if (!_titleProducts.ContainsKey(id))
			{
				if (!ProfileDataInstance.Titles.TryGetValue(id, out var value))
				{
					LogHelper.Instance.Log("KioskProductView.AddMissingProductIds - Product id {0} not found", id);
				}
				else
				{
					LogHelper.Instance.Log("KioskProductView.AddMissingProductIds - ProductId {0}", id);
					TitleCache titleCache = CacheDataInstance.AddTitleCache(id);
					TitleProduct titleProduct = AddTitleProduct(value, titleCache, (TitleType)value.ProductTypeId, ProfileDataInstance);
					if (force && titleProduct != null)
					{
						AddMDVProduct(titleProduct);
						AddTitleRollupProduct(titleProduct);
					}
				}
			}
		});
	}

	private void LoadMDVProducts()
	{
		_mdvProducts.Clear();
		ProfileDataInstance.ProductGroups.Where((KeyValuePair<long, ProductGroup> pair) => pair.Value.ProductGroupTypeId == 3).ForEach(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			if (pair.Value.ProductIds.Any((long id) => _titleProducts.ContainsKey(id)))
			{
				AddMissingProductIds(pair.Value.ProductIds);
				MDVProduct mDVProduct = new MDVProduct(pair.Value, _titleProducts, ProfileDataInstance, _store);
				_mdvProducts[mDVProduct.ProductGroupId] = mDVProduct;
			}
		});
	}

	private void LoadMDVWorkaroundProducts()
	{
		_mdvWorkaroundProducts.Clear();
		ProfileDataInstance.ProductGroups.Where((KeyValuePair<long, ProductGroup> pair) => pair.Value.ProductGroupTypeId == 6).ForEach(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			if (pair.Value.ProductIds.Any((long id) => _titleProducts.ContainsKey(id)))
			{
				AddMissingProductIds(pair.Value.ProductIds);
				MDVProduct mDVProduct = new MDVProduct(pair.Value, _titleProducts, ProfileDataInstance, _store);
				_mdvWorkaroundProducts[mDVProduct.ProductGroupId] = mDVProduct;
			}
		});
	}

	public ProductGroup GetMDVProductGroup(long productId)
	{
		ProductGroup result = null;
		if (ProfileDataInstance?.ProductGroups != null)
		{
			result = ProfileDataInstance.ProductGroups.Where(delegate(KeyValuePair<long, ProductGroup> pair)
			{
				KeyValuePair<long, ProductGroup> keyValuePair = pair;
				if (keyValuePair.Value.ProductGroupTypeId == 3)
				{
					keyValuePair = pair;
					return keyValuePair.Value.ProductIds.Contains(productId);
				}
				return false;
			}).Select(delegate(KeyValuePair<long, ProductGroup> pair)
			{
				KeyValuePair<long, ProductGroup> keyValuePair = pair;
				return keyValuePair.Value;
			}).FirstOrDefault();
		}
		return result;
	}

	public List<string> GetAllUnmatchedMultiDiscBarcodes()
	{
		List<string> list = new List<string>();
		IEnumerable<KeyValuePair<long, ProductGroup>> enumerable = ProfileDataInstance.ProductGroups.Where((KeyValuePair<long, ProductGroup> x) => x.Value.ProductGroupTypeId == 3);
		if (enumerable != null)
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			foreach (KeyValuePair<long, ProductGroup> item in enumerable)
			{
				if (item.Value?.ProductIds == null)
				{
					continue;
				}
				List<long> productIds = item.Value?.ProductIds;
				List<ProductAndBarcodes> productGroupBarcodes = GetProductGroupBarcodes(service, productIds);
				int minimumBarcodeCount = productGroupBarcodes.Min((ProductAndBarcodes x) => x.BarcodesAndRentalCounts.Count);
				if (productGroupBarcodes.Count <= 1 || !productGroupBarcodes.Any((ProductAndBarcodes x) => x.BarcodesAndRentalCounts.Count != minimumBarcodeCount))
				{
					continue;
				}
				foreach (ProductAndBarcodes item2 in productGroupBarcodes)
				{
					if (item2.BarcodesAndRentalCounts.Count > minimumBarcodeCount)
					{
						for (int num = minimumBarcodeCount; num < item2.BarcodesAndRentalCounts.Count; num++)
						{
							list.Add(item2.BarcodesAndRentalCounts[num].Barcode);
						}
					}
				}
			}
		}
		return list;
	}

	private List<ProductAndBarcodes> GetProductGroupBarcodes(IInventoryService inventoryService, List<long> productIds)
	{
		List<ProductAndBarcodes> list = new List<ProductAndBarcodes>();
		foreach (long productId in productIds)
		{
			ProductAndBarcodes productAndBarcodes = new ProductAndBarcodes
			{
				ProductId = productId
			};
			ITitleProduct titleProduct = GetProduct(productId, excludeTitleRollup: true) as ITitleProduct;
			if (titleProduct?.BarCodeProducts != null)
			{
				foreach (KeyValuePair<string, BarcodeProduct> barCodeProduct in titleProduct.BarCodeProducts)
				{
					if (barCodeProduct.Value != null)
					{
						BarcodeAndRentalCount barcodeAndRentalCount = new BarcodeAndRentalCount
						{
							Barcode = barCodeProduct.Value.Barcode
						};
						Inventory inventory = inventoryService.GetInventory(barCodeProduct.Value.Barcode);
						if (inventory != null)
						{
							barcodeAndRentalCount.RentalCount = (int)inventory.TotalRentalCount;
						}
						productAndBarcodes.BarcodesAndRentalCounts.Add(barcodeAndRentalCount);
					}
				}
			}
			productAndBarcodes.BarcodesAndRentalCounts.Sort((BarcodeAndRentalCount a, BarcodeAndRentalCount b) => a.RentalCount.CompareTo(b.RentalCount));
			list.Add(productAndBarcodes);
		}
		return list;
	}

	private MDVProduct AddMDVProduct(TitleProduct titleProduct)
	{
		ProductGroup mDVProductGroup = GetMDVProductGroup(titleProduct.ProductId);
		if (mDVProductGroup == null)
		{
			return null;
		}
		if (_mdvProducts.ContainsKey(mDVProductGroup.ProductGroupId))
		{
			LogHelper.Instance.Log("KioskProductView.AddMDVProduct - MDV product {0} already exists", mDVProductGroup.ProductGroupId);
			return null;
		}
		AddMissingProductIds(mDVProductGroup.ProductIds);
		MDVProduct mDVProduct = new MDVProduct(mDVProductGroup, _titleProducts, ProfileDataInstance, _store);
		_mdvProducts[mDVProduct.ProductGroupId] = mDVProduct;
		return mDVProduct;
	}

	private void LoadTitleRollupProducts()
	{
		_titleRollupProducts.Clear();
		List<long> sortOrderProductGroupIds = null;
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.Current.Browse.LoadSortOrderProductGroups && Redbox.Rental.Services.Configuration.Configuration.Instance.Current.Browse.NumberOfSortOrderProductGroupsToLoad > 0)
		{
			ProductGroupSortOrder productGroupSortOrder = ServiceLocator.Instance.GetService<IKioskClientService>()?.ProductGroups?.GetSortOrder();
			sortOrderProductGroupIds = ((productGroupSortOrder != null && productGroupSortOrder.SortOrderType > 0 && productGroupSortOrder.SortOrders != null && productGroupSortOrder.SortOrders.Count > 0) ? ((IEnumerable<SortOrder>)productGroupSortOrder.SortOrders).Select((Func<SortOrder, long>)((SortOrder so) => so.ProductGroupId)).Take(Redbox.Rental.Services.Configuration.Configuration.Instance.Current.Browse.NumberOfSortOrderProductGroupsToLoad).ToList() : null);
		}
		ProfileDataInstance.ProductGroups.Where((KeyValuePair<long, ProductGroup> pair) => pair.Value.ProductGroupTypeId == 1).ForEach(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			bool num = pair.Value.ProductIds.Any((long id) => _titleProducts.ContainsKey(id));
			bool flag = sortOrderProductGroupIds?.Contains(pair.Value.ProductGroupId) ?? false;
			bool flag2 = !num & flag;
			if (num | flag2)
			{
				AddMissingProductIds(pair.Value.ProductIds, flag2);
				TitleRollupProduct titleRollupProduct = new TitleRollupProduct(pair.Value, _titleProducts, _mdvProducts, ProfileDataInstance, _store);
				_titleRollupProducts[titleRollupProduct.ProductGroupId] = titleRollupProduct;
			}
		});
	}

	private TitleRollupProduct AddTitleRollupProduct(TitleProduct titleProduct)
	{
		ProductGroup productGroup = ProfileDataInstance.ProductGroups.Where(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			KeyValuePair<long, ProductGroup> keyValuePair = pair;
			if (keyValuePair.Value.ProductGroupTypeId == 1)
			{
				keyValuePair = pair;
				return keyValuePair.Value.ProductIds.Contains(titleProduct.ProductId);
			}
			return false;
		}).Select(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			KeyValuePair<long, ProductGroup> keyValuePair = pair;
			return keyValuePair.Value;
		}).FirstOrDefault();
		if (productGroup == null)
		{
			return null;
		}
		if (_titleRollupProducts.ContainsKey(productGroup.ProductGroupId))
		{
			LogHelper.Instance.Log("KioskProductView.AddTitleRollupProduct - Title Rollup product {0} already exists", productGroup.ProductGroupId);
			return null;
		}
		AddMissingProductIds(productGroup.ProductIds);
		TitleRollupProduct titleRollupProduct = new TitleRollupProduct(productGroup, _titleProducts, _mdvProducts, ProfileDataInstance, _store);
		_titleRollupProducts[titleRollupProduct.ProductGroupId] = titleRollupProduct;
		return titleRollupProduct;
	}

	private void LoadTitlesWithNoRollupOrMDV()
	{
		_titlesWithNoRollup.Clear();
		List<ITitleProduct> second = (from _003C_003Eh__TransparentIdentifier0 in _titleRollupProducts.SelectMany(delegate(KeyValuePair<long, TitleRollupProduct> item)
			{
				KeyValuePair<long, TitleRollupProduct> keyValuePair = item;
				return keyValuePair.Value.KioskProducts;
			}, (KeyValuePair<long, TitleRollupProduct> item, ITitleProduct title) => new { item, title })
			where _003C_003Eh__TransparentIdentifier0.title != null
			select _003C_003Eh__TransparentIdentifier0.title).ToList();
		List<ITitleProduct> second2 = _mdvProducts.SelectMany(delegate(KeyValuePair<long, MDVProduct> item)
		{
			KeyValuePair<long, MDVProduct> keyValuePair = item;
			return keyValuePair.Value.TitleProducts;
		}, (KeyValuePair<long, MDVProduct> item, ITitleProduct title) => title).ToList();
		_titleProducts.Select((KeyValuePair<long, ITitleProduct> item) => item.Value).ToList().Except(second)
			.Except(second2)
			.ToList()
			.ForEach(delegate(ITitleProduct item)
			{
				if (item.IsMovie)
				{
					LogHelper.Instance.Log($"KioskProductView.LoadTitlesWithNoRollup - Title has no rollup id:{item.ProductId} name:{item.ProductName}", LogEntryType.Debug);
				}
				_titlesWithNoRollup.Add(item);
			});
	}

	private void LoadMDVTitlesWithNoRollup()
	{
		_mdvTitlesWithNoRollup.Clear();
		List<MDVProduct> second = (from _003C_003Eh__TransparentIdentifier0 in _titleRollupProducts.SelectMany(delegate(KeyValuePair<long, TitleRollupProduct> item)
			{
				KeyValuePair<long, TitleRollupProduct> keyValuePair = item;
				return keyValuePair.Value.KioskProducts;
			}, (KeyValuePair<long, TitleRollupProduct> item, ITitleProduct title) => new { item, title })
			where _003C_003Eh__TransparentIdentifier0.title is MDVProduct
			select (MDVProduct)_003C_003Eh__TransparentIdentifier0.title).ToList();
		_mdvProducts.Select((KeyValuePair<long, MDVProduct> item) => item.Value).ToList().Except(second)
			.ToList()
			.ForEach(delegate(MDVProduct item)
			{
				if (item.IsMovie)
				{
					LogHelper.Instance.Log($"KioskProductView.LoadMDVTitlesWithNoRollup - MDV Title has no rollup productgroupid:{item.ProductGroupId} name:{item.ProductName}", LogEntryType.Debug);
				}
				_mdvTitlesWithNoRollup.Add(item);
			});
	}

	private void LoadMDVWorkaroundTitlesWithNoRollup()
	{
		_mdvWorkaroundTitlesWithNoRollup.Clear();
		_mdvWorkaroundProducts.Select((KeyValuePair<long, MDVProduct> item) => item.Value).ToList().ToList()
			.ForEach(delegate(MDVProduct item)
			{
				if (item.IsMovie)
				{
					LogHelper.Instance.Log($"KioskProductView.LoadMDVWorkaroundTitlesWithNoRollup - MDV Rollup Title has no rollup productgroupid:{item.ProductGroupId} name:{item.ProductName}", LogEntryType.Debug);
				}
				_mdvWorkaroundTitlesWithNoRollup.Add(item);
			});
	}

	private void LoadGames()
	{
		_gamesProducts.Clear();
		Dictionary<TitleType, bool> allowedGameTypes = GetEnabledGameTypes();
		_gamesProducts.AddRange((from a in _titleRollupProducts
			select a.Value into item
			where item.IsGame && allowedGameTypes.ContainsKey(item.TitleType) && allowedGameTypes[item.TitleType]
			select item).Select((Func<TitleRollupProduct, ITitleProduct>)((TitleRollupProduct item) => item)));
		_gamesProducts.AddRange(_mdvTitlesWithNoRollup.Where((MDVProduct item) => item.IsGame && allowedGameTypes.ContainsKey(item.TitleType) && allowedGameTypes[item.TitleType]).Select((Func<MDVProduct, ITitleProduct>)((MDVProduct item) => item)));
		_gamesProducts.AddRange(from item in _titlesWithNoRollup
			where item.IsGame && allowedGameTypes.ContainsKey(item.TitleType) && allowedGameTypes[item.TitleType]
			select (item));
	}

	private Dictionary<TitleType, bool> GetEnabledGameTypes()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		return new Dictionary<TitleType, bool>
		{
			{
				TitleType.Xbox360,
				GetGameConfig(service, "GameEnableXbox360")
			},
			{
				TitleType.XboxOne,
				GetGameConfig(service, "GameEnableXboxOne")
			},
			{
				TitleType.PS3,
				GetGameConfig(service, "GameEnablePS3")
			},
			{
				TitleType.PS4,
				GetGameConfig(service, "GameEnablePS4")
			},
			{
				TitleType.Wii,
				GetGameConfig(service, "GameEnableWii")
			},
			{
				TitleType.WiiU,
				GetGameConfig(service, "GameEnableWiiU")
			},
			{
				TitleType.NintendoSwitch,
				GetGameConfig(service, "GameEnableNintendoSwitch")
			}
		};
	}

	private bool GetGameConfig(IConfigurationService configService, string settingName)
	{
		bool value = false;
		if (configService != null && !configService.TryGetValue<bool>("system", "Games", settingName, out value))
		{
			value = false;
		}
		return value;
	}

	private void LoadMovies()
	{
		_movieProducts.Clear();
		_movieProducts.AddRange((from a in _titleRollupProducts
			select a.Value into item
			where item.IsMovie
			select item).Select((Func<TitleRollupProduct, ITitleProduct>)((TitleRollupProduct item) => item)));
		_movieProducts.AddRange(_mdvTitlesWithNoRollup.Where((MDVProduct item) => item.IsMovie).Select((Func<MDVProduct, ITitleProduct>)((MDVProduct item) => item)));
		_movieProducts.AddRange(from item in _titlesWithNoRollup
			where item.IsMovie
			select (item));
	}

	private void LoadMoreLikeThis()
	{
		try
		{
			LogHelper.Instance.Log("Sending productGroupIds to KCS for morelikethis.");
			IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
			IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
			List<long> productGroupIds = _movieProducts.Select((ITitleProduct x) => dataService.GetProductGroupId(x)).ToList();
			service.ProductGroups.RefreshAllMLTData(productGroupIds);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("KioskProductView.LoadMoreLikeThis - an unhandled exception occurred", e);
		}
	}

	private void LoadSubscriptionProducts()
	{
		LogHelper.Instance.Log("Loading subscription products.");
		ISubscriptionPlans subscriptionPlans = ServiceLocator.Instance.GetService<IKioskClientService>()?.Subscriptions?.GetSubscriptionPlans();
		_subscriptionProducts = subscriptionPlans?.SubscriptionProducts;
		_redboxPlusPromoCampaigns = subscriptionPlans?.RedboxPlusPromoCampaigns;
	}
}
