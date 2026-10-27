using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class PromoConfirmationViewModel
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public string MaskedPromoCode { get; set; }

	public string PromoCode { get; set; }

	public string DescriptionText { get; set; }

	public string CancelButtonText { get; set; }

	public string ApplyButtonText { get; set; }

	public string LegalText { get; set; }

	public DynamicRoutedCommand CancelCommand { get; set; }

	public DynamicRoutedCommand ApplyCommand { get; set; }
}
