using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class PerksOfferDetailsViewModel : DependencyObject
{
	public Action OnCancelButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public string OfferValueText { get; set; }

	public string OfferUnitsText { get; set; }

	public string ProgressText { get; set; }

	public int? CurrentValue { get; set; }

	public int? MaxValue { get; set; }

	public string NameText { get; set; }

	public string DescriptionText { get; set; }

	public string DateRangeText { get; set; }

	public string LegalInformationText { get; set; }

	public string CancelButtonText { get; set; }

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
