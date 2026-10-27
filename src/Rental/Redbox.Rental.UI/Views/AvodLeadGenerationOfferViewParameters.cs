using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

public class AvodLeadGenerationOfferViewParameters
{
	public string EmailAddress { get; set; }

	public OptInConfirmationStatus OptInConfirmationStatus { get; set; }

	public Action ContinueAction { get; set; }
}
