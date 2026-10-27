using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.UI.Models;

public class RecommendationOnPickupViewModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public Action OnPickupButtonClicked;

	public Action OnPayButtonClicked;

	public Action OnTermsAndPrivacyButtonClicked;

	public static readonly DependencyProperty ServiceFeeMessageProperty = DependencyProperty.Register("ServiceFeeMessage", typeof(string), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText", typeof(string), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PayButtonTextProperty = DependencyProperty.Register("PayButtonText", typeof(string), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ShoppingCartControlModelProperty = DependencyProperty.Register("ShoppingCartControlModel", typeof(BrowseControlModel), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PayButtonVisibilityProperty = DependencyProperty.Register("PayButtonVisibility", typeof(Visibility), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty PickupButtonVisibilityProperty = DependencyProperty.Register("PickupButtonVisibility", typeof(Visibility), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PricingModelProperty = DependencyProperty.Register("PricingModel", typeof(PricingModel), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

	public bool HasPickupOrPayActionExecuted { get; set; }

	public bool IsViewUpdatingCart { get; set; }

	public string AddMoreText { get; set; }

	public string TermsAndPrivacyButtonText { get; set; }

	public string ChargedOnceText { get; set; }

	public string AgreeToTermsText { get; set; }

	public string PickupButtonText { get; set; }

	public List<IRentalShoppingCartTitleItem> SecondaryMdvDiscs { get; set; }

	public BrowseControlModel ShoppingCartControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(ShoppingCartControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ShoppingCartControlModelProperty, value);
			});
		}
	}

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

	public PricingModel PricingModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (PricingModel)GetValue(PricingModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PricingModelProperty, value);
			});
		}
	}

	public string TitleText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleTextProperty, value);
			});
		}
	}

	public string ServiceFeeMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ServiceFeeMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ServiceFeeMessageProperty, value);
			});
		}
	}

	public string PayButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PayButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PayButtonTextProperty, value);
			});
		}
	}

	public Visibility PayButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PayButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PayButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility PickupButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PickupButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PickupButtonVisibilityProperty, value);
			});
		}
	}

	public RecommendationOnPickupViewModel()
	{
		PricingModel = new PricingModel();
		IsViewUpdatingCart = false;
	}

	public void ProcessOnTermsAndPrivacyButtonClicked()
	{
		OnTermsAndPrivacyButtonClicked?.Invoke();
	}

	public void ProcessOnPickupButtonClicked()
	{
		OnPickupButtonClicked?.Invoke();
	}

	public void ProcessOnPayButtonClicked()
	{
		OnPayButtonClicked?.Invoke();
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
