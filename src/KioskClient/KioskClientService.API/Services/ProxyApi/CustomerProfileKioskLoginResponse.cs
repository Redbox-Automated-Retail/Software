using System;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.ProxyApi;

public class CustomerProfileKioskLoginResponse : BaseResponse, IMessageScrub
{
	public Guid MessageId { get; set; }

	public KioskLoginResponse Response { get; set; }

	public CustomerProfileKioskLoginResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		try
		{
			CustomerProfileKioskLoginResponse customerProfileKioskLoginResponse = JsonConvert.DeserializeObject<CustomerProfileKioskLoginResponse>(this.ToJson());
			customerProfileKioskLoginResponse?.ScrubData();
			return customerProfileKioskLoginResponse?.ToJsonIndented();
		}
		catch (Exception arg)
		{
			return $"Exception in CustomerProfileKioskLoginResponse.Scrub: {arg}";
		}
	}

	public override void ScrubData()
	{
		Response?.ScrubData();
	}
}
