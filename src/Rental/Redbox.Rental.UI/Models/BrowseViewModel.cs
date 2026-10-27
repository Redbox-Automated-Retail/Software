using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class BrowseViewModel : DependencyObject
{
	public Action OnCheckOutButtonClicked;

	public Action OnToggleTitleFamilyButton1Clicked;

	public Action OnSignInButtonClicked;

	public Action OnSignOutButtonClicked;

	public Action OnMyPerksButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BrowseGroupSelectionProperty = DependencyProperty.Register("BrowseGroupSelection", typeof(DisplayGroupSelectionModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DisplayGroupItemsProperty = DependencyProperty.Register("DisplayGroupItems", typeof(List<DisplayGroupSelectionItemModel>), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CheckOutButtonTextProperty = DependencyProperty.Register("CheckOutButtonText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CheckOutButtonCountTextProperty = DependencyProperty.Register("CheckOutButtonCountText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CheckOutButtonStyleProperty = DependencyProperty.Register("CheckOutButtonStyle", typeof(Style), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddToADACartLabelTextProperty = DependencyProperty.Register("AddToADACartLabelText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SwitchProductFamilyButton1VisibilityProperty = DependencyProperty.Register("SwitchProductFamilyButton1Visibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AddToADACartLabelVisibilityProperty = DependencyProperty.Register("AddToADACartLabelVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty MiniCartVisibilityProperty = DependencyProperty.Register("MiniCartVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BackgroundImageVisibilityProperty = DependencyProperty.Register("BackgroundImageVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty SwitchProductFamilyButton1TextProperty = DependencyProperty.Register("SwitchProductFamilyButton1Text", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null, OnBrowseProductControlModelPropertyChanged));

	public static readonly DependencyProperty MiniCartControlModelProperty = DependencyProperty.Register("MiniCartControlModel", typeof(BrowseControlModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseFilterBarModelProperty = DependencyProperty.Register("BrowseFilterBarModel", typeof(FilterBarModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseTopMenuModelProperty = DependencyProperty.Register("BrowseTopMenuModel", typeof(TopMenuModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SignInButtonVisibilityProperty = DependencyProperty.Register("SignInButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty SignOutButtonVisibilityProperty = DependencyProperty.Register("SignOutButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty MyPerksButtonVisibilityProperty = DependencyProperty.Register("MyPerksButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty MyPerksRedboxPlusButtonVisibilityProperty = DependencyProperty.Register("MyPerksRedboxPlusButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty ViewRedboxPlusMoviesButtonVisibilityProperty = DependencyProperty.Register("ViewRedboxPlusMoviesButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty RedboxPlusMiniCartVisiblityProperty = DependencyProperty.Register("RedboxPlusMiniCartVisiblity", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty ADARedboxPlusSignOutGridVisibilityProperty = DependencyProperty.Register("ADARedboxPlusSignOutGridVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty RedboxPlusEmptyCartSpaceVisiblityProperty = DependencyProperty.Register("RedboxPlusEmptyCartSpaceVisiblity", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty RedboxPlusMiniCartMessageTextProperty = DependencyProperty.Register("RedboxPlusMiniCartMessageText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BackToAllMoviesButtonVisibilityProperty = DependencyProperty.Register("BackToAllMoviesButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty EmptyCartSpacesVisibilityProperty = DependencyProperty.Register("EmptyCartSpacesVisibilityVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty EmptyCartAddItemMessageVisibilityProperty = DependencyProperty.Register("EmptyCartAddItemMessageVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty ContinueButtonVisibilityProperty = DependencyProperty.Register("ContinueButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty CheckOutButtonVisibilityProperty = DependencyProperty.Register("CheckOutButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BrowseRedboxPlusTooltipModelProperty = DependencyProperty.Register("BrowseRedboxPlusTooltipModel", typeof(BrowseRedboxPlusTooltipModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseMessagePopupModelProperty = DependencyProperty.Register("BrowseMessagePopupModel", typeof(BrowseMessagePopupModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public DynamicRoutedCommand BackToAllMoviesCommand { get; set; }

	public DynamicRoutedCommand ContinueCommand { get; set; }

	public DynamicRoutedCommand ViewRedboxPlusMoviesCommand { get; set; }

	public string SignInButtonText { get; set; }

	public string SignUpButtonText { get; set; }

	public string SignInUpButtonSeparator { get; set; }

	public string SignOutButtonText { get; set; }

	public string MyPerksButtonText { get; set; }

	public string ViewRedboxPlusMoviesButtonText { get; set; }

	public string BackToAllMoviesButtonText { get; set; }

	public string ContinueButtonText { get; set; }

	public string DontMissYourPerksText { get; set; }

	public BrowseControlModel BrowseProductControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseProductControlModelProperty, value);
			});
		}
	}

	public BrowseControlModel MiniCartControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(MiniCartControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MiniCartControlModelProperty, value);
			});
		}
	}

	public FilterBarModel BrowseFilterBarModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (FilterBarModel)GetValue(BrowseFilterBarModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseFilterBarModelProperty, value);
			});
		}
	}

	public TopMenuModel BrowseTopMenuModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (TopMenuModel)GetValue(BrowseTopMenuModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseTopMenuModelProperty, value);
			});
		}
	}

	public string CheckOutButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CheckOutButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckOutButtonTextProperty, value);
			});
		}
	}

	public string CheckOutButtonCountText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CheckOutButtonCountTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckOutButtonCountTextProperty, value);
			});
		}
	}

	public Style CheckOutButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(CheckOutButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckOutButtonStyleProperty, value);
			});
		}
	}

	public string AddToADACartLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AddToADACartLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddToADACartLabelTextProperty, value);
			});
		}
	}

	public Visibility AddToADACartLabelVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddToADACartLabelVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddToADACartLabelVisibilityProperty, value);
			});
		}
	}

	public Visibility MiniCartVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MiniCartVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MiniCartVisibilityProperty, value);
			});
		}
	}

	public Visibility SwitchProductFamilyButton1Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SwitchProductFamilyButton1VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SwitchProductFamilyButton1VisibilityProperty, value);
			});
		}
	}

	public Visibility BackgroundImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BackgroundImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackgroundImageVisibilityProperty, value);
			});
		}
	}

	public string SwitchProductFamilyButton1Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SwitchProductFamilyButton1TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SwitchProductFamilyButton1TextProperty, value);
			});
		}
	}

	public bool IsADAAccessible { get; set; }

	public DisplayGroupSelectionModel BrowseGroupSelection
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DisplayGroupSelectionModel)GetValue(BrowseGroupSelectionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseGroupSelectionProperty, value);
			});
		}
	}

	public Visibility SignInButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SignInButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility SignOutButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SignOutButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignOutButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility MyPerksButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MyPerksButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MyPerksButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility MyPerksRedboxPlusButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MyPerksRedboxPlusButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MyPerksRedboxPlusButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility ViewRedboxPlusMoviesButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ViewRedboxPlusMoviesButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ViewRedboxPlusMoviesButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility RedboxPlusMiniCartVisiblity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusMiniCartVisiblityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RedboxPlusMiniCartVisiblityProperty, value);
			});
		}
	}

	public Visibility ADARedboxPlusSignOutGridVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ADARedboxPlusSignOutGridVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ADARedboxPlusSignOutGridVisibilityProperty, value);
			});
		}
	}

	public Visibility RedboxPlusEmptyCartSpaceVisiblity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusEmptyCartSpaceVisiblityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RedboxPlusEmptyCartSpaceVisiblityProperty, value);
			});
		}
	}

	public string RedboxPlusMiniCartMessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(RedboxPlusMiniCartMessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RedboxPlusMiniCartMessageTextProperty, value);
			});
		}
	}

	public Visibility BackToAllMoviesButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BackToAllMoviesButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackToAllMoviesButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility EmptyCartSpacesVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(EmptyCartSpacesVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EmptyCartSpacesVisibilityProperty, value);
			});
		}
	}

	public Visibility EmptyCartAddItemMessageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(EmptyCartAddItemMessageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EmptyCartAddItemMessageVisibilityProperty, value);
			});
		}
	}

	public Visibility ContinueButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ContinueButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility CheckOutButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CheckOutButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckOutButtonVisibilityProperty, value);
			});
		}
	}

	public BrowseMessagePopupModel BrowseMessagePopupModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseMessagePopupModel)GetValue(BrowseMessagePopupModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseMessagePopupModelProperty, value);
			});
		}
	}

	public BrowseRedboxPlusTooltipModel BrowseRedboxPlusTooltipModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseRedboxPlusTooltipModel)GetValue(BrowseRedboxPlusTooltipModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseRedboxPlusTooltipModelProperty, value);
			});
		}
	}

	public string AddToCartLabel1Text { get; set; }

	public string AddToCartLabel2Text { get; set; }

	public event Action<BrowseViewModel> OnBrowseProductControlModelChanged;

	public ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}

	public void ProcessOnCheckOutButtonClicked()
	{
		if (OnCheckOutButtonClicked != null)
		{
			OnCheckOutButtonClicked();
		}
	}

	public void ProcessOnToggleTitleFamilyButton1Clicked()
	{
		if (OnToggleTitleFamilyButton1Clicked != null)
		{
			OnToggleTitleFamilyButton1Clicked();
		}
	}

	public void ProcessOnSignInButtonClicked()
	{
		OnSignInButtonClicked?.Invoke();
	}

	public void ProcessOnSignOutButtonClicked()
	{
		OnSignOutButtonClicked?.Invoke();
	}

	public void ProcessOnMyPerksButtonClicked()
	{
		OnMyPerksButtonClicked?.Invoke();
	}

	private static void OnBrowseProductControlModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is BrowseViewModel { OnBrowseProductControlModelChanged: not null } browseViewModel)
		{
			browseViewModel.OnBrowseProductControlModelChanged(browseViewModel);
		}
	}
}
