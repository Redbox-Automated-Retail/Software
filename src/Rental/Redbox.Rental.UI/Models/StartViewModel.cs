using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.UI.Models;

public class StartViewModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty CarouselModelProperty = DependencyProperty.Register("CarouselModel", typeof(CarouselModel), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CarouselTestModelProperty = DependencyProperty.Register("CarouselTestModel", typeof(CarouselTestModel), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BannerImageProperty = DependencyProperty.Register("BannerImage", typeof(BitmapImage), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ExtraLargeButtonTextStyleProperty = DependencyProperty.Register("ExtraLargeButtonTextStyle", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine1Property = DependencyProperty.Register("AllMoviesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine2Property = DependencyProperty.Register("AllMoviesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine1StyleProperty = DependencyProperty.Register("AllMoviesButtonTextLine1Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine2StyleProperty = DependencyProperty.Register("AllMoviesButtonTextLine2Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PickUpButtonTextProperty = DependencyProperty.Register("PickUpButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ReturnButtonTextProperty = DependencyProperty.Register("ReturnButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ToggleLanguageButtonTextProperty = DependencyProperty.Register("ToggleLanguageButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ToggleSignInButtonTextProperty = DependencyProperty.Register("ToggleSignInButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BurnInVisibilityProperty = DependencyProperty.Register("BurnInVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BurnInViewLabelTextProperty = DependencyProperty.Register("BurnInViewLabelText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaTurnedOnLabelTextProperty = DependencyProperty.Register("AdaTurnedOnLabelText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaTurnedOnLabelVisibilityProperty = DependencyProperty.Register("AdaTurnedOnLabelVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AdaButtonStyleProperty = DependencyProperty.Register("AdaButtonStyle", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaButtonImageSourceProperty = DependencyProperty.Register("AdaButtonImageSource", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaButtonCheckmarkVisibilityProperty = DependencyProperty.Register("AdaButtonCheckmarkVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty CarouselGridRowHeightProperty = DependencyProperty.Register("CarouselGridRowHeight", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(438));

	public static readonly DependencyProperty BannerVisibilityProperty = DependencyProperty.Register("BannerVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty LoadingTitlesImageVisibilityProperty = DependencyProperty.Register("LoadingTitlesImageVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty SignInVisibilityProperty = DependencyProperty.Register("SignInVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AdaSignInVisibilityProperty = DependencyProperty.Register("AdaSignInVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty SignInButtonEnabledProperty = DependencyProperty.Register("SignInButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine1Property = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine1StyleProperty = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine1Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine2Property = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonVisibilityProperty = DependencyProperty.Register("BuyMoviesOrGamesButtonVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BuyMoviesEnabledProperty = DependencyProperty.Register("BuyMoviesButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty ReturnButtonEnabledProperty = DependencyProperty.Register("ReturnButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(true));

	public static readonly DependencyProperty UseClassicStartViewProperty = DependencyProperty.Register("UseClassicStartView", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UseTestStartViewProperty = DependencyProperty.Register("UseTestStartView", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UseRentAndBuyTestStartViewProperty = DependencyProperty.Register("UseRentAndBuyTestStartView", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UseScreenSaverStartViewProperty = DependencyProperty.Register("UseScreenSaverStartView", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UseTestScreenSaverProperty = DependencyProperty.Register("UseTestScreenSaver", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty IsReturnOnlyModeProperty = DependencyProperty.Register("IsReturnOnlyMode", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty SecondaryButtonsColumnProperty = DependencyProperty.Register("SecondaryButtonsColumn", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(2));

	public static readonly DependencyProperty SecondaryButtonsMarginProperty = DependencyProperty.Register("SecondaryButtonsMargin", typeof(Thickness), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RentAndBuyMoviesButtonTextVisibilityProperty = DependencyProperty.Register("RentAndBuyMoviesButtonTextVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty RentMoviesGamesTextVisibilityProperty = DependencyProperty.Register("RentMoviesGamesTextVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty RentAndBuyMoviesGamesTextVisibilityProperty = DependencyProperty.Register("RentAndBuyMoviesGamesTextVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty SignInButtonTooltipVisibilityProperty = DependencyProperty.Register("SignInButtonTooltipVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty NonBurnInBackgroundBrushProperty = DependencyProperty.Register("NonBurnInBackgroundBrush", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(Brushes.White));

	public static readonly DependencyProperty AdaTurnedOnLabelBackgroundBrushProperty = DependencyProperty.Register("AdaTurnedOnLabelBackgroundBrush", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(Brushes.White));

	public static readonly DependencyProperty AdaTurnedOnLabelForegroundBrushProperty = DependencyProperty.Register("AdaTurnedOnLabelForegroundBrush", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(72, 27, 84))));

	public static readonly DependencyProperty HandicapButtonStyleProperty = DependencyProperty.Register("HandicapButtonStyle", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty HandicapButtonIconOffForegroundBrushProperty = DependencyProperty.Register("HandicapButtonIconOffForegroundBrush", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(121, 45, 141))));

	public static readonly DependencyProperty HandicapButtonIconOnForegroundBrushProperty = DependencyProperty.Register("HandicapButtonIconOnForegroundBrush", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(121, 45, 141))));

	public static readonly DependencyProperty EllipseFillBrushProperty = DependencyProperty.Register("EllipseFillBrush", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(121, 45, 141))));

	public static readonly DependencyProperty TextBlockForegroundBrushProperty = DependencyProperty.Register("TextBlockForegroundBrush", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(Brushes.White));

	public static readonly DependencyProperty KioskClosingMessageVisibilityProperty = DependencyProperty.Register("KioskClosingMessageVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty KioskClosingMessageForegroundProperty = DependencyProperty.Register("KioskClosingMessageForeground", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(72, 27, 84))));

	public static readonly DependencyProperty KioskClosingMessageText1Property = DependencyProperty.Register("KioskClosingMessageText1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty KioskClosingMessageText2Property = DependencyProperty.Register("KioskClosingMessageText2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty EspanolButtonStyleProperty = DependencyProperty.Register("EspanolButtonStyle", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty HelpButtonStyleProperty = DependencyProperty.Register("HelpButtonStyle", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MainGridBackgroundProperty = DependencyProperty.Register("MainGridBackground", typeof(Brush), typeof(StartViewModel), new FrameworkPropertyMetadata(Brushes.White));

	public static readonly DependencyProperty ExtraLargePrimaryButtonStyleProperty = DependencyProperty.Register("ExtraLargePrimaryButtonStyle", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonImageSourceProperty = DependencyProperty.Register("AllMoviesButtonImageSource", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ExtraLargeSecondaryButtonStyleProperty = DependencyProperty.Register("ExtraLargeSecondaryButtonStyle", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MediumSecondaryButtonStyleProperty = DependencyProperty.Register("MediumSecondaryButtonStyle", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty IsBurnInViewProperty = DependencyProperty.Register("IsBurnInView", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty PressToStartButtonImageSourceProperty = DependencyProperty.Register("PressToStartButtonImageSource", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public bool BurnInModeEnabled { get; set; }

	public bool UseClassicStartView
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UseClassicStartViewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UseClassicStartViewProperty, value);
			});
		}
	}

	public bool UseTestStartView
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UseTestStartViewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UseTestStartViewProperty, value);
			});
		}
	}

	public bool UseRentAndBuyTestStartView
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UseRentAndBuyTestStartViewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UseRentAndBuyTestStartViewProperty, value);
			});
		}
	}

	public bool UseScreenSaverStartView
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UseScreenSaverStartViewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UseScreenSaverStartViewProperty, value);
			});
		}
	}

	public bool UseTestScreenSaver
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UseTestScreenSaverProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UseTestScreenSaverProperty, value);
			});
		}
	}

	public bool IsReturnOnlyMode
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsReturnOnlyModeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsReturnOnlyModeProperty, value);
			});
		}
	}

	public Style ExtraLargeButtonTextStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(ExtraLargeButtonTextStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ExtraLargeButtonTextStyleProperty, value);
			});
		}
	}

	public string AllMoviesButtonTextLine1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AllMoviesButtonTextLine1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonTextLine1Property, value);
			});
		}
	}

	public string AllMoviesButtonTextLine2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AllMoviesButtonTextLine2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonTextLine2Property, value);
			});
		}
	}

	public Style AllMoviesButtonTextLine1Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(AllMoviesButtonTextLine1StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonTextLine1StyleProperty, value);
			});
		}
	}

	public Style AllMoviesButtonTextLine2Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(AllMoviesButtonTextLine2StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonTextLine2StyleProperty, value);
			});
		}
	}

	public string BuyMoviesOrGamesButtonTextLine1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BuyMoviesOrGamesButtonTextLine1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonTextLine1Property, value);
			});
		}
	}

	public Style BuyMoviesOrGamesButtonTextLine1Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(BuyMoviesOrGamesButtonTextLine1StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonTextLine1StyleProperty, value);
			});
		}
	}

	public string BuyMoviesOrGamesButtonTextLine2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BuyMoviesOrGamesButtonTextLine2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonTextLine2Property, value);
			});
		}
	}

	public Visibility BuyMoviesOrGamesButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BuyMoviesOrGamesButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonVisibilityProperty, value);
			});
		}
	}

	public bool BuyMoviesEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(BuyMoviesEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesEnabledProperty, value);
			});
		}
	}

	public bool SignInButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(SignInButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInButtonEnabledProperty, value);
			});
		}
	}

	public string PickUpButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PickUpButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PickUpButtonTextProperty, value);
			});
		}
	}

	public string ReturnButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ReturnButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReturnButtonTextProperty, value);
			});
		}
	}

	public int SecondaryButtonsColumn
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(SecondaryButtonsColumnProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SecondaryButtonsColumnProperty, value);
			});
		}
	}

	public Thickness SecondaryButtonsMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(SecondaryButtonsMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SecondaryButtonsMarginProperty, value);
			});
		}
	}

	public Visibility RentAndBuyMoviesButtonTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RentAndBuyMoviesButtonTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RentAndBuyMoviesButtonTextVisibilityProperty, value);
			});
		}
	}

	public Visibility RentMoviesGamesTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RentMoviesGamesTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RentMoviesGamesTextVisibilityProperty, value);
			});
		}
	}

	public Visibility RentAndBuyMoviesGamesTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RentAndBuyMoviesGamesTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RentAndBuyMoviesGamesTextVisibilityProperty, value);
			});
		}
	}

	public string ToggleLanguageButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ToggleLanguageButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ToggleLanguageButtonTextProperty, value);
			});
		}
	}

	public string ToggleSignInButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ToggleSignInButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ToggleSignInButtonTextProperty, value);
			});
		}
	}

	public string BurnInViewLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BurnInViewLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BurnInViewLabelTextProperty, value);
			});
		}
	}

	public Visibility BurnInVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BurnInVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BurnInVisibilityProperty, value);
			});
		}
	}

	public Visibility AdaTurnedOnLabelVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AdaTurnedOnLabelVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaTurnedOnLabelVisibilityProperty, value);
			});
		}
	}

	public string AdaTurnedOnLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AdaTurnedOnLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaTurnedOnLabelTextProperty, value);
			});
		}
	}

	public Style AdaButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(AdaButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaButtonStyleProperty, value);
			});
		}
	}

	public string AdaButtonImageSource
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AdaButtonImageSourceProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaButtonImageSourceProperty, value);
			});
		}
	}

	public Visibility AdaButtonCheckmarkVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AdaButtonCheckmarkVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaButtonCheckmarkVisibilityProperty, value);
			});
		}
	}

	public int CarouselGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CarouselGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CarouselGridRowHeightProperty, value);
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

	public BitmapImage BannerImage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(BannerImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerImageProperty, value);
			});
		}
	}

	public Visibility LoadingTitlesImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(LoadingTitlesImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(LoadingTitlesImageVisibilityProperty, value);
			});
		}
	}

	public Visibility SignInVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SignInVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInVisibilityProperty, value);
			});
		}
	}

	public Visibility AdaSignInVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AdaSignInVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaSignInVisibilityProperty, value);
			});
		}
	}

	public CarouselModel CarouselModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (CarouselModel)GetValue(CarouselModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CarouselModelProperty, value);
			});
		}
	}

	public CarouselTestModel CarouselTestModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (CarouselTestModel)GetValue(CarouselTestModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CarouselTestModelProperty, value);
			});
		}
	}

	public bool ReturnButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ReturnButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReturnButtonEnabledProperty, value);
			});
		}
	}

	public Visibility SignInButtonTooltipVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SignInButtonTooltipVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInButtonTooltipVisibilityProperty, value);
			});
		}
	}

	public Brush NonBurnInBackgroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(NonBurnInBackgroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NonBurnInBackgroundBrushProperty, value);
			});
		}
	}

	public Brush AdaTurnedOnLabelBackgroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(AdaTurnedOnLabelBackgroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaTurnedOnLabelBackgroundBrushProperty, value);
			});
		}
	}

	public Brush AdaTurnedOnLabelForegroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(AdaTurnedOnLabelForegroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaTurnedOnLabelForegroundBrushProperty, value);
			});
		}
	}

	public Style HandicapButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(HandicapButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HandicapButtonStyleProperty, value);
			});
		}
	}

	public Brush HandicapButtonIconOffForegroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(HandicapButtonIconOffForegroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HandicapButtonIconOffForegroundBrushProperty, value);
			});
		}
	}

	public Brush HandicapButtonIconOnForegroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(HandicapButtonIconOnForegroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HandicapButtonIconOnForegroundBrushProperty, value);
			});
		}
	}

	public Brush EllipseFillBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(EllipseFillBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EllipseFillBrushProperty, value);
			});
		}
	}

	public Brush TextBlockForegroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(TextBlockForegroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TextBlockForegroundBrushProperty, value);
			});
		}
	}

	public Visibility KioskClosingMessageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(KioskClosingMessageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KioskClosingMessageVisibilityProperty, value);
			});
		}
	}

	public Brush KioskClosingMessageForeground
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(KioskClosingMessageForegroundProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KioskClosingMessageForegroundProperty, value);
			});
		}
	}

	public string KioskClosingMessageText1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(KioskClosingMessageText1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KioskClosingMessageText1Property, value);
			});
		}
	}

	public string KioskClosingMessageText2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(KioskClosingMessageText2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KioskClosingMessageText2Property, value);
			});
		}
	}

	public Style EspanolButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(EspanolButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EspanolButtonStyleProperty, value);
			});
		}
	}

	public Style HelpButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(HelpButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HelpButtonStyleProperty, value);
			});
		}
	}

	public Brush MainGridBackground
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(MainGridBackgroundProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MainGridBackgroundProperty, value);
			});
		}
	}

	public Style ExtraLargePrimaryButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(ExtraLargePrimaryButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ExtraLargePrimaryButtonStyleProperty, value);
			});
		}
	}

	public string AllMoviesButtonImageSource
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AllMoviesButtonImageSourceProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonImageSourceProperty, value);
			});
		}
	}

	public Style ExtraLargeSecondaryButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(ExtraLargeSecondaryButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ExtraLargeSecondaryButtonStyleProperty, value);
			});
		}
	}

	public Style MediumSecondaryButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(MediumSecondaryButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MediumSecondaryButtonStyleProperty, value);
			});
		}
	}

	public bool IsBurnInView
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsBurnInViewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsBurnInViewProperty, value);
			});
		}
	}

	public string PressToStartButtonImageSource
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PressToStartButtonImageSourceProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PressToStartButtonImageSourceProperty, value);
			});
		}
	}

	public StartViewBannerType BannerType { get; set; }

	public MarketingScreenSaverModel ScreenSaverModel { get; set; }

	public string SignUpButtonText { get; set; }

	public string SignInUpButtonSeparator { get; set; }

	public string SignInButtonTooltipText { get; set; }

	public IAdImpression AdImpression { get; set; }

	public DateTime CreatedDateTime { get; set; } = DateTime.Now;

	public TimeSpan Age => DateTime.Now - CreatedDateTime;

	public event Action OnRentMoviesButtonClicked;

	public event Action OnBuyMoviesButtonClicked;

	public event Action OnRentAndBuyMoviesButtonClicked;

	public event Action OnPickupButtonClicked;

	public event Action OnReturnButtonClicked;

	public event Action OnSignInButtonClicked;

	public event Action OnSignInButtonTooltipClicked;

	public event Action OnHelpButtonClicked;

	public event Action OnToggleLanguage;

	public event Action OnToggleADAMode;

	public event Action OnExitBurnInView;

	public event Action OnExitTestScreenSaver;

	public event Action OnBannerClicked;

	public void ProcessBannerClicked()
	{
		OnBannerClicked?.Invoke();
	}

	public void ProcessExitBurnInView()
	{
		OnExitBurnInView?.Invoke();
	}

	public void ProcessExitTestScreenSaver()
	{
		OnExitTestScreenSaver?.Invoke();
	}

	public void ProcessToggleLanguage()
	{
		OnToggleLanguage?.Invoke();
	}

	public void ProcessToggleADAMode()
	{
		OnToggleADAMode?.Invoke();
	}

	public void ProcessRentMovieButtonClick()
	{
		OnRentMoviesButtonClicked?.Invoke();
	}

	public void ProcessBuyMoviesButtonClick()
	{
		OnBuyMoviesButtonClicked?.Invoke();
	}

	public void ProcessRentAndBuyMoviesButtonClick()
	{
		OnRentAndBuyMoviesButtonClicked?.Invoke();
	}

	public void ProcessPickupButtonClick()
	{
		OnPickupButtonClicked?.Invoke();
	}

	public void ProcessReturnButtonClicked()
	{
		OnReturnButtonClicked?.Invoke();
	}

	public void ProcessHelpButtonClicked()
	{
		OnHelpButtonClicked?.Invoke();
	}

	public void ProcessSignInButtonClicked()
	{
		OnSignInButtonClicked?.Invoke();
	}

	public void ProcessSignInButtonTooltipClicked()
	{
		OnSignInButtonTooltipClicked?.Invoke();
	}

	public ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}
}
