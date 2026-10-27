using System;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Controllers;

public class SignInViewParameters
{
	public Action<LoginType, ILoginResult> AfterSignInAction { get; set; }

	public Action SignUpAction { get; set; }

	public SignUpType SignUpType { get; set; }
}
