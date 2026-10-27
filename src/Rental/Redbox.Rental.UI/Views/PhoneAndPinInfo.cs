using System;
using System.Security;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class PhoneAndPinInfo
{
	public PhoneAndPinViewModel PhoneAndPinViewModel { get; set; }

	public SignInCancelButtonTypes SignInCancelButtonType { get; set; }

	public Action<string, SecureString, bool> SignInAction { get; set; }

	public Action ContinueAction { get; set; }

	public string MobilePhoneNumber { get; set; }

	public bool IsSignUp { get; set; }

	public SignUpFlowContext SignUpFlowContext { get; set; }

	public bool ShowTextClubOption { get; set; }

	public string IdleTimerName { get; set; }
}
