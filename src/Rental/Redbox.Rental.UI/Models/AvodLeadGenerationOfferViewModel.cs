using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class AvodLeadGenerationOfferViewModel : DependencyObject
{
	public Action OnNoButtonClicked;

	public Action OnEmailButtonClicked;

	public Action OnQRCodeButtonClicked;

	public string GridBackgroundImageSource { get; set; }

	public string EmailText { get; set; }

	public string NoButtonText { get; set; }

	public Visibility EmailMessageTextVisibility { get; set; }

	public Visibility QRCodeStackPanelVisibility { get; set; }

	public void ProcessOnNoButtonClicked()
	{
		OnNoButtonClicked?.Invoke();
	}

	public void ProcessOnEmailButtonClicked()
	{
		OnEmailButtonClicked?.Invoke();
	}

	public void ProcessOnQRCodeButtonClicked()
	{
		OnQRCodeButtonClicked?.Invoke();
	}
}
