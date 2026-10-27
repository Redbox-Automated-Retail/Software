using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IGetCustomerOffersResult
{
	bool Success { get; set; }

	ICollection<Error> Errors { get; set; }

	string AccountNumber { get; }

	string CustomerProfileNumber { get; }

	string EmailAddress { get; }

	ICollection<CustomerOfferResult> StoredOffers { get; }

	ICollection<CustomerOfferResult> NewOffers { get; }
}
