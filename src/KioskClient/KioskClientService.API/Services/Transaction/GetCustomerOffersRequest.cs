using System;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Transaction;

public class GetCustomerOffersRequest : MessageBase
{
	public CreditCard CreditCard { get; set; }

	public ShoppingCart ShoppingCart { get; set; }

	public string UtcOffset { get; set; }

	public string Lang { get; set; }

	public string StorefrontEventType { get; set; }

	public override object Scrub()
	{
		try
		{
			ReconcileRequest reconcileRequest = JsonConvert.DeserializeObject<ReconcileRequest>(JsonConvert.SerializeObject(this));
			reconcileRequest?.ScrubData();
			return JsonConvert.SerializeObject(reconcileRequest, Formatting.Indented);
		}
		catch (Exception arg)
		{
			return $"Exception in GetCustomerOffersRequest.Scrub: {arg}";
		}
	}

	public override void ScrubData()
	{
		base.ScrubData();
		CreditCard?.ScrubData();
	}
}
