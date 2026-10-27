using System;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Transaction;

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
			AuthorizeRequest authorizeRequest = JsonConvert.DeserializeObject<AuthorizeRequest>(this.ToJson());
			authorizeRequest?.ScrubData();
			return authorizeRequest?.ToJsonIndented();
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
