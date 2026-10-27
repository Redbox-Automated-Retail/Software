using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Redbox.Rental.Model.Cache;

namespace Redbox.Rental.UI.Models;

public class TitleProductDetailDevModel : BaseModel<TitleDetailDevModel>
{
	public static readonly DependencyProperty ImgFileProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("ImgFileProperty", TYPES.OBJECT);

	public static readonly DependencyProperty TypeProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("TypeProperty", TYPES.STRING);

	public static readonly DependencyProperty ShowOutOfStockProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("ShowOutOfStockProperty", TYPES.BOOL, false);

	public static readonly DependencyProperty IsAvailableProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("IsAvailableProperty", TYPES.BOOL, false);

	public static readonly DependencyProperty ProductTypeNameProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("ProductTypeNameProperty", TYPES.STRING);

	public static readonly DependencyProperty ProductFamilyNameProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("ProductFamilyNameProperty", TYPES.STRING);

	public static readonly DependencyProperty CanBeSoldProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("CanBeSoldProperty", TYPES.BOOL, false);

	public static readonly DependencyProperty InStockProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("InStockProperty", TYPES.BOOL, false);

	public static readonly DependencyProperty IsComingSoonProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("IsComingSoonProperty", TYPES.BOOL, false);

	public static readonly DependencyProperty GenreIdsProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("GenreIdsProperty", TYPES.OBJECT);

	public static readonly DependencyProperty GenreListProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("GenreListProperty", TYPES.OBJECT);

	public static readonly DependencyProperty PurchaseProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("PurchaseProperty", TYPES.DECIMAL, 0m);

	public static readonly DependencyProperty PromoValueProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("PromoValueProperty", TYPES.DECIMAL, 0m);

	public static readonly DependencyProperty NonReturnDaysProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("NonReturnDaysProperty", TYPES.DECIMAL, 0m);

	public static readonly DependencyProperty NonReturnProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("NonReturnProperty", TYPES.DECIMAL, 0m);

	public static readonly DependencyProperty InitialNightProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("InitialNightProperty", TYPES.DECIMAL, 0m);

	public static readonly DependencyProperty PriceSetIdProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("PriceSetIdProperty", TYPES.STRING);

	public static readonly DependencyProperty ExpirationPriceProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("ExpirationPriceProperty", TYPES.DECIMAL, 0m);

	public static readonly DependencyProperty ExtraNightProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("ExtraNightProperty", TYPES.DECIMAL, 0m);

	public static readonly DependencyProperty IsAlternatePricedProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("IsAlternatePricedProperty", TYPES.BOOL, false);

	public static readonly DependencyProperty ReleaseDateProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("ReleaseDateProperty", TYPES.STRING);

	public static readonly DependencyProperty SellThruProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("SellThruProperty", TYPES.BOOL, false);

	public static readonly DependencyProperty SortDateProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("SortDateProperty", TYPES.STRING);

	public static readonly DependencyProperty MerchandiseDateProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("MerchandiseDateProperty", TYPES.STRING);

	public static readonly DependencyProperty ProductTypeIdProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("ProductTypeIdProperty", TYPES.INT, 0);

	public static readonly DependencyProperty RaitingIdProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("RaitingIdProperty", TYPES.INT, 0);

	public static readonly DependencyProperty TitleRollupProductGroupIdProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("TitleRollupProductGroupIdProperty", typeof(long), 0L);

	public static readonly DependencyProperty DiscountRestrictionProperty = BaseModel<TitleDetailDevModel>.CreateDependencyProperty("DiscountRestrictionProperty", TYPES.STRING);

	public static readonly DependencyProperty BarCodesProperty = DependencyProperty.Register("BarCodes", typeof(ObservableCollection<BarcodeProduct>), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(new ObservableCollection<BarcodeProduct>())
	{
		AffectsArrange = true
	});

	public static readonly DependencyProperty SellThruNewProperty = DependencyProperty.Register("SellThruNew", typeof(bool), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(false)
	{
		AffectsArrange = true
	});

	public static readonly DependencyProperty ClosedCaptionedProperty = DependencyProperty.Register("ClosedCaptioned", typeof(bool), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(false)
	{
		AffectsArrange = true
	});

