using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class RatingSelectionPopupViewModel : DependencyObject
{
	public string HeaderText { get; set; }

	public string DescriptionText { get; set; }

	public string CancelButtonText { get; set; }

	public string ApplyButtonText { get; set; }

	public List<RatingButtonModel> RatingButtonModels { get; set; }

	public event Action OnCancelButtonClicked;

	public event Action OnApplyButtonClicked;

	public void ProcessOnCancelButtonClicked()
	{
		OnCancelButtonClicked?.Invoke();
	}

	public void ProcessOnApplyButtonClicked()
	{
		OnApplyButtonClicked?.Invoke();
	}
}
