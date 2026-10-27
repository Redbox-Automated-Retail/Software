using System;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.UI.Models;

public class UserData
{
	public string Id { get; set; }

	public string Password { get; set; }

	public string StoreNumber { get; set; }

	public Action<IRemoteServiceResult> AuthenticationResultCallback { get; set; }
}