	public static readonly DependencyProperty BoxOfficeGrossProperty = DependencyProperty.Register("BoxOfficeGross", typeof(decimal), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty NationalStreetDateProperty = DependencyProperty.Register("NationalStreetDate", typeof(string), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("DevTitle", typeof(string), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ComingSoonDaysProperty = DependencyProperty.Register("ComingSoonDays", typeof(int), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty AddProductWithCustomPriceButtonVisibilityProperty = DependencyProperty.Register("AddProductWithCustomPriceButtonVisibility", typeof(Visibility), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DisplayAddProductWithCustomPriceButtonVisibilityProperty = DependencyProperty.Register("DisplayAddProductWithCustomPriceButtonVisibility", typeof(Visibility), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public bool HasRollup { get; set; }

	public bool IsDvd { get; set; }

	public bool IsBluRay { get; set; }

	public bool IsDigitalTitle { get; set; }

	public bool Is4KUhdTitle { get; set; }

	public long ProductId { get; set; }

	public string DevTitle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleProperty, value);
			});
		}
	}

	public int ComingSoonDays
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(ComingSoonDaysProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ComingSoonDaysProperty, value);
			});
		}
	}

	public string NationalStreetDate
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(NationalStreetDateProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NationalStreetDateProperty, value);
			});
		}
	}

	public decimal BoxOfficeGross
	{
		get
		{
			return base.Dispatcher.Invoke(() => (decimal)GetValue(BoxOfficeGrossProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BoxOfficeGrossProperty, value);
			});
		}
	}

	public bool ClosedCaptioned
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ClosedCaptionedProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ClosedCaptionedProperty, value);
			});
		}
	}

	public bool SellThruNew
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(SellThruNewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SellThruNewProperty, value);
			});
		}
	}

	public int ProductTypeId
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(ProductTypeIdProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ProductTypeIdProperty, value);
			});
		}
	}

	public string MerchandiseDate
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MerchandiseDateProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MerchandiseDateProperty, value);
			});
		}
	}

	public bool SellThru
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(SellThruProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SellThruProperty, value);
			});
		}
	}

	public string SortDate
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SortDateProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SortDateProperty, value);
			});
		}
	}

	public string ReleaseDate
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ReleaseDateProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReleaseDateProperty, value);
			});
		}
	}

	public int RaitingId
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(RaitingIdProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RaitingIdProperty, value);
			});
		}
	}

	public decimal PromoValue
	{
		get
		{
			return base.Dispatcher.Invoke(() => (decimal)GetValue(PromoValueProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoValueProperty, value);
			});
		}
	}

	public decimal ExtraNight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (decimal)GetValue(ExtraNightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ExtraNightProperty, value);
			});
		}
	}

	public decimal ExpirationPrice
	{
		get
		{
			return base.Dispatcher.Invoke(() => (decimal)GetValue(ExpirationPriceProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ExpirationPriceProperty, value);
			});
		}
	}

	public decimal InitialNight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (decimal)GetValue(InitialNightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(InitialNightProperty, value);
			});
		}
	}

	public string PriceSetId
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PriceSetIdProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PriceSetIdProperty, value);
			});
		}
	}

	public decimal Purchase
	{
		get
		{
			return base.Dispatcher.Invoke(() => (decimal)GetValue(PurchaseProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PurchaseProperty, value);
			});
		}
	}

	public decimal NonReturn
	{
		get
		{
			return base.Dispatcher.Invoke(() => (decimal)GetValue(NonReturnProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NonReturnProperty, value);
			});
		}
	}

	public decimal NonReturnDays
	{
		get
		{
			return base.Dispatcher.Invoke(() => (decimal)GetValue(NonReturnDaysProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NonReturnDaysProperty, value);
			});
		}
	}

	public List<long> GenreIds
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<long>)GetValue(GenreIdsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GenreIdsProperty, value);
			});
		}
	}

	public List<string> GenreList
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<string>)GetValue(GenreListProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GenreListProperty, value);
			});
		}
	}

	public bool IsComingSoon
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsComingSoonProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsComingSoonProperty, value);
			});
		}
	}

	public bool InStock
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(InStockProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(InStockProperty, value);
			});
		}
	}

	public bool CanBeSold
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(CanBeSoldProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CanBeSoldProperty, value);
			});
		}
	}

	public string ProductFamilyName
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ProductFamilyNameProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ProductFamilyNameProperty, value);
			});
		}
	}

	public string ProductTypeName
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ProductTypeNameProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ProductTypeNameProperty, value);
			});
		}
	}

	public bool IsAvailable
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAvailableProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAvailableProperty, value);
			});
		}
	}

	public bool ShowOutOfStock
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ShowOutOfStockProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ShowOutOfStockProperty, value);
			});
		}
	}

	public string Type
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TypeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TypeProperty, value);
			});
		}
	}

	public ObservableCollection<BarcodeProduct> BarCodes
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<BarcodeProduct>)GetValue(BarCodesProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BarCodesProperty, value);
			});
		}
	}

	public ImageSource ImgFile
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ImageSource)GetValue(ImgFileProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImgFileProperty, value);
			});
		}
	}

	public long TitleRollupProductGroupId
	{
		get
		{
			return base.Dispatcher.Invoke(() => (long)GetValue(TitleRollupProductGroupIdProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleRollupProductGroupIdProperty, value);
			});
		}
	}

	public Visibility AddProductWithCustomPriceButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddProductWithCustomPriceButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddProductWithCustomPriceButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility DisplayAddProductWithCustomPriceButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DisplayAddProductWithCustomPriceButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayAddProductWithCustomPriceButtonVisibilityProperty, value);
			});
		}
	}

	public string DiscountRestriction
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DiscountRestrictionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DiscountRestrictionProperty, value);
			});
		}
	}

	public TitleProductDetailDevModel()
	{
		GenreList = new List<string>();
	}
}
