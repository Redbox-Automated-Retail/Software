using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public class GetCustomerOffersResult : IGetCustomerOffersResult
{
	public bool Success { get; set; }

	public ICollection<Error> Errors { get; set; }

	public string AccountNumber { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string EmailAddress { get; set; }

	public ICollection<CustomerOfferResult> StoredOffers { get; set; }

	public ICollection<CustomerOfferResult> NewOffers { get; set; }
}
