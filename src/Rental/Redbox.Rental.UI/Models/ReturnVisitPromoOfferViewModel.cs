using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class ReturnVisitPromoOfferViewModel : DependencyObject
{
	public Action OnCloseButtonClicked;

	public Action OnUseNowButtonClicked;

	public string TitleText { get; set; }

	public string AmountText { get; set; }

	public string MessageText { get; set; }

	public string EligibleText { get; set; }

	public string PromoCodeText { get; set; }

	public string ExpirationText { get; set; }

	public string LegalText { get; set; }

	public string CloseButtonText { get; set; }

	public string UseNowButtonText { get; set; }

	public void ProcessOnCloseButtonClicked()
	{
		OnCloseButtonClicked?.Invoke();
	}

	public void ProcessOnUseNowButtonClicked()
	{
		OnUseNowButtonClicked?.Invoke();
	}
}
