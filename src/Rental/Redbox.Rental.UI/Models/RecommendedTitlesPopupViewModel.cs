using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class RecommendedTitlesPopupViewModel : DependencyObject
{
	public Action OnHowPointsWorkButtonClicked;

	public Action OnPickupButtonClicked;

	public Action OnCancelButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(RecommendedTitlesPopupViewModel), new FrameworkPropertyMetadata(null));

	public string PerksHeadingText { get; set; }

	public string PerksMessageText { get; set; }

	public string HowPointsWorkButtonText { get; set; }

	public string HeaderText { get; set; }

	public string PickupButtonText { get; set; }

	public string CancelButtonText { get; set; }

	public Visibility PerksInfoBorderVisibility { get; set; }

	public Visibility HowPointsWorkButtonVisibility { get; set; }

	public Visibility PickupButtonVisibility { get; set; }

	public CornerRadius RecommendationsBorderCornerRadius { get; set; }

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

	public void ProcessOnHowPointsWorkButtonClicked()
	{
		if (OnHowPointsWorkButtonClicked != null)
		{
			OnHowPointsWorkButtonClicked();
		}
	}

	public void ProcessOnPickupButtonClicked()
	{
		if (OnPickupButtonClicked != null)
		{
			OnPickupButtonClicked();
		}
	}

	public void ProcessOnCancelButtonClicked()
	{
		if (OnCancelButtonClicked != null)
		{
			OnCancelButtonClicked();
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
