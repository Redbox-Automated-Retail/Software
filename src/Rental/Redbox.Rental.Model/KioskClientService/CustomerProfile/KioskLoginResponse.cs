using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.CustomerProfile;

public class KioskLoginResponse
{
	public KioskCustomer Customer { get; set; }

	public KioskLoyalty Loyalty { get; set; }

	public KioskRecommendedTitles Recommendations { get; set; }

	public List<KioskAcceptedOffer> AcceptedOffers { get; set; }

	public List<KioskStoredPromoCode> PromoCodes { get; set; }

	public List<string> OptIns { get; set; }

	public KioskSubscriptions Subscriptions { get; set; }

	public virtual void ScrubData()
	{
		Customer.ScrubData();
	}
}
