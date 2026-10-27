using System;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class LoginInfo
{
	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public Action<UserData> AuthenticateAction { get; set; }

	public string StoreNumber { get; set; }

	public LoginModel Model { get; set; }
}
