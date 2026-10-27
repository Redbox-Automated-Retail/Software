using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Product;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskClientService.Product;

namespace Redbox.Rental.Services.KioskProduct;

public class TitleProduct : KioskProduct, ITitleProduct, IKioskProduct
{
	private Title _title;

	private TitleCache _titleCache;

	private TitleType _titleType;

	private IProfileDataInstance _profile;

	private IStoreManager _storeManager;

	private IPricingService _pricingService;

	private long _titleRollupProductGroupId;

	private List<string> _availableStatuses = new List<string> { "Known", "Thinned", "Redeployment", "Rebalancing" };

	public Dictionary<string, BarcodeProduct> BarCodeProducts => _titleCache.BarcodeProducts;

	public override string ProductName => _title.LongName;

	public override string ProductDescription => _title.Description;

	public long ProductId => _title.ProductId;

	public string RunningTime => _title.RunningTime;

	public List<string> Starring => _title.Starring;

	public int ProductTypeId => _title.ProductTypeId;

	public TitleType TitleType => GetTitleTypeFromProductTypeId(_title.ProductTypeId);

	public TitleFamily TitleFamily => TitleType.ToFamily();

	public long? RatingId => _title.RatingId;

	public string LongName => _title.LongName;

	public string ImageFile => _title.ImageFile;

	public string Description => _title.Description;

	public List<long> Genres => _title.Genres;

	public DateTime? ReleaseDate => _title.ReleaseDate;

	public decimal BoxOfficeGross => _title.BoxOfficeGross;

	public List<string> Directors => _title.Directors;

	public int ComingSoonDays => _title.ComingSoonDays;

	public DateTime? NationalStreetDate => _title.NationalStreetDate;

	public bool ClosedCaptioned => _title.ClosedCaptioned;

	public DateTime MerchandiseDate => _title.MerchandiseDate ?? DateTime.Now.AddYears(10);

	public DateTime? DoNotRentDate => _title.DoNotRentDate;

	public bool SellThru => _title.SellThru;

	public bool SellThruNew => _title.SellThruNew;

	public DateTime? SellThruDate => _title.SellThruDate;

	public decimal? SellThruPrice => _title.SellThruPrice;

	public long? SubPlatformId => _title.SubPlatformId;

	public string NumberOfPlayersText => _title.NumberOfPlayersText;

	public int? DiscNumber => _title.DiscNumber;

	public bool IsGame => TitleFamily == TitleFamily.Games;

	public bool IsMovie => TitleFamily == TitleFamily.Movies;

	public bool IsEmptyCase => ProductId == 1139;

	public List<long> RequiredAdditionalTitles { get; private set; }

	public string TivoQueryId { get; set; }

	public bool IsSellable
	{
		get
		{
			if (IsEmptyCase)
			{
				return true;
			}
			if (TitleFamily == TitleFamily.Games)
			{
				return false;
			}
			if (PriceEngineIsSellable)
			{
				return PurchasePrice > 0m;
			}
			return false;
		}
	}

