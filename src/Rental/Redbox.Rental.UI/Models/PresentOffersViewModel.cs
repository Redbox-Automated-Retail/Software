using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class PresentOffersViewModel : BaseModel<PresentOffersViewModel>
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public ObservableCollection<OfferInfoModel> Offers { get; set; }

	public string MessageText { get; set; }

	public Visibility MessageVisibility { get; set; }

	public string ExpirationText { get; set; }

	public Visibility ExpirationVisibility { get; set; }

	public string LegalText1 { get; set; }

	public string LegalText2 { get; set; }

	public string LegalText3 { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public string CancelButtonText { get; set; }

	public Visibility CancelButtonVisibility { get; set; }

	public Orientation OfferOrientation { get; set; }

	public string MultiChoiceBannerText { get; set; }

	public Visibility BannerVisibility { get; set; }

	public Visibility LegalText2Visibility { get; set; }

	public Visibility LegalText3Visibility { get; set; }

	public Visibility SingleOfferVisibility { get; set; }

	public double OfferAreaHeight { get; set; }

	public new ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}
}
