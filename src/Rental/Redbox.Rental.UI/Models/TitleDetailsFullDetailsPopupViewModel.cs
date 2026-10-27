using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class TitleDetailsFullDetailsPopupViewModel : BrowseItemModel
{
	public static readonly DependencyProperty StarringVisibilityProperty = DependencyProperty.Register("StarringVisibility", typeof(Visibility), typeof(TitleDetailsFullDetailsPopupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DirectorsVisibilityProperty = DependencyProperty.Register("DirectorsVisibility", typeof(Visibility), typeof(TitleDetailsFullDetailsPopupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RatingVisibilityProperty = DependencyProperty.Register("RatingVisibility", typeof(Visibility), typeof(TitleDetailsFullDetailsPopupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MultiNightPricesVisibilityProperty = DependencyProperty.Register("MultiNightPricesVisibility", typeof(Visibility), typeof(TitleDetailsFullDetailsPopupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MultiNightReturnTimesVisibilityProperty = DependencyProperty.Register("MultiNightReturnTimesVisibility", typeof(Visibility), typeof(TitleDetailsFullDetailsPopupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ServiceFeeMessageVisibilityProperty = DependencyProperty.Register("ServiceFeeMessageVisibility", typeof(Visibility), typeof(TitleDetailsFullDetailsPopupViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DealMessageVisibilityProperty = DependencyProperty.Register("DealMessageVisibility", typeof(Visibility), typeof(TitleDetailsFullDetailsPopupViewModel), new FrameworkPropertyMetadata(null));

	public Action OnFullDetailsCloseButtonClicked;

	public string Header { get; set; }

	public string Description { get; set; }

	public string StarringLabel { get; set; }

	public string Starring { get; set; }

	public string DirectorsLabel { get; set; }

	public string Directors { get; set; }

	public string RatingLabel { get; set; }

	public string Rating { get; set; }

	public string ClosedCaptionedText { get; set; }

	public string OneNightPriceLabel { get; set; }

	public string FormatInfo { get; set; }

	public string OneNightPrices { get; set; }

	public string OneNightReturnTimeLabel { get; set; }

	public string OneNightReturnTimes { get; set; }

	public string MultiNightPriceLabel { get; set; }

	public string MultiNightPrices { get; set; }

	public string MultiNightReturnTimeLabel { get; set; }

	public string MultiNightReturnTimes { get; set; }

	public string ServiceFeeMessage { get; set; }

	public string DealMessage { get; set; }

	public string CloseButtonText { get; set; }

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

	public Visibility DirectorsVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DirectorsVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DirectorsVisibilityProperty, value);
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

	public Visibility MultiNightPricesVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MultiNightPricesVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MultiNightPricesVisibilityProperty, value);
			});
		}
	}

	public Visibility MultiNightReturnTimesVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MultiNightReturnTimesVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MultiNightReturnTimesVisibilityProperty, value);
			});
		}
	}

	public Visibility ServiceFeeMessageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ServiceFeeMessageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ServiceFeeMessageVisibilityProperty, value);
			});
		}
	}

	public Visibility DealMessageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DealMessageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DealMessageVisibilityProperty, value);
			});
		}
	}

	public void ProcessOnFullDetailsCloseButtonClicked()
	{
		if (OnFullDetailsCloseButtonClicked != null)
		{
			OnFullDetailsCloseButtonClicked();
		}
	}
}
