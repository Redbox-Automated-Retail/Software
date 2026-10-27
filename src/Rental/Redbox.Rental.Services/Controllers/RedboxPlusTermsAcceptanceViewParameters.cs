using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.Controllers;

public class RedboxPlusTermsAcceptanceViewParameters
{
	public string EmailAddress { get; set; }

	public IRedboxPlusSubscriptionProduct RedboxPlusSubscriptionProduct { get; set; }
}
