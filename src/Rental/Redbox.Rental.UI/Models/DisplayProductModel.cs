using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class DisplayProductModel : BrowseItemModel
{
	public enum AddButtonPositions
	{
		LowerLeftCorner,
		OverhangLowerLeftCorner
	}

	public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DescriptionMaxHeightProperty = DependencyProperty.Register("DescriptionMaxHeight", typeof(int), typeof(DisplayProductModel), new FrameworkPropertyMetadata(144));

	public static readonly DependencyProperty RatingProperty = DependencyProperty.Register("Rating", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RunningTimeProperty = DependencyProperty.Register("RunningTime", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ReleaseYearProperty = DependencyProperty.Register("ReleaseYear", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CCSupportedProperty = DependencyProperty.Register("CCSupported", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GenresProperty = DependencyProperty.Register("Genres", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GenresLabelProperty = DependencyProperty.Register("GenresLabel", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty StarringProperty = DependencyProperty.Register("Starring", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty StarringLabelProperty = DependencyProperty.Register("StarringLabel", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DirectedByProperty = DependencyProperty.Register("DirectedBy", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OneNightReturnTimeProperty = DependencyProperty.Register("OneNightReturnTime", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OneNightReturnTimeWithTextProperty = DependencyProperty.Register("OneNightReturnTimeWithText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OneNightReturnTimeLabelProperty = DependencyProperty.Register("OneNightReturnTimeLabel", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MultiNightReturnTimeProperty = DependencyProperty.Register("MultiNightReturnTime", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MultiNightReturnTimeLabelProperty = DependencyProperty.Register("MultiNightReturnTimeLabel", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MultiNightReturnTimeVisibilityProperty = DependencyProperty.Register("MultiNightReturnTimeVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OneNightPriceProperty = DependencyProperty.Register("OneNightPrice", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OneNightPriceLabelProperty = DependencyProperty.Register("OneNightPriceLabel", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MultiNightPriceProperty = DependencyProperty.Register("MultiNightPrice", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MultiNightPriceLabelProperty = DependencyProperty.Register("MultiightPriceLabel", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MultiNightPriceVisibilityProperty = DependencyProperty.Register("MultiNightPriceVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ServiceFeeTextProperty = DependencyProperty.Register("ServiceFeeText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ServiceFeeTextVisibilityProperty = DependencyProperty.Register("ServiceFeeTextVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DealTextProperty = DependencyProperty.Register("DealText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DealTextVisibilityProperty = DependencyProperty.Register("DealTextVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RatingLabelProperty = DependencyProperty.Register("RatingLabel", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RatingDescriptionProperty = DependencyProperty.Register("RatingDescription", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RatingVisibilityProperty = DependencyProperty.Register("RatingVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RatingBorderThicknessProperty = DependencyProperty.Register("RatingBorderThickness", typeof(Thickness), typeof(DisplayProductModel), new FrameworkPropertyMetadata(new Thickness(0.0, 0.0, 1.0, 0.0)));

	public static readonly DependencyProperty RunningTimeVisibilityProperty = DependencyProperty.Register("RunningTimeVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RunningTimeBorderThicknessProperty = DependencyProperty.Register("RunningTimeBorderThickness", typeof(Thickness), typeof(DisplayProductModel), new FrameworkPropertyMetadata(new Thickness(0.0, 0.0, 1.0, 0.0)));

	public static readonly DependencyProperty ReleaseYearVisibilityProperty = DependencyProperty.Register("ReleaseYearVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ReleaseYearBorderThicknessProperty = DependencyProperty.Register("ReleaseYearBorderThickness", typeof(Thickness), typeof(DisplayProductModel), new FrameworkPropertyMetadata(new Thickness(0.0, 0.0, 1.0, 0.0)));

	public static readonly DependencyProperty BoxArtGridWidthProperty = DependencyProperty.Register("BoxArtGridWidth", typeof(int), typeof(DisplayProductModel), new FrameworkPropertyMetadata(512));

	public static readonly DependencyProperty DetailsGridWidthProperty = DependencyProperty.Register("DetailsGridWidth", typeof(int), typeof(DisplayProductModel), new FrameworkPropertyMetadata(512));

	public static readonly DependencyProperty AdImageProperty = DependencyProperty.Register("AdImage", typeof(BitmapImage), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdImageVisibilityProperty = DependencyProperty.Register("AdImageVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdHeightProperty = DependencyProperty.Register("AdHeight", typeof(int), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MainHeightProperty = DependencyProperty.Register("MainHeight", typeof(int), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty FlagMarginProperty = DependencyProperty.Register("FlagMargin", typeof(Thickness), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyTextProperty = DependencyProperty.Register("BuyText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyPriceTextProperty = DependencyProperty.Register("BuyPriceText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyButtonBuyTextStyleProperty = DependencyProperty.Register("BuyButtonBuyTextStyle", typeof(Style), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyButtonBuyPriceTextStyleProperty = DependencyProperty.Register("BuyButtonBuyPriceTextStyle", typeof(Style), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ShowBuyProperty = DependencyProperty.Register("ShowBuy", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OutOfStockVisibilityProperty = DependencyProperty.Register("OutOfStockVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OutOfStockLine1TextProperty = DependencyProperty.Register("OutOfStockLine1Text", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OutOfStockLine2TextProperty = DependencyProperty.Register("OutOfStockLine2Text", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OutOfStockLine3TextProperty = DependencyProperty.Register("OutOfStockLine3Text", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OutOfStockLine4TextProperty = DependencyProperty.Register("OutOfStockLine4Text", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty NotEmptyCaseVisibilityProperty = DependencyProperty.Register("NotEmptyCaseVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty StarringVisibilityProperty = DependencyProperty.Register("StarringVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ComingSoonVisibilityProperty = DependencyProperty.Register("ComingSoonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddButtonBarVisibilityProperty = DependencyProperty.Register("AddButtonBarVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ComingSoonLabelTextProperty = DependencyProperty.Register("ComingSoonLabelText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CornerContainerVisibilityProperty = DependencyProperty.Register("CornerContainerVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty CornerIncludeButtonVisibilityProperty = DependencyProperty.Register("CornerIncludeButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty CornerExcludeButtonVisibilityProperty = DependencyProperty.Register("CornerExcludeButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty MDVFieldVisibilityProperty = DependencyProperty.Register("MDVFieldVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty MDVNumberOfDiscsTextProperty = DependencyProperty.Register("MDVNumberOfDiscsText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MDVDiscVendTextProperty = DependencyProperty.Register("MDVDiscVendText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddButtonLabelTextProperty = DependencyProperty.Register("AddButtonLabelText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapImage), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ImageBorderColorProperty = DependencyProperty.Register("ImageBorderColor", typeof(Color), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Colors.Gray));

	public static readonly DependencyProperty ImageBorderThicknessProperty = DependencyProperty.Register("ImageBorderThickness", typeof(Thickness), typeof(DisplayProductModel), new FrameworkPropertyMetadata(new Thickness(0.0))
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ImageBorderCornerRadiusProperty = DependencyProperty.Register("ImageBorderCornerRadius", typeof(double), typeof(DisplayProductModel), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ImageOpacityProperty = DependencyProperty.Register("ImageOpacity", typeof(double), typeof(DisplayProductModel), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DisplayImageOpacityProperty = DependencyProperty.Register("DisplayImageOpacity", typeof(double), typeof(DisplayProductModel), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty AddButtonOpacityProperty = DependencyProperty.Register("AddButtonOpacity", typeof(double), typeof(DisplayProductModel), new FrameworkPropertyMetadata(1.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty AddButtonPositionProperty = DependencyProperty.Register("AddButtonPosition", typeof(AddButtonPositions), typeof(DisplayProductModel), new FrameworkPropertyMetadata(AddButtonPositions.LowerLeftCorner));

	public static readonly DependencyProperty AddButtonVisibilityProperty = DependencyProperty.Register("AddButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty AddButtonPlusSignVisibilityProperty = DependencyProperty.Register("AddButtonPlusSignVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty CornerAddButtonVisibilityProperty = DependencyProperty.Register("CornerAddButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ADAMiniCartAddButtonVisibilityProperty = DependencyProperty.Register("ADAMiniCartAddButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ADAMiniCartAddButtonTextProperty = DependencyProperty.Register("ADAMiniCartAddButtonText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty AddButtonTextVisibilityProperty = DependencyProperty.Register("AddButtonTextVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty CornerAddButtonTextProperty = DependencyProperty.Register("CornerAddButtonText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty RightAddButtonVisibilityProperty = DependencyProperty.Register("RightAddButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DisplayCornerAddButtonVisibilityProperty = DependencyProperty.Register("DisplayCornerAddButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty BannerVisibilityProperty = DependencyProperty.Register("BannerVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty BannerBackgroundBrushProperty = DependencyProperty.Register("BannerBackgroundBrush", typeof(Brush), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty BannerTextProperty = DependencyProperty.Register("BannerText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty BannerProperty = DependencyProperty.Register("Banner", typeof(Banner), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty WatchOptionsVisibilityProperty = DependencyProperty.Register("WatchOptionsVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty WatchOptionsAZVisibilityProperty = DependencyProperty.Register("WatchOptionsAZVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockVisibilityProperty = DependencyProperty.Register("DualInStockVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockAZVisibilityProperty = DependencyProperty.Register("DualInStockAZVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty OutOfStockTextAZVisibilityProperty = DependencyProperty.Register("OutOfStockTextAZVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockTextKioskTextProperty = DependencyProperty.Register("DualInStockTextKioskText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockTextAZKioskTextProperty = DependencyProperty.Register("DualInStockTextAZKioskText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockTitleDetailsVisibilityProperty = DependencyProperty.Register("DualInStockTitleDetailsVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockTextTitleDetailsOutOfStockKioskTextProperty = DependencyProperty.Register("DualInStockTextTitleDetailsOutOfStockKioskText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockTextTitleDetailsInStockKioskTextProperty = DependencyProperty.Register("DualInStockTextTitleDetailsInStockKioskText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty FlagVisibilityProperty = DependencyProperty.Register("FlagVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DisplayFlagVisibilityProperty = DependencyProperty.Register("DisplayFlagVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty FlagTextProperty = DependencyProperty.Register("FlagText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty FlagStyleProperty = DependencyProperty.Register("FlagStyle", typeof(Style), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty AddButtonStyleProperty = DependencyProperty.Register("AddButtonStyle", typeof(Style), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty CancelButtonVisibilityProperty = DependencyProperty.Register("CancelButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty MiniCartCancelButtonVisibilityProperty = DependencyProperty.Register("MiniCartCancelButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ImageBackgroundVisibilityProperty = DependencyProperty.Register("ImageBackgroundVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty SeparatorLineStyleProperty = DependencyProperty.Register("SeparatorLineStyle", typeof(Style), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty SeparatorLineVisibilityProperty = DependencyProperty.Register("SeparatorLineVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty TitleStyleProperty = DependencyProperty.Register("TitleStyle", typeof(Style), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ComingSoonButtonVisibilityProperty = DependencyProperty.Register("ComingSoonButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ComingSoonButtonTextProperty = DependencyProperty.Register("ComingSoonButtonText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ComingSoonButtonStyleProperty = DependencyProperty.Register("ComingSoonButtonStyle", typeof(Style), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ComingSoonButtonCheckMarkVisibilityProperty = DependencyProperty.Register("ComingSoonButtonCheckMarkVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty SeeFullDetailsTextProperty = DependencyProperty.Register("SeeFullDetailsText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MoreLikeThisTextProperty = DependencyProperty.Register("MoreLikeThisText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RemoveButtonTextProperty = DependencyProperty.Register("RemoveButtonText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RedboxPlusInfoTextProperty = DependencyProperty.Register("RedboxPlusInfoText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RedboxPlusInfoVisibilityProperty = DependencyProperty.Register("RedboxPlusInfoVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty RedboxPlusInfoADAGridHeightProperty = DependencyProperty.Register("RedboxPlusInfoADAGridHeight", typeof(int), typeof(DisplayProductModel), new FrameworkPropertyMetadata(0));

	public static readonly DependencyProperty RedboxPlusInfoADAVisibilityProperty = DependencyProperty.Register("RedboxPlusInfoADAVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty MoreLikeThisVisibilityProperty = DependencyProperty.Register("MoreLikeThisVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty DisplayProductMoreLikeThis1Property = DependencyProperty.Register("DisplayProductMoreLikeThis1", typeof(DisplayProductMoreLikeThisModel), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DisplayProductMoreLikeThis2Property = DependencyProperty.Register("DisplayProductMoreLikeThis2", typeof(DisplayProductMoreLikeThisModel), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DisplayProductMoreLikeThis3Property = DependencyProperty.Register("DisplayProductMoreLikeThis3", typeof(DisplayProductMoreLikeThisModel), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DisplayProductMoreLikeThis4Property = DependencyProperty.Register("DisplayProductMoreLikeThis4", typeof(DisplayProductMoreLikeThisModel), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DetailsButtonTextProperty = DependencyProperty.Register("DetailsButtonText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public bool TitleDetailPropertiesInitialized;

	public ObservableCollection<TilteDetailRentalFormatButtonData> ButtonData { get; set; }

	public Visibility SeparatorLineVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SeparatorLineVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SeparatorLineVisibilityProperty, value);
			});
		}
	}

	public bool LockAspectRatio { get; set; }

	public bool IsTitleDetailsFlag { get; set; }

	public BitmapImage Image
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(ImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageProperty, value);
			});
		}
	}

	public Color ImageBorderColor
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Color)GetValue(ImageBorderColorProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageBorderColorProperty, value);
			});
		}
	}

	public Thickness ImageBorderThickness
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(ImageBorderThicknessProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageBorderThicknessProperty, value);
			});
		}
	}

	public double ImageBorderCornerRadius
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(ImageBorderCornerRadiusProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageBorderCornerRadiusProperty, value);
			});
		}
	}

	public double AddButtonOpacity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(AddButtonOpacityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonOpacityProperty, value);
			});
		}
	}

	public string AddButtonLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AddButtonLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonLabelTextProperty, value);
			});
		}
	}

	public AddButtonPositions AddButtonPosition
	{
		get
		{
			return base.Dispatcher.Invoke(() => (AddButtonPositions)GetValue(AddButtonPositionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonPositionProperty, value);
			});
		}
	}

	public Visibility AddButtonPlusSignVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddButtonPlusSignVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonPlusSignVisibilityProperty, value);
			});
		}
	}

	public Visibility ComingSoonButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ComingSoonButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ComingSoonButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility ComingSoonButtonCheckMarkVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ComingSoonButtonCheckMarkVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ComingSoonButtonCheckMarkVisibilityProperty, value);
			});
		}
	}

	public Visibility MDVFieldVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MDVFieldVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MDVFieldVisibilityProperty, value);
			});
		}
	}

	public string MDVNumberOfDiscsText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MDVNumberOfDiscsTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MDVNumberOfDiscsTextProperty, value);
			});
		}
	}

	public string MDVDiscVendText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MDVDiscVendTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MDVDiscVendTextProperty, value);
			});
		}
	}

	public string ComingSoonButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ComingSoonButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ComingSoonButtonTextProperty, value);
			});
		}
	}

	public Style ComingSoonButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(ComingSoonButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ComingSoonButtonStyleProperty, value);
			});
		}
	}

	public double DisplayImageOpacity => base.Dispatcher.Invoke(() => (double)GetValue(DisplayImageOpacityProperty));

	public double ImageOpacity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(ImageOpacityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageOpacityProperty, value);
			});
			UpdateDisplayImageOpacity();
		}
	}

	public Visibility AddButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (!base.IsBorderItem) ? ((Visibility)GetValue(AddButtonVisibilityProperty)) : Visibility.Collapsed);
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility DisplayCornerAddButtonVisibility => base.Dispatcher.Invoke(() => (!base.IsBorderItem) ? ((Visibility)GetValue(DisplayCornerAddButtonVisibilityProperty)) : Visibility.Collapsed);

	public Visibility CornerAddButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CornerAddButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CornerAddButtonVisibilityProperty, value);
				UpdateDisplayCornerAddButtonVisibility();
			});
		}
	}

	public Visibility ADAMiniCartAddButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ADAMiniCartAddButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ADAMiniCartAddButtonVisibilityProperty, value);
			});
		}
	}

	public string ADAMiniCartAddButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ADAMiniCartAddButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ADAMiniCartAddButtonTextProperty, value);
			});
		}
	}

	public string CornerAddButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CornerAddButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CornerAddButtonTextProperty, value);
			});
		}
	}

	public Visibility AddButtonTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddButtonTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonTextVisibilityProperty, value);
			});
		}
	}

	public Visibility RightAddButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (!base.IsBorderItem) ? ((Visibility)GetValue(RightAddButtonVisibilityProperty)) : Visibility.Collapsed);
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RightAddButtonVisibilityProperty, value);
			});
		}
	}

	public Brush BackgroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(BackgroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackgroundBrushProperty, value);
			});
		}
	}

	public Visibility BannerVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BannerVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerVisibilityProperty, value);
			});
		}
	}

	public Brush BannerBackgroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(BannerBackgroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerBackgroundBrushProperty, value);
			});
		}
	}

	public string BannerText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BannerTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerTextProperty, value);
			});
		}
	}

	public Banner Banner
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Banner)GetValue(BannerProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerProperty, value);
			});
		}
	}

	public Visibility WatchOptionsVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(WatchOptionsVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(WatchOptionsVisibilityProperty, value);
			});
		}
	}

	public Visibility WatchOptionsAZVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(WatchOptionsAZVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(WatchOptionsAZVisibilityProperty, value);
			});
		}
	}

	public Visibility DualInStockVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DualInStockVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockVisibilityProperty, value);
			});
		}
	}

	public Visibility DualInStockAZVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DualInStockAZVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockAZVisibilityProperty, value);
			});
		}
	}

	public Visibility OutOfStockTextAZVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(OutOfStockTextAZVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OutOfStockTextAZVisibilityProperty, value);
			});
		}
	}

	public string DualInStockTextKioskText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DualInStockTextKioskTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockTextKioskTextProperty, value);
			});
		}
	}

	public string DualInStockTextAZKioskText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DualInStockTextAZKioskTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockTextAZKioskTextProperty, value);
			});
		}
	}

	public Visibility DualInStockTitleDetailsVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DualInStockTitleDetailsVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockTitleDetailsVisibilityProperty, value);
			});
		}
	}

	public string DualInStockTextTitleDetailsOutOfStockKioskText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DualInStockTextTitleDetailsOutOfStockKioskTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockTextTitleDetailsOutOfStockKioskTextProperty, value);
			});
		}
	}

	public string DualInStockTextTitleDetailsInStockKioskText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DualInStockTextTitleDetailsInStockKioskTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockTextTitleDetailsInStockKioskTextProperty, value);
			});
		}
	}

	public Visibility CancelButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CancelButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CancelButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility MiniCartCancelButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MiniCartCancelButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MiniCartCancelButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility ImageBackgroundVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ImageBackgroundVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageBackgroundVisibilityProperty, value);
			});
		}
	}

	public Visibility DisplayFlagVisibility => base.Dispatcher.Invoke(() => (Visibility)GetValue(DisplayFlagVisibilityProperty));

	public Visibility FlagVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(FlagVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FlagVisibilityProperty, value);
			});
			UpdateDisplayFlagVisibility();
		}
	}

	public string FlagText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(FlagTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FlagTextProperty, value);
			});
		}
	}

	public Style FlagStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(FlagStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FlagStyleProperty, value);
			});
		}
	}

	public Style AddButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(AddButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonStyleProperty, value);
			});
		}
	}

	public Style SeparatorLineStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(SeparatorLineStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SeparatorLineStyleProperty, value);
			});
		}
	}

	public Style TitleStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(TitleStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleStyleProperty, value);
			});
		}
	}

	public string Header
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(HeaderProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HeaderProperty, value);
			});
		}
	}

	public string Description
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DescriptionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DescriptionProperty, value);
			});
		}
	}

	public int DescriptionMaxHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(DescriptionMaxHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DescriptionMaxHeightProperty, value);
			});
		}
	}

	public string Rating
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(RatingProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RatingProperty, value);
			});
		}
	}

	public string Format
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(FormatProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FormatProperty, value);
			});
		}
	}

	public string RunningTime
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(RunningTimeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RunningTimeProperty, value);
			});
		}
	}

	public string ReleaseYear
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ReleaseYearProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReleaseYearProperty, value);
			});
		}
	}

	public Visibility CCSupported
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CCSupportedProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CCSupportedProperty, value);
			});
		}
	}

	public string Starring
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(StarringProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StarringProperty, value);
			});
		}
	}

	public string StarringLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(StarringLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StarringLabelProperty, value);
			});
		}
	}

	public string Genres
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(GenresProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GenresProperty, value);
			});
		}
	}

	public string GenresLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(GenresLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GenresLabelProperty, value);
			});
		}
	}

	public string DirectedBy
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DirectedByProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DirectedByProperty, value);
			});
		}
	}

	public string OneNightReturnTime
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OneNightReturnTimeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OneNightReturnTimeProperty, value);
			});
		}
	}

	public string OneNightReturnTimeWithText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OneNightReturnTimeWithTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OneNightReturnTimeWithTextProperty, value);
			});
		}
	}

	public string OneNightReturnTimeLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OneNightReturnTimeLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OneNightReturnTimeLabelProperty, value);
			});
		}
	}

	public string MultiNightReturnTime
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MultiNightReturnTimeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MultiNightReturnTimeProperty, value);
			});
		}
	}

	public string MultiNightReturnTimeLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MultiNightReturnTimeLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MultiNightReturnTimeLabelProperty, value);
			});
		}
	}

	public Visibility MultiNightReturnTimeVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MultiNightReturnTimeVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MultiNightReturnTimeVisibilityProperty, value);
			});
		}
	}

	public string OneNightPrice
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OneNightPriceProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OneNightPriceProperty, value);
			});
		}
	}

	public string OneNightPriceLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OneNightPriceLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OneNightPriceLabelProperty, value);
			});
		}
	}

	public string MultiNightPrice
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MultiNightPriceProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MultiNightPriceProperty, value);
			});
		}
	}

	public string MultiNightPriceLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MultiNightPriceLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MultiNightPriceLabelProperty, value);
			});
		}
	}

	public Visibility MultiNightPriceVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MultiNightPriceVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MultiNightPriceVisibilityProperty, value);
			});
		}
	}

	public string ServiceFeeText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ServiceFeeTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ServiceFeeTextProperty, value);
			});
		}
	}

	public Visibility ServiceFeeTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ServiceFeeTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ServiceFeeTextVisibilityProperty, value);
			});
		}
	}

	public string DealText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DealTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DealTextProperty, value);
			});
		}
	}

	public Visibility DealTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DealTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DealTextVisibilityProperty, value);
			});
		}
	}

	public string RatingLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(RatingLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RatingLabelProperty, value);
			});
		}
	}

	public string RatingDescription
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(RatingDescriptionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RatingDescriptionProperty, value);
			});
		}
	}

	public Visibility RatingVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RatingVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RatingVisibilityProperty, value);
			});
		}
	}

	public Thickness RatingBorderThickness
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(RatingBorderThicknessProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RatingBorderThicknessProperty, value);
			});
		}
	}

	public Visibility RunningTimeVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RunningTimeVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RunningTimeVisibilityProperty, value);
			});
		}
	}

	public Thickness RunningTimeBorderThickness
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(RunningTimeBorderThicknessProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RunningTimeBorderThicknessProperty, value);
			});
		}
	}

	public Visibility ReleaseYearVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ReleaseYearVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReleaseYearVisibilityProperty, value);
			});
		}
	}

	public Thickness ReleaseYearBorderThickness
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(ReleaseYearBorderThicknessProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReleaseYearBorderThicknessProperty, value);
			});
		}
	}

	public int BoxArtGridWidth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(BoxArtGridWidthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BoxArtGridWidthProperty, value);
			});
		}
	}

	public int DetailsGridWidth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(DetailsGridWidthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DetailsGridWidthProperty, value);
			});
		}
	}

	public BitmapImage AdImage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(AdImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdImageProperty, value);
			});
		}
	}

	public Visibility AdImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AdImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdImageVisibilityProperty, value);
			});
		}
	}

	public int AdHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(AdHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdHeightProperty, value);
			});
		}
	}

	public int MainHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(MainHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MainHeightProperty, value);
			});
		}
	}

	public Thickness FlagMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(FlagMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FlagMarginProperty, value);
			});
		}
	}

	public Visibility ShowBuy
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ShowBuyProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ShowBuyProperty, value);
			});
		}
	}

	public Visibility OutOfStockVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(OutOfStockVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OutOfStockVisibilityProperty, value);
			});
		}
	}

	public string OutOfStockLine1Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OutOfStockLine1TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OutOfStockLine1TextProperty, value);
			});
		}
	}

	public string OutOfStockLine2Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OutOfStockLine2TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OutOfStockLine2TextProperty, value);
			});
		}
	}

	public string OutOfStockLine3Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OutOfStockLine3TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OutOfStockLine3TextProperty, value);
			});
		}
	}

	public string OutOfStockLine4Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OutOfStockLine4TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OutOfStockLine4TextProperty, value);
			});
		}
	}

	public string BuyText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BuyTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyTextProperty, value);
			});
		}
	}

	public string BuyPriceText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BuyPriceTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyPriceTextProperty, value);
			});
		}
	}

	public Style BuyButtonBuyTextStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(BuyButtonBuyTextStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyButtonBuyTextStyleProperty, value);
			});
		}
	}

	public Style BuyButtonBuyPriceTextStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(BuyButtonBuyPriceTextStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyButtonBuyPriceTextStyleProperty, value);
			});
		}
	}

	public Visibility NotEmptyCaseVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(NotEmptyCaseVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NotEmptyCaseVisibilityProperty, value);
			});
		}
	}

	public Visibility StarringVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(StarringVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StarringVisibilityProperty, value);
			});
		}
	}

	public Visibility ComingSoonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ComingSoonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ComingSoonVisibilityProperty, value);
			});
		}
	}

	public Visibility AddButtonBarVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddButtonBarVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonBarVisibilityProperty, value);
			});
		}
	}

	public string ComingSoonLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ComingSoonLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ComingSoonLabelTextProperty, value);
			});
		}
	}

	public Visibility CornerContainerVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CornerContainerVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CornerContainerVisibilityProperty, value);
			});
		}
	}

	public Visibility CornerIncludeButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CornerIncludeButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CornerIncludeButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility CornerExcludeButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CornerExcludeButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CornerExcludeButtonVisibilityProperty, value);
			});
		}
	}

	public string RemoveButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(RemoveButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RemoveButtonTextProperty, value);
			});
		}
	}

	public string RedboxPlusInfoText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(RedboxPlusInfoTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RedboxPlusInfoTextProperty, value);
			});
		}
	}

	public string SeeFullDetailsText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SeeFullDetailsTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SeeFullDetailsTextProperty, value);
			});
		}
	}

	public string MoreLikeThisText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MoreLikeThisTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MoreLikeThisTextProperty, value);
			});
		}
	}

	public Visibility RedboxPlusInfoVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusInfoVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RedboxPlusInfoVisibilityProperty, value);
			});
		}
	}

	public int RedboxPlusInfoADAGridHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(RedboxPlusInfoADAGridHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RedboxPlusInfoADAGridHeightProperty, value);
			});
		}
	}

	public Visibility RedboxPlusInfoADAVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusInfoADAVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RedboxPlusInfoADAVisibilityProperty, value);
			});
		}
	}

	public Visibility MoreLikeThisVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MoreLikeThisVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MoreLikeThisVisibilityProperty, value);
			});
		}
	}

	public DisplayProductMoreLikeThisModel DisplayProductMoreLikeThis1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DisplayProductMoreLikeThisModel)GetValue(DisplayProductMoreLikeThis1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayProductMoreLikeThis1Property, value);
			});
		}
	}

	public DisplayProductMoreLikeThisModel DisplayProductMoreLikeThis2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DisplayProductMoreLikeThisModel)GetValue(DisplayProductMoreLikeThis2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayProductMoreLikeThis2Property, value);
			});
		}
	}

	public DisplayProductMoreLikeThisModel DisplayProductMoreLikeThis3
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DisplayProductMoreLikeThisModel)GetValue(DisplayProductMoreLikeThis3Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayProductMoreLikeThis3Property, value);
			});
		}
	}

	public DisplayProductMoreLikeThisModel DisplayProductMoreLikeThis4
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DisplayProductMoreLikeThisModel)GetValue(DisplayProductMoreLikeThis4Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayProductMoreLikeThis4Property, value);
			});
		}
	}

	public string DetailsButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DetailsButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DetailsButtonTextProperty, value);
			});
		}
	}

	public DisplayProductModelConstants DisplayProductModelConstants { get; set; }

	public IAdImpression AdImpression { get; set; }

	public DisplayProductModel()
	{
		ButtonData = new ObservableCollection<TilteDetailRentalFormatButtonData>();
	}

	protected override void SetVisibleItemIndex(int value)
	{
		base.SetVisibleItemIndex(value);
		string cornerAddButtonText = (ADAMiniCartAddButtonText = (value + 1).ToString());
		CornerAddButtonText = cornerAddButtonText;
	}

	protected override void SetIsBorderItem(bool value)
	{
		base.SetIsBorderItem(value);
		UpdateDisplayCornerAddButtonVisibility();
		UpdateDisplayImageOpacity();
		UpdateDisplayFlagVisibility();
	}

	protected override void SetIsBottomRow(bool value)
	{
		base.SetIsBottomRow(value);
		SeparatorLineVisibility = (base.IsBottomRow ? Visibility.Collapsed : Visibility.Visible);
	}

	private void UpdateDisplayImageOpacity()
	{
		base.Dispatcher.Invoke(delegate
		{
			SetValue(DisplayImageOpacityProperty, base.IsBorderItem ? 0.8 : ImageOpacity);
		});
	}

	private void UpdateDisplayCornerAddButtonVisibility()
	{
		base.Dispatcher.Invoke(delegate
		{
			SetValue(DisplayCornerAddButtonVisibilityProperty, base.IsBorderItem ? Visibility.Collapsed : CornerAddButtonVisibility);
		});
	}

	private void UpdateDisplayFlagVisibility()
	{
		base.Dispatcher.Invoke(delegate
		{
			SetValue(DisplayFlagVisibilityProperty, base.IsBorderItem ? Visibility.Collapsed : FlagVisibility);
		});
	}

	public void ConfigureForCarousel()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(2.0);
		ImageBorderCornerRadius = 5.0;
		ImageOpacity = 1.0;
		ImageBackgroundVisibility = Visibility.Collapsed;
		AddButtonOpacity = 1.0;
		AddButtonVisibility = Visibility.Collapsed;
		FlagVisibility = Visibility.Collapsed;
		AddButtonPosition = AddButtonPositions.OverhangLowerLeftCorner;
		CornerAddButtonVisibility = Visibility.Collapsed;
		CancelButtonVisibility = Visibility.Collapsed;
	}

	public void ConfigureForBrowse()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(1.0);
		ImageBorderCornerRadius = 0.0;
		ImageBackgroundVisibility = Visibility.Collapsed;
		AddButtonOpacity = 1.0;
		AddButtonPosition = AddButtonPositions.LowerLeftCorner;
		CancelButtonVisibility = Visibility.Collapsed;
		AddButtonVisibility = Visibility.Collapsed;
		BackgroundBrush = new SolidColorBrush(Colors.White);
	}

	public void ConfigureForAToZBrowse()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(1.0);
		ImageBorderCornerRadius = 0.0;
		ImageBackgroundVisibility = Visibility.Collapsed;
		AddButtonOpacity = 1.0;
		AddButtonPosition = AddButtonPositions.LowerLeftCorner;
		CancelButtonVisibility = Visibility.Collapsed;
		AddButtonVisibility = Visibility.Collapsed;
		CornerAddButtonVisibility = Visibility.Collapsed;
		FlagVisibility = Visibility.Collapsed;
	}

	public void ConfigureForMiniCart()
	{
		ImageBorderColor = Color.FromRgb(162, 16, 24);
		ImageBorderThickness = new Thickness(3.0);
		ImageBorderCornerRadius = 0.0;
		ImageOpacity = 1.0;
		ImageBackgroundVisibility = Visibility.Visible;
		AddButtonOpacity = 1.0;
		AddButtonVisibility = Visibility.Collapsed;
		FlagVisibility = Visibility.Hidden;
		AddButtonPosition = AddButtonPositions.LowerLeftCorner;
		CornerAddButtonVisibility = Visibility.Collapsed;
		AddButtonPlusSignVisibility = Visibility.Collapsed;
		ADAMiniCartAddButtonVisibility = Visibility.Collapsed;
	}

	public void ConfigureForRecommendationOnPickupCart()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(1.0);
		ImageBorderCornerRadius = 0.0;
		ImageOpacity = 1.0;
		ImageBackgroundVisibility = Visibility.Visible;
		AddButtonOpacity = 1.0;
		AddButtonVisibility = Visibility.Collapsed;
		FlagVisibility = Visibility.Hidden;
		AddButtonPosition = AddButtonPositions.LowerLeftCorner;
		CornerAddButtonVisibility = Visibility.Collapsed;
		AddButtonPlusSignVisibility = Visibility.Collapsed;
		ADAMiniCartAddButtonVisibility = Visibility.Collapsed;
	}

	public void ConfigureForRecommendedTitlesPopup()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(1.0);
		ImageBorderCornerRadius = 0.0;
		ImageOpacity = 1.0;
		LockAspectRatio = true;
		AddButtonPlusSignVisibility = Visibility.Visible;
		CornerAddButtonVisibility = Visibility.Visible;
		AddButtonPosition = AddButtonPositions.LowerLeftCorner;
		AddButtonOpacity = 1.0;
		base.CanAdd = true;
		BannerVisibility = Visibility.Collapsed;
		FlagVisibility = Visibility.Visible;
		ShowBuy = Visibility.Collapsed;
		AddButtonBarVisibility = Visibility.Visible;
		OutOfStockVisibility = Visibility.Collapsed;
		DualInStockTitleDetailsVisibility = Visibility.Collapsed;
		ComingSoonVisibility = Visibility.Collapsed;
	}

	public void ConfigureForUpsell()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(1.0);
		ImageBorderCornerRadius = 0.0;
		ImageOpacity = 1.0;
		ImageBackgroundVisibility = Visibility.Visible;
		AddButtonOpacity = 1.0;
		AddButtonVisibility = Visibility.Collapsed;
		FlagVisibility = Visibility.Hidden;
		AddButtonPosition = AddButtonPositions.LowerLeftCorner;
		CornerAddButtonVisibility = Visibility.Collapsed;
		AddButtonPlusSignVisibility = Visibility.Collapsed;
		ADAMiniCartAddButtonVisibility = Visibility.Collapsed;
	}

	public void ConfigureForUpdateADACart()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(1.0);
		ImageBorderCornerRadius = 0.0;
		ImageBackgroundVisibility = Visibility.Collapsed;
		CancelButtonVisibility = Visibility.Visible;
		AddButtonVisibility = Visibility.Collapsed;
		ImageOpacity = 1.0;
	}

	public void ConfigureForADAMiniCart()
	{
		ImageBackgroundVisibility = Visibility.Collapsed;
		AddButtonOpacity = 1.0;
		AddButtonPosition = AddButtonPositions.LowerLeftCorner;
		CancelButtonVisibility = Visibility.Collapsed;
		AddButtonVisibility = Visibility.Collapsed;
		ImageOpacity = 0.0;
	}

	public void ConfigureForTitleDetail()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(0.0);
		ImageBorderCornerRadius = 0.0;
		ImageOpacity = 1.0;
		ImageBackgroundVisibility = Visibility.Collapsed;
		AddButtonOpacity = 1.0;
		AddButtonVisibility = Visibility.Collapsed;
		FlagVisibility = Visibility.Visible;
		base.IsBorderItem = false;
		AddButtonPosition = AddButtonPositions.OverhangLowerLeftCorner;
		CornerAddButtonVisibility = Visibility.Collapsed;
		CancelButtonVisibility = Visibility.Collapsed;
		ComingSoonButtonCheckMarkVisibility = Visibility.Collapsed;
		ComingSoonButtonVisibility = Visibility.Collapsed;
		BannerVisibility = Visibility.Collapsed;
		WatchOptionsVisibility = Visibility.Collapsed;
		DualInStockVisibility = Visibility.Collapsed;
		AddButtonTextVisibility = Visibility.Collapsed;
		IsTitleDetailsFlag = true;
	}
}
