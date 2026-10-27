using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class RedboxPlusInfoPopupViewModel : DependencyObject
{
	public Action OnCloseButtonClicked;

	public string Header { get; set; }

	public string DescriptionLine1 { get; set; }

	public string DescriptionLine2 { get; set; }

	public string DescriptionLine3 { get; set; }

	public string Note { get; set; }

	public string CloseButtonText { get; set; }

	public void ProcessOnCloseButtonClicked()
	{
		OnCloseButtonClicked?.Invoke();
	}
}
