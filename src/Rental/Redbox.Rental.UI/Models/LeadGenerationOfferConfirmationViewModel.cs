using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class LeadGenerationOfferConfirmationViewModel : DependencyObject
{
	public Action OnGotItButtonClicked;

	public void ProcessOnGotItButtonClicked()
	{
		OnGotItButtonClicked?.Invoke();
	}
}
