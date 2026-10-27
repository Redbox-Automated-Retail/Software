using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Transaction;

public class GetCustomerOffersResponse : BaseResponse
{
	public string AccountNumber { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string EmailAddress { get; set; }

	public List<CustomerOffer> StoredOffers { get; set; }

	public List<CustomerOffer> NewOffers { get; set; }

	public GetCustomerOffersResponse()
		: base(200)
	{
		base.Success = true;
	}

	public GetCustomerOffersResponse(int statusCode)
		: base(statusCode)
	{
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
