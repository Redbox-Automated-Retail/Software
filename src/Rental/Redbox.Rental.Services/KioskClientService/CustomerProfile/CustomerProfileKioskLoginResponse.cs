using System;
using DeviceService.ComponentModel;
using Newtonsoft.Json;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.CustomerProfile;

namespace Redbox.Rental.Services.KioskClientService.CustomerProfile;

public class CustomerProfileKioskLoginResponse : BaseResponse, ICustomerProfileKioskLoginResponse, IBaseResponse, IMessageScrub
{
	public Guid MessageId { get; set; }

	public KioskLoginResponse Response { get; set; }

	public object Scrub()
	{
		try
		{
			CustomerProfileKioskLoginResponse customerProfileKioskLoginResponse = JsonConvert.DeserializeObject<CustomerProfileKioskLoginResponse>(JsonConvert.SerializeObject(this));
			customerProfileKioskLoginResponse?.ScrubData();
			return JsonConvert.SerializeObject(customerProfileKioskLoginResponse, Formatting.Indented);
		}
		catch (Exception arg)
		{
			return $"Exception in CustomerProfileKioskLoginResponse.Scrub: {arg}";
		}
	}

	public override void ScrubData()
	{
		base.ScrubData();
		Response?.ScrubData();
	}
}
