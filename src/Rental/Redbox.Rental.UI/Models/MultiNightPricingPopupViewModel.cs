using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.UI.Models;

public class MultiNightPricingPopupViewModel : DependencyObject
{
	public Action OnOneNightButtonClicked;

	public Action OnMultiNightButtonClicked;

	public Action OnBuyButtonClicked;

	public Action OnCancelButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public ITitleProduct TitleProduct { get; set; }

	public IList<IPricingRecord> PricingRecords { get; set; }

	public string HeaderText { get; set; }

	public string DescriptionText { get; set; }

	public Visibility OneNightButtonVisibility { get; set; }

	public string OneNightButtonText1 { get; set; }

	public string OneNightButtonText2 { get; set; }

	public Visibility MultiNightButtonVisibility { get; set; }

	public string MultiNightButtonText1 { get; set; }

	public string MultiNightButtonText2 { get; set; }

	public Visibility BuyButtonVisibility { get; set; }

	public string BuyButtonText1 { get; set; }

	public string BuyButtonText2 { get; set; }

	public string CancelButtonText { get; set; }

	public string DisclaimerText { get; set; }

	public void ProcessOnOneNightButtonClicked()
	{
		if (OnOneNightButtonClicked != null)
		{
			OnOneNightButtonClicked();
		}
	}

	public void ProcessOnMultiNightButtonClicked()
	{
		if (OnMultiNightButtonClicked != null)
		{
			OnMultiNightButtonClicked();
		}
	}

	public void ProcessOnBuyButtonClicked()
	{
		if (OnBuyButtonClicked != null)
		{
			OnBuyButtonClicked();
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
