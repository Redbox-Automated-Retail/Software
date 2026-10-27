using System;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models;

public class EmailOptInViewModel
{
	public BitmapImage Image { get; set; }

	public string YesButtonText { get; set; }

	public string NoThanksButtonText { get; set; }

	public string MessageText { get; set; }

	public bool HasActionExecuted { get; set; }

	public event Action OnYesButtonClicked;

	public event Action OnNoThanksButtonClicked;

	public void ProcessOnYesButtonClicked()
	{
		OnYesButtonClicked?.Invoke();
	}

	public void ProcessOnNoThanksButtonClicked()
	{
		OnNoThanksButtonClicked?.Invoke();
	}
}
