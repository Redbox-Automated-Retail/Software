using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class FilterBarModel : DependencyObject
{
	public static readonly DependencyProperty FilterBarTextProperty = DependencyProperty.Register("FilterBarText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty FilterBarTextVisibilityProperty = DependencyProperty.Register("FilterBarTextVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty StartOverButtonTextProperty = DependencyProperty.Register("StartOverButtonText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty StartOverButtonVisibilityProperty = DependencyProperty.Register("StartOverButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty HomeButtonTextProperty = DependencyProperty.Register("HomeButtonText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty HomeButtonVisibilityProperty = DependencyProperty.Register("HomeButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty FormatFilterButtonVisibilityProperty = DependencyProperty.Register("FormatFilterButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty FormatFilterButtonCheckmarkVisibilityProperty = DependencyProperty.Register("FormatFilterButtonCheckmarkVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty FormatFilterButtonTextProperty = DependencyProperty.Register("FormatFilterButtonText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(string.Empty));

	public static readonly DependencyProperty FormatFilterButtonStyleProperty = DependencyProperty.Register("FormatFilterButtonStyle", typeof(Style), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SelectedFormatProperty = DependencyProperty.Register("SelectedFormat", typeof(TitleType?), typeof(FilterBarModel), new FrameworkPropertyMetadata(null, SelectedFormatChanged));

	public static readonly DependencyProperty PriceRangeFilterButtonVisibilityProperty = DependencyProperty.Register("PriceRangeFilterButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty PriceRangeFilterButtonCheckmarkVisibilityProperty = DependencyProperty.Register("PriceRangeFilterButtonCheckmarkVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty PriceRangeFilterButtonTextProperty = DependencyProperty.Register("PriceRangeFilterButtonText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(string.Empty));

	public static readonly DependencyProperty PriceRangeFilterButtonStyleProperty = DependencyProperty.Register("PriceRangeFilterButtonStyle", typeof(Style), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SelectedPriceRangeProperty = DependencyProperty.Register("SelectedPriceRange", typeof(BrowsePriceRangeParameter), typeof(FilterBarModel), new FrameworkPropertyMetadata(new BrowsePriceRangeParameter(), SelectedPriceRangeChanged));

	public string BackButtonText { get; set; }

	public string PriceRangeButtonDefaultText { get; set; }

	public List<BrowseFilter> Filters { get; set; }

	public Thickness FilterMargin { get; set; }

	public Dictionary<TitleType, string> FormatFilterButtonStrings { get; set; }

	public Style FilterButtonUnselectedStyle { get; set; }

	public Style FilterButtonSelectedStyle { get; set; }

	public CheckMarkButtonModel BottomRedboxPlusMoviesButtonModel { get; set; }

	public string StartOverButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(StartOverButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StartOverButtonTextProperty, value);
			});
		}
	}

	public Visibility StartOverButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(StartOverButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StartOverButtonVisibilityProperty, value);
			});
		}
	}

	public string HomeButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(HomeButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HomeButtonTextProperty, value);
			});
		}
	}

	public Visibility HomeButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(HomeButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HomeButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility BackButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BackButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility FormatFilterButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(FormatFilterButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FormatFilterButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility FormatFilterButtonCheckmarkVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(FormatFilterButtonCheckmarkVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FormatFilterButtonCheckmarkVisibilityProperty, value);
			});
		}
	}

	public Visibility FilterBarTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(FilterBarTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FilterBarTextVisibilityProperty, value);
			});
		}
	}

	public string FilterBarText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(FilterBarTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FilterBarTextProperty, value);
			});
		}
	}

	public string FormatFilterButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(FormatFilterButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FormatFilterButtonTextProperty, value);
			});
		}
	}

	public TitleType SelectedFormat
	{
		get
		{
			return base.Dispatcher.Invoke(() => (TitleType)GetValue(SelectedFormatProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SelectedFormatProperty, value);
			});
		}
	}

	public Style FormatFilterButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(FormatFilterButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FormatFilterButtonStyleProperty, value);
			});
		}
	}

	public Visibility PriceRangeFilterButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PriceRangeFilterButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PriceRangeFilterButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility PriceRangeFilterButtonCheckmarkVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PriceRangeFilterButtonCheckmarkVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PriceRangeFilterButtonCheckmarkVisibilityProperty, value);
			});
		}
	}

	public Style PriceRangeFilterButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(PriceRangeFilterButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PriceRangeFilterButtonStyleProperty, value);
			});
		}
	}

	public string PriceRangeFilterButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PriceRangeFilterButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PriceRangeFilterButtonTextProperty, value);
			});
		}
	}

	public BrowsePriceRangeParameter SelectedPriceRange
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowsePriceRangeParameter)GetValue(SelectedPriceRangeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SelectedPriceRangeProperty, value);
			});
		}
	}

	private static bool IsADAMode => ServiceLocator.Instance.GetService<IApplicationState>()?.IsADAAccessible ?? false;

	public event Action OnStartOverButtonClicked;

	public event Action OnHomeButtonClicked;

	public event Action OnBackButtonClicked;

	public event Action<BrowseFilter> OnBrowseFilterButtonClicked;

	public event Action OnFormatFilterButtonClicked;

	public event Action OnPriceRangeFilterButtonClicked;

	public void ProcessOnStartOverButtonClicked()
	{
		if (OnStartOverButtonClicked != null)
		{
			OnStartOverButtonClicked();
		}
	}

	public void ProcessOnHomeButtonClicked()
	{
		OnHomeButtonClicked?.Invoke();
	}

	public void ProcessOnFormatFilterButtonClicked()
	{
		if (OnFormatFilterButtonClicked != null)
		{
			OnFormatFilterButtonClicked();
		}
	}

	public void ProcessOnPriceRangeFilterButtonClicked()
	{
		if (OnFormatFilterButtonClicked != null)
		{
			OnPriceRangeFilterButtonClicked();
		}
	}

	public void ProcessOnBrowseFilterButtonClicked(BrowseFilter browseFilter)
	{
		if (OnBrowseFilterButtonClicked != null)
		{
			OnBrowseFilterButtonClicked(browseFilter);
		}
	}

	public void ProcessOnBackButtonClicked()
	{
		if (OnBackButtonClicked != null)
		{
			OnBackButtonClicked();
		}
	}

	private static void SelectedPriceRangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
	{
		if (obj != null && obj is FilterBarModel)
		{
			FilterBarModel filterBarModel = (FilterBarModel)obj;
			BrowsePriceRangeParameter browsePriceRangeParameter = ((args.NewValue == null) ? null : ((BrowsePriceRangeParameter)args.NewValue));
			if (browsePriceRangeParameter != null)
			{
				filterBarModel.PriceRangeFilterButtonText = browsePriceRangeParameter.buttonText;
				filterBarModel.PriceRangeFilterButtonStyle = filterBarModel.FilterButtonSelectedStyle;
				filterBarModel.PriceRangeFilterButtonCheckmarkVisibility = ((!IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
			}
			else
			{
				filterBarModel.PriceRangeFilterButtonText = filterBarModel.PriceRangeButtonDefaultText;
				filterBarModel.PriceRangeFilterButtonStyle = filterBarModel.FilterButtonUnselectedStyle;
				filterBarModel.PriceRangeFilterButtonCheckmarkVisibility = Visibility.Collapsed;
			}
		}
	}

	private static void SelectedFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
	{
		if (obj != null && obj is FilterBarModel)
		{
			FilterBarModel filterBarModel = (FilterBarModel)obj;
			TitleType titleType = ((args.NewValue != null) ? ((TitleType?)args.NewValue).Value : TitleType.All);
			if (filterBarModel.FormatFilterButtonStrings.ContainsKey(titleType))
			{
				filterBarModel.FormatFilterButtonText = filterBarModel.FormatFilterButtonStrings[titleType];
				filterBarModel.FormatFilterButtonStyle = ((titleType == TitleType.All) ? filterBarModel.FilterButtonUnselectedStyle : filterBarModel.FilterButtonSelectedStyle);
				filterBarModel.FormatFilterButtonCheckmarkVisibility = ((filterBarModel.FormatFilterButtonVisibility != Visibility.Visible || titleType == TitleType.All) ? Visibility.Collapsed : Visibility.Visible);
			}
		}
	}
}
