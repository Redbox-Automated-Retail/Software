using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class TitleRollupProduct : KioskProduct, ITitleRollupProduct, ITitleProduct, IKioskProduct
{
	protected List<ITitleProduct> _kioskProducts = new List<ITitleProduct>();

	private Dictionary<long, ITitleProduct> _allTitleProducts;

	private Dictionary<long, MDVProduct> _allMDVProducts;

	private IProfileDataInstance _profile;

	private IKioskProduct _primaryProduct;

	private ITitleProduct _primaryTitle;

	private int _sortOrderType;

	private int _sortOrderPosition;

	public Dictionary<string, BarcodeProduct> BarCodeProducts
	{
		get
		{
			Dictionary<string, BarcodeProduct> retBarCodes = new Dictionary<string, BarcodeProduct>();
			foreach (ITitleProduct kioskProduct in _kioskProducts)
			{
				kioskProduct.BarCodeProducts.ToList().ForEach(delegate(KeyValuePair<string, BarcodeProduct> x)
				{
					retBarCodes[x.Key] = x.Value;
				});
			}
			return retBarCodes;
		}
	}

	public List<ITitleProduct> KioskProducts => _kioskProducts;

	private ProductGroup _productGroup { get; set; }

	public IKioskProduct PrimaryProduct => _primaryProduct;

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
				return _kioskProducts.All((ITitleProduct tp) => tp.IsValidProduct);
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
			return TitleType.DVD;
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

	public bool SellThru => _kioskProducts.Any((ITitleProduct kp) => kp.SellThru);

	public bool SellThruNew => _kioskProducts.Any((ITitleProduct kp) => kp.SellThruNew);

	public DateTime? SellThruDate => _kioskProducts.Min((ITitleProduct kp) => kp.SellThruDate);

	public decimal? SellThruPrice => _kioskProducts.Max((ITitleProduct kp) => kp.SellThruPrice);

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

	public bool IsSellable => _kioskProducts.Any((ITitleProduct kp) => kp.IsSellable);

	public bool IsRentable => _kioskProducts.Any((ITitleProduct kp) => kp.IsRentable);

	public bool IsSellableAndInStock => _kioskProducts.Any((ITitleProduct kp) => kp.IsSellableAndInStock);

	public bool IsRentableAndInStock => _kioskProducts.Any((ITitleProduct kp) => kp.IsRentableAndInStock);

	public decimal PurchasePrice => _kioskProducts.Max((ITitleProduct kp) => kp.PurchasePrice);

	public IPricingRecord PurchasePriceRecord => _primaryTitle?.PurchasePriceRecord;

	public IList<IPricingRecord> RentalPriceRecords => _primaryTitle?.RentalPriceRecords;

	public bool DoNotRent => _kioskProducts.All((ITitleProduct kp) => kp.DoNotRent);

	public bool IsAvailable => _kioskProducts.Any((ITitleProduct kp) => kp.IsAvailable);

	public bool ComingSoon
	{
		get
		{
			List<ITitleProduct> list = _kioskProducts.Where((ITitleProduct kp) => kp.TitleType != TitleType.DigitalCode).ToList();
			if (list.Count == 0)
			{
				return false;
			}
			return list.All((ITitleProduct kp) => kp.ComingSoon);
		}
	}

	public bool InStock => _kioskProducts.Any((ITitleProduct kp) => kp.InStock);

	public bool ShowOutOfStock
	{
		get
		{
			if (_kioskProducts.Where((ITitleProduct kp) => kp.ShowOutOfStock).ToList().Count == 0)
			{
				return false;
			}
			return _kioskProducts.All((ITitleProduct kp) => kp.ShowOutOfStock);
		}
	}

	public bool HasBeenInStock => _kioskProducts.Any((ITitleProduct kp) => kp.HasBeenInStock);

	public bool HasNeverBeenInStockAndSortOrderPosition
	{
		get
		{
			if (_kioskProducts.Where((ITitleProduct kp) => kp.HasNeverBeenInStockAndSortOrderPosition).ToList().Count == 0)
			{
				return false;
			}
			return _kioskProducts.All((ITitleProduct kp) => kp.HasNeverBeenInStockAndSortOrderPosition);
		}
	}

	public bool HasDeal => _kioskProducts.Any((ITitleProduct kp) => kp.HasDeal);

	public bool HasMultiNightPrice => _kioskProducts.Any((ITitleProduct kp) => kp.HasMultiNightPrice);

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

	public long TitleRollupProductGroupId => _primaryTitle?.TitleRollupProductGroupId ?? 0;

	public List<long> RequiredAdditionalTitles { get; private set; }

	public string TivoQueryId { get; set; }

	public decimal? DigitalRentPrice => KioskProducts.FirstOrDefault()?.DigitalRentPrice;

	public int SortOrderType
	{
		get
		{
			return _sortOrderType;
		}
		set
		{
			_sortOrderType = value;
			KioskProducts.ForEach(delegate(ITitleProduct p)
			{
				if (p != null)
				{
					p.SortOrderType = value;
				}
			});
		}
	}

	public int SortOrderPosition
	{
		get
		{
			return _sortOrderPosition;
		}
		set
		{
			_sortOrderPosition = value;
			KioskProducts.ForEach(delegate(ITitleProduct p)
			{
				if (p != null)
				{
					p.SortOrderPosition = value;
				}
			});
		}
	}

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

	public TitleRollupProduct(ProductGroup productGroup, Dictionary<long, ITitleProduct> titleProducts, Dictionary<long, MDVProduct> mdvProducts, IProfileDataInstance profile, Store store)
		: base(store)
	{
		base.KioskProductType = KioskProductType.TitleRollup;
		_productGroup = productGroup;
		_allTitleProducts = titleProducts;
		_allMDVProducts = mdvProducts;
		_profile = profile;
		LoadKioskProducts();
	}

	public override string ToString()
	{
		return $"TitleRollupProduct {ProductId} {ProductName}";
	}

	private void LoadKioskProducts()
	{
		try
		{
			_productGroup.ProductIds.ForEach(delegate(long id)
			{
				if (_allTitleProducts.ContainsKey(id))
				{
					ITitleProduct title = _allTitleProducts[id];
					MDVProduct mDVProduct = _allMDVProducts.Where(delegate(KeyValuePair<long, MDVProduct> item)
					{
						KeyValuePair<long, MDVProduct> keyValuePair = item;
						return keyValuePair.Value.TitleProducts.Contains(title);
					}).Select(delegate(KeyValuePair<long, MDVProduct> item)
					{
						KeyValuePair<long, MDVProduct> keyValuePair = item;
						return keyValuePair.Value;
					}).FirstOrDefault();
					if (mDVProduct != null)
					{
						if (!_kioskProducts.Contains(mDVProduct))
						{
							_kioskProducts.Add(mDVProduct);
						}
					}
					else
					{
						_kioskProducts.Add(title);
					}
					if (title.IsMovie && title.TitleType == TitleType.DVD)
					{
						_primaryProduct = title;
					}
					if (title.IsGame && title.TitleType == TitleType.Xbox360)
					{
						_primaryProduct = title;
					}
				}
				else
				{
					LogHelper.Instance.Log("TitleRollupProduct.LoadKioskProducts - ProductGroupId {0} has a product id {1} that does not exist.", _productGroup.ProductGroupId, id);
				}
			});
			if (_primaryProduct == null)
			{
				if (_kioskProducts.Count() == 0)
				{
					IsValidProduct = false;
				}
				else
				{
					_primaryProduct = _kioskProducts.First();
				}
			}
		}
		finally
		{
			if (_primaryProduct != null)
			{
				if (_primaryProduct is MDVProduct)
				{
					_primaryTitle = (_primaryProduct as MDVProduct).PrimaryTitle;
				}
				else if (_primaryProduct is TitleProduct)
				{
					_primaryTitle = _primaryProduct as TitleProduct;
				}
			}
		}
	}
}
