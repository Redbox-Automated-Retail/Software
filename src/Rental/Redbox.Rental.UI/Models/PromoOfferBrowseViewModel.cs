using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class PromoOfferBrowseViewModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MiniCartControlModelProperty = DependencyProperty.Register("MiniCartControlModel", typeof(BrowseControlModel), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaMiniCartControlModelProperty = DependencyProperty.Register("AdaMiniCartControlModel", typeof(BrowseControlModel), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MiniCartVisibilityProperty = DependencyProperty.Register("MiniCartVisibility", typeof(Visibility), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty PageNumberTextProperty = DependencyProperty.Register("PageNumberText", typeof(string), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(""));

	public static readonly DependencyProperty PageNumberProperty = DependencyProperty.Register("PageNumber", typeof(int), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(1));

	public static readonly DependencyProperty MiniCartTextProperty = DependencyProperty.Register("MiniCartText", typeof(string), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(""));

	public static readonly DependencyProperty AddToADACartLabelTextProperty = DependencyProperty.Register("AddToADACartLabelText", typeof(string), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddToADACartLabelVisibilityProperty = DependencyProperty.Register("AddToADACartLabelVisibility", typeof(Visibility), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty EmptyCartSpacesVisibilityProperty = DependencyProperty.Register("EmptyCartSpacesVisibilityVisibility", typeof(Visibility), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public string HeaderText { get; set; }

	public string CancelButtonText { get; set; }

	public string ContinueButtonText { get; set; }

	public Visibility AdaMiniCartVisibility { get; set; }

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

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public DynamicRoutedCommand ContinueButtonCommand { get; set; }

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

	public BrowseControlModel AdaMiniCartControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(AdaMiniCartControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaMiniCartControlModelProperty, value);
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

	public string PageNumberText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PageNumberTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageNumberTextProperty, value);
			});
		}
	}

	public int PageNumber
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(PageNumberProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageNumberProperty, value);
			});
		}
	}

	public string MiniCartText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MiniCartTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MiniCartTextProperty, value);
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
