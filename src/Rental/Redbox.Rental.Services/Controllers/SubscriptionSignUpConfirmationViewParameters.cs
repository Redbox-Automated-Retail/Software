using System;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.Controllers;

public class SubscriptionSignUpConfirmationViewParameters
{
	public ISubscriptionProduct SubscriptionProduct { get; set; }

	public Action ContinueAction { get; set; }
}
