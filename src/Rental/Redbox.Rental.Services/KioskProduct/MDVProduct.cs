using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class MDVProduct : KioskProduct, ITitleProduct, IKioskProduct
{
	private Dictionary<long, ITitleProduct> _allTitleProducts;

	private IProfileDataInstance _profile;

	private ITitleProduct _primaryTitle;

	protected List<ITitleProduct> _titleProducts = new List<ITitleProduct>();

	public Dictionary<string, BarcodeProduct> BarCodeProducts
	{
		get
		{
			Dictionary<string, BarcodeProduct> retBarCodes = new Dictionary<string, BarcodeProduct>();
			foreach (ITitleProduct titleProduct in _titleProducts)
			{
				titleProduct.BarCodeProducts.ToList().ForEach(delegate(KeyValuePair<string, BarcodeProduct> x)
				{
					retBarCodes.Add(x.Key, x.Value);
				});
			}
			return retBarCodes;
		}
	}

	private ProductGroup _productGroup { get; set; }

	public ITitleProduct PrimaryTitle => _primaryTitle;

	public override string ProductName => _productGroup.ProductGroupName;

	public override string ProductDescription
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Description;
			}
			return string.Empty;
		}
	}

	public override bool IsValidProduct
	{
		get
		{
			if (_isValidProduct)
			{
				return _titleProducts.All((ITitleProduct tp) => tp.IsValidProduct);
			}
			return false;
		}
		set
		{
			_isValidProduct = value;
		}
	}

	public long ProductGroupId => _productGroup.ProductGroupId;

	public string ProductGroupName => _productGroup.ProductGroupName;

	public List<ITitleProduct> TitleProducts => _titleProducts;

	public long ProductId
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ProductId;
			}
			return 0L;
		}
	}

	public string RunningTime
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.RunningTime;
			}
			return string.Empty;
		}
	}

	public List<string> Starring
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Starring;
			}
			return new List<string>();
		}
	}

	public int ProductTypeId
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ProductTypeId;
			}
			return 0;
		}
	}

	public TitleType TitleType
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.TitleType;
			}
			return TitleType.Xbox360;
		}
	}

	public TitleFamily TitleFamily => TitleType.ToFamily();

	public long? RatingId
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.RatingId;
			}
			return 0L;
		}
	}

	public string LongName
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.LongName;
			}
			return string.Empty;
		}
	}

	public string ImageFile
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ImageFile;
			}
			return string.Empty;
		}
	}

	public string Description
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Description;
			}
			return string.Empty;
		}
	}

	public List<long> Genres
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Genres;
			}
			return new List<long>();
		}
	}

	public DateTime? ReleaseDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ReleaseDate;
			}
			return DateTime.Now;
		}
	}

	public decimal BoxOfficeGross
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.BoxOfficeGross;
			}
			return 0m;
		}
	}

	public List<string> Directors
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Directors;
			}
			return new List<string>();
		}
	}

	public int ComingSoonDays
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ComingSoonDays;
			}
			return 0;
		}
	}

	public DateTime? NationalStreetDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.NationalStreetDate;
			}
			return DateTime.Now;
		}
	}

	public bool ClosedCaptioned
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ClosedCaptioned;
			}
			return false;
		}
	}

	public DateTime MerchandiseDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.MerchandiseDate;
			}
			return DateTime.Now.AddYears(10);
		}
	}

	public DateTime? DoNotRentDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.DoNotRentDate;
			}
			return DateTime.Now;
		}
	}

	public bool SellThru
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SellThru;
			}
			return false;
		}
	}

	public bool SellThruNew
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SellThruNew;
			}
			return false;
		}
	}

	public DateTime? SellThruDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SellThruDate;
			}
			return DateTime.Now;
		}
	}

	public decimal? SellThruPrice
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SellThruPrice;
			}
			return 0m;
		}
	}

	public long? SubPlatformId
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SubPlatformId;
			}
			return 0L;
		}
	}

	public string NumberOfPlayersText
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.NumberOfPlayersText;
			}
			return string.Empty;
		}
	}

	public int? DiscNumber
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.DiscNumber;
			}
			return 0;
		}
	}

	public bool IsGame => TitleFamily == TitleFamily.Games;

	public bool IsMovie => TitleFamily == TitleFamily.Movies;

	public bool IsEmptyCase => ProductId == 1139;

	public bool IsSellable => false;

	public bool IsRentable
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.IsRentable;
			}
			return false;
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
			if (_primaryTitle != null)
			{
				return _primaryTitle.PurchasePrice;
			}
			return 0m;
		}
	}

	public IPricingRecord PurchasePriceRecord => _primaryTitle?.PurchasePriceRecord;

	public IList<IPricingRecord> RentalPriceRecords => _primaryTitle?.RentalPriceRecords;

	public long TitleRollupProductGroupId => _primaryTitle?.TitleRollupProductGroupId ?? 0;

	public bool DoNotRent => _titleProducts.Any((ITitleProduct tp) => tp.DoNotRent);

	public bool IsAvailable
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.IsAvailable;
			}
			return false;
		}
	}

	public bool ComingSoon
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ComingSoon;
			}
			return false;
		}
	}

	public bool InStock => _titleProducts.All((ITitleProduct tp) => tp.InStock);

	public bool ShowOutOfStock => _titleProducts.Any((ITitleProduct tp) => tp.ShowOutOfStock);

	public bool HasBeenInStock
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.HasBeenInStock;
			}
			return false;
		}
	}

	public bool HasNeverBeenInStockAndSortOrderPosition
	{
		get
		{
			if (_titleProducts.Where((ITitleProduct tp) => tp.HasNeverBeenInStockAndSortOrderPosition).ToList().Count == 0)
			{
				return false;
			}
			return _titleProducts.All((ITitleProduct tp) => tp.HasNeverBeenInStockAndSortOrderPosition);
		}
	}

	public bool HasDeal => (ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(_primaryTitle.ProductId))?.Any((IPricingRecord pr) => pr?.IsDeal ?? false) ?? false;

	public bool HasMultiNightPrice => (ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(_primaryTitle.ProductId))?.Any((IPricingRecord pr) => pr?.IsMultiNightPrice ?? false) ?? false;

	public string SortAlpha
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortAlpha;
			}
			return string.Empty;
		}
	}

	public DateTime SortMerchandise
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortMerchandise;
			}
			return DateTime.Now;
		}
	}

	public DateTime SortDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortDate;
			}
			return DateTime.Now;
		}
	}

	public decimal SortBoxOfficeGross
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortBoxOfficeGross;
			}
			return 0m;
		}
	}

	public string SortName
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortName;
			}
			return string.Empty;
		}
	}

	public List<long> RequiredAdditionalTitles { get; private set; }

	public decimal? DigitalRentPrice
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public string TivoQueryId { get; set; }

	public int SortOrderType { get; set; }

	public int SortOrderPosition { get; set; }

	public string Studio
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Studio;
			}
			return string.Empty;
		}
	}

	public DateTime? RedboxPlusEligibleDate => _primaryTitle?.RedboxPlusEligibleDate;

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

	public DiscountRestriction DiscountRestriction
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.DiscountRestriction;
			}
			return DiscountRestriction.None;
		}
	}

	public string ProductWebPageUrl
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ProductWebPageUrl;
			}
			return string.Empty;
		}
	}

	public MDVProduct(ProductGroup productGroup, Dictionary<long, ITitleProduct> titleProducts, IProfileDataInstance profile, Store store)
		: base(store)
	{
		base.KioskProductType = KioskProductType.MDV;
		_productGroup = productGroup;
		_allTitleProducts = titleProducts;
		_profile = profile;
		LoadTitles();
	}

	public override string ToString()
	{
		return $"MDVProduct {ProductId} {ProductName}";
	}

	private void LoadTitles()
	{
		_productGroup.ProductIds.ForEach(delegate(long id)
		{
			if (_allTitleProducts.ContainsKey(id))
			{
				ITitleProduct titleProduct = _allTitleProducts[id];
				_titleProducts.Add(titleProduct);
				if (!titleProduct.DiscNumber.HasValue)
				{
					IsValidProduct = false;
					LogHelper.Instance.Log("MDVProduct.LoadTitles - ProductGroupId {0} has a product id {1} that does not have a disc number.", _productGroup.ProductGroupId, id);
				}
				else if (titleProduct.DiscNumber.Value == 1)
				{
					_primaryTitle = titleProduct;
				}
			}
			else
			{
				IsValidProduct = false;
				LogHelper.Instance.Log("MDVProduct.LoadTitles - ProductGroupId {0} has a product id {1} that does not exist.", _productGroup.ProductGroupId, id);
			}
		});
		if (_primaryTitle == null)
		{
			LogHelper.Instance.Log("MDVProduct.LoadTitles - ProductGroupId {0} does not have a primary disc (disc 1)", _productGroup.ProductGroupId);
			IsValidProduct = false;
		}
	}
}