	private bool PriceEngineIsSellable
	{
		get
		{
			if (IsEmptyCase)
			{
				return true;
			}
			if (DoNotRent || !IsValidProduct || !SellThru || !IsAvailable)
			{
				return false;
			}
			if (IsGame && !_storeManager.CanSellUsedGames)
			{
				return false;
			}
			if (IsMovie && !_storeManager.CanSellUsedMovies)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsRentable
	{
		get
		{
			if (IsEmptyCase)
			{
				return true;
			}
			if (DoNotRent || !IsValidProduct)
			{
				return false;
			}
			if (TitleType == TitleType.DigitalCode)
			{
				return false;
			}
			if (TitleFamily == TitleFamily.Games)
			{
				return false;
			}
			return IsAvailable;
		}
	}

	public bool IsSellableAndInStock
	{
		get
		{
			if (IsSellable)
			{
				return InStock;
			}
			return false;
		}
	}

	public bool IsRentableAndInStock
	{
		get
		{
			if (IsRentable)
			{
				return InStock;
			}
			return false;
		}
	}

	public decimal PurchasePrice
	{
		get
		{
			if (IsEmptyCase)
			{
				return PricingService.GetEmptyCasePrice();
			}
			if (PriceEngineIsSellable)
			{
				return PurchasePriceRecord?.Purchase ?? 0m;
			}
			return 0m;
		}
	}

	public IPricingRecord PurchasePriceRecord
	{
		get
		{
			if (IsEmptyCase)
			{
				return new PricingRecord
				{
					Purchase = PricingService.GetEmptyCasePrice()
				};
			}
			return PricingService.GetPurchasePricing(this) ?? new PricingRecord
			{
				Purchase = 0m
			};
		}
	}

	public IList<IPricingRecord> RentalPriceRecords => PricingService.GetProductPricingRecords(ProductId);

	private IConfiguration Config => Redbox.Rental.Services.Configuration.Configuration.Instance;

	private IPricingService PricingService
	{
		get
		{
			if (_pricingService == null)
			{
				_pricingService = ServiceLocator.Instance.GetService<IPricingService>();
			}
			return _pricingService;
		}
	}

	public long TitleRollupProductGroupId
	{
		get
		{
			if (_titleRollupProductGroupId > 0)
			{
				return _titleRollupProductGroupId;
			}
			_profile?.ProductGroups?.Where((KeyValuePair<long, Redbox.Rental.Model.Profile.ProductGroup> pair) => pair.Value.ProductGroupTypeId == 1).ForEach(delegate(KeyValuePair<long, Redbox.Rental.Model.Profile.ProductGroup> pair)
			{
				if (pair.Value.ProductIds.Contains(ProductId))
				{
					_titleRollupProductGroupId = pair.Value.ProductGroupId;
				}
			});
			return _titleRollupProductGroupId;
		}
	}

	public bool DoNotRent
	{
		get
		{
			if (!_title.DoNotRentDate.HasValue)
			{
				return false;
			}
			return _title.DoNotRentDate <= DateTime.Now;
		}
	}

	public bool IsAvailable
	{
		get
		{
			if (TitleType == TitleType.DigitalCode)
			{
				return true;
			}
			return DateTime.Now > MerchandiseDate;
		}
	}

	public bool ComingSoon
	{
		get
		{
			if (DoNotRent || !IsValidProduct)
			{
				return false;
			}
			if (ProductId == 1139)
			{
				return false;
			}
			if (TitleType == TitleType.DigitalCode || TitleType == TitleType.NintendoSwitch)
			{
				return false;
			}
			if (IsAvailable)
			{
				return false;
			}
			if (InStock)
			{
				return true;
			}
			return (MerchandiseDate - DateTime.Now).Days <= ComingSoonDays;
		}
	}

	public bool InStock
	{
		get
		{
			if (BarCodeProducts != null)
			{
				return BarCodeProducts.Any((KeyValuePair<string, BarcodeProduct> b) => _availableStatuses.Contains(b.Value.Status));
			}
			return false;
		}
	}

	public bool ShowOutOfStock
	{
		get
		{
			if (InStock)
			{
				return false;
			}
			if (TitleType == TitleType.DigitalCode)
			{
				return false;
			}
			if (HasNeverBeenInStockAndSortOrderPosition)
			{
				return true;
			}
			if (!HasBeenInStock)
			{
				return false;
			}
			if (_titleCache.ThinnedOut)
			{
				return false;
			}
			return (DateTime.Now.Date - _titleCache.LastDateOutOfStock.Value).Days < 25;
		}
	}

	public bool HasBeenInStock
	{
		get
		{
			if (!_titleCache.LastDateOutOfStock.HasValue)
			{
				return false;
			}
			return true;
		}
	}

	public bool HasNeverBeenInStockAndSortOrderPosition
	{
		get
		{
			if (!InStock && !HasBeenInStock && SortOrderPosition > 0)
			{
				return true;
			}
			return false;
		}
	}

	public bool HasDeal
	{
		get
		{
			IList<IPricingRecord> obj = ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(_title.ProductId);
			bool hasMdvWorkaround = RequiredAdditionalTitles.Count > 0;
			return obj?.Any(delegate(IPricingRecord pr)
			{
				if (pr == null)
				{
					return false;
				}
				return pr.IsDeal && !hasMdvWorkaround;
			}) ?? false;
		}
	}

	public bool HasMultiNightPrice => (ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(_title.ProductId))?.Any((IPricingRecord pr) => pr?.IsMultiNightPrice ?? false) ?? false;

	public string SortAlpha => _title.SortName.ToLower();

	public DateTime SortMerchandise => _title.MerchandiseDate ?? DateTime.Now;

	public DateTime SortDate => _title.SortDate ?? DateTime.Now;

	public decimal SortBoxOfficeGross => _title.BoxOfficeGross;

	public string SortName => _title.SortName.ToLower();

	public decimal? DigitalRentPrice
	{
		get
		{
			decimal? result = null;
			if (_title.DigitalSDRentPrice.HasValue || _title.DigitalHDRentPrice.HasValue)
			{
				decimal? num = _title.DigitalHDRentPrice ?? _title.DigitalSDRentPrice;
				decimal? num2 = _title.DigitalSDRentPrice ?? _title.DigitalHDRentPrice;
				result = Math.Min(num.Value, num2.Value);
			}
			return result;
		}
	}

	public int SortOrderType { get; set; }

	public int SortOrderPosition { get; set; }

	public string Studio => _title.Studio;

	public DateTime? RedboxPlusEligibleDate => _title.RedboxPlusEligibleDate;

	public bool IsRedboxPlusEligible
	{
		get
		{
			if (!RedboxPlusEligibleDate.HasValue)
			{
				return false;
			}
			return RedboxPlusEligibleDate <= DateTime.Now;
		}
	}

	public DiscountRestriction DiscountRestriction => _title.DiscountRestriction;

	public string ProductWebPageUrl
	{
		get
		{
			Product product = (ServiceLocator.Instance.GetService<IKioskClientService>()?.Product?.ProductsByTitleIds(new ProductByTitleIdsRequest
			{
				TitleIds = { _title.ProductId }
			}))?.Products?.FirstOrDefault();
			if (!string.IsNullOrEmpty(product?.ProductPage))
			{
				return product.ProductPage;
			}
			string text = _title.LongName.ToLower();
			if (text.EndsWith(")"))
			{
				text = text.Remove(text.Length - 1, 1);
			}
			text = text.Replace("\\s", "-").Replace("(", "-").Replace(")", "-")
				.Replace("/", "-")
				.Replace(":", "-")
				.Replace(" ", "-")
				.Replace("--", "-")
				.Replace("'", "")
				.Replace("*", "")
				.Replace("?", "")
				.Replace(".", "")
				.Replace("!", "")
				.Replace(",", "")
				.Replace("%", "")
				.Replace(">", "")
				.Replace("<", "")
				.Replace("+", "and")
				.Replace("$", "s")
				.Replace("#", "number")
				.Replace("@", "at")
				.Replace("=", "equals");
			return "https://www.redbox.com/movies/" + text;
		}
	}

	public TitleProduct(Title title, TitleCache titleCache, TitleType titleType, IProfileDataInstance profile, Store store, IStoreManager storeManager)
		: base(store)
	{
		base.KioskProductType = KioskProductType.Title;
		_title = title;
		_titleCache = titleCache;
		_titleType = titleType;
		_profile = profile;
		_storeManager = storeManager;
		RequiredAdditionalTitles = GetRequiredAdditionalTitles();
	}

	public override string ToString()
	{
		return $"TitleProduct {ProductId} {ProductName}";
	}

	public static TitleType GetTitleTypeFromProductTypeId(int productTypeId)
	{
		TitleType result = TitleType.DVD;
		if (Enum.IsDefined(typeof(TitleType), productTypeId))
		{
			result = (TitleType)productTypeId;
		}
		return result;
	}

	private decimal GetAdjustedPurchasePrice(decimal price)
	{
		if (_store == null || string.IsNullOrEmpty(_store.State))
		{
			return price;
		}
		if (_store.State == "MD")
		{
			decimal num = ((TitleType == TitleType.DigitalCode) ? _store.DigitalPurchaseTaxRate : _store.PurchaseTaxRate);
			if (num > 0m)
			{
				return Math.Round(price * (num / 100m + 1m), 2);
			}
		}
		return price;
	}

	private List<long> GetRequiredAdditionalTitles()
	{
		List<long> list = new List<long>();
		if (_profile != null && _title != null && _profile.MultiDiscWorkaroundXREF.ContainsKey(_title.ProductId))
		{
			long productGroupId = _profile.MultiDiscWorkaroundXREF[_title.ProductId].ProductGroupId;
			Redbox.Rental.Model.Profile.ProductGroup productGroup = _profile.ProductGroups[productGroupId];
			list.AddRange(productGroup.ProductIds.Where((long i) => i != _title.ProductId));
		}
		return list;
	}
}
