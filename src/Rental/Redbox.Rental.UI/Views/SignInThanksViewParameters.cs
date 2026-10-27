using System;

namespace Redbox.Rental.UI.Views;

public class SignInThanksViewParameters
{
	public string EmailAddress { get; set; }

	public int DelaySeconds { get; set; }

	public Action ContinueAction { get; set; }

	public bool PopOnlyOnViewGoBack { get; set; }
}
