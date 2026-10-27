using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Transaction;

public class ConvertedGetCustomerOffersResponse : BaseResponse
{
	public string AccountNumber { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string EmailAddress { get; set; }

	public List<ConvertedCustomerOffer> StoredOffers { get; set; }

	public List<ConvertedCustomerOffer> NewOffers { get; set; }

	public ConvertedGetCustomerOffersResponse()
		: base(200)
	{
		base.Success = true;
	}

	public ConvertedGetCustomerOffersResponse(int statusCode)
		: base(statusCode)
	{
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
