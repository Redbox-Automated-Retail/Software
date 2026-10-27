using System;
using Newtonsoft.Json;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class AuthorizeRequest : BaseAuthorizeRequest
{
	public bool? AuthAcceptFlag { get; set; }

	public int? AuthRuleId { get; set; }

	public decimal? AuthAmount { get; set; }

	public bool? SkipAuthRule { get; set; }

	public override object Scrub()
	{
		try
		{
			AuthorizeRequest authorizeRequest = JsonConvert.DeserializeObject<AuthorizeRequest>(JsonConvert.SerializeObject(this));
			authorizeRequest?.ScrubData();
			return JsonConvert.SerializeObject(authorizeRequest, Formatting.Indented);
		}
		catch (Exception arg)
		{
			return $"Exception in AuthorizeResult.Scrub: {arg}";
		}
	}

	public override void ScrubData()
	{
		base.ScrubData();
	}
}
