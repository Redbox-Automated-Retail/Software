using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class KeypadModel : DependencyObject
{
	public Action<string> OnNumberButtonClicked;

	public Action OnClearAllButtonClicked;

	public Action OnBackButtonClicked;

	public string ClearAllButtonText { get; set; }

	public string BackButtonText { get; set; }

	public void ProcessOnNumberButtonClicked(string key)
	{
		if (OnNumberButtonClicked != null)
		{
			OnNumberButtonClicked(key);
		}
	}

	public void ProcessOnClearAllButtonClicked()
	{
		if (OnClearAllButtonClicked != null)
		{
			OnClearAllButtonClicked();
		}
	}

	public void ProcessOnBackButtonClicked()
	{
		if (OnBackButtonClicked != null)
		{
			OnBackButtonClicked();
		}
	}
}
